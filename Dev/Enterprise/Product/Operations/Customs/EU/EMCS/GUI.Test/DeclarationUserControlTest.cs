using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class DeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestMinimumSize()
		{
			AssertEquals(ControlDpiScalingHelper.NewScaledSize(1186, 595, true), control.MinimumSize);
		}

		public void TestDeclarationOrganizationsDynamicLayoutPanel()
		{
			using (var form = new ZForm(declaration))
			using (var control = new DeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				DynamicLayoutPanelTest.AssertControlsOrder(control.DeclarationOrganizationsDynamicLayoutPanel,
						nameof(DeclarationOrganizationsControlBag.ConsignorDocAddressControl),
						nameof(DeclarationOrganizationsControlBag.ConsigneeDocAddressControl),
						nameof(DeclarationOrganizationsControlBag.OwnerDocAddressUserControl));
			}
		}

		public void TestCustomsOfficeGrid()
		{
			using (var form = new ZForm(declaration))
			using (var control = new DeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var customsOfficeGrid = control.CustomsOfficeGrid;
				var officeCodeColumn = customsOfficeGrid.GetColumnStyle("CY_Data");
				CombineAssertions(() =>
				{
					AssertEquals("CY_Code", true, customsOfficeGrid.Columns[EuOfficeCode.Schema.CY_Code].IsVisible);
					AssertEquals("Invisible, but available to be shown. The grid is quite small to have four columns", false, customsOfficeGrid.Columns[EuOfficeCode.Schema.CY_CodeDescription].IsVisible);
					AssertEquals("CY_Data", true, customsOfficeGrid.Columns[EuOfficeCode.Schema.CY_Data].IsVisible);
					AssertEquals("Office Code (CY_Data) - Character Casing should be Upper case", CharacterCasing.Upper, officeCodeColumn.CharacterCasing);
					AssertEquals("CY_OfficeDescription", true, customsOfficeGrid.Columns[EuOfficeCode.Schema.CY_OfficeDescription].IsVisible);
				});
			}
		}

		public void TestSpecialInstructionsTextBox()
		{
			CombineAssertions(() =>
			{
				var specialInstructionsControl = control.SpecialInstructionsTextBox;
				AssertType<WordWrappingTextBox>("Type", specialInstructionsControl);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, specialInstructionsControl.CharacterCasing);
			});
		}

		public void TestCertOfExemptionTextBox()
		{
			CombineAssertions(() =>
			{
				var certOfExemptionControl = control.CertOfExemptionTextBox;
				AssertType<LongTextControl>("Type", certOfExemptionControl);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, certOfExemptionControl.CharacterCasing);
				AssertEquals("BindingMember", nameof(EMCSJobDeclaration.ZG_CertOfExemption), certOfExemptionControl.GetBindingMember());
			});
		}

		public void TestCertOfExemptionTextBox_MaxLength()
		{
			using (var form = new ZForm(declaration))
			using (var control = new DeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var certOfExemptionControl = control.CertOfExemptionTextBox;
				declaration.ZG_CertOfExemption = ZString.Empty.PadRight(255, '1');
				CombineAssertions(() =>
				{
					AssertEquals(255, ((ZString)certOfExemptionControl.CurrentDataItem).Length);
					AssertEquals(ZString.Empty.PadRight(255, '1'), certOfExemptionControl.CurrentDataItem);
				});
			}
		}

		public void TestOwnerRefTextBox()
		{
			CombineAssertions(() =>
			{
				var ownerRefControl = control.OwnerRefTextBox;
				AssertType<ZTextBox>("Type", ownerRefControl);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, ownerRefControl.CharacterCasing);
			});
		}

		public void TestInvoiceNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var invoiceNumberControl = control.InvoiceNumberTextBox;
				AssertType<ZTextBox>("Type", invoiceNumberControl);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, invoiceNumberControl.CharacterCasing);
			});
		}

		public void TestEntryDetailsGroupBox()
		{
			CombineAssertions(() =>
			{
				var entryDetailsGroupBox = control.EntryDetailsGroupBox;
				AssertType<ZGroupBox>("Type", entryDetailsGroupBox);
				AssertEquals("Caption", "Entry Details", entryDetailsGroupBox.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			control = new DeclarationUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		EMCSJobDeclaration declaration;
		DeclarationUserControl control;
	}
}
