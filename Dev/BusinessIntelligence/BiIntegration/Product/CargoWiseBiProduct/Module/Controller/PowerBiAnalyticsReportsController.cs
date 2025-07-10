namespace CargoWise.Bi.Product.Module.Controller
{
	using System;
	using CargoWise.Bi.Product.Module.GUI;
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;

	class PowerBiAnalyticsReportsController : ZPopupController
	{
		public PowerBiAnalyticsReportsController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.PowerBiAnalyticsReports; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new PowerBiReportsForm();
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AnalyticsReports; }
		}
	}
}
