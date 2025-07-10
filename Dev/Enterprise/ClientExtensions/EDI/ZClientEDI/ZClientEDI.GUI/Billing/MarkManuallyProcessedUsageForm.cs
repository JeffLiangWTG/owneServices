using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public sealed partial class MarkManuallyProcessedUsageForm : ZChildForm
	{
		public MarkManuallyProcessedUsageForm()
		{
			InitializeComponent();
			var today = ZDateTime.Today;
			var lastMonth = new ZDateTime(today.Year, today.Month, 1).AddMonths(-1);
			EndDateEdit.DateTimeValue = lastMonth;
			StartDateEdit.DateTimeValue = lastMonth;
		}

		void MarkButton_Click(object sender, EventArgs e)
		{
			MarkOrUnmark(true);
		}

		public void UnmarkButton_Click(object sender, EventArgs e)
		{
			MarkOrUnmark(false);
		}

		void MarkOrUnmark(bool mark)
		{
			var codeText = CodeTextBox.Text.Trim();
			var codes = codeText.Replace(" ", "").Split(',');
			var orgText = OrgTextBox.Text.Trim();
			var orgs = orgText.Replace(" ", "").Split(',');

			if (!StartDateEdit.DateTimeValue.IsValid
				|| !EndDateEdit.DateTimeValue.IsValid
				|| StartDateEdit.DateTimeValue > EndDateEdit.DateTimeValue)
			{
				Globals.Message.ShowError("Date From/To must be entered and From must not be after To.");
			}
			else if (string.IsNullOrWhiteSpace(codeText))
			{
				Globals.Message.ShowError("Please enter at least one Usage Code");
			}
			else
			{
				int rowsAffected = MarkNonInvoiced(StartDateEdit.DateTimeValue, EndDateEdit.DateTimeValue, codes, orgs, mark);
				Globals.Message.ShowInformation("Rows affected: " + rowsAffected);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static int MarkNonInvoiced(ZDateTime from, ZDateTime to, string[] usageCodes, string[] orgCodes, bool mark = true)
		{
			using (var cmd = Db.Connection.Command(""))
			{
				cmd.AddParameterBasedOnDbColumn("@StartDate", from.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				cmd.AddParameterBasedOnDbColumn("@EndDate", to.ToDateTime(), ClientChargeableUsageSchema.U1_PeriodStart);
				cmd.AddParameterBasedOnDbColumn("@Mark", mark, ClientChargeableUsageSchema.U1_ManuallyProcessed);

				var sql = new StringBuilder();
				sql.Append("update dbo.ClientChargeableUsage set U1_ManuallyProcessed = @Mark" +
					" where U1_PeriodStart >= @StartDate\r\n" +
					" and U1_ManuallyProcessed != @Mark\r\n" +
					" and U1_PeriodStart <= @EndDate\r\n" +
					" and (U1_AH_Invoice is null or U1_AH_Invoice in (select AH_PK from dbo.AccTransactionHeader where AH_IsCancelled = 1))\r\n" +
					" and U1_Code in (");

				for (int i = 0; i < usageCodes.Length; ++i)
				{
					if (i != 0)
					{
						sql.Append(",");
					}
					string name = "@code" + i;
					sql.Append(name);
					cmd.AddParameterBasedOnDbColumn(name, usageCodes[i], ClientChargeableUsageSchema.U1_Code);
				}

				sql.Append(")\r\n");

				if (orgCodes != null && orgCodes.Length > 0 && orgCodes[0].Length > 0)
				{
					var orgListBuilder = new StringBuilder();
					for (int i = 0; i < orgCodes.Length; ++i)
					{
						if (i != 0)
						{
							orgListBuilder.Append(",");
						}
						string name = "@org" + i;
						orgListBuilder.Append(name);
						cmd.AddParameterBasedOnDbColumn(name, orgCodes[i], OrgHeaderSchema.OH_Code);
					}
					var orgList = orgListBuilder.ToString();

					sql.AppendLine(" and (");
					sql.Append("(U1_LCC in (select LCC_PK from dbo.EdiViewClientCompanyLicence where LC_OH in (select OH_PK from dbo.OrgHeader where OH_Code in (");
					sql.Append(orgList);
					sql.AppendLine("))))");

					sql.Append(" or ");
					sql.Append("(U1_LCC is null and U1_LD in (select LD_PK from dbo.EdiViewLicenceDatabaseOwner where LC_OH in (select OH_PK from dbo.OrgHeader where OH_Code in (");
					sql.Append(orgList);
					sql.AppendLine("))))");

					sql.Append(" or ");
					sql.Append("(U1_LCC is null and U1_LC in (select LC_PK from dbo.LicenceCompany where LC_OH in (select OH_PK from dbo.OrgHeader where OH_Code in (");
					sql.Append(orgList);
					sql.AppendLine("))))");

					sql.AppendLine(")");
				}

				sql.AppendLine(";");
				sql.Append("select @@rowcount");

				cmd.CommandText = sql.ToString();
				return (int)cmd.ExecuteScalar();
			}
		}
	}
}

