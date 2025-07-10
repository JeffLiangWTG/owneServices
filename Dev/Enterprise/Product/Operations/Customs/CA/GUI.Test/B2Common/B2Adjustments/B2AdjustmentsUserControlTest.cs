using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class B2AdjustmentsUserControlTest : BaseB2UserControlTest<B2AdjustmentsUserControl>
	{
		public void TestUseImporterSecurityNumberPopup()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABCDEFGH");
			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.New<OrgHeader>();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "12345678";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				declaration.JE_OH_Importer = importer.PK;
				AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("98765", declaration.TransactionNumber.AccountSecurityCode);
			}

			CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_OH_Importer = importer.PK;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.CA_UseImporterAccountSecurityNumber);
				AssertEquals("12345", declaration.TransactionNumber.AccountSecurityCode);
			}
		}

		public void TestTransactionNumberControlVisibility()
		{
			CombineAssertions("Test when registry is overridden to YES", () =>
			{
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var formattedTransactionNumberTextBox = form.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = form.Controls.Find("AccountSecurityCodeTextBox", true)[0] as ZTextBox;
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
				CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var formattedTransactionNumberTextBox = form.Controls.Find("FormattedTransactionNumberTextBox", true)[0] as ZTextBox;
					var securityCodeTextBox = form.Controls.Find("AccountSecurityCodeTextBox", true)[0] as ZTextBox;
					var sequentialNumberTextBox = form.Controls.Find("SequentialNumberTextBox", true)[0] as ZTextBox;
					var checkDigitTextBox = form.Controls.Find("CheckDigitTextBox", true)[0] as ZTextBox;
					Assert(formattedTransactionNumberTextBox.Visible);
					Assert(!securityCodeTextBox.Visible);
					Assert(!sequentialNumberTextBox.Visible);
					Assert(!checkDigitTextBox.Visible);
				}
			});
		}

		public void TestControlVisibilityForB3X()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var b2TypeDropEdit = form.Controls.Find("B2TypeDropEdit", true)[0] as ZArchitecture.GUI.ZDropEdit;
				var longTextDetailsGroupBox = form.Controls.Find("LongTextDetailsGroupBox", true)[0] as ZArchitecture.GUI.ZGroupBox;
				var mailToOrganisationControlWithMiscellaneous = form.Controls.Find("MailToOrganisationControlWithMiscellaneous", true)[0] as Customs.GUI.ZOrganisationControlWithMiscellaneous;
				var paymentCodeDropEdit = form.Controls.Find("PaymentCodeDropEdit", true)[0] as ZArchitecture.GUI.ZDropEdit;
				var transportModeDropEdit = form.Controls.Find("TransportModeDropEdit", true)[0] as ZArchitecture.GUI.ZDropEdit;
				var carrierCodeFindBox = form.Controls.Find("CarrierCodeFindBox", true)[0] as ZArchitecture.GUI.ZCodeFindBox;
				AssertEquals("B3X Type", b2TypeDropEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Vendor", mailToOrganisationControlWithMiscellaneous.GetExtension<ILabelCaptionRenderer>().Caption);
				Assert(!longTextDetailsGroupBox.Visible);
				Assert(mailToOrganisationControlWithMiscellaneous.Visible);
				Assert(transportModeDropEdit.Visible);
				Assert(carrierCodeFindBox.Visible);
				Assert(paymentCodeDropEdit.Visible);
				var docsAttachedCheckBox = form.Controls.Find("DocsAttachedCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(!docsAttachedCheckBox.Visible);
				var isOurFaultCheckBox = form.Controls.Find("IsOurFaultCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(!isOurFaultCheckBox.Visible);
				var claimedInterestAmountCalcEdit = form.Controls.Find("ClaimedInterestAmountCalcEdit", true)[0] as ZCalcEdit;
				Assert(!claimedInterestAmountCalcEdit.Visible);
				var anySightDepositAmountCalcEdit = form.Controls.Find("AnySightDepositAmountCalcEdit", true)[0] as ZCalcEdit;
				Assert(!anySightDepositAmountCalcEdit.Visible);
				var acceptedOverrideCheckBox = form.Controls.Find("AcceptedOverrideCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(acceptedOverrideCheckBox.Visible);
				var submittedOverrideCheckBox = form.Controls.Find("SubmittedOverrideCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(submittedOverrideCheckBox.Visible);
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var b2TypeDropEdit = form.Controls.Find("B2TypeDropEdit", true)[0] as ZArchitecture.GUI.ZDropEdit;
				var longTextDetailsGroupBox = form.Controls.Find("LongTextDetailsGroupBox", true)[0] as ZArchitecture.GUI.ZGroupBox;
				var mailToOrganisationControlWithMiscellaneous = form.Controls.Find("MailToOrganisationControlWithMiscellaneous", true)[0] as Customs.GUI.ZOrganisationControlWithMiscellaneous;
				var paymentCodeDropEdit = form.Controls.Find("PaymentCodeDropEdit", true)[0] as ZArchitecture.GUI.ZDropEdit;
				AssertEquals("B2 Type", b2TypeDropEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Mail To", mailToOrganisationControlWithMiscellaneous.GetExtension<ILabelCaptionRenderer>().Caption);
				Assert(longTextDetailsGroupBox.Visible);
				Assert(mailToOrganisationControlWithMiscellaneous.Visible);
				Assert(!paymentCodeDropEdit.Visible);
				var docsAttachedCheckBox = form.Controls.Find("DocsAttachedCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(docsAttachedCheckBox.Visible);
				var isOurFaultCheckBox = form.Controls.Find("IsOurFaultCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(isOurFaultCheckBox.Visible);
				var claimedInterestAmountCalcEdit = form.Controls.Find("ClaimedInterestAmountCalcEdit", true)[0] as ZCalcEdit;
				Assert(claimedInterestAmountCalcEdit.Visible);
				var anySightDepositAmountCalcEdit = form.Controls.Find("AnySightDepositAmountCalcEdit", true)[0] as ZCalcEdit;
				Assert(anySightDepositAmountCalcEdit.Visible);
				var acceptedOverrideCheckBox = form.Controls.Find("AcceptedOverrideCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(!acceptedOverrideCheckBox.Visible);
				var submittedOverrideCheckBox = form.Controls.Find("SubmittedOverrideCheckBox", true)[0] as ZArchitecture.GUI.ZCheckBox;
				Assert(!submittedOverrideCheckBox.Visible);
			}
		}

		protected override JobDeclaration GetJobDeclarationForBOSubscribersShouldBeDetached()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return declaration;
		}
	}
}
