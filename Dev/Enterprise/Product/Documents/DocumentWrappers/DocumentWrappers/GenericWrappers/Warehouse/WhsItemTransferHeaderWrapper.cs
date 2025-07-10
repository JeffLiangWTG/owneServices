using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemTransferHeaderWrapper : WarehouseJobGenericWrapper
	{
		public WhsItemTransferHeaderWrapper(WhsItemTransferHeader transferHeader, BusinessObjectFactory factory)
			: base(transferHeader, factory)
		{
		}

		#region Headers

		protected override ZString JobNumberHeadingCore
		{
			get { return Res.GetString("8eafeef9-499a-4c50-82e7-502987ba6e74", "Transfer ID"); }
		}

		protected override ZString JobNumberCore
		{
			get { return TransferHeaderBO.WTH_ReferenceNumber; }
		}

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = TransferHeaderBO.Warehouse;
				return warehouse != null
					? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(WarehouseTitle, warehouse, Factory))
					: null;
			}
		}

		#endregion

		#region WarehouseName

		public override LabelValuePairWrapper WarehouseName
		{
			get
			{
				return new LabelValuePairWrapper(WarehouseTitle,
					TransferHeaderBO.Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty,
					Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("9f448d39-f071-429f-8b98-3c3794d64e53", "Warehouse");

		#endregion

		#region GetJobLines

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return TransferHeaderBO.Lines != null
				? new WhsItemTransferLineWrapperCollection(TransferHeaderBO.Lines, Factory)
				: new WhsItemTransferLineWrapperCollection(Factory);
		}

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = TransferHeaderBO.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();

			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region JobType

		protected override ZString JobTypeCore => TransferHeaderBO.WTH_TransferType;

		#endregion

		#region Implementation

		WhsItemTransferHeader TransferHeaderBO => transferHeaderBO ?? (transferHeaderBO = (WhsItemTransferHeader)WrappedBO);
		WhsItemTransferHeader transferHeaderBO;

		#endregion

		#region IsFinalised

		protected override ZBool IsFinalisedCore => TransferHeaderBO.WTH_IsFinalised;

		#endregion
	}
}
