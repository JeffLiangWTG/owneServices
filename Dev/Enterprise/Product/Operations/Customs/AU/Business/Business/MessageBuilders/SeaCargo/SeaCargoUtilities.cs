using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal static class SeaCargoUtilities
	{
		internal static ZString ConvertPkgUnitToCMRPackageType(ZString code)
		{
			ZString result = ZString.Empty;

			switch (code)
			{
				case Core.Constants.PkgUnit.Bag:
					result = CMRPackageTypes.Codes.Bags;
					break;
				case Core.Constants.PkgUnit.BaleCompressed:
					result = CMRPackageTypes.Codes.BaleCompressed;
					break;
				case Core.Constants.PkgUnit.Basket:
					result = CMRPackageTypes.Codes.Basket;
					break;
				case Core.Constants.PkgUnit.Bottle:
					result = CMRPackageTypes.Codes.BottleNonProtectedCylinder;
					break;
				case Core.Constants.PkgUnit.Box:
					result = CMRPackageTypes.Codes.Box;
					break;
				case Core.Constants.PkgUnit.BaleUncompressed:
					result = CMRPackageTypes.Codes.Bulk;
					break;
				case Core.Constants.PkgUnit.Bundle:
					result = CMRPackageTypes.Codes.Bundle;
					break;
				case Core.Constants.PkgUnit.Carton:
					result = CMRPackageTypes.Codes.Carton;
					break;
				case Core.Constants.PkgUnit.Case:
					result = CMRPackageTypes.Codes.Case;
					break;
				case Core.Constants.PkgUnit.Coil:
					result = CMRPackageTypes.Codes.Coil;
					break;
				case Core.Constants.PkgUnit.Crate:
					result = CMRPackageTypes.Codes.Crate;
					break;
				case Core.Constants.PkgUnit.Cylinder:
					result = CMRPackageTypes.Codes.Cylinder;
					break;
				case Core.Constants.PkgUnit.Drum:
					result = CMRPackageTypes.Codes.Drum;
					break;
				case Core.Constants.PkgUnit.Envelope:
					result = CMRPackageTypes.Codes.Envelope;
					break;
				case Core.Constants.PkgUnit.Keg:
					result = CMRPackageTypes.Codes.Keg;
					break;
				case Core.Constants.PkgUnit.Package:
					result = CMRPackageTypes.Codes.Package;
					break;
				case Core.Constants.PkgUnit.Pail:
					result = CMRPackageTypes.Codes.Pail;
					break;
				case Core.Constants.PkgUnit.Pallet:
					result = CMRPackageTypes.Codes.PalletLift;
					break;
				case Core.Constants.PkgUnit.Reel:
					result = CMRPackageTypes.Codes.Reel;
					break;
				case Core.Constants.PkgUnit.Roll:
					result = CMRPackageTypes.Codes.Roll;
					break;
				case Core.Constants.PkgUnit.Sheet:
					result = CMRPackageTypes.Codes.Sheet;
					break;
				case Core.Constants.PkgUnit.Tote:
					result = CMRPackageTypes.Codes.Tote;
					break;
				case Core.Constants.PkgUnit.Tube:
					result = CMRPackageTypes.Codes.Tube;
					break;
				case Core.Constants.PkgUnit.BreakBulk:
				case Core.Constants.PkgUnit.BulkBag:
				case Core.Constants.PkgUnit.Container:
				case Core.Constants.PkgUnit.Cradle:
				case Core.Constants.PkgUnit.Dozen:
				case Core.Constants.PkgUnit.Gross:
				case Core.Constants.PkgUnit.Mix:
				case Core.Constants.PkgUnit.Piece:
				case Core.Constants.PkgUnit.RollOnRollOff:
				case Core.Constants.PkgUnit.Skid:
				case Core.Constants.PkgUnit.Spool:
				case Core.Constants.PkgUnit.Unit:
					result = CMRPackageTypes.Codes.UnpackedOrPacked;
					break;
			}

			return result;
		}

		internal static ZString ConvertCMRPackageTypeToPkgUnit(ZString code)
		{
			ZString result = ZString.Empty;

			switch (code)
			{
				case CMRPackageTypes.Codes.Bags:
				case CMRPackageTypes.Codes.Jutebag:
				case CMRPackageTypes.Codes.MultiplyBag:
				case CMRPackageTypes.Codes.MultiwallSack:
				case CMRPackageTypes.Codes.Pouch:
				case CMRPackageTypes.Codes.Sack:
					result = Core.Constants.PkgUnit.Bag;
					break;
				case CMRPackageTypes.Codes.BaleCompressed:
					result = Core.Constants.PkgUnit.BaleCompressed;
					break;
				case CMRPackageTypes.Codes.Basket:
					result = Core.Constants.PkgUnit.Basket;
					break;
				case CMRPackageTypes.Codes.BottleNonProtectedBolbous:
				case CMRPackageTypes.Codes.BottleNonProtectedCylinder:
				case CMRPackageTypes.Codes.GasBottle:
				case CMRPackageTypes.Codes.Wickerbottle:
					result = Core.Constants.PkgUnit.Bottle;
					break;
				case CMRPackageTypes.Codes.BaseBox:
				case CMRPackageTypes.Codes.Box:
				case CMRPackageTypes.Codes.Matchbox:
					result = Core.Constants.PkgUnit.Box;
					break;
				case CMRPackageTypes.Codes.Bin:
				case CMRPackageTypes.Codes.Board:
				case CMRPackageTypes.Codes.Girder:
				case CMRPackageTypes.Codes.Ingot:
				case CMRPackageTypes.Codes.Log:
				case CMRPackageTypes.Codes.Pipe:
				case CMRPackageTypes.Codes.Plank:
				case CMRPackageTypes.Codes.Rod:
					result = Core.Constants.PkgUnit.BreakBulk;
					break;
				case CMRPackageTypes.Codes.Bundle:
				case CMRPackageTypes.Codes.GirderInBundleBunchTruss:
				case CMRPackageTypes.Codes.IngotInBundleBunchTruss:
				case CMRPackageTypes.Codes.LogInBundleBunchTruss:
				case CMRPackageTypes.Codes.PipesPlusPlanksInBundleBunchTruss:
				case CMRPackageTypes.Codes.PlatesInBundleBunchTruss:
				case CMRPackageTypes.Codes.RodsInBundleBunchTruss:
				case CMRPackageTypes.Codes.SheetsInBundleBunchTruss:
				case CMRPackageTypes.Codes.TubesInBundlesBunchTruss:
					result = Core.Constants.PkgUnit.Bundle;
					break;
				case CMRPackageTypes.Codes.Carton:
					result = Core.Constants.PkgUnit.Carton;
					break;
				case CMRPackageTypes.Codes.Case:
					result = Core.Constants.PkgUnit.Case;
					break;
				case CMRPackageTypes.Codes.Coil:
					result = Core.Constants.PkgUnit.Coil;
					break;
				case CMRPackageTypes.Codes.BeerCrate:
				case CMRPackageTypes.Codes.Crate:
				case CMRPackageTypes.Codes.FramedCrate:
				case CMRPackageTypes.Codes.FruitCrate:
				case CMRPackageTypes.Codes.MilkCrate:
				case CMRPackageTypes.Codes.ShallowCrate:
					result = Core.Constants.PkgUnit.Crate;
					break;
				case CMRPackageTypes.Codes.CanCylindrical:
				case CMRPackageTypes.Codes.Cylinder:
				case CMRPackageTypes.Codes.TankCylindrical:
					result = Core.Constants.PkgUnit.Cylinder;
					break;
				case CMRPackageTypes.Codes.Drum:
					result = Core.Constants.PkgUnit.Drum;
					break;
				case CMRPackageTypes.Codes.Envelope:
					result = Core.Constants.PkgUnit.Envelope;
					break;
				case CMRPackageTypes.Codes.GrossBarrel:
					result = Core.Constants.PkgUnit.Gross;
					break;
				case CMRPackageTypes.Codes.Keg:
					result = Core.Constants.PkgUnit.Keg;
					break;
				case CMRPackageTypes.Codes.Package:
					result = Core.Constants.PkgUnit.Package;
					break;
				case CMRPackageTypes.Codes.Pail:
					result = Core.Constants.PkgUnit.Pail;
					break;
				case CMRPackageTypes.Codes.PalletLift:
					result = Core.Constants.PkgUnit.Pallet;
					break;
				case CMRPackageTypes.Codes.Reel:
					result = Core.Constants.PkgUnit.Reel;
					break;
				case CMRPackageTypes.Codes.Roll:
					result = Core.Constants.PkgUnit.Roll;
					break;
				case CMRPackageTypes.Codes.Sheet:
				case CMRPackageTypes.Codes.Sheetmetal:
				case CMRPackageTypes.Codes.Slipsheet:
					result = Core.Constants.PkgUnit.Sheet;
					break;
				case CMRPackageTypes.Codes.Spindle:
					result = Core.Constants.PkgUnit.Spool;
					break;
				case CMRPackageTypes.Codes.Tote:
					result = Core.Constants.PkgUnit.Tote;
					break;
				case CMRPackageTypes.Codes.CollapsibleTube:
				case CMRPackageTypes.Codes.Tube:
					result = Core.Constants.PkgUnit.Tube;
					break;
				case CMRPackageTypes.Codes.UnpackedOrPacked:
					result = Core.Constants.PkgUnit.Unit;
					break;
				case CMRPackageTypes.Codes.Bobbin:
				case CMRPackageTypes.Codes.Bolt:
				case CMRPackageTypes.Codes.Bucket:
				case CMRPackageTypes.Codes.Bulk:
				case CMRPackageTypes.Codes.BulkLiquefiedGasAtAbnormalTemperaturePressure:
				case CMRPackageTypes.Codes.BulkLiquid:
				case CMRPackageTypes.Codes.BulkSolidFineParticlesPowders:
				case CMRPackageTypes.Codes.BulkSolidGranularParticlesGrains:
				case CMRPackageTypes.Codes.BulkSolidLargeParticles:
				case CMRPackageTypes.Codes.CanRectanular:
				case CMRPackageTypes.Codes.Canvas:
				case CMRPackageTypes.Codes.CarboyProtected:
				case CMRPackageTypes.Codes.Cask:
				case CMRPackageTypes.Codes.Chest:
				case CMRPackageTypes.Codes.DemijohnNonProtected:
				case CMRPackageTypes.Codes.DemijohnProtected:
				case CMRPackageTypes.Codes.Dispenser:
				case CMRPackageTypes.Codes.Filmpack:
				case CMRPackageTypes.Codes.Firkin:
				case CMRPackageTypes.Codes.Flask:
				case CMRPackageTypes.Codes.Footlocker:
				case CMRPackageTypes.Codes.Frame:
				case CMRPackageTypes.Codes.Hamper:
				case CMRPackageTypes.Codes.Hogshead:
				case CMRPackageTypes.Codes.Jar:
				case CMRPackageTypes.Codes.JerricanCylinderical:
				case CMRPackageTypes.Codes.JerricanRectangular:
				case CMRPackageTypes.Codes.Jug:
				case CMRPackageTypes.Codes.Mat:
				case CMRPackageTypes.Codes.Nest:
				case CMRPackageTypes.Codes.Net:
				case CMRPackageTypes.Codes.Pack:
				case CMRPackageTypes.Codes.Packet:
				case CMRPackageTypes.Codes.Pad:
				case CMRPackageTypes.Codes.Parcel:
				case CMRPackageTypes.Codes.Pitcher:
				case CMRPackageTypes.Codes.Plate:
				case CMRPackageTypes.Codes.Pot:
				case CMRPackageTypes.Codes.Rednet:
				case CMRPackageTypes.Codes.Sachet:
				case CMRPackageTypes.Codes.SeaChest:
				case CMRPackageTypes.Codes.Shrinkwrapped:
				case CMRPackageTypes.Codes.SkeletonCase:
				case CMRPackageTypes.Codes.Suitcase:
				case CMRPackageTypes.Codes.TeaChest:
				case CMRPackageTypes.Codes.Tin:
				case CMRPackageTypes.Codes.TrayTrayPack:
				case CMRPackageTypes.Codes.Trunk:
				case CMRPackageTypes.Codes.Truss:
				case CMRPackageTypes.Codes.Tub:
				case CMRPackageTypes.Codes.Tun:
				case CMRPackageTypes.Codes.Vacuumpacked:
				case CMRPackageTypes.Codes.Vat:
					result = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
					break;
			}

			return result;
		}

		internal static ZString ConvertVolumeUnitToCMRVolumeUnit(ZString code)
		{
			ZString result = code;
			switch (code)
			{
				case Core.Constants.Volume.CubicMetres:
					result = CMRQuantityUnits.Codes.CubicMetre;
					break;
				case Core.Constants.Volume.CubicDecimetres:
					result = CMRQuantityUnits.Codes.CubicDecimetre;
					break;
			}
			return result;
		}

		internal static ZString ConvertCMRVolumeUnitToVolumeUnit(ZString code)
		{
			ZString result = Core.Constants.Volume.CubicMetres;
			switch (code)
			{
				case CMRQuantityUnits.Codes.CubicMetre:
					result = Core.Constants.Volume.CubicMetres;
					break;
				case CMRQuantityUnits.Codes.CubicDecimetre:
					result = Core.Constants.Volume.CubicDecimetres;
					break;
			}
			return result;
		}
	}
}
