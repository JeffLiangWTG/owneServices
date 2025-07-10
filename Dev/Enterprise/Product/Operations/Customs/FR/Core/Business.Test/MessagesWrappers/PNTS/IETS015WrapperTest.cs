using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class IETS015WrapperTest : Customs.Business.Testing.DataProviderTestCase<IETS015Wrapper>
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

		public void TestLrn()
		{
			AssertEquals("Lrn should equal CE_EntryNum of lrn entrynumber with storageHeader parent", "1234567", Provider.Lrn);
		}

		public void TestEnsReUseIndicator_TrueValue()
		{
			AssertEquals("EnsReUseIndicator value should equal EnsReUse", (byte)1, Provider.EnsReUseIndicator);
		}

		public void TestEnsReUseIndicator_FalseValue()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.ENSReuse = 0;
			var wrapper = IETS115Wrapper.New(storageHeader);
			AssertEquals("EnsReUseIndicator value should equal EnsReUse", (byte)0, wrapper.EnsReUseIndicator);
		}

		public void TestSupervisingCustomsOffice()
		{
			AssertEquals("ReferenceNumber should equal AMA_CustomsOffice", "FR002300", Provider.SupervisingCustomsOffice.ReferenceNumber);
		}

		public void TestConsignmentHeaderMasterLevel()
		{
			AssertType<ConsignmentHeaderMasterLevelWrapper>("ConsignmentHeaderMasterLevel should be of type ConsignmentHeaderMasterLevelWrapper", Provider.ConsignmentHeaderMasterLevel);
			AssertEquals("CustomsOfficeOfPresentation should equal ABL_RL_NKPortOfDischarge of BOL bill", "FRBER", Provider.ConsignmentHeaderMasterLevel.PlaceOfUnloading.UnLoCode);
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var provider = IETS115Wrapper.New(storageHeader);
			AssertEquals("CustomsOfficeOfPresentation should be empty when ABL_RL_NKPortOfDischarge of BOL bill is not set", string.Empty, provider.ConsignmentHeaderMasterLevel.PlaceOfUnloading.UnLoCode);
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

		protected override IETS015Wrapper GetProvider()
		{
			FRCustomsDataRegistry.Instance.RecipientID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCDEFG");
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			Factory.Save();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BOK";
			broker.GS_WorkPhone = "123456";
			broker.GS_EmailAddress = "PP@broker.job";
			storageHeader.AMA_GS_NKCustomsAgent = "BOK";

			var bolBill = storageHeader.Bills[0];
			bolBill.ABL_RL_NKPortOfDischarge = "FRBER";

			var bill1 = CreateBill();
			bill1.ABL_AMA = storageHeader.PK;
			bill1.ABL_UCRNumber = "UCRNumber1";
			bill1.ABL_GrossWeight = 77.65m;

			var bill2 = CreateBill();
			bill2.ABL_AMA = storageHeader.PK;
			bill2.ABL_UCRNumber = "UCRNumber2";
			bill2.ABL_GrossWeight = 32.47m;

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

			var newEntryNum2 = Factory.New<CusEntryNumber>();
			newEntryNum2.Parent = storageHeader;
			newEntryNum2.CE_EntryType = CusEntryNumberTypes.EU.CustomsRegistrationNumber;
			newEntryNum2.CE_EntryNum = "7654321";

			storageHeader.PresentationCustomsOffice = "data";

			storageHeader.AMA_CustomsOffice = "FR002300";
			storageHeader.ENSReuse = 1;

			return IETS015Wrapper.New(storageHeader);
		}

		AsycudaBill CreateBill()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_GrossWeight = 1.00;
			bill.ABL_BillNumber = "1X";

			var address = Factory.New<OrgAddress>();
			address.OA_City = "Insomnia";
			address.OA_RN_NKCountryCode = "LS";
			address.OA_PostCode = "000000";
			address.OA_Address1 = "Kings Street";
			address.OA_Address2 = "No.001";

			var header = Factory.New<OrgHeader>();
			var contact = header.Contacts.AddNew();
			contact.OC_Email = "a@a.com";
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			header.LocalBusinessRegNoObject.OK_CustomsRegNo = "1";
			header.OH_FullName = "Name";
			address.OA_OH = header.PK;

			bill.ABL_OA_Consignee = address.PK;
			bill.ABL_ConsigneeRegNoType = "2";
			bill.ABL_OA_Shipper = address.PK;
			bill.ABL_ShipperRegNoType = "3";
			bill.ABL_OA_NotifyParty = address.PK;
			bill.ABL_NotifyPartyRegNoType = "4";

			var container = Factory.New<AsycudaContainer>();
			container.ACN_ContainerNumber = "ContainerNumber";
			container.ACN_EmptyFullIndicator = "FUL";
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 3;
			pack.APA_MarksAndNumbers = "Marks & numbers";
			pack.APA_PackUQ = "CTN";
			pack.ContainerPK = container.PK;
			pack.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;

			var item = pack.PackedItem;
			item.API_GoodsDescription = "GoodsDescription";
			item.API_Tariff = "2345167890";
			item.API_GrossWeight = 55.33m;

			bill.ContainerPK = container.PK;
			return bill;
		}
	}
}
