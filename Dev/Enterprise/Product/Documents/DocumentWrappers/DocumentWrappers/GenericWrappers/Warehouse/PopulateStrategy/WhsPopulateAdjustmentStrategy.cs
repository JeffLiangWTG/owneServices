using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsPopulateAdjustmentStrategy : WhsPopulateDocketStrategy
	{
		#region Constructor

		public WhsPopulateAdjustmentStrategy(BusinessObject docket)
			: base(docket)
		{
		}

		#endregion

		#region Properties

		public override LabelValuePairWrapper SecondaryReference
		{
			get { return new LabelValuePairWrapper(Res.GetString("225b810a-eaa7-44d9-a953-ad334d165d90", "Reference"), AdjustmentBO.WD_ExternalReference, Factory); }
		}

		#endregion

		#region Implementation

		WhsAdjustment AdjustmentBO
		{
			get { return adjustmentBO ?? (adjustmentBO = (WhsAdjustment)WrappedBO); }
		}
		WhsAdjustment adjustmentBO;

		#endregion
	}
}
