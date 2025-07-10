using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.WLG
{
	class DocWLGARInvoice : DocARInvoice
	{
		#region Constructors and Type Overriding

		protected DocWLGARInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factory)
			: base(invoicingBase, factory)
		{
		}

		public new static DocWLGARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return (invoicingBase != null) ? new DocWLGARInvoice(invoicingBase, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocARInvoice OverriddenNewMethod(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return DocWLGARInvoice.New(invoicingBase, factory);
		}

		#endregion

		#region Properties

		public override ZBool PrintStandard
		{
			get { return !PrintClientSpecific; }
		}

		public override ZBool PrintClientSpecific
		{
			get { return (DocumentSupporter != null && DocumentSupporter.UseClientSpecific); }
		}

		#endregion

		#region Implementation

		protected WLGInvoicingBaseDocumentSupporter DocumentSupporter
		{
			get { return ((InvoicingBase)WrappedObject).DocumentSupporter as WLGInvoicingBaseDocumentSupporter; }
		}

		#endregion

	}
}
