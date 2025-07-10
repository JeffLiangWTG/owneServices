using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Schedule
{
	[CodeProperty(Schema.S5_ScheduleType), DescriptionProperty(Schema.S5_ScheduleDescription)]
	public class ArchiveScheduleTask : StmScheduleTask, IArchiveScheduleTask, IArchiveSchedule, IDocManagerSupport
	{
		public const string VersionFlag = "JsonV1.0";
		public ArchiveScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[List("ArchiveSystemList")]
		public override ZString S5_ScheduleType
		{
			get
			{
				return base.S5_ScheduleType;
			}
			set
			{
				base.S5_ScheduleType = value;

				if (ArchiveSystemList.ContainsCode(value))
				{
					S5_ScheduleDescription = ArchiveSystemList.GetMultilingualDescriptionFromCode(value).GetUnresolvedString();
				}
			}
		}

		public CodeDescriptionPairList ArchiveSystemList
		{
			get
			{
				if (archiveSystemList == null)
				{
					archiveSystemList = new CodeDescriptionPairList();

					var manager = NewArchiveManager();
					foreach (var system in manager.ArchiveSystemDescriptors)
					{
						archiveSystemList.AddPair(system.Code, system.Name);
					}
				}

				return archiveSystemList;
			}
		}
		CodeDescriptionPairList archiveSystemList;

		[TranslatableDataField(Schema.TableName, Schema.S5_ScheduleDescription, Type = typeof(StmScheduleTask), Asmid = ResString.AssemblyId)]
		public override ZString S5_ScheduleDescription
		{
			get => base.S5_ScheduleDescription;
			set => base.S5_ScheduleDescription = value;
		}

		public new MultilingualString S5_ScheduleDescriptionMultilingual
			=> GetMultilingual(S5_ScheduleDescriptionInfo);

		#region IsArchiveRecordsOnOrBeforeDate

		public ZBool IsArchiveRecordsOnOrBeforeDate
		{
			get
			{
				return isArchiveRecordsOnOrBeforeDate;
			}
			set
			{
				if (isArchiveRecordsOnOrBeforeDate != value)
				{
					isArchiveRecordsOnOrBeforeDate = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					IsArchiveRecordsOnOrBeforeDateInfo.RefreshBinding();
				}
			}
		}
		ZBool isArchiveRecordsOnOrBeforeDate;

		public ZPropertyInfo IsArchiveRecordsOnOrBeforeDateInfo
			=> GetZPropertyInfo(nameof(IsArchiveRecordsOnOrBeforeDate));

		#endregion

		#region IsArchiveRecordsOnOrBeforeRelativeDate

		public ZBool IsArchiveRecordsOnOrBeforeRelativeDate
		{
			get
			{
				return isArchiveRecordsOnOrBeforeRelativeDate;
			}
			set
			{
				if (isArchiveRecordsOnOrBeforeRelativeDate != value)
				{
					isArchiveRecordsOnOrBeforeRelativeDate = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					IsArchiveRecordsOnOrBeforeRelativeDateInfo.RefreshBinding();
				}
			}
		}
		ZBool isArchiveRecordsOnOrBeforeRelativeDate;

		public ZPropertyInfo IsArchiveRecordsOnOrBeforeRelativeDateInfo
			=> GetZPropertyInfo(nameof(IsArchiveRecordsOnOrBeforeRelativeDate));

		#endregion

		#region ArchiveRecordsOnOrBeforeDate

		public ZDateTime ArchiveRecordsOnOrBeforeDate
		{
			get
			{
				return archiveRecordsOnOrBeforeDate;
			}
			set
			{
				if (archiveRecordsOnOrBeforeDate != value)
				{
					archiveRecordsOnOrBeforeDate = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					ArchiveRecordsOnOrBeforeDateInfo.RefreshBinding();
				}
			}
		}
		ZDateTime archiveRecordsOnOrBeforeDate;

		public ZPropertyInfo ArchiveRecordsOnOrBeforeDateInfo
			=> GetZPropertyInfo(nameof(ArchiveRecordsOnOrBeforeDate));

		#endregion

		#region ArchiveRecordsOnOrBeforeNumber

		public ZInt ArchiveRecordsOnOrBeforeNumber
		{
			get
			{
				return archiveRecordsOnOrBeforeNumber;
			}
			set
			{
				if (archiveRecordsOnOrBeforeNumber != value)
				{
					archiveRecordsOnOrBeforeNumber = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					ArchiveRecordsOnOrBeforeNumberInfo.RefreshBinding();
				}
			}
		}
		ZInt archiveRecordsOnOrBeforeNumber;

		public ZPropertyInfo ArchiveRecordsOnOrBeforeNumberInfo
			=> GetZPropertyInfo(nameof(ArchiveRecordsOnOrBeforeNumber));

		#endregion

		#region DateParameter

		[List("DateParameterList")]
		[MaxLength(3)]
		public ZString DateParameter
		{
			get
			{
				return dateParameter;
			}
			set
			{
				if (dateParameter != value)
				{
					CheckMaximumLength(DateParameterInfo, value);
					dateParameter = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					DateParameterInfo.RefreshBinding();
				}
			}
		}
		ZString dateParameter;

		public ZPropertyInfo DateParameterInfo
		{
			get
			{
				if (dateParameterInfo == null)
				{
					dateParameterInfo = GetZPropertyInfo(nameof(DateParameter));
				}

				return dateParameterInfo;
			}
		}
		ZPropertyInfo dateParameterInfo;

		public CodeDescriptionPairList DateParameterList
		{
			get
			{
				if (dateParameterList == null)
				{
					dateParameterList = new CodeDescriptionPairList();
					dateParameterList.AddPair(DateParameterStrings.GetCode(DateParameterType.JCL), Res.GetString("f46a9054-c912-4a6c-af07-db463d240feb", "Job Close Date"));
					dateParameterList.AddPair(DateParameterStrings.GetCode(DateParameterType.JOP), Res.GetString("c3a5d5d4-2740-4e55-9f90-bc646aa89723", "Job Open Date"));
				}

				return dateParameterList;
			}
		}
		CodeDescriptionPairList dateParameterList;

		#endregion

		#region ArchiveRecordsOnOrBeforeType

		[List("ArchiveRecordsOnOrBeforeTypeList")]
		[MaxLength(1)]
		public ZString ArchiveRecordsOnOrBeforeType
		{
			get
			{
				return archiveRecordsOnOrBeforeType;
			}
			set
			{
				if (archiveRecordsOnOrBeforeType != value)
				{
					CheckMaximumLength(ArchiveRecordsOnOrBeforeTypeInfo, value);
					archiveRecordsOnOrBeforeType = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					ArchiveRecordsOnOrBeforeTypeInfo.RefreshBinding();
				}
			}
		}
		ZString archiveRecordsOnOrBeforeType;

		public ZPropertyInfo ArchiveRecordsOnOrBeforeTypeInfo
		{
			get
			{
				if (archiveRecordsOnOrBeforeTypeInfo == null)
				{
					archiveRecordsOnOrBeforeTypeInfo = GetZPropertyInfo(nameof(ArchiveRecordsOnOrBeforeType));
				}

				return archiveRecordsOnOrBeforeTypeInfo;
			}
		}
		ZPropertyInfo archiveRecordsOnOrBeforeTypeInfo;

		public CodeDescriptionPairList ArchiveRecordsOnOrBeforeTypeList
		{
			get
			{
				if (archiveRecordsOnOrBeforeTypeList == null)
				{
					archiveRecordsOnOrBeforeTypeList = new CodeDescriptionPairList();
					archiveRecordsOnOrBeforeTypeList.AddPair("D", Res.GetString("171c7d61-3625-40b1-9f18-7bc56df9c8e7", "Day(s) ago"));
					archiveRecordsOnOrBeforeTypeList.AddPair("W", Res.GetString("280ba9fb-fb30-4dd0-9647-1151604b82e6", "Week(s) ago"));
					archiveRecordsOnOrBeforeTypeList.AddPair("M", Res.GetString("350eb2f6-7091-4a44-9d0a-bc519c79ff18", "Month(s) ago"));
					archiveRecordsOnOrBeforeTypeList.AddPair("Y", Res.GetString("40eee3ed-e40d-4ebc-8df0-552720e7c2f9", "Year(s) ago"));
				}

				return archiveRecordsOnOrBeforeTypeList;
			}
		}
		CodeDescriptionPairList archiveRecordsOnOrBeforeTypeList;

		#endregion

		#region MaxRunDurationInMinutes

		public ZInt MaxRunDurationInMinutes
		{
			get
			{
				return maxRunDurationInMinutes;
			}
			set
			{
				if (maxRunDurationInMinutes != value)
				{
					maxRunDurationInMinutes = value;
					S5_ScheduleState = ZBlob.Empty;
					HasChanges = true;
					MaxRunDurationInMinutesInfo.RefreshBinding();
				}
			}
		}
		ZInt maxRunDurationInMinutes;

		public ZPropertyInfo MaxRunDurationInMinutesInfo
			=> GetZPropertyInfo(nameof(MaxRunDurationInMinutes));

		#endregion

		#region IsVerboseLog

		public ZBool IsVerboseLog
		{
			get { return isVerboseLog; }
			set
			{
				if (isVerboseLog != value)
				{
					isVerboseLog = value;
					HasChanges = true;
					IsVerboseLogInfo.RefreshBinding();
				}
			}
		}
		ZBool isVerboseLog;

		public ZPropertyInfo IsVerboseLogInfo
			=> GetZPropertyInfo(nameof(IsVerboseLog));

		#endregion

		#region UseOnOrBeforeDateWhenWatermarkReset

		public ZBool UseOnOrBeforeDateWhenWatermarkReset
		{
			get { return useOnOrBeforeDateWhenWatermarkReset; }
			set
			{
				if (useOnOrBeforeDateWhenWatermarkReset != value)
				{
					useOnOrBeforeDateWhenWatermarkReset = value;
					HasChanges = true;
					UseOnOrBeforeDateWhenWatermarkResetInfo.RefreshBinding();
				}
			}
		}
		ZBool useOnOrBeforeDateWhenWatermarkReset;

		public ZPropertyInfo UseOnOrBeforeDateWhenWatermarkResetInfo
			=> GetZPropertyInfo(nameof(UseOnOrBeforeDateWhenWatermarkReset));

		#endregion

		#region ShouldArchiveDeclaration

		public ZBool ShouldArchiveDeclaration
		{
			get
			{
				return shouldArchiveDeclaration;
			}
			set
			{
				if (shouldArchiveDeclaration != value)
				{
					shouldArchiveDeclaration = value;
					HasChanges = true;
					ShouldArchiveDeclarationInfo.RefreshBinding();
				}
			}
		}
		ZBool shouldArchiveDeclaration;

		public ZPropertyInfo ShouldArchiveDeclarationInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldArchiveDeclaration)); }
		}

		#endregion

		#region ShouldIncludeRecordsWithoutJobs

		public ZBool ShouldIncludeRecordsWithoutJobs
		{
			get
			{
				return shouldIncludeRecordsWithoutJobs;
			}
			set
			{
				if (shouldIncludeRecordsWithoutJobs != value)
				{
					shouldIncludeRecordsWithoutJobs = value;
					HasChanges = true;
					ShouldIncludeRecordsWithoutJobsInfo.RefreshBinding();
				}
			}
		}
		ZBool shouldIncludeRecordsWithoutJobs;

		public ZPropertyInfo ShouldIncludeRecordsWithoutJobsInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldIncludeRecordsWithoutJobs)); }
		}

		#endregion

		#region ShouldIncludeRecordsWithJobs

		public ZBool ShouldIncludeRecordsWithJobs
		{
			get
			{
				return shouldIncludeRecordsWithJobs;
			}
			set
			{
				if (shouldIncludeRecordsWithJobs != value)
				{
					shouldIncludeRecordsWithJobs = value;
					HasChanges = true;
					ShouldIncludeRecordsWithJobsInfo.RefreshBinding();
				}
			}
		}
		ZBool shouldIncludeRecordsWithJobs = true;

		public ZPropertyInfo ShouldIncludeRecordsWithJobsInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldIncludeRecordsWithJobs)); }
		}

		#endregion

		#region IsPurgeSystem

		public bool IsPurgeSystem
			=> S5_ScheduleType == ArchiveManagerConstants.Codes.PAR
			|| S5_ScheduleType == ArchiveManagerConstants.Codes.PDR
			|| S5_ScheduleType == ArchiveManagerConstants.Codes.PDO
			|| S5_ScheduleType == ArchiveManagerConstants.Codes.EST
			|| S5_ScheduleType == ArchiveManagerConstants.Codes.PAL
			|| S5_ScheduleType == ArchiveManagerConstants.Codes.RED;

		#endregion

		public void Run(IArchiveLogger archiveLogger, CancellationToken token)
		{
			logger = archiveLogger;
			try
			{
				Run(token);
			}
			finally
			{
				logger = null;
			}
		}

		protected override void RunCore(INotifications notifications, CancellationToken token)
		{
			if (logger == null)
			{
				logger = new NotificationsArchiveLogger(notifications);
			}

			currentArchiveManager = NewArchiveManager();

			var archiveSystemDescriptors = currentArchiveManager.ArchiveSystemDescriptors;
			var currentDescriptor = archiveSystemDescriptors.First(descriptor => descriptor.Code == S5_ScheduleType);

			var archiveOnOrBeforeDate = DateTime.MinValue;

			if (IsArchiveRecordsOnOrBeforeDate)
			{
				archiveOnOrBeforeDate = ArchiveRecordsOnOrBeforeDate.ToDateTime();
			}
			else if (IsArchiveRecordsOnOrBeforeRelativeDate)
			{
				archiveOnOrBeforeDate = GetArchiveOnOrBeforeDateFromRelativeDate();
			}

			var configIncludeDocumentsWithoutJobs = false;
			if (currentDescriptor.AllowRecordsWithOrWithoutJobs && ShouldIncludeRecordsWithoutJobs)
			{
				configIncludeDocumentsWithoutJobs = true;
			}

			var configUseOnOrBeforeDateWhenWatermarkReset = false;
			if (currentDescriptor.AllowUsingOnOrBeforeDateWhenWatermarkReset && UseOnOrBeforeDateWhenWatermarkReset)
			{
				configUseOnOrBeforeDateWhenWatermarkReset = true;
			}

			var config = NewArchiveConfiguration(archiveOnOrBeforeDate, maxRunDurationInMinutes, ZDateTime.UtcNow.ToDateTime(), configIncludeDocumentsWithoutJobs: configIncludeDocumentsWithoutJobs, configUseOnOrBeforeDateWhenWatermarkReset: configUseOnOrBeforeDateWhenWatermarkReset);

			if (currentDescriptor.AllowDateParameterSelection && DateParameter.Equals(DateParameterStrings.GetCode(DateParameterType.JOP)))
			{
				config.SetIsFilteringByJobOpenDate(true);
			}

			var archiveOnOrBeforeParameterDescription = string.Empty;

			if (IsArchiveRecordsOnOrBeforeDate)
			{
				archiveOnOrBeforeParameterDescription = $"On or Before {ArchiveRecordsOnOrBeforeDate.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)}";
			}
			else if (IsArchiveRecordsOnOrBeforeRelativeDate)
			{
				archiveOnOrBeforeParameterDescription = $"{ArchiveRecordsOnOrBeforeNumber} {ArchiveRecordsOnOrBeforeTypeList[ArchiveRecordsOnOrBeforeType].Description}";
			}

			var usageCollectorScopeProperties = new (string, object)[]
			{
				(UsageProperties.DatabaseName, Db.DatabaseName),
				(UsageProperties.ArchiveScheduleStartTime, ZDateTime.Now.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)),
				(UsageProperties.ArchiveSystemName, S5_ScheduleType),
				(UsageProperties.ArchiveSystemDateParameters, archiveOnOrBeforeParameterDescription),
				(UsageProperties.IncludeCustomsJobsInArchiving, config.ShouldIncludeDeclarations),
				(UsageProperties.ArchivingBatchSize, RegistryHelper.GetBatchSizeValue(S5_ScheduleType))
			};

			foreach (var pair in usageCollectorScopeProperties)
			{
				config.AMUsageReportValues.Add(pair.Item1, pair.Item2);
			}

			currentArchiveManager.Run(S5_ScheduleType, config, logger, this, token);
		}

		IArchiveManager currentArchiveManager;

		protected override void NotifyStaffThatTaskWasDeactivatedDueToException(Exception e, INotifications notifications)
		{
			base.NotifyStaffThatTaskWasDeactivatedDueToException(e, notifications);

			var email = new EmailDef();
			email.Subject = Res.GetString("23BF2423-BF7F-465D-8461-ADCABCDDB317", "Archive Schedule Task Was Deactivated");
			email.Body = Res.GetString("18972794-0B16-401C-90FA-C9237792D4AD", @$"Archive Schedule Task {base.S5_ScheduleDescription} was canceled due to an Exception with message: '{e.Message}'.
Please check the error message and reactivate the task.
If the error persists, consider lodging an eRequest.");

			var creatingUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, S5_SystemCreateUser);

			if (creatingUser != null && creatingUser.GS_IsActive && !creatingUser.GS_EmailAddress.IsEmpty)
			{
				email.AddRecipientForUserCommunication(creatingUser.GS_EmailAddress);
				notifications.AddError($"Archive Schedule Task was deactivated due to an exception. An email was sent to {creatingUser.GS_EmailAddress}. The exception was: '{e.Message}'");
			}
			else
			{
				email.AddRecipientForUserCommunication(new EmailGroupUtility().GetGroupEmailCollection(EnvProxy.Instance.Registry.PostMasterGroup, throwExceptionIfEmptyGroup: false));
				notifications.AddError($"Archive Schedule Task was deactivated due to an exception. An email was sent to the postmaster group, because the user with code '{S5_SystemCreateUser}' could not be found, is inactive or has no email address. The exception was: '{e.Message}'");
			}

			EnvProxy.Instance.OutgoingMailManager.CreateAndSave(email);
		}

		public DateTime GetArchiveOnOrBeforeDateFromRelativeDate()
		{
			var archiveOnOrBeforeDate = DateTime.MinValue;

			switch (ArchiveRecordsOnOrBeforeType)
			{
				case "D":
					archiveOnOrBeforeDate = ZDate.Today.AddDays(-ArchiveRecordsOnOrBeforeNumber).ToDateTime();
					break;
				case "W":
					archiveOnOrBeforeDate = ZDate.Today.AddDays(-ArchiveRecordsOnOrBeforeNumber * 7).ToDateTime();
					break;
				case "M":
					archiveOnOrBeforeDate = ZDate.Today.AddMonths(-ArchiveRecordsOnOrBeforeNumber).ToDateTime();
					break;
				case "Y":
					archiveOnOrBeforeDate = ZDate.Today.AddYears(-ArchiveRecordsOnOrBeforeNumber).ToDateTime();
					break;
			}

			return archiveOnOrBeforeDate;
		}

		protected virtual IArchiveManager NewArchiveManager()
		{
			return new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
		}

		protected virtual IArchiveConfiguration NewArchiveConfiguration(ZDateTime archiveJobsOnOrBeforeThisDate, ZInt maxRunDurationInMinutes, ZDateTime startRunTime, bool configIncludeDocumentsWithoutJobs = false, bool configUseOnOrBeforeDateWhenWatermarkReset = false)
			=> new ArchiveConfiguration(archiveJobsOnOrBeforeThisDate, MaxRunDurationInMinutes, startRunTime, IsVerboseLog, ShouldArchiveDeclaration, shouldIncludeRecordsWithoutJobs: configIncludeDocumentsWithoutJobs, useOnOrBeforeDateWhenWatermarkReset: configUseOnOrBeforeDateWhenWatermarkReset);

		IArchiveLogger logger;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ParentTableCode = Enterprise.Core.Constants.ArchiveManager.ParentTableCode;
			S5_ParentID = new Guid(ArchiveConstants.ParentIDGuid);
			IsArchiveRecordsOnOrBeforeRelativeDate = true;
			IsArchiveRecordsOnOrBeforeDate = false;
			ShouldArchiveDeclaration = false;
			ShouldIncludeRecordsWithJobs = true;
			MaxRunDurationInMinutes = 60;
		}

		protected override StmScheduleTaskValidation GetNewValidation()
			=> new ArchiveScheduleTaskValidation(this);

		public override void OnLoaded()
		{
			base.OnLoaded();
			Deserialise();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			Serialise();
		}

		void Serialise()
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add(IsArchiveRecordsOnOrBeforeDateInfo.Name, IsArchiveRecordsOnOrBeforeDate.ToString());
			dictionary.Add(IsArchiveRecordsOnOrBeforeRelativeDateInfo.Name, IsArchiveRecordsOnOrBeforeRelativeDate.ToString());
			dictionary.Add(ArchiveRecordsOnOrBeforeDateInfo.Name, ArchiveRecordsOnOrBeforeDate.ToString("ddMMyyyy"));
			dictionary.Add(ArchiveRecordsOnOrBeforeNumberInfo.Name, ArchiveRecordsOnOrBeforeNumber.ToString());
			dictionary.Add(ArchiveRecordsOnOrBeforeTypeInfo.Name, ArchiveRecordsOnOrBeforeType.ToString());
			dictionary.Add(MaxRunDurationInMinutesInfo.Name, MaxRunDurationInMinutes.ToString());
			dictionary.Add(IsVerboseLogInfo.Name, IsVerboseLog.ToString());
			dictionary.Add(UseOnOrBeforeDateWhenWatermarkResetInfo.Name, UseOnOrBeforeDateWhenWatermarkReset.ToString());
			dictionary.Add(ShouldArchiveDeclarationInfo.Name, ShouldArchiveDeclaration.ToString());
			dictionary.Add(ShouldIncludeRecordsWithJobsInfo.Name, ShouldIncludeRecordsWithJobs.ToString());
			dictionary.Add(ShouldIncludeRecordsWithoutJobsInfo.Name, ShouldIncludeRecordsWithoutJobs.ToString());
			dictionary.Add(DateParameterInfo.Name, DateParameter.ToString());

			var jsonText = VersionFlag + JsonSerializer.Serialize(dictionary);
			S5_ScheduleState = Encoding.UTF8.GetBytes(jsonText);
		}

		void Deserialise()
		{
			using (SuspendSettingHasChanges())
			{
				if (S5_ScheduleState != null)
				{
					var jsonText = Encoding.UTF8.GetString(S5_ScheduleState).Substring(VersionFlag.Length);
					var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);

					if (dictionary.ContainsKey(IsArchiveRecordsOnOrBeforeDateInfo.Name))
					{
						IsArchiveRecordsOnOrBeforeDate = new ZBool(dictionary[IsArchiveRecordsOnOrBeforeDateInfo.Name]);
					}

					if (dictionary.ContainsKey(IsArchiveRecordsOnOrBeforeRelativeDateInfo.Name))
					{
						IsArchiveRecordsOnOrBeforeRelativeDate = new ZBool(dictionary[IsArchiveRecordsOnOrBeforeRelativeDateInfo.Name]);
					}

					if (dictionary.ContainsKey(ArchiveRecordsOnOrBeforeDateInfo.Name) && !string.IsNullOrEmpty(dictionary[ArchiveRecordsOnOrBeforeDateInfo.Name]))
					{
						if (ZDateTime.TryParseExact(dictionary[ArchiveRecordsOnOrBeforeDateInfo.Name], out var result, "ddMMyyyy"))
						{
							ArchiveRecordsOnOrBeforeDate = result;
						}
					}

					if (dictionary.ContainsKey(ArchiveRecordsOnOrBeforeNumberInfo.Name))
					{
						ArchiveRecordsOnOrBeforeNumber = ZInt.ParseSafe(dictionary[ArchiveRecordsOnOrBeforeNumberInfo.Name], 0);
					}

					if (dictionary.ContainsKey(ArchiveRecordsOnOrBeforeTypeInfo.Name))
					{
						ArchiveRecordsOnOrBeforeType = dictionary[ArchiveRecordsOnOrBeforeTypeInfo.Name];
					}

					if (dictionary.ContainsKey(MaxRunDurationInMinutesInfo.Name))
					{
						maxRunDurationInMinutes = ZInt.ParseSafe(dictionary[MaxRunDurationInMinutesInfo.Name], 0);
					}

					if (dictionary.ContainsKey(IsVerboseLogInfo.Name))
					{
						IsVerboseLog = new ZBool(dictionary[IsVerboseLogInfo.Name]);
					}

					if (dictionary.ContainsKey(UseOnOrBeforeDateWhenWatermarkResetInfo.Name))
					{
						UseOnOrBeforeDateWhenWatermarkReset = new ZBool(dictionary[UseOnOrBeforeDateWhenWatermarkResetInfo.Name]);
					}

					if (dictionary.ContainsKey(ShouldArchiveDeclarationInfo.Name))
					{
						ShouldArchiveDeclaration = new ZBool(dictionary[ShouldArchiveDeclarationInfo.Name]);
					}

					if (dictionary.ContainsKey(ShouldIncludeRecordsWithJobsInfo.Name))
					{
						ShouldIncludeRecordsWithJobs = new ZBool(dictionary[ShouldIncludeRecordsWithJobsInfo.Name]);
					}

					if (dictionary.ContainsKey(ShouldIncludeRecordsWithoutJobsInfo.Name))
					{
						ShouldIncludeRecordsWithoutJobs = new ZBool(dictionary[ShouldIncludeRecordsWithoutJobsInfo.Name]);
					}

					if (dictionary.ContainsKey(DateParameterInfo.Name) && !string.IsNullOrEmpty(dictionary[DateParameterInfo.Name]))
					{
						DateParameter = dictionary[DateParameterInfo.Name];
					}
					else
					{
						DateParameter = DateParameterStrings.GetCode(DateParameterType.JCL);
					}
				}
			}
		}

		protected override void UpdateNextScheduledDateCore()
		{
			if (S5_NextScheduledPrintRunTimeUtc < UtcNow)
			{
				base.UpdateNextScheduledDateCore();
			}
		}

		#region IArchiveSchedule Members

		public IArchiveWatermark GetWatermark(string stageName)
		{
			var watermark = ArchiveManagerDataRegistry.Instance.GetWatermark(PK.ToGuid(), stageName);
			return watermark.WatermarkDate <= ZDateTime.MinSmallDateTimeValue
				? null
				: watermark;
		}

		public void SetWatermark(string stageName, IArchiveWatermark watermark)
		{
			if (S5_ScheduleType != ArchiveManagerConstants.Codes.PAR)
			{
				if (watermark != null)
				{
					ArchiveManagerDataRegistry.Instance.SetWatermark(PK.ToGuid(), stageName, watermark);

					logger?.LogInfo(S5_ScheduleType, $"Watermark was updated to '{watermark.WatermarkDate}'");
				}
				else
				{
					ArchiveManagerDataRegistry.Instance.SetWatermark(PK.ToGuid(), stageName, new ArchiveWatermark { WatermarkDate = ZDateTime.MinSmallDateTimeValue });

					logger?.LogInfo(S5_ScheduleType, $"Watermark was reset to '{ZDateTime.MinSmallDateTimeValue}'");
				}
			}
		}

		public void AttachEDoc(string filepath, string eDocFileName, string docType, string description)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			masterFactory.RefreshEnabled = false;

			var obtainLockResult = Db.Connection.TryGetLock($"ArchiveManagerGenerateReportLock_{PK}", TimeSpan.FromMilliseconds(-1), out var sqlLock);

			if (obtainLockResult)
			{
				using (sqlLock)
				{
					var storageMain = masterFactory.RetrieveExistingOrCreateStorageMainForPK(PK, this, ((IDocManagerSupport)this).DocManagerInfo.DocManagerCode);

					var attachment = File.ReadAllBytes(filepath);
					var doc = storageMain.AddFileOrDocument(attachment, new AddFileOrDocumentDto
					{
						FileName = eDocFileName,
						DocumentType = docType,
					});
					doc.SC_Desc = description;
					masterFactory.Save();
				}
			}
		}

		public Guid SchedulePK => PK.ToGuid();

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ArchiveSchedule);
				}

				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			SetDefaultValues();

			HasChanges = false;
		}
#endif
		#endregion
	}
}
