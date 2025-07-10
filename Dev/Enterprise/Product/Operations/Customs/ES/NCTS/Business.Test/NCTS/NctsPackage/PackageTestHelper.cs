using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	static class PackageTestHelper
	{
		public static NctsPackage GetNewDeparturePackageInNewItem(EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> goodsItemCollection, ZShort lineNo, ZString unitType, ZLong unitCount, ZString marksAndNumbers)
		{
			var goodsItem = goodsItemCollection.AddNew();
			goodsItem.BY_LineNo = lineNo;
			var package = goodsItem.Packages.AddNew();
			package.B5_UnitType = unitType;
			package.B5_MarksAndNumbers = marksAndNumbers;
			package.B5_UnitCount = unitCount;
			return package;
		}

		public static NctsPackage GetNewArrivalPackageInNewItem(EU.NCTS.Business.INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> goodsItemCollection, ZShort lineNo, ZString unitType, ZLong unitCount, ZString marksAndNumbers)
		{
			var goodsItem = goodsItemCollection.AddNew();
			goodsItem.BY_LineNo = lineNo;
			var package = goodsItem.Packages.AddNew();
			package.B5_UnitType = unitType;
			package.B5_MarksAndNumbers = marksAndNumbers;
			package.B5_UnitCount = unitCount;
			return package;
		}
	}
}
