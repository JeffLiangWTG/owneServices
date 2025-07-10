using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementLine))]
	sealed class CusStatementLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusStatementHeader>().StatementLines.AddNew();
		}

		public void TestChargesAreDeleted()
		{
			var statementLine = Factory.New<CusStatementHeader>().StatementLines.AddNew();
			var charge = statementLine.Charges.AddNew();

			statementLine.Delete();
			Assert(charge.IsDeleted);
		}

		public void TestFormattedNumber()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = GlbCompany.CurrentCompany.PK;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_AssociatedEntry = "0127012112100053179";

			AssertEquals("12345-20-000045M", statementLine.FormattedNumber);
			AssertEquals("0127-012-11-21-0-005317-9", statementLine.FormattedLinePaymentNumber);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.MonthlyReceipt;
			statementLine.B3_AssociatedEntry = "012112100053179";

			AssertEquals("012-11-21-0-005317-9", statementLine.FormattedLinePaymentNumber);
		}

		public void TestEntry()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = GlbCompany.CurrentCompany.PK;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Export;

			AssertNull(statementLine.Entry);

			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals(entry, statementLine.Entry);
		}

		public void TestTax()
		{
			var statementLine = Factory.New<CusStatementHeader>().StatementLines.AddNew();
			var chargeDTY = statementLine.Charges.AddNew();
			chargeDTY.B4_ChargeType = ChargeTypeList.Codes.Duty;
			chargeDTY.B4_ChargeAmount = 1;
			var chargeLQT = statementLine.Charges.AddNew();
			chargeLQT.B4_ChargeType = ChargeTypeList.Codes.LiquorTax;
			chargeLQT.B4_ChargeAmount = 2;
			var chargeAGT = statementLine.Charges.AddNew();
			chargeAGT.B4_ChargeType = ChargeTypeList.Codes.AgricultureTax;
			chargeAGT.B4_ChargeAmount = 3;
			var chargeVAT = statementLine.Charges.AddNew();
			chargeVAT.B4_ChargeType = ChargeTypeList.Codes.VAT;
			chargeVAT.B4_ChargeAmount = 4;
			var chargeVFV = statementLine.Charges.AddNew();
			chargeVFV.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			chargeVFV.B4_ChargeAmount = 5;
			var chargeTRT = statementLine.Charges.AddNew();
			chargeTRT.B4_ChargeType = ChargeTypeList.Codes.TransportationTax;
			chargeTRT.B4_ChargeAmount = 6;
			var chargeEDT = statementLine.Charges.AddNew();
			chargeEDT.B4_ChargeType = ChargeTypeList.Codes.EducationTax;
			chargeEDT.B4_ChargeAmount = 7;
			var chargePLT = statementLine.Charges.AddNew();
			chargePLT.B4_ChargeType = ChargeTypeList.Codes.PenaltyAndInterest;
			chargePLT.B4_ChargeAmount = 8;
			var chargeSCT = statementLine.Charges.AddNew();
			chargeSCT.B4_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			chargeSCT.B4_ChargeAmount = 9;
			var chargePMT = statementLine.Charges.AddNew();
			chargePMT.B4_ChargeType = ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration;
			chargePMT.B4_ChargeAmount = 10;

			AssertEquals(1m, statementLine.DutyAmount);
			AssertEquals(2m, statementLine.LiquorTax);
			AssertEquals(3m, statementLine.AgricultureTax);
			AssertEquals(4m, statementLine.VAT);
			AssertEquals(5m, statementLine.BaseAmount);
			AssertEquals(6m, statementLine.TransportationTax);
			AssertEquals(7m, statementLine.EducationTax);
			AssertEquals(8m, statementLine.PenaltyAndInterest);
			AssertEquals(9m, statementLine.SpecialConsumptionTax);
			AssertEquals(10m, statementLine.PenaltyForLatePayment);
		}

		[TestDate(2025, 01, 01)]
		public void TestIndividualCustomsDisbursementBillNoAndStatementNumber5WNDescription()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = GlbCompany.CurrentCompany.PK;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_StatementNumber = "EM123457789KR";
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement.B2_DueDate = new ZDateTime(2025, 01, 01);
			statement.B2_PaymentAuthorizationDate = new ZDateTime(2024, 12, 31);

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_AssociatedEntry = "0127012112100053179";

			AssertEquals(statement.B2_StatementType, StatementHeaderTypeList.Codes.Invoice);
			Assert(statement.B2_PaymentAuthorizationDate.IsValid);
			AssertEquals("Due Date: 2025-01-01, Paid on 2024-12-31", statementLine.StatementNumber5WNDescription);

			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			AssertEquals("EM123457789KR", statementLine.IndividualCustomsDisbursementBillNo);

			statement.B2_PaymentAuthorizationDate = ZDateTime.Empty;
			Assert(!statement.B2_PaymentAuthorizationDate.IsValid);
			AssertEquals("Due Date: 2025-01-01, Unpaid", statementLine.StatementNumber5WNDescription);
		}

		public void TestChangeMaxLengthAndDecialPlace()
		{
			var statementLine = Factory.New<CusStatementHeader>().StatementLines.AddNew();

			AssertHasCustomAttribute<DecimalPlacesAttribute>(statementLine.GetType(), CusStatementLine.Schema.B3_CustomsFeesTotal, true, attrib => attrib.DecimalPlaces == 0);
		}

		[TestedType(typeof(CusStatementLine.Loader))]
		class CusStatementLineLoaderTest : LoaderTestCase
		{
			protected override BusinessObject.Loader GetNewLoaderToTest()
			{
				return new CusStatementLine.Loader(Factory);
			}

			public void TestLoader()
			{
				var statementHeader1 = Factory.New<CusStatementHeader>();
				statementHeader1.B2_StatementNumber = "0127030012000018260";
				statementHeader1.B2_GC = GlbCompany.CurrentCompany.PK;
				statementHeader1.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
				var statementLine1 = statementHeader1.StatementLines.AddNew();
				statementLine1.B3_EntryNum = "1001";
				statementLine1.B3_EntryType = KRJobMessageTypeList.Codes.Import;

				var statementLine2 = statementHeader1.StatementLines.AddNew();
				statementLine2.B3_EntryNum = "1002";
				statementLine2.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				Factory.Save();

				var query = CusStatementLine.Loader.GetQuery("1001", KRJobMessageTypeList.Codes.Import, GlbCompany.CurrentCompany.PK.ToGuid(), new[] { StatementHeaderTypeList.Codes.Invoice });
				var statementLines = Factory.Load<CusStatementLine>(query);

				AssertEquals(1, statementLines.Length);
				AssertEquals(statementLine1, statementLines[0]);

				var newCompany = Factory.New<GlbCompany>();

				var statementHeader2 = Factory.New<CusStatementHeader>();
				statementHeader2.B2_StatementNumber = "0127030012000018261";
				statementHeader2.B2_GC = newCompany.PK;
				statementHeader2.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;
				var statementLine3 = statementHeader2.StatementLines.AddNew();
				statementLine3.B3_EntryNum = "1002";
				statementLine3.B3_EntryType = KRJobMessageTypeList.Codes.Import;

				var statementLine4 = statementHeader2.StatementLines.AddNew();
				statementLine4.B3_EntryNum = "1002";
				statementLine4.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				Factory.Save();

				query = CusStatementLine.Loader.GetQuery("1002", KRJobMessageTypeList.Codes.Import, newCompany.PK.ToGuid(), new[] { StatementHeaderTypeList.Codes.IndividualCollectionReceipt });
				statementLines = Factory.Load<CusStatementLine>(query);
				AssertEquals(2, statementLines.Length);
				AssertEquals(statementLine3, statementLines[0]);
				AssertEquals(statementLine4, statementLines[1]);
			}
		}
	}
}
