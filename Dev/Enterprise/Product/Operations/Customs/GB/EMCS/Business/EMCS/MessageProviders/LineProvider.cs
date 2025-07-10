using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class LineProvider : IEMCSLine
	{
		public LineProvider(EMCSJobComInvoiceLine emcsInvoiceLine)
		{
			this.emcsInvoiceLine = Argument.NotNull(emcsInvoiceLine, nameof(emcsInvoiceLine));
		}

		public int LineNumber => emcsInvoiceLine.JI_LineNo;

		public string ExciseProductCode => emcsInvoiceLine.ZG_ExciseProductCode;

		protected readonly EMCSJobComInvoiceLine emcsInvoiceLine;
	}
}
