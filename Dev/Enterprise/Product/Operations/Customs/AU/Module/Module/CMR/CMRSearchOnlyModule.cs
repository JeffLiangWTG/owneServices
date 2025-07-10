using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public abstract class CMRSearchOnlyModule : ZFilterGridModule
	{
		protected CMRSearchOnlyModule()
		{
		}

		public override ZBool HasActions
		{
			get { return ZBool.False; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
	}
}
