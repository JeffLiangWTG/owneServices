using System;
using System.Linq;
using System.Threading;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Async.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ExceptionReportBuilderTest : BaseExceptionBuilderTest
	{
		public void TestGenerateReport_ShouldIncludeFactoryCreationThreadID()
		{
			var attachment = TestExceptionReportBuilder.GenerateReport();
			var xml = new XmlDocument();
			xml.LoadXml(attachment);

			var creationThreadIDs = xml.SelectNodes("//FactoryStatistics/FactoryStatistic/ThreadID");
			Assert("Should have found at least one creation thread ID", creationThreadIDs.Count > 0);
		}

		public void TestGenerateReport_ShouldIncludeTestRigOriginWhenPresent()
		{
			var workItemNumber = "WI00761179";

			using (var cmd = Db.Connection.Command($"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (newid(), @name, convert(varbinary(max), @value))"))
			{
				cmd.AddParameter("@name", System.Data.SqlDbType.VarChar, "TEST_RIG_ORIGIN");
				cmd.AddParameter("@value", System.Data.SqlDbType.NVarChar, workItemNumber);

				_ = cmd.ExecuteNonQuery();
			}

			var attachment = TestExceptionReportBuilder.GenerateReport();
			var xml = new XmlDocument();
			xml.LoadXml(attachment);

			var nodes = xml.SelectNodes("//TestRigOrigin");

			AssertEquals("A single TestRigOrigin node should exist", 1, nodes.Count);

			AssertEquals(nodes[0].InnerText, workItemNumber);
		}

		public void TestGenerateReport_DoesNotIncludeTestRigOriginWhenAbsent()
		{
			var attachment = TestExceptionReportBuilder.GenerateReport();
			var xml = new XmlDocument();
			xml.LoadXml(attachment);

			var nodes = xml.SelectNodes("//TestRigOrigin");

			AssertEquals("No TestRigOrigin node should exist", 0, nodes.Count);
		}

		public void TestGenerateReport_OtherFactoriesSectionIsPresent()
		{
			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				var newOtherFactory = new BusinessObjectFactory { NameForDebugging = "DummyOtherFactory" };
				(newOtherFactory as IBusinessObjectFactoryInternals).IncludeWithOtherFactoriesForIssueReport = false;
				var newMainFactory = new BusinessObjectFactory { NameForDebugging = "DummyMainFactory" };
				(newMainFactory as IBusinessObjectFactoryInternals).IncludeWithOtherFactoriesForIssueReport = true;
				var attachment = TestExceptionReportBuilder.GenerateReport();
				var xml = new XmlDocument();
				xml.LoadXml(attachment);

				var otherFactories = xml.SelectNodes("//OtherFactoryStatistics");
				AssertEquals("Should have found the other factories section", 1, otherFactories.Count);
			}
		}

		public void TestGenerateReport_IncludeWithOtherFactoriesSectionIsAbsent()
		{
			var attachment = TestExceptionReportBuilder.GenerateReport();
			var xml = new XmlDocument();
			xml.LoadXml(attachment);

			var mainFactories = xml.SelectNodes("//FactoryStatistics");
			var otherFactories = xml.SelectNodes("//OtherFactoryStatistics");

			AssertEquals("Should have found the main factories section", 1, mainFactories.Count);
			AssertEquals("Should not have found the other factories section", 0, otherFactories.Count);
		}

		public void TestGenerateReport_ShouldIncludeFactoryInstance()
		{
			var attachment = TestExceptionReportBuilder.GenerateReport();
			var xml = new XmlDocument();
			xml.LoadXml(attachment);

			var factoryInstance = xml.SelectNodes("//FactoryStatistics/FactoryStatistic/FactoryInstance");
			Assert("Should have found at least one factory instance", factoryInstance.Count > 0);
		}

		public void TestGenerateReport_ShouldBeAbleToReportFactoryStatsCrossThread()
		{
			BusinessObjectFactory factory = null;
			string attachment = null;

			ThreadSwapper
				.SetupOtherThread(Db.DisposableActionForDbConnection)
				.ThenOnMain(() => { })
				// Arrange
				.ThenOnOther(() => factory = new BusinessObjectFactory { NameForDebugging = "FactoryCreatedOnOtherThread" })
				// Act
				.ThenOnMain(() => attachment = TestExceptionReportBuilder.GenerateReport())
				.Go();

			// Assert
			var xml = new XmlDocument();
			xml.LoadXml(attachment);
			var factories = xml.SelectNodes("//FactoryStatistics/FactoryStatistic");
			var targetFactory = factories.Cast<XmlNode>().FirstOrDefault(factoryNode => factoryNode.ChildNodes[0].InnerText == "FactoryCreatedOnOtherThread");
			var protectedAttributeDetail = targetFactory.ChildNodes[2].InnerText;

			AssertEquals($"*inaccessible from error reporter thread #{Thread.CurrentThread.ManagedThreadId}*", protectedAttributeDetail);
		}

		public void TestGenerateReport()
		{
			string attachment = TestExceptionReportBuilder.GenerateReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(attachment);
			Assert("Valid Xml", xmlDoc.OuterXml.Length > 0);
		}

		public void TestGenerateReportWithCharZero()
		{
			string attachment = TestExceptionReportBuilderWithCharZero.GenerateReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(attachment);
			Assert("Valid Xml", xmlDoc.OuterXml.Length > 0);
		}

		public void TestGenerateReport_IncludesLicenceInfo()
		{
			string attachment = TestExceptionReportBuilder.GenerateReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(attachment);

			XmlNodeList enterpriseCode = xmlDoc.SelectNodes("//LicenceEnterpriseCode");
			XmlNodeList companyCode = xmlDoc.SelectNodes("//LicenceCompanyCode");
			XmlNodeList serverCode = xmlDoc.SelectNodes("//LicenceServerCode");

			CombineAssertions(delegate
			{
				AssertEquals("EnterpriseCode", 1, enterpriseCode.Count);
				AssertEquals("EnterpriseCode", "EDI", enterpriseCode[0].InnerText);
				AssertEquals("CompanyCode", 1, companyCode.Count);
				AssertEquals("CompanyCode", "EDI", companyCode[0].InnerText);
				AssertEquals("ServerCode", 1, serverCode.Count);
				AssertEquals("ServerCode", "DAT", serverCode[0].InnerText);
			});
		}

		public void TestGenerateReport_IncludesExceptionStack()
		{
			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				var handler = new TopLevelExceptionHandler();

				SetUpForLogExceptionsThrownMethod(handler, 10);

				Assert(ErrorReporter.ExceptionsThrown.Count >= 10);

				var attachment = TestExceptionReportBuilder.GenerateReport();
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(attachment);

				var previousExceptionsNode = xmlDoc.SelectNodes("//PreviousExceptions");
				var previousExceptions = xmlDoc.SelectNodes("//Exception");
				CombineAssertions(delegate
				{
					AssertEquals("PreviousExceptions", 1, previousExceptionsNode.Count);
					AssertEquals("PreviousExceptions", 10, previousExceptions.Count);
					var expectedMessage = @"Type :System.Exception
Message :Something went wrong.
Stacktrace :";
					Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains(expectedMessage)).Any());
				});
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}
		}

		void SetUpForLogExceptionsThrownMethod(TopLevelExceptionHandler handler, int exceptionNumber)
		{
			var exceptionsToThrwon = Enumerable.Range(0, exceptionNumber).Select(i => new Exception("Something went wrong.")).ToArray();

			var exceptionhandled = true;
			foreach (var exception in exceptionsToThrwon)
			{
				handler.HandleUnhandledException(exception, (s, e) => exceptionhandled = false);
			}
			Assert(!exceptionhandled);
		}

		public void TestGenerateReport_IncludesCommandLine()
		{
			string attachment = TestExceptionReportBuilder.GenerateReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(attachment);

			AssertEquals("There should be command line information", 1, xmlDoc.SelectNodes("//CommandLine").Count);
		}

		public void TestGenerateReport_IncludesSessionIdAndSequence()
		{
			string attachment = TestExceptionReportBuilder.GenerateReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(attachment);

			var sessionId = xmlDoc.SelectNodes("//SessionId");
			var sequence = xmlDoc.SelectNodes("//Sequence");

			CombineAssertions(delegate
			{
				AssertEquals("SessionId", 1, sessionId.Count);
				AssertEquals("EnterpriseCode", "A3DA6FB6-11AE-4550-97DC-6155ADF7EBB3", sessionId[0].InnerText.ToUpper());
				AssertEquals("Sequence", 1, sequence.Count);
				AssertEquals("Sequence", "123", sequence[0].InnerText);
			});
		}

		public void TestGenerateReport_ErrorGettingCurrentUserCompanyBranchDepartment()
		{
			var mockEnv = new Mock<IEnv>();
			var mockEnvironment = new Mock<Enterprise.Environment.IEnvironment>();

			mockEnvironment.Setup(m => m.Time).Returns(new TimeFactory());
			mockEnvironment.Setup(m => m.CurrentUser).Throws(new InvalidOperationException("Test exception when getting current user"));
			mockEnvironment.Setup(m => m.CurrentCompany).Throws(new InvalidOperationException("Test exception when getting current company"));
			mockEnvironment.Setup(m => m.CurrentBranch).Throws(new InvalidOperationException("Test exception when getting current branch"));
			mockEnvironment.Setup(m => m.CurrentDepartment).Throws(new InvalidOperationException("Test exception when getting current department"));

			mockEnv.Setup(m => m.Instance).Returns(mockEnvironment.Object);

			using (EnvProxy.SetTemporaryEnvForTest(mockEnv.Object))
			{
				var attachment = TestExceptionReportBuilder.GenerateReport();
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(attachment);
				Assert("Valid Xml", xmlDoc.OuterXml.Length > 0);

				Assert("Exception thrown while generating report should be handled to see original exception", !attachment.Contains("Failed to get report:"));
			}
		}

		public void TestExceptionReporter_NullException()
		{
			var args = new ExceptionReportArgs(null, "", "Apple", "Apple fell from the tree");
			var builder = new ExceptionReportBuilder(args);
			AssertEquals(false, builder.GenerateReport().Contains("Object reference not set"));
		}

		public void TestReportBuildWithoutLoggin_NoCompanyAtAll()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			for (int i = 0; i < 5; i++)
			{
				var gloCompany = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			}
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				AssertEquals("Precondition: User context should be unset", null, EnvProxy.Instance.CurrentUser);
				AssertEquals("Precondition: User context should be unset", false, EnvProxy.Instance.IsLoggedIn);
				var attachment = TestExceptionReportBuilder.GenerateReport();

				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(attachment);

				AssertEquals("There should be no company.", 0, xmlDoc.SelectNodes("//Company").Count);
			}
		}

		public void TestGenerateReport_ShouldIncludeChildFactoryIDs()
		{
			var attachment = TestExceptionReportBuilder.GenerateReport();
			var xml = new XmlDocument();
			xml.LoadXml(attachment);

			var childFactoryIDs = xml.SelectNodes("//FactoryStatistics/FactoryStatistic/ChildFactoryIDs");
			Assert("Should have found at least one creation thread ID", childFactoryIDs.Count > 0);
		}

		#region TestRethrowCriticalException

		[ExpectNoExceptions]
		public void TestNoRethrowNoCriticalException()
		{
			ExceptionReportBuilder exceptionReportBuilder =
				new ExceptionReportBuilder(new ExceptionReportArgs(new Exception(), "e", "k", "d"))
				{
					TestActionHandler = delegate { throw new NullReferenceException(); }
				};

			Assert(!string.IsNullOrEmpty(exceptionReportBuilder.GenerateReport()));
		}

		[ExpectException(typeof(AppDomainUnloadedException))]
		public void TestRethrowCriticalException()
		{
			ExceptionReportBuilder exceptionReportBuilder =
				new ExceptionReportBuilder(new ExceptionReportArgs(new Exception(), "e", "k", "d"))
				{
					TestActionHandler = delegate { throw new AppDomainUnloadedException(); }
				};

			Assert(!string.IsNullOrEmpty(exceptionReportBuilder.GenerateReport()));
		}

		public void TestWhenExceptionReportBuilderHasErrorReportId()
		{
			var exceptionReportBuilder = new ExceptionReportBuilder(new ExceptionReportArgs(new Exception(), "", "k", "d"));
			Assert("No Error Report ID", string.IsNullOrEmpty(exceptionReportBuilder.ErrorReportID));

			exceptionReportBuilder = new ExceptionReportBuilder(new ExceptionReportArgs(new Exception(), "e", "k", "d"));
			AssertEquals("Has Report ID", "e", exceptionReportBuilder.ErrorReportID);
		}

		#endregion

		#region TestRemoveNulls

		public void TestEscapeNulls()
		{
			var input = "<element>test\\0test</eletment>";
			AssertEquals("<element>test\\0test</eletment>", ExceptionReportBuilder.EscapeNulls(input));

			input = "<eletment>test\\0</eletment>";
			AssertEquals("<eletment>test\\0</eletment>", ExceptionReportBuilder.EscapeNulls(input));

			input = "<element>test\0test</eletment>";
			AssertEquals("<element>test\\0test</eletment>", ExceptionReportBuilder.EscapeNulls(input));
		}

		#endregion

		#region Implementation

		ExceptionReportBuilder TestExceptionReportBuilder
		{
			get
			{
				if (testExceptionReportBuilder == null)
				{
					Exception ex = new Exception("Exception");
					ExceptionReportArgs reportArgs = new ExceptionReportArgs(ex, "Test Error ID", "Test Key", "Test Error");
					reportArgs.SessionId = new Guid("A3DA6FB6-11AE-4550-97DC-6155ADF7EBB3");
					reportArgs.Sequence = 123;
					testExceptionReportBuilder = new ExceptionReportBuilder(reportArgs);
				}
				return testExceptionReportBuilder;
			}
		}

		ExceptionReportBuilder TestExceptionReportBuilderWithCharZero
		{
			get
			{
				if (testExceptionReportBuilder == null)
				{
					Exception ex = new Exception("Exception");
					ExceptionReportArgs reportArgs = new ExceptionReportArgs(ex, "Test Error ID", "Test Key", "Test Error \0");
					reportArgs.SessionId = new Guid("A3DA6FB6-11AE-4550-97DC-6155ADF7EBB3");
					reportArgs.Sequence = 123;
					testExceptionReportBuilder = new ExceptionReportBuilder(reportArgs);
				}

				return testExceptionReportBuilder;
			}
		}

		protected override ExceptionBuilder TestBuilder
		{
			get { return TestExceptionReportBuilder; }
		}

		ExceptionReportBuilder testExceptionReportBuilder;

		#endregion Implementation
	}
}
