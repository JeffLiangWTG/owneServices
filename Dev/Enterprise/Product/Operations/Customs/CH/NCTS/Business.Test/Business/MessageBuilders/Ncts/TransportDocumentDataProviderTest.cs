using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class TransportDocumentDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		var nctsHeader = Factory.New<NctsHeader>();

		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		CombineAssertions(() =>
		{
			AssertNull("null", TransportDocumentDataProvider.NewCollection(null));
			AssertEquals("Count", 2, TransportDocumentDataProvider.NewCollection(nctsHeader.AdditionalDocuments).Count());
		});
	}

	public void TestSequenceNumber()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		var dataProviders = TransportDocumentDataProvider.NewCollection(nctsHeader.AdditionalDocuments);

		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		nctsHeader.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		CombineAssertions(() =>
		{
			AssertEquals("Sequence at 1 index", 1, dataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, dataProviders.ElementAt(1).SequenceNumber);
		});
	}

	public void TestProperties()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		var additionalInfo = nctsHeader.AdditionalDocuments.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		var dataProvider = TransportDocumentDataProvider.NewCollection(nctsHeader.AdditionalDocuments).First();

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
