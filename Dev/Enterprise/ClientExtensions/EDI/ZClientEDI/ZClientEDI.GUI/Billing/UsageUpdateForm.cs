using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public sealed partial class UsageUpdateForm : ZChildForm
	{
		public UsageUpdateForm()
		{
			InitializeComponent();
			var today = ZDateTime.Today;
			var lastMonth = new ZDateTime(today.Year, today.Month, 1).AddMonths(-1);
			var periodText = (lastMonth.Year * 100 + lastMonth.Month).ToString(CultureInfo.InvariantCulture);
			periodTextBox.Text = periodText;
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			var usageCode = UsageCodeTextBox.Text.Trim().ToUpperInvariant();
			var priceCode = PriceCodeTextBox.Text.Trim().ToUpperInvariant();
			var shouldUpdateConsolidation = updateConsolidationCheckBox.Checked;

			var periodText = periodTextBox.Text.Trim();
			int keyRefIndex = 0;
			if (!ZDateTime.TryParseExact(periodText, out var startDate, "yyyyMM"))
			{
				Globals.Message.ShowError("Period not valid.");
			}
			else if (!int.TryParse(keyRefIndexTextBox.Text.Trim(), out keyRefIndex))
			{
				Globals.Message.ShowError("Key Ref Index not valid.");
			}
			else if (string.IsNullOrWhiteSpace(usageCode))
			{
				Globals.Message.ShowError("Please enter Usage Code");
			}
			else if (shouldUpdateConsolidation && !EDIDataRegistry.Instance.ConsolidatedBillingSettings.Value.OfType<ConsolidatedBillingSetting>().Any(x => x.ProductCode.EqualsIgnoringCase(usageCode)))
			{
				Globals.Message.ShowError($"This Usage Code cannot be used for Consolidation. Please check: {EDIDataRegistry.Instance.ConsolidatedBillingSettings.HumanReadableRegistryPath()}");
			}
			else
			{
				try
				{
					using (new ZWaitCursorChanger(this))
					{
						Update(startDate, usageCode, priceCode, keyRefIndex, shouldUpdateConsolidation);
					}
					Globals.Message.ShowInformation("Update complete.");
					Close();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.ToString());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void Update(ZDateTime firstDayOfMonth, string usageCode, string priceCode, int keyRefIndex, bool shouldUpdateConsolidation)
		{
			var billingPeriod = new BillingPeriod(firstDayOfMonth);

			if (shouldUpdateConsolidation)
			{
				using (var cmd = Db.Connection.Command("EdiUpdateLicenceDatabaseConsolidation"))
				{
					const int TimeoutMinutes = 60;
					cmd.CommandTimeout = TimeoutMinutes * 60;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Period", SqlDbType.Int, (int)billingPeriod.Period);
					cmd.AddParameter("@ProductCodes", SqlDbType.VarChar, 3, usageCode);
					cmd.ExecuteProcedureWithReturnValue();
				}
			}

			using (var cmd = Db.Connection.Command("EdiChargeableUsageUpdate"))
			{
				const int TimeoutMinutes = 60;
				cmd.CommandTimeout = TimeoutMinutes * 60;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FirstDayOfMonth", SqlDbType.SmallDateTime, billingPeriod.PeriodStartDate.ToDateTime());
				cmd.AddParameter("@PeriodStartTimeUtc", SqlDbType.SmallDateTime, billingPeriod.StartTimeUtc.ToDateTime());
				cmd.AddParameter("@PeriodEndTimeUtc", SqlDbType.SmallDateTime, billingPeriod.EndTimeUtc.ToDateTime());
				cmd.AddParameter("@Code", SqlDbType.VarChar, 3, usageCode);
				cmd.AddParameter("@BillingDbPriceItemCode", SqlDbType.VarChar, 3, priceCode);
				cmd.AddParameter("@BillingDbKeyRefIndex1", SqlDbType.TinyInt, keyRefIndex);
				cmd.ExecuteProcedureWithReturnValue();
			}
		}

		void PriceCodeTextBox_TextChanged(object sender, EventArgs e)
		{
			var billingDbCodesList = EDIDataRegistry.Instance.BillingDbUsageCodesList.Value;
			var usageCode = UsageCodeTextBox.Text.Trim().ToUpperInvariant();
			var priceCode = PriceCodeTextBox.Text.Trim().ToUpperInvariant();
			foreach (BillingDbUsageCodes item in billingDbCodesList)
			{
				if (item.Category == usageCode && item.PriceItemCode == priceCode)
				{
					keyRefIndexTextBox.Text = item.KeyRefIndex1.ToString();
					return;
				}
			}

			keyRefIndexTextBox.Text = "0";
		}
	}
}

