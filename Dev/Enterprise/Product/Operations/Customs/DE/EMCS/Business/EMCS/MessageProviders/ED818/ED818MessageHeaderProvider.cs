using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED818MessageHeaderProvider : MessageHeaderProvider<ED818HeaderProvider>, IEMCSMessageHeader
	{
		public ED818MessageHeaderProvider(EMCSJobDeclaration emcs, IReportOfReceipt reportOfReceipt)
			: base(emcs)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly IReportOfReceipt reportOfReceipt;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new ED818HeaderProvider(emcsJobDeclaration, reportOfReceipt));
	}
}
