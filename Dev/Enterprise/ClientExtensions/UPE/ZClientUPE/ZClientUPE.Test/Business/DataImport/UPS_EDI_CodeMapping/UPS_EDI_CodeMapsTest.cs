using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPS_EDI_CodeMapsTest : TestCase
	{
		public void TestUnitsOfQuantityUPSToEDICode()
		{
			AssertEquals(Core.Constants.PkgUnit.Piece, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("EA"));
			AssertEquals(Core.Constants.PkgUnit.Piece, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PR"));
			AssertEquals(Core.Constants.PkgUnit.Piece, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PRS"));
			AssertEquals(Core.Constants.PkgUnit.Piece, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("SET"));
			AssertEquals(Core.Constants.PkgUnit.Piece, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PC"));
			AssertEquals(Core.Constants.PkgUnit.Piece, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PCS"));
			AssertEquals(Core.Constants.PkgUnit.Drum, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("BA"));
			AssertEquals(Core.Constants.PkgUnit.Drum, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("BU"));
			AssertEquals(Core.Constants.PkgUnit.Bag, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("BG"));
			AssertEquals(Core.Constants.PkgUnit.Box, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("BX"));
			AssertEquals(Core.Constants.PkgUnit.Bundle, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("BH"));
			AssertEquals(Core.Constants.PkgUnit.Bundle, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("BE"));
			AssertEquals(Core.Constants.PkgUnit.Cylinder, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CI"));
			AssertEquals(Core.Constants.PkgUnit.Carton, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CT"));
			AssertEquals(Core.Constants.PkgUnit.Case, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CS"));
			AssertEquals(Core.Constants.Length.Centimetres, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CM"));
			AssertEquals(Core.Constants.PkgUnit.Container, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CON"));
			AssertEquals(Core.Constants.PkgUnit.Crate, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CR"));
			AssertEquals(Core.Constants.PkgUnit.Cylinder, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("CY"));
			AssertEquals(Core.Constants.PkgUnit.Dozen, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("DOZ"));
			AssertEquals(Core.Constants.PkgUnit.Envelope, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("EN"));
			AssertEquals(Core.Constants.Length.Feet, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("FT"));
			AssertEquals(Core.Constants.Weight.Kilograms, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("KG"));
			AssertEquals(Core.Constants.Weight.Kilograms, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("KGS"));
			AssertEquals(Core.Constants.Volume.Litre, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("L"));
			AssertEquals(Core.Constants.Volume.Litre, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PF"));
			AssertEquals(Core.Constants.Length.Metres, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("M"));
			AssertEquals(Core.Constants.PkgUnit.Package, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PK"));
			AssertEquals(Core.Constants.PkgUnit.Package, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PA"));
			AssertEquals(Core.Constants.PkgUnit.Pallet, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("PAL"));
			AssertEquals(Core.Constants.Weight.Pounds, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("LB"));
			AssertEquals(Core.Constants.Weight.Pounds, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("LBS"));
			AssertEquals(Core.Constants.PkgUnit.Roll, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("RL"));
			AssertEquals(Core.Constants.Area.SquareMetre, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("SME"));
			AssertEquals(Core.Constants.Area.SquareYard, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("SYD"));
			AssertEquals(Core.Constants.PkgUnit.Tube, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("TU"));
			AssertEquals(Core.Constants.Length.Yards, UPS_EDI_CodeMaps.UnitsOfQuantity.EDICode("Y"));
		}
	}
}
