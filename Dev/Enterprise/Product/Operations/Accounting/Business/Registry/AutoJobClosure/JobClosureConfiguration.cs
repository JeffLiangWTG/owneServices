using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobClosureConfiguration : ChargeGroupSetting, IJobConfigurationSelector, IRegistryWithLogs
	{
		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string JobClosureDateOptionCode = "JobClosureDateOptionCode";
			public const string Offset = "Offset";
			public const string ReopenRestrictionOffset = "ReopenRestrictionOffset";
			public const string OffsetType = "OffsetType";
			public const string FromJobStatus = "FromJobStatus";
			public const string DepartmentPK = "DepartmentPK";
			public const string CloseJobsWithOpenWip = "CloseJobsWithOpenWip";
			public const string CloseJobsWithOpenAcr = "CloseJobsWithOpenAcr";
			public const string JobChargeRecognitionFilter = "JobChargeRecognitionFilter";
			public const string ReopenRestrictionOffsetType = "ReopenRestrictionOffsetType";
			public const string ConfigurationType = "ConfigurationType";
			public const string ToJobStatus = "ToJobStatus";
			public const string Identifier = "Identifier";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobClosureConfiguration();
		}

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(JobClosureConfigurationCollection));
				return parentCollection != null ? parentCollection.Cast<JobClosureConfiguration>().ToArray() : Array.Empty<JobClosureConfiguration>();
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFromJobStatus();
			ValidateJobClosureDateOptionCode();
			ValidateOffset();
			ValidateReopenRestrictionOffset();
			ValidateOffsetType();
			ValidateConfigurationType();
		}

		public new JobClosureConfigurationValidation Validation
		{
			get { return (JobClosureConfigurationValidation)base.Validation; }
		}

		protected override JobConfigurationSelectorValidation GetNewValidation()
		{
			return new JobClosureConfigurationValidation(this);
		}

		#endregion

		#region Default Values

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			ReopenRestrictionOffsetType = OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
			Offset = 1;
			ReopenRestrictionOffset = 0;
			JobChargeRecognitionFilter = ChargeRecognitionFilterOptionList.Codes.ALL;
			ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close;
			Identifier = ZGuid.NewZGuid();
		}

		#endregion

		public new BusinessObjectFactory Factory => CurrentFactory;

		#region From Job Status

		[ResourceStringData("bba107e2-b8be-482b-833e-8dbe40e57ab1", Caption = "From Job Status")]
		[MaxLength(50)]
		public ZString FromJobStatus
		{
			get { return fromJobStatus; }
			set
			{
				SetNonPersistentPropertyValue(FromJobStatusInfo, ref fromJobStatus, value);
				if (!IsValidationSuspended)
				{
					ValidateFromJobStatus();
				}
			}
		}

		public ZPropertyInfo FromJobStatusInfo => GetZPropertyInfo(Schema.FromJobStatus);

		public void ValidateFromJobStatus()
		{
			FromJobStatusInfo.ClearAllNotifications();
			Validation.ValidateFromJobStatus();
		}

		public ZString[] GetFromJobStatusAsList() =>
			FromJobStatus.Split(",").Select(s => s.Trim()).ToArray();

		public bool IsJobInAllowedStatusToBeUpdated(Job job) =>
			FromJobStatus.IsEmpty || GetFromJobStatusAsList().Contains(job?.JH_Status ?? ZString.Empty);

		ZString fromJobStatus;

		#endregion

		#region Department

		[ResourceStringData("973f8ee1-04e8-4447-82d1-12527118ccff", Caption = "Department")]
		[List(nameof(Departments))]
		public ZGuid DepartmentPK
		{
			get { return departmentPK; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentPKInfo, ref departmentPK, value);
				if (!IsValidationSuspended)
				{
					ValidateDepartmentPK();
				}
			}
		}
		ZGuid departmentPK;

		public ZPropertyInfo DepartmentPKInfo => GetZPropertyInfo(Schema.DepartmentPK);

		public void ValidateDepartmentPK()
		{
			DepartmentPKInfo.ClearAllNotifications();
			Validation.ValidateDepartment();
		}

		public GlbDepartment Department => Factory.Load<GlbDepartment>(DepartmentPK);

		public GlbDepartmentCollection Departments => JobClosureConfigurationLookup.Departments;

		#endregion

		#region JobClosureDateOption

		[MaxLength(3)]
		[List(nameof(JobClosureDateOptionList))]
		public ZString JobClosureDateOptionCode
		{
			get { return fJobClosureDateOptionCode; }
			set
			{
				CheckMaximumLength(JobClosureDateOptionCodeInfo, value);
				SetNonPersistentPropertyValue(JobClosureDateOptionCodeInfo, ref fJobClosureDateOptionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateJobClosureDateOptionCode();
				}
			}
		}

		public ZPropertyInfo JobClosureDateOptionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JobClosureDateOptionCode); }
		}

		public void ValidateJobClosureDateOptionCode()
		{
			JobClosureDateOptionCodeInfo.ClearAllNotifications();

			Validation.ValidateJobClosureDateOptionCode();
		}

		ZString fJobClosureDateOptionCode;

		public ZString JobClosureDateOptionDescription
		{
			get
			{
				ICodeDescription recognitionDateOption = JobClosureConfigurationLookups.CompleteJobClosureDateOptionList[JobClosureDateOptionCode];
				return recognitionDateOption != null ? recognitionDateOption.Description : "";
			}
		}

		public ZPropertyInfo JobClosureDateOptionDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(JobClosureDateOptionDescription)); }
		}

		public CodeDescriptionPairList JobClosureDateOptionList => JobClosureConfigurationLookup.JobClosureDateOptionList;

		#endregion

		#region ReopenRestrictionOffset

		[ReadOnlyMember(nameof(ReopenRestrictionOffset_ReadOnly))]
		public ZInt ReopenRestrictionOffset
		{
			get { return fReopenRestrictionOffset; }
			set
			{
				SetNonPersistentPropertyValue(ReopenRestrictionOffsetInfo, ref fReopenRestrictionOffset, value);
				if (!IsValidationSuspended)
				{
					ValidateReopenRestrictionOffset();
				}
			}
		}

		public ZPropertyInfo ReopenRestrictionOffsetInfo
		{
			get { return GetZPropertyInfo(Schema.ReopenRestrictionOffset); }
		}

		public void ValidateReopenRestrictionOffset()
		{
			ReopenRestrictionOffsetInfo.ClearAllNotifications();

			Validation.ValidateReopenRestrictionOffset();
		}

		ZInt fReopenRestrictionOffset;

		bool ReopenRestrictionOffset_ReadOnly => IsUpdate;

		#endregion

		#region Offset

		[ResourceStringData("7ca175cd-ed5d-4ac2-9934-82252eae6b60", Caption = "Offset")]
		public ZInt Offset
		{
			get { return OffsetInfo.ReadOnly ? (ZInt)1 : fOffset; }
			set
			{
				SetNonPersistentPropertyValue(OffsetInfo, ref fOffset, value);
				if (!IsValidationSuspended)
				{
					ValidateOffset();
				}
			}
		}

		public ZPropertyInfo OffsetInfo
		{
			get { return GetZPropertyInfo(Schema.Offset); }
		}

		public void ValidateOffset()
		{
			OffsetInfo.ClearAllNotifications();

			Validation.ValidateOffset();
		}

		ZInt fOffset;

		#endregion

		#region Offset Type

		[ResourceStringData("16232e87-2ea6-4b5a-bd3f-a9b6ca572c60", Caption = "Offset Type")]
		[List(nameof(OffSetTypes))]
		public ZString OffsetType
		{
			get { return offsetType; }
			set
			{
				SetNonPersistentPropertyValue(OffsetTypeInfo, ref offsetType, value);
				if (!IsValidationSuspended)
				{
					ValidateOffsetType();
				}
			}
		}
		ZString offsetType;

		public ZPropertyInfo OffsetTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OffsetType); }
		}

		public void ValidateOffsetType()
		{
			OffsetTypeInfo.ClearAllNotifications();
			Validation.ValidateOffsetType();
		}

		public CodeDescriptionPairList OffSetTypes => JobClosureConfigurationLookup.OffSetTypes;

		#endregion

		#region CloseJobsWithOpenWip

		[ReadOnlyMember(nameof(CloseJobsWithOpenWip_ReadOnly))]
		[ResourceStringData("ba695baa-9c40-420c-8f45-3983a9558e80", Caption = "Open WIPs")]
		public ZBool CloseJobsWithOpenWip
		{
			get { return closeJobsWithOpenWip; }
			set { SetNonPersistentPropertyValue(CloseJobsWithOpenWipInfo, ref closeJobsWithOpenWip, value); }
		}

		public ZPropertyInfo CloseJobsWithOpenWipInfo => GetZPropertyInfo(Schema.CloseJobsWithOpenWip);
		ZBool closeJobsWithOpenWip;

		bool CloseJobsWithOpenWip_ReadOnly => IsUpdate;

		#endregion

		#region CloseJobsWithOpenAcr

		[ReadOnlyMember(nameof(CloseJobsWithOpenAcr_ReadOnly))]
		[ResourceStringData("a30d8ebc-342c-4c0c-b8e2-e462bf280886", Caption = "Open Accruals")]
		public ZBool CloseJobsWithOpenAcr
		{
			get { return closeJobsWithOpenAcr; }
			set { SetNonPersistentPropertyValue(CloseJobsWithOpenAcrInfo, ref closeJobsWithOpenAcr, value); }
		}

		public ZPropertyInfo CloseJobsWithOpenAcrInfo => GetZPropertyInfo(Schema.CloseJobsWithOpenAcr);
		ZBool closeJobsWithOpenAcr;

		bool CloseJobsWithOpenAcr_ReadOnly => IsUpdate;

		#endregion

		#region JobChargeRecognitionFilter

		[ResourceStringData("b36d53d9-a44e-4fec-b3cf-2b5a48ed88c7", Caption = "Recognized Charges")]
		[List(nameof(JobChargeRecognitionFilterOptions))]
		[ReadOnlyMember(nameof(JobChargeRecognitionFilter_ReadOnly))]
		public ZString JobChargeRecognitionFilter
		{
			get { return jobChargeRecognitionFilter; }
			set
			{
				SetNonPersistentPropertyValue(JobChargeRecognitionFilterInfo, ref jobChargeRecognitionFilter, value);
				if (!IsValidationSuspended)
				{
					ValidateJobChargeRecognitionFilter();
				}
			}
		}
		ZString jobChargeRecognitionFilter;

		public ZPropertyInfo JobChargeRecognitionFilterInfo => GetZPropertyInfo(Schema.JobChargeRecognitionFilter);

		public void ValidateJobChargeRecognitionFilter()
		{
			JobChargeRecognitionFilterInfo.ClearAllNotifications();
			Validation.ValidateJobChargeRecognitionFilter();
		}

		public CodeDescriptionPairList JobChargeRecognitionFilterOptions => JobClosureConfigurationLookup.JobChargeRecognitionFilterOptions;

		bool JobChargeRecognitionFilter_ReadOnly => IsUpdate;

		#endregion

		#region Reopen Offset Type

		[ReadOnlyMember(nameof(ReopenRestrictionOffsetType_ReadOnly))]
		[ResourceStringData("ef52a673-389b-4da1-94e9-78298205da43", Caption = "Re-Open Restriction Offset Type")]
		[List(nameof(OffSetTypes))]
		public ZString ReopenRestrictionOffsetType
		{
			get { return reopenRestrictionOffsetType; }
			set
			{
				SetNonPersistentPropertyValue(ReopenRestrictionOffsetTypeInfo, ref reopenRestrictionOffsetType, value);
				if (!IsValidationSuspended)
				{
					ValidateReopenRestrictionOffsetType();
				}
			}
		}
		ZString reopenRestrictionOffsetType;

		public ZPropertyInfo ReopenRestrictionOffsetTypeInfo => GetZPropertyInfo(Schema.ReopenRestrictionOffsetType);

		public void ValidateReopenRestrictionOffsetType()
		{
			ReopenRestrictionOffsetTypeInfo.ClearAllNotifications();
			Validation.ValidateReopenRestrictionOffsetType();
		}

		bool ReopenRestrictionOffsetType_ReadOnly => IsUpdate;

		#endregion

		#region JobClosureConfigurationLookups

		public JobClosureConfigurationLookups JobClosureConfigurationLookup
		{
			get { return (JobClosureConfigurationLookups)ChargeGroupSettingLookups; }
		}

		protected override JobConfigurationSelectorLookups GetNewLookups()
		{
			return new JobClosureConfigurationLookups(this);
		}

		#endregion

		#region Configuration Type

		[ResourceStringData("d1a5ee2f-9df1-42b4-9f24-24d50d2c99e1", Caption = "Update/Close")]
		[List(nameof(ConfigurationTypeList))]
		public ZString ConfigurationType
		{
			get { return configurationType; }
			set
			{
				SetNonPersistentPropertyValue(ConfigurationTypeInfo, ref configurationType, value);
				if (!IsValidationSuspended)
				{
					ValidateConfigurationType();
				}

				UpdateToJobStatus();
			}
		}
		ZString configurationType;

		public ZPropertyInfo ConfigurationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ConfigurationType); }
		}

		public void ValidateConfigurationType()
		{
			ConfigurationTypeInfo.ClearAllNotifications();
			Validation.ValidateConfigurationType();
		}

		public CodeDescriptionPairList ConfigurationTypeList => JobClosureConfigurationLookup.ConfigurationTypeList;

		bool IsUpdate => ConfigurationType == JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;

		void UpdateToJobStatus()
		{
			if (ConfigurationType == JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close)
			{
				ToJobStatus = JobHeaderStatus.Closed.Code;
			}
			else if (ConfigurationType == JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update)
			{
				ToJobStatus = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			}
		}

		#endregion

		#region To Job Status

		[ResourceStringData("b31a394c-67a8-4824-9501-42596a50e6ff", Caption = "To Job Status")]
		[ReadOnly(true)]
		public ZString ToJobStatus
		{
			get { return toJobStatus; }
			set
			{
				SetNonPersistentPropertyValue(ToJobStatusInfo, ref toJobStatus, value);
			}
		}
		ZString toJobStatus;

		public ZPropertyInfo ToJobStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ToJobStatus); }
		}

		#endregion

		#region Identifier

		public ZGuid Identifier
		{
			get { return identifier; }
			set
			{
				SetNonPersistentPropertyValue(IdentifierInfo, ref identifier, value);
			}
		}
		ZGuid identifier;

		public ZPropertyInfo IdentifierInfo
		{
			get { return GetZPropertyInfo(Schema.Identifier); }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobClosureDateOptionCode, JobClosureDateOptionCode);
			writer.WriteElementString(Schema.Offset, Offset.ToString());
			writer.WriteElementString(Schema.ReopenRestrictionOffset, ReopenRestrictionOffset.ToString());
			writer.WriteElementString(Schema.DepartmentPK, DepartmentPK.IsValid ? DepartmentPK.ToString() : null);
			writer.WriteElementString(Schema.CloseJobsWithOpenAcr, CloseJobsWithOpenAcr.ToString());
			writer.WriteElementString(Schema.CloseJobsWithOpenWip, CloseJobsWithOpenWip.ToString());
			writer.WriteElementString(Schema.JobChargeRecognitionFilter, JobChargeRecognitionFilter);
			writer.WriteElementString(Schema.OffsetType, OffsetType.ToString());
			writer.WriteElementString(Schema.ReopenRestrictionOffsetType, ReopenRestrictionOffsetType.ToString());
			writer.WriteElementString(Schema.FromJobStatus, FromJobStatus);
			writer.WriteElementString(Schema.ConfigurationType, ConfigurationType);
			writer.WriteElementString(Schema.ToJobStatus, ToJobStatus);
			writer.WriteElementString(Schema.Identifier, Identifier.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			JobClosureDateOptionCode = reader.ReadElementString(Schema.JobClosureDateOptionCode);
			Offset = reader.ReadElementStringAsZInt(Schema.Offset);
			ReopenRestrictionOffset = reader.ReadElementStringAsZInt(Schema.ReopenRestrictionOffset);
			DepartmentPK = new ZGuid(reader.ReadElementString(Schema.DepartmentPK));
			CloseJobsWithOpenAcr = reader.ReadElementStringAsZBool(Schema.CloseJobsWithOpenAcr);
			CloseJobsWithOpenWip = reader.ReadElementStringAsZBool(Schema.CloseJobsWithOpenWip);
			JobChargeRecognitionFilter = reader.ReadElementString(Schema.JobChargeRecognitionFilter);
			OffsetType = reader.ReadElementString(Schema.OffsetType);
			ReopenRestrictionOffsetType = reader.ReadElementString(Schema.ReopenRestrictionOffsetType);
			FromJobStatus = reader.ReadElementString(Schema.FromJobStatus);
			ConfigurationType = reader.ReadElementString(Schema.ConfigurationType);
			ToJobStatus = reader.ReadElementString(Schema.ToJobStatus);
			Identifier = new ZGuid(reader.ReadElementString(Schema.Identifier));
		}

		#endregion

		#region RegistryChangeLogger

		static string AddEventName => Res.GetString("822d03a6-8bd9-472a-b0eb-bed5e910db1b", "Configuration Added");
		static string EditEventName => Res.GetString("a86709a7-150d-482d-8d06-ff743d60e66f", "Configuration Changed");
		static string DeleteEventName => Res.GetString("2879e74d-fcdd-41f2-88de-068e5a9fd474", "Configuration Deleted");

		public bool IsSameItem(IRegistryWithLogs item) => Identifier == (item as JobClosureConfiguration).Identifier;

		public bool IsEqual(IRegistryWithLogs item)
		{
			var result = true;
			var expectJobClosureConfiguration = item as JobClosureConfiguration;
			foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All))
			{
				if (this[propertyInfo.Name]?.ToString() != expectJobClosureConfiguration[propertyInfo.Name]?.ToString())
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public string GetLogText(RegistryChangeLogger.EventType eventType)
		{
			return Res.GetString("05922ac6-12df-48ee-b040-86a6ef21a497", "{0}: Job Type = {1}, Direction = {2}, Mode = {3}, Dept = {4}, Update/Close = {5}, From Status = {6}, To Status = {7}, Open Accruals = {8}, Open WIPs = {9}, Recognized Charges = {10}, Relevant Date = {11}, Offset = {12}, Offset Type = {13}, Reopen Offset = {14}, Reopen Offset Type = {15}",
				GetLogEventName(eventType),
				JobType,
				DirectionCode,
				Mode,
				Department?.GE_Code,
				ConfigurationTypeList.GetDescriptionFromCode(ConfigurationType),
				FromJobStatus,
				ToJobStatus,
				((bool)CloseJobsWithOpenAcr).ToYesNoString(),
				((bool)CloseJobsWithOpenWip).ToYesNoString(),
				JobChargeRecognitionFilter,
				JobClosureDateOptionCode,
				Offset,
				OffsetType,
				ReopenRestrictionOffset,
				ReopenRestrictionOffsetType);
		}

		string GetLogEventName(RegistryChangeLogger.EventType eventType)
		{
			switch (eventType)
			{
				case RegistryChangeLogger.EventType.Add:
					return AddEventName;
				case RegistryChangeLogger.EventType.Delete:
					return DeleteEventName;
				case RegistryChangeLogger.EventType.Edit:
					return EditEventName;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognized Registry Change Logger Event Type: {0}", eventType.ToString()));
			}
		}

		#endregion
	}
}
