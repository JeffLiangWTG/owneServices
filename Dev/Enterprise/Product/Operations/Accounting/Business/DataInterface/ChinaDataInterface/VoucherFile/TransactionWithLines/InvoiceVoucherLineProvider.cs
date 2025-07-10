using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class InvoiceVoucherLineProvider : VoucherLineProvider
	{
		public InvoiceVoucherLineProvider(AccTransactionLines transactionLine)
			: base(transactionLine)
		{
		}

		protected override AccGLHeader GetGLAccountFromLine()
		{
			AccGLHeader gLAccount = null;
			if (TransactionLine.GLHeader != null)
			{
				gLAccount = TransactionLine.GLHeader;
			}
			else if (TransactionLine.AL_AC.IsValid)
			{
				if (TransactionLine.AL_LineType == TransactionLineTypes.Revenue)
				{
					gLAccount = TransactionLine.ChargeCode.RevenueAccount;
				}
				else if (TransactionLine.AL_LineType == TransactionLineTypes.Cost)
				{
					gLAccount = TransactionLine.ChargeCode.CostAccount;
				}
			}
			return gLAccount;
		}

		protected override ZString GetOrganisationCodeFromLine()
		{
			ZString result = ZString.Empty;

			if (IncludeOrganisationCode && TransactionLine != null &&
				TransactionLine.TransactionHeader != null && TransactionLine.TransactionHeader.Header != null)
			{
				result = " (" + TransactionLine.TransactionHeader.Header.OH_Code + ")";
			}

			return result;
		}
	}
}
