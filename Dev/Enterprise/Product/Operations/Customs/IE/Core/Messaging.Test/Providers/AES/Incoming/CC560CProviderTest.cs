using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC560C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC560CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestControlTypes()
		{
			AssertEquals("Collection Contains", 2, provider.ControlTypes.Count);
			AssertEquals("01", provider.ControlTypes.First().Type);
			AssertEquals("Physical Control", provider.ControlTypes.ElementAt(1).Text);
		}

		public void TestRequestedDocuments()
		{
			AssertEquals("Collection Contains", 2, provider.RequestedDocuments.Count);
			AssertEquals("Z740", provider.RequestedDocuments.First().DocumentType);
			AssertEquals("House waybill", provider.RequestedDocuments.ElementAt(1).Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC560CProvider(new Cc560C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType22
				{
					Lrn = "LRN",
					Mrn = "MRN",
					ControlNotificationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					NotificationType = "",
					Text = "",
					AnticipatedControlDate = ZDateTime.BrettsBirthday.AddDays(1).ToDateTime(),
				},
				TypeOfControls = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.TypeOfControlsType01>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.TypeOfControlsType01
					{
						SequenceNumber = "1",
						Type = "01",
						Text = "Documentary Control"
					},
					new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.TypeOfControlsType01
					{
						SequenceNumber = "2",
						Type = "02",
						Text = "Physical Control"
					}
				},
				RequestedDocument = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.RequestedDocumentType01>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.RequestedDocumentType01
					{
						SequenceNumber = "1",
						DocumentType = "Z740",
						Description = "Air waybill"
					},
					new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.RequestedDocumentType01
					{
						SequenceNumber = "2",
						DocumentType = "703",
						Description = "House waybill"
					}
				}
			});
		}
		CC560CProvider provider;
	}
}
