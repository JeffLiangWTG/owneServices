using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARInvoiceDocWrapperCopyInfoTest : TransactionedTestCase
	{
		public void TestIncludeTradingTerms()
		{
			InvoiceCopy copy = new InvoiceCopy();
			ARInvoiceDocWrapperCopyInfo info = new ARInvoiceDocWrapperCopyInfo(copy);
			AssertEquals("Include trading terms should be false by default", false, info.IncludeTradingTerms);

			copy.IncludeTradingTerms = true;
			info = new ARInvoiceDocWrapperCopyInfo(copy);
			AssertEquals("Should be true after set in constructor from the copy", true, info.IncludeTradingTerms);
		}

		public void TestConstructor()
		{
			InvoiceCopy copy = new InvoiceCopy();
			copy.Name = (NoResString)"test";
			copy.IncludeTradingTerms = true;
			copy.DeliveryMethod = "EML";

			ARInvoiceDocWrapperCopyInfo info = new ARInvoiceDocWrapperCopyInfo(copy);
			AssertEquals("Should take properties from the InvoiceCopy object", "test", info.Name);
			AssertEquals("Should take properties from the InvoiceCopy object", true, info.IncludeTradingTerms);
			AssertEquals("Should take properties from the InvoiceCopy object", PrintCopyType.EML, info.DeliveryMethod);

			copy.DeliveryMethod = "ABC";
			info = new ARInvoiceDocWrapperCopyInfo(copy);
			AssertEquals("should default to method of ALL if the delivery method is not valid", PrintCopyType.ALL, info.DeliveryMethod);
		}
	}
}