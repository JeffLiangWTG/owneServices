using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class AdditionalInfosUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalInfosGrid_Removed()
		{
			using (var control = new PlugIn.AdditionalInfosUserControl())
			{
				AssertNull(control.FindSingleOrDefault<ZGrid>("AdditionalInfosGrid"));
			}
		}

		public void TestAddInfoTypeCodeDropEdit_Removed()
		{
			using (var control = new PlugIn.AdditionalInfosUserControl())
			{
				AssertNull(control.FindSingleOrDefault<ZGrid>("AddInfoTypeCodeDropEdit"));
			}
		}

		public void TestAddiInfoDescriptionTextBox_UpperLowerCase()
		{
			using (var control = new PlugIn.AdditionalInfosUserControl())
			{
				var additionalInfoDescriptionTextBox = control.FindSingle<ZTextBox>("AddiInfoDescriptionTextBox");
				AssertEquals(CharacterCasing.Normal, additionalInfoDescriptionTextBox.CharacterCasing);
			}
		}

		public void TestPanel_Expansion()
		{
			using (var form = new ZForm(GetImportDeclarationWithLine()))
			using (var control = new PlugIn.AdditionalInfosUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var panel = control.FindSingle<ZPanel>("AdditionalInfosPanel");
				CombineAssertions(() =>
				{
					AssertEquals("Dock Style", DockStyle.Fill, panel.Dock);
					AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), panel.Location);
				});
			}
		}

		public void TestDescriptionTextBox_Expansion()
		{
			using (var form = new ZForm(GetImportDeclarationWithLine()))
			using (var control = new PlugIn.AdditionalInfosUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var descriptionTextBox = control.FindSingle<ZTextBox>("AddiInfoDescriptionTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true), descriptionTextBox.Location);
					AssertEquals("Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 255, true), descriptionTextBox.Size);
					AssertEquals("Binding Member", "FilteredInvoiceLines.AdditionalInfoDescription", control.BindingSource.GetBindingMember(descriptionTextBox));
				});
			}
		}

		JobDeclaration GetImportDeclarationWithLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			return declaration;
		}
	}
}
