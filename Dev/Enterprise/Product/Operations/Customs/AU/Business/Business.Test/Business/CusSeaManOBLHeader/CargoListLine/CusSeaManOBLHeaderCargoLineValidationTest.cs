using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLHeaderCargoLineValidationTest : BaseCusSeaManOBLHeaderValidationTest
	{
		public void TestBO_HeaderCargoType()
		{
			Line.BO_HeaderCargoType = ZString.Empty;
			AssertHasMessageError("When BO_HeaderCargoType is empty...", Line.BO_HeaderCargoTypeInfo, "You have not entered a Cargo Type.");

			Line.BO_HeaderCargoType = "@";
			AssertHasMessageError("When BO_HeaderCargoType is invalid...", Line.BO_HeaderCargoTypeInfo, "The code you have selected is not in the list.");

			Line.BO_HeaderCargoType = CMRCargoCodes.Codes.Empty;
			AssertNoMessageErrors(Line.BO_HeaderCargoTypeInfo);

			Line.BO_HeaderCargoType = CMRCargoCodes.Codes.Export;
			AssertNoMessageErrors(Line.BO_HeaderCargoTypeInfo);

			Line.BO_HeaderCargoType = CMRCargoCodes.Codes.Cabotage;
			AssertNoMessageErrors(Line.BO_HeaderCargoTypeInfo);
		}

		CusSeaManOBLHeaderCargoLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = (CusSeaManOBLHeaderCargoLine)GetNewOBLHeader();
				}
				return fLine;
			}
		}
		CusSeaManOBLHeaderCargoLine fLine;

		protected override BaseCusSeaManOBLHeader GetNewOBLHeader()
		{
			return (BaseCusSeaManOBLHeader)Factory.New(typeof(CusSeaManOBLHeaderCargoLine));
		}
	}
}
