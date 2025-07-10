using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR060C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing;

sealed class TR060CProviderTest : TestCaseWithFactory
{
	public void TestCustomsOfficeOfDestination()
	{
		AssertEquals("CUSTOFFREF123", provider.CustomsOfficeOfDestination);
	}

	public void TestMRN()
	{
		AssertEquals("21IEDU4EX144268149", provider.MRN);
	}

	public void TestControlNotificationDateAndTime()
	{
		AssertEquals(new System.DateTime(2025, 04, 29, 15, 45, 0), provider.ControlNotificationDateAndTime);
	}

	public void TestNotificationType()
	{
		AssertEquals("AB123C", provider.NotificationType);
	}

	public void TestControlTypes()
	{
		AssertEquals("ControlTypes is empty", 0, new TR060CProvider(GetEmptyXmlObject()).ControlTypes.Count);

		var controlTypes = provider.ControlTypes;
		AssertEquals("Count", 2, controlTypes.Count);
		AssertType<TR060CTypeOfControlProvider>(controlTypes.ElementAt(0));
		AssertSame("Is cached", controlTypes, provider.ControlTypes);
	}

	public void TestRequestedDocuments()
	{
		AssertEquals("RequestedDocuments is empty", 0, new TR060CProvider(GetEmptyXmlObject()).RequestedDocuments.Count);

		var requestedDocuments = provider.RequestedDocuments;
		AssertEquals("Count", 2, requestedDocuments.Count);
		AssertType<TR060CRequestedDocumentProvider>(requestedDocuments.ElementAt(0));
		AssertSame("Is cached", requestedDocuments, provider.RequestedDocuments);
	}

	protected override void SetUp()
	{
		base.SetUp();
		provider = new TR060CProvider(new Tr060C
		{
			CustomsOfficeOfDestination = new CustomsOfficeOfDestination
			{
				ReferenceNumber = "CUSTOFFREF123",
			},
			TransitOperation = new TransitOperationType50
			{
				Mrn = "21IEDU4EX144268149",
				ControlNotificationDateAndTime = new System.DateTime(2025, 04, 29, 15, 45, 0),
				NotificationType = "AB123C",
			},
			TypeOfControls = new System.Collections.ObjectModel.Collection<TypeOfControlsType>
			{
				new TypeOfControlsType
				{
					SequenceNumber = "1",
					Type = "ABC",
					Text = "Some text about the type of control",
				},
				new TypeOfControlsType
				{
					SequenceNumber = "2",
					Type = "DEF",
					Text = "Some text about the next type of control",
				}
			},
			RequestedDocument = new System.Collections.ObjectModel.Collection<RequestedDocumentType>
			{
				new RequestedDocumentType
				{
					SequenceNumber = "1",
					DocumentType = "DT001",
					Description = "Description"
				},
				new RequestedDocumentType
	{
					SequenceNumber = "2",
					DocumentType = "DT002",
					Description = "Description 2"
	}
			},
		});
}
	TR060CProvider provider;

	Tr060C GetEmptyXmlObject()
	{
		return new Tr060C
		{
			TransitOperation = new TransitOperationType50 { },
			CustomsOfficeOfDestination = new CustomsOfficeOfDestination { },
			RequestedDocument = new System.Collections.ObjectModel.Collection<RequestedDocumentType> { },
		};
	}
}
