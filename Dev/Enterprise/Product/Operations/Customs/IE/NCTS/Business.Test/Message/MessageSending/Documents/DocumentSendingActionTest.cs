using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentSendingAction))]
	class DocumentSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader() => AssertType<NctsHeader>(GetNewDocumentSendingAction().nctsHeader);

		public void TestMovementReference_Caption()
		{
			var sendingObj = GetNewDocumentSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.MovementReferenceInfo);
			AssertEquals("MovementReferenceInfo caption", "Movement Reference Number", resData.Caption);
			AssertEquals("MovementReferenceInfo ShortCaption", "MRN", resData.ShortCaption);
		}

		public void TestMovementReference()
		{
			var sendingObj = GetNewDocumentSendingAction();
			AssertEquals("MovementReference", "MRN123", sendingObj.MovementReference);
		}

		public void TestLocalReference_Caption()
		{
			var sendingObj = GetNewDocumentSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.LocalReferenceInfo);
			AssertEquals("LocalReferenceInfo caption", "Local Reference Number", resData.Caption);
			AssertEquals("LocalReferenceInfo ShortCaption", "LRN", resData.ShortCaption);
		}

		public void TestLocalReference()
		{
			var sendingObj = GetNewDocumentSendingAction();
			AssertEquals("LocalReference", "LRN123", sendingObj.LocalReference);
		}

		public void TestValidationType()
		{
			var sendingObj = GetNewDocumentSendingAction();
			AssertType("Should have created a correct Validation.", typeof(DocumentSendingActionValidation), sendingObj.Validation);
		}

		public void TestDefaultValue()
		{
			var sendingObj = GetNewDocumentSendingAction();
			Assert("ShouldSend", sendingObj.ShouldSend);
		}

		public void TestCreateSender()
		{
			var sendingObj = GetNewDocumentSendingAction();
			AssertType<DocumentsSender>("CreateSender", sendingObj.CreateSender());
		}

		public void TestMessageAttachee()
		{
			var sendingObj = GetNewDocumentSendingAction();
			AssertType<NctsDepartureMovementHeader>("Departure MessageAttachee", sendingObj.MessageAttachee);

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			AssertType<NctsHeader>("Arrival MessageAttachee", new DocumentSendingAction(header).MessageAttachee);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, MasterFiles.Business.GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";
			header.MovementHeader.BM_PaperlessInbondNum = "LRN123";
			return new DocumentSendingAction(header);
		}

		DocumentSendingAction GetNewDocumentSendingAction() => (DocumentSendingAction)GetNewBusinessObject();
	}
}
