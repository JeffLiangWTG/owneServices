

namespace Enterprise.Client.UPE.Business
{
	public static class UPS_EDI_CodeMaps
	{
		internal static class UnitsOfQuantity
		{
			public static string EDICode(string uPSCode)
			{
				string result = uPSCode;

				switch (uPSCode)
				{
					case "EA": //each:
					case "PR":
					case "PRS": //pairs
					case "SET": //set
					case "PC":
					case "PCS":
						result = Core.Constants.PkgUnit.Piece;
						break;
					case "BA": //barrel:
					case "BU": //butt
						result = Core.Constants.PkgUnit.Drum;
						break;
					case "BG":
						result = Core.Constants.PkgUnit.Bag;
						break;
					case "BX":
						result = Core.Constants.PkgUnit.Box;
						break;
					case "BH": //bunch	
					case "BE":
						result = Core.Constants.PkgUnit.Bundle;
						break;
					case "CI": //cannister
						result = Core.Constants.PkgUnit.Cylinder;
						break;
					case "CT":
						result = Core.Constants.PkgUnit.Carton;
						break;
					case "CS":
						result = Core.Constants.PkgUnit.Case;
						break;
					case "CM":
						result = Core.Constants.Length.Centimetres;
						break;
					case "CON":
						result = Core.Constants.PkgUnit.Container;
						break;
					case "CR":
						result = Core.Constants.PkgUnit.Crate;
						break;
					case "CY":
						result = Core.Constants.PkgUnit.Cylinder;
						break;
					case "DOZ":
						result = Core.Constants.PkgUnit.Dozen;
						break;
					case "EN":
						result = Core.Constants.PkgUnit.Envelope;
						break;
					case "FT":
						result = Core.Constants.Length.Feet;
						break;
					case "KG":
					case "KGS":
						result = Core.Constants.Weight.Kilograms;
						break;
					case "L":
					case "PF": //proof litres
						result = Core.Constants.Volume.Litre;
						break;
					case "M":
						result = Core.Constants.Length.Metres;
						break;
					case "PK":
					case "PA": //packet
						result = Core.Constants.PkgUnit.Package;
						break;
					case "PAL":
						result = Core.Constants.PkgUnit.Pallet;
						break;
					case "LB":
					case "LBS":
						result = Core.Constants.Weight.Pounds;
						break;
					case "RL":
						result = Core.Constants.PkgUnit.Roll;
						break;
					case "SME":
						result = Core.Constants.Area.SquareMetre;
						break;
					case "SYD":
						result = Core.Constants.Area.SquareYard;
						break;
					case "TU":
						result = Core.Constants.PkgUnit.Tube;
						break;
					case "Y":
						result = Core.Constants.Length.Yards;
						break;
				}

				return result;
			}
		}
	}
}
