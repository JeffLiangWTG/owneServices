using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Aggregator
{
	[SuppressControlRequiresTextBasher]
	public partial class ReAggregateSinglePeriodForm : ZChildForm
	{
		public ReAggregateSinglePeriodForm()
			: base()
		{
			InitializeComponent();
		}

		public ReAggregateSinglePeriodForm(SinglePeriodReaggregator aggregator)
			: base(aggregator)
		{
			this.ReAggregator = aggregator;
		}

		readonly SinglePeriodReaggregator ReAggregator;

		void ExecuteButton_Click(object sender, EventArgs e)
		{
			string message = Res.GetString("806d7fef-bfb1-47d5-8361-6272b60d8acc", @"Are you sure you want to re-aggregate this company and period?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this.");
			if (Globals.Message.Show(message, "ReAggregate", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				ReAggregator.ReAggregate();
				Globals.Message.ShowInformation(string.Format((NoResString)"Finished ReAggregation"), "ReAggregate"); // developers only string
			}
		}

		void zButton2_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
