using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.FLF
{
	public class DocFLFARInvoice : DocARInvoice
	{
		#region Constructors and Type Overriding

		protected DocFLFARInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factory)
			: base(invoicingBase, factory)
		{
		}

		public new static DocFLFARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return (invoicingBase != null) ? new DocFLFARInvoice(invoicingBase, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocFLFARInvoice OverriddenNewMethod(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return DocFLFARInvoice.New(invoicingBase, factory);
		}

		#endregion

		#region Menu Filter Fields

		public override ZBool PrintStandard
		{
			get
			{
				return !PrintClientSpecific;
			}
		}

		public override ZBool PrintClientSpecific
		{
			get
			{
				return ZBool.True;
			}
		}
		#endregion
	}
}

#region SetUp && Overrides
#endregion
