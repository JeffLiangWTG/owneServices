using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Core;
using static Enterprise.Customs.CA.Registry.CAPackageTypePairCollection;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CustomsUnitOfMeasureListTest : TestCaseWithFactory
	{
		public void TestConvertStockUnitsToCustomsUnits()
		{
			CombineAssertions(() =>
			{
				AssertEquals("", "", CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits("", Factory));
				var list = Factory.GetCachedValue("CACustoms.UnitConversions", () => new CodeDescriptionPairList());
				AssertNotEquals(0, list.Count);
				var codes = list.OfType<ICodeDescription>().Select(x => x.Code).ToList();
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Kilograms), Constants.Weight.Kilograms, CustomsUnitOfMeasureList.Codes.Kilogram);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Length.Centimetres), Constants.Length.Centimetres, CustomsUnitOfMeasureList.Codes.Centimeter);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.CubicDecimetres), Constants.Volume.CubicDecimetres, CustomsUnitOfMeasureList.Codes.CubicDecimetre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.CubicMetres), Constants.Volume.CubicMetres, CustomsUnitOfMeasureList.Codes.CubicMetre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Decitons), Constants.Weight.Decitons, CustomsUnitOfMeasureList.Codes.Deciton);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Grams), Constants.Weight.Grams, CustomsUnitOfMeasureList.Codes.Gram);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Hectograms), Constants.Weight.Hectograms, CustomsUnitOfMeasureList.Codes.Hectogram);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Length.Kilometres), Constants.Length.Kilometres, CustomsUnitOfMeasureList.Codes.Kilometre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Kilotonnes), Constants.Weight.Kilotonnes, CustomsUnitOfMeasureList.Codes.Kiloton);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.Litre), Constants.Volume.Litre, CustomsUnitOfMeasureList.Codes.Litre);
				AssertConvertStockUnitsToCustomsUnits(codes, "LitersofAlcohol", "LA", CustomsUnitOfMeasureList.Codes.LitrePureAlcohol);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.MegaLitre), Constants.Volume.MegaLitre, CustomsUnitOfMeasureList.Codes.Megalitre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Length.Metres), Constants.Length.Metres, CustomsUnitOfMeasureList.Codes.Metre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.MetricCarat), Constants.Weight.MetricCarat, CustomsUnitOfMeasureList.Codes.MetricCarat);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Tonnes), Constants.Weight.Tonnes, CustomsUnitOfMeasureList.Codes.MetricTon);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Milligrams), Constants.Weight.Milligrams, CustomsUnitOfMeasureList.Codes.Milligram);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Length.Millimetres), Constants.Length.Millimetres, CustomsUnitOfMeasureList.Codes.Millimeter);
				AssertConvertStockUnitsToCustomsUnits(codes, "Number EA => NMB", CustomsUnitOfMeasureList.Codes.NumberEA, CustomsUnitOfMeasureList.Codes.Number);
				AssertConvertStockUnitsToCustomsUnits(codes, "H87 => NMB", CustomsUnitOfMeasureList.Codes.PieceH87, CustomsUnitOfMeasureList.Codes.Number);
				AssertConvertStockUnitsToCustomsUnits(codes, "PA => NMB", CustomsUnitOfMeasureList.Codes.Pack, CustomsUnitOfMeasureList.Codes.Number);
				AssertConvertStockUnitsToCustomsUnits(codes, "PK => NMB", CustomsUnitOfMeasureList.Codes.Packet, CustomsUnitOfMeasureList.Codes.Number);

				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Area.SquareCentimetre), Constants.Area.SquareCentimetre, CustomsUnitOfMeasureList.Codes.SquareCentimetre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Area.SquareMetre), Constants.Area.SquareMetre, CustomsUnitOfMeasureList.Codes.SquareMetre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Area.SquareMillimetre), Constants.Area.SquareMillimetre, CustomsUnitOfMeasureList.Codes.SquareMillimetre);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(IIDUnitOfCountCodeList.Codes.Pair), IIDUnitOfCountCodeList.Codes.Pair, CustomsUnitOfMeasureList.Codes.PairsPR);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Dozen), Constants.PkgUnit.Dozen, CustomsUnitOfMeasureList.Codes.Dozen);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Gross), Constants.PkgUnit.Gross, CustomsUnitOfMeasureList.Codes.Gross);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Bag), Constants.PkgUnit.Bag, CustomsUnitOfMeasureList.Codes.Bag);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.BaleCompressed), Constants.PkgUnit.BaleCompressed, CustomsUnitOfMeasureList.Codes.Bale);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.BaleUncompressed), Constants.PkgUnit.BaleUncompressed, CustomsUnitOfMeasureList.Codes.Bale);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Bundle), Constants.PkgUnit.Bundle, CustomsUnitOfMeasureList.Codes.Bundle);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Bottle), Constants.PkgUnit.Bottle, CustomsUnitOfMeasureList.Codes.Bottle);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Box), Constants.PkgUnit.Box, CustomsUnitOfMeasureList.Codes.Box);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Basket), Constants.PkgUnit.Basket, CustomsUnitOfMeasureList.Codes.Basket);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Case), Constants.PkgUnit.Case, CustomsUnitOfMeasureList.Codes.Case);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Coil), Constants.PkgUnit.Coil, CustomsUnitOfMeasureList.Codes.Coil);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Crate), Constants.PkgUnit.Crate, CustomsUnitOfMeasureList.Codes.Crate);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Carton), Constants.PkgUnit.Carton, CustomsUnitOfMeasureList.Codes.Carton);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Cylinder), Constants.PkgUnit.Cylinder, CustomsUnitOfMeasureList.Codes.Cylinder);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Drum), Constants.PkgUnit.Drum, CustomsUnitOfMeasureList.Codes.Drum);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Envelope), Constants.PkgUnit.Envelope, CustomsUnitOfMeasureList.Codes.Envelope);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Keg), Constants.PkgUnit.Keg, CustomsUnitOfMeasureList.Codes.Keg);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Mix), Constants.PkgUnit.Mix, CustomsUnitOfMeasureList.Codes.Mix);
				AssertConvertStockUnitsToCustomsUnits(codes, "PAC", "PAC", CustomsUnitOfMeasureList.Codes.Package);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Pail), Constants.PkgUnit.Pail, CustomsUnitOfMeasureList.Codes.Pail);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(PackType.Freight.PC), PackType.Freight.PC, CustomsUnitOfMeasureList.Codes.PieceH87);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(CustomsUnitOfMeasureList.Codes.Piece), Constants.PkgUnit.Piece, CustomsUnitOfMeasureList.Codes.PieceH87);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(PackType.Freight.PCS), PackType.Freight.PCS, CustomsUnitOfMeasureList.Codes.PieceH87);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Package), Constants.PkgUnit.Package, CustomsUnitOfMeasureList.Codes.Package);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Pallet), Constants.PkgUnit.Pallet, CustomsUnitOfMeasureList.Codes.Pallet);
				AssertConvertStockUnitsToCustomsUnits(codes, "PRS", "PRS", CustomsUnitOfMeasureList.Codes.PairsPR);
				AssertConvertStockUnitsToCustomsUnits(codes, "PAR", "PAR", CustomsUnitOfMeasureList.Codes.PairsPR);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Reel), Constants.PkgUnit.Reel, CustomsUnitOfMeasureList.Codes.Reel);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Roll), Constants.PkgUnit.Roll, CustomsUnitOfMeasureList.Codes.Roll);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Sheet), Constants.PkgUnit.Sheet, CustomsUnitOfMeasureList.Codes.Sheet);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Skid), Constants.PkgUnit.Skid, CustomsUnitOfMeasureList.Codes.Skid);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Spool), Constants.PkgUnit.Spool, CustomsUnitOfMeasureList.Codes.Spool);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Tote), Constants.PkgUnit.Tote, CustomsUnitOfMeasureList.Codes.Tote);
				AssertConvertStockUnitsToCustomsUnits(codes, "Tray", "TRY", CustomsUnitOfMeasureList.Codes.TRAY);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Tube), Constants.PkgUnit.Tube, CustomsUnitOfMeasureList.Codes.Tube);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.PkgUnit.Unit), Constants.PkgUnit.Unit, CustomsUnitOfMeasureList.Codes.Unit);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Length.Yards), Constants.Length.Yards, CustomsUnitOfMeasureList.Codes.Yards);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Pounds), Constants.Weight.Pounds, CustomsUnitOfMeasureList.Codes.Pound);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.PoundsTroy), Constants.Weight.PoundsTroy, CustomsUnitOfMeasureList.Codes.PoundsTroy);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.OuncesTroy), Constants.Weight.OuncesTroy, CustomsUnitOfMeasureList.Codes.OuncesTroy);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.Ounces), Constants.Weight.Ounces, CustomsUnitOfMeasureList.Codes.Ounce);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.LongTons), Constants.Weight.LongTons, CustomsUnitOfMeasureList.Codes.LongTons);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Weight.ShortTons), Constants.Weight.ShortTons, CustomsUnitOfMeasureList.Codes.ShortTons);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.CubicFeet), Constants.Volume.CubicFeet, CustomsUnitOfMeasureList.Codes.CubicFeet);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.CubicYards), Constants.Volume.CubicYards, CustomsUnitOfMeasureList.Codes.CubicYards);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Area.SquareFoot), Constants.Area.SquareFoot, CustomsUnitOfMeasureList.Codes.SquareFoot);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Area.SquareInch), Constants.Area.SquareInch, CustomsUnitOfMeasureList.Codes.SquareInch);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Area.SquareYard), Constants.Area.SquareYard, CustomsUnitOfMeasureList.Codes.SquareYard);

				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.USGallons), Constants.Volume.USGallons, CustomsUnitOfMeasureList.Codes.Gallons);
				AssertConvertStockUnitsToCustomsUnits(codes, nameof(Constants.Volume.ImperialGallons), Constants.Volume.ImperialGallons, CustomsUnitOfMeasureList.Codes.ImperialGallons);
				AssertEquals("Unit => EA", CustomsUnitOfMeasureList.Codes.Unit, CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits("UNT", Factory, true));
				AssertConvertStockUnitsToCustomsUnits(codes, CustomsUnitOfMeasureList.Codes.Number, CustomsUnitOfMeasureList.Codes.Number, CustomsUnitOfMeasureList.Codes.Number);
				AssertEquals("Number NMB => EA", CustomsUnitOfMeasureList.Codes.NumberEA, CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits(CustomsUnitOfMeasureList.Codes.Number, Factory, true));

				Assert("Missing test for these codes " + string.Join(", ", codes), codes.Count > 0);
			});
		}

		void AssertConvertStockUnitsToCustomsUnits(List<string> codes, string message, string code, string expectedResult)
		{
			AssertEquals(message, expectedResult, CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits(code, Factory));
			codes.Remove(code);
		}

		public void TestConvertCustomsUnitsToStockUnits()
		{
			CombineAssertions(() =>
			{
				AssertEquals("", "", CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits("", Factory));
				var list = Factory.GetCachedValue("CACustoms.UnitConversions", () => new CodeDescriptionPairList());
				AssertNotEquals(0, list.Count);
				var codes = list.OfType<ICodeDescription>().Select(x => x.Code).ToList();

				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Kilograms), Constants.Weight.Kilograms, CustomsUnitOfMeasureList.Codes.Kilogram);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Length.Centimetres), Constants.Length.Centimetres, CustomsUnitOfMeasureList.Codes.Centimeter);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.CubicDecimetres), Constants.Volume.CubicDecimetres, CustomsUnitOfMeasureList.Codes.CubicDecimetre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.CubicMetres), Constants.Volume.CubicMetres, CustomsUnitOfMeasureList.Codes.CubicMetre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Decitons), Constants.Weight.Decitons, CustomsUnitOfMeasureList.Codes.Deciton);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Grams), Constants.Weight.Grams, CustomsUnitOfMeasureList.Codes.Gram);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Hectograms), Constants.Weight.Hectograms, CustomsUnitOfMeasureList.Codes.Hectogram);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Length.Kilometres), Constants.Length.Kilometres, CustomsUnitOfMeasureList.Codes.Kilometre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Kilotonnes), Constants.Weight.Kilotonnes, CustomsUnitOfMeasureList.Codes.Kiloton);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.Litre), Constants.Volume.Litre, CustomsUnitOfMeasureList.Codes.Litre);
				AssertConvertCustomsUnitsToStockUnits(codes, "LitersofAlcohol", "LA", CustomsUnitOfMeasureList.Codes.LitrePureAlcohol);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.MegaLitre), Constants.Volume.MegaLitre, CustomsUnitOfMeasureList.Codes.Megalitre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Length.Metres), Constants.Length.Metres, CustomsUnitOfMeasureList.Codes.Metre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.MetricCarat), Constants.Weight.MetricCarat, CustomsUnitOfMeasureList.Codes.MetricCarat);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Tonnes), Constants.Weight.Tonnes, CustomsUnitOfMeasureList.Codes.MetricTon);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Milligrams), Constants.Weight.Milligrams, CustomsUnitOfMeasureList.Codes.Milligram);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Length.Millimetres), Constants.Length.Millimetres, CustomsUnitOfMeasureList.Codes.Millimeter);
				AssertConvertCustomsUnitsToStockUnits(codes, "Number", "NO", CustomsUnitOfMeasureList.Codes.NumberEA);

				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Area.SquareCentimetre), Constants.Area.SquareCentimetre, CustomsUnitOfMeasureList.Codes.SquareCentimetre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Area.SquareMetre), Constants.Area.SquareMetre, CustomsUnitOfMeasureList.Codes.SquareMetre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Area.SquareMillimetre), Constants.Area.SquareMillimetre, CustomsUnitOfMeasureList.Codes.SquareMillimetre);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(IIDUnitOfCountCodeList.Codes.Pair), IIDUnitOfCountCodeList.Codes.Pair, CustomsUnitOfMeasureList.Codes.PairsPR);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Dozen), Constants.PkgUnit.Dozen, CustomsUnitOfMeasureList.Codes.Dozen);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Gross), Constants.PkgUnit.Gross, CustomsUnitOfMeasureList.Codes.Gross);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Bag), Constants.PkgUnit.Bag, CustomsUnitOfMeasureList.Codes.Bag);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.BaleCompressed), Constants.PkgUnit.BaleCompressed, CustomsUnitOfMeasureList.Codes.Bale);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.BaleUncompressed), Constants.PkgUnit.BaleCompressed, CustomsUnitOfMeasureList.Codes.Bale);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Bundle), Constants.PkgUnit.Bundle, CustomsUnitOfMeasureList.Codes.Bundle);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Bottle), Constants.PkgUnit.Bottle, CustomsUnitOfMeasureList.Codes.Bottle);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Box), Constants.PkgUnit.Box, CustomsUnitOfMeasureList.Codes.Box);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Basket), Constants.PkgUnit.Basket, CustomsUnitOfMeasureList.Codes.Basket);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Case), Constants.PkgUnit.Case, CustomsUnitOfMeasureList.Codes.Case);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Coil), Constants.PkgUnit.Coil, CustomsUnitOfMeasureList.Codes.Coil);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Crate), Constants.PkgUnit.Crate, CustomsUnitOfMeasureList.Codes.Crate);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Carton), Constants.PkgUnit.Carton, CustomsUnitOfMeasureList.Codes.Carton);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Cylinder), Constants.PkgUnit.Cylinder, CustomsUnitOfMeasureList.Codes.Cylinder);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Drum), Constants.PkgUnit.Drum, CustomsUnitOfMeasureList.Codes.Drum);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Envelope), Constants.PkgUnit.Envelope, CustomsUnitOfMeasureList.Codes.Envelope);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Keg), Constants.PkgUnit.Keg, CustomsUnitOfMeasureList.Codes.Keg);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Mix), Constants.PkgUnit.Mix, CustomsUnitOfMeasureList.Codes.Mix);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Pail), Constants.PkgUnit.Pail, CustomsUnitOfMeasureList.Codes.Pail);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(PackType.Freight.PC), PackType.Freight.PC, CustomsUnitOfMeasureList.Codes.PieceH87);
				AssertConvertCustomsUnitsToStockUnits(codes, "Package PAC", "PAC", CustomsUnitOfMeasureList.Codes.Package);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Pallet), Constants.PkgUnit.Pallet, CustomsUnitOfMeasureList.Codes.Pallet);
				AssertConvertCustomsUnitsToStockUnits(codes, "PairPRS", "PR", CustomsUnitOfMeasureList.Codes.PairsPR);
				AssertConvertCustomsUnitsToStockUnits(codes, "PairPAR", "PR", CustomsUnitOfMeasureList.Codes.PairsPR);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Reel), Constants.PkgUnit.Reel, CustomsUnitOfMeasureList.Codes.Reel);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Roll), Constants.PkgUnit.Roll, CustomsUnitOfMeasureList.Codes.Roll);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Sheet), Constants.PkgUnit.Sheet, CustomsUnitOfMeasureList.Codes.Sheet);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Skid), Constants.PkgUnit.Skid, CustomsUnitOfMeasureList.Codes.Skid);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Spool), Constants.PkgUnit.Spool, CustomsUnitOfMeasureList.Codes.Spool);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Tote), Constants.PkgUnit.Tote, CustomsUnitOfMeasureList.Codes.Tote);
				AssertConvertCustomsUnitsToStockUnits(codes, "Tray", "TRY", CustomsUnitOfMeasureList.Codes.TRAY);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Tube), Constants.PkgUnit.Tube, CustomsUnitOfMeasureList.Codes.Tube);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.PkgUnit.Unit), "NO", CustomsUnitOfMeasureList.Codes.Unit);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Length.Yards), Constants.Length.Yards, CustomsUnitOfMeasureList.Codes.Yards);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Pounds), Constants.Weight.Pounds, CustomsUnitOfMeasureList.Codes.Pound);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.PoundsTroy), Constants.Weight.PoundsTroy, CustomsUnitOfMeasureList.Codes.PoundsTroy);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.OuncesTroy), Constants.Weight.OuncesTroy, CustomsUnitOfMeasureList.Codes.OuncesTroy);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.Ounces), Constants.Weight.Ounces, CustomsUnitOfMeasureList.Codes.Ounce);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.LongTons), Constants.Weight.LongTons, CustomsUnitOfMeasureList.Codes.LongTons);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Weight.ShortTons), Constants.Weight.ShortTons, CustomsUnitOfMeasureList.Codes.ShortTons);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.CubicFeet), Constants.Volume.CubicFeet, CustomsUnitOfMeasureList.Codes.CubicFeet);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.CubicYards), Constants.Volume.CubicYards, CustomsUnitOfMeasureList.Codes.CubicYards);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Area.SquareFoot), Constants.Area.SquareFoot, CustomsUnitOfMeasureList.Codes.SquareFoot);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Area.SquareInch), Constants.Area.SquareInch, CustomsUnitOfMeasureList.Codes.SquareInch);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Area.SquareYard), Constants.Area.SquareYard, CustomsUnitOfMeasureList.Codes.SquareYard);
				AssertConvertCustomsUnitsToStockUnits(codes, "EA", "NO", CustomsUnitOfMeasureList.Codes.NumberEA);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.USGallons), Constants.Volume.USGallons, CustomsUnitOfMeasureList.Codes.Gallons);
				AssertConvertCustomsUnitsToStockUnits(codes, nameof(Constants.Volume.ImperialGallons), Constants.Volume.ImperialGallons, CustomsUnitOfMeasureList.Codes.ImperialGallons);

				AssertEquals("NMB => NO", "NO", CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(CustomsUnitOfMeasureList.Codes.Number, Factory, false));
				AssertEquals("NO => NO", "NO", CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits("NO", Factory, true));

				Assert("Missing test for these codes " + string.Join(", ", codes), codes.Count > 0);
			});
		}

		void AssertConvertCustomsUnitsToStockUnits(List<string> codes, string message, string expectedResult, string code)
		{
			AssertEquals(message, expectedResult, CustomsUnitOfMeasureList.ConvertCustomsUnitsToStockUnits(code, Factory));
			codes.Remove(expectedResult);
		}
	}
}
