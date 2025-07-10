using System;
using CargoWise.Data;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJobQueueResetter
	{
		void Reset();
	}

	public class JobQueueResetter : IJobQueueResetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "No BusinessObject for this data")]
		public void Reset()
		{
			using (var cmd = Db.Connection.Command("TRUNCATE TABLE JobToCloseQueue"))
			{
				cmd.ExecuteNonQuery();
			}
			AccountingConfigurationRegistry.Instance.NumberOfJobsProcessedForAutoClosingInTheCurrentBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}
	}
}
