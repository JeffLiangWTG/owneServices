using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	sealed class LicenceConsumptionLogSenderProcessTest : TestCaseWithFactory
	{
		[TestDate(2008, 1, 3)]
		public void TestExecute()
		{
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 1, 9, 1, 0));
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 1, 9, 4, 0));
			CreateLicenceUsageRecord(EnvProxy.Instance.Licence.Core, "XX", new ZDateTime(2008, 1, 1, 9, 2, 0));

			ZDateTime dateFromUtcInclusive = new DateTime(2008, 1, 1);
			ZDateTime dateToUtcExclusive = new DateTime(2008, 1, 2);
			LicenceUsageReportBuilder usageReport = new LicenceUsageReportBuilder(new BusinessObjectFactory() { RefreshEnabled = false });
			usageReport.BuildLatest(dateFromUtcInclusive, dateToUtcExclusive);
			var expectedReport = usageReport.GetCompressedEncryptedText();

			LicenceConsumptionLogSenderProcess task = new LicenceConsumptionLogSenderProcess();
			string report = task.Execute(dateFromUtcInclusive, dateToUtcExclusive);
			AssertEquals("Report created", expectedReport, report);
		}

		#region Implementation

		StmActivityLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc)
		{
			return CreateLicenceUsageRecord(checkpoint, userInitials, usageTimeUtc, GlbCompany.CurrentCompany.PK, ModuleLicenceType.NON);
		}

		StmActivityLog CreateLicenceUsageRecord(ILicenceCheckpoint checkpoint, string userInitials, ZDateTime usageTimeUtc, ZGuid companyPK, ModuleLicenceType licenceType)
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_OpenDateTimeUtc = usageTimeUtc;
			log.S7_FormCaption = checkpoint.Name;
			log.S7_GS_NKUser = userInitials;
			log.S7_ParentID = companyPK;
			log.S7_ControllerID = LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString();
			log.S7_MouseClicks = (int)licenceType;
			return log;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
		}

		#endregion
	}
}
