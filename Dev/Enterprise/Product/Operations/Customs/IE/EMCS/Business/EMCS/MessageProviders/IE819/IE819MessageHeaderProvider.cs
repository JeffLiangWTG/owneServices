using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE819MessageHeaderProvider : MessageHeaderProvider<IE819HeaderProvider>, IEMCSMessageHeader
	{
		public IE819MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IAlertOrReject alertOrReject) : base(emcsJobDeclaration)
		{
			this.alertOrReject = Argument.NotNull(alertOrReject, nameof(alertOrReject));
		}
		readonly IAlertOrReject alertOrReject;

		new public string Recipient => "NDEA.IE";
		new public string Sender => "NDEA.IE";

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE819HeaderProvider(emcsJobDeclaration, alertOrReject));
	}
}
