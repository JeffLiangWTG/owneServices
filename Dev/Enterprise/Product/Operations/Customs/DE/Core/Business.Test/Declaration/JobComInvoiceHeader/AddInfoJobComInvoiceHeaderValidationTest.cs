using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "2", "EUN Code", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "DE Code", yesterday, tomorrow);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceHeader.ZG_AgreedPlaceCodeInfo, "2", "1");
		}

		public void TestCheckZG_AgreedPlaceCode_StockMovement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			var invoice = declaration.Invoices.AddNew();

			invoice.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoMessageErrors(invoice.ZG_AgreedPlaceCodeInfo);
		}

		public void TestCheckZG_TransportChargesMethodOfPayment_WarningIfNotEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var header = declaration.Invoices.AddNew();
			ValidationTestHelper.AssertWarningIfNotEntered(header.ZG_TransportChargesMethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZG_TransportChargesMethodOfPayment_ListValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var header = declaration.Invoices.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(header.ZG_TransportChargesMethodOfPaymentInfo, "1", DEExportMethodOfPaymentList.Codes.A);
		}

		public void TestCheckZG_IncoTermDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(header.ZG_IncoTermDescriptionInfo, header.JZ_IncoTermInfo, (ZString)Core.Constants.IncoTerms.Other);
		}
	}
}
