using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDetailsUserControlType()
	{
		AssertUserControlType("DetailsUserControl", typeof(LayoutEntryInstructionDetailBasicUserControl));
	}

	public void TestGridUserControlType()
	{
		AssertUserControlType("EntryInstructionGridUserControl", typeof(EntryInstructionGridUserControl));
	}

	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
	}

	public void TestAdditionalInfosTabPage_Import()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (var frm = new ZForm(declaration))
		using (var userControl = new EntryInstructionDetailsUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Documents", tabPage.CaptionResourceString.Caption);
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("AdditionalInfoUserControl");
				AssertEquals("UserControl type", typeof(AdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
				AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
			});
		}
	}

	public void TestAdditionalInfosTabPage_Export()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (var frm = new ZForm(declaration))
		using (var userControl = new EntryInstructionDetailsUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Documents", tabPage.CaptionResourceString.Caption);
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("AdditionalInfoUserControl");
				AssertEquals("UserControl type", typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
				AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
			});
		}
	}

	public void TestSupportingDocumentsTabPage_Import()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (var frm = new ZForm(declaration))
		using (var userControl = new EntryInstructionDetailsUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Supporting Documents", tabPage.CaptionResourceString.Caption);

				AssertEquals("SupportingDocumentsTabPage visible when import", true, tabPage.TabVisible);
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl");
				AssertEquals("UserControl type", typeof(ImportSupportingDocumentsUserControl), foundUserControl.UserControlType);
			});
		}
	}

	public void TestSupportingDocumentsTabPage_Export()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using (var frm = new ZForm(declaration))
		using (var userControl = new EntryInstructionDetailsUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Supporting Documents", tabPage.CaptionResourceString.Caption);

				AssertEquals("SupportingDocumentsTabPage visible when export", true, tabPage.TabVisible);
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl");
				AssertEquals("UserControl type", typeof(ExportSupportingDocumentsUserControl), foundUserControl.UserControlType);
			});
		}
	}

	public void TestPreviousDocumentsUserControlType()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		using (var frm = new ZForm(declaration))
		using (var userControl = new EntryInstructionDetailsUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Previous Documents", tabPage.CaptionResourceString.Caption);

				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("PreviousDocumentsUserControl");
				AssertEquals("UserControl type", typeof(LayoutPreviousDocumentsUserControl), foundUserControl.UserControlType);
			});
		}
	}

	EntryInstructionDetailsUserControl control;
	protected override void SetUp()
	{
		base.SetUp();
		control = new EntryInstructionDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	void AssertUserControlType(ZString userControlName, Type userControlType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			var userControl = control.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
			AssertEquals($"{userControlName}.UserControlType", userControlType, userControl.UserControlType);
		}
	}
}
