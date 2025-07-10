using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSAddInfoJobComInvoiceLine))]
	public class EMCSAddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.InvoiceHeader;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			return new EMCSAddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
		}
	}
}
