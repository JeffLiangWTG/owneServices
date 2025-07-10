using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Module.Testing
{
	[TestedType(typeof(Module))]
	public class ModuleTest : ZModuleBasherTest
	{
		public void TestFilterControl()
		{
			using (var module = new Module())
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<DeclarationFilterStripControl>(filterControl);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new Module())
			{
				AssertEquals(Env.Security.EuCustomsEmcs, module.SecurityCheckpoint);
			}
		}

		public void TestEMCSModuleAllows()
		{
			using (var module = new Module())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
				AssertEquals("module.AllowView", true, module.AllowView);
			}
		}

		public void TestToolBarButtons()
		{
			using (var module = new Module())
			{
				AssertEquals("Module should have 7 standard buttons", 7, module.ToolBarButtons.Length);
				AssertEquals("View", "View", module.ToolBarButtons[0].Text);
				AssertEquals("New", "New", module.ToolBarButtons[1].Text);
				AssertEquals("Edit", "Edit", module.ToolBarButtons[2].Text);
				AssertEquals("Copy", "Copy", module.ToolBarButtons[3].Text);
				AssertEquals("Delete", "Delete", module.ToolBarButtons[4].Text);
				AssertEquals("Actions", "Actions", module.ToolBarButtons[5].Text);
				AssertEquals("Hide/Show Filters", "Hide/Show Filters", module.ToolBarButtons[6].Text);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.EMCS;

		protected override bool HasController() => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType) => factory.NewWithValidTestData<EMCSJobDeclaration>();
	}
}
