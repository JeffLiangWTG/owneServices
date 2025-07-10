using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.MFI.DocWrappers
{
	public class MFIInvoicingBaseDocumentSupporter : InvoicingBaseDocumentSupporter
	{
		protected MFIInvoicingBaseDocumentSupporter(InvoicingBase invoice)
			: base(invoice)
		{
		}

		public new static InvoicingBaseDocumentSupporter New(InvoicingBase invoice)
		{
			return (invoice == null) ? null : new MFIInvoicingBaseDocumentSupporter(invoice);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Core.Constants.DataContext.ARInvoice)
			{
				result = new DocumentWrapper[] { DocMFIARInvoice.New(Invoice, Factory) };
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			return result;
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			string result = null;

			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandardInvoice:
					result = (MFIConstants.NZ.ClientSpecificCondition) ? ZBool.False.ToString() : ZBool.True.ToString();
					break;
				case MenuTemplateFilterType.PrintClientSpecificInvoice:
					result = MFIConstants.NZ.ClientSpecificCondition.ToString();
					break;
				default:
					result = ZBool.False.ToString();
					break;
			}

			return result;
		}
	}
}
