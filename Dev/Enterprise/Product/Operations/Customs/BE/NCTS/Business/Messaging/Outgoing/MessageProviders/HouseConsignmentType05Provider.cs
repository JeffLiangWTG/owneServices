using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class HouseConsignmentType05Provider : IHouseConsignmentType05
	{
		readonly NctsBill bill;
		bool isMissing => bill.UnloadedStatus == NctsUnloadedStateList.Codes.MIS;
		public HouseConsignmentType05Provider(NctsBill bill, ZString sequenceNumber)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			SequenceNumber = int.TryParse(sequenceNumber, out int value) ? value : 0;
		}

		public int SequenceNumber { get; }

		public decimal? GrossMass
		{
			get
			{
				decimal? result = null;
				if (!isMissing)
				{
					var movementDetail = bill.MovementDetail;
					var unloadedState = movementDetail.B9_UnloadedState;
					if (unloadedState != NctsUnloadedStateList.Codes.DIF)
					{
						result = unloadedState == NctsUnloadedStateList.Codes.MIS ? null : bill.B0_Weight;
					}
					else
					{
						var differenceMoveDetail = movementDetail.DifferenceMoveDetail;
						if (differenceMoveDetail != null)
						{
							result = differenceMoveDetail.DifferenceWeight;
						}
					}
				}

				return result;
			}
		}

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??=
			isMissing 
				? Array.Empty<IDepartureTransportMeans>()
				: bill.ArrivalTransportInfos
					.Where(t => t.TPM_TransportState == NctsUnloadedStateList.Codes.NEW || t.TPM_TransportState == NctsUnloadedStateList.Codes.MIS)
					.Select(t => new CC044CDepartureTransportMeansProvider(t))
					.ToArray<IDepartureTransportMeans>();
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= Array.Empty<ISupportingDocument>();
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??=
			isMissing
				? Array.Empty<IDocument>()
				: NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
					.Where(t => t.CSI_Status != NctsUnloadedStateList.Codes.DEC)
					.Select(t => new CC044CAdditionalReferenceProvider(t))
					.ToArray();
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IConsignmentItemType05> ConsignmentItems => consignmentItems ?? (consignmentItems = isMissing ? Array.Empty<IConsignmentItemType05>() : bill.ArrivalGoodsItems
			.Where(i => i.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF || (i.BY_UnloadedState == NctsUnloadedStateList.Codes.MIS && bill.MovementDetail.B9_UnloadedState != NctsUnloadedStateList.Codes.MIS) || i.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW)
			.Select(i => new ConsignmentItemType05Provider(i))
			.OrderBy(x => x.GoodsItemNumber)
			.ToArray());
		IReadOnlyCollection<IConsignmentItemType05> consignmentItems;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ??=
			isMissing
				? Array.Empty<ITransportDocument>()
				: NctsDataRetrieveMethods.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
					.Where(t => t.CSI_Status != NctsUnloadedStateList.Codes.DEC)
					.Select(t => new CC044CTransportDocumentProvider(t))
					.ToArray();
		IReadOnlyCollection<ITransportDocument> transportDocuments;
	}
}
