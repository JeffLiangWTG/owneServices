using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface ICFEHelper
	{
		(string serie, string numero) GetInvoiceSerieAndNumber(ZString transactionReference);
	}

	class CFEHelper : ICFEHelper
	{
		(string serie, string numero) ICFEHelper.GetInvoiceSerieAndNumber(ZString transactionReference)
		{
			var invoiceSerie = ZString.Empty;
			var i = 0;

			for (; i <= transactionReference.Length - 1 && !char.IsDigit(transactionReference[i]); i++)
			{
				invoiceSerie += transactionReference[i];
			}

			return (invoiceSerie, transactionReference.Substring(i, transactionReference.Length - i));
		}
	}
}
