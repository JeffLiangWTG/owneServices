using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;
using Enterprise.Customs.Common;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper : IDeclarationGoodsShipmentConsignmentTransportContractDocumentDMExt
	{
		DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper NewOrNull(JobDeclaration jobDeclaration)
			=> jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper(jobDeclaration) : null;

		public IIDType SecondCargoId => IDTypeWrapper.NewOrNull(GetSecondCargoIDBasedOnTransportMode());

		public IIDType ThirdCargoId => IDTypeWrapper.NewOrNull(GetThirdCargoIDBasedOnTransportMode());

		ZString GetSecondCargoIDBasedOnTransportMode()
			=> jobDeclaration?.JE_TransportMode.ToString() switch
			{
				TransportModes.Air => jobDeclaration.JE_MasterBill,
				TransportModes.Sea => CusEntryNumber.Load(jobDeclaration, IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, CountryCodes.Israel)?.CE_EntryNum ?? ZString.Empty,
				_ => ZString.Empty
			};

		ZString GetThirdCargoIDBasedOnTransportMode()
			=> jobDeclaration?.JE_TransportMode.ToString() switch
			{
				TransportModes.Air => jobDeclaration.JE_HouseBill.KeepNumericCharacters(),
				_ => ZString.Empty
			};

		readonly JobDeclaration jobDeclaration;
	}
}
