using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5HouseConsignmentWrapper : NCTS5CommonHouseConsignmentWrapper, INotifUnloadingNCTSHouseConsignment
	{
		public NotificationUnloadingNCTS5HouseConsignmentWrapper(NctsBill houseConsignment) : base(houseConsignment)
		{
			isUnloadedStateNEWorDIF = houseConsignment.MovementDetail.B9_UnloadedState.IsUnloadingStateNEWorDIF();
		}
		readonly ZBool isUnloadedStateNEWorDIF;

		protected override ZDecimal GrossMassCore => houseConsignment.MovementDetail.B9_UnloadedState.IsUnloadingStateDIF()
																	? GetUnloadedGrossMass()
																	: base.GrossMassCore;

		public ZBool GrossMassSpecified => isUnloadedStateNEWorDIF && houseConsignment.GrossWeightInKilograms.Round(WeightMaxDecimals) != GetUnloadedGrossMass();

		public IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = new List<CommonDepartureTransportMeansWrapper>().AsReadOnly());
		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> departureTransportMeans;

		public IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument
		{
			get
			{
				if (supportingDocument == null)
				{
					if (isUnloadedStateNEWorDIF)
					{
						var addDocs = houseConsignment.SupportingDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Status.IsUnloadingStateNEWorMISorDIF()).ToList();
						var orderedDocs = addDocs.OrderBy(doc => doc.CSI_LineNo);

						var docList = new List<NCTS5CommonDocumentWithInfoWrapper>();
						foreach (var document in orderedDocs)
						{
							docList.Add(document.CSI_Status.IsUnloadingStateNEWorDIF()
													? new NCTS5CommonDocumentWithInfoWrapper(document, document.CSI_LineNo)
													: new NCTS5CommonDocumentWithInfoWrapper(ZString.Empty, ZString.Empty, ZString.Empty, document.CSI_LineNo));
						}

						supportingDocument = docList.AsReadOnly();
					}
					else
					{
						supportingDocument = new List<NCTS5CommonDocumentWithInfoWrapper>().AsReadOnly();
					}
				}
				return supportingDocument;
			}
		}
		IReadOnlyCollection<NCTS5CommonDocumentWithInfoWrapper> supportingDocument;

		public IReadOnlyCollection<INotifUnloadingNCTSConsignmentItem> ConsignmentItem => 
				consignmentItem ?? (consignmentItem = isUnloadedStateNEWorDIF
														? houseConsignment.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>()
																						.Where(x => x.BY_UnloadedState.IsUnloadingStateMISorNEW() || x.HasNctsArrivalGoodItemDifferences())
																						.Select(x => new NotificationUnloadingNCTS5ConsignmentItemWrapper(x))
																						.ToList().AsReadOnly()
														: null);
		IReadOnlyCollection<NotificationUnloadingNCTS5ConsignmentItemWrapper> consignmentItem;

		ZDecimal GetUnloadedGrossMass() => houseConsignment.B0_GrossWeightUnloaded.Round(WeightMaxDecimalsFinalPeriod);

		protected override IReadOnlyCollection<CommonDocumentSequenceNumberWrapper> GetAdditionalDocumentsWrapperListForSubType(ZString subType)
		{
			if (isUnloadedStateNEWorDIF)
			{
				var docs = new List<CommonDocumentSequenceNumberWrapper>();
				var addDocs = houseConsignment.AdditionalDocuments.Cast<CusSupportingInfo>().Where(doc => doc.CSI_Status.IsUnloadingStateNEWorMISorDIF() && doc.CSI_SubType == subType).ToList();
				var orderedDocs = addDocs.OrderBy(doc => doc.CSI_LineNo);

				foreach (var doc in orderedDocs)
				{
					docs.Add(doc.CSI_Status.IsUnloadingStateNEWorDIF()
													? new CommonDocumentSequenceNumberWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber, doc.CSI_LineNo)
													: new CommonDocumentSequenceNumberWrapper(ZString.Empty, ZString.Empty, doc.CSI_LineNo));
				}
				return docs.AsReadOnly();
			}
			else
			{
				return new List<CommonDocumentSequenceNumberWrapper>().AsReadOnly();
			}
		}
	}
}
