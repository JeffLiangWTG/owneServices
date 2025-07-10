using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public class LicenceDatabaseModuleForRegistration : ZFilterGridModule
	{
		readonly FilterBusinessObject filterBusinessObject;
		readonly ZFilterStripControl filterStripControl;
		readonly IBusinessObjectCollection gridCollection;

		public LicenceDatabaseModuleForRegistration(ZFilterStripControl filterStripControl, IBusinessObjectCollection gridCollection, FilterBusinessObject filterBusinessObject)
			: base()
		{
			this.filterBusinessObject = filterBusinessObject;
			this.filterStripControl = filterStripControl;
			this.gridCollection = gridCollection;
		}

		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.LicenceDatabase;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Organisation;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(Modules.ClientControllerRegistration.LicenceDatabase);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => filterBusinessObject;
		protected override IFilterControl GetNewFilterControl() => filterStripControl;
		protected override IBusinessObjectCollection GetNewGridCollection() => gridCollection;
		public override bool AllowToggleFilterVisibilityMenuItem => false;

		public event EventHandler ShowNewFormClickEvent;

		protected override IZForm ShowNewForm()
		{
			var form = base.ShowNewForm();
			ShowNewFormClickEvent?.Invoke(form, null);
			return form;
		}
	}
}
