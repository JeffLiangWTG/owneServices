using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(BulkCancelCommissionLinesForm))]
	public class BulkCancelCommissionLinesFormTest : ZFormBasherTest
	{
		#region Buttons

		public void TestRemoveErrorLinesButton()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			var action = new BulkCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine });

			using (var form = new BulkCancelCommissionLinesFormForTest(action))
			{
				form.Show();
				Application.DoEvents();

				form.RemoveErrorLinesButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Text", "All entity commissions can be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder("CancelCommissionLineActions", new[] { commissionLine }, action.CancelCommissionLineActions.Select(x => x.CommissionLine));
				});

				commissionLine.VCL_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.RemoveErrorLinesButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertContainsExactElementsInAnyOrder("CancelCommissionLineActions", Enumerable.Empty<CancelCommissionLineAction>(), action.CancelCommissionLineActions);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.RemoveErrorLinesButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Text", "No entity commissions have been selected.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder("CancelCommissionLineActions", Enumerable.Empty<CancelCommissionLineAction>(), action.CancelCommissionLineActions.Select(x => x.CommissionLine));
				});
			}
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var accCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var commissionLine = Factory.LoadTop1<ViewCommissionLine>(new ZQuery(ViewCommissionLineSchema.PK, accCommissionLine.PK));

			var action = new BulkCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine });
			using (var form = new BulkCancelCommissionLinesForm(action))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cancel Confirmation", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "You are about to cancel these entity commissions. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("IsCancelled", true, commissionLine.IsCancelled);
				});
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var bulkCancelAction = new BulkCancelCommissionLinesAction(Factory);
			bulkCancelAction.Initialise(new[] { Factory.New<ViewCommissionLine>() });
			return new BulkCancelCommissionLinesForm(bulkCancelAction);
		}

		#endregion
	}

	class BulkCancelCommissionLinesFormForTest : BulkCancelCommissionLinesForm
	{
		public BulkCancelCommissionLinesFormForTest(BulkCancelCommissionLinesAction action)
			: base(action)
		{
		}

		public ZButton RemoveErrorLinesButton_Exposed
		{
			get { return base.RemoveErrorLinesButton; }
		}
	}
}
