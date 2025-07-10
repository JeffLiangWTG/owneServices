using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class K84DeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibilityWhenIsCADEnabled()
		{
			var cadDec = Factory.New<JobDeclaration>();
			cadDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			cadDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var cadEntry = cadDec.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			using (var cadForm = new JobDeclarationForm(cadDec))
			{
				cadForm.Show();
				var cadControl = (CustomsBrokerageUserControl)cadForm.CustomsBrokerageUserControl;
				cadControl.MainTabControl.SelectedTab = cadControl.K84TabPage;
				AssertEquals("B3EntrySubmittedDateEdit", "CAD Submitted Date", cadControl.K84TabPage.Controls.Find("B3EntrySubmittedDateEdit", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("B3AcceptedDateEdit", "CAD Accepted Date", cadControl.K84TabPage.Controls.Find("B3AcceptedDateEdit", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("B3EntryStatusTextBox", "CAD Entry Status", cadControl.K84TabPage.Controls.Find("B3EntryStatusTextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("B3EntryMessageTextBox", "CAD Message Status", cadControl.K84TabPage.Controls.Find("B3EntryMessageTextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("CADVersionTextBox", true, cadControl.K84TabPage.Controls.Find("CADVersionTextBox", true)[0].Visible);
			}
		}

		public void TestTransactionNumberControlVisibility()
		{
			CombineAssertions("Test display with messages sent declaration", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = testDeclaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				entryHeader.Messages.AddNew();
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var formattedTransactionNumberTextBox = brokerageUserControl.K84TabPage.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = brokerageUserControl.K84TabPage.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = brokerageUserControl.K84TabPage.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = brokerageUserControl.K84TabPage.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});

			CombineAssertions("Test display with empty declaration", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var formattedTransactionNumberTextBox = brokerageUserControl.K84TabPage.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = brokerageUserControl.K84TabPage.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = brokerageUserControl.K84TabPage.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = brokerageUserControl.K84TabPage.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(!formattedTransactionNumberTextBox.Visible);
					Assert(securityCodeTextBox.Visible);
					Assert(sequentialNumberTextBox.Visible);
					Assert(checkDigitTextBox.Visible);
				}
			});

			CombineAssertions("Test display with registry set", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				var testDeclaration = Factory.New<JobDeclaration>();
				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (var testForm = new JobDeclarationForm(testDeclaration))
				{
					testForm.Show();
					var brokerageUserControl = (CustomsBrokerageUserControl)testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.K84TabPage;
					var formattedTransactionNumberTextBox = brokerageUserControl.K84TabPage.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = brokerageUserControl.K84TabPage.Controls.Find("SecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = brokerageUserControl.K84TabPage.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = brokerageUserControl.K84TabPage.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});
		}
	}
}
