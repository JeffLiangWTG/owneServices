using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(ImportMessageSendingForm))]
	sealed class ImportMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
			dec.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;

			var parent = new ImportDeclarationMessageSendingActionParent(dec);
			return new ImportMessageSendingForm(parent);
		}

		public void TestColumns()
		{
			using (var form = GetFormToBashCore())
			{
				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];
				AssertEquals(typeof(ZCheckBoxColumnStyle), grid.GetColumnStyle(nameof(ImportEntryMessageSendingAction.CusCon)).ColumnStyleType);
			}
		}

		public void TestImportBottomSectionUserControl()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNoExceptionThrown(() => form.FindSingle<ImportBottomSectionUserControl>("ImportBottomSectionUserControl"));
			}
		}

		public void TestWarningsStillAllowSend()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OwnerRef = string.Empty;
			declaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
			declaration.CustomsEntryHeaders.AddNew().CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;

			var parent = new ImportDeclarationMessageSendingActionParent(declaration);
			parent.SendingObjectsCollection[0].ShouldSend = true;
			using (var form = new ImportMessageSendingForm(parent))
			{
				form.Show();
				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;
				AssertHasWarnings(declaration.JE_OwnerRefInfo);
				AssertEquals("Even with warnings, send should be enabled", true, form.FindSingle<ZButton>("SendButton").Enabled);
			}
		}
	}
}
