using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Customs.IT.NCTS.DataTransfer.Testing;

sealed class DepartureGoodsItemFeeDataObjectWriterTest : DataObjectWriterTest
{
	public void TestPopulateDataObject()
	{
		SetUpRefDb();
		Factory.Save();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var fee = nctsHeader.MovementHeader.GoodsItems.AddNew().Fees.AddNew();
		fee.BFE_ChargeAmount = 100m;
		fee.BFE_BaseValue = 12m;
		fee.BFE_MethodOfCalculation = NctsCargoDescFeeMethodOfCalculationList.Codes.Multiplicative;
		fee.BFE_MethodOfPayment = "A";
		fee.BFE_ChargeType = NctsCargoDescFeeChargeTypeList.Codes._149;
		fee.BFE_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

		var writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeader)));
		var dataObject = writer.GetDataObject(nctsHeader);

		var invoices = dataObject.CommercialInfo.CommercialInvoiceCollection;
		AssertEquals("CommercialInvoiceCollection Count", 1, invoices.Count);
		var invoiceLines = dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
		AssertEquals("CommercialInvoiceLineCollection Count", 1, invoiceLines.Count);
		var taxOrFeeCollection = invoiceLines[0].TaxOrFeeCollection;
		AssertEquals("TaxOrFeeCollection Count", 1, taxOrFeeCollection.Count);

		CombineAssertions(() =>
		{
			var taxOrFee = taxOrFeeCollection[0];
			AssertEquals("Amount", 100m, taxOrFee.Amount);
			AssertNull("BaseQuantity", taxOrFee.BaseQuantity);
			AssertNull("BaseQuantityUQ", taxOrFee.BaseQuantityUQ);
			AssertEquals("BaseValue", 12m, taxOrFee.BaseValue);
			AssertEquals("MethodOfCalculation Code", NctsCargoDescFeeMethodOfCalculationList.Codes.Multiplicative, taxOrFee.MethodOfCalculation.Code);
			AssertEquals("MethodOfCalculation Description", NctsCargoDescFeeMethodOfCalculationList.Descriptions.Multiplicative, taxOrFee.MethodOfCalculation.Description);
			AssertEquals("MethodOfPayment Code", "A", taxOrFee.MethodOfPayment.Code);
			AssertEquals("MethodOfPayment Description", "A Desc", taxOrFee.MethodOfPayment.Description);
			AssertEquals("RateReasonOverride Code", RateOverrideReasonList.Codes.Additional, taxOrFee.RateReasonOverride.Code);
			AssertEquals("RateReasonOverride Description", RateOverrideReasonList.Descriptions.Additional, taxOrFee.RateReasonOverride.Description);
			AssertEquals("Type Code", NctsCargoDescFeeChargeTypeList.Codes._149, taxOrFee.Type.Code);
			AssertEquals("Type Description", NctsCargoDescFeeChargeTypeList.Descriptions._149, taxOrFee.Type.Description);
		});
	}

	void SetUpRefDb()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
		var cusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "A Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
	}
}
