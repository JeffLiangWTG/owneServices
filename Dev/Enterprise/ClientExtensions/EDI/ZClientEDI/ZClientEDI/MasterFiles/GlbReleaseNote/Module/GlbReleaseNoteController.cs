#if DEBUG
using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Business;
using Enterprise.Client.EDI.Gui;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Modules
{
	public class GlbReleaseNoteController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.GlbReleaseNote; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbReleaseNoteManagerForSourceSafe); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new GlbReleaseNoteManagerForSourceSafe(Factory);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			GlbReleaseNoteManagerForSourceSafe manager = (GlbReleaseNoteManagerForSourceSafe)businessEntity;
			return new GlbReleaseNoteEditForm(manager);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}
	}
}
#endif
