using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public abstract class UpdateJobActionMethodApplicatorBase : OperationalActionMethodApplicator, IObsoleteValidation
	{
		protected UpdateJobActionMethodApplicatorBase(BusinessObjectFactory factory, string name) : base(
			name, factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0 || targets.Any(x => !(x is IJobInvoicingPlugIn)))
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("6113695f-9ea7-426b-943e-c64bde152237", "No target object with job selected."));
			}
			else
			{
				var hasError = false;
				foreach (IJobInvoicingPlugIn plugIn in targets)
				{
					var job = plugIn.InvoicingSupporter?.Job;
					if (job != null && !hasError)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
							Res.GetString("8d0f7a79-26b5-40fe-a65f-6e5de8a25047", "Job {0}: Start Process.", job.JH_JobNum));
						if (GetPropertyReadonly(job))
						{
							hasError = true;
							log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("37700f5f-e8e5-4045-a1df-cc9ca2177b5e", "The field is read-only in this job.") +
								System.Environment.NewLine);
						}
						else if (GetValueSameWithPrevious(job))
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("c6765b5f-5de7-49b1-9fd0-4da111bb7229", "Skipped, because the value you want to set is same with previous.") +
								System.Environment.NewLine);
						}
						else
						{
							var errors = UpdateJobProperty(job);
							if (errors.Any())
							{
								hasError = true;
								log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("08d1a5fb-5cee-491e-9924-e31cb83dbabc", "Job has errors: {0}", string.Join("\r\n", errors)) +
									System.Environment.NewLine);
							}
							else
							{
								log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("e1aedc40-a95b-4776-a259-37fa759b9256", "Job {0}: Processed.", job.JH_JobNum) +
									System.Environment.NewLine);
							}
						}
					}
				}
			}
		}

		protected abstract bool GetPropertyReadonly(JobHeader job);

		protected abstract bool GetValueSameWithPrevious(JobHeader job);

		protected abstract string[] UpdateJobProperty(JobHeader job);

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		protected abstract void ValidateAll();

		#endregion
	}
}
