using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Aggregator.Testing
{
	[TestedType(typeof(ReAggregateSinglePeriodForm))]
	public class ReAggregateSinglePeriodFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			SinglePeriodReaggregator reAggregator = new SinglePeriodReaggregator(Factory);
			ReAggregateSinglePeriodForm result = new ReAggregateSinglePeriodForm(reAggregator);
			return result;
		}

		public void TestExecuteButtonConfirmation_Yes()
		{
			AssertExecuteButtonCallsReAggregatorWithMessage(DialogResult.Yes, 1);
		}

		public void TestExecuteButtonConfirmation_No()
		{
			AssertExecuteButtonCallsReAggregatorWithMessage(DialogResult.No, 0);
		}

		#region Helpers

		void AssertExecuteButtonCallsReAggregatorWithMessage(DialogResult response, int expectedTimes)
		{
			var mockReAggregator = new SinglePeriodReAggregatorForTest(Factory);

			using (var formForTest = new ReAggregateSinglePeriodForm(mockReAggregator))
			{
				formForTest.Show();

				UnitTestUserNotification.Instance.AddAnswer(response);
				formForTest.GetControl<ZButton>("ExecuteButton").PerformClick();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
						@"Are you sure you want to re-aggregate this company and period?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."));

				AssertEquals(expectedTimes, mockReAggregator.TimesCalled);
			}
		}

		class SinglePeriodReAggregatorForTest : SinglePeriodReaggregator
		{
			public SinglePeriodReAggregatorForTest(BusinessObjectFactory factory) : base(factory)
			{
				TimesCalled = 0;
			}

			public int TimesCalled;

			override public void ReAggregate()
			{
				TimesCalled++;
			}
		}

		#endregion
	}
}
