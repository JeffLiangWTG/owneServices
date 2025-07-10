using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaPackCollectionSynchroniser : ASYCUDA.Business.AsycudaPackCollectionSynchroniser
	{
		public AsycudaPackCollectionSynchroniser(ForwardingShipment source, AsycudaBill destination) : base(source, destination)
		{
		}

		protected override void SynchronisePacks(IEnumerable<PackLine> sourcePacks)
			=> OneTimeBillItemsSynchronise(sourcePacks);

		void OneTimeBillItemsSynchronise(IEnumerable<PackLine> sourcePackLinesToAdd)
		{
			var packedItems = Destination.PackedItems;
			if (packedItems.Count > 0)
			{
				return;
			}

			foreach (var packLine in sourcePackLinesToAdd)
			{
				var newPack = (AsycudaPack)Destination.Packs.AddNew();
				new AsycudaPackSynchroniser(newPack, packLine).Synchronise(true);

				var containerNumber = packLine.JL_Calc_ContainerNum;
				var asyCont = Destination.Header.Containers.OfType<AsycudaContainer>().FirstOrDefault(ac => ac.ACN_ContainerNumber == containerNumber);
				if (asyCont != null)
				{
					newPack.ContainerPK = asyCont.PK;
				}

				var newPackedItem = (AsycudaPackedItem)packedItems.AddNew();
				PackedItemSynchronizer(newPackedItem, packLine);

				var link = newPackedItem.AsycudaLinkPackages.FirstOrDefault(x => x.Package == newPack);
				if (link is null)
				{
					link = newPackedItem.AsycudaLinkPackages.AddNew();
					link.Package = newPack;
				}
				link.IsLinked = true;
			}
		}

		public void PackedItemSynchronizer(AsycudaPackedItem newPackedItem, PackLine packLine)
		{
			newPackedItem.API_FormattedTariff = packLine.JL_HarmonisedCode;
			newPackedItem.API_GrossWeight = packLine.JL_ActualWeight;
			newPackedItem.API_GrossWeightUQ = packLine.JL_ActualWeightUQ;
			newPackedItem.API_GoodsDescription = packLine.JL_Description.Left(AsycudaPackedItem.Schema.API_GoodsDescriptionMaxLength);

			foreach (var item in packLine.UNDGs)
			{
				var newUNDG = newPackedItem.UNDGs.AddNew();
				newUNDG.DI_DG = item.DI_DG;
				newUNDG.DI_IMOClass = item.DI_IMOClass;
				newUNDG.DI_DGFlashPoint = item.DI_DGFlashPoint;
				newUNDG.DI_TechnicalName = item.DI_TechnicalName;
				newUNDG.DI_OC_DGContact = item.DI_OC_DGContact;
			}
		}
	}
}
