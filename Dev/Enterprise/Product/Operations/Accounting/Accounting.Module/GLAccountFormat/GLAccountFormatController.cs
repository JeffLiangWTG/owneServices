using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GLAccountFormat;
using Enterprise.Accounting.GUI.GLAccountFormat;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GLAccountFormatController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.GLAccountFormat; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GLAccountFormatter); }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new FormatChangeForm(new GLAccountFormatter());
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new GLAccountFormatter();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GLAccountFormatChange; }
		}

		#endregion
	}
}
