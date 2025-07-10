using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestCheckSellerOrgPK()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			invoiceHeader.JZ_OA_SellerAddress = consignee.PK;
			ValidationTestHelper.AssertErrorIfInvalidPK(invoiceHeader.SellerOrgPKInfo, consignee.Header.PK, consignor.Header.PK);
		}

		public void TestCheckBuyerOrgPK()
		{
			var (consignor, consignee) = SetUpOrganizationData();
			invoiceHeader.JZ_OA_BuyerAddress = consignor.PK;
			ValidationTestHelper.AssertErrorIfInvalidPK(invoiceHeader.BuyerOrgPKInfo, consignor.Header.PK, consignee.Header.PK);
		}

		public void TestCheckJZ_RX_NKInvoice_CurrencyIsMandatory()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertHasMessageErrorContaining("Currency is always required", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.MustBeEntered);

			invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
			AssertNoMessageErrorContaining("Currency is always required", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.MustBeEntered);
		}

		protected override Type GetTypeForTest() => typeof(ImportJobComInvoiceHeaderValidation);

		public void TestCheck_JZ_ValuationCode_MandatoryForEAV() => AssertJZ_ValuationCode_Mandatory(ImportDeclarationTypeList.Codes.EAV);

		public void TestCheck_JZ_ValuationCode_MandatoryForEZA() => AssertJZ_ValuationCode_Mandatory(ImportDeclarationTypeList.Codes.EZA);

		public void TestCheck_JZ_ValuationCode_MandatoryForAZ() => AssertJZ_ValuationCode_Mandatory(ImportDeclarationTypeList.Codes.AZ);

		public void TestCheck_JZ_ValuationCode_MandatoryForVZA() => AssertJZ_ValuationCode_Mandatory(ImportDeclarationTypeList.Codes.VZA);

		public void TestCheck_JZ_ValuationCode_MandatoryForAAV() => AssertJZ_ValuationCode_Mandatory(ImportDeclarationTypeList.Codes.AAV);

		public void TestCheck_JZ_ValuationCode_MandatoryForVAV() => AssertJZ_ValuationCode_Mandatory(ImportDeclarationTypeList.Codes.VAV);

		void AssertJZ_ValuationCode_Mandatory(string ceiStyle)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var propertyInfo = invoiceHeader.JZ_ValuationCodeInfo;
			CombineAssertions(() =>
			{
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				entryInstruction.CEI_Style = ceiStyle;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);
			});
		}

		public void TestValidateHasAtLeastOneInvoiceSupportingDocument()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			declaration.Invoices.Add(invoiceHeader);

			var invoiceValidation = invoiceHeader.Validation;
			var expectedMessage = "At least one Supporting Document of type: N380, N325, N935, D005 or D008 must be present at the Invoice Header level or in all of its Invoice Lines.";
			var notifications = invoiceHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);

			var supportingTypes = invoiceHeader.SupportingDocuments.Helper.GetInvoiceSupportingDocumentTypes().ToArray();

			invoiceValidation.ValidateAll();
			Assert("No Supporting Document Added to the Header, Warning message should be added", notifications.Any(x => x.Message.Contains(expectedMessage)));

			var supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "AAA";
			invoiceValidation.ValidateAll();
			Assert($"Supporting Document: {supportingDocument.CSI_Code}, Added to the Header, Warning message should be found", notifications.Any(x => x.Message.Contains(expectedMessage)));

			foreach (var supportingType in supportingTypes)
			{
				supportingDocument.CSI_Code = supportingType;
				invoiceValidation.ValidateAll();
				Assert($"Supporting Document: {supportingDocument.CSI_Code}, Added to the Header, Warning message should not be found", !notifications.Any(x => x.Message.Contains(expectedMessage)));
			}

			invoiceHeader.SupportingDocuments.RemoveAll();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceValidation.ValidateAll();
			Assert("No Supporting Document Added to the Header and InvoiceLine, Warning message should be added", notifications.Any(x => x.Message.Contains(expectedMessage)));

			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "AAA";
			invoiceValidation.ValidateAll();
			Assert($"Supporting Document: {supportingDocument.CSI_Code}, Added to the InvoiceLine, Warning message should be found", notifications.Any(x => x.Message.Contains(expectedMessage)));

			foreach (var supportingType in supportingTypes)
			{
				supportingDocument.CSI_Code = supportingType;
				invoiceValidation.ValidateAll();
				Assert($"Supporting Document: {supportingDocument.CSI_Code}, Added to the InvoiceLine, Warning message should be found", notifications.Any(x => x.Message.Contains(expectedMessage)));
			}
		}

		public void TestShouldValidateNeedAtLeastOneInvoiceSupportingDocument()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			declaration.Invoices.Add(invoiceHeader);
			var instruction = invoiceHeader.JobDeclaration.CustomsEntryInstructions.AddNew();

			var expectedMessage = "At least one Supporting Document of type: N380, N325, N935, D005 or D008 must be present at the Invoice Header level or in all of its Invoice Lines.";
			var notifications = invoiceHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);

			invoiceHeader.Validation.ValidateAll();
			AssertEquals("No Supporting Document Can Be Added to the Header, Warnings", true, notifications.Any(x => x.Message.Contains(expectedMessage)));

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			notifications = invoiceHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);

			invoiceHeader.Validation.ValidateAll();
			AssertEquals("No Supporting Document Can Be Added to the Header, no Warnings", false, notifications.Any(x => x.Message.Contains(expectedMessage)));
		}

		public override void TestValidateBalance()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_InvoiceAmount = 100.121m;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_InvoiceQuantity = 1;
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.JI_LinePrice = 100.124m;
				declaration.ResumeApportionment();
				AssertNoNotifications(invoiceHeader.JZ_Calc_BalanceInfo);
				invoiceLine.JI_LinePrice = 100.128m;
				declaration.ResumeApportionment();
				AssertHasMessageError(invoiceHeader.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			});
		}
		protected override string MessageType => MessageTypeList.Codes.Import;
		protected override ZBool JZ_IncoTermPlaceIsMandatory => ZBool.True;

		(OrgAddress consignor, OrgAddress consignee) SetUpOrganizationData()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_Code = "H1";
			header1.OrganisationTypes = OrganisationTypes.Consignor;
			var address1 = header1.Addresses.AddNew();
			address1.OA_Address1 = "ADD1";
			address1.AddAddressType(OrgAddressType.Office);

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_Code = "H2";
			header2.OrganisationTypes = OrganisationTypes.Consignee;
			var address2 = header2.Addresses.AddNew();
			address2.OA_Address1 = "ADD2";
			address2.AddAddressType(OrgAddressType.Office);

			Factory.Save();
			return (address1, address2);
		}
	}
}
