using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module for JobManagement.
	/// </summary>
	public partial class JobManagementModuleBase : ZFilterGridModule
	{
		public JobManagementModuleBase()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobHeader; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobHeader);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobManagementFilterControlBase(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JobManagementCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobManagementFilterBusinessObjectBase();
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			return base.ShowEditForm(selectedBusinessObject);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			// Remove Unused Items
			menuItems.Remove(NewMenuItem);
			menuItems.Remove(EditMenuItem);
			menuItems.Remove(DeleteMenuItem);

			return menuItems.ToArray();
		}

		#region License and Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.JobManagement; }
		}

		#endregion
	}
}
