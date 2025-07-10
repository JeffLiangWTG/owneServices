using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE819MessageHeaderProvider : MessageHeaderProvider<IE819HeaderProvider>, IEMCSMessageHeader
	{
		public IE819MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IAlertOrReject alertOrReject) : base(emcsJobDeclaration)
		{
			this.alertOrReject = Argument.NotNull(alertOrReject, nameof(alertOrReject));
		}
		readonly IAlertOrReject alertOrReject;

		new public string Recipient => "NDEA.GB";
		new public string Sender => "NDEA.GB";

		IEMCSHeader IEMCSMessageHeader.Header => header ?? (header = new IE819HeaderProvider(emcsJobDeclaration, alertOrReject));
	}
}
