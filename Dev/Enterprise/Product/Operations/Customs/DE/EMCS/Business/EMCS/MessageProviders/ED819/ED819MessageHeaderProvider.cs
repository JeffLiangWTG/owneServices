using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED819MessageHeaderProvider : MessageHeaderProvider<ED819HeaderProvider>, IEMCSMessageHeader
	{
		public ED819MessageHeaderProvider(EMCSJobDeclaration emcs, IAlertOrReject alertOrReject)
			: base(emcs)
		{
			this.alertOrReject = Argument.NotNull(alertOrReject, nameof(alertOrReject));
		}
		readonly IAlertOrReject alertOrReject;

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new ED819HeaderProvider(emcsJobDeclaration, alertOrReject));
	}
}
