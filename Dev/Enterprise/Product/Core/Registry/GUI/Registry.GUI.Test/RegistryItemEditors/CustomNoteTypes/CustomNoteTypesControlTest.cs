using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CustomNoteTypesControl))]
	sealed class CustomNoteTypesControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CustomNoteTypes();
		}

		public void TestSelectingNodeInTree()
		{
			CustomNoteTypes bizO = new CustomNoteTypes();
			AssertEquals("precondition", String.Empty, bizO.CurrentSelectedModuleID);
			AssertEquals("precondition", String.Empty, bizO.CurrentSelectedCountryCode);

			using (ZForm testForm = new ZForm(bizO))
			{
				using (CustomNotesControlForTesting testControl = new CustomNotesControlForTesting())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();

					ZModulePointNode testNode = new ZModulePointNode("itemName", "displayText", null, "SG");
					testControl.SimulateNodeClick(new TreeViewEventArgs(testNode));
					AssertEquals(String.Empty, bizO.CurrentSelectedModuleID);
					AssertEquals("SG", bizO.CurrentSelectedCountryCode);
					AssertEquals(true, bizO.SelectedModule.CustomNoteTypesList.ReadOnly);

					testNode = new ZModulePointNode("itemName", "displayText", ModuleIDs.Organisation, "XB");
					testControl.SimulateNodeClick(new TreeViewEventArgs(testNode));
					AssertEquals(ModuleIDs.Organisation.Name, bizO.CurrentSelectedModuleID);
					AssertEquals("XB", bizO.CurrentSelectedCountryCode);
					AssertEquals(false, bizO.SelectedModule.CustomNoteTypesList.ReadOnly);
				}
			}
		}
	}
}
