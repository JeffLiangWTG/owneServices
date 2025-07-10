using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class IdentificationProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(IdentificationProvider.New(null));
			AssertType<IdentificationProvider>(IdentificationProvider.New(Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew()));
		}

		public void TestImporterRegistrationNumber()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "IMPORTER COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;

			var dataProvider = IdentificationProvider.New(declaration.CustomsEntryInstructions.AddNew());
			AssertNull("ImporterRegistrationNumber", dataProvider.ImporterRegistrationNumber);
			AssertNull("ImporterNumberType", dataProvider.ImporterNumberType);

			declaration.JE_OH_Importer = consignee.PK;
			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
			dataProvider = IdentificationProvider.New(declaration.CustomsEntryInstructions[0]);

			AssertEquals("ImporterRegistrationNumber", "58500398000105", dataProvider.ImporterRegistrationNumber);
			AssertEquals("ImporterNumberType", "CNPJ", dataProvider.ImporterNumberType);

			consignee.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			consignee.PrimaryRegistrationNumber.Number = "999.999.999-80";
			AssertEquals("ImporterRegistrationNumber", "99999999980", dataProvider.ImporterRegistrationNumber);
			AssertEquals("ImporterNumberType", "CPF", dataProvider.ImporterNumberType);
		}

		public void TestAdditionalInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var dataProvider = IdentificationProvider.New(declaration.CustomsEntryInstructions.AddNew());

			AssertEquals("AdditionalInformation empty", ZString.Empty, dataProvider.AdditionalInformation);

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.FreeTextAndSystemGenerated;
			instruction.AdditionalInformation = "TEST_AUTO";
			instruction.AdditionalInformationManual = "TEST_MANUALLY";
			dataProvider = IdentificationProvider.New(instruction);

			AssertEquals("AdditionalInformation must be", $"TEST_MANUALLY{System.Environment.NewLine}TEST_AUTO", dataProvider.AdditionalInformation);
		}
	}
}
