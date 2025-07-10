using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE818MessageHeaderProvider : MessageHeaderProvider<IE818HeaderProvider>, IEMCSMessageHeader
	{
		public IE818MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IReportOfReceipt reportOfReceipt) : base(emcsJobDeclaration)
		{
			this.reportOfReceipt = Argument.NotNull(reportOfReceipt, nameof(reportOfReceipt));
		}
		readonly IReportOfReceipt reportOfReceipt;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE818HeaderProvider(emcsJobDeclaration, reportOfReceipt));
	}
}
