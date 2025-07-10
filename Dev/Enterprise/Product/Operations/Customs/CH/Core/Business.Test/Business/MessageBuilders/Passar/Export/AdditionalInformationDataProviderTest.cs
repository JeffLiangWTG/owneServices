using System.Linq;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalInformationDataProvider))]
sealed class AdditionalInformationDataProviderTest : BasePassarDataProviderTest<AdditionalInformationDataProvider>
{
	public void TestNewCollectionEmtpy() => AssertEquals("empty collection", Enumerable.Empty<AdditionalInformationDataProvider>(), AdditionalInformationDataProvider.NewCollection(null));

	public void TestNewCollectionUseLineNo() => AssertNewCollection(false, 2, 2);

	public void TestNewCollectionGenerateSequenceNumber() => AssertNewCollection(true, 2, 1);

	public void TestConstructorNullArgument() => AssertNull(AdditionalInformationDataProvider.New(null, 0));

	public void TestNewCollectionWithSubTypeEmtpy() => AssertEquals("empty collection with subtype", Enumerable.Empty<AdditionalInformationDataProvider>(), AdditionalInformationDataProvider.NewCollection(null, string.Empty));

	public void TestNewCollectionWithSubType() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var addInfo2 = InvoiceLine.AdditionalInformations.AddNew();
		addInfo2.CSI_SubType = "INF";
		var addInfo3 = InvoiceLine.AdditionalInformations.AddNew();
		addInfo3.CSI_SubType = "INF";
		var addInfo4 = InvoiceLine.AdditionalInformations.AddNew();
		addInfo4.CSI_SubType = "REF";

		var dataProviders = AdditionalInformationDataProvider.NewCollection(InvoiceLine.AdditionalInformations, "INF");

		AssertEquals(2, dataProviders.Count());
		AssertEquals("Sequence at 1 index", 1, dataProviders.ElementAt(0).SequenceNumber);
		AssertEquals("Sequence at 2 index", 2, dataProviders.ElementAt(1).SequenceNumber);
	});

	public void TestProvider() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var additionalInformation = InvoiceLine.AdditionalInformations.AddNew();
		additionalInformation.CSI_Code = "ABCD";
		additionalInformation.CSI_Description = "Sample Description";
		additionalInformation.CSI_LineNo = 1;

		AssertEquals("Code", "ABCD", DataProvider.Code);
		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("Text", "Sample Description", DataProvider.Text);
	});

	public void TestNew() => CombineAssertions(() =>
	{
		var additionalInformationDataProvider = AdditionalInformationDataProvider.New(10, "W1101", "3GCUKTEC6EG430795");

		AssertEquals("SequenceNumber", 10, additionalInformationDataProvider.SequenceNumber);
		AssertEquals("Code", "W1101", additionalInformationDataProvider.Code);
		AssertEquals("Text", "3GCUKTEC6EG430795", additionalInformationDataProvider.Text);
	});

	public void TestNewCusSupportingInfo() => CombineAssertions(() =>
	{
		var supportingInfo = Factory.New<CusSupportingInfo>();
		supportingInfo.CSI_Code = "W1101";
		supportingInfo.CSI_Description = "3GCUKTEC6EG430795";

		var additionalInformationDataProvider = AdditionalInformationDataProvider.New(supportingInfo, 10);

		AssertEquals("SequenceNumber", 10, additionalInformationDataProvider.SequenceNumber);
		AssertEquals("Code", "W1101", additionalInformationDataProvider.Code);
		AssertEquals("Text", "3GCUKTEC6EG430795", additionalInformationDataProvider.Text);
	});

	protected override AdditionalInformationDataProvider CreateDataProvider() => AdditionalInformationDataProvider.NewCollection(InvoiceLine.AdditionalInformations).First();

	void AssertNewCollection(bool generateSequenceNumber, int initialLineNo, int expectedLineNo) => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var additionalInformation1 = EntryInstruction.AdditionalInformations.AddNew();
		additionalInformation1.CSI_LineNo = initialLineNo;
		var additionalInformation2 = EntryInstruction.AdditionalInformations.AddNew();
		additionalInformation2.CSI_LineNo = initialLineNo + 1;

		var dataProviders = AdditionalInformationDataProvider.NewCollection(EntryInstruction.AdditionalInformations, generateSequenceNumber);

		AssertEquals(2, dataProviders.Count());
		AssertEquals("Sequence first element", expectedLineNo, dataProviders.ElementAt(0).SequenceNumber);
		AssertEquals("Sequence first element", expectedLineNo + 1, dataProviders.ElementAt(1).SequenceNumber);
	});
}
