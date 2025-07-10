using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public abstract class AccQueryClaimModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override bool AllowDelete => false;

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#endregion
	}
}
