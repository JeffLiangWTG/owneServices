using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.GCG.DocWrappers
{
	public class GCGInvoicingBaseDocumentSupporter : InvoicingBaseDocumentSupporter
	{
		public GCGInvoicingBaseDocumentSupporter(InvoicingBase invoice) : base(invoice)
		{
		}

		public new static InvoicingBaseDocumentSupporter New(InvoicingBase invoice)
		{
			return (invoice == null) ? null : new GCGInvoicingBaseDocumentSupporter(invoice);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			string result = null;

			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandardInvoice:
					result = ZBool.False.ToString();
					break;

				case MenuTemplateFilterType.PrintClientSpecificInvoice:
					result = ZBool.True.ToString();
					break;
			}

			return result;
		}
	}
}
