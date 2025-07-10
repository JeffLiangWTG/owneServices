using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.Dash.Business.Testing
{
	public class DashBusinessObjectTestHelpers
	{
		readonly BusinessObjectFactory Factory;
		readonly DocumentFactory DocumentFactory;

		public DashBusinessObjectTestHelpers(BusinessObjectFactory factory) {
			Factory = factory;
			DocumentFactory = new DocumentFactoryProvider().GetFactory(Factory);
		}

		public DashDocument CreateDashDocument(ZGuid eDocPK, string parseType) {
			var dashDocument = Factory.New<DashDocument>();
			dashDocument.DDD_ParseType = parseType;
			dashDocument.DDD_ParseStatus = "REV";
			dashDocument.DDD_DocID = eDocPK;

			return dashDocument;
		}

		public DashDocument CreateDashDocument(string parseType)
		{
			return CreateDashDocument(ZGuid.Empty, parseType);
		}

		public DashCommercialInvoice CreateDashCommercialInvoice(ZGuid dashDocumentPK)
		{
			var dashCommercialInvoice = Factory.New<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_DDD_DashDocID = dashDocumentPK;
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_Incoterm = "CFR";
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "GBP";

			return dashCommercialInvoice;
		}

		public DashCommercialInvoiceLineItem CreateDashCommercialInvoiceLineItem(ZGuid dashCommercialInvoicePK)
		{
			var dashCommercialInvoiceLineItem = Factory.New<DashCommercialInvoiceLineItem>();
			dashCommercialInvoiceLineItem.DLI_DCI_HeaderID = dashCommercialInvoicePK;
			dashCommercialInvoiceLineItem.DLI_F3_NKUnitType = "BAG";
			dashCommercialInvoiceLineItem.DLI_MatchedType = "EXT";

			return dashCommercialInvoiceLineItem;
		}

		public DashDocCoordinate CreateDashDocCoordinate(ZGuid dashDocumentPK, ZGuid dashPK, string parseType)
		{
			var dashDocCoordinate = Factory.New<DashDocCoordinate>();
			dashDocCoordinate.DDC_DDD_DashDocID = dashDocumentPK;
			dashDocCoordinate.DDC_ParentID = dashPK;
			switch (parseType)
			{
				case "CIV":
					dashDocCoordinate.DDC_ParentTableCode = "DCI";
					break;
				case "PIN":
					dashDocCoordinate.DDC_ParentTableCode = "DPI";
					break;
				default:
					break;
			}
			dashDocCoordinate.DDC_AttributeName = "InvoiceNumber";

			return dashDocCoordinate;
		}

		public DashAPInvoice CreateDashAPInvoice(ZGuid dashDocumentPK)
		{
			var dashAPInvoice = Factory.New<DashAPInvoice>();
			dashAPInvoice.DPI_DDD_DashDocID = dashDocumentPK;

			return dashAPInvoice;
		}

		public DashAPInvoiceChargeLine CreateDashAPInvoiceChargeLine(ZGuid dashAPInvoiceClusterPK)
		{
			var dashAPInvoiceChargeLine = Factory.New<DashAPInvoiceChargeLine>();
			dashAPInvoiceChargeLine.DPL_DPC_ClusterID = dashAPInvoiceClusterPK;

			return dashAPInvoiceChargeLine;
		}

		public DashAPInvoiceChargeLineRef CreateDashAPInvoiceChargeLineRef(ZGuid dashAPInvoiceChargeLinePK, ZGuid dashAPInvoiceRefPK)
		{
			var dashAPInvoiceChargeLineRef = Factory.New<DashAPInvoiceChargeLineRef>();
			dashAPInvoiceChargeLineRef.DLR_DPL_ChargeLineID = dashAPInvoiceChargeLinePK;
			dashAPInvoiceChargeLineRef.DLR_DPR_RefID = dashAPInvoiceRefPK;

			return dashAPInvoiceChargeLineRef;
		}

		public DashAPInvoiceCluster CreateDashAPInvoiceCluster(ZGuid dashAPInvoicePK)
		{
			var dashAPInvoiceCluster = Factory.New<DashAPInvoiceCluster>();
			dashAPInvoiceCluster.DPC_DPI_HeaderID = dashAPInvoicePK;

			return dashAPInvoiceCluster;
		}

		public DashAPInvoiceClusterRef CreateDashAPInvoiceClusterRef(ZGuid dashAPInvoiceClusterPK, ZGuid dashAPInvoiceRefPK)
		{
			var dashAPInvoiceClusterRef = Factory.New<DashAPInvoiceClusterRef>();
			dashAPInvoiceClusterRef.DRC_DPC_ClusterID = dashAPInvoiceClusterPK;
			dashAPInvoiceClusterRef.DRC_DPR_RefID = dashAPInvoiceRefPK;

			return dashAPInvoiceClusterRef;
		}

		public DashAPInvoiceRef CreateDashAPInvoiceRef(ZGuid dashAPInvoicePK)
		{
			var dashAPInvoiceRef = Factory.New<DashAPInvoiceRef>();
			dashAPInvoiceRef.DPR_DPI_HeaderID = dashAPInvoicePK;

			return dashAPInvoiceRef;
		}

		public DashDocumentText CreateDashDocumentText(ZGuid dashDocumentPK, string text)
		{
			var dashDocumentText = Factory.New<DashDocumentText>();
			dashDocumentText.DDT_DDD_DashDocID = dashDocumentPK;
			dashDocumentText.DDT_DocumentText = text;

			return dashDocumentText;
		}

		public DashMatchingConfig CreateDashMatchingConfig(string url, string text)
		{
			var dashMatchingConfig = Factory.New<DashMatchingConfig>();
			dashMatchingConfig.DMC_VersionURL = url;
			dashMatchingConfig.DMC_MatchingConfig = ZBlob.FromUTF8(text);

			return dashMatchingConfig;
		}

		public DashOrgCandidate CreateDashOrgCandidate(ZGuid dashDocumentPK, string orgType)
		{
			var dashOrgCandidate = Factory.New<DashOrgCandidate>();
			dashOrgCandidate.DOG_DDD_DashDocID = dashDocumentPK;
			dashOrgCandidate.DOG_OrgType = orgType;

			return dashOrgCandidate;
		}

		public DashOrgCandidateAddress CreateDashOrgCandidateAddress(ZGuid orgCandidatePK, string address)
		{
			var dashOrgCandidateAddress = Factory.New<DashOrgCandidateAddress>();
			dashOrgCandidateAddress.DOA_DOG_DashOrgCandidateID = orgCandidatePK;
			dashOrgCandidateAddress.DOA_Address = address;

			return dashOrgCandidateAddress;
		}

		public DashOrgCandidateName CreateDashOrgCandidateName(ZGuid orgCandidatePK, string name)
		{
			var dashOrgCandidateName = Factory.New<DashOrgCandidateName>();
			dashOrgCandidateName.DON_DOG_DashOrgCandidateID = orgCandidatePK;
			dashOrgCandidateName.DON_Name = name;

			return dashOrgCandidateName;
		}

		public StorageDocsBase CreateStorageFile()
		{
			const string parseType = "CIV";
			const string documentFileType = "PDF";

			var eDoc = DocumentFactory.NewWithParent(typeof(StorageFile));
			eDoc.SC_DocType = parseType;
			eDoc.SC_DataType = documentFileType;
			eDoc.ParentMain.SM_DB = 1;
			eDoc.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
			eDoc.ParentMain.SM_Type = Enterprise.Core.Constants.DocManagerCodes.Shipment;

			DocumentFactory.Save();

			return eDoc;
		}
	}
}
