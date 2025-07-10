namespace CargoWise.Bi.Product.Manager.Controller
{
	using System;
	using CargoWise.Bi.Product.Manager.Business;
	using CargoWise.Bi.Product.Manager.GUI;
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;

	[WTG.StaticAnalysis.Annotation.CodeAlive("For BI Manager controller")]
	internal class BiManagerController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.BiManager; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BiMonitorBusinessObject); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new BiMonitorBusinessObject();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BiManager; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BiManagerForm((BiMonitorBusinessObject)businessEntity);
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.ReadOnly;
		}
	}
}
