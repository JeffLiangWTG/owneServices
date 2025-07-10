namespace Enterprise.Customs.AE.Business;

public partial class PaymentMethodList
{
	public void SortNumerically()
	{
		Elements.Clear();
		AddPair(Codes.CashPayment, Descriptions.CashPayment);
		AddPair(Codes.TelexTransfer, Descriptions.TelexTransfer);
		AddPair(Codes.LetterOfCredit, Descriptions.LetterOfCredit);
		AddPair(Codes.ElectronicPayment, Descriptions.ElectronicPayment);
		AddPair(Codes.Others, Descriptions.Others);
		AddPair(Codes.Draft, Descriptions.Draft);
		AddPair(Codes.BankTransfer, Descriptions.BankTransfer);
		AddPair(Codes.DocAgainstAccept, Descriptions.DocAgainstAccept);
		AddPair(Codes.DocAgainstPayment, Descriptions.DocAgainstPayment);
		AddPair(Codes.Courier, Descriptions.Courier);
	}
}
