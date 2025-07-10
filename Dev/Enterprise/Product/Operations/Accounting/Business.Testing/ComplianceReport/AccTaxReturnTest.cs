using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccTaxReturn))]
	class AccTaxReturnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var taxReturn = Factory.New<AccTaxReturn>();
			AssertEquals(AccTaxReturn.Schema.ATR_Status, "SAV", taxReturn.ATR_Status);
			AssertEquals(AccTaxReturn.Schema.ATR_Version, 0, taxReturn.ATR_Version);
		}

		public void TestCanApplyDataRefreshIsImplemented()
		{
			var taxReturn = Factory.New<AccTaxReturn>();
			var dataRefreshSupporter = taxReturn as ICanApplyDataRefresh;

			AssertNotNull(dataRefreshSupporter);
			AssertEquals(false, dataRefreshSupporter.CanApplyDataRefresh(DataRefreshAction.None, null));
		}

		public void TestColumnsSizeAreSync()
		{
			var taxReturn = Factory.New<AccTaxReturn>();
			var company = Factory.New<GlbCompany>();

			AssertEquals("The size of ATR_CompanyName column should be the same as GC_Name column.", company.GC_NameInfo.MaxLength, taxReturn.ATR_CompanyNameInfo.MaxLength);
			AssertEquals("The size of ATR_VATRegNo column should be the same as GC_BusinessRegNo column.", company.GC_BusinessRegNoInfo.MaxLength, taxReturn.ATR_VATRegNoInfo.MaxLength);
		}

		public void TestVersionNumberIsUpdatedDuringSaving()
		{
			var taxReturn = ObjectCreator.CreateAccTaxReturn(addTaxReturnColumn: true);

			Factory.Save();
			AssertEquals("Version # should change", 1, taxReturn.ATR_Version);

			taxReturn.ATR_GovtReturnIdentifier = "GB-MTD-002";
			Factory.Save();
			AssertEquals("Version # should change", 2, taxReturn.ATR_Version);

			taxReturn.Columns[0].ATC_ColumnName = "BOX10";
			taxReturn.Columns[0].ATC_Comment = "bla..bla..";
			Factory.Save();
			AssertEquals("Version # should change", 3, taxReturn.ATR_Version);

			Factory.Save();
			AssertEquals("Version # should not change", 3, taxReturn.ATR_Version);
		}

		public void TestLoggingIsOn()
		{
			var taxReturn = ObjectCreator.CreateAccTaxReturn(addTaxReturnColumn: true);

			Factory.Save();
			var logs = taxReturn.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals("Logs", 1, logs.Count());

			taxReturn.Columns[0].ATC_ColumnName = "BOX9";
			Factory.Save();
			logs = taxReturn.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals("Logs", 2, logs.Count());
		}

		[TestDate(2019, 03, 1)]
		public void TestAuditInfoIsCollected()
		{
			var testDate = TestDateAttribute.Date;
			var taxReturn = ObjectCreator.CreateAccTaxReturn(addTaxReturnColumn: true);
			AssertEquals("ATR_SystemCreateUser", ZString.Empty, taxReturn.ATR_SystemCreateUser);
			AssertEquals("ATR_SystemCreateTimeUtc", ZDateTime.Empty, taxReturn.ATR_SystemCreateTimeUtc);
			AssertEquals("ATR_SystemLastEditUser", ZString.Empty, taxReturn.ATR_SystemLastEditUser);
			AssertEquals("ATR_SystemLastEditTimeUtc", ZDateTime.Empty, taxReturn.ATR_SystemLastEditTimeUtc);

			Factory.Save();
			AssertEquals("ATR_SystemCreateUser", Env.CurrentUser.Initials, taxReturn.ATR_SystemCreateUser);
			AssertEquals("ATR_SystemCreateTimeUtc", testDate, taxReturn.ATR_SystemCreateTimeUtc.Date);
			AssertEquals("ATR_SystemLastEditUser", Env.CurrentUser.Initials, taxReturn.ATR_SystemLastEditUser);
			AssertEquals("ATR_SystemLastEditTimeUtc", testDate, taxReturn.ATR_SystemLastEditTimeUtc.Date);

			TestDateAttribute.AddDays(1);
			var newTestDate = TestDateAttribute.Date;
			taxReturn.Columns[0].ATC_ColumnName = "BOX9";
			Factory.Save();
			AssertEquals("ATR_SystemCreateUser", Env.CurrentUser.Initials, taxReturn.ATR_SystemCreateUser);
			AssertEquals("ATR_SystemCreateTimeUtc", testDate, taxReturn.ATR_SystemCreateTimeUtc.Date);
			AssertEquals("ATR_SystemLastEditUser", Env.CurrentUser.Initials, taxReturn.ATR_SystemLastEditUser);
			AssertEquals("ATR_SystemLastEditTimeUtc", newTestDate, taxReturn.ATR_SystemLastEditTimeUtc.Date);
		}

		public void TestAccTaxReturnColumnComment_MaxLength()
		{
			var taxReturn = ObjectCreator.CreateAccTaxReturn(addTaxReturnColumn: true);

			taxReturn.Columns[0].ATC_Comment = new ZString('A', 1000);
			AssertEquals("An AccTaxReturnColumn comment can contain 1000 characters", new ZString('A', 1000), taxReturn.Columns[0].ATC_Comment);
		}

		public void TestDelete()
		{
			var report = ObjectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.LiquidazioneIVA);
			Factory.Save();

			var taxReturn = ObjectCreator.CreateAccTaxReturn(report, true);
			Factory.Save();

			taxReturn.Delete();
			Assert(taxReturn.IsDeleted);
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ObjectCreator.CreateAccTaxReturn();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return new TestObjectCreator(factory).CreateAccTaxReturn();
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
