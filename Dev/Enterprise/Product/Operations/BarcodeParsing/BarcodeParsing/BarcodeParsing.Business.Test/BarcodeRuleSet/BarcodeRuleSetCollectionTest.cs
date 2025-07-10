using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeRuleSetCollection))]
	class BarcodeRuleSetCollectionTest : ActiveBusinessObjectCollectionTestCase<BarcodeRuleSetCollection>
	{
	}
}
