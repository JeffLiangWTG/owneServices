using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObject))]
	public class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderMessageSendingObject(null));
		}

		public void TestGetNewLookups()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsHeaderMessageSendingObjectLookups>("Type", SendingObject.Lookups);
				AssertSame("cached", SendingObject.Lookups, SendingObject.Lookups);
			});
		}

		public void TestCaptions_LRN()
		{
			NCTSTestHelper.AssertCaptions(SendingObject.LRNInfo, "Local Reference Number", "LRN", "LRN");
		}

		public void TestCaptions_MRN()
		{
			NCTSTestHelper.AssertCaptions(SendingObject.MRNInfo, "Movement Reference Number", "MRN", "MRN");
		}

		public void TestCaptions_MessageType()
		{
			NCTSTestHelper.AssertCaptions(SendingObject.MessageTypeInfo, "Message Type", "Msg. Type", "Type");
		}

		public void TestCaptions_ReleaseRequest()
		{
			NCTSTestHelper.AssertCaptions(SendingObject.ReleaseRequestInfo, "Release Request", "Release Req.", "Release");
		}

		public void TestCaptions_Justification()
		{
			AssertEquals("Justification", DataBoundResourceStrings.GetDataForProperty(SendingObject.JustificationInfo).Caption);
		}

		public void TestCaptions_DepartureOfficeOfEnquiry()
		{
			AssertEquals("Customs Office Of Enquiry At Departure", DataBoundResourceStrings.GetDataForProperty(SendingObject.DepartureOfficeOfEnquiryInfo).Caption);
		}

		public void TestCaptions_AdditionalText()
		{
			AssertEquals("Additional Text", DataBoundResourceStrings.GetDataForProperty(SendingObject.AdditionalTextInfo).Caption);
		}

		public void TestReleaseRequest()
		{
			CombineAssertions(() =>
			{
				var info = SendingObject.ReleaseRequestInfo;
				SendingObject.MessageType = nctsHeader.Configuration.MessageSendingConfiguration.ReleaseRequestCode;
				AssertEquals("info.ReadOnly", false, info.ReadOnly);
				SendingObject.ReleaseRequest = ReleaseRequestedFlagList.Codes.Yes;
				SendingObject.MessageType = NCTS5DeparturePhaseList.Codes.Amendment;
				AssertEquals("info.ReadOnly", true, info.ReadOnly);
				AssertEquals("Data should have been cleared out", ZString.Empty, SendingObject.ReleaseRequest);
			});
		}

		public void TestSchema()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength LRN", 22, SendingObject.LRNInfo.MaxLength);
				AssertEquals("MaxLength MRN", 18, SendingObject.MRNInfo.MaxLength);
				AssertEquals("MaxLength MessageType", 10, SendingObject.MessageTypeInfo.MaxLength);
				AssertEquals("MaxLength ReleaseRequest", 1, SendingObject.ReleaseRequestInfo.MaxLength);
				AssertEquals("MaxLength Justification", 512, SendingObject.JustificationInfo.MaxLength);
				AssertEquals("MaxLength DepartureOfficeOfEnquiry", 10, SendingObject.DepartureOfficeOfEnquiryInfo.MaxLength);
				AssertEquals("MaxLength AdditionalText", 512, SendingObject.AdditionalTextInfo.MaxLength);
			});
		}

		public void TestRegistrationNumber()
		{
			NctsHeader.MovementHeader.BM_PaperlessInbondNum = "22CH123456789012N0";
			AssertEquals("22CH123456789012N0", SendingObject.LRN);
		}

		public void TestShouldSend()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Ticked by default", true, SendingObject.ShouldSend);
				AssertEquals("ReadOnly", true, SendingObject.ShouldSendInfo.ReadOnly);
			});
		}

		public void TestJustification()
		{
			CombineAssertions(() =>
			{
				NctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var info = SendingObject.JustificationInfo;
				SendingObject.MessageType = NCTS5DeparturePhaseList.Codes.Cancellation;
				AssertEquals("info.ReadOnly", false, info.ReadOnly);
				SendingObject.Justification = "My Justification";
				SendingObject.MessageType = NCTS5DeparturePhaseList.Codes.Amendment;
				AssertEquals("Phase5 - info.ReadOnly", true, info.ReadOnly);
				AssertEquals("Data should have been cleared out", ZString.Empty, SendingObject.Justification);
				NctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("Phase4 - info.ReadOnly", false, info.ReadOnly);
			});
		}

		public void TestDefaultMessageType()
		{
			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, new MessageSendingConfigurationForTest()))
			{
				AssertEquals("123", SendingObject.MessageType);
			}
		}

		public void TestMessageType_UpperCase()
		{
			SendingObject.MessageType = "t13";
			AssertEquals("T13", SendingObject.MessageType);
		}

		public void TestActualOfficeOfDestination_MaxLength()
		{
			AssertEquals(10, SendingObject.ActualOfficeOfDestinationInfo.MaxLength);
		}

		public void TestActualOfficeOfDestination_Caption()
		{
			AssertEquals("Actual Office of Destination", DataBoundResourceStrings.GetDataForProperty(SendingObject.ActualOfficeOfDestinationInfo).Caption);
		}

		public void TestActualOfficeOfDestination_IsPersistent()
		{
			Assert("ActualOfficeOfDestination.IsPersistent", !SendingObject.ActualOfficeOfDestinationInfo.IsPersistent);
		}

		public void TestActualConsignee_Caption()
		{
			AssertEquals("Actual Consignee", DataBoundResourceStrings.GetDataForProperty(typeof(NctsHeaderMessageSendingObject), nameof(NctsHeaderMessageSendingObject.ActualConsignee)).Caption);
		}

		public void TestActualConsigneeAdditionalValidation()
		{
			AssertType<NctsActualConsigneeJobDocAddressValidation>(SendingObject.ActualConsignee.AdditionalValidation);
		}

		public void TestDefaultActualOfficeOfDestination()
		{
			NctsHeader.MovementHeader.DestinationCustomsOfficeCodeForDeparture = "BE101000";
			var newMessageSendingObject = new NctsHeaderMessageSendingObject(NctsHeader);

			AssertEquals("BE101000", newMessageSendingObject.ActualOfficeOfDestination);
		}

		public void TestQueryInformation_Caption()
		{
			AssertEquals("Query Information", DataBoundResourceStrings.GetDataForProperty(SendingObject.QueryInformationInfo).Caption);
		}

		public void TestQueryInformation_MaxLength()
		{
			AssertEquals(200, SendingObject.QueryInformationInfo.MaxLength);
		}

		public void TestTCI11DeliveryDate()
		{
			AssertEquals("TC11 Delivery Date should not be readonly", false, SendingObject.TC11DeliveryDateInfo.ReadOnly);
			NCTSTestHelper.AssertCaptions(SendingObject.TC11DeliveryDateInfo, "TC11 Delivery Date", string.Empty, string.Empty);
		}

		public void TestEnquiryText()
		{
			AssertEquals("Enquiry Text should not be readonly", false, SendingObject.EnquiryTextInfo.ReadOnly);
			AssertEquals("MaxLength", 512, SendingObject.EnquiryTextInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(SendingObject.EnquiryTextInfo, "Enquiry Text", string.Empty, string.Empty);
		}

		public void TestConsignee()
		{
			AssertEquals("Consignee should be readonly", true, SendingObject.ConsigneeInfo.ReadOnly);
			AssertEquals("MaxLength", 17, SendingObject.ConsigneeInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(SendingObject.ConsigneeInfo, "Actual Consignee", string.Empty, string.Empty);
		}

		public void TestDestinationCustomsOfficeCode()
		{
			AssertEquals("Destination Customs Office Code should be readonly", true, SendingObject.DestinationCustomsOfficeCodeInfo.ReadOnly);
			AssertEquals("MaxLength", 10, SendingObject.DestinationCustomsOfficeCodeInfo.MaxLength);
			NCTSTestHelper.AssertCaptions(SendingObject.DestinationCustomsOfficeCodeInfo, "Actual Destination Office", string.Empty, string.Empty);
		}

		public void TestDepartureOfficeOfEnquiryReadOnly()
		{
			CombineAssertions(() =>
			{
				var customsOffice = NctsHeader.MovementHeader.CustomsOffices.AddNew();
				AssertEquals("DepartureOfficeOfEnquiry should not be readonly", false, SendingObject.DepartureOfficeOfEnquiryInfo.ReadOnly);

				customsOffice.CY_Code = EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry;
				customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
				AssertEquals("DepartureOfficeOfEnquiry should be readonly", true, SendingObject.DepartureOfficeOfEnquiryInfo.ReadOnly);

				customsOffice.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
				customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
				AssertEquals("DepartureOfficeOfEnquiry should not be readonly", false, SendingObject.DepartureOfficeOfEnquiryInfo.ReadOnly);
			});
		}

		public void TestDepartureOfficeOfEnquiryDefaultValue()
		{
			CombineAssertions(() =>
			{
				var customsOffice = NctsHeader.MovementHeader.CustomsOffices.AddNew();
				AssertEquals("DepartureOfficeOfEnquiry's value should be empty", ZString.Empty, SendingObject.DepartureOfficeOfEnquiry);

				customsOffice.CY_Code = EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry;
				customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
				customsOffice.CY_Data = "12";
				var sendingObjectWithCustomsOfficeReused = new NctsHeaderMessageSendingObject(NctsHeader);
				AssertEquals("DepartureOfficeOfEnquiry's value should be same with customs office", customsOffice.CY_Data, sendingObjectWithCustomsOfficeReused.DepartureOfficeOfEnquiry);

				customsOffice.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
				customsOffice.CY_Type = EU.Business.CusCodeDataTypeList.Codes.OfficeCode;
				customsOffice.CY_Data = "12";
				var sendingObjectWithCustomsOfficeNotReused = new NctsHeaderMessageSendingObject(NctsHeader);
				AssertEquals("DepartureOfficeOfEnquiry's value should not follow the value of customs office", ZString.Empty, sendingObjectWithCustomsOfficeNotReused.DepartureOfficeOfEnquiry);
			});
		}

		public void TestMessageStatus()
		{
			AssertEquals("Message status", "ACK", SendingObject.MessageStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NctsHeaderMessageSendingObject(NctsHeader);
		}

		NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
		NctsHeader nctsHeader;
		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.EffectiveMessageStatus = "ACK";
			return nctsHeader;
		}

		NctsHeaderMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new NctsHeaderMessageSendingObject(NctsHeader));
		NctsHeaderMessageSendingObject sendingObject;
	}
}
