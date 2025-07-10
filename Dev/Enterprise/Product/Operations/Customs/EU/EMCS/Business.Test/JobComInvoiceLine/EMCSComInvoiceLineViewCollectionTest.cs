using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSJobComInvoiceLineViewCollection))]
	class EMCSComInvoiceLineViewCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<EMCSJobComInvoiceLineViewCollection>
	{
		protected override EMCSJobComInvoiceLineViewCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader.JobComInvoiceLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.New<EMCSJobComInvoiceLine>();
			line.JI_JZ = invoiceHeader.PK;

			if (invoiceHeader.JobComInvoiceLines.Contains(line))
			{
				invoiceHeader.JobComInvoiceLines.Remove(line);
			}

			return line;
		}

		EMCSJobComInvoiceHeader invoiceHeader;
	}
}
