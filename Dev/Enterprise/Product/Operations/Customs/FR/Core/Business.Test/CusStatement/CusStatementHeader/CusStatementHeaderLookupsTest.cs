using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	class CusStatementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			AssertEquals("Status code list lookup content should match ClosingDeclarationStatusList code pair description list", Factory.GetCachedValue<StatementStatusList>().CodesAsString, lookups.StatusList.CodesAsString);
		}

		public void TestProfiles()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ReportingPeriodList.Codes.DAY, "AD49C94A");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI002", ZString.Empty, ReportingPeriodList.Codes.TEN, "B92F8A8B");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ReportingPeriodList.Codes.DAY, "D34C0059");
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G2, "DGE002", ZString.Empty, ReportingPeriodList.Codes.TEN, "E7A1CC18");

			AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), lookups.Profiles.GetAllCodes());

			statement.B2_OH_Importer = importer.PK;
			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"DGI002"
			}, lookups.Profiles.GetAllCodes());

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Export;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"DGE002"
			}, lookups.Profiles.GetAllCodes());
		}

		public void TestPaymentTypeList()
		{
			AssertEquals("MethodOfPayment code list lookup content should match MethodOfPaymentList code pair description list", Factory.GetCachedValue<MethodOfPaymentList>().CodesAsString, lookups.MethodOfPaymentList.CodesAsString);
		}

		public void TestStatementTypeList()
		{
			AssertContainsExactElementsInAnyOrder("Periodicity code list lookup content should match the whole ClosingDeclarationStatusList code pair description list minus 'Unknown' code when statement Entry Number is not set", new[] { "J", "D", "M" }, lookups.PeriodicityList.GetAllCodes());

			statement.EntryNumber = "BLABLA";
			AssertEquals("Periodicity code list lookup content should match the whole ClosingDeclarationStatusList code pair description list when statement Entry Number is set", Factory.GetCachedValue<StatementPeriodicityList>().CodesAsString, lookups.PeriodicityList.CodesAsString);
		}

		public void TestBranchDesignationList()
		{
			AssertEquals("Direction code list lookup content should match ClosingDeclarationDirectionList code pair description list", Factory.GetCachedValue<StatementEntryTypeImpExpList>().CodesAsString, lookups.DirectionList.CodesAsString);
		}

		protected override void SetUp()
		{
			statement = Factory.New<CusStatementHeader>();
			lookups = statement.Lookups;
		}

		CusStatementHeader statement;
		CusStatementHeaderLookups lookups;
	}
}
