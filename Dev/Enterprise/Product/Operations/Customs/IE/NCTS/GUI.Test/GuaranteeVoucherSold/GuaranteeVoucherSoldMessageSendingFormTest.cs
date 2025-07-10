using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	[TestedType(typeof(GuaranteeVoucherSoldMessageSendingForm))]
	class GuaranteeVoucherSoldMessageSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetForm() as ZForm)
			{
				AssertEquals("FormHeading", "Guarantee Voucher Sold", form.FormHeading);
				form.Show();
				AssertEquals("Text", "Guarantee Voucher Sold", form.Text);
			}
		}

		public void TestSize()
		{
			using (var form = GetForm())
			{
				form.Show();
				AssertEquals("MinimumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 193, true), form.MinimumSize);
			}
		}

		public void TestShowFormType()
		{
			GuaranteeVoucherSoldMessageSendingForm.ShowForm(header);
			AssertType<GuaranteeVoucherSoldMessageSendingForm>("Dialog form type = GuaranteeVoucherSoldMessageSendingForm", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestHolderOfTransitProcedure()
		{
			using (var form = new GuaranteeVoucherSoldMessageSendingForm(messageSendingActionParent))
			{
				form.Show();
				var holderOfTransitFindBox = form.FindSingle<ZCodeFindBox>("HolderOfTransitProcedureFindBox");
				CombineAssertions(() =>
				{
					AssertNotNull(holderOfTransitFindBox);
					AssertEquals("HolderOfTransitProcedureFindBox.Caption", "Transit Holder", holderOfTransitFindBox.CaptionResourceString.Caption);
					AssertEquals("HolderOfTransitProcedureFindBox.Location", ControlDpiScalingHelper.NewScaledPoint(91, 17, true), holderOfTransitFindBox.Location);
					AssertEquals("HolderOfTransitProcedureFindBox.CodeBox.Size", ControlDpiScalingHelper.NewScaledSize(60, 20), holderOfTransitFindBox.CodeBox.Size);
				});
			}
		}

		public void TestTIRCarnet()
		{
			using (var form = new GuaranteeVoucherSoldMessageSendingForm(messageSendingActionParent))
			{
				form.Show();
				var tirCarnetCheckBox = form.FindSingle<ZCheckBox>("TIRCarnetCheckBox");
				CombineAssertions(() =>
				{
					AssertNotNull(tirCarnetCheckBox);
					AssertEquals("TIRCarnetCheckBox.Caption", "TIR Carnet", tirCarnetCheckBox.CaptionResourceString.Caption);
					AssertEquals("TIRCarnetCheckBox.Location", ControlDpiScalingHelper.NewScaledPoint(91, 47, true), tirCarnetCheckBox.Location);
				});
			}
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			using (var form = new GuaranteeVoucherSoldMessageSendingForm(messageSendingActionParent))
			{
				form.Show();
				var guaranteeOfficeFindBox = form.FindSingle<ZCodeFindBox>("CustomsOfficeOfGuaranteeFindBox");
				CombineAssertions(() =>
				{
					AssertNotNull(guaranteeOfficeFindBox);
					AssertEquals("CustomsOfficeOfGuaranteeFindBox.Caption", "Guarantee Office", guaranteeOfficeFindBox.CaptionResourceString.Caption);
					AssertEquals("CustomsOfficeOfGuaranteeFindBox.Location", ControlDpiScalingHelper.NewScaledPoint(385, 17, true), guaranteeOfficeFindBox.Location);
					AssertEquals("CustomsOfficeOfGuaranteeFindBox.CodeBox.Size", ControlDpiScalingHelper.NewScaledSize(60, 20), guaranteeOfficeFindBox.CodeBox.Size);
				});
			}
		}

		public void TestVoucherAmount()
		{
			using (var form = new GuaranteeVoucherSoldMessageSendingForm(messageSendingActionParent))
			{
				form.Show();
				var voucherAmountCalcEdit = form.FindSingle<ZArchitecture.ZCalcEdit>("VoucherAmountCalcEdit");
				CombineAssertions(() =>
				{
					AssertNotNull(voucherAmountCalcEdit);
					AssertEquals("VoucherAmountCalcEdit.Caption", "Voucher Amount", voucherAmountCalcEdit.CaptionResourceString.Caption);
					AssertEquals("VoucherAmountCalcEdit.Location", ControlDpiScalingHelper.NewScaledPoint(385, 47, true), voucherAmountCalcEdit.Location);
				});
			}
		}

		[RequiresSTA]
		public void TestButtonsCaptions()
		{
			using (var form = new GuaranteeVoucherSoldMessageSendingForm(messageSendingActionParent))
			{
				form.Show();
				CombineAssertions(() =>
				{
					var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
					AssertNotNull(sendButton);
					AssertEquals("SendButton.Caption", "Send", sendButton.CaptionResourceString.Caption);

					var cancelButton2 = form.FindSingleOrDefault<ZButton>(c => c.Name == "CancelButton2");
					AssertNotNull(cancelButton2);
					AssertEquals("CancelButton2.Caption", "Cancel", cancelButton2.CaptionResourceString.Caption);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return GetForm();
		}

		GuaranteeVoucherSoldMessageSendingForm GetForm()
		{
			return new GuaranteeVoucherSoldMessageSendingForm(messageSendingActionParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusGuaranteeHeader>();
			messageSendingActionParent = new GuaranteeVoucherSoldSendingActionParent(header);
		}
		CusGuaranteeHeader header;
		GuaranteeVoucherSoldSendingActionParent messageSendingActionParent;
	}
}
