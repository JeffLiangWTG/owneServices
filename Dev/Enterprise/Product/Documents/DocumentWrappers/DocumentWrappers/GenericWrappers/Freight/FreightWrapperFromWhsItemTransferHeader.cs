using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromWhsItemTransferHeader : FreightWrapper
	{
		public FreightWrapperFromWhsItemTransferHeader(WhsItemTransferHeader transferHeader, BusinessObjectFactory factory)
			: base(transferHeader, factory)
		{
			TransferHeader = transferHeader ?? Factory.GetNull<WhsItemTransferHeader>();
		}

		readonly WhsItemTransferHeader TransferHeader;

		#region JobNumbers

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("b5e0257c-6d90-443e-b612-c243d9c2b6b9", "Transfer");
		}

		protected override ZString GetJobNumber()
		{
			return TransferHeader.WTH_ReferenceNumber;
		}

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WhsItemTransferHeaderWrapper(TransferHeader, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages() => WarehouseJob.Packages;

		#endregion
	}
}
