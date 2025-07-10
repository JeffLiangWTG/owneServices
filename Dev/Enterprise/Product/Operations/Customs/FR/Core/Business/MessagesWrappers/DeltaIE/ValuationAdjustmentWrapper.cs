using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ValuationAdjustmentWrapper : IValuationAdjustment
	{
		ValuationAdjustmentWrapper(JobComInvoiceLine line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}

		public static ValuationAdjustmentWrapper New(JobComInvoiceLine line) => line == null ? null : new ValuationAdjustmentWrapper(line);

		public string ValuationIndicators
		{
			get
			{
				if (valuationIndicators == null)
				{
					valuationIndicators = ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(line.InvoiceHeader?.RelatedIndicator ?? false, line.JI_RelatedIndicator)
										+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(line.InvoiceHeader?.RelatedIndicator2 ?? false, line.ZG_RelatedIndicator2)
										+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(line.InvoiceHeader?.RelatedIndicator3 ?? false, line.ZG_RelatedIndicator3)
										+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(line.InvoiceHeader?.RelatedIndicator4 ?? false, line.ZG_RelatedIndicator4);
				}
				return valuationIndicators;
			}
		}
		string valuationIndicators;

		readonly JobComInvoiceLine line;
	}
}
