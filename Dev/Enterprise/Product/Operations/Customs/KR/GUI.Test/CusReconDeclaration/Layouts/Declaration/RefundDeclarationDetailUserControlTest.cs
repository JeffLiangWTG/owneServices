using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class CustomsDetailUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestBindingMembers()
		{
			AssertEquals("EntryNumberTextBox Binding", "FormattedRefundDeclarationNumber", control.FindSingle<ZTextBox>("EntryNumberTextBox").BindTo);
			AssertEquals("TotalRefundAmountCalcEdit Binding", "TotalRefundAmount", control.FindSingle<ZCalcEdit>("TotalRefundAmountCalcEdit").BindTo);
			AssertEquals("MessageStatusDropEdit Binding", "CRD_MessageStatus", control.FindSingle<ZDropEdit>("MessageStatusDropEdit").BindTo);
			AssertEquals("EntryStatusDropEdit Binding", "CRD_CustomsStatus", control.FindSingle<ZDropEdit>("EntryStatusDropEdit").BindTo);
			AssertEquals("AcceptedDateEdit Binding", "AcceptedDate", control.FindSingle<ZDateEdit>("AcceptedDateEdit").BindTo);
			AssertEquals("RefundApprovalDateEdit Binding", "RefundApprovalDate", control.FindSingle<ZDateEdit>("RefundApprovalDateEdit").BindTo);
			AssertEquals("RefundApprovalNumberTextBox Binding", "RefundApprovalNumber", control.FindSingle<ZTextBox>("RefundApprovalNumberTextBox").BindTo);
			AssertEquals("ProvisionDateEdit Binding", "ProvisionDate", control.FindSingle<ZDateEdit>("ProvisionDateEdit").BindTo);
			AssertEquals("ProvisionNumberTextBox Binding", "ProvisionNumber", control.FindSingle<ZTextBox>("ProvisionNumberTextBox").BindTo);
			AssertEquals("RefundTypeDropEdit Binding", "CRD_DeclarationType", control.FindSingle<ZDropEdit>("RefundTypeDropEdit").BindTo);
			AssertEquals("RefundCauseDropEdit Binding", "CRD_RefundCauseCode", control.FindSingle<ZDropEdit>("RefundCauseDropEdit").BindTo);
			AssertEquals("RefundReasonDropEdit Binding", "CRD_RefundReasonCode", control.FindSingle<ZDropEdit>("RefundReasonDropEdit").BindTo);
			AssertEquals("CustomsOfficeCodeFindBox Binding", "CRD_CustomsOffice", control.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox").BindTo);
			AssertEquals("DepartmentCodeFindBox Binding", "CRD_CustomsDivision", control.FindSingle<ZCodeFindBox>("DepartmentCodeFindBox").BindTo);
			AssertEquals("TaxOfficeCodeFindBox Binding", "CRD_TaxOffice", control.FindSingle<ZCodeFindBox>("TaxOfficeCodeFindBox").BindTo);
			AssertEquals("BranchCodeFindBox Binding", "CRD_GB_Branch", control.FindSingle<ZGuidFindBox>("BranchCodeGuidFindBox").BindTo);
			AssertEquals("BrokerCodeFindBox Binding", "CRD_GS_NKCustomsAgent", control.FindSingle<ZCodeFindBox>("BrokerCodeFindBox").BindTo);
			AssertEquals("PayerAddressControl Binding", "CRD_OA_DeclarantAddress", control.FindSingle<ZAddressControl>("PayerAddressControl").BindTo);
			AssertEquals("BankAccountNumberTextBox Binding", "BankAccountNumber", control.FindSingle<ZTextBox>("BankAccountNumberTextBox").BindTo);
			AssertEquals("RegistrationNumberOneTextBox Binding", "RegistrationNumberOne", control.FindSingle<ZTextBox>("RegistrationNumberOneTextBox").BindTo);
			AssertEquals("KoreanRegistrationNumberOfCEOTextBox Binding", "KoreanRegistrationNumberOfCEO", control.FindSingle<ZTextBox>("KoreanRegistrationNumberOfCEOTextBox").BindTo);
			AssertEquals("PayerBankDropEdit Binding", "PayerBank", control.FindSingle<ZDropEdit>("PayerBankDropEdit").BindTo);
		}

		 public void TestRefundReasonVisibility()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using (var form = new RefundDeclarationForm(reconDeclaration))
			{
				form.Show();
				var refundDeclarationUserControl = form.FindSingle<RefundDeclarationUserControl>("RefundDeclarationUserControl");
				using (var control = refundDeclarationUserControl)
				{
					reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._03;
					AssertEquals("If Refund Cause is (03,04,05,06,07,08,09,10) then  Refund Reason is visible, else non-visible.", true, control.FindSingle<ZDropEdit>("RefundReasonDropEdit").Visible);
					reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._01;
					AssertEquals(false, control.FindSingle<ZDropEdit>("RefundReasonDropEdit").Visible);
					reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._04;
					AssertEquals(true, control.FindSingle<ZDropEdit>("RefundReasonDropEdit").Visible);
					reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._11;
					AssertEquals(false, control.FindSingle<ZDropEdit>("RefundReasonDropEdit").Visible);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new RefundDeclarationDetailUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		RefundDeclarationDetailUserControl control;
	}
}
