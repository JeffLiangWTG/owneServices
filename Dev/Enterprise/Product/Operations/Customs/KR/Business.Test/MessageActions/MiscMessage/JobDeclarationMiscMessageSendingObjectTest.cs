using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationMiscMessageSendingObject))]
	sealed class JobDeclarationMiscMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			return new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
		}

		public void TestNewDate()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var messageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
			AssertEquals(ZDate.Empty, messageSendingObject.NewDate);

			messageSendingObject.NewDate = new ZDate("2021-12-30");
			AssertEquals(new ZDate("2021-12-30"), messageSendingObject.NewDate);
		}

		public void TestCustomsReceiptNumber()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			var messageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DP, MessageFunctions.MessageFunctionCode.Amendment, x => true);
			AssertEquals("010-D8-21-000350-1", messageSendingObject.CustomsReceiptNumber);
		}

		public void TestAmendmentTypeAndDescription()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			var message5AS_Extend = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);
			AssertEquals(_5ASAmendmentType.Codes.Extension, message5AS_Extend.AmendmentType);
			AssertEquals(_5ASAmendmentType.Descriptions.Extension, message5AS_Extend.AmendmentTypeDescription);

			var messageDKJ_Cancellation = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DKJ, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
			AssertEquals(_5ASAmendmentType.Codes.Cancellation, messageDKJ_Cancellation.AmendmentType);
			AssertEquals(_5ASAmendmentType.Descriptions.Cancellation, messageDKJ_Cancellation.AmendmentTypeDescription);

			var message5DR_Cancellation = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
			AssertEquals(LocalExportAmendmentTypeList.Codes.Cancellation, message5DR_Cancellation.AmendmentType);
			AssertEquals(LocalExportAmendmentTypeList.Descriptions.Cancellation, message5DR_Cancellation.AmendmentTypeDescription);

			var message5DS_Cancellation = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DS, MessageFunctions.MessageFunctionCode.Cancellation, x => true);
			AssertEquals(LocalExportAmendmentTypeList.Codes.Cancellation, message5DS_Cancellation.AmendmentType);
			AssertEquals(LocalExportAmendmentTypeList.Descriptions.Cancellation, message5DS_Cancellation.AmendmentTypeDescription);
		}

		public void TestMultipleCaptionsKey()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var parent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._DKJ, MessageFunctions.MessageFunctionCode.Cancellation);
			var multiplekey = ((ISupportMultipleResourceStringData)parent).MultipleKeysToUse[1];

			var attribute = typeof(JobDeclarationMiscMessageSendingObject).GetProperty(nameof(JobDeclarationMiscMessageSendingObject.AmendmentReason)).GetCustomAttributes<ResourceStringDataAttribute>().FirstOrDefault(x => x.MultipleKey == JobDeclarationMiscMessageSendingObject.CaptionKeyDKJCancellation);
			AssertEquals(multiplekey, attribute.MultipleKey);

			attribute = typeof(JobDeclarationMiscMessageSendingObject).GetProperty(nameof(JobDeclarationMiscMessageSendingObject.ReasonCode)).GetCustomAttributes<ResourceStringDataAttribute>().FirstOrDefault(x => x.MultipleKey == JobDeclarationMiscMessageSendingObject.CaptionKeyDKJCancellation);
			AssertEquals(multiplekey, attribute.MultipleKey);

			parent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Cancellation);
			multiplekey = ((ISupportMultipleResourceStringData)parent).MultipleKeysToUse[1];

			attribute = typeof(JobDeclarationMiscMessageSendingObject).GetProperty(nameof(JobDeclarationMiscMessageSendingObject.AmendmentReason)).GetCustomAttributes<ResourceStringDataAttribute>().FirstOrDefault(x => x.MultipleKey == JobDeclarationMiscMessageSendingObject.CaptionKey5DRCancellation);
			AssertEquals(multiplekey, attribute.MultipleKey);

			parent = new JobDeclarationMiscMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5DS, MessageFunctions.MessageFunctionCode.Cancellation);
			multiplekey = ((ISupportMultipleResourceStringData)parent).MultipleKeysToUse[1];

			attribute = typeof(JobDeclarationMiscMessageSendingObject).GetProperty(nameof(JobDeclarationMiscMessageSendingObject.AmendmentReason)).GetCustomAttributes<ResourceStringDataAttribute>().FirstOrDefault(x => x.MultipleKey == JobDeclarationMiscMessageSendingObject.CaptionKey5DSCancellation);
			AssertEquals(multiplekey, attribute.MultipleKey);
		}

		public void TestFilterEntryLines()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 50m;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 100m;
			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_CustomsValue = 200m;

			var messageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original, x => x.CL_CustomsValue > 50m);
			AssertEquals(2, messageSendingObject.MessageSendingEntryLines.Count);
		}

		public void TestPayerBusinessNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageStatus = KRJobMessageTypeList.Codes.Import;
			var payer = Factory.New<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.Business;
			payer.CustomsCodes.AddNew(IdentificationType.CorporationCode, "12341234");
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "56785678");
			declaration.JE_OH_DutyPayer = payer.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var parent = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original, x => true);
			AssertEquals("12341234", parent.PayerBusinessNumber);

			var payerNonIndividual = Factory.New<OrgHeader>();
			payerNonIndividual.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			payerNonIndividual.CustomsCodes.AddNew(IdentificationType.CorporationCode, "98769876");
			payerNonIndividual.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "43214321");
			declaration.JE_OH_DutyPayer = payerNonIndividual.PK;
			parent = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original, x => true);
			AssertEquals("43214321", parent.PayerBusinessNumber);
		}

		public void TestPopulateCancellationObject()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var messageDKJ = entry.Messages.AddNew();
			messageDKJ.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			messageDKJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageDKJ.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			var fileReader = new TestFileReader(typeof(ExportCancellationDetailsTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestFilesPath, "GOVCBRDKJ_Test.xml");
			messageDKJ.EM_MessageData = testMsgFile;
			var cancellationDetails = new ExportCancellationDetails(messageDKJ);
			AssertEquals("D", cancellationDetails.MessageSendingObjectDKJ.FaultParty);
			AssertEquals("99", cancellationDetails.MessageSendingObjectDKJ.ReasonCode);
			AssertEquals("화주업무 오류", cancellationDetails.MessageSendingObjectDKJ.AmendmentReason);
		}

		public void TestCaptions()
		{
			var sendingObject = GetNewBusinessObject() as JobDeclarationMiscMessageSendingObject;
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "AmendmentVersion", true, attrib => attrib.Caption == "Version No.");
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";
	}
}
