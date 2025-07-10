using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class RecoverPartOfGldPeriodsDateRangeForm : GldDateRangeForm<RecoverPartOfGldPeriodsDateRangeSetting>
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RecoverPartOfGldPeriodsDateRangeForm() : base()
		{
		}

		public RecoverPartOfGldPeriodsDateRangeForm(RecoverPartOfGldPeriodsDateRangeSetting recoverPartOfGldPeriodsDateRangeSetting) : base(recoverPartOfGldPeriodsDateRangeSetting)
		{
		}

		protected override void OverrideInit()
		{
			CaptionResourceString = Res.GetData("RecoverPartOfGldPeriodsDateRangeForm|765D3FD5-C12F-489F-8939-FA3EC7C265D9", "Regenerate GLD (This Company Only)");
			OKButton.CaptionResourceString = Res.GetData("RecoverPartOfGldPeriodsDateRangeForm|38951D17-65AC-488E-AD1F-74CB9B4438A2", "Continue");
		}

		protected override void EventForOk(RecoverPartOfGldPeriodsDateRangeSetting recoverPartOfGldPeriodsDateRangeSetting)
		{
			if (!ConfirmInvalidateFinalisedComplianceReport(recoverPartOfGldPeriodsDateRangeSetting))
			{
				return;
			}

			var message = Res.GetString("RecoverPartOfGldPeriodsDateRangeForm|788CF952-7DB8-48D0-A3EF-1B9F69E6E759"
				, "Are you sure you want to Delete and Regenerate all General Ledger Data in specified date range for Current Company [{0}]?"
				, GlbCompany.CurrentCompany.CompanyName
			);

			var confirmingResult = Globals.Message.Show(message, base.FormCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (confirmingResult == DialogResult.Yes)
			{
				Db.Connection.RunInTransaction(() =>
				{
					GeneralLedgerDataQueue.RemoveNarrowGLD(Db.Connection
						, companyPK: Env.CurrentCompanyPK
						, startDate: recoverPartOfGldPeriodsDateRangeSetting.StartDate.Date.ToDateTime()
						, endDate: recoverPartOfGldPeriodsDateRangeSetting.EndDate.Date.AddDays(1).ToDateTime()
					);

					GeneralLedgerDataQueue.QueueTransaction(Db.Connection
						, companyPK: Env.CurrentCompanyPK
						, startDate: recoverPartOfGldPeriodsDateRangeSetting.StartDate.Date.ToDateTime()
						, endDate: recoverPartOfGldPeriodsDateRangeSetting.EndDate.Date.AddDays(1).ToDateTime()
						, endSystemCreateTime: DateTime.MaxValue
						, isCheckDuplicated: true
					);
				});

				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("GLP");

				Globals.Message.Show(Res.GetString("88DF94F0-1170-42A7-9CD0-6CBB5DD182BB", "General Ledger Data (GLD) for Period has been been deleted for {0} to {1}, it will be regenerated after the next Service Task cycle."
					, recoverPartOfGldPeriodsDateRangeSetting.StartDate.Date.ToISO8601ShortDateString()
					, recoverPartOfGldPeriodsDateRangeSetting.EndDate.Date.ToISO8601ShortDateString()));
			}
		}

		bool ConfirmInvalidateFinalisedComplianceReport(RecoverPartOfGldPeriodsDateRangeSetting recoverPartOfGldPeriodsDateRangeSetting)
		{
			var sql = $@"
SELECT TOP 1 ACL_ACR_Report FROM dbo.AccComplianceReportTransactionPivot
	JOIN dbo.AccGeneralLedgerData ON ACL_ParentID = GLD_PK
	JOIN dbo.AccComplianceReport ON ACL_ACR_Report = ACR_PK
WHERE GLD_GC_Company = @CompanyPK
	AND GLD_PostDate >= @StartDate
	AND GLD_PostDate < @EndDate
	AND ACR_Status = 'FIN'
";
			var result = Db.Connection.ExecuteScalar(sql, command =>
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.AddParameter("@StartDate", SqlDbType.DateTime, recoverPartOfGldPeriodsDateRangeSetting.StartDate.Date.ToDateTime());
				command.AddParameter("@EndDate", SqlDbType.DateTime, recoverPartOfGldPeriodsDateRangeSetting.EndDate.Date.ToDateTime());
			});

			return result == null || AccountingMessageHelper.ConfirmInvalidateFinalisedComplianceReport(base.FormCaption);
		}

		IGeneralLedgerDataQueue GeneralLedgerDataQueue => generalLedgerDataQueue ?? (generalLedgerDataQueue = ObjectFactory.Get<IGeneralLedgerDataQueue>());
		IGeneralLedgerDataQueue generalLedgerDataQueue;
	}
}
