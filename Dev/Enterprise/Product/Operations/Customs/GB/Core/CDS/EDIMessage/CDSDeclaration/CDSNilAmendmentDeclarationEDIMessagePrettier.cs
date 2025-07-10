using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSNilAmendmentDeclarationEDIMessagePrettier : CDSAmendDeclarationEDIMessagePrettier
	{
		public CDSNilAmendmentDeclarationEDIMessagePrettier(CDSAmendDeclarationEDIMessage message, JobDeclarationMessageSendingObject messageSendingObject) : base(message, messageSendingObject)
		{
		}

		protected override ZString MessageTitle => "Nil Amendment Request";
	}
}
