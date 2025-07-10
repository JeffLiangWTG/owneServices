using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARContraController : ContraController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARContra; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewReceivablesContra; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesContra; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesContra; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesContra; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
		}

		protected override IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			var result = (Contra)base.GetTopLevelBusinessObject(sourceEntity);

			if (result != null && result.ControllerLedger.IsEmpty)
			{
				result.ControllerLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			}

			return result;
		}
	}
}
