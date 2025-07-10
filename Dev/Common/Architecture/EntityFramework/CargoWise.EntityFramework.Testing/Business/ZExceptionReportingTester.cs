using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZExceptionReportingTester : TestCaseWithFactory // TODO: move to ZExceptionReportingTest.cs
	{
		class MockNotificationHandler : INotificationHandler
		{
			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				ReportErrorCallTrace.Add(new Tuple<string, string>(message, caption));
			}

			public void ReportInformation(string message, string caption)
			{
				ReportInformationCallTrace.Add(new Tuple<string, string>(message, caption));
			}

			readonly List<Tuple<string, string>> ReportErrorCallTrace = new List<Tuple<string, string>>();
			readonly List<Tuple<string, string>> ReportInformationCallTrace = new List<Tuple<string, string>>();

			public (string Message, string Caption) GetLastErrorMessage() => (ReportErrorCallTrace.Last().Item1, ReportErrorCallTrace.Last().Item2);
			public (string Message, string Caption) GetLastInformationMessage() => (ReportInformationCallTrace.Last().Item1, ReportInformationCallTrace.Last().Item2);

			public bool ReportErrorWasCalled => ReportErrorCallTrace?.Count > 0;
			public bool ReportInformationWasCalled => ReportInformationCallTrace?.Count > 0;
		}

		class MockSaveInitiator : ISaveInitiator
		{
			class DummyBusinessObjectWithValidation : DummyBusinessObject
			{
				public DummyBusinessObjectWithValidation(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
				{ }

				protected override void RunPreSaveValidationCore()
				{
					base.RunPreSaveValidationCore();
					Z0_AnotherNumberInfo.AddError("Error on this field");
				}
			}

			public MockSaveInitiator(BusinessObjectFactory factory)
			{
				BusinessEntityForValidation = factory.New<DummyBusinessObjectWithValidation>();
			}

			public bool SaveExceptionCaughtAlready { get; set; }
			public IBusiness BusinessEntityForValidation { get; }
			public void ShowErrorsDialog() { }
		}

		public ZExceptionReportingTester()
		{
			notifier = new MockNotificationHandler();
			saveInitiator = new MockSaveInitiator(Factory);
		}

		readonly MockNotificationHandler notifier;
		readonly MockSaveInitiator saveInitiator;

		void HandleSaveException(Exception ex, INotificationHandler nh = null, ISaveInitiator si = null) => ZExceptionReporting.HandleSaveException(ex, nh ?? notifier, si ?? saveInitiator);

		public void TestHandleSaveException_NullException()
		{
			AssertExceptionThrown<ArgumentException>("", "Exception to handle cannot be null", () => HandleSaveException(null));
		}

		[Serializable] // required to suppress a reflection test which for some reason doesn't ignore test classes
		class ZDataExceptionFriendly : ZDataException
		{
			public ZDataExceptionFriendly() : base(new Exception("test_ex"), _message, "Some debug message", null, Db.Connection)
			{ }

#if NETFRAMEWORK
			// required to make unit tests pass
			protected ZDataExceptionFriendly(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{ }
#endif

			public const string _message = "Stay a while, and listen";

			public override bool ShouldBeReportedToEDI => false;
		}

		public void TestHandleLoadingUntrustedAssemblyException()
		{
			const string expectedErrorCaption = "SQL Loading Untrusted Assembly Exception";
			const string expectedErrorMessage = "A fix has been applied on loading an untrusted assembly exception, please try your last action again.";
			const string message = "An error occurred in the Microsoft .NET Framework while trying to load assembly id 65564. The server may be running out of resources, or the assembly may not be trusted with PERMISSION_SET = EXTERNAL_ACCESS or UNSAFE. Run the query again, or check documentation to see how to solve the assembly trust issues.";
			var sqlException = SqlExceptionBuilder.CreateSqlException(10314, message);

			ErrorReporter.Clear();

			AssertNoExceptionThrown(() => HandleSaveException(sqlException));

			Assert(notifier.ReportErrorWasCalled);
			AssertEquals(expectedErrorMessage, notifier.GetLastErrorMessage().Message);
			AssertEquals(expectedErrorCaption, notifier.GetLastErrorMessage().Caption);
			AssertContains(expectedErrorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestHandleUnhandleableConcurrencyException()
		{
			var isFirstRun = true;
			var hits = 0;
			using (var table = new DataTable())
			{
				AssertExceptionThrown<ZSaveConcurrencyException>(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
			   {
				   hits++;
				   if (isFirstRun)
				   {
					   isFirstRun = false;
					   throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), table.NewRow(), Db.Connection), Factory);
				   }
			   }, () => { }, throwOnMergeFailure: true));
			}

			AssertEquals(1, hits);
		}

		public void TestProcessWithConcurrencyHandling_SuccessOnRetry()
		{
			var hits = 0;
			var isFirstRun = true;
			using (var table = new DataTable())
			{
				AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					hits++;
					if (isFirstRun)
					{
						isFirstRun = false;
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), table.NewRow(), Db.Connection), Factory);
					}
				}, () => { }, 3));
			}

			AssertEquals(2, hits);
		}

		public void TestProcessWithConcurrencyHandling_AlwaysFail()
		{
			var hits = 0;
			using (var table = new DataTable())
			{
				AssertExceptionThrown<ZSaveConcurrencyException>(() => ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					hits++;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), table.NewRow(), Db.Connection), Factory);
				}, () => { }, 3));
			}

			AssertEquals(4, hits);
		}

		public void TestHandleSaveException_ZSaveExceptionHandled()
		{
			var de = new ZDataExceptionFriendly();
			var ex = new ZSaveException(de, Factory);
			AssertNoExceptionThrown(() => HandleSaveException(ex));

			Assert("Notification handler wasn't called for ZCannotSaveException", notifier.ReportErrorWasCalled);
			AssertEquals("Friendly notification message for exception handler was different to the expected message", ZDataExceptionFriendly._message, notifier.GetLastErrorMessage().Message);
			AssertEquals("Debug notification message for exception handler was different to the expected message", "Cannot Save...", notifier.GetLastErrorMessage().Caption);
		}

		public void TestHandleSaveException_ZSaveExceptionRethrownIfNoFriendlyMessage()
		{
			var dataException = new ZDataException(new Exception("test_ex"), null, Db.Connection);
			var saveException = new ZSaveException(dataException, Factory);

			var ex = AssertExceptionThrown<RethrownByExceptionHandlerException>(
				"Expected SqlException to be re-thrown but it wasn't",
				() => HandleSaveException(saveException));

			Assert("This level should be ZSaveException", ex.InnerException is ZSaveException);
			Assert("This level should be ZDataException", ex.InnerException?.InnerException is ZDataException);
			Assert(ex.InnerException?.InnerException?.InnerException?.Message == "test_ex");
		}

		public void TestHandleSaveException_LinkedServerDoesNotExist_ZServerConfigurationException()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(7202, "Could not find server 'LOOPBACK' in sys.servers. Verify that the correct server name was specified.");

			AssertNoExceptionThrown("Message", () => HandleSaveException(sqlException));

			(var message, string caption) = notifier.GetLastErrorMessage();
			Assert(notifier.ReportErrorWasCalled);
			AssertEquals("The system cannot perform this operation as a crucial server configuration object is missing. The 'ConfigureServer' procedure has not been run on this server. Please contact your system administrator.", message);
			AssertEquals("Server Misconfiguration Error", caption);
		}

		public void TestHandleSaveException_ZCannotSaveException()
		{
			var de = new ZDataException(new Exception("test_ex"), null, Db.Connection);
			var ex = new ZCannotSaveException("a", "b", false, de);

			AssertNoExceptionThrown(() => HandleSaveException(ex));
			Assert("Notification handler wasn't called for ZCannotSaveException", notifier.ReportErrorWasCalled);
		}

		public void TestHandleSaveException_SqlException()
		{
			var ex = AssertExceptionThrown<RethrownByExceptionHandlerException>(
				"Expected SqlException to be re-thrown but it wasn't",
				() => HandleSaveException(SqlExceptionBuilder.CreateSqlException(1, "fail")));

			Assert("Inner exception was expected to be SqlException", ex.InnerException is SqlException);
		}

		public void TestProcessWithSaveExceptionHandling_NoInnerSqlException()
		{
			NotificationHandler.Instance = new ZGUINotificationHandler();
			using (var table = new DataTable())
			{
				var isFirstRun = true;
				AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
				{
					if (isFirstRun)
					{
						isFirstRun = false;
						throw new ZSaveException(new ZDataExceptionWithFriendlyMessage(new Exception(), "TestProcessWithSaveExceptionHandling_NoInnerSqlException", table.NewRow(), Db.Connection), Factory);
					}
				}, () => { }));
			}
		}

		public void TestProcessWithSaveExceptionHandling_ActionCreatesNewBizo()
		{
			DummyBusinessObject dummy = null;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) => throw new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired"), ((INeedRow)dummy).Row, Db.Connection), factory));

			AssertExceptionThrown<ZSaveException>(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(
				() =>
				{
					dummy = Factory.New<DummyBusinessObject>();
					Factory.Save();
				}, null, true));
		}

		[Serializable]
		class ZDataExceptionWithFriendlyMessage : ZDataException
		{
			internal ZDataExceptionWithFriendlyMessage(Exception innerEx, string friendlyMessage, DataRow row, DbConnection connection)
				: base(innerEx, friendlyMessage, string.Empty, row, connection)
			{
			}

#if NETFRAMEWORK
			protected ZDataExceptionWithFriendlyMessage(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}
}
