using System;
using System.Xml.Serialization;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is a helper class for producing XML groups simple types are required")]
	public static class TcaCommonXml
	{
		[Serializable]
		[XmlRoot(Namespace = "")]
		public class DeviceIdentityType
		{
			[XmlElement(Order = 1)]
			public string id { get; set; }
			[XmlElement(Order = 2)]
			public DeviceTypeType type { get; set; }

			[XmlIgnore]
			public bool typeSpecified { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public enum DeviceTypeType
		{
			IVU,
			TID,
			ECU,
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public abstract class ServiceProviderSectionExtensionType
		{
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class DeviceInstallationType
		{
			[XmlElement(Order = 1)]
			public DeviceIdentityType deviceIdentity { get; set; }
			[XmlElement(Order = 2)]
			public DateTime installationDateTime { get; set; }
			[XmlElement(Order = 3)]
			public string deviceLocation { get; set; }
			[XmlElement(Order = 4)]
			public string gpsAntennaLocation { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		[XmlInclude(typeof(PrimaryUnitInstallationType))]
		public class VehicleInstallationType
		{
			[XmlElement(Order = 1)]
			public VehicleIdentityType vehicleIdentity { get; set; }
			[XmlElement("installedDevice", Order = 2)]
			public DeviceInstallationType[] installedDevice { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class VehicleIdentityType
		{
			[XmlElement("nonVinIdentifier", typeof(string), Order = 1)]
			[XmlElement("vin", typeof(string), Order = 1)]
			[XmlChoiceIdentifier("ItemElementName")]
			public string Item { get; set; }

			[XmlIgnore]
			public ItemChoiceType ItemElementName { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public enum ItemChoiceType
		{
			[XmlEnum(":nonVinIdentifier")]
			nonVinIdentifier,

			[XmlEnum(":vin")]
			vin,
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class PrimaryUnitInstallationType : VehicleInstallationType
		{
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ServiceProviderSectionType
		{
			[XmlElement(Order = 1)]
			public ServiceProviderInformationType serviceProvider { get; set; }
			[XmlElement(Order = 2)]
			public PrimaryUnitInstallationType primaryUnitInstallation { get; set; }
			[XmlElement("extension", Order = 3)]
			public ServiceProviderSectionExtensionType[] extension { get; set; }
			[XmlElement(Order = 4)]
			public string comments { get; set; }
			[XmlElement(Order = 5)]
			public DateTime issuedDateTime { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ServiceProviderInformationType
		{
			[XmlElement(Order = 1)]
			public CompanyIdentificationType identity { get; set; }
			[XmlElement(Order = 2)]
			public AddressType postalAddress { get; set; }
			[XmlElement(Order = 3)]
			public string businessHoursPhone { get; set; }
			[XmlElement(Order = 4)]
			public string fax { get; set; }
			[XmlElement(Order = 5)]
			public string emailAddress { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		[XmlInclude(typeof(OperatorIdentificationType))]
		public class CompanyIdentificationType
		{
			[XmlElement(Order = 1)]
			public string companyName { get; set; }
			[XmlElement(Order = 2)]
			public string abn { get; set; }
			[XmlElement(Order = 3)]
			public string acn { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class OperatorIdentificationType : CompanyIdentificationType
		{
			[XmlElement(Order = 4)]
			public string name { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class AddressType
		{
			[XmlElement(Order = 1)]
			public string lineOne { get; set; }
			[XmlElement(Order = 2)]
			public string lineTwo { get; set; }
			[XmlElement(Order = 3)]
			public string locality { get; set; }
			[XmlElement(Order = 4)]
			public StateEnum stateCode { get; set; }
			[XmlElement(Order = 5)]
			public string postCode { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public enum StateEnum
		{
			NSW,
			VIC,
			QLD,
			SA,
			NT,
			ACT,
			WA,
			TAS,
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public abstract class OperatorSectionExtensionType
		{
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class PrimaryUnitInformationType : VehicleInformationType
		{
			[XmlElement(Order = 3)]
			public string make { get; set; }
			[XmlElement(Order = 4)]
			public string model { get; set; }
			[XmlElement(Order = 5)]
			public AddressType garagingAddress { get; set; }
			[XmlElement(Order = 6)]
			public string bodyType { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		[XmlInclude(typeof(PrimaryUnitInformationType1))]
		[XmlInclude(typeof(PrimaryUnitInformationType))]
		public class VehicleInformationType
		{
			[XmlElement(Order = 1)]
			public VehicleRegistrationType registration { get; set; }
			[XmlElement(Order = 2)]
			public VehicleIdentityType identity { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class VehicleRegistrationType
		{
			public string number { get; set; }
			public RegistrationStateEnum stateCode { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public enum RegistrationStateEnum
		{
			NSW,
			VIC,
			QLD,
			SA,
			NT,
			ACT,
			WA,
			TAS,
			FIRS,
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class PrimaryUnitInformationType1 : VehicleInformationType
		{
			[XmlElement(Order = 3)]
			public DeviceIdentityType installedDevice { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public abstract class ConditionsExtensionType
		{
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class QualifyingConditionReferenceType
		{
			[XmlElement(Order = 1)]
			public string identifier { get; set; }
			[XmlElement(Order = 2)]
			public bool inverted { get; set; } = false;
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public abstract class OperatingConditionType
		{
			[XmlElement(Order = 1)]
			public string identifier { get; set; }
			[XmlElement(Order = 2)]
			public string description { get; set; }

			[XmlElement("qualifyingCondition", Order = 3)]
			public QualifyingConditionReferenceType[] qualifyingCondition { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ConditionsType
		{
			[XmlElement("qualifyingCondition", Order = 1)]
			public OperatingConditionType[] qualifyingCondition { get; set; }

			[XmlElement("baseCondition", Order = 2)]
			public OperatingConditionType[] baseCondition { get; set; }

			[XmlElement("extension", Order = 3)]
			public ConditionsExtensionType[] extension { get; set; }

			[XmlElement(Order = 4)]
			public string comments { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class AuthorityInformationType
		{
			[XmlElement(Order = 1)]
			public string authorityCode { get; set; }
			[XmlElement(Order = 2)]
			public AddressType postalAddress { get; set; }
			[XmlElement(Order = 3)]
			public string businessHoursPhone { get; set; }
			[XmlElement(Order = 4)]
			public string fax { get; set; }
			[XmlElement(Order = 5)]
			public string emailAddress { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ApplicationReferenceType
		{
			[XmlElement(Order = 1)]
			public string name { get; set; }
			[XmlElement(Order = 2)]
			public string version { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		[XmlInclude(typeof(EnrolmentFormType))]
		[XmlInclude(typeof(EnrolmentReportType))]
		public abstract class AbstractDocumentType
		{
			[XmlElement(Order = 1)]
			public ApplicationReferenceType application { get; set; }
		}
	}
}
