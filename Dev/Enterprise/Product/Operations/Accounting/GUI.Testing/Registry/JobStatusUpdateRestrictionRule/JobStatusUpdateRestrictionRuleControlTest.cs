using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(JobStatusUpdateRestrictionRuleControl))]
	public class JobStatusUpdateRestrictionRuleControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new JobStatusUpdateRestrictionRuleCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var jobStatusUpdateRestrictionRuleControl = control as JobStatusUpdateRestrictionRuleControl;
			AssertNotNull(jobStatusUpdateRestrictionRuleControl);

			var grid = jobStatusUpdateRestrictionRuleControl.FindSingleOrDefault<ZGrid>("JobStatusUpdateRestrictionRuleGrid");
			AssertNotNull(grid);

			return grid.ReadOnly;
		}

		public void TestGridColumns()
		{
			var columns = new string[]
			{
				"JobStatusDescription",
				"RelatedSecurityRightDescription",
				"Working",
				"WorkOnHold",
				"InvoiceOnHold",
				"CustomsProcessActive",
				"JobReadyForRevenueAndCostPosting",
				"JobReadyForRevenuePosting",
				"JobReadyForCostPosting",
				"JobReadyForDelivery",
				"JobInvoiced",
				"Complete",
				"JobReadyForFinancialClosure",
				"ScheduledForArchive"
			};

			var columnCaptions = new string[]
			{
				"Job Status",
				"Related Security Right",
				"Working",
				"Work on Hold",
				"Invoice on Hold",
				"Customs Processing Active",
				"Job Ready for Revenue and Cost Posting",
				"Job Ready for Revenue Posting",
				"Job Ready for Cost Posting",
				"Job Ready for Delivery",
				"Job Invoiced",
				"Complete",
				"Job Ready for Financial Closure",
				"Schedule for Archive"
			};

			var columnShortCaptions = new string[]
			{
				"",
				"",
				"WRK",
				"WHL",
				"IHL",
				"CUS",
				"JRA",
				"JRB",
				"JRC",
				"RDD",
				"INV",
				"CMP",
				"JFC",
				"ARC"
			};

			var rule = new JobStatusUpdateRestrictionRule();
			using (var form = new ZForm(rule))
			{
				var control = new JobStatusUpdateRestrictionRuleControl();
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingleOrDefault<ZGrid>("JobStatusUpdateRestrictionRuleGrid");
				AssertNotNull("JobStatusUpdateRestrictionRuleGrid", grid);
				AssertEquals(columns.Length, grid.ColumnStyles.Count);

				for (int i = 0; i < grid.ColumnStyles.Count; i++)
				{
					var columnInfo = (ZGridColumnInfo)grid.ColumnStyles[i];
					AssertEquals($"{columnInfo.ColumnName} should be present", columns[i], columnInfo.ColumnName);
					AssertEquals($"Caption", columnCaptions[i], columnInfo.CaptionResourceString.Caption);
					AssertEquals($"Short Caption", columnShortCaptions[i], columnInfo.CaptionResourceString.ShortCaption);
					AssertEquals($"{columnInfo.Caption} should be visibility", true, columnInfo.IsVisible);
				}
			}
		}
	}
}
