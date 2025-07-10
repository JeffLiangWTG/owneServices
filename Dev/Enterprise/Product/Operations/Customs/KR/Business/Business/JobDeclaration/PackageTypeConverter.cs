using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class PackageTypeConverter
	{
		public static ZString GetCustomsPackageType(ZString packageType)
		{
			var result = ZString.Empty;
			switch (packageType)
			{
				case Constants.CW1PackType.Barrels:
					result = PackageKindCodeList.Codes.BA;
					break;

				case PkgUnit.Bundle:
					result = PackageKindCodeList.Codes.BE;
					break;

				case PkgUnit.Bag:
					result = PackageKindCodeList.Codes.BG;
					break;

				case PkgUnit.BaleCompressed:
					result = PackageKindCodeList.Codes.BL;
					break;

				case PkgUnit.Bottle:
					result = PackageKindCodeList.Codes.BV;
					break;

				case Constants.CW1PackType.Can:
					result = PackageKindCodeList.Codes.CA;
					break;

				case PkgUnit.Coil:
					result = PackageKindCodeList.Codes.CL;
					break;

				case PkgUnit.Crate:
				case Constants.CW1PackType.PlywoodBox:
					result = PackageKindCodeList.Codes.CR;
					break;

				case PkgUnit.Case:
					result = PackageKindCodeList.Codes.CS;
					break;

				case PkgUnit.Box:
				case Constants.CW1PackType.Carton:
				case PkgUnit.Container:
				case PkgUnit.Dozen:
					result = PackageKindCodeList.Codes.CT;
					break;

				case PkgUnit.Drum:
					result = PackageKindCodeList.Codes.DR;
					break;

				case PkgUnit.Sheet:
				case PkgUnit.Unit:
				case Constants.CW1PackType.Each:
				case PkgUnit.Piece:
				case Constants.CW1PackType.Pieces:
					result = PackageKindCodeList.Codes.GT;
					break;

				case Constants.CW1PackType.Tray:
					result = PackageKindCodeList.Codes.PU;
					break;

				case PkgUnit.Reel:
					result = PackageKindCodeList.Codes.RL;
					break;

				case PkgUnit.Roll:
					result = PackageKindCodeList.Codes.RO;
					break;

				case PkgUnit.BulkBag:
					result = PackageKindCodeList.Codes.VO;
					break;

				case PkgUnit.BreakBulk:
					result = PackageKindCodeList.Codes.VT;
					break;
			}
			return result;
		}
	}
}

