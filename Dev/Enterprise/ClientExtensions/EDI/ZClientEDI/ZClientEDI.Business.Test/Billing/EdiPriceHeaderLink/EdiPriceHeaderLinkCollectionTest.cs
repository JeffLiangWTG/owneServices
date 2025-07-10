using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceHeaderLinkCollection))]
	internal class EdiPriceHeaderLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiPriceHeaderLinkCollection>
	{
	}
}
