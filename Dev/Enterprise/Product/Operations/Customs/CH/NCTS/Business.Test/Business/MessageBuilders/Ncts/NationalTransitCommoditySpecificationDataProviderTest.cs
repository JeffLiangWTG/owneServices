using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NationalTransitCommoditySpecificationDataProvider))]
class NationalTransitCommoditySpecificationDataProviderTest : BaseDepartureDataProviderTest<ICommoditySpecification, NctsHeaderDepartureMessageSendingObject>
{
	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertEquals("BorderValue", 0, DataProvider.BorderValue);
		AssertEquals("InvoiceCurrency", null, DataProvider.InvoiceCurrency);
		AssertEquals("GoodsReturned", null, DataProvider.GoodsReturned);
		AssertEquals("NonTradingGoods", null, DataProvider.NonTradingGoods);
		AssertEquals("OwnPropulsion", null, DataProvider.OwnPropulsion);
		AssertEquals("CompensationType", 0, DataProvider.CompensationType);
		AssertEquals("Repair", null, DataProvider.Repair);
		AssertNull("CountryOfOrigin", DataProvider.CountryOfOrigin);
		AssertNull("CountryOfProduction", DataProvider.CountryOfProduction);
		AssertNull("InvoiceValue", DataProvider.InvoiceValue);
		AssertNull("NetAssessment", DataProvider.NetAssessment);
		AssertNull("Preference", DataProvider.Preference);
		AssertNull("AdditionalTaxes", DataProvider.AdditionalTaxes);
		AssertNull("Fees", DataProvider.Fees);
		AssertNull("NetWeightAssessment", DataProvider.NetWeightAssessment);
		AssertNull("TaxInformation", DataProvider.TaxInformation);
	});

	public void TestRestrictions() => CombineAssertions(() =>
	{
		DepartureGoodsItem.Restrictions.AddNew();
		DepartureGoodsItem.Restrictions.AddNew();
		AssertEquals("count", 2, DataProvider.Restrictions.Count);
		AssertSame("cached", DataProvider.Restrictions, DataProvider.Restrictions);
	});

	public void TestRestrictionObligation() => CombineAssertions(() =>
	{
		AssertEquals("No restriction", false, DataProvider.RestrictionObligation);

		ResetDataProvider();
		DepartureGoodsItem.Restrictions.AddNew();
		AssertEquals("With restrictions", true, DataProvider.RestrictionObligation);
	});

	public void TestAdditionalInformation() => CombineAssertions(() =>
	{
		var additionalDoc1 = DepartureGoodsItem.AdditionalInfos.AddNew();
		additionalDoc1.CSI_SubType = "INF";
		additionalDoc1.CSI_Code = "A1100";
		additionalDoc1.CSI_Description = "Description A1100";
		var additionalDoc2 = DepartureGoodsItem.AdditionalInfos.AddNew();
		additionalDoc2.CSI_SubType = "XXX";
		additionalDoc2.CSI_Code = "X1100";
		additionalDoc2.CSI_Description = "Description X1100";
		var additionalDoc3 = DepartureGoodsItem.AdditionalInfos.AddNew();
		additionalDoc3.CSI_SubType = "INF";
		additionalDoc3.CSI_Code = "A1101";
		additionalDoc3.CSI_Description = "Description A1101";

		AssertEquals("2 AdditionalInfos of Type INF", 2, DataProvider.AdditionalInformations.Count);

		var additionalInfo1 = DataProvider.AdditionalInformations.ToCollection()[0];
		AssertEquals("AdditionalInfo1 SequenceNo", 1, additionalInfo1.SequenceNumber);
		AssertEquals("AdditionalInfo1 Code", "A1100", additionalInfo1.Code);
		AssertEquals("AdditionalInfo1 Description", "Description A1100", additionalInfo1.Text);

		var additionalInfo2 = DataProvider.AdditionalInformations.ToCollection()[1];
		AssertEquals("AdditionalInfo2 SequenceNo", 2, additionalInfo2.SequenceNumber);
		AssertEquals("AdditionalInfo2 Code", "A1101", additionalInfo2.Code);
		AssertEquals("AdditionalInfo2 Description", "Description A1101", additionalInfo2.Text);
	});

	protected override ICommoditySpecification CreateDataProvider() => NationalTransitCommoditySpecificationDataProvider.New(DepartureGoodsItem);
}
