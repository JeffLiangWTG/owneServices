using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ComponentAddInfo))]
	sealed class ComponentAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = "Y";
			var cNSCPGAHeader = invoiceLine.CNSCPGAHeader;
			return new ComponentAddInfo(cNSCPGAHeader.Components.AddNew().B7_AddInfoDataInfo);
		}
	}
}
