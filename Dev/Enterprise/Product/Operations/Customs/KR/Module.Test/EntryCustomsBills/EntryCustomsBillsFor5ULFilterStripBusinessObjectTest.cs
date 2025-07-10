using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryCustomsBillsFor5ULFilterStripBusinessObject))]
	sealed class EntryCustomsBillsFor5ULFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilter()
		{
			var filter = new EntryCustomsBillsFor5ULFilterStripBusinessObject();
			AssertNotNull(filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.Payer]);
			AssertNotNull(filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.EntryNum]);
			AssertNotNull(filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.AcceptedDate]);
			AssertNotNull(filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.PaymentDate]);
		}

		public void TestPayer()
		{
			var filter = new EntryCustomsBillsFor5ULFilterStripBusinessObject();
			var payerFilter = (ModuleGuidFilter)filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.Payer];
			payerFilter.Property = testPayerPK;
			payerFilter.IsActive = true;

			var query = new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL();
			var coll = new KREntryCustomsBillsViewCollection(Factory, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;

			AssertEquals(1, coll.Count);
			AssertEquals("1111111111111111111", coll.Cast<KREntryCustomsBillsView>().First().KEB_CustomsDisbursementBillNumber);
		}

		public void TestEntryNumber()
		{
			var filter = new EntryCustomsBillsFor5ULFilterStripBusinessObject();
			var entryNumberFilter = (ModuleNumberFilter)filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.EntryNum];
			entryNumberFilter.Property = "1234522123451X";
			entryNumberFilter.IsActive = true;

			var query = new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL();
			var coll = new KREntryCustomsBillsViewCollection(Factory, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;

			AssertEquals(1, coll.Count);
			AssertEquals("4444444444444444444", coll.Cast<KREntryCustomsBillsView>().First().KEB_CustomsDisbursementBillNumber);
		}

		public void TestEntryIssueDate()
		{
			var filter = new EntryCustomsBillsFor5ULFilterStripBusinessObject();
			var entryIssueDateFilter = (ModuleDateFilter)filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.AcceptedDate];
			entryIssueDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			entryIssueDateFilter.Property1 = new ZDateTime(2025, 01, 01);
			entryIssueDateFilter.Property2 = new ZDateTime(2026, 01, 01);
			entryIssueDateFilter.IsActive = true;

			var query = new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL();
			var coll = new KREntryCustomsBillsViewCollection(Factory, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;

			AssertEquals(1, coll.Count);
			AssertEquals("4444444444444444444", coll.Cast<KREntryCustomsBillsView>().First().KEB_CustomsDisbursementBillNumber);
		}

		public void TestPaymentAuthorizationDate()
		{
			var filter = new EntryCustomsBillsFor5ULFilterStripBusinessObject();
			var paymentDateFilter = (ModuleDateFilter)filter[EntryCustomsBillsFor5ULFilterStripBusinessObject.Schema.PaymentDate];
			paymentDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			paymentDateFilter.Property2 = new ZDateTime(2025, 01, 01);
			paymentDateFilter.IsActive = true;

			var query = new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL();
			var coll = new KREntryCustomsBillsViewCollection(Factory, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;

			AssertEquals(1, coll.Count);
			AssertEquals("1111111111111111111", coll.Cast<KREntryCustomsBillsView>().First().KEB_CustomsDisbursementBillNumber);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryCustomsBillsFor5ULFilterStripBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			#region TestData1
			var payer1 = Factory.New<OrgHeader>();
			payer1.OH_Category = "BUS";
			payer1.OH_Code = "RK Payer";
			payer1.OH_FullName = "RK Payer Test";
			testPayerPK = payer1.PK;

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			declaration1.JE_ApplicationCode = "BLT";

			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = new ZDateTime(2024, 07, 01);

			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = "IMP";
			entryNum1.CE_EntryNum = "1234522123450X";
			entryNum1.CE_IssueDate = new ZDateTime(2024, 08, 01);

			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_OH_Importer = payer1.PK;
			statement1.B2_StatementType = "D";
			statement1.B2_PaymentStatus = "PYC";
			statement1.B2_StatementNumber = "1111111111111111111";
			statement1.B2_PaymentAuthorizationDate = new ZDateTime(2024, 09, 01);
			statement1.B2_PrintDate = new ZDateTime(2024, 10, 01);
			statement1.B2_ProcessDate = new ZDateTime(2024, 11, 01);
			statement1.B2_DueDate = new ZDateTime(2024, 12, 01);

			var statementLine1 = statement1.StatementLines.AddNew();
			statementLine1.B3_EntryNum = entryNum1.CE_EntryNum;
			statementLine1.B3_AssociatedEntry = "2222222222222222222";
			statementLine1.B3_CustomsFeesTotal = 1234500m;

			SetChargeAmount(statementLine1, "DTY", 1000m);
			SetChargeAmount(statementLine1, "VAT", 2000m);
			SetChargeAmount(statementLine1, "LQT", 3000m);
			SetChargeAmount(statementLine1, "AGT", 4000m);
			SetChargeAmount(statementLine1, "SCT", 5000m);
			SetChargeAmount(statementLine1, "TRT", 6000m);
			SetChargeAmount(statementLine1, "EDT", 7000m);
			SetChargeAmount(statementLine1, "PLT", 8000m);
			SetChargeAmount(statementLine1, "PMT", 9000m);
			SetChargeAmount(statementLine1, "VFV", 10000m);
			#endregion

			#region TestData2
			var payer2 = Factory.New<OrgHeader>();
			payer2.OH_Category = "BUS";
			payer2.OH_Code = "RK Payer 2";
			payer2.OH_FullName = "RK Payer Test 2";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_ApplicationCode = "BLT";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = new ZDateTime(2025, 07, 01);

			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = "IMP";
			entryNum2.CE_EntryNum = "1234522123451X";
			entryNum2.CE_IssueDate = new ZDateTime(2025, 08, 01);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_OH_Importer = payer2.PK;
			statement2.B2_StatementType = "I";
			statement2.B2_PaymentStatus = "PYC";
			statement2.B2_StatementNumber = "3333333333333333333";
			statement2.B2_PaymentAuthorizationDate = new ZDateTime(2025, 09, 01);
			statement2.B2_PrintDate = new ZDateTime(2025, 10, 01);
			statement2.B2_ProcessDate = new ZDateTime(2025, 11, 01);
			statement2.B2_DueDate = new ZDateTime(2025, 12, 01);

			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = entryNum2.CE_EntryNum;
			statementLine2.B3_AssociatedEntry = "4444444444444444444";
			statementLine2.B3_CustomsFeesTotal = 5678900m;

			SetChargeAmount(statementLine2, "DTY", 100000m);
			SetChargeAmount(statementLine2, "VAT", 200000m);
			SetChargeAmount(statementLine2, "LQT", 300000m);
			SetChargeAmount(statementLine2, "AGT", 400000m);
			SetChargeAmount(statementLine2, "SCT", 500000m);
			SetChargeAmount(statementLine2, "TRT", 600000m);
			SetChargeAmount(statementLine2, "EDT", 700000m);
			SetChargeAmount(statementLine2, "PLT", 800000m);
			SetChargeAmount(statementLine2, "PMT", 900000m);
			SetChargeAmount(statementLine2, "VFV", 1000000m);
			#endregion

			#region TestData3
			var payer3 = Factory.New<OrgHeader>();
			payer3.OH_Category = "BUS";
			payer3.OH_Code = "RK Payer 3";
			payer3.OH_FullName = "RK Payer Test 3";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = "IMP";
			declaration3.JE_ApplicationCode = "BLT";

			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = new ZDateTime(2025, 07, 01);

			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = "IMP";
			entryNum3.CE_EntryNum = "1234522123452X";
			entryNum3.CE_IssueDate = new ZDateTime(2025, 08, 01);

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_OH_Importer = payer3.PK;
			statement3.B2_StatementType = "I";
			statement3.B2_PaymentStatus = "PYI";
			statement3.B2_StatementNumber = "5555555555555555555";
			statement3.B2_PaymentAuthorizationDate = new ZDateTime(2025, 09, 01);
			statement3.B2_PrintDate = new ZDateTime(2025, 10, 01);
			statement3.B2_ProcessDate = new ZDateTime(2025, 11, 01);
			statement3.B2_DueDate = new ZDateTime(2025, 12, 01);

			var statementLine3 = statement3.StatementLines.AddNew();
			statementLine3.B3_EntryNum = entryNum3.CE_EntryNum;
			statementLine3.B3_AssociatedEntry = "6666666666666666666";
			statementLine3.B3_CustomsFeesTotal = 100m;

			SetChargeAmount(statementLine3, "DTY", 1m);
			SetChargeAmount(statementLine3, "VAT", 2m);
			SetChargeAmount(statementLine3, "LQT", 3m);
			SetChargeAmount(statementLine3, "AGT", 4m);
			SetChargeAmount(statementLine3, "SCT", 5m);
			SetChargeAmount(statementLine3, "TRT", 6m);
			SetChargeAmount(statementLine3, "EDT", 7m);
			SetChargeAmount(statementLine3, "PLT", 8m);
			SetChargeAmount(statementLine3, "PMT", 9m);
			SetChargeAmount(statementLine3, "VFV", 10m);
			#endregion
			Factory.Save();
		}
		void SetChargeAmount(CusStatementLine statementLine, ZString chargeType, ZDecimal chargeAmount)
		{
			var charge = statementLine.Charges.AddNew();
			charge.B4_ChargeType = chargeType;
			charge.B4_ChargeAmount = chargeAmount;
		}

		ZGuid testPayerPK;
	}
}
