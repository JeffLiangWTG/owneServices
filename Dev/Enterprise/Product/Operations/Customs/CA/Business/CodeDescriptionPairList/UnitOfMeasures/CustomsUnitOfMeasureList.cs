using CargoWise.EntityFramework;
using Enterprise.Core;
using static Enterprise.Customs.CA.Registry.CAPackageTypePairCollection;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class CustomsUnitOfMeasureList
	{
		static CodeDescriptionPairList UnitConversions(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CACustoms.UnitConversions", delegate
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(Constants.Weight.Kilograms, Codes.Kilogram);
				result.AddPair(Constants.Length.Centimetres, Codes.Centimeter);
				result.AddPair(Constants.Volume.CubicDecimetres, Codes.CubicDecimetre);
				result.AddPair(Constants.Volume.CubicMetres, Codes.CubicMetre);
				result.AddPair(Constants.Weight.Decitons, Codes.Deciton);
				result.AddPair(Constants.Weight.Grams, Codes.Gram);
				result.AddPair(Constants.Weight.Hectograms, Codes.Hectogram);
				result.AddPair(Constants.Length.Kilometres, Codes.Kilometre);
				result.AddPair(Constants.Weight.Kilotonnes, Codes.Kiloton);
				result.AddPair(Constants.Volume.Litre, Codes.Litre);
				result.AddPair(LitersofAlcohol, Codes.LitrePureAlcohol);
				result.AddPair(Constants.Volume.MegaLitre, Codes.Megalitre);
				result.AddPair(Constants.Length.Metres, Codes.Metre);
				result.AddPair(Constants.Weight.MetricCarat, Codes.MetricCarat);
				result.AddPair(Constants.Weight.Tonnes, Codes.MetricTon);
				result.AddPair(Constants.Weight.Milligrams, Codes.Milligram);
				result.AddPair(Constants.Length.Millimetres, Codes.Millimeter);

				result.AddPair(Codes.NumberEA, Codes.Number);
				result.AddPair(Codes.PieceH87, Codes.Number);
				result.AddPair(Codes.Pound, Codes.Kilogram);
				result.AddPair(Codes.Pack, Codes.Number);
				result.AddPair(Codes.Packet, Codes.Number);

				result.AddPair(Number, Codes.NumberEA);
				result.AddPair(Constants.Area.SquareCentimetre, Codes.SquareCentimetre);
				result.AddPair(Constants.Area.SquareMetre, Codes.SquareMetre);
				result.AddPair(Constants.Area.SquareMillimetre, Codes.SquareMillimetre);
				result.AddPair(IIDUnitOfCountCodeList.Codes.Pair, Codes.PairsPR);
				result.AddPair(Constants.PkgUnit.Dozen, Codes.Dozen);
				result.AddPair(Constants.PkgUnit.Gross, Codes.Gross);

				result.AddPair(Constants.PkgUnit.Bag, Codes.Bag);
				result.AddPair(Constants.PkgUnit.BaleCompressed, Codes.Bale);
				result.AddPair(Constants.PkgUnit.BaleUncompressed, Codes.Bale);
				result.AddPair(Constants.PkgUnit.Bundle, Codes.Bundle);
				result.AddPair(Constants.PkgUnit.Bottle, Codes.Bottle);

				result.AddPair(Constants.PkgUnit.Box, Codes.Box);
				result.AddPair(Constants.PkgUnit.Basket, Codes.Basket);
				result.AddPair(Constants.PkgUnit.Case, Codes.Case);
				result.AddPair(Constants.PkgUnit.Coil, Codes.Coil);
				result.AddPair(Constants.PkgUnit.Crate, Codes.Crate);
				result.AddPair(Constants.PkgUnit.Carton, Codes.Carton);
				result.AddPair(Constants.PkgUnit.Cylinder, Codes.Cylinder);
				result.AddPair(Constants.PkgUnit.Drum, Codes.Drum);
				result.AddPair(Constants.PkgUnit.Envelope, Codes.Envelope);
				result.AddPair(Constants.PkgUnit.Keg, Codes.Keg);

				result.AddPair(Constants.PkgUnit.Mix, Codes.Mix);
				result.AddPair(PackagePAC, Codes.Package);
				result.AddPair(Constants.PkgUnit.Pail, Codes.Pail);
				result.AddPair(PackType.Freight.PC, Codes.PieceH87);
				result.AddPair(Constants.PkgUnit.Piece, Codes.PieceH87);
				result.AddPair(PackType.Freight.PCS, Codes.PieceH87);
				result.AddPair(Constants.PkgUnit.Package, Codes.Package);
				result.AddPair(Constants.PkgUnit.Pallet, Codes.Pallet);
				result.AddPair(PairPRS, Codes.PairsPR);
				result.AddPair(PairPAR, Codes.PairsPR);
				result.AddPair(Constants.PkgUnit.Reel, Codes.Reel);

				result.AddPair(Constants.PkgUnit.Roll, Codes.Roll);
				result.AddPair(Constants.PkgUnit.Sheet, Codes.Sheet);
				result.AddPair(Constants.PkgUnit.Skid, Codes.Skid);
				result.AddPair(Constants.PkgUnit.Spool, Codes.Spool);
				result.AddPair(Constants.PkgUnit.Tote, Codes.Tote);
				result.AddPair(Tray, Codes.TRAY);
				result.AddPair(Constants.PkgUnit.Tube, Codes.Tube);
				result.AddPair(Constants.PkgUnit.Unit, Codes.Unit);
				result.AddPair(Constants.Length.Yards, Codes.Yards);
				result.AddPair(Constants.Weight.Pounds, Codes.Pound);

				result.AddPair(Constants.Weight.PoundsTroy, Codes.PoundsTroy);
				result.AddPair(Constants.Weight.OuncesTroy, Codes.OuncesTroy);
				result.AddPair(Constants.Weight.Ounces, Codes.Ounce);
				result.AddPair(Constants.Weight.LongTons, Codes.LongTons);
				result.AddPair(Constants.Weight.ShortTons, Codes.ShortTons);
				result.AddPair(Constants.Volume.CubicFeet, Codes.CubicFeet);
				result.AddPair(Constants.Volume.CubicYards, Codes.CubicYards);
				result.AddPair(Constants.Area.SquareFoot, Codes.SquareFoot);
				result.AddPair(Constants.Area.SquareInch, Codes.SquareInch);
				result.AddPair(Constants.Area.SquareYard, Codes.SquareYard);

				result.AddPair(NumberNMB, Codes.NumberEA);
				result.AddPair(Constants.Volume.USGallons, Codes.Gallons);
				result.AddPair(Constants.Volume.ImperialGallons, Codes.ImperialGallons);
				return result;
			});
		}

		const string NumberNMB = "NMB";
		const string Number = "NO";
		const string LitersofAlcohol = "LA";
		const string PackagePAC = "PAC";
		const string PairPRS = "PRS";
		const string PairPAR = "PAR";
		const string Tray = "TRY";

		public static string ConvertStockUnitsToCustomsUnits(string stockUnit, BusinessObjectFactory factory, bool isIID = false)
		{
			if (!isIID)
			{
				switch (stockUnit)
				{
					case Codes.Number:
					case Number:
						return Codes.Number;
				}
			}

			return UnitConversions(factory).GetDescriptionFromCode(stockUnit) ?? stockUnit;
		}

		public static string ConvertCustomsUnitsToStockUnits(string customsUnit, BusinessObjectFactory factory, bool isIID = false)
		{
			if (!isIID)
			{
				switch (customsUnit)
				{
					case Codes.Number:
						return Number;
				}
			}

			return UnitConversions(factory).GetCodeFromDescription(customsUnit) ?? customsUnit;
		}
	}
}
