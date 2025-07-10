using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

static class PackageTestHelper
{
	public static NctsPackage GetNewPackageInNewItem(NctsDepartureCargoDescCollection goodsItemCollection, ZShort lineNo, ZString unitType, ZLong unitCount, ZString marksAndNumbers)
	{
		var goodsItem = goodsItemCollection.AddNew();
		goodsItem.BY_LineNo = lineNo;
		var package = (NctsPackage)goodsItem.Packages.AddNew();
		package.B5_UnitType = unitType;
		package.B5_MarksAndNumbers = marksAndNumbers;
		package.B5_UnitCount = unitCount;
		return package;
	}
}
