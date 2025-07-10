using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseAdjustmentLineWrapper : WarehouseDocketLineWrapper
	{
		public WarehouseAdjustmentLineWrapper(WhsAdjustmentLine whsLineBO, BusinessObjectFactory factory)
			: base(whsLineBO, factory)
		{
		}

		#region Properties

		protected override ZString ReasonCodeCore
		{
			get
			{
				var docketLine = DocketLineBO;
				return docketLine != null ? docketLine.WE_ReasonCode : ZString.Empty;
			}
		}

		protected override ZString ReasonDescriptionCore
		{
			get
			{
				var docketLine = DocketLineBO;
				return docketLine != null ? docketLine.WE_ReasonDescription : ZString.Empty;
			}
		}

		#endregion

		#region Implementations

		protected override WhsDocket DocketBO
		{
			get { return (DocketLineBO != null) ? DocketLineBO.Docket : null; }
		}

		protected new WhsAdjustmentLine DocketLineBO
		{
			get { return (WhsAdjustmentLine)WrappedBO; }
		}

		#endregion
	}
}
