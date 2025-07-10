using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class RefundForCancelUserControlTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntryLine = reconDeclaration.CusReconEntryLines.AddNew();
			var contractRevocation = reconEntryLine.ContractRevocations.AddNew();

			using (var reconDeclarationForm = new RefundDeclarationForm(reconDeclaration))
			{
				reconDeclarationForm.Show();

				var tabControl = reconDeclarationForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				var entriesTab = tabControl.FindSingle<ZTabPage>("ImportEntriesTabPage");
				tabControl.SelectedTab = entriesTab;

				var control = entriesTab.FindSingle<ZUserControl>("RefundDeclarationImportEntriesUserControl");
				control.Show();
				AssertEquals(null, control.FindSingle<ZDropEdit>(nameof(RefundForCancelControlBag.CancelReasonDropEdit)).CaptionResourceString.Caption);
				AssertEquals("Cancel Reason", contractRevocation.CSI_CodeInfo.HumanReadableName);

				AssertEquals("EXP Entry No./ Line No.", control.FindSingle<ZTextBox>(nameof(RefundForCancelControlBag.ExportEntryNumberTextBox)).CaptionResourceString.Caption);
				AssertEquals("Export Entry Number", contractRevocation.ExportEntryNumberInfo.HumanReadableName);

				AssertEquals(null, control.FindSingle<ZTextBox>(nameof(RefundForCancelControlBag.DisposalNumberTextBox)).CaptionResourceString.Caption);
				AssertEquals("Disposal Number", contractRevocation.DisposalNumberInfo.HumanReadableName);

				AssertEquals("/", control.FindSingle<ZTextBox>(nameof(RefundForCancelControlBag.ExportEntryLineNumberTextBox)).CaptionResourceString.Caption);
				AssertEquals("Export Entry Line Number", contractRevocation.FormattedLineNoInfo.HumanReadableName);

				AssertEquals(null, control.FindSingle<ZDateEdit>(nameof(RefundForCancelControlBag.DisposalDateEdit)).CaptionResourceString.Caption);
				AssertEquals("Disposal Date", contractRevocation.CSI_DateOfExpiryInfo.HumanReadableName);

				AssertEquals(null, control.FindSingle<Customs.GUI.LongTextControl>(nameof(RefundForCancelControlBag.GoodsLocationDescriptionLongTextControl)).CaptionResourceString.Caption);
				AssertEquals("Goods Location Description", contractRevocation.CSI_AdditionalDescriptionInfo.HumanReadableName);

				AssertEquals(null, control.FindSingle<Customs.GUI.LongTextControl>(nameof(RefundForCancelControlBag.ResidualSubstanceDescriptionLongTextControl)).CaptionResourceString.Caption);
				AssertEquals("Residual Substance Description", contractRevocation.CSI_ReferenceNumber2Info.HumanReadableName);

				AssertEquals(null, control.FindSingle<Customs.GUI.LongTextControl>(nameof(RefundForCancelControlBag.DamageSituationLongTextControl)).CaptionResourceString.Caption);
				AssertEquals("Damage Situation", contractRevocation.CSI_DescriptionInfo.HumanReadableName);
			}
		}
	}
}
