using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceUsageMappingCollection))]
	internal class EdiPriceUsageMappingCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiPriceUsageMappingCollection>
	{
	}
}
