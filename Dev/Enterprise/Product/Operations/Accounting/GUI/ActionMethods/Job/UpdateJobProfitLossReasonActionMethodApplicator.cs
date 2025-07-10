using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobProfitLossReasonActionMethodApplicator : UpdateJobActionMethodApplicatorBase
	{
		public UpdateJobProfitLossReasonActionMethodApplicator(BusinessObjectFactory factory) : base(factory,
			"UpdateJobProfitLossReasonActionMethodApplicator")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string ProfitLossReason = "ProfitLossReason";
		}

		#endregion

		protected override string[] UpdateJobProperty(JobHeader job)
		{
			var errors = Array.Empty<string>();

			job.JH_ProfitLossReasonCode = ProfitLossReason;
			if (job.JH_ProfitLossReasonCodeInfo.HasErrors())
			{
				errors = job.JH_ProfitLossReasonCodeInfo.GetErrors().Select(x => x.Message).ToArray();
			}

			return errors;
		}

		protected override bool GetPropertyReadonly(JobHeader job)
		{
			return job.JH_ProfitLossReasonCodeInfo.ReadOnly;
		}

		protected override bool GetValueSameWithPrevious(JobHeader job)
		{
			return job.JH_ProfitLossReasonCode == ProfitLossReason;
		}

		[List("ProfitLossReasonCodeList")]
		public ZString ProfitLossReason
		{
			get
			{
				return fProfitLossReason;
			}
			set
			{
				SetNonPersistentPropertyValue(ProfitLossReasonInfo, ref fProfitLossReason, value);
				if (!IsValidationSuspended)
				{
					ValidateProfitLossReason();
				}
			}
		}

		ZString fProfitLossReason;

		public ZPropertyInfo ProfitLossReasonInfo => GetZPropertyInfo(Schema.ProfitLossReason);

		public CodeDescriptionPairList ProfitLossReasonCodeList
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value
					.GetCodeDescriptionPairList();
			}
		}

		#region Validation

		protected override void ValidateAll()
		{
			ValidateProfitLossReason();
		}

		void ValidateProfitLossReason()
		{
			ProfitLossReasonInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ProfitLossReasonInfo, ProfitLossReasonCodeList);
		}

		#endregion
	}
}
