using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVXUserControlTest : TestCaseWithFactory
	{
		public void TestEditButton()
		{
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;

			using (var form = new JobDeclarationForm(lvxJob))
			{
				form.Show();
				var editButton = (ZButton)form.Controls.Find("EditButton", true)[0];
				Assert("Enabled", !editButton.Enabled);
			}

			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);

			Factory.Save();

			using (var form = new JobDeclarationForm(lvxJob))
			{
				form.Show();
				var editButton = (ZButton)form.Controls.Find("EditButton", true)[0];
				Assert("Enabled", editButton.Enabled);
				var lvxUserControl = (LVXUserControl)form.CustomsBrokerageUserControl.DeclarationUserControl;
				AssertNull(ZFormModaliser.LastFormShownForTest as JobDeclarationForm);
				editButton.PerformClick();
				var lastShownForm = ZFormModaliser.LastFormShownForTest as JobDeclarationForm;
				AssertNotNull(lastShownForm);
				AssertNotNull(lastShownForm.ControllerID);
				AssertEquals(form, ZFormModaliser.GetParentFormForModalForm(lastShownForm));
			}
		}

		public void TestTransactionNumberControlVisibility()
		{
			CombineAssertions("Test when registry is overridden to YES", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var formattedTransactionNumberTextBox = form.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = form.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = form.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = form.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(!formattedTransactionNumberTextBox.Visible);
					Assert(securityCodeTextBox.Visible);
					Assert(sequentialNumberTextBox.Visible);
					Assert(checkDigitTextBox.Visible);
				}
			});

			CombineAssertions("Test when registry is set to No", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var formattedTransactionNumberTextBox = form.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = form.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = form.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = form.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});
		}
	}
}
