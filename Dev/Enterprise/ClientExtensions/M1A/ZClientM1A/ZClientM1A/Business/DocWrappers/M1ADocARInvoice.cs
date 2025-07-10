using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.M1A.Business
{
	public class M1ADocARInvoice : DocARInvoice
	{
		#region Constructors and Type Overriding
		protected M1ADocARInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factory)
			: base(invoicingBase, factory)
		{
		}

		public new static M1ADocARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return (invoicingBase != null) ? new M1ADocARInvoice(invoicingBase, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocARInvoice OverriddenNewMethod(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return M1ADocARInvoice.New(invoicingBase, factory);
		}

		#endregion

		public override ZBool PrintStandard
		{
			get { return ZBool.False; }
		}

		public override ZBool PrintClientSpecific
		{
			get { return ZBool.True; }
		}
	}
}

#region Implementation
#endregion
