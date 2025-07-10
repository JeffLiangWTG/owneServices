using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public static class NctsHeaderDocumentsSequenceNumberHelper
	{
		public static void AssignSupportingDocumentsSequenceNumbers(NctsHeader header)
		{
			if (IsPhase5DepartureAndTransitionPeriod(header))
			{
				MoveSupportingDocumentsToGoodsItems(header);

				foreach (var bill in header.Bills)
				{
					var latestAssignedSeqInBill = 0;

					foreach (var goodsItem in bill.GoodsItems)
					{
						AssignSequenceToGoodsItemDocs(goodsItem, DocumentType.SupportingDocument, latestAssignedSeqInBill);
					}
				}
			}
		}

		static void MoveSupportingDocumentsToGoodsItems(NctsHeader header)
		{
			var headerDocs = header.MovementHeader.SupportingDocuments.Cast<CusSupportingInfo>().ToList();

			foreach (var bill in header.Bills)
			{
				var billDocs = bill.SupportingDocuments.Cast<CusSupportingInfo>().ToList();

				foreach (var goodsItem in bill.GoodsItems)
				{
					foreach (var billDoc in billDocs)
					{
						goodsItem.SupportingDocuments.Insert(0, billDoc.Clone());
					}

					foreach (var headerDoc in headerDocs)
					{
						goodsItem.SupportingDocuments.Insert(0, headerDoc.Clone());
					}
				}

				bill.SupportingDocuments.RemoveAndDeleteAll();
			}

			header.MovementHeader.SupportingDocuments.RemoveAndDeleteAll();
		}

		public static void AssignAdditionalDocumentsSequenceNumbers(NctsHeader header)
		{
			if (IsPhase5DepartureAndTransitionPeriod(header))
			{
				AssignSequenceToAllDocsOfSpecificType(header, DocumentType.AdditionalInfo_INF);
				AssignSequenceToAllDocsOfSpecificType(header, DocumentType.AdditionalInfo_REF);
				AssignSequenceToAllDocsOfSpecificType(header, DocumentType.AdditionalInfo_TRA);
			}
		}

		static bool IsPhase5DepartureAndTransitionPeriod(NctsHeader header) => header != null && header.IsPhase5Departure && header.IsInPhase5TransitionPeriod;

		static void AssignSequenceToAllDocsOfSpecificType(NctsHeader header, DocumentType docType)
		{
			var latestAssignedSeqInHeader = AssignSequenceToHeaderDocs(header, docType);

			foreach (var bill in header.Bills)
			{
				var latestAssignedSeqInBill = AssignSequenceToBillDocs(bill, docType, latestAssignedSeqInHeader);

				foreach (var goodsItem in bill.GoodsItems)
				{
					AssignSequenceToGoodsItemDocs(goodsItem, docType, latestAssignedSeqInBill);
				}
			}
		}

		static int AssignSequenceToHeaderDocs(NctsHeader header, DocumentType docType)
		{
			var headerDocs = new List<CusSupportingInfo>();

			if (docType == DocumentType.AdditionalInfo_INF)
			{
				headerDocs = header.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_REF)
			{
				headerDocs = header.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_TRA)
			{
				headerDocs = header.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToList();
			}

			var headerBaseNumber = 1;
			var unassignedHeaderDocs = headerDocs
				.Select((g, i) => (Document: g, Increment: i));

			foreach (var (document, increment) in unassignedHeaderDocs)
			{
				document.CSI_LineNo = headerBaseNumber + increment;
			}

			return LatestAssignedSequenceForDocuments(headerDocs);
		}

		static int AssignSequenceToBillDocs(NctsBill bill, DocumentType docType, int latestAssignedSeqInHeader)
		{
			var billDocs = new List<CusSupportingInfo>();

			if (docType == DocumentType.AdditionalInfo_INF)
			{
				billDocs = bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_REF)
			{
				billDocs = bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_TRA)
			{
				billDocs = bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToList();
			}

			var billBaseNumber = latestAssignedSeqInHeader + 1;
			var unassignedBillDocs = billDocs
				.Select((g, i) => (Document: g, Increment: i));

			foreach (var (document, increment) in unassignedBillDocs)
			{
				document.CSI_LineNo = billBaseNumber + increment;
			}

			return LatestAssignedSequenceForDocuments(billDocs);
		}

		static void AssignSequenceToGoodsItemDocs(NctsDepartureCargoDesc goodsItem, DocumentType docType, int latestAssignedSeqInBill)
		{
			var goodsItemDocs = new List<CusSupportingInfo>();

			if (docType == DocumentType.SupportingDocument)
			{
				goodsItemDocs = goodsItem.SupportingDocuments.Cast<CusSupportingInfo>().OrderBy(doc => doc.CSI_Code.IsEmpty ? 0 : 1).ThenBy(doc => !doc.CSI_Code.IsEmpty && !char.IsDigit(doc.CSI_Code[0]) ? 0 : 1).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_INF)
			{
				goodsItemDocs = goodsItem.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_REF)
			{
				goodsItemDocs = goodsItem.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToList();
			}
			else if (docType == DocumentType.AdditionalInfo_TRA)
			{
				goodsItemDocs = goodsItem.AdditionalInfos.Cast<CusSupportingInfo>().Where(doc => doc.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToList();
			}

			var goodsItemBaseNumber = latestAssignedSeqInBill + 1;
			var unassignedGoodsItemDocs = goodsItemDocs
				.Select((g, i) => (Document: g, Increment: i));

			foreach (var (document, increment) in unassignedGoodsItemDocs)
			{
				document.CSI_LineNo = goodsItemBaseNumber + increment;
			}
		}

		static int LatestAssignedSequenceForDocuments(IEnumerable<CusSupportingInfo> documentList)
		{
			return documentList.MaxOrDefault(e => e.CSI_LineNo);
		}

		public static void ResetSupportingDocumentsLineNoWhenPhase5(NctsHeader header, Integration.Customs.ICusSupportingInfoTypeSupporter parent, bool forceReset = false)
		{
			if (IsPhase5DepartureAndTransitionPeriod(header) && (forceReset || (header.MovementHeader?.IsCustomsStatusPRE ?? false)))
			{
				if (parent is NctsCommonCargoDesc)
				{
					((NctsCommonCargoDesc)parent).SupportingDocuments.ForEach(x => x.CSI_LineNo = 0);
				}
				else if (parent is NctsBill)
				{
					var bill = (NctsBill)parent;
					bill.SupportingDocuments.ForEach(x => x.CSI_LineNo = 0);
					bill.GoodsItems.ForEach(i => i.SupportingDocuments.ForEach(x => x.CSI_LineNo = 0));
				}
				else if (parent is NctsDepartureMovementHeader)
				{
					header.MovementHeader.SupportingDocuments.ForEach(x => x.CSI_LineNo = 0);
					header.Bills.ForEach(b => b.SupportingDocuments.ForEach(x => x.CSI_LineNo = 0));
					header.Bills.ForEach(b => b.GoodsItems.ForEach(i => i.SupportingDocuments.ForEach(x => x.CSI_LineNo = 0)));
				}
			}
		}

		public static void ResetAdditionalDocumentsLineNoWhenPhase5(NctsHeader header, Integration.Customs.ICusSupportingInfoTypeSupporter parent, ZString subType, bool forceReset = false)
		{
			if (IsPhase5DepartureAndTransitionPeriod(header) && (forceReset || (header.MovementHeader?.IsCustomsStatusPRE ?? false)))
			{
				if (parent is NctsCommonCargoDesc)
				{
					((NctsCommonCargoDesc)parent).AdditionalInfos.Where(doc => doc.CSI_SubType == subType).ForEach(x => x.CSI_LineNo = 0);
				}
				else if (parent is NctsBill)
				{
					var bill = (NctsBill)parent;
					bill.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Where(doc => doc.CSI_SubType == subType).ForEach(x => x.CSI_LineNo = 0);
					bill.GoodsItems.ForEach(i => i.AdditionalInfos.Where(doc => doc.CSI_SubType == subType).ForEach(x => x.CSI_LineNo = 0));
				}
				else if (parent is NctsHeader)
				{
					header.AdditionalDocuments.Where(doc => doc.CSI_SubType == subType).ForEach(x => x.CSI_LineNo = 0);
					header.Bills.ForEach(b => b.AdditionalDocuments.Cast<NctsBillAdditionalDocument>().Where(doc => doc.CSI_SubType == subType).ForEach(x => x.CSI_LineNo = 0));
					header.Bills.ForEach(b => b.GoodsItems.ForEach(i => i.AdditionalInfos.Where(doc => doc.CSI_SubType == subType).ForEach(x => x.CSI_LineNo = 0)));
				}
			}
		}

		public enum DocumentType
		{
			SupportingDocument,
			AdditionalInfo_INF,
			AdditionalInfo_REF,
			AdditionalInfo_TRA,
		}
	}
}
