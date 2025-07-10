using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CargoImpVersionConfiguration : RegistryBusinessObjectTemplate
	{
		public CargoImpVersionConfiguration()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new CargoImpVersionConfiguration();
			result.CurrentFallbackLevel = fallbackLevel;
			return result;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (CargoImpVersionConfiguration)clone;
			if (AirlineImpVersionMappings != null)
			{
				castedClone.versionCollection = (AirlineImpVersionCollection)AirlineImpVersionMappings.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.versionCollection);
			}
		}

		readonly ZString impVersionV16 = "V16";
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			DefaultImpVersion = impVersionV16;
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DefaultImpVersion = reader.ReadElementString(nameof(DefaultImpVersion));
			if (string.IsNullOrEmpty(DefaultImpVersion))
			{
				DefaultImpVersion = impVersionV16;
			}
			versionCollection = (AirlineImpVersionCollection)XmlSerialiser.Deserialize(reader);
			versionCollection.Sort("AirlinePrefix");
			RegisterEditableChildObject(AirlineImpVersionMappings);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(DefaultImpVersion), DefaultImpVersion);
			XmlSerialiser.Serialize(writer, AirlineImpVersionMappings);
		}

		public AirlineImpVersionCollection AirlineImpVersionMappings
		{
			get
			{
				if (versionCollection == null)
				{
					versionCollection = new AirlineImpVersionCollection();
					versionCollection.CurrentFallbackLevel = CurrentFallbackLevel;
					RegisterEditableChildObject(versionCollection);
				}
				return versionCollection;
			}
		}
		AirlineImpVersionCollection versionCollection;

		ZXmlSerializer XmlSerialiser => serialiser ??= ZXmlSerializer.New(typeof(AirlineImpVersionCollection));
		ZXmlSerializer serialiser;

		ZString defaultImpVersion;
		[List("ImpVersions")]
		[ResourceStringData("CargoImpVersionConfiguration.DefaultImpVersion", Caption = "Default IMP Version")]

		public ZString DefaultImpVersion
		{
			get
			{
				return defaultImpVersion;
			}
			set
			{
				SetNonPersistentPropertyValue(DefaultImpVersionInfo, ref defaultImpVersion, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultImpVersion();
				}
			}
		}

		public ZPropertyInfo DefaultImpVersionInfo => GetZPropertyInfo(nameof(DefaultImpVersion));

		AirlineImpVersion cargoImpVersionBusinessObject;
		public CodeDescriptionPairList ImpVersions
		{
			get
			{
				cargoImpVersionBusinessObject ??= new AirlineImpVersion();
				return cargoImpVersionBusinessObject.ImpVersions;
			}
		}

		public void ValidateDefaultImpVersion()
		{
			DefaultImpVersionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DefaultImpVersionInfo);
			ListValidation.ErrorIfInvalidCode(DefaultImpVersionInfo, ImpVersions);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDefaultImpVersion();
		}

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get => base.CurrentFallbackLevelCore;
			set
			{
				base.CurrentFallbackLevelCore = value;
				if (versionCollection != null)
				{
					versionCollection.CurrentFallbackLevel = value;
					foreach (AirlineImpVersion item in versionCollection)
					{
						item.CurrentFallbackLevel = value;
					}
				}
			}
		}
	}
}
