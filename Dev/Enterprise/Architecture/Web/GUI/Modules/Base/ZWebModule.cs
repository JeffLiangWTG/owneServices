using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Web.Modules
{
	/// <summary>
	/// Base class for alll web modules - tree view and Filter grid
	/// </summary>
	public abstract class ZWebModule : ZModule
	{
		public ZWebModule(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		protected BusinessObjectFactory Factory;

		protected internal BusinessObjectFactory FactoryInternal => Factory;

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}
	}
}
