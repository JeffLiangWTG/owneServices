using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	public class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
		}
	}
}
