using System;
using CargoWise.Customs.AR.MessageContracts;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	class BLArgentinaWrapperContainer : IContainer
	{
		internal BLArgentinaWrapperContainer(AsycudaContainer container)
		{
			this.container = CargoWise.Common.Argument.NotNull(container, "AsycudaContainer cannot be null");
		}
		readonly AsycudaContainer container;

		string IContainer.ContainerNumber => container.ACN_ContainerNumber;

		string IContainer.ContainerCondition => container.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.EmptyContainer ?
			ARMessageConstants.EmptyContainerCondition : ARMessageConstants.NotEmptyContainerCondition;

		string IContainer.Description => container.ContainerType?.RC_Code ?? string.Empty;

		decimal IContainer.GrossWeight => container.ACN_GoodsWeight;

		string IContainer.SealNumber => container.ACN_Seal1;

		DateTime IContainer.ContainerExpireDate => container.ACN_ExpireDate.ToDateTime();

		string IContainer.ACEP => container.ACN_ACEP;
	}
}
