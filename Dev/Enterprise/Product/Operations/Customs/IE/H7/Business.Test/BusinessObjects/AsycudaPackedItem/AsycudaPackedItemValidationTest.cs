using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	class AsycudaPackedItemValidationTest : TestCaseWithFactory
	{
		public void TestCheckAPI_GrossWeight()
		{
			var item = Factory.New<AsycudaManifestHeader>()
				.Bills.AddNew()
				.PackedItems.AddNew();
			item.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertHasMessageError(item.API_GrossWeightInfo, "You have not entered a Gross Weight.");

				item.API_GrossWeight = -3;
				AssertHasErrorContaining(item.API_GrossWeightInfo, "Please enter a non-negative value.");
			});
		}

		public void TestCheckAPI_Tariff()
		{
			var item = Factory.New<AsycudaManifestHeader>()
				.Bills.AddNew()
				.PackedItems.AddNew();
			item.API_Tariff = "123456";
			item.RunPreSaveValidation();

			AssertHasMessageError(item.API_TariffInfo, "[BR0020] The code you have selected is not in the list.");

			item.API_Tariff = IE.Business.Constants.TariffCodes.H7InvalidTariffCodes.First();
			AssertHasMessageError(item.API_TariffInfo, "[CD0185] Tariff cannot contain codes '3303.00.10 00' or '3303.00.90 00'.");
		}

		public void TestCheckAPI_GoodsValue_ProcedureC07()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C07;

			var item = bill.PackedItems.AddNew();
			item.API_GoodsValue = 160m;
			item.API_RX_NKGoodsValueCurrency = "EUR";
			item.RunPreSaveValidation();

			AssertHasMessageError(item.API_GoodsValueInfo, "[BR600005] Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.");

			item.API_GoodsValue = 10m;
			item.RunPreSaveValidation();

			AssertNoMessageError(item.API_GoodsValueInfo, "[BR600005] Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.");

			var item2 = bill.PackedItems.AddNew();
			item2.API_GoodsValue = 150m;
			item2.API_RX_NKGoodsValueCurrency = "EUR";
			item.RunPreSaveValidation();
			AssertHasMessageError(item.API_GoodsValueInfo, "[BR600005] Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.");
		}

		public void TestCheckAPI_GoodsValue_ProcedureC08()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C08;

			var item = bill.PackedItems.AddNew();
			item.API_GoodsValue = 40m;
			item.API_RX_NKGoodsValueCurrency = "EUR";
			item.RunPreSaveValidation();

			AssertNoMessageError(item.API_GoodsValueInfo, "[BR600005] Sum of Intrinsic Value (Items), Transport Value and Insurance Value must not exceed EUR 45 when Add. Procedure(s) is C08.");

			bill.ABL_InsuranceValue = 10m;
			bill.ABL_RX_NKInsuranceValueCurrency = "EUR";
			item.RunPreSaveValidation();

			AssertHasMessageError(item.API_GoodsValueInfo, "[BR600005] Sum of Intrinsic Value (Items), Transport Value and Insurance Value must not exceed EUR 45 when Add. Procedure(s) is C08.");
		}

		public void TestValidatePackedItemPreviousDocuments()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			header.AMA_ApplicationCode = SubmitTypeList.Codes.V1;
			bill.ABL_ShipmentType = SubStyleCodeList.Codes.NormalDeclaration;
			packedItem.Validation.ValidateAll();
			AssertHasRowMessageError("Should error for V1, Normal Declaration and no Previous Document", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2011RuleMessage());

			header.AMA_ApplicationCode = SubmitTypeList.Codes.V2;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error if not V1", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2011RuleMessage());

			header.AMA_ApplicationCode = SubmitTypeList.Codes.V1;
			bill.ABL_ShipmentType = SubStyleCodeList.Codes.PreliminaryDeclaration;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error if not Normal Declaration", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2011RuleMessage());

			bill.ABL_ShipmentType = SubStyleCodeList.Codes.NormalDeclaration;
			packedItem.PreviousDocuments.AddNew();
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error if any Previous Document exists", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2011RuleMessage());
		}

		public void TestValidatePackedItemContainsMandatoryCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var supportingDoc = packedItem.SupportingDocuments.AddNew();

			header.AMA_ApplicationCode = SubmitTypeList.Codes.V1;
			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C07;
			packedItem.Validation.ValidateAll();
			AssertHasRowMessageError("V1 Should error when when mandatory code not included", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2037RuleMessage());

			supportingDoc.CSI_Code = IE.Business.Constants.SupportingDocumentCodes.H7NonC08MandatoryCodes[0];
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error when containing mandatory code", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2037RuleMessage());

			header.AMA_ApplicationCode = SubmitTypeList.Codes.V2;
			supportingDoc.CSI_Code = ZString.Empty;
			packedItem.Validation.ValidateAll();
			AssertHasRowMessageError("V2 Should error when when mandatroy code not included", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2032RuleMessage());

			supportingDoc.CSI_Code = IE.Business.Constants.SupportingDocumentCodes.H7NonC08MandatoryCodes[0];
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error when containing mandatory code", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2032RuleMessage());

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C08;
			supportingDoc.CSI_Code = ZString.Empty;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error when when procedure is C08", packedItem, header.ValidationConfiguration.ValidationMessage.GetBR2032RuleMessage());
		}

		public void TestValidatePackedItemHaveTransportDocument()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.Validation.ValidateAll();
			AssertHasRowMessageError("Should error when no transport doc", packedItem, header.ValidationConfiguration.ValidationMessage.GetC0634RuleMessage());

			var transportDoc = packedItem.AdditionalDocuments.AddNew();
			transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error when transport doc on packed item", packedItem, header.ValidationConfiguration.ValidationMessage.GetC0634RuleMessage());

			bill.PackedItems.Clear();
			transportDoc = bill.AdditionalDocuments.AddNew();
			transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("Should not error when transport doc on bill", packedItem, header.ValidationConfiguration.ValidationMessage.GetC0634RuleMessage());
		}
	}
}
