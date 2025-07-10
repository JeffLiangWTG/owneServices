using System;

using Enterprise.Accounting.Business.Aggregator;

using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.Aggregator
{
	public partial class ExpectedAggregationResultsForm : ZChildForm
	{
		public ExpectedAggregationResultsForm()
			: base()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(this);
		}

		public ExpectedAggregationResultsForm(AggregationDiscrepanciesCalculator aggregator)
			: base(aggregator)
		{
			this.ReAggregator = aggregator;
			MissingResourceStringChecker.ExcludeFromTest(this);
		}

		readonly AggregationDiscrepanciesCalculator ReAggregator;

		void ExecuteButton_Click(object sender, EventArgs e)
		{
			ReAggregator.CalculateAggregationDiscrepancies();
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
