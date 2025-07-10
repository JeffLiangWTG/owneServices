using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UAInvoiceLine))]
	public class UAInvoiceLineTest : APInvoiceLineTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<UAInvoiceLine>();
		}

		public new void TestValidationForIncompleteTransactionLine()
		{
			Assert(true);
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return true; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return true; }
		}

		protected override Type MasterHeaderType
		{
			get { return typeof(UAInvoice); }
		}

		protected override bool AcceptAL_AC
		{
			get { return false; }
		}

		protected override bool AcceptAL_AG
		{
			get { return false; }
		}
	}
}
