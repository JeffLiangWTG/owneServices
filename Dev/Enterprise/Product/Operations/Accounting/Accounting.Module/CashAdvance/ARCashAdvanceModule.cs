using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class ARCashAdvanceModule : CashAdvanceModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ARCashAdvance;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ARCashAdvance);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARCashAdvanceFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ARCashAdvance; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(PrintMenuText, new EventHandler(HandlePrint)));

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalMenuItems());

			result.Add(new ZMenuItem(PrintMenuText, HandlePrint));

			return result.ToArray();
		}

		static MultilingualString PrintMenuText => ResString.GetMultilingualString("6CDA463B-3A72-42A6-B4C8-BA06A788B9FE", "&Print");

		protected override SecurityCheckpoint MarkAsCancelSecurityCheckpoint => Env.Security.ARCashAdvanceRequestCancel;
		protected override SecurityCheckpoint MarkAsPaidSecurityCheckpoint => Env.Security.ARCashAdvanceRequestMarkAsPaid;
		protected override SecurityCheckpoint MarkAsUnpaidSecurityCheckpoint => Env.Security.ARCashAdvanceRequestMarkAsUnPaid;
		protected override bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}
	}
}
