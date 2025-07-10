using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class AccConsolidationBatch : AutoAccConsolidationBatch
	{
		public AccConsolidationBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[RelatedBusinessObject("ConsolidationGroup")]
		public override CargoWise.Types.ZGuid YB_YR_ConsolidationGroup
		{
			get { return base.YB_YR_ConsolidationGroup; }
			set { base.YB_YR_ConsolidationGroup = value; }
		}

		public AccConsolidationGroup ConsolidationGroup
		{
			get { return Factory.Load<AccConsolidationGroup>(YB_YR_ConsolidationGroup); }
		}
	}
}