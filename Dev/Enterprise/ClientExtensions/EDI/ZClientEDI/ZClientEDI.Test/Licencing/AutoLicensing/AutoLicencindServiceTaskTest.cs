using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Client.EDI.Licencing.AutoLicensing.ServiceTasks.Testing
{
	[TestedType(typeof(AutoLicensingServiceTask))]
	public class AutoLicencingServiceTaskTest : ServiceTaskTestCase<AutoLicensingServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		public void TestRunTask()
		{
			var licDefault = BillingTestHelper.CreateLicence(Factory, "ENT");
			var licTarget = BillingTestHelper.CreateLicence(Factory, "CW1");
			Factory.Save();
			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licDefault.Database.EnterpriseID);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			licTarget.Database.LD_OH_WebAccessOrg = org1.PK;

			var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
			ld1.LD_OH_WebAccessOrg = ZGuid.Empty;
			ld1.LD_LE = licDefault.Database.LicEnterprise.PK;
			ld1.LD_MasterOrgSuggestedUTC = ZDateTime.Empty;
			var info = new LicenceDatabaseRegistrationAdditionalInfo();
			info.EnterpriseCode = licTarget.Database.EnterpriseCode;
			info.EnterpriseID = licTarget.Database.EnterpriseID;
			ld1.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, info.ToString());

			Factory.Save();

			var process = new AutoLicensingServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			logger.ClearLog();
			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			ld1 = newFactory.Load<LicenceDatabase>(ld1.PK);
			Assert(ld1.LD_OH_WebAccessOrg.IsEmpty);
			Assert(!ld1.LD_MasterOrgSuggestedUTC.IsEmpty);

			var suggestions = newFactory.Load<EdiLicenceDatabaseOrgSuggestion>(new ZQuery());
			AssertEquals(1, suggestions.Length);
			var suggestionsLD1 = suggestions.Single(x => x.LDS_LD == ld1.PK);
			AssertEquals(ld1.PK, suggestionsLD1.LDS_LD);
			AssertEquals(org1.PK, suggestionsLD1.LDS_OH);
			AssertEquals(4000, suggestionsLD1.LDS_TotalScore);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StmNoteSchema.Constants.TableName,
						null,
						StmNoteSchema.Constants.ST_Table + "=" + LicenceDatabaseSchema.Constants.TableName),
				};
			}
		}
	}
}
