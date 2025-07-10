using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC044CHouseConsignmentWrapper : HouseConsignmentWrapper
	{
		protected CC044CHouseConsignmentWrapper(NctsBill bill) : base(bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		public new static CC044CHouseConsignmentWrapper New(NctsBill bill) => bill == null ? null : new CC044CHouseConsignmentWrapper(bill);

		protected bool StatusIsDif => bill.UnloadedStatus == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;

		public override decimal? GrossMass => grossMass ?? (grossMass = StatusIsDif ? bill.B0_GrossWeightUnloaded : bill.B0_Weight);
		decimal? grossMass;

		protected override ICollection<IDepartureTransportMeans> GetDepartureTransportMeans()
		{
			var result = new Collection<IDepartureTransportMeans>();
			bill.Header.ArrivalMovementHeader.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>().Where(t => t.TPM_TransportState == NctsUnloadedStateList.Codes.NEW || t.TPM_TransportState == NctsUnloadedStateList.Codes.MIS).ForEach(transportMeans => result.Add(CC044CArrivalTransportMeansWrapper.New(transportMeans)));
			return result;
		}

		protected override ICollection<ISupportingDocument> GetSupportingDocuments()
		{
			var result = new Collection<ISupportingDocument>();
			bill.SupportingDocuments.Cast<NctsSupportingDocument>().Where(s => s.CSI_Status != NctsUnloadedStateList.Codes.DEC).ForEach(document => result.Add(CC044CSupportingDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IDocument> GetTransportDocuments()
		{
			var result = new Collection<IDocument>();
			bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument && x.CSI_Status != NctsUnloadedStateList.Codes.DEC).ForEach(document => result.Add(CC044CDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IDocument> GetAdditionalReferences()
		{
			var result = new Collection<IDocument>();
			bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && x.CSI_Status != NctsUnloadedStateList.Codes.DEC).ForEach(document => result.Add(CC044CDocumentWrapper.New(document)));
			return result;
		}
		protected override ICollection<IConsignmentItem> GetConsignmentItems()
		{
			var result = new Collection<IConsignmentItem>();
			bill.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().Where(i => i.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF || i.BY_UnloadedState == NctsUnloadedStateList.Codes.MIS || i.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW).ForEach(item => result.Add(CC044CConsignmentItemWrapper.New(item)));
			return result;
		}
	}
}
