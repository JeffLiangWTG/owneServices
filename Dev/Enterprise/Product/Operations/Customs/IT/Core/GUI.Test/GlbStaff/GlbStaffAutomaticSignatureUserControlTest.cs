using System.Windows.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(GlbStaffAutomaticSignatureUserControl))]
sealed class GlbStaffAutomaticSignatureUserControlTest : BasherTest
{
	public void TestBindingSourceType()
	{
		AssertEquals("BindingSource.DataSourceType", typeof(Business.GlbStaffWrapper), control.BindingSource.DataSourceType);
	}

	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
	}

	public void TestIsConfigurationActiveCheckBox()
	{
		control.AssertContainsControl<ZCheckBox>("IsConfigurationActiveCheckBox", x => x
			.WithBindTo("AutomaticSignaturePasswordCollection.IsConfigurationActive")
		);
	}

	public void TestDelegateDropEdit()
	{
		control.AssertContainsControl<ZDropEdit>("DelegateDropEdit", x => x
			.WithBindTo("AutomaticSignaturePasswordCollection.GP_MailBoxID")
		);
	}

	public void TestUserTextBox()
	{
		control.AssertContainsControl<ZTextBox>("UserTextBox", x => x
			.WithBindTo("AutomaticSignaturePasswordCollection.GP_UserID")
			.WithCharacterCasing(CharacterCasing.Normal)
		);
	}

	public void TestFiscalUserTextBox()
	{
		control.AssertContainsControl<ZTextBox>("FiscalUserTextBox", x => x
			.WithBindTo("AutomaticSignaturePasswordCollection.GP_Name")
		);
	}

	public void TestAddAutomaticSignatureButton()
	{
		control.AssertContainsControl<ZButton>("AddAutomaticSignatureButton", x => x
			.WithCaption("Add Signature")
		);
	}

	public void TestClearAutomaticSignatureButton()
	{
		control.AssertContainsControl<ZButton>("ClearAutomaticSignatureButton", x => x
			.WithCaption("Clear Signature")
		);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new GlbStaffAutomaticSignatureUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	public override Form GetFormToBash()
	{
		var staff = Factory.New<GlbStaff>();
		var staffProvider = new GlbStaffWrapperProvider();

		var form = new ZChildForm() { CaptionRenderingEnabled = true };
		control.Dock = DockStyle.Fill;
		form.Controls.Add(control);
		form.SetDataBinding(staffProvider.GetWrapper(staff), "");
		return form;
	}

	GlbStaffAutomaticSignatureUserControl control;
}
