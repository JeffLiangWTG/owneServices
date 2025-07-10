using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	internal class YearAsDigitElementStrategy : ElementStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public override string Key
		{
			get { return BillOfLadingNumberCustomisationElement.Keys.YearAsDigit; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard-coded constant")]
		public override string Name
		{
			get { return "Year as Digit(s)"; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public override string Description
		{
			get { return Res.GetString("5dc8f2bc-493b-4614-aaec-3ea06a5a2482", "Year as 1, 2 or 4 digits.  For example: as a single digit (2005 = 5, 2010 = 0), as a double digit (2005 = 05, 2010 = 10) and as 4 digits (2005 = 2005)"); }
		}
		public override bool Force
		{
			get { return false; }
		}

		public override bool UseDetail
		{
			get { return true; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public override string DefaultDetail
		{
			get { return "1"; }
		}
		public override int DetailMaxLength
		{
			get { return 1; }
		}
		public override FieldType DetailFieldType
		{
			get { return FieldType.Integer; }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return ZInt.ParseSafe(parent.Detail, 1);
		}

		public override BillOfLadingNumberCustomisationElementValidation GetValidation(BillOfLadingNumberCustomisationElement parent)
		{
			return new BillOfLadingNumberCustomisationElementYearAsDigitValidation(parent);
		}

		public override NumberCustomisationElementCategories Categories => NumberCustomisationElementCategories.ClientContract | base.Categories;

		/*
		 * Output example: [0-9]{4}
		 * Check the text is "number" and length is correct.
		 * 
		 * [0-9]{4}
		 * "9999" => true
		 * "1234" => true
		 * "99999" => false
		 * "a999" => false
		 * "999" => false
		 */
		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			=> $"[0-9]{{{CalcMaxGeneratedLength(parent)}}}";
	}
}
