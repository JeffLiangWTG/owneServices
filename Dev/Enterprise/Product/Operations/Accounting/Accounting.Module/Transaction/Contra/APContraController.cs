using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APContraController : ContraController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APContra; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewPayablesContra; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewPayablesContra; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesContra; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReversePayablesContra; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		protected override IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			var result = (Contra)base.GetTopLevelBusinessObject(sourceEntity);

			if (result != null && result.ControllerLedger.IsEmpty)
			{
				result.ControllerLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			}

			return result;
		}
	}
}
