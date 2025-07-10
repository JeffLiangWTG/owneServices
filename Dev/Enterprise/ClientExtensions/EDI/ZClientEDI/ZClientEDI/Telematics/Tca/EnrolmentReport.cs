using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is a serializable class producing XML simple types are required")]
	[Serializable]
	[XmlRoot("enrolmentReport", Namespace = "http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07", IsNullable = false)]
	public class EnrolmentReportType : TcaCommonXml.AbstractDocumentType
	{
		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 1)]
		public string identifier { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 2)]
		public string authorityCode { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 3)]
		public DateTimePeriodType reportPeriod { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 4)]
		public TcaCommonXml.ServiceProviderInformationType serviceProvider { get; set; }

		[XmlElement("enrolmentSummary", Form = XmlSchemaForm.Unqualified, Order = 5)]
		public EnrolmentSummaryType[] enrolmentSummary { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 6)]
		public ReportedEventCountsType reportedEventCounts { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 7)]
		public string comments { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 8)]
		public DateTime issuedDateTime { get; set; }

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class EnrolmentSummaryType
		{
			[XmlElement("vehicle", typeof(TcaCommonXml.VehicleInformationType), Order = 1)]
			[XmlElement("vehicleIdentity", typeof(TcaCommonXml.VehicleIdentityType), Order = 1)]
			[XmlElement("vehicleRegistration", typeof(TcaCommonXml.VehicleRegistrationType), Order = 1)]
			public object Item { get; set; }

			[XmlElement("enrolment", Order = 2)]
			public EnrolmentReferenceType1[] enrolment { get; set; }

			[XmlElement(Order = 3)]
			public DateTime entryDateTime { get; set; }

			[XmlIgnore]
			public bool entryDateTimeSpecified { get; set; }

			[XmlElement(Order = 4)]
			public DateTime exitDateTime { get; set; }

			[XmlIgnore]
			public bool exitDateTimeSpecified { get; set; }

			[XmlElement(Order = 5)]
			public TcaCommonXml.DeviceIdentityType installedDevice { get; set; }

			[XmlElement(Order = 6)]
			public ReportedEventCountsType reportedEventCounts { get; set; }

			[XmlElement("extension", Order = 7)]
			public EnrolmentSummaryExtensionType[] extension { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ReportedEventCountsType
		{
			[XmlElement("typeCount", Order = 1)]
			public ReportedEventCountType[] typeCount { get; set; }
			[XmlElement(Order = 2)]
			public int totalCount { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		[XmlInclude(typeof(ReportedEventCountType))]
		public class EventCountType
		{
			[XmlElement(Order = 1)]
			public string typeCode { get; set; }
			[XmlElement(Order = 2)]
			public int count { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ReportedEventCountType : EventCountType
		{
			[XmlElement("subTypeCount", Order = 3)]
			public EventCountType[] subTypeCount { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public abstract class EnrolmentSummaryExtensionType
		{
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		[XmlInclude(typeof(EnrolmentReferenceType1))]
		public class EnrolmentReferenceType
		{
			[XmlElement(Order = 1)]
			public string enrolmentIdentifier { get; set; }
			[XmlElement(Order = 2)]
			public string scheme { get; set; }
			[XmlElement(Order = 3)]
			public OffTheShelfConditionsReferenceType offTheShelfConditions { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class OffTheShelfConditionsReferenceType
		{
			[XmlElement(Order = 1)]
			public string identifier { get; set; }
			[XmlElement(Order = 2)]
			public string revision { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class EnrolmentReferenceType1 : EnrolmentReferenceType
		{
			[XmlElement(Order = 4)]
			public TcaCommonXml.OperatorIdentificationType @operator { get; set; }
			[XmlElement("extension", Order = 5)]
			public EnrolmentReferenceExtensionType[] extension { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public abstract class EnrolmentReferenceExtensionType
		{
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class DateTimePeriodType
		{
			[XmlElement(Order = 1)]
			public DateTime startDateTime { get; set; }
			[XmlElement(Order = 2)]
			public DateTime endDateTime { get; set; }
		}
	}
}
