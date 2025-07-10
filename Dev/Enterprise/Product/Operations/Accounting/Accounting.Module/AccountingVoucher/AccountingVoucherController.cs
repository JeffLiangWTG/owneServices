using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.Accounting.GUI.AccountingVoucherPrinting;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingVoucherController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccoutingVoucherPrintForm(new AccountingVoucherPrintWrapper());
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccountingVoucher; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccountingVoucherPrintWrapper); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new AccountingVoucherPrintWrapper();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AccountingVoucher; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}
	}
}
