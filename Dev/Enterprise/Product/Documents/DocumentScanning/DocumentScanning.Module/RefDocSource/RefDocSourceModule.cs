using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	public class RefDocSourceModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefDocSource; }
		}

		#endregion

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.RefDocSource }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefDocSource);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefDocSourceFilterControl(GridCollection, (RefDocSourceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefDocSourceCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefDocSourceFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DocumentSources; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.DocManager; }
		}

		#endregion

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Remove(DeleteMenuItem);
			return menuItems.ToArray();
		}
	}
}
