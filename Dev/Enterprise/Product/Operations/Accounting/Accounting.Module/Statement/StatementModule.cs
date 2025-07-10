using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for Statement.
	/// </summary>
	public class StatementModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Statement; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.ReceivablesCollectionDocuments;
			}
		}

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ControllerIDs.Statement);
		}
	}
}
