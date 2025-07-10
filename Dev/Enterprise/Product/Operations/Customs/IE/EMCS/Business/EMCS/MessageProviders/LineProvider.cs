using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class LineProvider : IEMCSLine
	{
		public LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine)
		{
			this.emcsInvoiceLine = Argument.NotNull(emcsInvoiceLine, nameof(emcsInvoiceLine));
			lineProviderHelper = new LineProviderHelper(emcsInvoiceLine);
		}
		readonly LineProviderHelper lineProviderHelper;

		public int LineNumber => lineProviderHelper.LineNumber;

		public string ExciseProductCode => lineProviderHelper.ExciseProductCode;

		protected readonly EMCSJobComInvoiceLine emcsInvoiceLine;
	}
}
