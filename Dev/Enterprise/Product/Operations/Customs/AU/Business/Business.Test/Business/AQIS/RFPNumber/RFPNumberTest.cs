using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RFPNumber))]
	sealed class RFPNumberTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<RFPNumber>
	{
		public void TestClone()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			RFPNumberWrapperTest.AddRFPNumber(invoiceLine, "TEST1", 1);
			Factory.Save();

			var clonedInvoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.RFPNumbers.Clone(clonedInvoiceLine);

			AssertEquals("Cloned RFPNumbers count", 1, clonedInvoiceLine.RFPNumbers.Count);
			AssertEquals("Cloned ZA_RFPNumber", invoiceLine.RFPNumbers[0].ZA_RFPNumber, clonedInvoiceLine.RFPNumbers[0].ZA_RFPNumber);
			AssertEquals("Cloned ZA_RFPLine", invoiceLine.RFPNumbers[0].ZA_RFPLine, clonedInvoiceLine.RFPNumbers[0].ZA_RFPLine);
			AssertEquals("Cloned ZA_RFPNetQuantity", invoiceLine.RFPNumbers[0].ZA_RFPNetQuantity, clonedInvoiceLine.RFPNumbers[0].ZA_RFPNetQuantity);
			AssertEquals("Cloned ZA_RFPPackCount", invoiceLine.RFPNumbers[0].ZA_RFPPackCount, clonedInvoiceLine.RFPNumbers[0].ZA_RFPPackCount);
			AssertEquals("Cloned ZA_RFPPackType", invoiceLine.RFPNumbers[0].ZA_RFPPackType, clonedInvoiceLine.RFPNumbers[0].ZA_RFPPackType);
			AssertEquals("Cloned ZA_RFPQtyUM", invoiceLine.RFPNumbers[0].ZA_RFPQtyUM, clonedInvoiceLine.RFPNumbers[0].ZA_RFPQtyUM);

			AssertEquals("Has cloned RFPNumber changes", false, clonedInvoiceLine.RFPNumbers[0].ZA_RFPNumberInfo.HasChanges);
			AssertEquals("Has cloned RFPNumber notifications", false, clonedInvoiceLine.RFPNumbers[0].HasNotifications());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var parent = invoiceLine;
			var result = factory.New<RFPNumber>();
			result.B7_ParentID = parent.PK;
			result.B7_ParentTableCode = parent.TablePrefix;
			return result;
		}
	}
}
