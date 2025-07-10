using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class NRCanPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_PackUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AE", "Aerosol", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var message = "The code you have selected is not in the list.";

			var header = SetUpNRCanPGAHeader();
			header.CA_RDAProgramInd = YesNoList.Codes.Yes;
			header.CA_PackQty1 = 1.22m;
			header.CA_PackQty2 = 2.22m;
			header.CA_PackQty3 = 3.22m;

			header.AddInfoValidation.ValidateCA_PackUQ1();
			header.AddInfoValidation.ValidateCA_PackUQ2();
			header.AddInfoValidation.ValidateCA_PackUQ3();
			AssertHasMessageErrorContaining(header.CA_PackUQ1Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.CA_PackUQ2Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.CA_PackUQ3Info, MandatoryValidation.YouHaveNotEntered);

			header.CA_PackUQ1 = "AE";
			header.CA_PackUQ2 = "AE";
			header.CA_PackUQ3 = "AE";
			AssertNoMessageError(header.CA_PackUQ1Info, message);
			AssertNoMessageError(header.CA_PackUQ2Info, message);
			AssertNoMessageError(header.CA_PackUQ3Info, message);
			AssertNoMessageErrorContaining(header.CA_PackUQ1Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.CA_PackUQ2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.CA_PackUQ3Info, MandatoryValidation.YouHaveNotEntered);

			header.CA_PackUQ1 = "AA";
			header.CA_PackUQ2 = "AA";
			header.CA_PackUQ3 = "AA";
			AssertHasMessageErrorContaining(header.CA_PackUQ1Info, message);
			AssertHasMessageErrorContaining(header.CA_PackUQ2Info, message);
			AssertHasMessageErrorContaining(header.CA_PackUQ3Info, message);
		}

		public void TestCheckCA_CaratWeight()
		{
			var header = SetUpNRCanPGAHeader();
			header.CA_RDAProgramInd = YesNoList.Codes.Yes;
			header.CA_CaratWeight = 0;
			AssertHasMessageErrorContaining(header.CA_CaratWeightInfo, MandatoryValidation.YouHaveNotEntered);
			header.CA_CaratWeight = 5;
			AssertNoMessageError(header.CA_CaratWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_AuthorizedProductID()
		{
			var header = SetUpNRCanPGAHeader();
			header.CA_EXPProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_AuthorizedProductID();
			AssertHasMessageErrorContaining(header.CA_AuthorizedProductIDInfo, "If Trade Name is not provided, Authorized Product ID should not be empty.");

			header.CA_EXPProgramInd = YesNoList.Codes.No;
			header.AddInfoValidation.ValidateCA_AuthorizedProductID();
			AssertNoMessageErrorContaining(header.CA_AuthorizedProductIDInfo, "If Trade Name is not provided, Authorized Product ID should not be empty.");

			header.InvoiceLine.CA_TradeName = "Trade";
			header.AddInfoValidation.ValidateCA_AuthorizedProductID();
			AssertNoMessageErrorContaining(header.CA_AuthorizedProductIDInfo, "If Trade Name is not provided, Authorized Product ID should not be empty.");
		}

		public void TestCheckCA_AuthorizedParty()
		{
			var header = SetUpNRCanPGAHeader();
			header.CA_EXPProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_AuthorizedParty();
			AssertHasMessageErrorContaining(header.CA_AuthorizedPartyInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_AuthorizedParty = "IMP";
			AssertNoMessageErrorContaining(header.CA_AuthorizedPartyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_IntendedUseCode()
		{
			string message = "You have not entered a value.";
			string nr04IsNotAllowedForOEE = "NR04 is only allowed for Explosives Program.";
			var header = SetUpNRCanPGAHeader();
			header.CA_EEFProgramInd = YesNoList.Codes.Yes;
			header.CA_IntendedUseCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeInfo, message);

			header.CA_IntendedUseCode = NRCanIntendedUseCodes.Codes.NR04;
			AssertHasMessageError(header.CA_IntendedUseCodeInfo, nr04IsNotAllowedForOEE);

			header.CA_IntendedUseCode = NRCanIntendedUseCodes.Codes.NR01;
			AssertNoMessageError(header.CA_IntendedUseCodeInfo, message);

			header.CA_EXPProgramInd = YesNoList.Codes.Yes;
			header.CA_IsNotRegulatedByExplosives = false;
			AssertNoMessageError(header.CA_IntendedUseCodeInfo, nr04IsNotAllowedForOEE);

			header.CA_IsNotRegulatedByExplosives = true;
			AssertHasMessageError(header.CA_IntendedUseCodeInfo, nr04IsNotAllowedForOEE);

			header.CA_EEFProgramInd = YesNoList.Codes.No;
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertNoMessageError(header.CA_IntendedUseCodeInfo, nr04IsNotAllowedForOEE);
		}

		NRCanPGAHeader SetUpNRCanPGAHeader()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1014";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.JI_Weight = 4;
			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			return invoiceLine.NRCanPGAHeader;
		}
	}
}
