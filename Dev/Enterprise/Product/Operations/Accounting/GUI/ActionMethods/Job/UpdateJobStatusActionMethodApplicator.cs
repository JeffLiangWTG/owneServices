using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobStatusActionMethodApplicator : UpdateJobActionMethodApplicatorBase
	{
		public UpdateJobStatusActionMethodApplicator(BusinessObjectFactory factory) : base(factory, "UpdateJobStatusActionMethodApplicator")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string StatusCode = "StatusCode";
		}

		#endregion

		protected override string[] UpdateJobProperty(JobHeader job)
		{
			var errors = Array.Empty<string>();

			job.JH_Status = StatusCode;
			if (job.JH_StatusInfo.HasErrors())
			{
				errors = job.JH_StatusInfo.GetErrors().Select(x => x.Message).ToArray();
			}

			return errors;
		}

		protected override bool GetPropertyReadonly(JobHeader job)
		{
			return job.JH_StatusInfo.ReadOnly;
		}

		protected override bool GetValueSameWithPrevious(JobHeader job)
		{
			return job.JH_Status == StatusCode;
		}

		[List("StatusCodeList")]
		public ZString StatusCode
		{
			get
			{
				return fStatusCode;
			}
			set
			{
				SetNonPersistentPropertyValue(StatusCodeInfo, ref fStatusCode, value);
				if (!IsValidationSuspended)
				{
					ValidateStatus();
				}
			}
		}
		ZString fStatusCode;

		public ZPropertyInfo StatusCodeInfo => GetZPropertyInfo(Schema.StatusCode);

		public CodeDescriptionPairList StatusCodeList
		{
			get { return Factory.GetCachedValue<JobHeaderStatusList>(); }
		}

		#region Validation

		protected override void ValidateAll()
		{
			ValidateStatus();
		}

		void ValidateStatus()
		{
			StatusCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StatusCodeInfo);
			ListValidation.ErrorIfInvalidCode(StatusCodeInfo, StatusCodeList);
		}

		#endregion
	}
}
