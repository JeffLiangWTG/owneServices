using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper : IDeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacility
	{
		readonly JobDeclaration jobDeclaration;

		DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper NewOrNull(JobDeclaration jobDeclaration)
			=> jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper(jobDeclaration) : null;

		public ICodeType FacilityType => CodeTypeWrapper.NewOrNull("004");

		public IIDType ID => IDTypeWrapper.NewOrNull(jobDeclaration.JE_LocationOfGoods);
	}
}
