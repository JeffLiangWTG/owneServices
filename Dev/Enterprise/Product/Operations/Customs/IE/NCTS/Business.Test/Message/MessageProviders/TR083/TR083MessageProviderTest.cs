using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class TR083MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<TR083MessageProvider>
	{
		public void TestLRN()
		{
			AssertEquals("LRN", "22045281480600000001", Provider.LRN);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		public void TestRequestDate()
		{
			AssertEquals("RequestDate", ZDateTime.Today.ToDateTime(), Provider.RequestDate);
		}

		public void TestAdditionalInformations()
		{
			var requestedDocument = nctsHeader.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var sendingObj = new AdditionalInfoSendingObject(Factory, Core.Constants.CountryCodes.Ireland);

			((IDocumentSendingMapper)Provider).AddAdditionalInfomation(sendingObj);
			AssertType<AdditionalInfoSendingObjectProvider>(Provider.AdditionalInformations.Single());
		}

		public void TestDeclaration()
		{
			AssertType<TR083MessageProvider>("Declaration", Provider.Declaration);
		}

		public void TestSupportingDocuments()
		{
			var suppDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var sendingObj = new DocumentSendingObject(nctsHeader);
			sendingObj.EDoc = suppDoc.UniqueKey;

			((IDocumentSendingMapper)Provider).AddSupportingDocument(sendingObj);
			AssertType<TR083SupportingDocumentProvider>(Provider.SupportingDocuments.Single());
		}

		protected override TR083MessageProvider GetProvider() => new TR083MessageProvider(new DocumentSendingAction(nctsHeader));

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			CusEntryNumber.LoadOrCreate(nctsHeader, "MRN", "IE").CE_EntryNum = "MRN001";
			var movementHeader = nctsHeader.MovementHeaders[0] ?? nctsHeader.MovementHeaders.AddNew();
			movementHeader.BM_PaperlessInbondNum = "22045281480600000001";
		}
		NctsHeader nctsHeader;
	}
}
