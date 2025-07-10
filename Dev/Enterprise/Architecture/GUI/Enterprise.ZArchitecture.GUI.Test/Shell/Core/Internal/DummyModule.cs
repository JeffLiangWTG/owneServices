using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyModule : ZModule
	{
		public override ModuleIdentifier ID
		{
			get { return DummyModuleIDs.Dummy; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return DummyCheckPointWithSecuritySet; }
		}

		DummyCheckPointWithSecuritySet DummyCheckPointWithSecuritySet
		{
			get
			{
				if (fDummyCheckPointWithSecuritySet == null)
				{
					fDummyCheckPointWithSecuritySet = new DummyCheckPointWithSecuritySet(true);
				}
				return fDummyCheckPointWithSecuritySet;
			}
		}
		DummyCheckPointWithSecuritySet fDummyCheckPointWithSecuritySet;

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.Core; }
		}

		public override IZForm ShowPopup()
		{
			return null;
		}
	}
}
