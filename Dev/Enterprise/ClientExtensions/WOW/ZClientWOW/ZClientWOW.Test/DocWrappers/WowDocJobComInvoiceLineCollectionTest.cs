using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WowDocJobComInvoiceLineCollection))]
	public class WowDocJobComInvoiceLineCollectionTest : DocBaseJobComInvoiceLineCollectionTest<WowDocJobComInvoiceLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceLine jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			return DocJobComInvoiceLine.New(jobComInvoiceLine, Factory);
		}

		protected override WowDocJobComInvoiceLineCollection GetCollectionToTest()
		{
			return new WowDocJobComInvoiceLineCollection(Factory);
		}
	}
}
