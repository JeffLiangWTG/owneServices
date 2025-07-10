using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PackingGroup : BasePackingGroup, Integration.Customs.CH.IPackingGroup
{
	public PackingGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ZString GetConvertedPackType(ZString packType) => packType.IsEmpty ? packType : JobDeclarationHelper.GetTwoCharacterUnitType(packType);
}
