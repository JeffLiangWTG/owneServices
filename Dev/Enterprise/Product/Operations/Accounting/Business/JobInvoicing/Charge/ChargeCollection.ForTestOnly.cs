#if DEBUG

using System;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class ChargeCollection
	{
		public bool FetchOnlyFromLocalCache_ForTestOnly => FetchOnlyFromLocalCache;

		public bool ContainsDuplicateDisplaySequence_ForTestOnly => ContainsDuplicateDisplaySequence;

		public bool ContainsChargesFromChildJob_ForTestOnly => ContainsChargesFromChildJob;

		public ZShort GetBiggestSequenceNumber_ForTestOnly()
		{
			return GetBiggestSequenceNumber();
		}

		public Comparison<Charge> PostedCostSequenceSort_ForTestOnly
		{
			get { return PostedCostSequenceSort; }
			set { PostedCostSequenceSort = value; }
		}

		public ZGuid[] AdditionalJobsToLoadChargesFor_ForTestOnly
		{
			get { return AdditionalJobsToLoadChargesFor; }
			set { AdditionalJobsToLoadChargesFor = value; }
		}

		public bool AllowSort_ForTestOnly => AllowSort;
	}
}

#endif
