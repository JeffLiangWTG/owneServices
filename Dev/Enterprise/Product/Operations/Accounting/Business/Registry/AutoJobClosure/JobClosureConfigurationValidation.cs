using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class JobClosureConfigurationValidation : JobConfigurationSelectorValidation
	{
		public JobClosureConfigurationValidation(JobClosureConfiguration parent)
			: base(parent)
		{
		}
		protected new JobClosureConfiguration Parent
		{
			get { return (JobClosureConfiguration)base.Parent; }
		}

		#region Job Type
		public override void ValidateJobType()
		{
			base.ValidateJobType();

			if (!Parent.JobTypeInfo.HasErrors())
			{
				ValidateJobClosureDateOptionCode();
			}
		}
		#endregion

		#region Direction

		public override void ValidateDirectionCode()
		{
			base.ValidateDirectionCode();
			if (!Parent.DirectionCodeInfo.HasErrors())
			{
				CheckForDuplicate(Parent.DirectionCodeInfo);
			}
		}

		#endregion

		#region TransportMode

		public override void ValidateMode()
		{
			base.ValidateMode();
			if (!Parent.ModeInfo.HasErrors())
			{
				CheckForDuplicate(Parent.ModeInfo);
			}
		}

		#endregion

		#region JobClosureDateOptionCode

		public void ValidateJobClosureDateOptionCode()
		{
			MandatoryValidation.CheckEntered(Parent.JobClosureDateOptionCodeInfo, (IMultilingualString)ResString.GetMultilingualString("b2597708-3c89-425f-8979-f7073528a95e", "Job Closure Date Option"));
			ListValidation.ErrorIfInvalidCode(Parent.JobClosureDateOptionCodeInfo, Parent.JobClosureDateOptionList);

			if (!Parent.JobClosureDateOptionCodeInfo.HasErrors())
			{
				Parent.ValidateOffsetType();
				Parent.ValidateOffset();
			}
		}

		#endregion

		#region Offset

		public void ValidateOffset()
		{
			if (!Parent.OffsetInfo.ReadOnly)
			{
				if (Parent.Offset <= 0)
				{
					Parent.OffsetInfo.AddError(Res.GetString("fe8fe036-2cd0-4bf3-8513-501dc2f881ee", "Days Offset must be a non zero positive number"));
				}
			}
		}

		#endregion

		#region FromJobStatus

		public void ValidateFromJobStatus()
		{
			if (!Parent.FromJobStatus.IsEmpty)
			{
				var invalidCodes = Parent.GetFromJobStatusAsList().Select(x => x.ToString()).Except(GetValidJobHeaderStatusCodes());
				if (invalidCodes.Any())
				{
					Parent.FromJobStatusInfo.AddError(Res.GetString("fd6baba1-432c-4fb0-85e5-a06c671ac37d", "Invalid status code : {0}", string.Join(", ", invalidCodes)));
				}
			}

			if (!Parent.FromJobStatusInfo.HasErrors() && Parent.ConfigurationType == JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close)
			{
				var relatedItem = GetRelatedItem(JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
				if (relatedItem != null && Parent.FromJobStatus != relatedItem.ToJobStatus)
				{
					Parent.FromJobStatusInfo.AddError(Res.GetString("cc9d37ca-db6f-431e-b034-a4068b0959a2", "For combination (Job Type + Direction + Mode + Department), the 'To Status'(i.e. JFC) of Update Row Type does not match the 'From Status'(i.e. JFC) of the Close Row Type."));
				}
			}
		}

		string[] GetValidJobHeaderStatusCodes()
		{
			var validStatuses = new JobHeaderStatusList();
			validStatuses.RemoveCode(JobHeaderStatus.Closed);

			if (Parent.ConfigurationType == JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update)
			{
				validStatuses.RemoveCode(JobHeaderStatus.JobReadyForFinancialClosure);
			}

			return validStatuses.GetAllCodes();
		}

		#endregion

		#region Department

		public void ValidateDepartment()
		{
			if (Parent.DepartmentPK.IsValid)
			{
				ListValidation.ErrorIfInvalidPK(Parent.DepartmentPKInfo);
			}

			if (!Parent.DepartmentPKInfo.HasErrors())
			{
				CheckForDuplicate(Parent.DepartmentPKInfo);
			}
		}

		#endregion

		#region ReopenRestrictionOffset

		public void ValidateReopenRestrictionOffset()
		{
			if (Parent.ReopenRestrictionOffset < 0)
			{
				Parent.ReopenRestrictionOffsetInfo.AddError(Res.GetString("7AB4B3A6-5973-400D-B503-814618343C8A", "Reopen Restriction Days Offset must be a non positive number"));
			}
		}

		#endregion

		#region Offset Type

		public void ValidateOffsetType()
		{
			MandatoryValidation.CheckEntered(Parent.OffsetTypeInfo, JobConfigurationSelectorHelper.OffSetTypeText);
			ListValidation.ErrorIfInvalidCode(Parent.OffsetTypeInfo);
		}

		#endregion

		#region JobChargeRecognitionFilter

		public void ValidateJobChargeRecognitionFilter()
		{
			MandatoryValidation.CheckEntered(Parent.JobChargeRecognitionFilterInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JobChargeRecognitionFilterInfo);
		}

		#endregion

		#region Reopen Offset Type

		public void ValidateReopenRestrictionOffsetType()
		{
			MandatoryValidation.CheckEntered(Parent.ReopenRestrictionOffsetTypeInfo, JobConfigurationSelectorHelper.OffSetTypeText);
			ListValidation.ErrorIfInvalidCode(Parent.ReopenRestrictionOffsetTypeInfo);
		}

		#endregion

		#region Configuration Type

		public void ValidateConfigurationType()
		{
			MandatoryValidation.CheckEntered(Parent.ConfigurationTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ConfigurationTypeInfo);

			if (!Parent.ConfigurationTypeInfo.HasErrors())
			{
				CheckForDuplicate(Parent.ConfigurationTypeInfo);
			}

			if (!Parent.ConfigurationTypeInfo.HasErrors() && Parent.ConfigurationType == JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update)
			{
				var relatedItem = GetRelatedItem(JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
				if (relatedItem == null)
				{
					Parent.ConfigurationTypeInfo.AddError(Res.GetString("5ec32b7e-dbff-4147-a438-faef2ed0552e", "For each combination (Job Type + Direction + Mode + Department), there must always be a 'Close' row type."));
				}
			}
		}

		#endregion

		protected override bool IsDuplicateJobParameter(IJobConfigurationSelector item)
		{
			return ItemMatchesParent(item) &&
				Parent.ConfigurationType == (item as JobClosureConfiguration).ConfigurationType;
		}

		JobClosureConfiguration GetRelatedItem(string configurationType) =>
			Parent.ParentCollectionForValidation.Cast<JobClosureConfiguration>().FirstOrDefault(item =>
				item != Parent &&
				item.ConfigurationType == configurationType &&
				ItemMatchesParent(item));

		bool ItemMatchesParent(IJobConfigurationSelector item) =>
			Parent.JobType == item.JobType &&
			Parent.DirectionCode == item.DirectionCode &&
			Parent.Mode == item.Mode &&
			Parent.DepartmentPK == (item as JobClosureConfiguration).DepartmentPK;

		protected override string DuplicateJobParametersError
		{
			get
			{
				return Res.GetString("44A7D78B-4BA9-4364-93EC-2173A81DD5B6", "At least one more record already sets Job Closure Configuration for the same Job parameters.");
			}
		}
	}
}
