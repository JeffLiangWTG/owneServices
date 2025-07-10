using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSFECAmendmentDeclarationEDIMessagePrettier : CDSAmendDeclarationEDIMessagePrettier
	{
		public CDSFECAmendmentDeclarationEDIMessagePrettier(CDSAmendDeclarationEDIMessage message, JobDeclarationMessageSendingObject messageSendingObject) : base(message, messageSendingObject)
		{
		}

		protected override ZString MessageTitle => "FEC Confirmation Request";
	}
}
