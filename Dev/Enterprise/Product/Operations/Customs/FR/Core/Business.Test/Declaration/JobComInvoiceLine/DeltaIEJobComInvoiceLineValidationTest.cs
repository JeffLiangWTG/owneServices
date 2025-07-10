using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIEJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestCheckJI_ProcedureForRuleNat_044()
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "40";
			procedure.ZZ6_PreviousProcedureCode = "00";
			procedure.ZZ6_Concession = "C08";
			procedure.ZZ6_Description = "description";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_ShipmentType = "EXP";

			invoiceLine.JI_Procedure = "4000C08";
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, "You have entered the supplementary scheme code C08, so the special mention G0008 \"unidentified VAT debtor in France\" must be served at GS level.");

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = FR.Business.UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, "You have entered the supplementary scheme code C08, so the special mention G0008 \"unidentified VAT debtor in France\" must be served at GS level.");

			addInfo.Delete();
			invoiceLine.JI_Procedure = "4000C09";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, "You have entered the supplementary scheme code C08, so the special mention G0008 \"unidentified VAT debtor in France\" must be served at GS level.");

			invoiceLine.JI_Procedure = "4000C08";
			addInfo = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
			addInfo.CSI_Code = FR.Business.UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, "You have entered the supplementary scheme code C08, so the special mention G0008 \"unidentified VAT debtor in France\" must be served at GS level.");

			addInfo.Delete();
			addInfo = invoiceLine.Declaration.AdditionalInfos.AddNew();
			addInfo.CSI_Code = FR.Business.UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError(invoiceLine.JI_ProcedureInfo, "You have entered the supplementary scheme code C08, so the special mention G0008 \"unidentified VAT debtor in France\" must be served at GS level.");
		}

		public void TestCheckJI_OA_ExporterAddress()
		{
			invoiceLine.JI_OA_ExporterAddress = orgHeader.MainAddress.PK;
			var address = invoiceLine.ExporterAddress;
			var info = invoiceLine.JI_OA_ExporterAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, invoiceLine.Validation.ValidateJI_OA_ExporterAddress);
		}

		public void TestCheckJI_OA_ConsigneeAddress()
		{
			invoiceLine.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
			var address = invoiceLine.ConsigneeAddress;
			var info = invoiceLine.JI_OA_ConsigneeAddressInfo;
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(address, info, invoiceLine.Validation.ValidateJI_OA_ConsigneeAddress);
		}

		public void TestValidateBuyerDocAddress()
		{
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(invoiceLine.BuyerDocAddress, orgHeader);

			invoiceLine.BuyerDocAddress.E2_OA_Address = ZGuid.Empty;
			invoiceLine.BuyerDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("No error should show on Buyer because it is not mandatory.", invoiceLine.BuyerDocAddress.OrganisationPKInfo, "Please enter an organization with an EORI or enter full address details.");
		}

		public void TestValidateSellerDocAddress()
		{
			DeclarationValidationTestHelper.AssertEORIOrFullAddress(invoiceLine.SellerDocAddress, orgHeader);

			invoiceLine.SellerDocAddress.E2_OA_Address = ZGuid.Empty;
			invoiceLine.SellerDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("No error should show on Seller because it is not mandatory.", invoiceLine.SellerDocAddress.OrganisationPKInfo, "Please enter an organization with an EORI or enter full address details.");
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			orgHeader = Factory.New<OrgHeader>();
		}

		JobComInvoiceLine invoiceLine;
		OrgHeader orgHeader;
	}
}
