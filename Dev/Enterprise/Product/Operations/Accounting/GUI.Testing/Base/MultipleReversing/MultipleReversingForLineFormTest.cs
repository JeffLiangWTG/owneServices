using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Base.Testing
{
	[TestedType(typeof(MultipleReversingForLineForm))]
	public class MultipleReversingForLineFormTest : MultipleReversingBaseFormTest
	{
		public void TestRemoveErrorTransactionsButtonClick()
		{
			var wip = Factory.NewWithValidTestData<WIP>();
			var acr = Factory.NewWithValidTestData<Accrual>();

			var multipleReversingProvider = new MultipleReversingProviderForLine();
			multipleReversingProvider.BizObjectsForReversing.Add(wip);
			multipleReversingProvider.BizObjectsForReversing.Add(acr);

			wip.AddRowError("Error for testing only");

			var wipReversingLine = new TransactionLineForReversing(wip);
			var acrReversingLine = new TransactionLineForReversing(acr);
			multipleReversingProvider.TransactionLinesAlreadyReversed.Add(wipReversingLine);
			multipleReversingProvider.TransactionLinesAlreadyReversed.Add(acrReversingLine);

			Assert(multipleReversingProvider.TransactionLinesAlreadyReversed.Contains(wipReversingLine));
			Assert(multipleReversingProvider.TransactionLinesAlreadyReversed.Contains(acrReversingLine));

			using (var form = new MultipleReversingForLineForm(multipleReversingProvider))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				Application.DoEvents();

				form.RemoveErrorTransactionsButton_Click_ForTestOnly(this, new EventArgs());

				Assert(!multipleReversingProvider.TransactionLinesAlreadyReversed.Contains(wipReversingLine));
				Assert(multipleReversingProvider.TransactionLinesAlreadyReversed.Contains(acrReversingLine));
			}
		}

		public override void TestFormVerb()
		{
			using (var form = (MultipleReversingForLineForm)GetFormToBashCore())
			{
				AssertEquals("Reverse", form.FormVerb);
			}
		}

		public new void TestMakeRequiredFieldsEditable()
		{
			var multipleReversingProvider = new MultipleReversingProviderForLine();
			var wip = Factory.NewWithValidTestData<WIP>();
			multipleReversingProvider.BizObjectsForReversing.Add(wip);
			wip.SetModeToReversing();
			multipleReversingProvider.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(wip));

			using (var testForm = new MultipleReversingForLineForm(multipleReversingProvider))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				var header = ((MultipleReversingProviderForLine)testForm.BusinessEntity).TransactionLinesAlreadyReversed[0] as BusinessObject;

				AssertEquals("ReverseDateInfo.ReadOnly", false, header.ZPropertyInfoHash["ReverseDate"].ReadOnly);

				foreach (ZPropertyInfo info in header.ZPropertyInfoHash)
				{
					if (info.Name != "ReverseDate" && info.Name != "AL_ReverseDate")
					{
						AssertEquals(info.Name + " should be readonly", true, info.ReadOnly);
					}
				}
			}
		}

		public void TestFormCaption()
		{
			using (var form = (MultipleReversingForLineForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Caption: ", "Multiple WIP/Accruals", form.FormCaption);
			}
		}

		public void TestConcurrencyHandling()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);
			var job = creator.CreateJob("S00001222", creator.ABIGAS, 0m, null, 0m);
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = creator.CC1.PK;
			var wip = creator.CreateWIP(job, creator.CC1, 1m, "wip", 100m, charge1);
			var acr = creator.CreateAccrual(job, creator.CC1, 1m, "acr", 200m, charge1);
			Factory.Save();

			var multipleReversingProvider1 = new MultipleReversingProviderForLine();
			multipleReversingProvider1.Factory.RefreshEnabled = false;
			multipleReversingProvider1.BizObjectsForReversing.Add(wip);
			multipleReversingProvider1.BizObjectsForReversing.Add(acr);
			var wipInmultipleReversingProvider1Factory = multipleReversingProvider1.Factory.Load<WIP>(wip.PK);
			wipInmultipleReversingProvider1Factory.SetModeToReversing();
			var acrInmultipleReversingProvider1Factory = multipleReversingProvider1.Factory.Load<Accrual>(acr.PK);
			acrInmultipleReversingProvider1Factory.SetModeToReversing();
			multipleReversingProvider1.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(wipInmultipleReversingProvider1Factory));
			multipleReversingProvider1.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(acrInmultipleReversingProvider1Factory));

			var multipleReversingProvider2 = new MultipleReversingProviderForLine();
			multipleReversingProvider2.Factory.RefreshEnabled = false;
			multipleReversingProvider2.BizObjectsForReversing.Add(wip);
			multipleReversingProvider2.BizObjectsForReversing.Add(acr);
			var wipInmultipleReversingProvider2Factory = multipleReversingProvider2.Factory.Load<WIP>(wip.PK);
			wipInmultipleReversingProvider2Factory.SetModeToReversing();
			var acrInmultipleReversingProvider2Factory = multipleReversingProvider2.Factory.Load<Accrual>(acr.PK);
			acrInmultipleReversingProvider2Factory.SetModeToReversing();
			multipleReversingProvider2.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(wipInmultipleReversingProvider2Factory));
			multipleReversingProvider2.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(acrInmultipleReversingProvider2Factory));

			using (var reverseForm1 = new MultipleReversingForLineForm(multipleReversingProvider1))
			{
				reverseForm1.DisplayMode = ODisplayMode.Delete;
				AssertWIPAccrualIsReversed(wip.PK, acr.PK, false);
				Assert("Reverse Form 1 should be editable.", reverseForm1.DisplayMode == ODisplayMode.Delete);

				using (var reverseForm2 = new MultipleReversingForLineForm(multipleReversingProvider2))
				{
					reverseForm2.DisplayMode = ODisplayMode.Delete;
					AssertWIPAccrualIsReversed(wip.PK, acr.PK, false);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					reverseForm2.PostingButtons_ForTestOnly.SaveAndCloseButton.PerformClick();
					AssertWIPAccrualIsReversed(wip.PK, acr.PK, true);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(() => reverseForm1.PostingButtons_ForTestOnly.SaveAndCloseButton.PerformClick());
				var expectedConcurrencyWarning = @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
Invoice Lines (CargoWise Support @";
				AssertContains(expectedConcurrencyWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Reverse Form 1 should be read only.", reverseForm1.DisplayMode == ODisplayMode.ReadOnly);
			}

			void AssertWIPAccrualIsReversed(ZGuid wipPk, ZGuid accrualPK, bool expectedToBeReversed)
			{
				var reloadFactory = new BusinessObjectFactory();
				var reloadedWIP = reloadFactory.Load<Accrual>(wipPk);
				var reloadedACR = reloadFactory.Load<Accrual>(accrualPK);
				AssertEquals(expectedToBeReversed, reloadedWIP.IsReversed);
				AssertEquals(expectedToBeReversed, reloadedACR.IsReversed);
			}
		}

		public void TestDontPostIfThereareNoTransactions()
		{
			var multipleReversingProvider = new MultipleReversingProviderForLine();
			var wip = Factory.NewWithValidTestData<WIP>();
			wip.SetModeToReversing();

			multipleReversingProvider.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(wip));
			using (var form = new MultipleReversingForLineForm(multipleReversingProvider))
			{
				form.Show();
				Application.DoEvents();

				form.PostingButtons_ForTestOnly.SaveButton.Enabled = form.PostingButtons_ForTestOnly.SaveAndCloseButton.Enabled = true;

				Assert("Precondition: SaveButton is enabled.", form.PostingButtons_ForTestOnly.SaveButton.Enabled);
				Assert("Precondition: SaveAndCloseButton is enabled.", form.PostingButtons_ForTestOnly.SaveAndCloseButton.Enabled);

				multipleReversingProvider.TransactionLinesAlreadyReversed.RemoveAll();
				Assert("SaveButton is disabled.", !form.PostingButtons_ForTestOnly.SaveButton.Enabled);
				Assert("SaveAndCloseButton is disabled.", !form.PostingButtons_ForTestOnly.SaveAndCloseButton.Enabled);
			}
		}

		public void TestSetReadOnlyIncludingChildren()
		{
			var multipleReversingProvider = new MultipleReversingProviderForLine();
			var wip = Factory.NewWithValidTestData<WIP>();
			multipleReversingProvider.BizObjectsForReversing.Add(wip);
			wip.SetModeToReversing();
			multipleReversingProvider.TransactionLinesAlreadyReversed.Add(new TransactionLineForReversing(wip));

			using (var testForm = new MultipleReversingForLineForm(multipleReversingProvider))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				foreach (Control control in testForm.Controls)
				{
					Assert(control.Name + " must be editable", !control.GetReadOnly());
				}
			}
		}

		#region TestMakeRequiredFieldsEditableForReversingOnBizObject

		protected override void AssertNotReadonlyWhenNoRowErrors(string message, NonPersistentBusinessObject wrapper)
		{
			//there is no additional fields to check
		}

		#endregion

		#region Implementation

		protected override NonPersistentBusinessObject GetNewAlreadyReversedBusinessObjectWrappedForBinding()
		{
			var wip = Factory.NewWithValidTestData<WIP>();
			wip.SetModeToReversing();

			return new TransactionLineForReversing(wip);
		}

		protected override BusinessObject GetOriginalTransaction(BusinessObject reversedBizo) => reversedBizo;

		protected override MultipleReversingProviderBase GetNewMultipleReversingProvider() => new MultipleReversingProviderForLine();

		protected override MultipleReversingBaseForm GetNewForm(MultipleReversingProviderBase multipleReversingProvider) => new MultipleReversingForLineForm((MultipleReversingProviderForLine)multipleReversingProvider);

		protected override bool ExpectedIsReveringModeValue(AccountingZForm testForm) => testForm.DisplayMode == ODisplayMode.Delete;

		#endregion
	}
}
