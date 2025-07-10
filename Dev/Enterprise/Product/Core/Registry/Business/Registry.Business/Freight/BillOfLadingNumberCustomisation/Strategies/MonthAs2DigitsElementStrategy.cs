using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class MonthAs2DigitsElementStrategy : ElementStrategy
	{
		public override string Key
		{
			get { return BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard-coded constant")]
		public override string Name
		{
			get { return "Month as 2 Digits"; }
		}

		public override string Description
		{
			get { return Res.GetString("b138f06d-ce5e-446d-87a5-0891478255c4", "Month as 2 digits, JAN=01, FEB=02...DEC=12"); }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return 2;
		}

		/*
		 * Output: (0[1-9]|1[0-2])
		 * Check the text is 
		 *      "01 or 02 or 03 or 04 or 05 or 06 or 07 or 08 or 09"
		     or "10 or 11 or 12".
		 * 
		 * (0[1-9]|1[0-2])
		 * "00" => false
		 * "01" => true
		 * "11" => true
		 * "21" => false
		 * "12" => true
		 * "13" => false
		 */
		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			=> (NoResString)"(0[1-9]|1[0-2])";
	}
}
