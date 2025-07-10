using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZExceptionReportingTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstraintExceptionDueToBadValidation_HandledByValidation()
		{
			DummyDependantBusinessObject dummy = Factory.New<DummyDependantBusinessObject>();
			dummy.ZD1_Z0 = ZGuid.NewZGuid();
			dummy.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			try
			{
				Factory.Save();
				Fail("Expected an exception");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex, new DummySaveInitiator(dummy));
			}
			ErrorReporter.Clear();
		}

		public void TestConstraintExceptionDueToBadValidation_NotHandledByValidation()
		{
			DummyDependantBusinessObject dummy = Factory.New<DummyDependantBusinessObject>();
			dummy.FillWithValidTestData();
			dummy[DummyDependentBizoSchema.ZD1_Z0] = ZGuid.NewZGuid();
			dummy.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;
			using (dummy.GetValidationSuspender())
			{
				try
				{
					Factory.Save();
					Fail("Expected an exception");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex, new DummySaveInitiator(dummy));
				}
			}

			AssertEquals("The DummyDependentBizo cannot be inserted/updated, requires a reference to a valid DummyBizo.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCannotSaveExceptionHandling()
		{
			NotificationHandler.Instance = new ZGUINotificationHandler();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			ZExceptionReporting.HandleSaveException(new ZCannotSaveException("CannotSaveExceptionMessage", "Vote Clinty"));
			AssertEquals("Right message", "CannotSaveExceptionMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Right message", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestUnknownExceptionsGetReThrown()
		{
			bool gotException = false;
			try
			{
				ZExceptionReporting.HandleSaveException(new NoConcreteTypeException("JooWozHere"));
			}
			catch (Exception e)
			{
				gotException = true;
				AssertEquals("Exception re-thrown with original exception in Inner", typeof(NoConcreteTypeException).FullName, e.InnerException.GetType().FullName);
			}
			Assert("Expected exception was not thrown", gotException);
		}

		public void TestZCannotSaveExceptionThrownTillMaxAttemptReached()
		{
			NotificationHandler.Instance = new ZGUINotificationHandler();
			var recoveryActionAttempted = false;
			var numberOfAttempts = 0;
			Action action = () =>
			{
				numberOfAttempts++;
				throw new ZCannotSaveException(String.Format("This has been handled {0} times.", numberOfAttempts), "This is not a string");
			};
			Action recoveryAction = () => { recoveryActionAttempted = true; };

			AssertExceptionThrown<ZCannotSaveException>(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(action, recoveryAction, false, false, 8));
			AssertEquals("Exception handled 8 times.", "This has been handled 8 times.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Recovery action was attempted.", recoveryActionAttempted);
		}

		[ExpectNoExceptions]
		public void TestHandleSaveException_FindUniqueErrorKey()
		{
			var exception = new InvalidOperationException("Invalid Operation Exception");
			void Action()
			{
				throw exception;
			}

			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Action, null, false, false);
			}
			catch (InvalidOperationException)
			{
			}

			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Action, null, false, false);
			}
			catch (InvalidOperationException)
			{
			}
		}

		public void TestNullExceptionIsReThrow()
		{
			bool gotException = false;
			try
			{
				ZExceptionReporting.HandleSaveException(null);
			}
			catch (ArgumentException e)
			{
				gotException = true;
				AssertEquals("ArgumentException was not thrown due to null argument", "Exception to handle cannot be null", e.Message);
			}
			Assert("Expected ArgumentException was not thrown", gotException);
		}

		public void TestNotificationHandler()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			DummyNotificationHandler testHandler = new DummyNotificationHandler();
			ZExceptionReporting.HandleSaveException(new CannotDeleteException("CannotDeleteExceptionMessage"), testHandler);
			Assert("TestHandler Show error was not called", testHandler.ShowErrorCalled);
		}

		public void TestProcessWithSaveExceptionHandlingForUnrecoverableException()
		{
			var dummy1 = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy1.Z0_Code = "DU1";
			dummy1.Factory.Save();

			var dummy2 = new BusinessObjectFactory { RefreshEnabled = false }.NewWithPrimaryKey<DummyBusinessObject>(dummy1.PK.ToGuid());
			dummy2.Z0_Code = "DU2";

			AssertExceptionThrown<ExceptionHandlingAbortedAfterErrorMessageException>(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(dummy2.Factory.Save, null, true));
		}

		public void TestProcessWithSaveExceptionHandlingWithSecondAttemptException()
		{
			var dummy = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy.Z0_Code = "DU1";
			dummy.Factory.Save();

			var reloadedDummy = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);
			reloadedDummy.Z0_Code = "DU2";
			reloadedDummy.Factory.Save();

			dummy.Z0_Code = "DU3";

			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(dummy.Factory.Save, () => throw new ApplicationException("2nd error"), true);

				Fail("Exception should have happened here.");
			}
			catch (ZSaveConcurrencyException ex)
			{
				AssertContains("Top level exception should be about original error (concurrency)", "Tablename: DummyBizo", ex.Message);

				Assert("Should contain data about new exception during handling original one", ex.Data.Contains("ExceptionHandlingException"));

				var ex1Object = ex.Data["ExceptionHandlingException"];
				AssertNotNull("Should contain non-null exception data object", ex1Object);
				Assert("New excetpion data should be exception itself", ex1Object is Exception);

				var ex1 = (Exception)ex1Object;

				AssertEquals(typeof(ApplicationException), ex1.GetType());
				AssertEquals("2nd error", ex1.Message);
			}
			catch (Exception ex)
			{
				Fail("Unexpected exception thrown: " + ex);
			}
		}
	}

	sealed class ZExceptionReportingDummyTest : TestCaseWithDummy
	{
		public void TestSaveExceptionWithFriendlyMessage()
		{
			DummyDependantBusinessObject depDummy = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			depDummy.ZD1_Z0 = Dummy.PK;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();

			bool gotException = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException)
			{
				gotException = true;
			}
			AssertEquals("Should not have thrown an exception saving the BizOs", false, gotException);

			ZSaveException friendlyException = null;
			Dummy.Delete();
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				friendlyException = ex;
			}

			AssertNotNull("Expected Exception was not thrown", friendlyException);

			AssertEquals("Pre-Condition: Need Friendly message to be set", false, string.IsNullOrEmpty(friendlyException.FriendlyMessage));
			ZExceptionReporting.HandleSaveException(friendlyException);
			AssertEquals("Friendly message from SaveException should have been returned", friendlyException.FriendlyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Excepted MessageType.Error", UnitTestUserNotification.Instance.LastMessage.WasError);
		}
	}

	#region DummyNotificationHandler

	public class DummyNotificationHandler : INotificationHandler
	{
		public bool ShowInformationCalled
		{
			get { return fInfoCalled; }
		}
		bool fInfoCalled;

		public bool ShowErrorCalled
		{
			get { return fErrorCalled; }
		}
		bool fErrorCalled;

		public void ClearFlags()
		{
			fErrorCalled = false;
			fInfoCalled = false;
		}

		#region INotificationHandler Members

		public void ReportInformation(string message, string caption)
		{
			fInfoCalled = true;
		}

		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			fErrorCalled = true;
		}

		#endregion
	}

	#endregion DummyNotificationHandler

	#region DummySaveInitiator

	class DummySaveInitiator : ISaveInitiator
	{
		readonly BusinessObject bo;

		public DummySaveInitiator(BusinessObject bo)
		{
			this.bo = bo;
		}

		public bool SaveExceptionCaughtAlready
		{
			get { return false; }
			set { }
		}

		public IBusiness BusinessEntityForValidation
		{
			get { return bo; }
		}

		public void ShowErrorsDialog()
		{
		}
	}

	#endregion
}
