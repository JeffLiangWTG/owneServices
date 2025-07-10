using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperContainer : IItemContainer
	{
		internal BLSendChileWrapperContainer(AsycudaContainer container, ZString weightUQ, UNDGDataItemCollection undgDataItems)
		{
			this.container = Argument.NotNull(container, nameof(container));
			undgs = Argument.NotNull(undgDataItems, nameof(undgDataItems));
			this.weightUQ = weightUQ;
		}
		readonly AsycudaContainer container;
		readonly UNDGDataItemCollection undgs;
		readonly ZString weightUQ;

		string IItemContainer.Initials => container.ACN_ContainerNumber.Left(4);

		string IItemContainer.Number => container.ACN_ContainerNumber.SubstringSafe(4, 6);

		string IItemContainer.CheckDigit => container.ACN_ContainerNumber.Right(1);

		string IItemContainer.IsoCode => container.ContainerType?.RC_Code ?? ZString.Empty;

		string IItemContainer.GoodsWeight => CLMessageHelper.WeightConvertion(weightUQ, container.ACN_GoodsWeight);

		string IItemContainer.ShipmentOwner => container.Header.ShippingAgent?.Header?.OH_FullName ?? container.Header.Carrier?.Header?.OH_FullName ?? ZString.Empty;

		string IItemContainer.Status => BLSendChileHelper.ContainerStatusCodeCalculator(container.ACN_EmptyFullIndicator);

		IReadOnlyCollection<IIMO> IItemContainer.ContainersIMO
		{
			get
			{
				var result = new List<IIMO>();

				foreach (UNDGDataItem undg in undgs)
				{
					result.Add(new BLSendChileWrapperIMO(undg));
				}

				return result.ToArray();
			}
		}

		IReadOnlyCollection<IItemContainerSeal> IItemContainer.ContainerSeals => new List<IItemContainerSeal>() { new BLSendChileWrapperContainerSeal(container.ACN_Seal1, container.ACN_SealingPartyType), new BLSendChileWrapperContainerSeal(container.ACN_Seal2, container.ACN_SealingPartyType2), new BLSendChileWrapperContainerSeal(container.ACN_Seal3, container.ACN_SealingPartyType3) };
	}
}
