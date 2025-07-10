using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineColsHeader))]
	sealed class QuarantineColsHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCusEntryHeader()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			AssertNull(colsHeader.CusEntryHeader);

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			AssertSame(cusEntryHeader, colsHeader.CusEntryHeader);
		}

		public void TestJobDeclaration()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			AssertNotNull("Precondition", colsHeader.CusEntryHeader);
			AssertSame(jobDeclaration, colsHeader.JobDeclaration);
		}

		public void TestResponsibleParty()
		{
			var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.Branch.GB_OH_OrgProxy = orgProxy.PK;
			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = GlbStaff.CurrentUser.GS_FullName.ToUpper();
			var contact2 = orgProxy.Contacts.AddNew();
			contact2.OC_ContactName = "should not be use";
			jobDeclaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			var colsHeader = Factory.NewWithValidTestData<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			AssertNotNull("Precondition", colsHeader.CusEntryHeader);

			colsHeader.ClearHasChanges();
			AssertSame(jobDeclaration, colsHeader.JobDeclaration);
			AssertEquals("Populate OrgProxy", jobDeclaration.Branch.OrgProxy.PK, colsHeader.ResponsibleParty.OrganisationPK);
			AssertEquals("Populate Contact", GlbStaff.CurrentUser.GS_FullName.ToUpper(), colsHeader.ResponsibleParty.E2_Contact);
			AssertEquals("Getting ResponsibleParty shouldn't update colsHeader.HasChanges", false, colsHeader.HasChanges);
		}

		public void TestResponsibleParty_Requirements() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			AssertEquals("Precondition: LRN is empty", ZString.Empty, colsHeader.LRN);
			AssertType<COLSResponsiblePartyAddressRequirement>("ResponsibleParty.Requirement = COLSResponsiblePartyAddressRequirement when LRN is empty", colsHeader.ResponsibleParty.Requirement);

			colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryNumber = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			cusEntryNumber.CE_EntryNum = "LRN1111";
			AssertEquals("Precondition: LRN is not empty", "LRN1111", colsHeader.LRN);
			AssertNull("ResponsibleParty.Requirement is null when LRN is not empty", colsHeader.ResponsibleParty.Requirement);
		});

		public void TestDeliveryOrUnpack()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			AssertNotNull("Precondition", colsHeader.CusEntryHeader);
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryAddress = deliveryOrg.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "ijkl";
			deliveryAddress.OA_Phone = "345";
			var deliveryOrUnpackDocAddress = colsHeader.DeliveryOrUnpack;
			deliveryOrUnpackDocAddress.E2_OA_Address = deliveryAddress.PK;
			AssertEquals("The Delivery/Unpack org should use its own OrgCusCode Address in order to obtain the relevant AAN value", deliveryOrg.PK, deliveryOrUnpackDocAddress.OrganisationPK);

			colsHeader.QCH_ApprovedArrangementRefNum = "12345";
			deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("QCH_ApprovedArrangementRefNum should be cleared if no AAN config number exists in OrgHeader", ZString.Empty, colsHeader.QCH_ApprovedArrangementRefNum);

			var mainAddress = deliveryOrg.Addresses.MainAddress;
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to a main address which is not Delivery or PickupAndDelivery address and doesn't have AAN code", mainAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum is not default to anything", ZString.Empty, colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, false);
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to Delivery address which is not checked as Main and doesn't have AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum is not default to anything", ZString.Empty, colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false);
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to PickupAndDelivery address which is not checked as Main and doesn't have AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum is not default to anything", ZString.Empty, colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to Delivery address which is checked as Main and doesn't have AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum is not default to anything", ZString.Empty, colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to PickupAndDelivery address which is checked as Main and doesn't have AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum is not default to anything", ZString.Empty, colsHeader.QCH_ApprovedArrangementRefNum);

			var aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE1", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = mainAddress.PK;
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to a main address which is not Delivery or PickupAndDelivery address and has AAN code", mainAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum defaults to AAN config number against a main address which is not Delivery or PickupAndDelivery address", "CODE1", colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, false);
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE2", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to Delivery address which is not checked as Main and has AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum defaults to AAN config number against Delivery address which is not checked as Main", "CODE2", colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false);
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE3", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to PickupAndDelivery address which is not checked as Main and has AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum defaults to AAN config number against PickupAndDelivery address which is not checked as Main", "CODE3", colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.Delivery, true);
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE4", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to Delivery address which is checked as Main and has AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum defaults to AAN config number against Delivery address which is checked as Main", "CODE4", colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryAddress = deliveryOrg.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			aanConfigCode = deliveryOrg.CustomsCodes.AddNew(GovRegNumTypeList.Codes.AAN, "CODE5", Core.Constants.CountryCodes.Australia);
			aanConfigCode.OK_OA_PremisesAddress = deliveryAddress.PK;
			deliveryOrUnpackDocAddress.OrganisationPK = ZGuid.Empty;
			deliveryOrUnpackDocAddress.OrganisationPK = deliveryOrg.PK;
			AssertEquals("Address is default to PickupAndDelivery address which is checked as Main and has AAN code", deliveryAddress.PK, deliveryOrUnpackDocAddress.E2_OA_Address);
			AssertEquals("QCH_ApprovedArrangementRefNum defaults to AAN config number against PickupAndDelivery address which is checked as Main", "CODE5", colsHeader.QCH_ApprovedArrangementRefNum);
		}

		public void TestDeliveryOrUnpackUsesAANCode()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryAddress = deliveryOrg.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "100 Back Laneway";
			deliveryAddress.OA_City = "Melbourne";
			deliveryAddress.OA_Phone = "03 4592 3700";

			var orgAAN = deliveryOrg.CustomsCodes.AddNew(AustraliaCodeTypes.ApprovedArrangementNumber, "54321", Core.Constants.CountryCodes.Australia);
			orgAAN.OK_OA_PremisesAddress = deliveryAddress.PK;
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			AssertNotNull("Precondition", colsHeader.CusEntryHeader);
			colsHeader.DeliveryOrUnpack.E2_OA_Address = deliveryAddress.PK;

			AssertEquals("The Delivery/Unpack org should use its own OrgCusCode Address for the AAN value", deliveryOrg.PK, colsHeader.DeliveryOrUnpack.OrganisationPK);
			var customsRegNo = colsHeader.DeliveryOrUnpack.Organisation.CustomsCodes.GetCustomsRegNo(AustraliaCodeTypes.ApprovedArrangementNumber);
			AssertEquals("54321", customsRegNo);
		}

		public void TestUpdateDeliveryClassificationFromDeliveryOrUnpackAddressPostcode() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("AUPC", "AQIS Postcodes", "AU");
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2001", "Sydney", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2026", "Bondi", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode3 = helper.CreateNewOrGetExistingCusCodeList("AU", "AUPC", "2032", "Kingsford", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PostcodeDeliveryClassification", "Postcode Delivery Classification", "AUPC", "AU");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, "PostcodeDeliveryClassification", "Metro");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, "PostcodeDeliveryClassification", "Rural");
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3.PK, "PostcodeDeliveryClassification", "Split");

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryAddress1 = deliveryOrg.Addresses.AddNew();
			deliveryAddress1.OA_Address1 = "100 Back Laneway";
			deliveryAddress1.OA_PostCode = "2001";
			var deliveryAddress2 = deliveryOrg.Addresses.AddNew();
			deliveryAddress2.OA_Address1 = "200 Back Laneway";
			deliveryAddress2.OA_PostCode = "2026";
			var deliveryAddress3 = deliveryOrg.Addresses.AddNew();
			deliveryAddress3.OA_Address1 = "300 Back Laneway";
			deliveryAddress3.OA_PostCode = "2032";
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var deliveryOrUnpackAddress = colsHeader.DeliveryOrUnpack;
			AssertEquals("Prerequisite: QCH_DeliveryClassification is empty", ZString.Empty, colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_OA_Address = deliveryAddress1.PK;
			AssertEquals("Address selected, PostcodeDeliveryClassification = 'Metro'", "Metro", colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Address cleared", ZString.Empty, colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_OA_Address = deliveryAddress2.PK;
			AssertEquals("Address selected, PostcodeDeliveryClassification = 'Rural'", "Rural", colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_OA_Address = deliveryAddress3.PK;
			AssertEquals("Address selected, PostcodeDeliveryClassification = 'Split'", ZString.Empty, colsHeader.QCH_DeliveryClassification);

			deliveryOrUnpackAddress.E2_AddressOverride = true;
			deliveryOrUnpackAddress.E2_Postcode = "2001";
			AssertEquals("Address overriden, PostcodeDeliveryClassification = 'Metro'", "Metro", colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_Postcode = ZString.Empty;
			AssertEquals("Postcode cleared", ZString.Empty, colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_Postcode = "2026";
			AssertEquals("Address overriden, PostcodeDeliveryClassification = 'Rural'", "Rural", colsHeader.QCH_DeliveryClassification);
			deliveryOrUnpackAddress.E2_Postcode = "2032";
			AssertEquals("Address overriden, PostcodeDeliveryClassification = 'Split'", ZString.Empty, colsHeader.QCH_DeliveryClassification);
		});

		public void TestUpdateAAIDFromDeliveryOrUnpackAddress()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryOrgMainAddress = deliveryOrg.MainAddress;
			var deliveryAddress1 = deliveryOrg.Addresses.AddNew();
			deliveryAddress1.OA_Address1 = "100 Test Street";
			deliveryAddress1.CustomsCodes.AddNew(AustraliaCodeTypes.ApprovedArrangementNumber, "Test1", Core.Constants.CountryCodes.Australia);
			var deliveryAddress2 = deliveryOrg.Addresses.AddNew();
			deliveryAddress2.OA_Address1 = "200 Test Street";
			deliveryAddress2.CustomsCodes.AddNew(AustraliaCodeTypes.ApprovedArrangementNumber, "Test2", Core.Constants.CountryCodes.Australia);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var deliveryOrUnpackAddress = colsHeader.DeliveryOrUnpack;
			deliveryOrUnpackAddress.E2_OA_Address = deliveryAddress1.PK;
			AssertEquals("Address1 AAID", "Test1", colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryOrUnpackAddress.E2_OA_Address = deliveryAddress2.PK;
			AssertEquals("Address2 AAID", "Test2", colsHeader.QCH_ApprovedArrangementRefNum);

			deliveryOrUnpackAddress.E2_OA_Address = deliveryOrgMainAddress.PK;
			AssertEquals("Main Address AAID (none)", "", colsHeader.QCH_ApprovedArrangementRefNum);
		}

		public void TestMessagesCollection() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var ediMessage = Factory.New<COLSMessage>();
			ediMessage.EM_LinkedObject = colsHeader;

			var docPivot1 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage1 = Factory.New<COLSMessage>();
			attachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage1.EM_LinkedObject = docPivot1;

			var docPivot2 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage2 = Factory.New<COLSMessage>();
			attachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage2.EM_LinkedObject = docPivot2;

			AssertEquals("colsHeader messages also includes CNA messages from docPivots", 3, colsHeader.Messages.Count);
			AssertEquals("EM_LinkUniqueID of ediMessage", colsHeader.PK, ((EDIMessage)colsHeader.Messages.First(x => x.PK == ediMessage.PK)).EM_LinkUniqueID);
			AssertEquals("EM_LinkUniqueID of attachmentMessage1", docPivot1.PK, ((EDIMessage)colsHeader.Messages.First(x => x.PK == attachmentMessage1.PK)).EM_LinkUniqueID);
			AssertEquals("EM_LinkUniqueID of attachmentMessage2", docPivot2.PK, ((EDIMessage)colsHeader.Messages.First(x => x.PK == attachmentMessage2.PK)).EM_LinkUniqueID);
		});

		public void TestReloadMessages() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var ediMessage = Factory.New<COLSMessage>();
			ediMessage.EM_LinkedObject = colsHeader;

			var docPivot1 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage1 = Factory.New<COLSMessage>();
			attachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage1.EM_LinkedObject = docPivot1;

			AssertEquals("colsHeader messages initially", 2, colsHeader.Messages.Count);

			var docPivot2 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage2 = Factory.New<COLSMessage>();
			attachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage2.EM_LinkedObject = docPivot2;

			colsHeader.ReloadMessages();
			AssertEquals("colsHeader messages after ReloadMessages", 3, colsHeader.Messages.Count);
		});

		public void TestNonDiscardedMessagesCollection()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var ediMessage = Factory.New<COLSMessage>();
			ediMessage.EM_LinkedObject = colsHeader;

			var ediMessage1 = colsHeader.Messages.AddNew();
			ediMessage1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.COLS;
			ediMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddNewLodgement;
			ediMessage1.EM_LinkedObject = colsHeader;
			ediMessage1.EM_Status = EDIMessageStatusList.Codes.Sent;

			var docPivot1 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage1 = Factory.New<COLSMessage>();
			attachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage1.EM_LinkedObject = docPivot1;
			attachmentMessage1.EM_Status = EDIMessageStatusList.Codes.Sent;

			var attachmentFailureMessage = Factory.New<COLSMessage>();
			attachmentFailureMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.XtMessageError;
			attachmentFailureMessage.EM_LinkedObject = docPivot1;
			attachmentFailureMessage.EM_Status = EDIMessageStatusList.Codes.Received;

			var docPivot2 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage2 = Factory.New<COLSMessage>();
			attachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage2.EM_LinkedObject = docPivot2;
			attachmentMessage2.EM_Status = EDIMessageStatusList.Codes.Discarded;

			AssertEquals("colsHeader NonDiscarded messages", 4, colsHeader.NonDiscardedMessages.Count);
			AssertEquals("EM_LinkUniqueID of ediMessage", colsHeader.PK, ((EDIMessage)colsHeader.NonDiscardedMessages.First(x => x.PK == ediMessage.PK)).EM_LinkUniqueID);
			AssertEquals("EM_LinkUniqueID of ediMessage1", colsHeader.PK, ((EDIMessage)colsHeader.NonDiscardedMessages.First(x => x.PK == ediMessage1.PK)).EM_LinkUniqueID);
			AssertEquals("EM_LinkUniqueID of attachmentMessage1", docPivot1.PK, ((EDIMessage)colsHeader.NonDiscardedMessages.First(x => x.PK == attachmentMessage1.PK)).EM_LinkUniqueID);
			AssertEquals("EM_LinkUniqueID of attachmentMessage1", docPivot1.PK, ((EDIMessage)colsHeader.NonDiscardedMessages.First(x => x.PK == attachmentFailureMessage.PK)).EM_LinkUniqueID);
		}

		public void TestDiscardedMessagesCollection()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var ediMessage = Factory.New<COLSMessage>();
			ediMessage.EM_LinkedObject = colsHeader;

			var docPivot1 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage1 = Factory.New<COLSMessage>();
			attachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage1.EM_LinkedObject = docPivot1;
			attachmentMessage1.EM_Status = EDIMessageStatusList.Codes.Discarded;

			var docPivot2 = colsHeader.EDocPivotCollection.AddNew();
			var attachmentMessage2 = Factory.New<COLSMessage>();
			attachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			attachmentMessage2.EM_LinkedObject = docPivot2;
			attachmentMessage2.EM_Status = EDIMessageStatusList.Codes.Discarded;

			AssertEquals("colsHeader discarded messages", 2, colsHeader.DiscardedMessages.Count);
			AssertEquals("EM_LinkUniqueID of attachmentMessage1", docPivot1.PK, ((EDIMessage)colsHeader.DiscardedMessages.First(x => x.PK == attachmentMessage1.PK)).EM_LinkUniqueID);
			AssertEquals("EM_LinkUniqueID of attachmentMessage2", docPivot2.PK, ((EDIMessage)colsHeader.DiscardedMessages.First(x => x.PK == attachmentMessage2.PK)).EM_LinkUniqueID);
		}

		public void TestEDocPivotCollection()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusStorageDocPivot = Factory.New<CusStorageDocPivot>();
			cusStorageDocPivot.CSD_ParentID = colsHeader.PK;

			AssertEquals("colsHeader EDocPivotCollection has one attached document", 1, colsHeader.EDocPivotCollection.Count);
		}

		public void TestLRNEntryNumber()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var cusEntryNumber1 = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			cusEntryNumber1.CE_EntryNum = "LRN1111";
			cusEntryNumber1.CE_EntryStatus = COLSEntryStatusList.Codes.LrnActive;
			cusEntryNumber1.CE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);

			var cusEntryNumber2 = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			cusEntryNumber2.CE_EntryNum = "LRN2222";
			cusEntryNumber2.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			cusEntryNumber2.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var entryNumber = colsHeader.LRNCusEntryNumber;
			AssertEquals("Recent Entry Number", cusEntryNumber2.CE_EntryNum, entryNumber.CE_EntryNum);
			AssertEquals("Recent Entry Status", cusEntryNumber2.CE_EntryStatus, entryNumber.CE_EntryStatus);
		}

		public void TestAddANewLRN()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			colsHeader.AddANewLRN("LRN450228", COLSEntryStatusList.Codes.LrnActive);

			var lrnEntryNumber = colsHeader.LRNCusEntryNumber;
			AssertEquals("Check which object the LRN is attached to - CusEntryHeader or QuarantineColsHeader", colsHeader.PK, lrnEntryNumber.CE_ParentID);
			AssertEquals("Confirm the business object the LRN is attached to is the QuarantineColsHeader, not the base CusEntryHeader", "QuarantineColsHeader", lrnEntryNumber.CE_ParentTable);
		}

		public void TestLodgementStatus()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_LodgementStatus = COLSLodgementStatusList.Codes.AdditionalInformationRequested;
			AssertEquals("Should get description from code", COLSLodgementStatusList.Descriptions.AdditionalInformationRequested, colsHeader.LodgementStatus);
		}

		public void TestLodgementResultMessage()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var message1 = colsHeader.Messages.AddNew();
			message1.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			message1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message1.EM_Status = EDIMessageStatusList.Codes.Sent;
			var message2 = colsHeader.Messages.AddNew();
			message2.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			message2.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_MessageText = "{ \"status\": null, \"receivedDate\": null, \"type\": null, \"resultMessage\": null, \"result\": \"VALIDATION FAILED\", \"validationMessages\": \"Validation error\" }";
			AssertEquals("Should leave blank when no success lodgement status response exists", ZString.Empty, colsHeader.LodgementResultMessage);

			var message3 = colsHeader.Messages.AddNew();
			message3.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			message3.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message3.EM_Status = EDIMessageStatusList.Codes.Received;
			message3.EM_SystemCreateTimeUtc = ZDateTime.MinSmallDateTimeValue;
			message3.EM_MessageText = "{ \"status\": \"Escalated\", \"receivedDate\": \"Thu 8 June 2023 at 11:33:35 AEST\", \"type\": \"ASSESSMENT\", \"resultMessage\": \"Result string 1\", \"result\": \"SUCCESS\", \"validationMessages\": null }";
			var message4 = colsHeader.Messages.AddNew();
			message4.EM_MessageType = AUCOLSMessageTypeList.Codes.LodgementStatus;
			message4.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message4.EM_Status = EDIMessageStatusList.Codes.Received;
			message4.EM_SystemCreateTimeUtc = ZDateTime.MaxSmallDateTime;
			message4.EM_MessageText = "{ \"status\": \"Escalated\", \"receivedDate\": \"Thu 8 June 2023 at 11:33:35 AEST\", \"type\": \"ASSESSMENT\", \"resultMessage\": \"Result string 2\", \"result\": \"SUCCESS\", \"validationMessages\": null }";
			AssertEquals("Should get latest success lodgement status response result string", "Result string 2", colsHeader.LodgementResultMessage);
		}

		public void TestIsCOLSFunctionEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PCOLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals("COLS is not enabled when both FUNCS and PFUNC are not enabled", false, QuarantineColsHeader.IsCOLSFunctionEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PCOLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals("COLS is enabled when either FUNCS and PFUNC is enabled", true, QuarantineColsHeader.IsCOLSFunctionEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PCOLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals("COLS is enabled when either FUNCS and PFUNC is enabled", true, QuarantineColsHeader.IsCOLSFunctionEnabled);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.COLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PCOLS, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals("COLS is enabled when both FUNCS and PFUNC are enabled", true, QuarantineColsHeader.IsCOLSFunctionEnabled);
			}
		}

		public void TestDiscardPendingMessages()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();

			var outboundMessage = colsHeader.Factory.New<COLSMessage>();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Queued;
			outboundMessage.EM_LinkedObject = colsHeader;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAdditionalDocument;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage1 = Factory.New<COLSMessage>();
			outboundAttachmentMessage1.EM_LinkedObject = doc1;
			outboundAttachmentMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage1.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage1.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAttachmentMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage1.EM_Status = EDIMessage.Status.Pending;
			var attachment1 = outboundAttachmentMessage1.MessageAttachments.AddNew();
			attachment1.EG_FileName = doc1.FileName;

			var doc2 = colsHeader.EDocPivotCollection.AddNew();
			var outboundAttachmentMessage2 = Factory.New<COLSMessage>();
			outboundAttachmentMessage2.EM_LinkedObject = doc2;
			outboundAttachmentMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundAttachmentMessage2.EM_MessageType = AUCOLSMessageTypeList.Codes.AddAttachment;
			outboundAttachmentMessage2.EM_MessageSubType = AUCOLSMessageSubTypeList.Codes.NormalAttachment;
			outboundAttachmentMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundAttachmentMessage2.EM_Status = EDIMessage.Status.Pending;
			var attachment2 = outboundAttachmentMessage1.MessageAttachments.AddNew();
			attachment2.EG_FileName = doc2.FileName;

			AssertNotNull("Pre-condition colsHeader Pending attachment messages", colsHeader.PendingAddAttachmentMessages);
			Assert(colsHeader.PendingAddAttachmentMessages.Length > 0);

			colsHeader.DiscardPendingAttachmentMessages();
			AssertEquals("Pending message should have been discarded - status should be set to DCD", EDIMessageStatusList.Codes.Discarded, outboundAttachmentMessage1.EM_Status);
			AssertEquals("Pending message should have been discarded", EDIMessageStatusList.Codes.Discarded, outboundAttachmentMessage2.EM_Status);
			AssertEquals("Associated document message status should be set to Discarded (DCD)", COLSDocumentStatusList.Codes.Discarded, doc1.CSD_MessageStatus);
			AssertEquals("Associated document message status should be set to DCD", COLSDocumentStatusList.Codes.Discarded, doc2.CSD_MessageStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			return colsHeader;
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new QuarantineColsHeaderTestLightValidationTester(bizObjToTest);

		sealed class QuarantineColsHeaderTestLightValidationTester : LightValidationTester
		{
			public QuarantineColsHeaderTestLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var bizo = info.BizObj;
				return !(bizo is AUJobDocAddress) && base.ShouldTestProperty(info);
			}
		}
	}
}
