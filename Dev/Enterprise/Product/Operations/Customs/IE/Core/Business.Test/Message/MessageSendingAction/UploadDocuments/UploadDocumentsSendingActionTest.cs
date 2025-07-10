using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Moq;
using NUnit.Framework;
using ZPropertyInfoExtensions = Enterprise.Customs.Business.ZPropertyInfoExtensions;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingAction))]
	sealed class UploadDocumentsSendingActionTest : CusEntryHeaderMessageSendingActionTest<UploadDocumentsSendingAction>
	{
		public void TestIsUCC5()
		{
			var declaration = entryHeader.Declaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var sendingAction = new UploadDocumentsSendingAction(entryHeader);
			AssertEquals("Precondition: declaration.IsUCC5 is true", true, declaration.IsUCC5);
			AssertEquals("Return declaration.IsUCC5", true, sendingAction.IsUCC5);

			entryHeader = Factory.New<CusEntryHeader>();
			sendingAction = new UploadDocumentsSendingAction(entryHeader);
			AssertNull("Precondition: declaration is null", entryHeader.Declaration);
			AssertEquals("Return false when declaration is null", false, sendingAction.IsUCC5);
		}

		public void TestShouldSend_Set_CallsValidateAll()
		{
			var sendingActionForTest = new UploadDocumentsSendingActionForTest(entryHeader);

			sendingActionForTest.ShouldSend = ZBool.False;

			(sendingActionForTest.GetValidationMock()).Verify(v => v.ValidateAll(), Times.Once);

			Assert("Needed to avoid 'Empty test' error", true);
		}

		public void TestMessageTypeDescription()
		{
			uploadDocumentsSendingAction.MessageType = AISUploadDocumentsMessageTypeList.Codes.IM483;
			AssertEquals(uploadDocumentsSendingAction.MessageTypeDescription, "Upload Documents");
		}

		public void TestMovementReference()
		{
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MovementReference", "MRN001", uploadDocumentsSendingAction.MovementReferenceNumber);
		}

		public void TestMovementReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(uploadDocumentsSendingAction.MovementReferenceNumberInfo);
			AssertEquals("MovementReferenceInfo full description", "Movement Reference Number", resData.FullDescription);
			AssertEquals("MovementReferenceInfo Caption", "MRN", resData.Caption);
		}

		public void TestAddInfoCollection()
		{
			AssertType("AddInfoCollection type", typeof(AdditionalInfoSendingObjectCollection), uploadDocumentsSendingAction.AddInfoCollection);
		}

		public void TestAddInfoCollectionForIM483()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AddNewRequestedDocument("1Q99", "Please submit relevant documentation to support the declarabon.", "");
			AddNewRequestedDocument("1Q99", "Please submit documents in PDF format, the actual documents can not open.", "");
			AddNewRequestedDocument("1R24", "Please reupload the documents as they are not readable.", "");
			AddNewRequestedDocument("1Q99", "The files are sill unreadable. Please try uploading in a different format e.g. PDF", "RN001");
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingAction = new UploadDocumentsSendingAction(entryHeader);

			var addInfoCollection = sendingAction.AddInfoCollection;

			AssertEquals("Count", 2, addInfoCollection.Count);
			AssertEquals("DocumentType is not 1Q99", "1R24", addInfoCollection[0].DocumentType);
			AssertEquals("ReferenceNumber has value", "RN001", addInfoCollection[1].ReferenceNumber);

			void AddNewRequestedDocument(ZString type, ZString info, ZString referenceNumber)
			{
				var requestedDocument = instruction.RequestedDocuments.AddNew();
				requestedDocument.CSI_Code = type;
				requestedDocument.RequestInformation = info;
				requestedDocument.CSI_DateOfIssue = ZDateTime.Now.AddDays(-1);
				requestedDocument.CSI_DateOfExpiry = ZDateTime.Now.AddDays(6);
				requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
				requestedDocument.CSI_ReferenceNumber = referenceNumber;
			}
		}

		public void TestDefaultValues()
		{
			var newUploadDocumentsSendingAction = new UploadDocumentsSendingAction(Factory.NewWithValidTestData<CusEntryHeader>());
			CombineAssertions(() =>
			{
				AssertEquals("ShouldSend", true, newUploadDocumentsSendingAction.ShouldSend);
				AssertEquals("MessageType", AISUploadDocumentsMessageTypeList.Codes.IM483, newUploadDocumentsSendingAction.MessageType);
			});
		}

		public void TestAlternativeDateOfAcceptance()
		{
			AssertEquals("Alternative Date of Acceptance", ZDateTime.Empty, uploadDocumentsSendingAction.AlternativeDateOfAcceptance);
		}

		public void TestCustomsReferenceNumber()
		{
			AssertEquals("Customs Reference", ZString.Empty, uploadDocumentsSendingAction.CustomsReferenceNumber);
		}

		public void TestCustomsJustification()
		{
			AssertEquals("Customs Justification", ZString.Empty, uploadDocumentsSendingAction.CustomsJustification);
		}

		protected override Type ExpectedLookupsType => typeof(UploadDocumentsSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(UploadDocumentsSender);

		protected override Type ExpectedValidationType => typeof(UploadDocumentsSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject() => uploadDocumentsSendingAction;

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
		}

		CusEntryHeader entryHeader;
		UploadDocumentsSendingAction uploadDocumentsSendingAction;

		class UploadDocumentsSendingActionForTest : UploadDocumentsSendingAction
		{
			public UploadDocumentsSendingActionForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public new UploadDocumentsSendingActionValidation Validation => (UploadDocumentsSendingActionValidation)GetNewValidation();

			public Mock<UploadDocumentsSendingActionValidation> GetValidationMock()
			{
				return validationMock ??= new Mock<UploadDocumentsSendingActionValidation>(this);
			}
			Mock<UploadDocumentsSendingActionValidation> validationMock;

			protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => GetValidationMock().Object;
		}
	}
}
