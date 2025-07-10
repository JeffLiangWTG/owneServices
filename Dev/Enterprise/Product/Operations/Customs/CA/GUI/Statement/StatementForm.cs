using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI
{
	public partial class StatementForm : ZTemplateForm, IPostingButtonsProvider
	{
		public StatementForm(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
			this.statementHeader = statementHeader;
			statementHeader.ReadOnly = true;

			WorkflowTabPage.Initialize(statementHeader);

			if (!statementHeader.B2_IsMonthlyStatement)
			{
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
			}
		}

		readonly CusStatementHeader statementHeader;

		public override string FormCaption => statementHeader.HumanReadableName;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var header = dataSource as CusStatementHeader;
			if (header != null)
			{
				SetColumnVisible(header);

				var isMonthyStatement = header.B2_IsMonthlyStatement;

				DailyStatementTabPage.TabVisible = isMonthyStatement;

				ChargesGroupBox.Visible = !isMonthyStatement;
				TransactionsSplitter.Visible = !isMonthyStatement;
			}
		}

		void SetColumnVisible(CusStatementHeader header)
		{
			var columnsForLineGrid = new List<string>();
			var columnsForLineGroupGrid = new List<string>();
			if (header.B2_IsMonthlyStatement)
			{
				columnsForLineGrid = new List<string>
				{
					CusStatementLineSchema.Constants.B3_EntryDate,
					CusStatementLineSchema.Constants.B3_EntryProcessPort,
					CusStatementLineSchema.Constants.B3_BrokerReference,
					CusStatementLine.Schema.B4_ChargeAmountDTY,
					CusStatementLine.Schema.B4_ChargeAmountEXS,
					CusStatementLine.Schema.B4_ChargeAmountGSTOrGSD,
					CusStatementLine.Schema.B4_ChargeAmountSIM,
					CusStatementLine.Schema.B4_ChargeAmountOTH,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_Duties,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_ExciseTax,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_ExciseDuties,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_GSTAndHSTAndPST,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_Interests,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_Others,
					CusStatementLine.Schema.B4_CARMDNChargeAmount_SIMA,
					nameof(CusStatementLine.PaymentMethod),
					CusStatementLine.Schema.ImporterCode,
					CusStatementLine.Schema.ImporterName,
					CusStatementLineSchema.Constants.B3_ScheduledProcessDate
				};

				columnsForLineGroupGrid = new List<string>
				{
					nameof(CusStatementLineGroup.B10_TotalPaymentReceived),
					nameof(CusStatementLineGroup.PaymentMethod),
					nameof(CusStatementLineGroup.B10_ImporterName),
					nameof(CusStatementLineGroup.Total),
					nameof(CusStatementLineGroup.TotalPayableByBrokerOnDailyStatement),
					nameof(CusStatementLineGroup.TotalPayableByImporterOnDailyStatement),
					nameof(CusStatementLineGroup.TotalInterests),
					nameof(CusStatementLineGroup.TotalExciseDuties),
				};
			}
			else
			{
				columnsForLineGrid = new List<string>
				{
					CusStatementLine.Schema.EntryStatusDescription,
					CusStatementLineSchema.Constants.B3_EIIndicator,
					CusStatementLineSchema.Constants.B3_CreditNote,
					CusStatementLineSchema.Constants.B3_CreditNoteDate,
				};
				columnsForLineGroupGrid = new List<string>
				{
					nameof(CusStatementLineGroup.B10_PreviousMonthlyStatementTotal),
					nameof(CusStatementLineGroup.B10_PaymentReceivedSinceLastMonthlyStatement),
					nameof(CusStatementLineGroup.B10_Refund),
					nameof(CusStatementLineGroup.B10_UnpaidBalanceForward),
					nameof(CusStatementLineGroup.B10_ArrearsInterest),
					nameof(CusStatementLineGroup.B10_TransactionTotal),
					nameof(CusStatementLineGroup.B10_OtherCharges),
					nameof(CusStatementLineGroup.B10_TotalCredits),
					nameof(CusStatementLineGroup.B10_InterestAmount),
					nameof(CusStatementLineGroup.B10_GIPastTotal),
					nameof(CusStatementLineGroup.B10_GICurrentTotal),
					nameof(CusStatementLineGroup.B10_TotalPayableForImporterSoAStatement),
					nameof(CusStatementLineGroup.B10_TotalPayableForBrokerSoAStatement),
				};

				if (header.IsCARMDailyNotice)
				{
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_ChargeAmountDTY);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_ChargeAmountEXS);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_ChargeAmountGSTOrGSD);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_ChargeAmountSIM);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_ChargeAmountOTH);
				}
				else
				{
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_Duties);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_ExciseTax);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_ExciseDuties);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_GSTAndHSTAndPST);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_Interests);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_Others);
					columnsForLineGrid.Add(CusStatementLine.Schema.B4_CARMDNChargeAmount_SIMA);

					columnsForLineGroupGrid.Add(nameof(CusStatementLineGroup.TotalInterests));
					columnsForLineGroupGrid.Add(nameof(CusStatementLineGroup.TotalExciseDuties));
				}
			}

			StatementLinesGrid.RemoveFromAvailableColumns(columnsForLineGrid.ToArray());
			LineGroupGrid.RemoveFromAvailableColumns(columnsForLineGroupGrid.ToArray());
		}

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		#endregion
	}
}
