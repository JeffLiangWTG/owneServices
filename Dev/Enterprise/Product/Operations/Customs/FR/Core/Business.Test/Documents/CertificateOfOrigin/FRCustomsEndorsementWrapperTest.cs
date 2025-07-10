using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class FRCustomsEndorsementWrapperTest : TestCaseWithFactory
	{
		public void TestForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICustomsEndorsement wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("Form", "FR", wrapper.Form);

			declaration.JE_EntryStyle = "44";
			wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("Form", "44", wrapper.Form);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "number";
			wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("Form", "44", wrapper.Form);

			entry.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
			entry.EntryInstruction.CEI_SubStyle = "sub";
			wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("Form", "44sub", wrapper.Form);
		}

		public void TestIssuingCountry()
		{
			SetUpCustomsOffice();

			var declaration = Factory.New<JobDeclaration>();
			ICustomsEndorsement wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("IssuingCountry", "", wrapper.IssuingCountry);

			var cauOffice = declaration.CustomsOffices.Cast<EU.Business.EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "CAU") ?? declaration.CustomsOffices.AddNew();
			cauOffice.CY_Type = "EUO";
			cauOffice.CY_Code = "CAU";
			cauOffice.CY_Data = "FR000000";
			wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("When JE_CustomsOffice exists, CustomsOffice", "France", wrapper.IssuingCountry);
		}

		public void TestGetPlace()
		{
			SetUpCustomsOffice();

			var declaration = Factory.New<JobDeclaration>();
			ICustomsEndorsement wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("IssuingCountry", "", wrapper.Place);

			var cauOffice = declaration.CustomsOffices.Cast<EU.Business.EuOfficeCode>().SingleOrDefault(x => x.CY_Code == "CAU") ?? declaration.CustomsOffices.AddNew();
			cauOffice.CY_Type = "EUO";
			cauOffice.CY_Code = "CAU";
			cauOffice.CY_Data = "FR000000";
			wrapper = new FRCustomsEndorsementWrapper(declaration);
			AssertEquals("When JE_CustomsOffice exists, CustomsOffice", "creteil", wrapper.Place);
		}

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eunZZZPK);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000000", "creteil", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
		}

		public void TestShowEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICustomsEndorsement wrapper = new FRCustomsEndorsementWrapper(declaration);
			Assert("ShowEntryNumber should be true in France", wrapper.ShowEntryNumber);
		}
	}
}
