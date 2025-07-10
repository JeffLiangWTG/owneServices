using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class BondedWarehouseOperationDeterminer : Customs.GUI.BondedWarehouseOperationDeterminer
	{
		public BondedWarehouseOperationDeterminer(JobDeclaration supporter)
			: base(supporter)
		{
		}

		protected new JobDeclaration supporter
		{
			get { return (JobDeclaration)base.supporter; }
		}

		#region Bonded Warehouse Data

		protected override bool CanCancelBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanCancelBondedWarehouseOutwardCheckCore();
			if (CMRImportMessageStatusList.IsAwaitingResponse(supporter.JE_MessageStatus))
			{
				result = false;
				Globals.Message.ShowError("Cannot cancel Inventory stock release while waiting for a response.");
			}
			return result;
		}

		protected override bool CanCancelUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanCancelUpdateBondedWarehouseInwardCheckCore();
			if (CMRImportMessageStatusList.IsAwaitingResponse(supporter.JE_MessageStatus))
			{
				result = false;
				Globals.Message.ShowError("Cannot cancel Inventory stock levels update while waiting for a response.");
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseInwardCheckCore();
			if (CMRImportMessageStatusList.IsAwaitingResponse(supporter.JE_MessageStatus))
			{
				result = false;
				Globals.Message.ShowError("Cannot update Inventory stock levels while waiting for a response.");
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseOutwardCheckCore();
			if (CMRImportMessageStatusList.IsAwaitingResponse(supporter.JE_MessageStatus))
			{
				result = false;
				Globals.Message.ShowError("Cannot update Inventory stock release while waiting for a response.");
			}
			return result;
		}

		#endregion
	}
}
