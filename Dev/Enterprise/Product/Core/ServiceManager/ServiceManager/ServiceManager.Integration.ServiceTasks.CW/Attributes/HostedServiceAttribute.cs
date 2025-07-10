using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	[Serializable]
	[XmlSerializerAssembly("ServiceManager.Integration.ServiceTasks.CW.XmlSerializers")]
	public sealed class HostedServiceAttribute : AssemblyMetaDataAttributeWithType, IHostedServiceAttribute, ICustomizableDataCaptionSource, IEquatable<HostedServiceAttribute>
	{
		public HostedServiceAttribute(string code, string description, string category, Type type)
			: base(type)
		{
			Code = code;
			Description = description;
			Category = category;
		}

		/// <summary>
		/// This Constructor is used for ResourceStringAnalyzer only, Please donot use it.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="description"></param>
		/// <param name="category"></param>
		public HostedServiceAttribute(string code, string description, string category)
			: this(code, description, category, typeof(ServiceProviderImpl))
		{
		}

		public HostedServiceAttribute()
		{ }

		public string Code { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }

		public string ProcessFileName { get; set; }
		public string ProcessArguments { get; set; }

		public bool IsMandatory { get; set; }
		public bool AllowsMultipleInstances { get; set; }

		public string MinimumPeriod { get; set; }
		public string MaximumPeriod { get; set; }
		public bool IsScheduleReadOnly { get; set; }
		public bool IsReadOnlyForWiseCloudClient { get; set; }
		public string RequiresCompanyInCountry { get; set; }
		public bool CanRunInAnyBranch { get; set; }
		public string ConfigControlTypeName { get; set; }
		public string ConfigControlTypeAssemblyName { get; set; }
		public MutuallyExclusiveServiceTaskGroups MutuallyExclusiveTaskGroup { get; set; }
		public bool AlwaysRunAtStartup { get; set; }
		public bool ActiveByDefault { get; set; }

		public string DefaultScheduleRunEvery { get; set; }
		public DayOfWeek[] DefaultScheduleDaysOfWeek { get; set; }
		public int DefaultScheduleDayOfMonth { get; set; }
		public string DefaultScheduleStartAtLocal { get; set; }
		public string DefaultScheduleStartAtUtc { get; set; }
		public string DefaultScheduleRandomStartOffset { get; set; }
		public string DefaultScheduleEndAtLocal { get; set; }
		public string DefaultScheduleEndAtUtc { get; set; }
		public string DefaultScheduleDoNotRunTillNextDueTimeIfOverdue { get; set; }

		IDefaultSchedule defaultSchedule;
		[XmlIgnore]
		public IDefaultSchedule DefaultSchedule => defaultSchedule ??=
			new DefaultScheduleImpl()
			{
				RunEvery = DefaultScheduleRunEvery,
				DaysOfWeek = DefaultScheduleDaysOfWeek,
				DayOfMonth = DefaultScheduleDayOfMonth,
				StartAtLocal = DefaultScheduleStartAtLocal,
				StartAtUtc = DefaultScheduleStartAtUtc,
				RandomStartOffset = DefaultScheduleRandomStartOffset,
				EndAtLocal = DefaultScheduleEndAtLocal,
				EndAtUtc = DefaultScheduleEndAtUtc,
				DoNotRunTillNextDueTimeIfOverdue = DefaultScheduleDoNotRunTillNextDueTimeIfOverdue
			};

		internal class DefaultScheduleImpl : IDefaultSchedule
		{
			public string RunEvery { get; internal set; }
			public DayOfWeek[] DaysOfWeek { get; internal set; }
			public int DayOfMonth { get; internal set; }
			public string StartAtLocal { get; internal set; }
			public string StartAtUtc { get; internal set; }
			public string RandomStartOffset { get; internal set; }
			public string EndAtLocal { get; internal set; }
			public string EndAtUtc { get; internal set; }
			public string DoNotRunTillNextDueTimeIfOverdue { get; internal set; }
		}

		[XmlIgnore]
		public Type ConfigControlType
		{
			get
			{
				if (!string.IsNullOrEmpty(ConfigControlTypeName) && !string.IsNullOrEmpty(ConfigControlTypeAssemblyName))
				{
					return Type.GetType(ConfigControlTypeName + "," + ConfigControlTypeAssemblyName, false);
				}

				return null;
			}
			set
			{
				ConfigControlTypeName = value?.FullName ?? throw new ArgumentNullException(nameof(ConfigControlType));
				ConfigControlTypeAssemblyName = Path.GetFileNameWithoutExtension(value.Assembly.Location);
			}
		}

		[XmlIgnore]
		public Type TaskSpecificValidationType
		{
			get
			{
				return (string.IsNullOrEmpty(TaskSpecificValidationTypeName) ||
						string.IsNullOrEmpty(TaskSpecificValidationTypeAssemblyName))
					? null
					: Type.GetType($"{TaskSpecificValidationTypeName},{TaskSpecificValidationTypeAssemblyName}", throwOnError: true);
			}
			set
			{
				TaskSpecificValidationTypeName = value.FullName;
				TaskSpecificValidationTypeAssemblyName = Path.GetFileNameWithoutExtension(value.Assembly.Location);
			}
		}

		public string TaskSpecificValidationTypeName { get; set; }
		public string TaskSpecificValidationTypeAssemblyName { get; set; }

		public bool IsForClient()
		{
			return ClientSpecificCode == Clients.None || ClientHookLoader.Instance.Client == ClientSpecificCode;
		}

		public const ushort HostedServiceDescriptionAssemblyId = 9;
		#region IResourceStringAnalyzerSource member

		ushort ICustomizableDataCaptionSource.Asmid
		{
			get
			{
				return HostedServiceDescriptionAssemblyId;
			}
			set { }
		}

		string ICustomizableDataCaptionSource.Description
		{
			get { return "ServiceTask" + "|" + Code + "|" + Description; }
		}

		string ICustomizableDataCaptionSource.GetKey(object context, string caption)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(caption));
		}

		[SuppressMessage("CargoWiseOne", "CW1173:Do Not Invoke Res Underscore GetString", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "Baseline")]
		IEnumerable<IResString> ICustomizableDataCaptionSource.GetCompileTimeSystemCaptions()
		{
			yield return ResString._GetMultilingualString(((ICustomizableDataCaptionSource)this).Asmid, ((ICustomizableDataCaptionSource)this).GetKey(null, Description), Description);
		}

		int ICustomizableDataCaptionSource.MaxLength { get { return 0; } }

		IEnumerable<IResString> ICustomizableDataCaptionSource.GetRuntimeCaptions(IResString userCaption, object context)
		{
			yield return CustomizableDataResourceStrings.GetMultilingualString(this, null, this.Description);
		}

		#endregion

		#region IEquatable<HostedServiceAttribute>

		public bool Equals(HostedServiceAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) &&
				   (Code,
					   Description,
					   Category,
					   ProcessFileName,
					   ProcessArguments,
					   IsMandatory,
					   AllowsMultipleInstances,
					   MinimumPeriod,
					   MaximumPeriod,
					   IsScheduleReadOnly,
					   IsReadOnlyForWiseCloudClient,
					   RequiresCompanyInCountry,
					   CanRunInAnyBranch,
					   ConfigControlTypeName,
					   ConfigControlTypeAssemblyName,
					   MutuallyExclusiveTaskGroup,
					   AlwaysRunAtStartup,
					   ActiveByDefault,
					   DefaultScheduleRunEvery,
					   DefaultScheduleDaysOfWeek?.Sum(d => (int)d),
					   DefaultScheduleDayOfMonth,
					   DefaultScheduleStartAtLocal,
					   DefaultScheduleStartAtUtc,
					   DefaultScheduleRandomStartOffset,
					   DefaultScheduleEndAtLocal,
					   DefaultScheduleEndAtUtc,
					   DefaultScheduleDoNotRunTillNextDueTimeIfOverdue,
					   TaskSpecificValidationTypeName,
					   TaskSpecificValidationTypeAssemblyName
				   ).Equals(
					   (other.Code,
						   other.Description,
						   other.Category,
						   other.ProcessFileName,
						   other.ProcessArguments,
						   other.IsMandatory,
						   other.AllowsMultipleInstances,
						   other.MinimumPeriod,
						   other.MaximumPeriod,
						   other.IsScheduleReadOnly,
						   other.IsReadOnlyForWiseCloudClient,
						   other.RequiresCompanyInCountry,
						   other.CanRunInAnyBranch,
						   other.ConfigControlTypeName,
						   other.ConfigControlTypeAssemblyName,
						   other.MutuallyExclusiveTaskGroup,
						   other.AlwaysRunAtStartup,
						   other.ActiveByDefault,
						   other.DefaultScheduleRunEvery,
						   other.DefaultScheduleDaysOfWeek?.Sum(d => (int)d),
						   other.DefaultScheduleDayOfMonth,
						   other.DefaultScheduleStartAtLocal,
						   other.DefaultScheduleStartAtUtc,
						   other.DefaultScheduleRandomStartOffset,
						   other.DefaultScheduleEndAtLocal,
						   other.DefaultScheduleEndAtUtc,
						   other.DefaultScheduleDoNotRunTillNextDueTimeIfOverdue,
						   other.TaskSpecificValidationTypeName,
						   other.TaskSpecificValidationTypeAssemblyName
					   ));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is HostedServiceAttribute other && Equals(other);
		}

		public override int GetHashCode() => (base.GetHashCode(),
				Code,
				Description,
				Category,
				ProcessFileName,
				ProcessArguments,
				IsMandatory,
				AllowsMultipleInstances,
				MinimumPeriod,
				MaximumPeriod,
				IsScheduleReadOnly,
				IsReadOnlyForWiseCloudClient,
				RequiresCompanyInCountry,
				CanRunInAnyBranch,
				ConfigControlTypeName,
				ConfigControlTypeAssemblyName,
				MutuallyExclusiveTaskGroup,
				AlwaysRunAtStartup,
				ActiveByDefault,
				DefaultScheduleRunEvery,
				DefaultScheduleDaysOfWeek?.Sum(d => (int)d),
				DefaultScheduleDayOfMonth,
				DefaultScheduleStartAtLocal,
				DefaultScheduleStartAtUtc,
				DefaultScheduleRandomStartOffset,
				DefaultScheduleEndAtLocal,
				DefaultScheduleEndAtUtc,
				DefaultScheduleDoNotRunTillNextDueTimeIfOverdue,
				TaskSpecificValidationTypeName,
				TaskSpecificValidationTypeAssemblyName
			).GetHashCode();

		#endregion
	}
}
