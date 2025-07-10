using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperContainerSeal : IItemContainerSeal
	{
		internal BLSendChileWrapperContainerSeal(ZString sealNumber, ZString sealPartyType)
		{
			Number = sealNumber;
			this.sealPartyType = sealPartyType;
		}
		readonly ZString sealPartyType;

		public string Number { get; }

		string IItemContainerSeal.Code
		{
			get
			{
				var sealCode = ZString.Empty;
				switch (sealPartyType)
				{
					case Core.Constants.ContainerSealParties.Codes.CarrierShippingLine:
						sealCode = ContainerSealParties.CarrierShippingLine.Code;
						break;
					case Core.Constants.ContainerSealParties.Codes.ConsignorShipper:
						sealCode = ContainerSealParties.ConsignorShipper.Code;
						break;
					case Core.Constants.ContainerSealParties.Codes.Customs:
						sealCode = ContainerSealParties.Customs.Code;
						break;
					case Core.Constants.ContainerSealParties.Codes.Terminal:
						sealCode = ContainerSealParties.Terminal.Code;
						break;
				}
				return sealCode;
			}
		}

		string IItemContainerSeal.PartyName
		{
			get
			{
				var sealName = ZString.Empty;
				switch (sealPartyType)
				{
					case Core.Constants.ContainerSealParties.Codes.CarrierShippingLine:
						sealName = ContainerSealParties.CarrierShippingLine.Name;
						break;
					case Core.Constants.ContainerSealParties.Codes.ConsignorShipper:
						sealName = ContainerSealParties.ConsignorShipper.Name;
						break;
					case Core.Constants.ContainerSealParties.Codes.Customs:
						sealName = ContainerSealParties.Customs.Name;
						break;
					case Core.Constants.ContainerSealParties.Codes.Terminal:
						sealName = ContainerSealParties.Terminal.Name;
						break;
				}
				return sealName;
			}
		}
	}
}
