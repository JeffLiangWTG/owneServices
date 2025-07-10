using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class AdditionalReferenceDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		var nctsHeader = Factory.New<NctsHeader>();

		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		CombineAssertions(() =>
		{
			AssertNull("null", AdditionalReferenceDataProvider.NewCollection(null));
			AssertEquals("Count", 2, AdditionalReferenceDataProvider.NewCollection(nctsHeader.AdditionalDocuments).Count());
		});
	}

	public void TestSequenceNumber()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		var dataProviders = AdditionalReferenceDataProvider.NewCollection(nctsHeader.AdditionalDocuments);

		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		CombineAssertions(() =>
		{
			AssertEquals("Sequence at 1 index", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, dataProviders.ElementAt(1).SequenceNumber);
		});
	}

	public void TestPropertiesForNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		var additionalInfo = nctsHeader.AdditionalDocuments.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		var dataProvider = AdditionalReferenceDataProvider.NewCollection(nctsHeader.AdditionalDocuments).First();
		AssertProperties(additionalInfo, dataProvider);
	}

	public void TestPropertiesForConsignmentItem()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var additionalInfo = goodsItem.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		var dataProvider = AdditionalReferenceDataProvider.NewCollection(goodsItem.AdditionalInfos).First();
		AssertProperties(additionalInfo, dataProvider);
	}

	void AssertProperties(AdditionalInfo additionalInfo, IAdditionalReference dataProvider)
	{
		const string code = "456";
		additionalInfo.CSI_Code = code;
		const string referenceNumber = "123";
		additionalInfo.CSI_ReferenceNumber = referenceNumber;

		CombineAssertions(() =>
		{
			AssertEquals("SequenceNumber", 1, dataProvider.SequenceNumber);
			AssertEquals("ReferenceNumber", referenceNumber, dataProvider.ReferenceNumber);
			AssertEquals("Type", code, dataProvider.Type);
		});
	}
}
