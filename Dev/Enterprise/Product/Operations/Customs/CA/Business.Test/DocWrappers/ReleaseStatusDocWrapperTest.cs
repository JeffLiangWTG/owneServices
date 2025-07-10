using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ReleaseStatusDocumentWrapper))]
	sealed class ReleaseStatusDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReleaseStatusDocWrapperProperties()
		{
			CACSubLocationTest.CreateSubLocation(Factory, "3072", "ADAMS CARGO LTD");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "Toronto International Airport (Pearson)", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var message = Factory.New<EDIReleaseMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			message.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207400004068+11'
LOC+22+0497:129::3072'
DTM+58:201011250820:203'
GIS+4'
FTX+AAG+++DELIVERY INSTRUCTIONS LINE 1:LINE 2'
EQD+CN+CONTAINER1'
EQD+CN+CONTAINER2'
EQD+CN+CONTAINER3'
RFF+XC:37132536987'
UNT+10+1'".Replace("\r\n", "");
			message.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, new ZString("10207400004068"));
			message.SetSystemDefinedValue(EDIReleaseMessage.Schema.CargoControlNumber, new ZString("37132536987"));

			var releaseStatusDocWrapper = new ReleaseStatusDocumentWrapper(message);
			AssertEquals("ServiceOptionDescription", "257 RMD, EDI", releaseStatusDocWrapper.ServiceOptionDescription);
			AssertEquals("ServiceOptionDescription", "10207 40000406 8", releaseStatusDocWrapper.TransactionNumber);
			AssertEquals("ProcessingIndicatorDescription", "4 - Goods Released", releaseStatusDocWrapper.ProcessingIndicatorDescription.ToString());
			AssertEquals("ReleaseDate", new ZDateTime(2010, 11, 25, 8, 20, 0), releaseStatusDocWrapper.ReleaseDate);
			AssertEquals("Processing Date", ZDateTime.Empty, releaseStatusDocWrapper.ProcessingDate);
			AssertEquals("CCN", "37132536987", releaseStatusDocWrapper.CCN);
			AssertEquals("Delivery instructions 1", "DELIVERY INSTRUCTIONS LINE 1", releaseStatusDocWrapper.DeliveryInstructions1);
			AssertEquals("Delivery instructions 2", "LINE 2", releaseStatusDocWrapper.DeliveryInstructions2);
			AssertEquals("Place of report", "0497 - Toronto International Airport (Pearson)", releaseStatusDocWrapper.ReleaseOffice);
			AssertEquals("Warehouse", "3072 - ADAMS CARGO LTD", releaseStatusDocWrapper.Warehouse);
			AssertEquals("Containers", "CONTAINER1, CONTAINER2, CONTAINER3", releaseStatusDocWrapper.Containers);
		}

		public void TestReleaseStatusDocWrapperPropertiesWithMinimalMessage()
		{
			var message = Factory.New<EDIReleaseMessage>();
			message.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_MessageSubType = EntryStatusList.Codes.Clear;
			message.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'UNT+2+1'";

			var releaseStatusDocWrapper = new ReleaseStatusDocumentWrapper(message);
			AssertEquals("ServiceOptionDescription", ZString.Empty, releaseStatusDocWrapper.ServiceOptionDescription);
			AssertEquals("ProcessingIndicatorDescription", ZString.Empty, releaseStatusDocWrapper.ProcessingIndicatorDescription.ToString());
			AssertEquals("ReleaseDate", ZDateTime.Empty, releaseStatusDocWrapper.ReleaseDate);
			AssertEquals("Processing Date", ZDateTime.Empty, releaseStatusDocWrapper.ProcessingDate);
			AssertEquals("CCN", ZString.Empty, releaseStatusDocWrapper.CCN);
			AssertEquals("Delivery instructions 1", ZString.Empty, releaseStatusDocWrapper.DeliveryInstructions1);
			AssertEquals("Delivery instructions 2", ZString.Empty, releaseStatusDocWrapper.DeliveryInstructions2);
			AssertEquals("Place of report", ZString.Empty, releaseStatusDocWrapper.ReleaseOffice);
			AssertEquals("Warehouse", ZString.Empty, releaseStatusDocWrapper.Warehouse);
			AssertEquals("Containers", ZString.Empty, releaseStatusDocWrapper.Containers);
		}

		public void TestReleaseStatusDocWrapperPropertiesWithNoMessage()
		{
			var releaseStatusDocWrapper = new ReleaseStatusDocumentWrapper();
			AssertEquals("ServiceOptionDescription", ZString.Empty, releaseStatusDocWrapper.ServiceOptionDescription);
			AssertEquals("ProcessingIndicatorDescription", ZString.Empty, releaseStatusDocWrapper.ProcessingIndicatorDescription.ToString());
			AssertEquals("ReleaseDate", ZDateTime.Empty, releaseStatusDocWrapper.ReleaseDate);
			AssertEquals("Processing Date", ZDateTime.Empty, releaseStatusDocWrapper.ProcessingDate);
			AssertEquals("CCN", ZString.Empty, releaseStatusDocWrapper.CCN);
			AssertEquals("Delivery instructions 1", ZString.Empty, releaseStatusDocWrapper.DeliveryInstructions1);
			AssertEquals("Delivery instructions 2", ZString.Empty, releaseStatusDocWrapper.DeliveryInstructions2);
			AssertEquals("Place of report", ZString.Empty, releaseStatusDocWrapper.ReleaseOffice);
			AssertEquals("Warehouse", ZString.Empty, releaseStatusDocWrapper.Warehouse);
			AssertEquals("Containers", ZString.Empty, releaseStatusDocWrapper.Containers);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReleaseStatusDocumentWrapper(Factory.New<EDIMessage>());
		}
	}
}
