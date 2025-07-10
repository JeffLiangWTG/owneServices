using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(CommonAnnexH7SendMessageWrapper))]
	public class CommonAnnexH7SendMessageWrapperTest : H7CommonSendMessageWrapperBaseTest<CommonAnnexH7SendMessageWrapper>
	{
		public void TestOperation()
		{
			AssertEquals("Expect Operation code to be 02", "02", Provider.Operation);
		}

		public void TestReference()
		{
			AssertEquals("Expect Reference to be H7 MRN", h7CusEntryNum.CE_EntryNum, Provider.Reference);
		}

		public void TestRequestDispatchTagName()
		{
			AssertEquals("Expected filled RequestDispatchTagName is SolicitudDespacho", "SolicitudDespacho", Provider.RequestDispatchTagName);
		}

		public void TestDispatchRequest()
		{
			AssertEquals("Expect DispatchRequest to be 'S'", "S", Provider.DispatchRequest);
		}

		public void TestDispatchRequest_ClearanceNotRequested()
		{
			sendingAction.ClearanceRequested = false;

			AssertEquals("Expect DispatchRequest to be 'N'", "N", Provider.DispatchRequest);
		}

		public void TestDocument()
		{
			AssertNotNull(Provider.Document);
			AssertType<CommonAnnexDocWrapper>(Provider.Document);
		}

		public void TestAdministrationCode()
		{
			AssertEquals("Expect CustomsOffice to be 'ATC'", "ATC", Provider.AdministrationCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header.AMA_CustomsOffice = "ES0035test";

			h7CusEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			h7CusEntryNum.CE_ParentTable = "AsycudaBill";
			h7CusEntryNum.CE_ParentID = bill.PK;
			h7CusEntryNum.CE_EntryType = "MRN";
			h7CusEntryNum.CE_EntryLineReference = "H7";
			h7CusEntryNum.CE_RN_NKCountryCode = "ES";
			h7CusEntryNum.CE_EntryNum = "H7123";

			var g3CusEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			g3CusEntryNum.CE_ParentTable = "AsycudaBill";
			g3CusEntryNum.CE_ParentID = bill.PK;
			g3CusEntryNum.CE_EntryType = "MRN";
			g3CusEntryNum.CE_EntryLineReference = "G3";
			g3CusEntryNum.CE_RN_NKCountryCode = "ES";
			h7CusEntryNum.CE_EntryNum = "G3123";

			sendingAction = new UploadDocumentsSendingAction(bill);
			sendingAction.ClearanceRequested = true;
			var supportingDocument = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = "OPE";
			docSendingObject = sendingAction.AddInfoCollection[0].EDocsCollection.AddNew();
			docSendingObject.EDoc = supportingDocument.UniqueKey;
		}

		protected override CommonAnnexH7SendMessageWrapper GetWrapperCore(AsycudaBill bill, ICertificateProvider certificate)
		{
			return new CommonAnnexH7SendMessageWrapper(bill, certificate, docSendingObject.Document, sendingAction.AddInfoCollection[0], sendingAction);
		}

		UploadDocumentsSendingAction sendingAction;
		CusEntryNumber h7CusEntryNum;
		DocumentSendingObject docSendingObject;
	}
}
