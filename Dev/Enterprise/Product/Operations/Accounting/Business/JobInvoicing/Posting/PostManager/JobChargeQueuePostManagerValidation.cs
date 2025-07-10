using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargeQueuePostManagerValidation : PostManagerValidation
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public JobChargeQueuePostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<(Job job, IEnumerable<Charge> charges)> jobsWithSelectedCharges, IEnumerable<IJobChargePostingQueue> jobChargePostingQueues)
			: base(jobs, postingOption, jobs)
		{
			Argument.NotNull(jobsWithSelectedCharges, nameof(jobsWithSelectedCharges));
			Argument.NotNull(jobChargePostingQueues, nameof(jobChargePostingQueues));

			this.JobsWithSelectedCharges = jobsWithSelectedCharges;
			this.JobChargePostingQueues = jobChargePostingQueues;
		}

		IEnumerable<(Job job, IEnumerable<Charge> charges)> JobsWithSelectedCharges
		{
			get;
#if DEBUG
			set;
#endif
		}
		IEnumerable<IJobChargePostingQueue> JobChargePostingQueues { get; }

		#region Override

		protected override bool ShouldAllowLoadingChargesDuringJobValidation => true;

		protected override IEnumerable<Job> GetJobsToValidate(IEnumerable<Job> jobs)
		{
			var result = base.GetJobsToValidate(jobs);

			foreach (var jobToValidate in result)
			{
				var jobWithSelectedCharges = JobsWithSelectedCharges.FirstOrDefault(x => x.job.PK == jobToValidate.PK);
				if (jobWithSelectedCharges != default)
				{
					using (jobToValidate.ChargesLoadSuspender.GetSuspender())
					{
						foreach (var chargeToValidate in jobWithSelectedCharges.charges)
						{
							if (jobToValidate.Charges.FindByPK(chargeToValidate.PK) == null)
							{
								jobToValidate.Charges.Add(chargeToValidate);
							}
						}
					}
				}
			}

			return result;
		}

		protected override IEnumerable<Charge> GetCharges(Job job)
		{
			var jobWithSelectdCharges = JobsWithSelectedCharges.FirstOrDefault(x => x.job.PK == job.PK);
			return jobWithSelectdCharges != default ? jobWithSelectdCharges.charges : Array.Empty<Charge>();
		}

		protected override bool IsAllowedToPostSellCharge(Charge charge)
		{
			return true;
		}

		protected override PostManagerNotification ValidateErrorNotificationCore()
		{
			var validationActions = new Func<PostManagerNotification>[] { RunJobChargesPostingQueueValidationHashMismatch,
																		  base.ValidateErrorNotificationCore,
																		  RunAdditionalJobChargesPostingQueueValidation };

			foreach (var action in validationActions)
			{
				var error = action();
				if (error != null)
				{
					return error;
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		PostManagerNotification RunJobChargesPostingQueueValidationHashMismatch()
		{
			Func<string, PostManagerNotification> createNotification = (x) => GetErrorNotification(x, PostManagerValidationType.JobChargePostingHashMismatch);

			if (JobChargePostingQueues.Any())
			{
				foreach (var job in Jobs)
				{
					var jobCharges = GetCharges(job);

					foreach (var charge in jobCharges)
					{
						var queueList = JobChargePostingQueues.Where(x => x.ChargePK == charge.PK);
						foreach (var queueRecord in queueList)
						{
							ZBlob hashValue = null;
							if (PostingOption == JobInvoicingPostingOption.Costs)
							{
								hashValue = charge.CalculateCostPartHash(queueRecord.HashVersion);
							}
							else if (PostingOption == JobInvoicingPostingOption.Revenue)
							{
								hashValue = charge.CalculateSellPartHash(queueRecord.HashVersion);
							}

							if (hashValue == null || queueRecord.ChargeValuesHash != hashValue)
							{
								return createNotification(ChargeHashInfoMisMatch);
							}
						}
					}
				}
			}

			return null;
		}

		PostManagerNotification RunAdditionalJobChargesPostingQueueValidation()
		{
			Func<string, PostManagerNotification> createNotification = (x) => GetErrorNotification(x, PostManagerValidationType.JobChargePostingAdditionalValidation);

			if (JobChargePostingQueues.Any() && PostingOption == JobInvoicingPostingOption.Costs)
			{
				foreach (var job in Jobs)
				{
					var jobCharges = GetCharges(job);
					var chargeGroups = jobCharges.Where(x => x.JR_OH_CostAccount != ZGuid.Empty && x.JR_APInvoiceNum != ZString.Empty).GroupBy(x => new { x.JR_OH_CostAccount, x.JR_APInvoiceNum });

					foreach (var group in chargeGroups)
					{
						var firstCharge = group.First();

						foreach (var charge in group.Except(firstCharge))
						{
							var notification = ValidateChargeDetails(firstCharge, charge, createNotification);
							if (notification != null)
							{
								return notification;
							}
						}

						var chargePKWithQueue = JobChargePostingQueues.Select(x => x.ChargePK).Distinct();

						var query = new ZQuery(JobChargeSchema.JR_GC, job.JH_GC);  // Add GC here to use NR_RX__JR_GC_JR_OH_CostAccount_JR_APInvoiceNum index.
						query.AddToFilter(JobChargeSchema.JR_OH_CostAccount, group.Key.JR_OH_CostAccount);
						query.AddToFilter(JobChargeSchema.JR_APInvoiceNum, group.Key.JR_APInvoiceNum);
						query.AddToFilter(JobChargeSchema.JR_JH, job.PK);
						query.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, chargePKWithQueue);

						var newFactory = new BusinessObjectFactory();
						var relatedCharges = newFactory.Load<Charge>(query).Where(x => !x.JR_IsCostPosted);  // JR_IsCostPosted is not a DB column,

						if (relatedCharges.Any())
						{
							return createNotification(NoPostingInstructionErrorMsg);
						}
					}
				}
			}

			return null;
		}

		PostManagerNotification ValidateChargeDetails(Charge sourceCharge, Charge targetCharge, Func<string, PostManagerNotification> createNotification)
		{
			if (sourceCharge.JR_APInvoiceDate != targetCharge.JR_APInvoiceDate)
			{
				return createNotification(InvoiceDateErrorMsg);
			}

			if (sourceCharge.JR_PaymentDate != targetCharge.JR_PaymentDate)
			{
				return createNotification(InvoiceDueDateErrorMsg);
			}

			if (sourceCharge.JR_PaymentType != targetCharge.JR_PaymentType)
			{
				return createNotification(PaymentMethodPaymentTypeErrorMsg);
			}

			if (sourceCharge.JR_AB != targetCharge.JR_AB)
			{
				return createNotification(PaymentMethodBankAccountErrorMsg);
			}

			if (sourceCharge.JR_AK != targetCharge.JR_AK)
			{
				return createNotification(PaymentMethodChequeBookErrorMsg);
			}

			if (sourceCharge.JR_ChequeNo != targetCharge.JR_ChequeNo)
			{
				return createNotification(PaymentMethodChequeNumberErrorMsg);
			}

			return null;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string InvoiceDateErrorMsg = "Charges of the job cannot be posted because charges for the same creditor and invoice number should have same invoice date.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string InvoiceDueDateErrorMsg = "Charges of the job cannot be posted because charges for the same creditor and invoice number should have same invoice due date.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string PaymentMethodPaymentTypeErrorMsg = "Charges of the job cannot be posted because charges for the same creditor and invoice number should have same payment type.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string PaymentMethodBankAccountErrorMsg = "Charges of the job cannot be posted because charges for the same creditor and invoice number should have same bank account.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string PaymentMethodChequeBookErrorMsg = "Charges of the job cannot be posted because charges for the same creditor and invoice number should have same cheque book.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string PaymentMethodChequeNumberErrorMsg = "Charges of the job cannot be posted because charges for the same creditor and invoice number should have same cheque number.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string NoPostingInstructionErrorMsg = "Charges of the job cannot be posted because some charges with the same creditor and invoice number do not have posting instruction.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is for service task")]
		const string ChargeHashInfoMisMatch = "The charges were changed after creating the posting queue.";

		#endregion
	}
}
