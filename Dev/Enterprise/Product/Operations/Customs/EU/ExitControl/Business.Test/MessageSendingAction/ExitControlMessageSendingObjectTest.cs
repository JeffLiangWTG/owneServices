using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObject))]
	class ExitControlMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var consignment = Factory.New<CusExitConsignment>();
			consignment.CXC_MovementReference = "Test2222";
			exitReport.CER_CXC_Consignment = consignment.PK;
			exitReport.CER_TransportID = "TRANS123";
			exitReport.CER_Type = "ABC";
			exitReport.CER_Location = "LOC123";
			exitReport.CER_DateTime = DateTime.Now;
			exitReport.CER_OfficeOfExit = "EXT";
			exitReport.CER_MessageStatus = "SNT";
			exitReport.CER_Status = "REJ";
			CombineAssertions(() =>
			{
				var sendingObject = (ExitControlMessageSendingObject)GetNewBusinessObject();
				AssertEquals("Transport ID", "TRANS123", sendingObject.TransportID);
				AssertEquals("Type", "ABC", sendingObject.Type);
				AssertEquals("Location", "LOC123", sendingObject.Location);
				AssertEquals("DateTime", exitReport.CER_DateTime.ToLocalZDateTime(), sendingObject.DateTime);
				AssertEquals("ExitOffice", "EXT", sendingObject.ExitOffice);
				AssertEquals("MRN", "Test2222", sendingObject.MRN);
				AssertEquals("MessageStatus", "SNT", sendingObject.MessageStatus);
				AssertEquals("CustomsStatus", "REJ", sendingObject.CustomsStatus);
				AssertEquals("MRN/LRN", "MRN:Test2222", sendingObject.MRN_LRN);

				sendingObject.MessageType = "507";
				AssertEquals("MessageType", "507", sendingObject.MessageType);
			});
		}

		public void TestCaptions()
		{
			var sendingObject = (ExitControlMessageSendingObject)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				var captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.TypeInfo);
				AssertEquals("TypeInfo should have correct Caption", "Type", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.TransportIDInfo);
				AssertEquals("TransportIDInfo should have correct Caption", "Transport ID", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.LocationInfo);
				AssertEquals("LocationInfo should have correct Caption", "Location", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.DateTimeInfo);
				AssertEquals("DateTimeInfo should have correct Caption", "Departure Date + Time", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.ExitOfficeInfo);
				AssertEquals("ExitOfficeInfo should have correct Caption", "Office of Exit", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.MRNInfo);
				AssertEquals("MRNInfo should have correct Caption", "MRN", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.CustomsStatusInfo);
				AssertEquals("CustomsStatusInfo should have correct Caption", "Customs Status", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.MessageStatusInfo);
				AssertEquals("MessageStatusInfo should have correct Caption", "Message Status", captionAttr.Caption);

				captionAttr = DataBoundResourceStrings.GetDataForProperty(sendingObject.MRN_LRNInfo);
				AssertEquals("MRN/LRN", captionAttr.Caption);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExitControlMessageSendingObject(exitReport);
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitReport = Factory.New<CusExitReport>();
		}
		CusExitReport exitReport;
	}
}
