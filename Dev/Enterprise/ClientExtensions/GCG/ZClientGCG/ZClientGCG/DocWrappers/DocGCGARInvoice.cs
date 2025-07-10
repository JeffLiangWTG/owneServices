using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.GCG.DocWrappers
{
	public class DocGCGARInvoice : DocARInvoice
	{
		#region Constructors and Type Overriding

		protected DocGCGARInvoice(InvoicingBase invoicingBase, BusinessObjectFactory factory) : base(invoicingBase, factory)
		{
		}

		public new static DocGCGARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return (invoicingBase != null) ? new DocGCGARInvoice(invoicingBase, factory) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocARInvoice OverriddenNewMethod(InvoicingBase invoicingBase, BusinessObjectFactory factory)
		{
			return DocGCGARInvoice.New(invoicingBase, factory);
		}

		#endregion

		#region Properties

		public override MultilingualString CreditTerms
		{
			get
			{
				MultilingualString result;

				if (InvoiceTerm == "COD")
				{
					result = (NoResString)"Immediate Payment";
				}
				else
				{
					result = base.CreditTerms;
				}

				return result;
			}
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
