using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using Enterprise.Customs.Business;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

class EdecIMPGoodsItemDataProviderTest : EdecGoodsItemDataProviderTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	protected override EdecGoodsItemDataProvider GetEdecGoodsItemDataProvider(CusEntryLine entryLine) => EdecIMPGoodsItemDataProvider.New(entryLine);

	public override void TestProvider()
	{
		invoiceLine.JI_Tariff = "10001000123456";
		invoiceLine.JI_WeightIncludingInnerPackage = 150;
		invoiceLine.JI_WeightIncludingInnerPackageUQ = "KG";
		invoiceLine.JI_StorageType = RefCusCodeTestHelper.ValidStorageTypeCode;

		base.TestProvider();
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(messageBuilder.CustomsNetWeight), 150m, messageBuilder.CustomsNetWeight);
			AssertEquals(nameof(messageBuilder.StorageType), RefCusCodeTestHelper.ValidStorageTypeCode, messageBuilder.StorageType);
			AssertNull(nameof(messageBuilder.UNDangerousGoodsCode), messageBuilder.UNDangerousGoodsCode);
		});
	}

	public void TestFeeType()
	{
		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		AssertEquals($"Is {nameof(EdecIMPFeeDataProvider)}", true, dataProvider.Fees is IEnumerable<EdecIMPFeeDataProvider>);
		AssertSame("Cached", dataProvider.Fees, dataProvider.Fees);
	}

	public void TestCustomsNetWeight()
	{
		declaration.JE_MessageType = MessageType;
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertNull("When MessageType is Import and WeightIncludingInnerPackage is not set, CustomsNetWeight should be null", messageBuilder.CustomsNetWeight);

		messageBuilder = GetConfiguredGoodsItemDataProvider((i, v) => i.JI_WeightIncludingInnerPackageUQ = v, (i, v) => i.JI_WeightIncludingInnerPackage = v, Core.Constants.Weight.Grams, 10.01m, Core.Constants.Weight.Kilograms, 1);

		AssertEquals(nameof(messageBuilder.CustomsNetWeight), 1.1m, messageBuilder.CustomsNetWeight);
	}

	public void TestOrigin()
	{
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertNotNull("Origin should be defined", messageBuilder.Origin);
			AssertEquals(true, messageBuilder.Origin is IEdecGoodsItemOrigin);
		});
	}

	public void TestGoodsItemValuation()
	{
		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertNotNull(dataProvider.Valuation);
			AssertSame("Cached", dataProvider.Valuation, dataProvider.Valuation);
		});
	}

	public override void TestAdditionalTaxes()
	{
		var additionalTariffDetail = invoiceLine.AdditionalTaxes.AddNew();
		additionalTariffDetail.BZ_Tariff = "123-456";
		var dataProvider = GetEdecGoodsItemDataProvider(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("collection provided", 1, dataProvider.AdditionalTaxes.Count());
			AssertSame("cached", dataProvider.AdditionalTaxes, dataProvider.AdditionalTaxes);
		});
	}

	public void TestStatisticalCode()
	{
		invoiceLine.JI_Tariff = "10001000123456";
		invoiceLine.InAndOutwardProcessingRepair = true;
		invoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
		var messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertEquals(nameof(messageBuilder.StatisticalCode), "456", messageBuilder.StatisticalCode);

		invoiceLine.JI_Procedure = ProcedureCodesEdec.ExemptFromDuty;
		messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertEquals(nameof(messageBuilder.StatisticalCode), "456", messageBuilder.StatisticalCode);

		invoiceLine.InAndOutwardProcessingRepair = false;
		messageBuilder = GetEdecGoodsItemDataProvider(entryLine);
		AssertNull(messageBuilder.StatisticalCode);
	}
}
