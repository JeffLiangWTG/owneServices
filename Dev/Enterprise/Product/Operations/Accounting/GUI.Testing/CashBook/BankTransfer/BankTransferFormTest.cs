using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferForm))]
	public class BankTransferFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BankTransferForm(new BankTransfer(Factory, null));
		}

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				AssertEquals("Verb should be 'Reverse'", "Reverse", testForm.FormVerb);
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'new'", "New", testForm.FormVerb);
			}
		}

		public void TestHideTaxFields()
		{
			bool oldValue = GlbCompany.CurrentCompany.GC_IsGSTRegistered;

			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

				using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
				{
					testForm.Show();
					AssertEquals(true, ((BankTransferForm)testForm).TaxPanel_ForTestOnly.Visible);
				}

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

				using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
				{
					testForm.Show();
					AssertEquals(false, ((BankTransferForm)testForm).TaxPanel_ForTestOnly.Visible);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldValue;
			}
		}

		public void TestShowSetsFactoryContext()
		{
			BankTransfer testTransfer = new BankTransfer(Factory, null);
			testTransfer.IsReverseTransaction = true;
			using (BankTransferForm testForm = new BankTransferForm(testTransfer))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				testForm.OnShown_ForTestOnly(new EventArgs());
				Assert("The Factory should contain BusinessContext.ReverseDateForm", testForm.BusinessEntity.Factory.HasContext(BusinessContext.ReverseDateForm));
			}
		}

		public void TestFinanceChargeDescriptionIsReadOnly()
		{
			BankTransfer testTransfer = new BankTransfer(Factory, null);
			using (BankTransferForm testForm = new BankTransferForm(testTransfer))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();
				AssertEquals(true, testForm.BankChargeDescriptionTextBox_ForTestOnly.ReadOnly);
			}
		}

		public void TestFinanceChargeTaxDate()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			using (var form = new BankTransferForm(bankTransfer))
			{
				form.Show();
				AssertEquals(true, form.TaxDateEdit_ForTestOnly.Visible);
			}
		}

		public void TestBankChargeGovtChargeCodeVisibility()
		{
			foreach (var enableGovernmentChargeCode in new bool[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableGovernmentChargeCode))
				{
					var bankTransfer = new BankTransfer(Factory, null);
					using (var form = new BankTransferForm(bankTransfer))
					{
						form.Show();
						Application.DoEvents();
						var bankChargeGovtChargeCodeTextBox = form.FindSingleOrDefault<ZArchitecture.ZTextBox>("BankChargeGovtChargeCodeTextBox");
						var govtChargeCodePanel = form.FindSingleOrDefault<ZPanel>("GovtChargeCodePanel");
						AssertEquals(enableGovernmentChargeCode, govtChargeCodePanel.Visible);
						AssertEquals(enableGovernmentChargeCode, bankChargeGovtChargeCodeTextBox.Visible);
					}
				}
			}
		}

		public void TestCalcExVarianceCheckBoxBindingProperty()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			using (var form = new BankTransferForm(bankTransfer))
			{
				form.Show();
				AssertEquals(false, form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked);
				AssertEquals(false, bankTransfer.ShouldCalculateExchangeVariance);

				form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked = true;
				AssertEquals(true, form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked);
				AssertEquals(true, bankTransfer.ShouldCalculateExchangeVariance);

				Assert("PreCondition", UnitTestUserNotification.Instance.LastMessage.WasNone);

				AssertEquals("PreCondition", true, bankTransfer.ShouldCalculateExchangeVariance);
				AssertPrompt(() => {
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
					form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked = false;
				});
				AssertEquals("Do not update checkbox status because answer is not YES.", true, form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked);
				AssertEquals("Do not update checkbox status because answer is not YES.", true, bankTransfer.ShouldCalculateExchangeVariance);

				AssertEquals("PreCondition", true, bankTransfer.ShouldCalculateExchangeVariance);
				AssertPrompt(() => {
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
					form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked = false;
				});
				AssertEquals("Update checkbox status because answer is YES.", false, form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked);
				AssertEquals("Update checkbox status because answer is YES.", false, bankTransfer.ShouldCalculateExchangeVariance);
			}

			void AssertPrompt(Action action)
			{
				const string expectedCaption = "Confirm Buy Local Amount re-defaulting";
				const string expectedMessage = @"Canceling the Exchange Variance Calculation will result in equal Local Sell and Local Buy Amounts.
Depending on the currency of the To Bank Account, either Buy Amount or Buy exchange rate will be recalculated and should be confirmed as correct before posting the Bank Transfer.
Do you wish to cancel the exchange variance?";

				UnitTestUserNotification.Instance.ClearMessages();
				action?.Invoke();
				CombineAssertions("Show message to confirm to do re-defaulting", () => {
					AssertEquals(expectedCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestExRateGainLossCalcFindBoxVisibility()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			using (var form = new BankTransferForm(bankTransfer))
			{
				form.Show();
				AssertEquals(false, form.GetControl<ZCalcFindBox>("ExRateGainLossCalcFindBox").Visible);
			}

			using (var form = new BankTransferForm(bankTransfer))
			{
				form.Show();
				form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked = true;
				AssertEquals(true, form.GetControl<ZCalcFindBox>("ExRateGainLossCalcFindBox").Visible);

				form.GetControl<ZCheckBox>("CalcExVarianceCheckBox").Checked = false;
				AssertEquals(false, form.GetControl<ZCalcFindBox>("ExRateGainLossCalcFindBox").Visible);
			}
		}
	}
}
