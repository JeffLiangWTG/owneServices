using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	public class JobComInvoiceLineTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var ji = Factory.New<JobComInvoiceLine>();
			return ji.Taxes;
		}
	}
}
