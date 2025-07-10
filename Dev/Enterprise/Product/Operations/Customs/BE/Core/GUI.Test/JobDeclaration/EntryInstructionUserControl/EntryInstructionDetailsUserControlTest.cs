using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

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

	public void TestAdditionalInfosTabCaption()
	{
		AssertUserControlLabel("AdditionalInfoTabPage", "Additional Documents");
	}

	public void TestPreviousDocumentsTabCaption()
	{
		AssertUserControlLabel("PreviousDocumentsTabPage", "[UCC 2/1] Previous documents");
	}

	public void TestSupportingDocumentsTabCaption()
	{
		AssertUserControlLabel("SupportingDocumentsTabPage", "[UCC 2/3] Supporting documents");
	}

	public void TestSupplyChainActorReferencesTabIsPresent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
			var supplyChainActorReferencesTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == "SupplyChainActorReferencesTabPage");
			instructionTabControl.SelectTab(supplyChainActorReferencesTabPage);
			var supplyChainActorReferencesUserControl = supplyChainActorReferencesTabPage.Controls.Find("SupplyChainActorReferencesUserControl", true).First();

			CombineAssertions(() =>
			{
				AssertEquals("TabVisible", true, supplyChainActorReferencesTabPage.TabVisible);
				AssertEquals("Tab caption", "Add. Supply Chain Actor", supplyChainActorReferencesTabPage.CaptionResourceString.Caption);
				AssertType<EU.GUI.PlugIn.SupplyChainActorReferencesUserControl>("Type", supplyChainActorReferencesUserControl);
			});
		}
	}

	public void TestTabPagesOrder_Export()
	{
		var tabOrderThatMustBeMaintained = new ZString[] { "DetailsTabPage", "SupportingDocumentsTabPage", "AdditionalInfoTabPage", "PreviousDocumentsTabPage", "SupplyChainActorReferencesTabPage", "AuthorisationsTabPage" };
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
			var tabPagesOrder = instructionTabControl.AllTabPages.OfType<ZTabPage>().Where(tp => tp.TabVisible && tabOrderThatMustBeMaintained.Contains(tp.Name)).Select(tp => new ZString(tp.Name)).ToArray();

			AssertArrayEqualsByElements(tabOrderThatMustBeMaintained, tabPagesOrder);
		}
	}

	public void TestTabPagesOrder_Import()
	{
		var tabOrderThatMustBeMaintained = new ZString[] { "DetailsTabPage", "FiscalReferencesTabPage", "SupportingDocumentsTabPage", "AdditionalInfoTabPage", "PreviousDocumentsTabPage", "GuaranteesTabPage", "SupplyChainActorReferencesTabPage", "AuthorisationsTabPage" };
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.CustomsEntryInstructions.AddNew();

		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
			var tabPagesOrder = instructionTabControl.AllTabPages.OfType<ZTabPage>().Where(tp => tp.TabVisible && tabOrderThatMustBeMaintained.Contains(tp.Name)).Select(tp => new ZString(tp.Name)).ToArray();

			AssertArrayEqualsByElements(tabOrderThatMustBeMaintained, tabPagesOrder);
		}
	}

	public void TestGuaranteesUserControlType()
	{
			AssertUserControlType("GuaranteesUserControl", typeof(EntryInstructionGuaranteesUserControl));
	}

	void AssertUserControlType(ZString userControlName, Type userControlType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		if(userControlName == "GuaranteesUserControl")
		{
			declaration.JE_MessageType = "IMP";
		}

		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			var userControl = control.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
			AssertEquals($"{userControlName}.UserControlType", userControlType, userControl.UserControlType);
		}
	}

	void AssertUserControlLabel(ZString userControlName, ZString userControlLabel)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();

		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl(declaration))
		{
			form.Controls.Add(control);
			form.Show();

			var userControl = control.FindSingle<ZTabPage>(userControlName);
			AssertEquals($"{userControlName}.Caption", userControlLabel, userControl.CaptionResourceString.Caption);
		}
	}
}
