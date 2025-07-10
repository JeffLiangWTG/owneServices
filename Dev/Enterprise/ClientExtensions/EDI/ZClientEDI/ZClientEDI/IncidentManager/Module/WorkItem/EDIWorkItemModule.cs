using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIWorkItemModule : WorkItemModule
	{
#if WINZOR
		protected override System.Windows.Forms.MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new System.Collections.Generic.List<System.Windows.Forms.MenuItem>(base.GetNewActionMenuItems());

			menuItems.Add(new ZArchitecture.GUI.ZMenuItem(ZClientEDI.ResString.GetMultilingualString("71614DBE-09AD-459F-8776-4623529FFFDF", "Create Rotations Work Items"), CreateWorkItemsMenuItem_Click));

			return menuItems.ToArray();
		}

		void CreateWorkItemsMenuItem_Click(object sender, System.EventArgs e)
		{
			var form = new Client.EDI.IncidentManager.GUI.CreateRotationWorkItemsForm();
			form.Show();
		}
#endif

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIWorkItemFilterBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new EDIWorkItemFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override ModuleIdentifier GetRecentItemsModuleIDCore()
		{
			return ModuleIDs.WorkItem;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EDIWorkItemCollection(base.Factory);
		}
	}
}
