using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	public class RegistryController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Registry; }
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
			return new RegistryForm();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SystemRegistry; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}
