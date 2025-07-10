using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper : IDeclarationConsignmentConsignmentItemTransportEquipmentSeal
	{
		public DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper(BusinessObjectFactory factory, string seal, string unloadingState, string sealType, string sealPartyType)
		{
			this.seal = Argument.NotNull(seal, nameof(seal));
			this.factory = factory;
			this.unloadingState = unloadingState;
			this.sealType = sealType;
			this.sealPartyName = sealPartyType;
		}

		public static IDeclarationConsignmentConsignmentItemTransportEquipmentSeal NewOrNull(BusinessObjectFactory factory, string seal, string unloadingState, string sealType, string sealPartyType)
			=> (factory == null || seal.IsNullOrEmpty()) ? null : new DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper(factory, seal, unloadingState, sealType, sealPartyType);

		public ISealID Id => SealIDWrapper.NewOrNull(seal);

		public int? ConditionCode => MapConditionCode(unloadingState);

		public int? TypeCode => MapTypeCode(sealType, sealPartyName);

		int? MapConditionCode(string unloadingState)
		{
			var cw1Code = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusMaps.ILSealUnloadingState, unloadingState, ZDateTime.Now);

			return int.TryParse(cw1Code, out var code) ? code : null;
		}

		int? MapTypeCode(string sealType, string sealPartyName)
		{
			switch (sealType)
			{
				case SealTypeList.Codes.ElectronicSeal:
					switch (sealPartyName)
					{
						case Core.Constants.ContainerSealParties.Codes.CarrierShippingLine:
						case Core.Constants.ContainerSealParties.Codes.ConsignorShipper:
						case Core.Constants.ContainerSealParties.Codes.Quarantine:
						case Core.Constants.ContainerSealParties.Codes.Terminal:
							return 6;
						case Core.Constants.ContainerSealParties.Codes.Customs:
							return 2;
					}
					break;
				case SealTypeList.Codes.MechanicalSeal:
					switch (sealPartyName)
					{
						case Core.Constants.ContainerSealParties.Codes.CarrierShippingLine:
						case Core.Constants.ContainerSealParties.Codes.ConsignorShipper:
						case Core.Constants.ContainerSealParties.Codes.Quarantine:
						case Core.Constants.ContainerSealParties.Codes.Terminal:
							return 5;
						case Core.Constants.ContainerSealParties.Codes.Customs:
							return 1;
					}
					break;
				case ILSealTypeList.Codes.RfidSeal:
					switch (sealPartyName)
					{
						case Core.Constants.ContainerSealParties.Codes.CarrierShippingLine:
						case Core.Constants.ContainerSealParties.Codes.ConsignorShipper:
						case Core.Constants.ContainerSealParties.Codes.Quarantine:
						case Core.Constants.ContainerSealParties.Codes.Terminal:
							return 7;
						case Core.Constants.ContainerSealParties.Codes.Customs:
							return 3;
					}
					break;
			}

			return null;
		}

		readonly string seal;
		readonly BusinessObjectFactory factory;
		readonly string unloadingState;
		readonly string sealType;
		readonly string sealPartyName;
	}
}
