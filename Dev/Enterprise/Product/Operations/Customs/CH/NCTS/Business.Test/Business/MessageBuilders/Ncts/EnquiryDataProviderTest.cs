using System;
using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class EnquiryDataProviderTest : BaseDepartureDataProviderTest<EnquiryDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	protected override EnquiryDataProvider CreateDataProvider() => EnquiryDataProvider.New(MessageSendingObject);

	public void TestNew()
	{
		AssertNull("null argument", EnquiryDataProvider.New(null));
	}

	public void TestProvider() => CombineAssertions(() =>
	{
		MessageSendingObject.ReasonCode = "5";
		MessageSendingObject.ReasonText = "text";
		MessageSendingObject.DoubleEntryMRN = "mrn";
		MessageSendingObject.TC11DeliveryDate = new ZDateTime(2023, 5, 12);

		AssertEquals("Reason", "5", DataProvider.Reason);
		AssertEquals("Text", "text", DataProvider.Text);
		AssertEquals("MRNDoubleEntry", "mrn", DataProvider.MRNDoubleEntry);
		AssertEquals("TC11DeliveryDate", new DateTime(2023, 5, 12), DataProvider.TC11DeliveryDate);
	});

	public void TestText()
	{
		MessageSendingObject.ReasonText = ZString.Empty;
		AssertNull("No text", DataProvider.Text);
	}

	public void TestMRNDoubleEntry()
	{
		MessageSendingObject.DoubleEntryMRN = ZString.Empty;
		AssertNull("No MRN", DataProvider.MRNDoubleEntry);
	}

	public void TestTC11DeliveryDate()
	{
		MessageSendingObject.TC11DeliveryDate = ZDateTime.Empty;
		AssertNull("No date", DataProvider.TC11DeliveryDate);
	}
}
