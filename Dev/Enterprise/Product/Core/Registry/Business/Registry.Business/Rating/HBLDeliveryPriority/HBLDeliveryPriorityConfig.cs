using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HBLDeliveryPriorityConfig : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ContainerMode = nameof(ContainerMode);
			public const string HBLDeliveryMode = nameof(HBLDeliveryMode);

			public const int ContainerModeMaxLength = 3;
			public const int HBLDeliveryModeMaxLength = 9;
		}

		#endregion

		public HBLDeliveryPriorityConfig()
			: this(null, null)
		{
		}

		public HBLDeliveryPriorityConfig(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Container Mode

		[MaxLength(Schema.ContainerModeMaxLength)]
		[List("ContainerModeList")]
		[ResourceStringData("HBLDeliveryPriority|ContainerMode", Caption = "Container Mode")]
		public ZString ContainerMode
		{
			get { return containerMode; }
			set
			{
				CheckMaximumLength(ContainerModeInfo, value);
				SetNonPersistentPropertyValue(ContainerModeInfo, ref containerMode, value);
				ValidateContainerMode();
			}
		}

		ZString containerMode;

		public ZPropertyInfo ContainerModeInfo => GetZPropertyInfo(Schema.ContainerMode);

		public void ValidateContainerMode()
		{
			if (!IsValidationSuspended)
			{
				ContainerModeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ContainerModeInfo);
				ListValidation.ErrorIfInvalidCode(ContainerModeInfo, ContainerModeList);

				ValidateCompositeKey();
			}
		}
		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				if (containerModeList == null)
				{
					containerModeList = new CodeDescriptionPairList();
					containerModeList.AddPair(ContainerModes.BreakBulk, ContainerModeDescriptions.BreakBulk);
					containerModeList.AddPair(ContainerModes.BuyersConsol, ContainerModeDescriptions.BuyersConsol);
					containerModeList.AddPair(ContainerModes.Bulk, ContainerModeDescriptions.Bulk);
					containerModeList.AddPair(ContainerModes.FCL, ContainerModeDescriptions.FCL);
					containerModeList.AddPair(ContainerModes.LCL, ContainerModeDescriptions.LCL);
					containerModeList.AddPair(ContainerModes.Liquid, ContainerModeDescriptions.Liquid);
					containerModeList.AddPair(ContainerModes.Loose, ContainerModeDescriptions.Loose);
					containerModeList.AddPair(ContainerModes.RollOnRollOff, ContainerModeDescriptions.RollOnRollOff);
					containerModeList.AddPair(ContainerModes.ULD, ContainerModeDescriptions.ULD);
				}

				return containerModeList;
			}
		}
		CodeDescriptionPairList containerModeList;

		#endregion

		#region HBL Delivery Mode

		[MaxLength(Schema.HBLDeliveryModeMaxLength)]
		[List("HBLDeliveryModeList")]
		[ResourceStringData("HBLDeliveryPriority|HBLDeliveryMode", Caption = "HBL Delivery Mode")]
		public ZString HBLDeliveryMode
		{
			get { return hblDeliveryMode; }
			set
			{
				CheckMaximumLength(HBLDeliveryModeInfo, value);
				SetNonPersistentPropertyValue(HBLDeliveryModeInfo, ref hblDeliveryMode, value);
				ValidateHBLDeliveryMode();
			}
		}

		ZString hblDeliveryMode;

		public ZPropertyInfo HBLDeliveryModeInfo => GetZPropertyInfo(Schema.HBLDeliveryMode);

		public void ValidateHBLDeliveryMode()
		{
			if (!IsValidationSuspended)
			{
				HBLDeliveryModeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(HBLDeliveryModeInfo);
				ListValidation.ErrorIfInvalidCode(HBLDeliveryModeInfo, HBLDeliveryModeList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList HBLDeliveryModeList
		{
			get
			{
				return GetHBLDeliveryModes(ContainerMode);
			}
		}

		CodeDescriptionPairList GetHBLDeliveryModes(ZString containerMode)
		{
			var hblDeliveryModes = new Dictionary<string, CodeDescriptionPairList>
			{
				{ ContainerModes.FCL, fclHBLDeliveryModes },
				{ ContainerModes.LCL, lclHBLDeliveryModes },
				{ ContainerModes.BuyersConsol, bcnHBLDeliveryModes },
				{ ContainerModes.ShippersConsol, scnHBLDeliveryModes },
				{ ContainerModes.Loose, lseHBLDeliveryModes },
				{ ContainerModes.ULD, uldHBLDeliveryModes },
				{ ContainerModes.RollOnRollOff, bbK_ROR_BLK_LQD_HBLDeliveryModes },
				{ ContainerModes.BreakBulk, bbK_ROR_BLK_LQD_HBLDeliveryModes },
				{ ContainerModes.Bulk, bbK_ROR_BLK_LQD_HBLDeliveryModes },
				{ ContainerModes.Liquid, bbK_ROR_BLK_LQD_HBLDeliveryModes }
			};

			if (hblDeliveryModes.ContainsKey(containerMode))
			{
				hblDeliveryModes[containerMode] = hblDeliveryModes[containerMode] ?? GetHBLDeliveryModesFromRegistry(containerMode);
				return hblDeliveryModes[containerMode];
			}

			return new CodeDescriptionPairList();
		}

		readonly CodeDescriptionPairList fclHBLDeliveryModes;
		readonly CodeDescriptionPairList lclHBLDeliveryModes;
		readonly CodeDescriptionPairList bcnHBLDeliveryModes;
		readonly CodeDescriptionPairList scnHBLDeliveryModes;
		readonly CodeDescriptionPairList lseHBLDeliveryModes;
		readonly CodeDescriptionPairList uldHBLDeliveryModes;
		readonly CodeDescriptionPairList bbK_ROR_BLK_LQD_HBLDeliveryModes;

		CodeDescriptionPairList GetHBLDeliveryModesFromRegistry(ZString containerMode)
		{
			switch (containerMode)
			{
				case ContainerModes.FCL:
					return FreightDataRegistry.Instance.HBLDeliveryMode_FCL.Value.HBLDeliveryModesList;

				case ContainerModes.LCL:
					return FreightDataRegistry.Instance.HBLDeliveryMode_LCL.Value.HBLDeliveryModesList;

				case ContainerModes.BuyersConsol:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BCN.Value.HBLDeliveryModesList;

				case ContainerModes.ShippersConsol:
					return FreightDataRegistry.Instance.HBLDeliveryMode_SCN.Value.HBLDeliveryModesList;

				case ContainerModes.RollOnRollOff:
				case ContainerModes.BreakBulk:
				case ContainerModes.Bulk:
				case ContainerModes.Liquid:
					return FreightDataRegistry.Instance.HBLDeliveryMode_BBK_ROR_BLK_LQD.Value.HBLDeliveryModesList;

				case ContainerModes.Loose:
					return FreightDataRegistry.Instance.HBLDeliveryMode_LSE.Value.HBLDeliveryModesList;

				case ContainerModes.ULD:
					return FreightDataRegistry.Instance.HBLDeliveryMode_ULD.Value.HBLDeliveryModesList;

				default:
					return new CodeDescriptionPairList();
			}
		}

		#endregion

		#region Settings

		[ChildEditable(true)]
		public HBLDeliveryPrioritySettingCollection Settings
		{
			get
			{
				if (settings == null)
				{
					settings = new HBLDeliveryPrioritySettingCollection(CurrentFallbackLevel, CurrentFactory);
					settings.ParentConfiguration = this;

					RegisterEditableChildObject(settings);
				}

				return settings;
			}
		}

		HBLDeliveryPrioritySettingCollection settings;

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateContainerMode();
			ValidateHBLDeliveryMode();

			ValidateCompositeKey();
		}

		void ValidateCompositeKey()
		{
			var parentCollection = GetParentCollection(this, typeof(HBLDeliveryPriorityConfigCollection)) as HBLDeliveryPriorityConfigCollection;
			var configurations = parentCollection?.Cast<HBLDeliveryPriorityConfig>() ?? Enumerable.Empty<HBLDeliveryPriorityConfig>();

			var count = configurations
				.Count(x =>
					string.Equals(ContainerMode, x.containerMode, System.StringComparison.OrdinalIgnoreCase) &&
					string.Equals(HBLDeliveryMode, x.HBLDeliveryMode, System.StringComparison.OrdinalIgnoreCase));

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
				"c5859980-d246-4743-a50e-4b182c826cc1",
				"The same Container Mode and HBL Delivery Mode configuration already exists");

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HBLDeliveryPriorityConfig(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			if (Settings != null && clone is HBLDeliveryPriorityConfig configuration)
			{
				configuration.CloneSettingsFrom(configuration, Settings);
			}
		}

		void CloneSettingsFrom(HBLDeliveryPriorityConfig parentConfiguration, HBLDeliveryPrioritySettingCollection existingSettings)
		{
			settings = (HBLDeliveryPrioritySettingCollection)existingSettings.Clone(CurrentFallbackLevel, CurrentFactory);
			settings.ParentConfiguration = parentConfiguration;

			RegisterEditableChildObject(settings);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
			writer.WriteElementString(Schema.HBLDeliveryMode, HBLDeliveryMode);

			SettingsSerializer.Serialize(writer, Settings);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ContainerMode = reader.ReadElementString(Schema.ContainerMode);
			HBLDeliveryMode = reader.ReadElementString(Schema.HBLDeliveryMode);

			settings = SettingsSerializer.Deserialize(reader) as HBLDeliveryPrioritySettingCollection;
			CloneSettingsFrom(this, settings);
		}

		ZXmlSerializer SettingsSerializer
			=> settingsSerializer ?? (settingsSerializer = ZXmlSerializer.New(typeof(HBLDeliveryPrioritySettingCollection)));

		ZXmlSerializer settingsSerializer;

		#endregion
	}
}
