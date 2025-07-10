using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
{
	public void TestGuaranteesTabVisibility()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			userControl.SetTabPagesVisibility();

			declaration.JE_MessageType = "EXP";
			entryInstruction.CEI_Style = "H1";
			AssertTabPages(userControl, expectedGuarateesTabPageVisible: false);

			entryInstruction.CEI_Style = "H2";
			AssertTabPages(userControl, expectedGuarateesTabPageVisible: false);

			declaration.JE_MessageType = "IMP";
			userControl.SetTabPagesVisibility();
			entryInstruction.CEI_Style = "H1";
			AssertTabPages(userControl, expectedGuarateesTabPageVisible: true);
		}

		void AssertTabPages(EntryInstructionDetailsUserControl userControl, bool expectedGuarateesTabPageVisible)
		{
			var detailTabPage = userControl.FindSingle<ZTabPage>("DetailsTabPage");
			AssertEquals("DetailsTabPage Visible", true, detailTabPage.TabVisible);

			if (expectedGuarateesTabPageVisible)
			{
				var guaranteesTabPage = userControl.FindSingle<ZTabPage>("GuaranteesTabPage");
				AssertEquals("GuaranteesTabPage Visible", expectedGuarateesTabPageVisible, guaranteesTabPage.TabVisible);
			}
		}
	}

	public void TestEntryInstructionGridUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var entryInstructionGridUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("EntryInstructionGridUserControl");
			AssertEquals("EntryInstructionGridUserControl Type", typeof(EntryInstructionGridUserControl), entryInstructionGridUserControl.UserControlType);
		}
	}

	public void TestGuaranteesUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var guaranteesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("GuaranteesUserControl");
			AssertEquals("GuaranteesUserControl Type", typeof(EntryInstructionGuaranteesUserControl), guaranteesUserControl.UserControlType);
		}
	}

	public void TestAdditionalInfosTab_Caption()
	{
		using (var control = new EntryInstructionDetailsUserControlForTest())
		{
			AssertEquals("Additional Infos Tab Caption", "Additional Documents", control.GetAdditionalInfosTabCaption_Exposed()?.Caption);
		}
	}

	public void TestSupportingDocumentsTabPageCaption()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, "");
			form.Show();

			var supportingDocumentsTabPage = control.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
			AssertEquals("SupportingDocumentsTabPage Caption", "[44] Supporting Documents", supportingDocumentsTabPage.CaptionResourceString.Caption);
		}
	}

	public void TestSupportingDocumentsControlType_WhenExport()
	{
		declaration.JE_MessageType = "EXP";

		AssertSupportingDocumentsControlType<ExportEntryInstructionLayoutSupportingDocumentsUserControl>();
	}

	public void TestSupportingDocumentsControlType_ForImport()
	{
		declaration.JE_MessageType = "IMP";

		AssertSupportingDocumentsControlType<ImportEntryInstructionLayoutSupportingDocumentsUserControl>();
	}

	public void TestPreviousDocumentsTabPageCaption()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, "");
			form.Show();

			var previousDocumentsTabPage = control.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
			AssertEquals("PreviousDocumentsTabPage Caption", "[40] Previous Documents", previousDocumentsTabPage.CaptionResourceString.Caption);
		}
	}

	public void TestPreviousDocumentsTabPageCaption_Ucc6Export()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, "");
			form.Show();

			var previousDocumentsTabPage = control.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
			AssertEquals("PreviousDocumentsTabPage Caption for Ucc6 Export", "Previous Documents", previousDocumentsTabPage.CaptionResourceString.Caption);
		}
	}

	public void TestPreviousDocumentsControlType()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, "");
			form.Show();

			var previousDocumentsTabPage = control.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
			previousDocumentsTabPage.Show();

			var previousDocumentsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("PreviousDocumentsUserControl");
			AssertEquals(typeof(EntryInstructionLayoutPreviousDocumentsUserControl), previousDocumentsUserControl.UserControlType);
		}
	}

	public void TestPreviousDocumentsControlType_Ucc6Export()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, "");
			form.Show();

			var previousDocumentsTabPage = control.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
			previousDocumentsTabPage.Show();

			var previousDocumentsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("PreviousDocumentsUserControl");
			AssertEquals(typeof(LayoutUcc6ExportEntryInstructionPreviousDocumentsUserControl), previousDocumentsUserControl.UserControlType);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	void AssertSupportingDocumentsControlType<T>()
	{
		using (var form = new ZForm())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(declaration, "");
			form.Show();

			var supportingDocumentsTabPage = control.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
			supportingDocumentsTabPage.Show();

			var supportingDocumentsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl");
			AssertEquals(typeof(T), supportingDocumentsUserControl.UserControlType);
		}
	}
}

sealed class EntryInstructionDetailsUserControlForTest : EntryInstructionDetailsUserControl
{
	internal ResourceStringData GetAdditionalInfosTabCaption_Exposed() => base.GetAdditionalInfosTabCaption();
}
