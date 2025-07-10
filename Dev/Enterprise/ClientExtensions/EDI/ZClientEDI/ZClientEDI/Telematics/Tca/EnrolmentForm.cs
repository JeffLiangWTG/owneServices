using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is a serializable class producing XML simple types are required")]
	[Serializable]
	[XmlRoot("enrolmentForm", Namespace = "http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07", IsNullable = false)]
	public class EnrolmentFormType : TcaCommonXml.AbstractDocumentType
	{
		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 2)]
		public string identifier { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 3)]
		public string previousEnrolmentIdentifier { get; set; }

		[XmlElement("secondPartyReference", Form = XmlSchemaForm.Unqualified, Order = 4)]
		public string[] secondPartyReference { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 5)]
		public EnrolmentProcessEnum enrolmentProcess { get; set; } = EnrolmentProcessEnum.ASP;

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 6)]
		public AuthoritySectionType authoritySection { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 7)]
		public EnrolmentStatusEnum statusCode { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 8)]
		public DateTime commencementDateTime { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 9)]
		public DateTime cessationDateTime { get; set; }

		[XmlIgnore]
		public bool cessationDateTimeSpecified { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 10)]
		public OperatorSectionType operatorSection { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 11)]
		public InterimApprovalSectionType interimApprovalSection { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 12)]
		public TcaCommonXml.ServiceProviderSectionType serviceProviderSection { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 13)]
		public ApprovalSectionType approvalSection { get; set; }

		[XmlElement(Form = XmlSchemaForm.Unqualified, Order = 14)]
		public CancellationSectionType cancellationSection { get; set; }

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class ApprovalSectionType
		{
			[XmlElement(Order = 1)]
			public bool approved { get; set; }
			[XmlElement(Order = 2)]
			public string comments { get; set; }
			[XmlElement(Order = 3)]
			public CompanyOfficerType authorisingOfficer { get; set; }
			[XmlElement(Order = 4)]
			public DateTime issuedDateTime { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class CancellationSectionType
		{
			[XmlElement(Order = 1)]
			public string comments { get; set; }
			[XmlElement(Order = 2)]
			public CompanyOfficerType authorisingOfficer { get; set; }
			[XmlElement(Order = 3)]
			public DateTime issuedDateTime { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class CompanyOfficerType
		{
			[XmlElement(Order = 1)]
			public string officerName { get; set; }
			[XmlElement(Order = 2)]
			public string positionName { get; set; }
			[XmlElement(Order = 3)]
			public string businessHoursPhone { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class AuthoritySectionType
		{
			[XmlElement(Order = 1)]
			public TcaCommonXml.AuthorityInformationType authority { get; set; }
			[XmlElement(Order = 2)]
			public string scheme { get; set; }

			[XmlElement("offTheShelfConditionsIdentifier", typeof(string), Order = 3)]
			[XmlElement("uniqueConditions", typeof(TcaCommonXml.ConditionsType), Order = 3)]
			public object Item { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class InterimApprovalSectionType
		{
			[XmlElement(Order = 1)]
			public DateTime lapseDateTime { get; set; }
			[XmlElement(Order = 2)]
			public string comments { get; set; }
			[XmlElement(Order = 3)]
			public CompanyOfficerType authorisingOfficer { get; set; }
			[XmlElement(Order = 4)]
			public DateTime issuedDateTime { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class OperatorSectionType
		{
			[XmlElement(Order = 1)]
			public OperatorInformationType @operator { get; set; }
			[XmlElement(Order = 2)]
			public CompanyOfficerType nominatedOfficer { get; set; }
			[XmlElement(Order = 3)]
			public TcaCommonXml.PrimaryUnitInformationType primaryUnitInformation { get; set; }
			[XmlElement("extension", Order = 4)]
			public TcaCommonXml.OperatorSectionExtensionType[] extension { get; set; }
			[XmlElement(Order = 5)]
			public string comments { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public class OperatorInformationType
		{
			[XmlElement(Order = 1)]
			public TcaCommonXml.OperatorIdentificationType identity { get; set; }
			[XmlElement(Order = 2)]
			public TcaCommonXml.AddressType postalAddress { get; set; }
			[XmlElement(Order = 3)]
			public string businessHoursPhone { get; set; }
			[XmlElement(Order = 4)]
			public string afterHoursPhone { get; set; }
			[XmlElement(Order = 5)]
			public string fax { get; set; }
			[XmlElement(Order = 6)]
			public string emailAddress { get; set; }
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public enum EnrolmentProcessEnum
		{
			AUTHORITY,
			ASP,
		}

		[Serializable]
		[XmlRoot(Namespace = "")]
		public enum EnrolmentStatusEnum
		{
			INTERIM,
			INTERIM_CANCELLED,
			LAPSED,
			DENIED,
			APPROVED,
			CEASED,
			CANCELLED,
		}
	}
}
