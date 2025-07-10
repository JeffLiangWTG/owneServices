using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentGovernmentAgencyGoodsItemWrapper : IDeclarationConsignmentGovernmentAgencyGoodsItem
	{
		DeclarationConsignmentGovernmentAgencyGoodsItemWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentGovernmentAgencyGoodsItemWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentGovernmentAgencyGoodsItemWrapper(asycudaBill) : null;

		public ICollection<IDeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformation> AdditionalInformation
		{
			get
			{
				var collection = new Collection<IDeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformation>();
				if (asycudaBill.Header.IsRoad)
				{
					collection.Add(DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper.NewOrNull(ZString.Empty, IL.Manifest.Business.Constants.AdditionalInformation.RoadStatementCodeContainerizedCargo, IL.Manifest.Business.Constants.AdditionalInformation.RoadStatementTypeCodeContainerizedCargo));
				}

				foreach (var asycudaAdditionalInfo in asycudaBill.AdditionalInfos)
				{
					collection.Add(DeclarationConsignmentGovernmentAgencyGoodsItemAdditionalInformationWrapper.NewOrNull(
						asycudaAdditionalInfo.CSI_Description,
						asycudaAdditionalInfo.CSI_ReferenceNumber,
						asycudaAdditionalInfo.CSI_Code
						));
				}

				return collection;
			}
		}

		readonly AsycudaBill asycudaBill;
	}
}
