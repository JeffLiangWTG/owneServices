#region Test
#if DEBUG

using NUnit.Framework;

namespace Enterprise.NumberFountain.Testing
{
	public class CheckDigitHelperTest : TestCase
	{
		public void TestCalculateCheckDigitStandard()
		{
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "1"));
			AssertEquals("8", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "23"));
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "456"));
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "789A"));
			AssertEquals("3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "BCDEF"));
			AssertEquals("B", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"));
			AssertEquals("Case and non alphanumeric characters are ignored.", "B", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Standard, "AbcdefgHIJKLM nopq, rstu-vwxyz.0123456789!"));
		}

		public void TestCalculateCheckDigitMAWB()
		{
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "1"));
			AssertEquals("2", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "23"));
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "456"));
			AssertEquals("4", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "789A"));
			AssertEquals("3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "BCDEF"));
			AssertEquals("5", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"));
			AssertEquals("Case and non alphanumeric characters are ignored.", "5", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MAWB, "AbcdefgHIJKLM nopq, rstu-vwxyz.0123456789!"));
		}

		public void TestCalculateCheckDigitRCC()
		{
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "1"));
			AssertEquals("8", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "23"));
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "456"));
			AssertEquals("1", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "456A"));
			AssertEquals("3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "00193"));
			AssertEquals("3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "S00193"));
			AssertEquals("3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "S00193ssss"));
			AssertEquals("7", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "1234500006789"));
			AssertEquals("3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"));
			AssertEquals("Case and non alphanumeric characters are ignored.", "3", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.CanadaCustoms, "AbcdefgHIJKLM nopq, rstu-vwxyz.0123456789!"));
		}

		public void TestCalculateCheckDigitRecursiveMOD10()
		{
			AssertEquals("8", CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.RecursiveMOD10, "313947143000901"));
		}

		public void TestCalculateCheckDigitMOD10()
		{
			var result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MOD10, "7992739871");
			AssertEquals("3", result);

			result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MOD10, "4002169000013");
			AssertEquals("6", result);

			result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MOD10, "5289154398");
			AssertEquals("6", result);
		}

		public void TestCalculateCheckDigitMMOD10V05()
		{
			var result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MOD10V05, "120319");
			AssertEquals("6", result);

			result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MOD10V05, "561237785");
			AssertEquals("3", result);
		}

		public void TestCalculateAlgorithm731()
		{
			var result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Algorithm731, "12131295");
			AssertEquals("2", result);

			result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Algorithm731, "13467891");
			AssertEquals("3", result);

			result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.Algorithm731, "135792468");
			AssertEquals("9", result);
		}

		public void TestCalculateCheckDigitMOD97()
		{
			var result = CheckDigitHelper.CalculateCheckDigit(CheckDigitAlgorithm.MOD97, "GB00WEST12345698765432");
			AssertEquals("82", result);
		}
	}
}

#endif
#endregion
