using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class IETS414WrapperTest : Customs.Business.Testing.DataProviderTestCase<IETS414Wrapper>
	{
		[TestDate(2023, 01, 18)]
		public void TestMessageHeader()
		{
			AssertEquals("Sender should equal PNTSConstants.MessageHeaderWrapperConstants.Sender", "45244585100028", Provider.MessageHeader.Sender);
			AssertEquals("Recipient should equal PNTSConstants.MessageHeaderWrapperConstants.Recipient", "TSD", Provider.MessageHeader.Recipient);
			AssertEquals("MessageTimestamp should equal DateTime.UtcNow", new ZDateTime(2023, 01, 18), Provider.MessageHeader.MessageTimestamp);
			AssertEquals("MessageId should equal EDIMessage.MessageNumberPlaceHolder", EDIMessage.MessageNumberPlaceHolder, Provider.MessageHeader.MessageId);
			AssertEquals("RefToMessageId should equal MessageBuilderBase<object>.CorrelationIdPlaceholder", MessageBuilderBase<object>.CorrelationidPlaceholder, Provider.MessageHeader.RefToMessageId);
			AssertEquals("CorrelationId should equal null", null, Provider.MessageHeader.CorrelationId);
			AssertEquals("LanguageCode should equal FR", Core.Constants.CountryCodes.France, Provider.MessageHeader.LanguageCode);
		}

		public void TestCrn()
		{
			AssertEquals("Crn should equal 1234567", "1234567", Provider.Crn);
		}

		public void TestInvalidationReason()
		{
			AssertEquals("InvalidationReason should equal MessageFunction.VOCReason", "ABCD", Provider.InvalidationReason);
		}

		public void TestDeclarant()
		{
			AssertEquals("IdentificationNumber should equal ABC", "FRABC", Provider.Declarant.IdentificationNumber);
			AssertEquals("Name should equal orgHeader.OH_FullName", "ABCD", Provider.Declarant.Name);
			AssertEquals("Identifier should equal CustomsAgent.GS_EmailAddress.", "PP@broker.job", Provider.Declarant.Communication.ElementAt(0).Identifier);
			AssertEquals("Type should equal EM.", "EM", Provider.Declarant.Communication.ElementAt(0).Type);
		}

		public void TestRepresentative()
		{
			AssertEquals("IdentificationNumber should equal ABC", "FRABC", Provider.Representative.IdentificationNumber);
			AssertEquals("Name should equal orgHeader.OH_FullName", "ABCD", Provider.Representative.Name);
			AssertEquals("Identifier should equal contact2.OC_Phone", "123456", Provider.Representative.Communication.Identifier);
			AssertEquals("Type should equal TE", "TE", Provider.Representative.Communication.Type);
			AssertEquals("Status should equal 3", (byte)3, Provider.Representative.Status);
		}

		protected override IETS414Wrapper GetProvider()
		{
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BOK";
			broker.GS_WorkPhone = "123456";
			broker.GS_EmailAddress = "PP@broker.job";
			storageHeader.AMA_GS_NKCustomsAgent = "BOK";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABCD";
			orgHeader.Addresses.MainAddress.Address1 = "ABCDE";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader.AMA_OA_Declarant = orgHeader.Addresses.MainAddress.PK;
			storageHeader.AMA_OA_Representative = orgHeader.Addresses.MainAddress.PK;

			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			orgHeader.CustomsCodes.AddNew("EOR", "ABC", storageHeader.AMA_RN_NKCountry);
			orgHeader.CustomsCodes.AddNew("ABM", "DEF", Core.Constants.CountryCodes.UnitedKingdom);

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "Test Contact 1";
			contact1.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;
			contact1.OC_Email = "abc@abc.com";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Test Contact 2";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact2.OC_Phone = "123456";

			var newEntryNum = Factory.New<CusEntryNumber>();
			newEntryNum.Parent = storageHeader;
			newEntryNum.CE_EntryType = "CRN";
			newEntryNum.CE_EntryNum = "1234567";

			var sendingObject = new TemporaryStorageMessageSendingObject(storageHeader);
			sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD;
			sendingObject.VOCReason = "ABCD";
			var invalidationRequestTSDMessageFunction = new InvalidationRequestTSDMessageFunction(sendingObject);

			return IETS414Wrapper.New(storageHeader, invalidationRequestTSDMessageFunction);
		}
	}
}
