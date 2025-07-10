using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsAdjustmentLine : DocWhsDocketLine
	{
		#region Static

		public static DocWhsAdjustmentLine New(WhsAdjustmentLine whsAdjustmentLine, BusinessObjectFactory factoryToWrap)
		{
			return (whsAdjustmentLine == null) ? null : new DocWhsAdjustmentLine(whsAdjustmentLine, factoryToWrap);
		}

		#endregion

		#region Constructors

		DocWhsAdjustmentLine(WhsAdjustmentLine whsAdjustmentLine, BusinessObjectFactory factoryToWrap)
			: base(whsAdjustmentLine, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		WhsAdjustmentLine WhsAdjustmentLine
		{
			get { return (WhsAdjustmentLine)WrappedObject; }
		}

		public override DocWhsDocket Docket
		{
			get { return DocWhsAdjustment.New(WhsAdjustmentLine.Docket, Factory); }
		}

		#endregion

		public ZDateTime ArrivalDate
		{
			get { return WhsAdjustmentLine.WE_AdjustmentArrivalDate.ToZDateTime(); }
		}
	}
}
