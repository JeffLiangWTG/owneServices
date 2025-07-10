using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public abstract class ZReceiptController : AccountingTransactionController
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			SetArgsForNewForm(new[] { ModuleID?.ToString() });
			return new ReceiptForm((Receipt)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}
	}
}
