using System;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Aggregator.Testing
{
	class AggregatorQuestionTest : TestCaseWithFactory
	{
		public void TestQuestionToRunAggregation()
		{
			RunWithMainFormForDATsBenefit(() =>
			{
				Env.Security.TakeUpSubLedgerTranactions.IsAllowed = true;

				AggregateController controller = new AggregateController();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				controller.PerformAggregationIfRequired();
				AssertEquals("Shouldn't have populated registry item values for last aggregate", Guid.Empty, AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value);
				AssertEquals("Shouldn't have populated registry item values for last aggregate", DateTime.MinValue, AccountingConfigurationRegistry.Instance.LastAggregationDate.Value);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				controller.PerformAggregationIfRequired();
				AssertEquals("Should have populated registry item values for last aggregate", GlbStaff.CurrentUser.PK.ToGuid(), AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value);
				Assert("Should have populated registry item values for last aggregate", (ZDateTime.Now.ToDateTime() - AccountingConfigurationRegistry.Instance.LastAggregationDate.Value) < new TimeSpan(0, 0, 5, 0, 0));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				controller.PerformAggregationIfRequired();
				AssertEquals("Should have populated registry item values for last aggregate", GlbStaff.CurrentUser.PK.ToGuid(), AccountingConfigurationRegistry.Instance.LastAggregationStaff.Value);
				Assert("Should have populated registry item values for last aggregate", (ZDateTime.Now.ToDateTime() - AccountingConfigurationRegistry.Instance.LastAggregationDate.Value) < new TimeSpan(0, 0, 5, 0, 0));
			});
		}

		[TestDate(2014, 06, 06, 13, 0, 0)]
		public void TestMessagesWhenUserDoesNotHavePermissionForTakeup()
		{
			RunWithMainFormForDATsBenefit(() =>
			{
				Env.Security.TakeUpSubLedgerTranactions.IsAllowed = false;

				AggregateController controller = new AggregateController();

				controller.PerformAggregationIfRequired();

				var expectedMessage = @"You do not have the necessary security rights to perform GL take up. Please contact your system administrator to grant you the following security right:
Manage -> General Ledger -> Reports -> Take Up Sub Ledger Transactions

It is also possible to automatically run GL Take up via the service task 'ATU'. To enable that you need to setup the following registry: Accounting/Allow Scheduling Automatic Sub-Ledger (A/R & A/P) Take up";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				AutomaticProcessRegistryBusinessObject bizO = new AutomaticProcessRegistryBusinessObject(Factory);
				bizO.NextRunDateTime = ZDateTime.Now;
				bizO.IntervalType = "DAYS";
				bizO.Interval = 1;

				AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bizO);

				controller.PerformAggregationIfRequired();

				expectedMessage = @"You do not have the necessary security rights to perform GL take up. Please contact your system administrator to grant you the following security right:
Manage -> General Ledger -> Reports -> Take Up Sub Ledger Transactions

Your system is set to run an automatic GL Account take up on: 06-Jun-14 13:00:00";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestAggregateControllerGetTheLastUpdateDateCorrectly()
		{
			RunWithMainFormForDATsBenefit(() =>
			{
				Env.Security.TakeUpSubLedgerTranactions.IsAllowed = true;
				var oldTakeupDate = ZDateTime.MinSmallDateTimeValue.AddDays(1);
				var user = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code));
				AccountingConfigurationRegistry.Instance.LastAggregationStaff.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, user.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.LastAggregationDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldTakeupDate.ToDateTime());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var controller = new AggregateController();
				controller.PerformAggregationIfRequired();
				AssertContains("Should show the oldTakeupDate", oldTakeupDate.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);

				byte[] registryValue = new byte[] { 50, 0, 48, 0, 49, 0, 53, 0, 45, 0, 48, 0, 54, 0, 45, 0, 51, 0, 48, 0, 32, 0, 49, 0, 52, 0, 58, 0, 50, 0, 54, 0, 58, 0, 49, 0, 52, 0, 46, 0, 56, 0, 52, 0, 55, 0 }; // 30/06/2015 2:26:14

				string query = @"Update dbo.StmData  
										set SD_BinaryValue = @SD_BinaryValue
										WHERE SD_Owner = @SD_Owner 
										AND SD_Name = 'LastGLAggregateDate'";
				using (DbCommand command = Db.Connection.Command(query))
				{
					command.AddParameterBasedOnDbColumn("@SD_Owner", GlbCompany.CurrentCompany.PK.ToGuid(), StmDataSchema.SD_Owner);
					command.AddParameterBasedOnDbColumn("@SD_BinaryValue", registryValue, StmDataSchema.SD_BinaryValue);
					command.ExecuteNonQuery();
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				controller.PerformAggregationIfRequired();
				AssertNotContains("Should show the new take up date which is set to the registry", oldTakeupDate.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestNoProgressFormIsShown()
		{
			RunWithMainFormForDATsBenefit(() =>
			{
				Env.Security.TakeUpSubLedgerTranactions.IsAllowed = true;

				var controller = new AggregateController();

				ZFormModaliser.LastFormShownForTest = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				controller.PerformAggregationIfRequired();

				AssertNull("No form should be shown during aggregation", ZFormModaliser.LastFormShownForTest);
			});
		}

		#region Implementation

		void RunWithMainFormForDATsBenefit(Action action)
		{
			using (var mainForm = new ZForm())
			{
				mainForm.Show();

				ZFormModaliser.SetApplicationActiveForm(mainForm);
				try
				{
					action();
				}
				finally
				{
					mainForm.Close();
					ZFormModaliser.SetApplicationActiveForm(null);
				}
			}
		}

		#endregion
	}
}
