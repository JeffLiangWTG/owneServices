using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Message.MessagesWrappers.Declaration
{
	public class DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper : IDeclarationGoodsShipmentConsignmentUnloadingLocation
	{
		readonly JobDeclaration jobDeclaration;

		DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper NewOrNull(JobDeclaration jobDeclaration)
			=> jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper(jobDeclaration) : null;

		public string ArrivalDateTime => jobDeclaration.JE_DateOfArrival.IsEmpty ? null : jobDeclaration.JE_DateOfArrival.ToCustomsDateTimeString();

		public IIDType ID => IDTypeWrapper.NewOrNull(jobDeclaration.JE_CustomsDischargePort.IsEmpty ? jobDeclaration.JE_RL_NKPortOfArrival : jobDeclaration.JE_CustomsDischargePort);
	}
}
