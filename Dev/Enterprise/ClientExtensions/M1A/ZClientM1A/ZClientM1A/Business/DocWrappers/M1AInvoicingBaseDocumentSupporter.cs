using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.M1A.Business
{
	public class M1AInvoicingBaseDocumentSupporter : InvoicingBaseDocumentSupporter
	{
		public M1AInvoicingBaseDocumentSupporter(InvoicingBase invoice)
			: base(invoice)
		{
		}

		public new static InvoicingBaseDocumentSupporter New(InvoicingBase invoice)
		{
			return (invoice == null) ? null : new M1AInvoicingBaseDocumentSupporter(invoice);
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

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.ARInvoice };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.ARInvoice)
			{
				return new DocumentWrapper[] { M1ADocARInvoice.New(Invoice, Factory) };
			}
			else
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}
	}
}
