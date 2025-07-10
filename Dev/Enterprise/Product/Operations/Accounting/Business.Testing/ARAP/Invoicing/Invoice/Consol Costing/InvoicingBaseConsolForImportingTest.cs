using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseConsolForImporting))]
	public class InvoicingBaseConsolForImportingTest : BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObject result = Factory.New(GetExpectedBusinessObjectType());
			return result;
		}
	}
}
