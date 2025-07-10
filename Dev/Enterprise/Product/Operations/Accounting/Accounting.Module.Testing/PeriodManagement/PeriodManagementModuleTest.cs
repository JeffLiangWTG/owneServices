using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(PeriodManagementModule))]
	public class PeriodManagementModuleTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PeriodManagement;
		}

		public void TestOpenFormCacheIsUpdatedWhenModuleIsOpened()
		{
			using (var topLevelModule = new AccComplianceReportModule())
			using (topLevelModule.ShowPopup())
			{
				AssertEquals(1, OpenedFormCache.GetInstance().AdditionalFormsCount);

				using (var module = new PeriodManagementModule())
				using (module.ShowPopup())
				{
					AssertEquals(2, OpenedFormCache.GetInstance().AdditionalFormsCount);
				}
				AssertEquals(1, OpenedFormCache.GetInstance().AdditionalFormsCount);
			}
		}

		public void TestOpenFormCacheIsNotUpdatedWhenNewFormNotCreated()
		{
			using (var topLevelModule = new AccComplianceReportModule())
			using (topLevelModule.ShowPopup())
			{
				AssertEquals(1, OpenedFormCache.GetInstance().AdditionalFormsCount);

				using (var module = new PeriodManagementModule())
				{
					AssertEquals(1, OpenedFormCache.GetInstance().AdditionalFormsCount);
				}
				AssertEquals(1, OpenedFormCache.GetInstance().AdditionalFormsCount);
			}
		}

		public void TestSimulateAggregateButtonConfirmation_Yes()
		{
			AssertSimulateButtonCallsReAggregatorWithMessage(DialogResult.Yes, Times.Once());
		}

		public void TestSimulateAggregateButtonConfirmation_No()
		{
			AssertSimulateButtonCallsReAggregatorWithMessage(DialogResult.No, Times.Never());
		}

		public void TestAggregateAndReportAsXMLConfirmation_Yes()
		{
			AssertReportOnlyButtonCallsReAggregatorWithMessage(DialogResult.Yes, Times.Once());
		}

		public void TestAggregateAndReportAsXMLConfirmation_No()
		{
			AssertReportOnlyButtonCallsReAggregatorWithMessage(DialogResult.No, Times.Never());
		}

		#region Helpers

		void AssertSimulateButtonCallsReAggregatorWithMessage(DialogResult response, Times expectedTimes)
		{
			var mockReAggregatorRunner = new Mock<IAggregateRunner>();
			ObjectFactory.Substitute(mockReAggregatorRunner.Object);

			using (var module = new PeriodManagementModule())
			{
				using (var formForTest = module.EmbeddedControl)
				{
					formForTest.Show();

					UnitTestUserNotification.Instance.AddAnswer(response);
					formForTest.GetControl<ZButton>("SimulateButton").PerformClick();

					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
							@"Are you sure you want to re-aggregate transactions and send the Developer Message if there is any discrepancy?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."));

					mockReAggregatorRunner.Verify(r => r.ReAggregate(), expectedTimes);
				}
			}
		}

		void AssertReportOnlyButtonCallsReAggregatorWithMessage(DialogResult response, Times expectedTimes)
		{
			var mockReAggregatorRunner = new Mock<IAggregateRunner>();
			ObjectFactory.Substitute(mockReAggregatorRunner.Object);

			using (var module = new PeriodManagementModule())
			{
				using (var formForTest = module.EmbeddedControl)
				{
					formForTest.Show();

					UnitTestUserNotification.Instance.AddAnswer(response);
					formForTest.GetControl<ZButton>("ReportOnlyButton").PerformClick();

					Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
							@"Are you sure you want to re-aggregate transactions and generate result as XML?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."));

					mockReAggregatorRunner.Verify(r => r.ReAggregateAndReportAsXML(), expectedTimes);
				}
			}
		}

		#endregion
	}
}
