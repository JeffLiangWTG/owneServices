using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	public class CusStatementHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingB2_OH_ImporterShouldDefaultB2_StatementType()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ReportingPeriodList.Codes.DAY, "E902C629");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.TEN, "D0A5974E");

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			statement.B2_OH_Importer = importer.PK;
			AssertEquals(StatementPeriodicityList.Codes.Decade, statement.B2_StatementType);
		}

		[TestDate(2020, 1, 15)]
		public void TestSettingB2_StatementTypeShouldDefaultB2_PeriodStartDate()
		{
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			AssertEquals(ZDate.Empty, statement.B2_PeriodStartDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			AssertEquals(new ZDate(2020, 1, 1), statement.B2_PeriodStartDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Month;
			AssertEquals(new ZDate(2019, 12, 1), statement.B2_PeriodStartDate);
		}

		[TestDate(2020, 1, 11)]
		public void TestSettingB2_StatementTypeShouldDefaultB2_PeriodStartDate_WhenItIsExactly11st()
		{
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			AssertEquals(ZDate.Empty, statement.B2_PeriodStartDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			AssertEquals(new ZDate(2020, 1, 1), statement.B2_PeriodStartDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Month;
			AssertEquals(new ZDate(2019, 12, 1), statement.B2_PeriodStartDate);
		}

		[TestDate(2020, 1, 15)]
		public void TestSettingB2_StatementTypeShouldDefaultB2_PeriodEndDate()
		{
			statement.B2_PeriodStartDate = new ZDate(2020, 1, 15);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			AssertEquals(new ZDate(2020, 1, 15), statement.B2_PeriodEndDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			AssertEquals(new ZDate(2020, 1, 10), statement.B2_PeriodEndDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Month;
			AssertEquals(new ZDate(2019, 12, 31), statement.B2_PeriodEndDate);
		}

		[TestDate(2020, 1, 31)]
		public void TestSettingB2_StatementTypeShouldDefaultB2_PeriodEndDate_WhenItIsExactly31st()
		{
			statement.B2_PeriodStartDate = new ZDate(2020, 1, 31);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			AssertEquals(new ZDate(2020, 1, 31), statement.B2_PeriodEndDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			AssertEquals(new ZDate(2020, 1, 20), statement.B2_PeriodEndDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Month;
			AssertEquals(new ZDate(2019, 12, 31), statement.B2_PeriodEndDate);
		}

		public void TestSettingB2_PeriodStartDateShouldDefaultB2_PeriodEndDate()
		{
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			statement.B2_PeriodStartDate = new ZDate(2020, 1, 5);
			AssertEquals(new ZDate(2020, 1, 5), statement.B2_PeriodEndDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Decade;
			statement.B2_PeriodStartDate = new ZDate(2020, 2, 5);
			AssertEquals(new ZDate(2020, 2, 10), statement.B2_PeriodEndDate);

			statement.B2_StatementType = StatementPeriodicityList.Codes.Month;
			statement.B2_PeriodStartDate = new ZDate(2020, 3, 5);
			AssertEquals(new ZDate(2020, 3, 31), statement.B2_PeriodEndDate);
		}

		public void TestSettingB2_BranchDesignationShouldDefaultB2_EntryFilerCode()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI001", ZString.Empty, ReportingPeriodList.Codes.DAY, "C20C53AD");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G2, "DGE001", ZString.Empty, ReportingPeriodList.Codes.TEN, "21126746");

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			statement.B2_OH_Importer = importer.PK;
			AssertEquals("DGI001", statement.B2_EntryFilerCode);

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Export;
			AssertEquals("DGE001", statement.B2_EntryFilerCode);
		}

		public void TestSettingB2_OH_ImporterShouldDefaultB2_EntryFilerCode()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ReportingPeriodList.Codes.DAY, "E902C629");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.TEN, "8ABCD7B5");

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			statement.B2_OH_Importer = importer.PK;
			AssertEquals("DGI002", statement.B2_EntryFilerCode);
		}

		public void TestSettingB2_OH_ImporterShouldDefaultB2_ImporterCustomsID()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupCusCode(OrgCusCode.CodeTypes.BrokerageRegistration, "CBR001");

			statement.B2_OH_Importer = importer.PK;
			AssertEquals("CBR001", statement.B2_ImporterCustomsID);
		}

		public void TestSettingB2_OH_ImporterShouldDefaultB2_ImporterCustomsID_AndTrimItToMaxLength()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupCusCode(OrgCusCode.CodeTypes.BrokerageRegistration, new ZString('A', 254));

			statement.B2_OH_Importer = importer.PK;
			AssertEquals(new ZString('A', 20), statement.B2_ImporterCustomsID);
		}

		public void TestSettingB2_OH_ImporterShouldDefaultB2_CheckNo()
		{
			statement.B2_PaymentType = MethodOfPaymentList.Codes.R;

			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "DAN001");

			statement.B2_OH_Importer = importer.PK;
			AssertEquals("DAN001", statement.B2_CheckNo);
		}

		public void TestSettingB2_PaymentTypeShouldClearB2_CheckNo_IfNeitherRNorM()
		{
			statement.B2_PaymentType = MethodOfPaymentList.Codes.M;
			statement.B2_CheckNo = "123";
			AssertEquals("Give B2_CheckNo a value.", "123", statement.B2_CheckNo);

			statement.B2_PaymentType = ZString.Empty;
			AssertEquals("Setting B2_PaymentType to empty value should clear B2_CheckNo.", ZString.Empty, statement.B2_CheckNo);

			statement.B2_PaymentType = MethodOfPaymentList.Codes.M;
			statement.B2_CheckNo = "123";
			AssertEquals("Give B2_CheckNo a value.", "123", statement.B2_CheckNo);

			statement.B2_PaymentType = MethodOfPaymentList.Codes.A;
			AssertEquals("Setting B2_PaymentType to 'A' should clear B2_CheckNo.", ZString.Empty, statement.B2_CheckNo);

			statement.B2_PaymentType = MethodOfPaymentList.Codes.M;
			statement.B2_CheckNo = "123";
			AssertEquals("Give B2_CheckNo a value.", "123", statement.B2_CheckNo);

			statement.B2_PaymentType = MethodOfPaymentList.Codes.R;
			AssertEquals("Setting B2_PaymentType to 'R/M' should not clear B2_CheckNo.", "123", statement.B2_CheckNo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statement = Factory.New<CusStatementHeader>();
		}

		CusStatementHeader statement;
	}
}
