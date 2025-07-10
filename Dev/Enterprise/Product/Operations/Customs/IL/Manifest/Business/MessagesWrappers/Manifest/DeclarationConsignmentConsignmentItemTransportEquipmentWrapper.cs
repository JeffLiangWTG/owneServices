using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemTransportEquipmentWrapper : IDeclarationConsignmentConsignmentItemTransportEquipment
	{
		DeclarationConsignmentConsignmentItemTransportEquipmentWrapper(AsycudaContainer asycudaContainer)
		{
			this.asycudaContainer = Argument.NotNull(asycudaContainer, nameof(asycudaContainer));
		}

		public static IDeclarationConsignmentConsignmentItemTransportEquipment NewOrNull(AsycudaContainer asycudaContainer)
			=> asycudaContainer == null ? null : new DeclarationConsignmentConsignmentItemTransportEquipmentWrapper(asycudaContainer);

		public ICodeType CharacteristicCode => CodeTypeWrapper.NewOrNull(asycudaContainer.ContainerType?.RC_ISOType.ToString());

		public ICodeType FullnessCode => CodeTypeWrapper.NewOrNull(asycudaContainer.ACN_EmptyFullIndicator.ToString() switch
		{
			EmptyFullIndicatorList.Codes.EmptyContainer => Constants.MessagesWrappers.DeclarationConsignmentConsignmentItemTransportEquipment.FullnessCode4,
			_ => Constants.MessagesWrappers.DeclarationConsignmentConsignmentItemTransportEquipment.FullnessCode5
		});

		public IIDType Id => IDTypeWrapper.NewOrNull(asycudaContainer.ACN_ContainerNumber);

		public ICollection<IDeclarationConsignmentConsignmentItemTransportEquipmentSeal> Seal
		{
			get
			{
				var list = new List<IDeclarationConsignmentConsignmentItemTransportEquipmentSeal>();

				if (!asycudaContainer.ACN_Seal1.IsEmpty)
				{
					list.Add(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(asycudaContainer.Factory, asycudaContainer.ACN_Seal1, asycudaContainer.ACN_Seal1UnloadingState, asycudaContainer.ACN_SealType1, asycudaContainer.ACN_SealingPartyType));
				}
				if (!asycudaContainer.ACN_Seal2.IsEmpty)
				{
					list.Add(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(asycudaContainer.Factory, asycudaContainer.ACN_Seal2, asycudaContainer.ACN_Seal2UnloadingState, asycudaContainer.ACN_SealType2, asycudaContainer.ACN_SealingPartyType2));
				}
				if (!asycudaContainer.ACN_Seal3.IsEmpty)
				{
					list.Add(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(asycudaContainer.Factory, asycudaContainer.ACN_Seal3, asycudaContainer.ACN_Seal3UnloadingState, asycudaContainer.ACN_SealType3, asycudaContainer.ACN_SealingPartyType3));
				}

				list.AddRange(asycudaContainer.AdditionalSeals
					.Where(s => !s.BK_SealNumber.IsEmpty)
					.Select(s => DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper
						.NewOrNull(s.Factory, s.BK_SealNumber, s.BK_UnloadingState, s.BK_SealType, s.BK_SealingPartyType)));

				return new Collection<IDeclarationConsignmentConsignmentItemTransportEquipmentSeal>(list.ToArray());
			}
		}

		public ICodeType EventStatusCode => null;

		public bool? LegalStatusIndicator => null;

		public ICodeType LicensePlateIssuingCountryCode => null;

		public IIDType SealId => null;

		public ICodeType SupplierPartyTypeCode => null;

		readonly AsycudaContainer asycudaContainer;
	}
}
