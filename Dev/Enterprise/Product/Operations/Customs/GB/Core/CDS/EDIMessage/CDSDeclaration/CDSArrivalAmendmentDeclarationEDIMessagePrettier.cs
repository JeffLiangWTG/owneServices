using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSArrivalAmendmentDeclarationEDIMessagePrettier : CDSAmendDeclarationEDIMessagePrettier
	{
		public CDSArrivalAmendmentDeclarationEDIMessagePrettier(CDSArrivalAmendmentDeclarationEDIMessage message, JobDeclarationMessageSendingObject messageSendingObject) : base(message, messageSendingObject)
		{
		}

		protected override ZString MessageTitle => "Notification of arrival of goods";
	}
}
