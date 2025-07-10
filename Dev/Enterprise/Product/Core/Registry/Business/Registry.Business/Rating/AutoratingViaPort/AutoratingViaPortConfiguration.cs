using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AutoratingViaPortConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobType = nameof(JobType);
			public const string TransportMode = nameof(TransportMode);

			public const int JobTypeMaxLength = 3;
			public const int TransportModeMaxLength = 3;
		}

		#endregion

		public AutoratingViaPortConfiguration()
			: this(null, null)
		{
		}

		public AutoratingViaPortConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region JobType

		[MaxLength(Schema.JobTypeMaxLength)]
		[List("JobTypeList")]
		[ResourceStringData("AutoratingViaPort|JobType", Caption = "Job Type")]
		public ZString JobType
		{
			get { return jobType; }
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				SetNonPersistentPropertyValue(JobTypeInfo, ref jobType, value);
				ValidateJobType();
			}
		}

		ZString jobType;

		public ZPropertyInfo JobTypeInfo => GetZPropertyInfo(Schema.JobType);

		public void ValidateJobType()
		{
			if (!IsValidationSuspended)
			{
				JobTypeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(JobTypeInfo);
				ListValidation.ErrorIfInvalidCode(JobTypeInfo, JobTypeList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList JobTypeList =>
			new CodeDescriptionPairList()
			{
				AutoratingViaPortHelper.JobType.ForwardingConsol,
				AutoratingViaPortHelper.JobType.Shipment,
				AutoratingViaPortHelper.JobType.QuotedBooking,
			};

		#endregion

		#region TransportMode

		[MaxLength(Schema.TransportModeMaxLength)]
		[List("TransportModeList")]
		[ResourceStringData("AutoratingViaPort|TransportMode", Caption = "Transport Mode")]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				ValidateTransportMode();
			}
		}

		ZString transportMode;

		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		public void ValidateTransportMode()
		{
			if (!IsValidationSuspended)
			{
				TransportModeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(TransportModeInfo, TransportModeList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList TransportModeList =>
			new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(Core.Constants.TransportModes.All, Core.Constants.TransportModeDescriptions.All),
				new CodeDescriptionPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air),
				new CodeDescriptionPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea),
			};

		#endregion

		#region Settings

		[ChildEditable(true)]
		public AutoratingViaPortSettingCollection Settings
		{
			get
			{
				if (settings == null)
				{
					settings = new AutoratingViaPortSettingCollection(CurrentFallbackLevel, CurrentFactory);
					settings.ParentConfiguration = this;

					RegisterEditableChildObject(settings);
				}

				return settings;
			}
		}

		AutoratingViaPortSettingCollection settings;

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			using (SuspendCompositeKeyValidation())
			{
				ValidateJobType();
				ValidateTransportMode();
			}

			ValidateCompositeKey();
		}

		public IDisposable SuspendCompositeKeyValidation() =>
			new DisposableAction
			(
				() => ++compositeKeyValidationSuspenderIndex,
				() => --compositeKeyValidationSuspenderIndex
			);

		int compositeKeyValidationSuspenderIndex;

		public bool IsCompositeKeyValidationSuspend => compositeKeyValidationSuspenderIndex > 0;

		void ValidateCompositeKey()
		{
			if (IsCompositeKeyValidationSuspend || IsValidationSuspended)
			{
				return;
			}

			var parentCollection = GetParentCollection(this, typeof(AutoratingViaPortConfigurationCollection)) as AutoratingViaPortConfigurationCollection;
			var configurations = parentCollection?.Cast<AutoratingViaPortConfiguration>() ?? Enumerable.Empty<AutoratingViaPortConfiguration>();

			var count = configurations
				.Count(x =>
					string.Equals(JobType, x.jobType, System.StringComparison.OrdinalIgnoreCase) &&
					string.Equals(TransportMode, x.TransportMode, System.StringComparison.OrdinalIgnoreCase));

			if (count > 1)
			{
				AddRowError(IdenticalConfigurationExists);
			}
			else
			{
				RemoveRowError(IdenticalConfigurationExists);
			}
		}

		public static readonly MultilingualString IdenticalConfigurationExists =
			ResString.GetMultilingualString(
				"C2F76AAE-127B-4ECA-A8B2-5458EDBA6F79",
				"The same Job Type and Transport Mode configuration already exists");

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoratingViaPortConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			if (Settings != null && clone is AutoratingViaPortConfiguration configuration)
			{
				configuration.CloneSettingsFrom(configuration, Settings);
			}
		}

		void CloneSettingsFrom(AutoratingViaPortConfiguration parentConfiguration, AutoratingViaPortSettingCollection existingSettings)
		{
			settings = (AutoratingViaPortSettingCollection)existingSettings.Clone(CurrentFallbackLevel, CurrentFactory);
			settings.ParentConfiguration = parentConfiguration;

			RegisterEditableChildObject(settings);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobType, JobType);
			writer.WriteElementString(Schema.TransportMode, TransportMode);

			SettingsSerializer.Serialize(writer, Settings);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobType = reader.ReadElementString(Schema.JobType);
			TransportMode = reader.ReadElementString(Schema.TransportMode);

			settings = SettingsSerializer.Deserialize(reader) as AutoratingViaPortSettingCollection;
			CloneSettingsFrom(this, settings);
		}

		ZXmlSerializer SettingsSerializer
			=> settingsSerializer ?? (settingsSerializer = ZXmlSerializer.New(typeof(AutoratingViaPortSettingCollection)));

		ZXmlSerializer settingsSerializer;

		#endregion
	}
}
