using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class IIDUniversalShipmentMessageInterpretationGeneratorTest : TestCase
	{
		public void GetInterpretatedHTML()
		{
			AssertEquals(string.Empty, IIDUniversalShipmentMessageInterpretationGenerator.GetInterpretatedHTML(null));
		}
	}
}
