using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class IETS007WrapperTest : Customs.Business.Testing.DataProviderTestCase<IETS007Wrapper>
	{
		[TestDate(2023, 01, 18)]
		public void TestMessageHeader()
		{
			AssertEquals("Sender should equal PNTSConstants.MessageHeaderWrapperConstants.Sender", "45244585100028", Provider.MessageHeader.Sender);
			AssertEquals("Recipient should equal PNTSConstants.MessageHeaderWrapperConstants.IETS007Recipient", "PN", Provider.MessageHeader.Recipient);
			AssertEquals("MessageTimestamp should equal DateTime.UtcNow", new ZDateTime(2023, 01, 18), Provider.MessageHeader.MessageTimestamp);
			AssertEquals("MessageId should equal EDIMessage.MessageNumberPlaceHolder", EDIMessage.MessageNumberPlaceHolder, Provider.MessageHeader.MessageId);
			AssertEquals("RefToMessageId should equal MessageBuilderBase<object>.CorrelationIdPlaceholder", MessageBuilderBase<object>.CorrelationidPlaceholder, Provider.MessageHeader.RefToMessageId);
			AssertEquals("CorrelationId should equal null", null, Provider.MessageHeader.CorrelationId);
			AssertEquals("LanguageCode should equal FR", Core.Constants.CountryCodes.France, Provider.MessageHeader.LanguageCode);
		}
		public void TestLrn()
		{
			AssertEquals("Lrn should equal CE_EntryNum of lrn entrynumber with storageHeader parent", "1234567", Provider.Lrn);
		}

		public void TestCrn()
		{
			AssertEquals("Crn should equal 7654321", "7654321", Provider.Crn);
		}

		public void TestDeclarationDate()
		{
			var expectedDateTime = new ZDateTime(2021, 12, 22, 5, 48, 28).ToUniversalBranchTime();
			AssertEquals("DeclarationDate value should equal CE_IssueDate of lrn entry num with utc DateTime format", expectedDateTime, Provider.DeclarationDate);
		}

		public void TestDateAndTimeOfPresentationOfTheGoods()
		{
			var expectedDateTime = new ZDateTime(2022, 12, 22, 5, 48, 28).ToUniversalBranchTime();
			AssertEquals("DateAndTimeOfPresentationOfTheGoods value should equal AMA_DateAtCustomsOffice with utc DateTime format", expectedDateTime, Provider.DateAndTimeOfPresentationOfTheGoods);
		}

		public void TestCustomsOfficeOfPresentation()
		{
			AssertEquals("CustomsOfficeOfPresentation should equal CusCodeData.CY_Data", "data", Provider.CustomsOfficeOfPresentation.ReferenceNumber);
		}

		public void TestPersonPresentingTheGoods()
		{
			AssertEquals("IdentificationNumber should equal eori number of presenter", "FRDEF", Provider.PersonPresentingTheGoods.IdentificationNumber);
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

		public void TestConsignmentHeaderMasterLevel()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var provider = IETS115Wrapper.New(storageHeader);
			AssertType<ConsignmentHeaderMasterLevelWrapper>("ConsignmentHeaderMasterLevel should be of type ConsignmentHeaderCombinedMasterLevelWrapper", provider.ConsignmentHeaderMasterLevel);
		}

		protected override IETS007Wrapper GetProvider()
		{
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			Factory.Save();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BOK";
			broker.GS_WorkPhone = "123456";
			broker.GS_EmailAddress = "PP@broker.job";
			storageHeader.AMA_GS_NKCustomsAgent = "BOK";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_FullName = "CarrierName";
			carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.France);
			storageHeader.AMA_OA_Carrier = carrier.MainAddress.PK;

			var bolBill = storageHeader.Bills[0];
			bolBill.ABL_RL_NKPortOfDischarge = "FRBER";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABCD";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader.AMA_OA_Declarant = orgAddress.PK;
			storageHeader.AMA_OA_Representative = orgAddress.PK;

			orgHeader.CustomsCodes.AddNew("EOR", "ABC", storageHeader.AMA_RN_NKCountry);
			orgHeader.CustomsCodes.AddNew("ABM", "DEF", Core.Constants.CountryCodes.UnitedKingdom);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "pre";
			orgHeader2.OH_FullName = "pres";
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_OH = orgHeader2.PK;
			storageHeader.AMA_OA_Presenter = orgAddress2.PK;

			orgHeader2.CustomsCodes.AddNew("EOR", "DEF", storageHeader.AMA_RN_NKCountry);

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_OA_OrgAddress = orgAddress.PK;
			contact1.OC_ContactName = "Test Contact 1";
			contact1.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;
			contact1.OC_Email = "abc@abc.com";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_OA_OrgAddress = orgAddress.PK;
			contact2.OC_ContactName = "Test Contact 2";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact2.OC_Phone = "123456";

			var newEntryNum = Factory.New<CusEntryNumber>();
			newEntryNum.Parent = storageHeader;
			newEntryNum.CE_EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber;
			newEntryNum.CE_EntryNum = "1234567";
			newEntryNum.CE_IssueDate = new ZDateTime(2021, 12, 22, 5, 48, 28,120);

			var newEntryNum2 = Factory.New<CusEntryNumber>();
			newEntryNum2.Parent = storageHeader;
			newEntryNum2.CE_EntryType = CusEntryNumberTypes.EU.CustomsRegistrationNumber;
			newEntryNum2.CE_EntryNum = "7654321";

			storageHeader.PresentationCustomsOffice = "data";

			storageHeader.AMA_CustomsOffice = "FR002300";
			storageHeader.ENSReuse = 1;
			storageHeader.AMA_DateAtCustomsOffice = new ZDateTime(2022, 12, 22, 5, 48, 28,120);

			return IETS007Wrapper.New(storageHeader);
		}
	}
}
