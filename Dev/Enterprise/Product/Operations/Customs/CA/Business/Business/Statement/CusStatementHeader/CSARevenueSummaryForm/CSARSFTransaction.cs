using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFTransaction : AutoCSARSFTransaction
	{
		public CSARSFTransaction(CusStatementLine cusStatementLine)
			: base(cusStatementLine.Factory)
		{
			this.cusStatementLine = cusStatementLine;
			if (cusStatementLine.B3_EntryType == JobMessageTypeList.Codes.Import)
			{
				this.dutyCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._490101, false);
				this.gstCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._491211, false);
				this.simaCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._49011, false);
				this.exciseCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._49475, false);
			}
			else if (cusStatementLine.B3_EntryType == JobMessageTypeList.Codes.XTypeEntry)
			{
				this.dutyCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._490102, false)
					?? cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFCreditCodes.Codes._49017, false);
				this.gstCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._491212, false);
				this.simaCharge = cusStatementLine.GetOrCreateNewTransactionLineCharge(CSARSFCreditCodes.Codes._49018, false);
				this.exciseCharge = null;
			}
		}

		readonly CusStatementLine cusStatementLine;
		readonly CusStatementLineCharge dutyCharge;
		readonly CusStatementLineCharge gstCharge;
		readonly CusStatementLineCharge simaCharge;
		readonly CusStatementLineCharge exciseCharge;

		public ZString DocumentType
		{
			get => cusStatementLine?.B3_EntryType ?? ZString.Empty;
		}

		public ZString TransactionNumber
		{
			get => cusStatementLine?.B3_BrokerReference ?? ZString.Empty;
		}

		public ZString EntryStatus
		{
			get => cusStatementLine?.B3_EntryStatus ?? ZString.Empty;
		}

		public ZDecimal Duties
		{
			get => (dutyCharge?.B4_ChargeAmount ?? ZDecimal.Zero).Round(2);
		}

		public ZDecimal GST
		{
			get => (gstCharge?.B4_ChargeAmount ?? ZDecimal.Zero).Round(2);
		}

		public ZDecimal SIMA
		{
			get => (simaCharge?.B4_ChargeAmount ?? ZDecimal.Zero).Round(2);
		}

		public ZDecimal Excise
		{
			get => (exciseCharge?.B4_ChargeAmount ?? ZDecimal.Zero).Round(2);
		}

		public ZDecimal Total
		{
			get => Duties + GST + SIMA + Excise;
		}
	}
}
