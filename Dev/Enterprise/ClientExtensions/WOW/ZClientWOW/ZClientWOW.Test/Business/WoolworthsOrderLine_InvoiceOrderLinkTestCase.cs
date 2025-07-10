using System.Data;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrderLine))]
	public class WoolworthsOrderLine_InvoiceOrderLinkTestCase : InvoiceOrderLinkTestCase
	{
		public void TestHasJobComInvoiceLineLink_WhenDeclarationInactive()
		{
			TestWoolworthsOrderLine orderLine = Factory.Load<TestWoolworthsOrderLine>(fOrderLine.PK);
			AssertEquals("Should have a link to the invoice line initially", true, orderLine.HasJobComInvoiceLineLink);
			fDeclaration.JE_IsCancelled = true;
			AssertEquals("Should not a link now that the declaration has been made inactive", false, orderLine.HasJobComInvoiceLineLink);
		}

		#region Test Classes
		class TestWoolworthsOrderLine : WoolworthsOrderLine
		{
			public TestWoolworthsOrderLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new bool HasJobComInvoiceLineLink
			{
				get
				{
					return base.HasJobComInvoiceLineLink;
				}
			}
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<WoolworthsOrderLine>();
		}
		#endregion
	}
}
