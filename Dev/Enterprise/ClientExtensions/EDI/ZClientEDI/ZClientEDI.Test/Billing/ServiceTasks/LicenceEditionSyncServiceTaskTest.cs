using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Billing.ServiceTasks.Testing
{
	[TestedType(typeof(LicenceEditionSyncServiceTask))]
	public class LicenceEditionSyncServiceTaskTest : ServiceTaskTestCase<LicenceEditionSyncServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var newLicHeaders = Enumerable.Range(11, 5).Select(x => CreateLicenceHeader("L" + x.ToString())).OrderBy(x => x.PK).ToArray();
			var query = new ZQuery(LicenceHeaderSchema.PK, newLicHeaders.Select(x => x.PK));
			var licHeaders = (new BusinessObjectFactory()).Load<LicenceHeader>(query);
			AssertEquals(5, licHeaders.Length);
			AssertEquals(true, licHeaders.All(x => x.LA_LicenceAdvStdOth == "ODM"));
			AssertEquals(true, licHeaders.All(x => x.Edition == "STL"));
			var process = new LicenceEditionSyncServiceTaskForTest();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(process.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => process.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestRunTask()
		{
			var newLicHeaders = Enumerable.Range(11, 5).Select(x => CreateLicenceHeader("L" + x.ToString())).OrderBy(x => x.PK).ToArray();
			var query = new ZQuery(LicenceHeaderSchema.PK, newLicHeaders.Select(x => x.PK));
			var licHeaders = (new BusinessObjectFactory()).Load<LicenceHeader>(query);
			AssertEquals(5, licHeaders.Length);
			AssertEquals(true, licHeaders.All(x => x.LA_LicenceAdvStdOth == "ODM"));
			AssertEquals(true, licHeaders.All(x => x.Edition == "STL"));
			var process = new LicenceEditionSyncServiceTaskForTest();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			process.RunTask();
			var linesExpected = @"
Organization L11L11, database L11 updated to STL edition
Organization L12L12, database L12 updated to STL edition
Organization L13L13, database L13 updated to STL edition
Organization L14L14, database L14 updated to STL edition
Organization L15L15, database L15 updated to STL edition".Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			var log = logger.ToString();
			AssertEquals(true, linesExpected.All(x => log.Contains(x)));
			licHeaders = (new BusinessObjectFactory()).Load<LicenceHeader>(query);
			AssertEquals(5, licHeaders.Length);
			AssertEquals(true, licHeaders.All(x => x.LA_LicenceAdvStdOth == "STL"));
			AssertEquals(true, licHeaders.All(x => x.Edition == "STL"));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		LicenceHeader CreateLicenceHeader(string licCode)
		{
			var prodLic1 = BillingTestHelper.CreateLicence(Factory, licCode);
			prodLic1.LA_LicenceAdvStdOth = "ODM";
			var priceHeader = prodLic1.Company.PriceHeaders.AddNew();
			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = new ZDateTime(2010, 1, 1);
			priceLink.PHL_RX_NKCurrency = "AUD";
			Factory.Save();
			AssertEquals("ODM", prodLic1.LA_LicenceAdvStdOth);
			AssertEquals("STL", prodLic1.Edition);
			return prodLic1;
		}

		class LicenceEditionSyncServiceTaskForTest : LicenceEditionSyncServiceTask
		{
			protected override int MaximumLoadRows => 2;
		}
	}
}
