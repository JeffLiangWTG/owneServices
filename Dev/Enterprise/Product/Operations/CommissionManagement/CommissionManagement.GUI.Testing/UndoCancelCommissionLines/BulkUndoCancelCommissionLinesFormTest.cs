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
	[TestedType(typeof(BulkUndoCancelCommissionLinesForm))]
	public class BulkUndoCancelCommissionLinesFormTest : ZFormBasherTest
	{
		#region Buttons

		public void TestRemoveErrorLinesButton()
		{
			var commissionLine = Factory.New<ViewCommissionLine>();
			commissionLine.VCL_CancelledDateTimeUtc = ZDateTime.Today;
			var action = new BulkUndoCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine });
			using (var form = new BulkUndoCancelCommissionLinesFormForTest(action))
			{
				form.Show();
				Application.DoEvents();

				form.RemoveErrorLinesButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Text", "All entity commissions can be undo canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder("UndoCancelCommissionLineActions", new[] { commissionLine }, action.UndoCancelCommissionLineActions.Select(x => x.CommissionLine));
				});

				commissionLine.VCL_CancelledDateTimeUtc = ZDateTime.Empty;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.RemoveErrorLinesButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertContainsExactElementsInAnyOrder("UndoCancelCommissionLineActions", Enumerable.Empty<UndoCancelCommissionLineAction>(), action.UndoCancelCommissionLineActions);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.RemoveErrorLinesButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Text", "No entity commissions have been selected.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder("UndoCancelCommissionLineActions", Enumerable.Empty<UndoCancelCommissionLineAction>(), action.UndoCancelCommissionLineActions.Select(x => x.CommissionLine));
				});
			}
		}

		#endregion

		#region Delete

		public void TestUndoCancel()
		{
			var accCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			accCommissionLine.CL0_CancelledDateTimeUtc = ZDateTime.Today;
			Factory.Save();

			var commissionLine = Factory.LoadTop1<ViewCommissionLine>(new ZQuery(ViewCommissionLineSchema.PK, accCommissionLine.PK));

			var action = new BulkUndoCancelCommissionLinesAction(Factory);
			action.Initialise(new[] { commissionLine });

			using (var form = new BulkUndoCancelCommissionLinesForm(action))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Undo Cancel Confirmation", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "You are about to undo cancel these entity commissions. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("IsCancelled", false, commissionLine.IsCancelled);
				});
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var bulkUndoCancelAction = new BulkUndoCancelCommissionLinesAction(Factory);
			bulkUndoCancelAction.Initialise(new[] { Factory.New<ViewCommissionLine>() });
			return new BulkUndoCancelCommissionLinesForm(bulkUndoCancelAction);
		}

		#endregion
	}

	class BulkUndoCancelCommissionLinesFormForTest : BulkUndoCancelCommissionLinesForm
	{
		public BulkUndoCancelCommissionLinesFormForTest(BulkUndoCancelCommissionLinesAction action)
			: base(action)
		{
		}

		public ZButton RemoveErrorLinesButton_Exposed
		{
			get { return base.RemoveErrorLinesButton; }
		}
	}
}

