using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobStatusUpdateRestrictionRuleLookups : ZLookups
	{
		public JobStatusUpdateRestrictionRuleLookups(JobStatusUpdateRestrictionRule parent) : base(parent)
		{
		}

		public JobHeaderStatusRestrictionList JobStatusList => new JobHeaderStatusRestrictionList();

		public YesNoList YesOrNoList => new YesNoList();

		public class YesNoList : CodeDescriptionPairList
		{
			public YesNoList()
			{
				AddPair(Codes.No, ResString.GetMultilingualString("139d6061-1417-4ad5-8a33-af217f934e19", "NO"));
				AddPair(Codes.Yes, ResString.GetMultilingualString("881620ff-1a81-4085-ae2e-fd931b4c45f7", "YES"));
			}

			public static class Codes
			{
				public const string No = "NO";
				public const string Yes = "YES";
			}
		}

		public class JobHeaderStatusRestrictionList : CodeDescriptionPairList
		{
			public JobHeaderStatusRestrictionList()
			{
				Add(JobHeaderStatus.Working);
				Add(JobHeaderStatus.WorkOnHold);
				Add(JobHeaderStatus.InvoiceOnHold);
				Add(JobHeaderStatus.CustomsProcessActive);
				Add(JobHeaderStatus.JobReadyForRevenueAndCostPosting);
				Add(JobHeaderStatus.JobReadyForRevenuePosting);
				Add(JobHeaderStatus.JobReadyForCostPosting);
				Add(JobHeaderStatus.JobReadyForDelivery);
				Add(JobHeaderStatus.JobInvoiced);
				Add(JobHeaderStatus.Complete);
				Add(JobHeaderStatus.JobReadyForFinancialClosure);
				Add(JobHeaderStatus.ScheduledForArchive);
			}
		}
	}
}
