using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	class BondedWarehouseOperationDeterminer : Customs.GUI.BondedWarehouseOperationDeterminer
	{
		public BondedWarehouseOperationDeterminer(JobDeclaration supporter)
			: base(supporter)
		{
		}

		protected new JobDeclaration supporter
		{
			get { return (JobDeclaration)base.supporter; }
		}

		#region Bonded Warehouse

		bool CannotUpdateBondedWarehouse()
		{
			var b3MessageStatus = supporter.B3EntryHeader?.CH_Status;
			return b3MessageStatus.HasValue && MessageStatusList.IsAwaiting(b3MessageStatus.Value);
		}

		protected override bool CanUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseInwardCheckCore();
			if (result && CannotUpdateBondedWarehouse())
			{
				result = false;
				Globals.Message.ShowError(Res.GetString("f571a351-996a-4219-802a-6a4f650bb9dc", "Cannot update Inventory stock levels while waiting for a response."));
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseOutwardCheckCore();
			if (result && CannotUpdateBondedWarehouse())
			{
				result = false;
				Globals.Message.ShowError(Res.GetString("074e519b-70b0-494f-ac93-19d577d1ae26", "Cannot update Inventory stock release while waiting for a response."));
			}
			return result;
		}

		protected override bool CanCancelBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanCancelBondedWarehouseOutwardCheckCore();
			if (result && CannotUpdateBondedWarehouse())
			{
				result = false;
				Globals.Message.ShowError(Res.GetString("ca842a27-dab6-4a56-86ff-cdc8d463c02c", "Cannot cancel Inventory stock release while waiting for a response."));
			}
			return result;
		}

		protected override bool CanCancelUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanCancelUpdateBondedWarehouseInwardCheckCore();
			if (result && CannotUpdateBondedWarehouse())
			{
				result = false;
				Globals.Message.ShowError(Res.GetString("ce54555e-901f-487c-86cc-fd3a09ef912b", "Cannot cancel Inventory stock levels while waiting for a response."));
			}
			return result;
		}

		#endregion
	}
}
