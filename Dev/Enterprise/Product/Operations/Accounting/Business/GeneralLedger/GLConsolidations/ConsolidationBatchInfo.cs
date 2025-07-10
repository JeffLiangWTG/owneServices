using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class ConsolidationBatchInfo
	{
		public ConsolidationBatchInfo(ZGuid company, ZGuid consolidationGroup, ZGuid periodPK, ZInt periodInt)
		{
			this.Company = company;
			this.ConsolidationGroup = consolidationGroup;
			this.PeriodPK = periodPK;
			this.PeriodInt = periodInt;
			BatchRows = new List<BatchRow>();
		}

		public ZGuid BatchPK;
		public ZDecimal BatchNumber;
		public readonly ZGuid PeriodPK;
		public readonly ZInt PeriodInt;
		public readonly ZGuid Company;
		public readonly ZGuid ConsolidationGroup;
		public readonly List<BatchRow> BatchRows;
	}

	public class BatchRow
	{
		public BatchRow(ZGuid parentID, ZString parentTableCode, ZString type)
		{
			this.ParentID = parentID;
			this.ParentTableCode = parentTableCode;
			this.Type = type;
		}

		public readonly ZGuid ParentID;
		public readonly ZString ParentTableCode;
		public readonly ZString Type;
	}
}
