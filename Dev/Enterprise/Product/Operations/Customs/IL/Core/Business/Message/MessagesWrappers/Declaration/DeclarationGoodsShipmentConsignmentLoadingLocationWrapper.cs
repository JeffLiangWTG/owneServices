using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentConsignmentLoadingLocationWrapper : IDeclarationGoodsShipmentConsignmentLoadingLocation
	{
		readonly JobDeclaration jobDeclaration;

		DeclarationGoodsShipmentConsignmentLoadingLocationWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentLoadingLocationWrapper NewOrNull(JobDeclaration jobDeclaration)
			=> jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentLoadingLocationWrapper(jobDeclaration) : null;

		public IIDType ID => IDTypeWrapper.NewOrNull(jobDeclaration.JE_RL_NKPortOfLoading);
	}
}
