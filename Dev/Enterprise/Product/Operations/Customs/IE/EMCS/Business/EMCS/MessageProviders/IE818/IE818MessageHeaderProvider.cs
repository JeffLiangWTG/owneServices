using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE818MessageHeaderProvider : MessageHeaderProvider<IE818HeaderProvider>, IEMCSMessageHeader
	{
		public IE818MessageHeaderProvider(EMCSJobDeclaration emcs, IReportOfReceipt reportOfReceipt) : base(emcs)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly IReportOfReceipt reportOfReceipt;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE818HeaderProvider(emcsJobDeclaration, reportOfReceipt));
	}
}
