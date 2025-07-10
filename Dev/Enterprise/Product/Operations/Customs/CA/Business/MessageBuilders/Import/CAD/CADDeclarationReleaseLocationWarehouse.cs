using CargoWise.Customs.CA.MessageContracts.CAD;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationReleaseLocationWarehouse : ICADMessageDeclarationReleaseLocationWarehouse
{
	public CADDeclarationReleaseLocationWarehouse(string id = "", string typeCode = "", string roleCode = "")
	{
		this.id = id;
		this.typeCode = typeCode;
		this.roleCode = roleCode;
	}

	readonly string id;
	readonly string typeCode;
	readonly string roleCode;

	#region ICADMessageDeclarationReleaseLocationWarehouse

	string ICADMessageDeclarationReleaseLocationWarehouse.ID => id;

	string ICADMessageDeclarationReleaseLocationWarehouse.TypeCode => typeCode;

	string ICADMessageDeclarationReleaseLocationWarehouse.RoleCode => roleCode;

	#endregion
}
