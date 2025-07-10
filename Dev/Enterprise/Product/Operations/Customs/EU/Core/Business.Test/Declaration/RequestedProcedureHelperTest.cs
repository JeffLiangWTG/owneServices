using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class RequestedProcedureHelperTest : TestCaseWithFactory
	{
		public void TestGetCachedRequestedProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC1", "11", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "A", "CPC2", "22", "222", "CPC2 Desc", "EXP", group: "H2,H7");
			helper.CreateRefCusProcedure(currentCountry, "B", "CPC3", "33", "333", "CPC3 Desc", "EXP", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC1", "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC2", "CPC2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "CPC3", "CPC3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var procedureCodeList = RequestedProcedureHelper.GetCachedRequestedProcedureCodeList(Factory, currentCountry, "IMP", "H1");
			AssertEquals("Only IMP and H1 is included", "CPC1", procedureCodeList.CodesAsString);

			procedureCodeList = RequestedProcedureHelper.GetCachedRequestedProcedureCodeList(Factory, currentCountry, new ZString[] { "IMP", "EXP" }, "H2");
			AssertEquals("Both EXP and IMP and H2 are included", "CPC1, CPC2", procedureCodeList.CodesAsString);
		}

		public void TestGetCachedPreviousProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "01", "P1", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "02", "P1", "111", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "04", "P3", "333", "CPC3 Desc", "EXP", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P1", "Pre1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P2", "Pre2 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "P3", "Pre3 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var codeList = RequestedProcedureHelper.GetCachedPreviousProcedureCodeList(Factory, currentCountry, "IMP", "H1", "01");
			AssertEquals("Only IMP, H1 and 01 is included", "P1", codeList.CodesAsString);

			codeList = RequestedProcedureHelper.GetCachedPreviousProcedureCodeList(Factory, currentCountry, new ZString[] { "IMP", "EXP" }, "H1", ZString.Empty);
			AssertEquals("Both IMP, EXP and H1 are included", "P1, P3", codeList.CodesAsString);
		}

		public void TestGetCachedAdditionalProcedureCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "01", "P1", "C01", "CPC1 Desc", "IMP", group: "H1,H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "01", "P2", "C02", "CPC1 Desc", "IMP", group: "H2");
			helper.CreateRefCusProcedure(currentCountry, "B", "04", "P4", "C03", "CPC3 Desc", "EXP", group: "H1");

			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "Procedure Code");
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C01", "C01 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C02", "C02 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(currentCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "C03", "C03 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var codeList = RequestedProcedureHelper.GetCachedAdditionalProcedureCodeList(Factory, currentCountry, "IMP", "H1", "01", "P1");
			AssertEquals("Only IMP, H1, 01 amd P1 is included", "C01", codeList.CodesAsString);

			codeList = RequestedProcedureHelper.GetCachedAdditionalProcedureCodeList(Factory, currentCountry, new ZString[] { "IMP", "EXP" }, "H1", ZString.Empty, ZString.Empty);
			AssertEquals("Both IMP, EXP and H1 are included", "C01, C03", codeList.CodesAsString);
		}

		public void TestApplyAllowedComparisonOperatorList()
		{
			var moduleTextFilter = new ModuleTextFilter("TestCEI_ProcedureFilter", CusEntryInstructionSchema.CEI_Procedure);
			RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(moduleTextFilter);
			AssertEquals("Comparison Operator List", "exact", moduleTextFilter.ComparisonOperator_List.CodesAsString);
		}

		public void TestGetJobDeclarationFromRequestedProcedureQuery()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "1122345";
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "1132345";
			Factory.Save();

			var query = RequestedProcedureHelper.GetJobDeclarationFromRequestedProcedureQuery("112%");
			var loadedDeclaration = Factory.Load<JobDeclaration>(query).Single();

			AssertSame(declaration, loadedDeclaration);
		}

		public void TestGetCusEntryHeaderFromRequestedProcedureQuery()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "1122345";
			var entryHeader2 = declaration1.CustomsEntryHeaders.AddNew();
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			var invoiceLine2 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "1132345";
			var declaration3 = Factory.New<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			var entryLine3 = entryHeader3.MergedLines.AddNew();
			var invoiceLine3 = declaration3.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "1132345";
			Factory.Save();

			var query = RequestedProcedureHelper.GetCusEntryHeaderFromRequestedProcedureQuery("112%");
			var loadedCusEntryHeader = Factory.Load<CusEntryHeader>(query).Single();

			AssertSame(entryHeader1, loadedCusEntryHeader);
		}

		public void TestGetRequestedProcedureFilterTextWithWildcards()
		{
			AssertEquals("10%", RequestedProcedureHelper.GetRequestedProcedureFilterTextWithWildcards("10"));
		}

		public void TestGetPreviousProcedureFilterTextWithWildcards()
		{
			AssertEquals("__01%", RequestedProcedureHelper.GetPreviousProcedureFilterTextWithWildcards("01"));
		}

		public void TestGetAdditionalProcedureFilterTextWithWildcards()
		{
			AssertEquals("____121", RequestedProcedureHelper.GetAdditionalProcedureFilterTextWithWildcards("121"));
		}
	}
}
