using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentWrapper : IDeclarationConsignment
	{
		DeclarationConsignmentWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentWrapper(asycudaBill) : null;

		public ICollection<IDeclarationConsignmentAcceptancePlace> AcceptancePlace
			=> new Collection<IDeclarationConsignmentAcceptancePlace> { DeclarationConsignmentAcceptancePlaceWrapper.NewOrNull(asycudaBill) };

		public ICollection<IDeclarationConsignmentAdditionalDocument> AdditionalDocument => new List<IDeclarationConsignmentAdditionalDocument>().AsReadOnly();

		public ICollection<IDeclarationConsignmentAssociatedTransportDocument> AssociatedTransportDocument
			=> new List<IDeclarationConsignmentAssociatedTransportDocument>().AsReadOnly();

		public IQuantityType BoardedQuantity => null;

		public ICollection<IDeclarationConsignmentCarrier> Carrier => new List<IDeclarationConsignmentCarrier>().AsReadOnly();

		public ICollection<IDeclarationConsignmentConsignee> Consignee
			=> new Collection<IDeclarationConsignmentConsignee> { DeclarationConsignmentConsigneeWrapper.NewOrNull(asycudaBill) };

		public ICollection<IDeclarationConsignmentConsignmentItem> ConsignmentItem
		{
			get
			{
				var collection = new Collection<IDeclarationConsignmentConsignmentItem>();

				foreach (var packedItem in asycudaBill.PackedItems)
				{
					collection.Add(DeclarationConsignmentConsignmentItemWrapper.NewOrNull(packedItem));
				}

				return collection;
			}
		}

		public ICollection<IDeclarationConsignmentConsignor> Consignor
			=> new Collection<IDeclarationConsignmentConsignor> { DeclarationConsignmentConsignorWrapper.NewOrNull(asycudaBill) };

		public ICollection<IDeclarationConsignmentFreight> Freight => new List<IDeclarationConsignmentFreight>().AsReadOnly();

		public ICollection<IDeclarationConsignmentGoodsConsignedPlace> GoodsConsignedPlace => new List<IDeclarationConsignmentGoodsConsignedPlace>().AsReadOnly();

		public ICollection<IDeclarationConsignmentGoodsReceiptPlace> GoodsReceiptPlace
			=> new Collection<IDeclarationConsignmentGoodsReceiptPlace> { DeclarationConsignmentGoodsReceiptPlaceWrapper.NewOrNull(asycudaBill) };

		public ICollection<IDeclarationConsignmentGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItem
			=> new Collection<IDeclarationConsignmentGovernmentAgencyGoodsItem> { DeclarationConsignmentGovernmentAgencyGoodsItemWrapper.NewOrNull(asycudaBill) };

		public IMeasureType GrossVolumeMeasure => MeasureTypeWrapper.NewOrNull(asycudaBill.ABL_Volume, asycudaBill.ABL_VolumeUQ);

		public ICollection<IDeclarationConsignmentLoadingLocation> LoadingLocation
			=> new Collection<IDeclarationConsignmentLoadingLocation> { DeclarationConsignmentLoadingLocationWrapper.NewOrNull(asycudaBill.Header?.MasterBill) };

		public ICollection<IDeclarationConsignmentNotifyParty> NotifyParty
		{
			get
			{
				var collection = new Collection<IDeclarationConsignmentNotifyParty>();

				var item = DeclarationConsignmentNotifyPartyWrapper.NewOrNull(asycudaBill);
				if (item != null)
				{
					collection.Add(item);
				}

				return collection;
			}
		}

		public decimal? SequenceNumeric => asycudaBill.ABL_SequenceNumber;

		public IQuantityType TotalPackageQuantity
			=> QuantityTypeWrapper.NewOrNull((ZDecimal)asycudaBill.ABL_ManifestQty, null);

		public ICollection<IDeclarationConsignmentTranshipmentLocation> TranshipmentLocation => new List<IDeclarationConsignmentTranshipmentLocation>().AsReadOnly();

		public ICollection<IDeclarationConsignmentTransitDestination> TransitDestination => new List<IDeclarationConsignmentTransitDestination>().AsReadOnly();

		public ICollection<IDeclarationConsignmentTransportContractDocument> TransportContractDocument
		{
			get
			{
				var collection = new Collection<IDeclarationConsignmentTransportContractDocument>();
				var i = 0;
				foreach (var transportDocument in asycudaBill.TransportDocuments)
				{
					++i;
					collection.Add(DeclarationConsignmentTransportContractDocumentWrapper.NewOrNull(transportDocument, i));
				}

				return collection;
			}
		}

		public bool? TransportSplitIndicator => null;

		public ICollection<IDeclarationConsignmentUNDangerousGoodsContact> UndgContact
		{
			get
			{
				var collection = new Collection<IDeclarationConsignmentUNDangerousGoodsContact>();

				foreach (var dg in asycudaBill.PackedItems.SelectMany(pi => pi.UNDGs.Where(dg => dg.DGContact != null)))
				{
					collection.Add(DeclarationConsignmentUndgContactWrapper.NewOrNull(dg.DGContact));
				}

				return collection;
			}
		}

		public ICollection<IDeclarationConsignmentUnloadingLocation> UnloadingLocation
			=> new Collection<IDeclarationConsignmentUnloadingLocation> { DeclarationConsignmentUnloadingLocationWrapper.NewOrNull(asycudaBill) };

		public IAmountType ValueAmount
			=> AmountTypeWrapper.NewOrNull(asycudaBill.ABL_CustomsValue, asycudaBill.ABL_RX_NKCustomsValueCurrency);

		readonly AsycudaBill asycudaBill;
	}
}
