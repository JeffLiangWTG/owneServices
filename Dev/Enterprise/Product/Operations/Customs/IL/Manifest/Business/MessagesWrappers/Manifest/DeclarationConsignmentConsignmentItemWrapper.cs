using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemWrapper : IDeclarationConsignmentConsignmentItem
	{
		DeclarationConsignmentConsignmentItemWrapper(AsycudaPackedItem asycudaPackedItem)
		{
			this.asycudaPackedItem = Argument.NotNull(asycudaPackedItem, nameof(asycudaPackedItem));
		}

		public static IDeclarationConsignmentConsignmentItem NewOrNull(AsycudaPackedItem asycudaPackedItem)
			=> asycudaPackedItem == null ? null : new DeclarationConsignmentConsignmentItemWrapper(asycudaPackedItem);

		public decimal? SequenceNumeric => asycudaPackedItem.API_LineNo;

		public ICodeType GoodsStatusCode => CodeTypeWrapper.NewOrNull(asycudaPackedItem.API_PackStatus == ILPackStatusList.Codes.D ? ILPackStatusList.Codes.D : ILPackStatusList.Codes.N);

		public ICollection<IDeclarationConsignmentConsignmentItemAdditionalInformation> AdditionalInformation
		{
			get
			{
				var result = new Collection<IDeclarationConsignmentConsignmentItemAdditionalInformation>();

				foreach (var additionalInfo in asycudaPackedItem.AdditionalInfos)
				{
					result.Add(DeclarationConsignmentConsignmentItemAdditionalInformationWrapper.NewOrNull(additionalInfo));
				}

				return result;
			}
		}

		public ICollection<IDeclarationConsignmentConsignmentItemCommodity> Commodity
			=> new Collection<IDeclarationConsignmentConsignmentItemCommodity>() { DeclarationConsignmentConsignmentItemCommodityWrapper.NewOrNull(asycudaPackedItem) };

		public ICollection<IDeclarationConsignmentConsignmentItemGoodsMeasure> GoodsMeasure
		{
			get
			{
				var result = new Collection<IDeclarationConsignmentConsignmentItemGoodsMeasure>();

				asycudaPackedItem.PackagesPivot.Cast<AsycudaPackPackedItemPivot>().ForEach(pivot =>
				{
					result.Add(DeclarationConsignmentConsignmentItemGoodsMeasureWrapper.NewOrNull((AsycudaPack)pivot.Pack));
				});

				return result;
			}
		}

		public ICollection<IDeclarationConsignmentConsignmentItemGovernmentProcedure> GovernmentProcedure
			=> new Collection<IDeclarationConsignmentConsignmentItemGovernmentProcedure>() { DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper.NewOrNull((AsycudaManifestHeader)asycudaPackedItem.Header) };

		public ICollection<IDeclarationConsignmentConsignmentItemPackaging> Packaging
		{
			get
			{
				var result = new Collection<IDeclarationConsignmentConsignmentItemPackaging>();

				asycudaPackedItem.PackagesPivot.Cast<AsycudaPackPackedItemPivot>().ForEach(pivot =>
				{
					result.Add(DeclarationConsignmentConsignmentItemPackagingWrapper.NewOrNull((AsycudaPack)pivot.Pack));
				});

				return result;
			}
		}

		public ICollection<IDeclarationConsignmentConsignmentItemTransportEquipment> TransportEquipment
		{
			get
			{
				var result = new Collection<IDeclarationConsignmentConsignmentItemTransportEquipment>();

				asycudaPackedItem.PackagesPivot.Cast<AsycudaPackPackedItemPivot>().ForEach(pivot =>
				{
					if (pivot.Pack.Container != null)
					{
						result.Add(DeclarationConsignmentConsignmentItemTransportEquipmentWrapper.NewOrNull((AsycudaContainer)pivot.Pack.Container));
					}
				});

				return result;
			}
		}

		public ICollection<IDeclarationConsignmentConsignmentItemUcr> Ucr => new List<IDeclarationConsignmentConsignmentItemUcr>().AsReadOnly();

		readonly AsycudaPackedItem asycudaPackedItem;
	}
}
