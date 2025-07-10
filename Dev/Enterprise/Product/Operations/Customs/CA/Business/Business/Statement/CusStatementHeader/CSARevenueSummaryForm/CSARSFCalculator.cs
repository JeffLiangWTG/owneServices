using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFCalculator
	{
		public CSARSFCalculator(CusStatementHeader cusStatementHeader)
		{
			this.cusStatementHeader = cusStatementHeader;
			startDate = cusStatementHeader.B2_PeriodStartDate;
			endDate = cusStatementHeader.B2_PeriodEndDate;
		}

		readonly CusStatementHeader cusStatementHeader;
		readonly ZDate startDate;
		readonly ZDate endDate;

		public bool CalculateRSF()
		{
			var result = false;
			if (cusStatementHeader.Importer != null && !startDate.IsEmpty && !endDate.IsEmpty)
			{
				cusStatementHeader.ResetCSFRSFData();
				GenerateTransactions();
				cusStatementHeader.CSARSFTransactions.Load();
				cusStatementHeader.Debits.CalculatePayments();
				cusStatementHeader.Credits.CalculatePayments();
				cusStatementHeader.InterimPayments.CalculatePayments();
				result = true;
			}
			return result;
		}

		void GenerateTransactions()
		{
			var declarations = cusStatementHeader.Factory.Load<JobDeclaration>(cusStatementHeader.GetCSARSFDeclarationQuery());
			CreateStatementLinesFromDeclarations(declarations);
		}

		void CreateStatementLinesFromDeclarations(JobDeclaration[] declarations)
		{
			foreach (var dec in declarations.Where(x => !x.CA_CSAEntry))
			{
				var line = cusStatementHeader.StatementLines.AddNew();
				line.B3_EntryType = dec.JE_MessageType;
				line.B3_EntryNum = dec.TransactionNumber;
				line.B3_EntryStatus = dec.IsB3X ? dec.JE_EntryStatus : dec.B3EntryHeader?.CH_EntryStatus ?? ZString.Empty;
				line.B3_BrokerReference = dec.JE_DeclarationReference;

				if (dec.JE_MessageType == JobMessageTypeList.Codes.Import)
				{
					var dutyCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._490101);
					dutyCharge.B4_ChargeAmount = dec.TotalNormalDuty;

					var gstCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._491211);
					gstCharge.B4_ChargeAmount = dec.TotalGST;

					var simaCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._49011);
					simaCharge.B4_ChargeAmount = dec.TotalSimaDuty;

					var exciseCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._49475);
					exciseCharge.B4_ChargeAmount = dec.TotalExciseTax;
				}
				else if (dec.JE_MessageType == JobMessageTypeList.Codes.XTypeEntry)
				{
					if (dec.TotalNormalDuty > 0)
					{
						var dutyCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._490102);
						dutyCharge.B4_ChargeAmount = dec.TotalNormalDuty;
					}
					else if (dec.TotalNormalDuty < 0)
					{
						var dutyCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFCreditCodes.Codes._49017);
						dutyCharge.B4_ChargeAmount = dec.TotalNormalDuty;
					}

					if (dec.TotalGST > 0)
					{
						var gstCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFDebitCodes.Codes._491212);
						gstCharge.B4_ChargeAmount = dec.TotalGST;
					}

					var simaCharge = line.GetOrCreateNewTransactionLineCharge(CSARSFCreditCodes.Codes._49018);
					simaCharge.B4_ChargeAmount = dec.TotalSimaDuty;
				}
			}
		}
	}
}
