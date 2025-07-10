using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderCollection))]
	sealed class IndependantJobComInvoiceHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobComInvoiceHeaderCollection(Factory);
		}
	}
}
