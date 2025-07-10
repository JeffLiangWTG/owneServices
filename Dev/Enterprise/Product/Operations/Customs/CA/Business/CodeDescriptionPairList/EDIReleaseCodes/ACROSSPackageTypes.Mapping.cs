using CargoWise.Types;
using Enterprise.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class ACROSSPackageTypes : CodeDescriptionPairList
	{
		public static ZString FromFreightPackageType(ZString freightPackageType)
		{
			switch (freightPackageType)
			{
				case Constants.PkgUnit.Bag:
					return Codes.BAG;
				case Constants.PkgUnit.BaleCompressed:
				case Constants.PkgUnit.BaleUncompressed:
					return Codes.BALEBAL;
				case Constants.PkgUnit.Basket:
					return Codes.BASKETORHAMPER;
				case Constants.PkgUnit.Bottle:
					return Codes.BOTTLE;
				case Constants.PkgUnit.Box:
					return Codes.BOX;
				case Constants.PkgUnit.BreakBulk:
					return Codes.UNIT;
				case Constants.PkgUnit.BulkBag:
					return Codes.BULKBAG;
				case Constants.PkgUnit.Bundle:
					return Codes.BUNDLE;
				case Constants.PkgUnit.Carton:
					return Codes.CARTON;
				case Constants.PkgUnit.Case:
					return Codes.CASE;
				case Constants.PkgUnit.Coil:
					return Codes.COIL;
				case Constants.PkgUnit.Container:
					return Codes.CONTAINER;
				case Constants.PkgUnit.Cradle:
					return Codes.CRADLE;
				case Constants.PkgUnit.Crate:
					return Codes.CRATE;
				case Constants.PkgUnit.Cylinder:
					return Codes.CYLINDER;
				case Constants.PkgUnit.Drum:
					return Codes.DRUM;
				case Constants.PkgUnit.Envelope:
					return Codes.ENVELOPE;
				case Constants.PkgUnit.Keg:
					return Codes.KEG;
				case Constants.PkgUnit.Mix:
					return Codes.MIXEDTYPEPACK;
				case Constants.PkgUnit.Package:
					return Codes.PACKAGE;
				case Constants.PkgUnit.Pail:
					return Codes.PAIL;
				case Constants.PkgUnit.Pallet:
					return Codes.PALLET;
				case Constants.PkgUnit.Piece:
					return Codes.PIECE;
				case Constants.PkgUnit.Reel:
					return Codes.REEL;
				case Constants.PkgUnit.Roll:
					return Codes.ROLL;
				case Constants.PkgUnit.Sheet:
					return Codes.SHEET;
				case Constants.PkgUnit.Skid:
					return Codes.SKID;
				case Constants.PkgUnit.Spool:
					return Codes.SPOOL;
				case Constants.PkgUnit.Tube:
					return Codes.TUBE;
				case Constants.PkgUnit.Unit:
					return Codes.UNIT;
				default:
					return freightPackageType;
			}
		}

		public static ZString ToFreightPackageType(ZString customsPackageType)
		{
			switch (customsPackageType)
			{
				case Codes.BAG:
					return Constants.PkgUnit.Bag;
				case Codes.BALEBAL:
				case Codes.BALEBLE:
					return Constants.PkgUnit.BaleUncompressed;
				case Codes.BARREL:
					return Constants.PkgUnit.Drum;
				case Codes.BASKETORHAMPER:
					return Constants.PkgUnit.Basket;
				case Codes.BOTTLE:
					return Constants.PkgUnit.Bottle;
				case Codes.BOX:
				case Codes.BOXWITHINNERCONTAINER:
					return Constants.PkgUnit.Box;
				case Codes.BULKBAG:
					return Constants.PkgUnit.BulkBag;
				case Codes.BUNDLE:
					return Constants.PkgUnit.Bundle;
				case Codes.CANCASE:
					return Constants.PkgUnit.Case;
				case Codes.CARTON:
					return Constants.PkgUnit.Carton;
				case Codes.CASE:
					return Constants.PkgUnit.Case;
				case Codes.COIL:
					return Constants.PkgUnit.Coil;
				case Codes.COMMERCIALHIGHWAYLIFTCONTAINER:
				case Codes.CONTAINER:
				case Codes.CONTAINEREXPRESS:
				case Codes.CONTAINERSOFBULKCARGO:
					return Constants.PkgUnit.Container;
				case Codes.CRADLE:
					return Constants.PkgUnit.Cradle;
				case Codes.CRATE:
					return Constants.PkgUnit.Crate;
				case Codes.CYLINDER:
					return Constants.PkgUnit.Cylinder;
				case Codes.DOUBLELENGTHSKID:
					return Constants.PkgUnit.Skid;
				case Codes.DRUM:
					return Constants.PkgUnit.Drum;
				case Codes.EGGCRATING:
					return Constants.PkgUnit.Crate;
				case Codes.ENGINECONTAINER:
					return Constants.PkgUnit.Container;
				case Codes.ENVELOPE:
					return Constants.PkgUnit.Envelope;
				case Codes.FORWARDREEL:
					return Constants.PkgUnit.Reel;
				case Codes.HOUSEHOLDGOODCONTAINERWOOD:
					return Constants.PkgUnit.Container;
				case Codes.KEG:
					return Constants.PkgUnit.Keg;
				case Codes.MILITARYAIRCRAFTCONTAINER:
					return Constants.PkgUnit.Container;
				case Codes.MIXEDCONTAINERTYPES:
				case Codes.MIXEDTYPEPACK:
					return Constants.PkgUnit.Mix;
				case Codes.MULTIROLLPACK:
					return Constants.PkgUnit.Roll;
				case Codes.MULTIWALLSECUREDWAREHOUSEPALLET:
					return Constants.PkgUnit.Pallet;
				case Codes.NAVYCARGOTRANSPORTCONTAINER:
					return Constants.PkgUnit.Container;
				case Codes.PACKAGE:
				case Codes.PACKEDNOTOTHERWISESPECIFIED:
					return Constants.PkgUnit.Package;
				case Codes.PAIL:
					return Constants.PkgUnit.Pail;
				case Codes.PALLET:
				case Codes.PALLET2WAY:
				case Codes.PALLET4WAY:
					return Constants.PkgUnit.Pallet;
				case Codes.PIECE:
					return Constants.PkgUnit.Piece;
				case Codes.PIECES:
					return Constants.PkgUnit.Unit;
				case Codes.PRIMARYLIFTCONTAINER:
					return Constants.PkgUnit.Container;
				case Codes.REEL:
					return Constants.PkgUnit.Reel;
				case Codes.REVERSEREEL:
					return Constants.PkgUnit.Reel;
				case Codes.ROLL:
					return Constants.PkgUnit.Roll;
				case Codes.SHEET:
					return Constants.PkgUnit.Sheet;
				case Codes.SKID:
				case Codes.SKIDELEVATINGOFLIFTTRUCK:
					return Constants.PkgUnit.Skid;
				case Codes.SLIPSHEET:
					return Constants.PkgUnit.Sheet;
				case Codes.SPOOL:
					return Constants.PkgUnit.Spool;
				case Codes.TRAILERCONTAINERLOADRAIL:
					return Constants.PkgUnit.Container;
				case Codes.TUBE:
					return Constants.PkgUnit.Tube;
				case Codes.UNIT:
					return Constants.PkgUnit.Unit;
				default:
					return customsPackageType;
			}
		}
	}
}
