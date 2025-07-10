using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public partial class PackageType
	{
		public static ZString GetMappedPackageType(string pkgUnit)
		{
			ZString result;
			switch (pkgUnit)
			{
				case Core.Constants.PkgUnit.BreakBulk:
					result = Codes.Bulk;
					break;
				case Core.Constants.PkgUnit.Coil:
				case Core.Constants.PkgUnit.Gross:
					result = Codes.Naked;
					break;
				case Core.Constants.PkgUnit.Bag:
				case Core.Constants.PkgUnit.BulkBag:
					result = Codes.Bag;
					break;
				case Core.Constants.PkgUnit.Box:
				case Core.Constants.PkgUnit.Case:
				case Core.Constants.PkgUnit.Carton:
					result = Codes.PaperBox;
					break;
				case Core.Constants.PkgUnit.Crate:
					result = Codes.WoodBox;
					break;
				case Core.Constants.PkgUnit.Keg:
				case Core.Constants.PkgUnit.Pail:
					result = Codes.PaperBarrel;
					break;
				case Core.Constants.PkgUnit.Pallet:
				case Core.Constants.PkgUnit.Skid:
					result = Codes.RecycledWood;
					break;
				default:
					result = Codes.Other;
					break;
			}
			return result;
		}
	}
}
