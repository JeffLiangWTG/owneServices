using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class AutoRateInfoWrapperCollection : GenericWrapperCollection<AutoRateInfoWrapper>
	{
		#region Constructor

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AutoRateInfoWrapperCollection(Job job, BusinessObjectFactory factory)
			: base(factory)
		{
			if (job != null)
			{
				foreach (var autoRateInfo in job.AutoRatedInfosForJobRevenue)
				{
					Add(new AutoRateInfoWrapper(autoRateInfo, job.Factory));
				}

				foreach (var autoRateInfo in job.AutoRatedInfoGroupByChargeForJobRevenue)
				{
					TotalAutoRatedInfoGroupByCharge += autoRateInfo.Amount;
				}
			}
		}

		#endregion

		public ZDecimal TotalAutoRatedInfoGroupByCharge { get; private set; }
	}
}
