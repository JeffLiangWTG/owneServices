using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class CondenserTest : TestCase
	{
		public void TestAllCondensation()
		{
			var input = new ZString[] { "D0008", "D0009", "D0010", "D0011" };
			var expectedOutput = "D0008-11";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			input = new ZString[] { " 08", " 0009  ", " 10", " 11" };
			expectedOutput = "8-11";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			input = new ZString[] { "AAA", "AAB" };
			expectedOutput = "AAA-B";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			input = new ZString[] { "D", "E", "F", "G", "H", "I", "J" };
			expectedOutput = "D-J";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			input = new ZString[] { "S00A", "S00B", "S00C" };
			expectedOutput = "S00A-C";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			// It's be good to get this working one day too....
			//input = new ZString[] { "S0010", "S0020", "S0030" };
			//expectedOutput = "S0010, S0020, S0030";
			//AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			// Gaps
			input = new ZString[] { "D", "E", "G", "H", "I", "J" };
			expectedOutput = "D, E, G, H, I, J";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			// It works on ASCII, so A does not directly follow 9 (as in Hex)
			input = new ZString[] { "0x8", "0x9", "0xA" };
			expectedOutput = "0x8, 0x9, 0xA";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));

			// Kinda sequential, but not good enough 
			input = new ZString[] { "D8", "D9", "D10", "D11" };
			expectedOutput = "D8, D9, D10, D11";
			AssertEquals(expectedOutput, ConsecutiveSequenceCondenser.Condense(input));
		}
	}
}
