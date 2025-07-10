using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(CusGuaranteeHeader))]
	public abstract class CusGuaranteeHeaderAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountrySpecificInstructionIsCorrectlySubclass()
		{
			var guarantee = (CusGuaranteeHeader)GetNewBusinessObject();
			AssertNoExceptionThrown(() => _ = guarantee.CountrySpecificInstruction);
		}
	}
}
