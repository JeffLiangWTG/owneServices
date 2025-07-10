using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.JAS.Business.Invoicing.Testing
{
	internal class JASInvoicingBaseForTest : ARInvoice, IJASInvoicingBase
	{
		public JASInvoicingBaseForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IJASInvoicingBase Members
		public ZString CreditNoteOrInvoice
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public InvoicingBase InvoicingBase
		{
			get
			{
				return null;
			}
		}

		InvoicingBase IJASInvoicingBase.InvoicingBase
		{
			get
			{
				return InvoicingBase;
			}
		}

		public bool IsInJasSpecificNeedsCoreValidation { get; set; }
		#endregion
	}
}
