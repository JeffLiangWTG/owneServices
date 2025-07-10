namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message871HeaderProviderHelper : HeaderProviderHelper
	{
		public Message871HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration) : base(emcsJobDeclaration)
		{
		}

		public string SubmitterType => emcsJobDeclaration.JE_DeclarantType;

		public bool CanSendLine(EMCSJobComInvoiceLine invoiceLine) => !invoiceLine.Outturn.ObservedDifference.IsEmpty && !invoiceLine.Outturn.C5_OutturnResultReason.IsEmpty;
	}
}
