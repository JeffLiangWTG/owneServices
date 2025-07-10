using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.DbRestoreKey
{
	public class DbRestoreKeyController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.DbRestoreKey; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DbRestoreKeyGenerator); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DbRestoreKeyForm((DbRestoreKeyGenerator)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new DbRestoreKeyGenerator(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}
	}
}
