using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CreditReportItem : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string CountryCode = "CountryCode";
			public const string Country = "Country";
			public const string CountryEnabledForCompany = "CountryEnabledForCompany";
			public const string CountryEnabledForOrganisation = "CountryEnabledForOrganisation";
			public const string ComprehensiveReportEnabled = "ComprehensiveReportEnabled";
			public const string FailureRiskEnabled = "FailureRiskEnabled";
			public const string LatePaymentRiskEnabled = "LatePaymentRiskEnabled";
			public const string CommercialBureauEnquiryEnabled = "CommercialBureauEnquiryEnabled";
		}

		#endregion

		#region Bound Properties

		ZString countryCode;
		public ZString CountryCode
		{
			get => countryCode;
			set => SetNonPersistentPropertyValue(CountryCodeInfo, ref countryCode, value);
		}

		public ZPropertyInfo CountryCodeInfo => GetZPropertyInfo(nameof(CountryCode));

		ZString country;
		public ZString Country
		{
			get => country;
			set => SetNonPersistentPropertyValue(CountryInfo, ref country, value);
		}

		public ZPropertyInfo CountryInfo => GetZPropertyInfo(nameof(Country));

		ZBool comprehensiveReportEnabled;
		[ReadOnlyMember(nameof(ComprehensiveReportEnabled_ReadOnly))]
		public ZBool ComprehensiveReportEnabled
		{
			get => comprehensiveReportEnabled;
			set => SetNonPersistentPropertyValue(ComprehensiveReportEnabledInfo, ref comprehensiveReportEnabled, value);
		}

		public ZPropertyInfo ComprehensiveReportEnabledInfo => GetZPropertyInfo(nameof(ComprehensiveReportEnabled));

		ZBool failureRiskEnabled;
		[ReadOnlyMember(nameof(FailureRiskEnabled_ReadOnly))]
		public ZBool FailureRiskEnabled
		{
			get => failureRiskEnabled;
			set => SetNonPersistentPropertyValue(FailureRiskEnabledInfo, ref failureRiskEnabled, value);
		}

		public ZPropertyInfo FailureRiskEnabledInfo => GetZPropertyInfo(nameof(FailureRiskEnabled));

		ZBool latePaymentRiskEnabled;
		[ReadOnlyMember(nameof(LatePaymentRiskEnabled_ReadOnly))]
		public ZBool LatePaymentRiskEnabled
		{
			get => latePaymentRiskEnabled;
			set => SetNonPersistentPropertyValue(LatePaymentRiskEnabledInfo, ref latePaymentRiskEnabled, value);
		}

		public ZPropertyInfo LatePaymentRiskEnabledInfo => GetZPropertyInfo(nameof(LatePaymentRiskEnabled));

		ZBool commercialBureauEnquiryEnabled;
		[ReadOnlyMember(nameof(CommercialBureauEnquiryEnabled_ReadOnly))]
		public ZBool CommercialBureauEnquiryEnabled
		{
			get => commercialBureauEnquiryEnabled;
			set => SetNonPersistentPropertyValue(CommercialBureauEnquiryEnabledInfo, ref commercialBureauEnquiryEnabled, value);
		}

		public ZPropertyInfo CommercialBureauEnquiryEnabledInfo => GetZPropertyInfo(nameof(CommercialBureauEnquiryEnabled));

		ZBool countryEnabledForCompany;
		public ZBool CountryEnabledForCompany
		{
			get => countryEnabledForCompany;
			set => SetNonPersistentPropertyValue(CountryEnabledForCompanyInfo, ref countryEnabledForCompany, value);
		}

		public ZPropertyInfo CountryEnabledForCompanyInfo => GetZPropertyInfo(nameof(CountryEnabledForCompany));

		ZBool countryEnabledForOrganisation;
		public ZBool CountryEnabledForOrganisation
		{
			get => countryEnabledForOrganisation;
			set
			{
				SetNonPersistentPropertyValue(CountryEnabledForOrganisationInfo, ref countryEnabledForOrganisation, value);
				if (!value)
				{
					CommercialBureauEnquiryEnabled = false;
					LatePaymentRiskEnabled = false;
					ComprehensiveReportEnabled = false;
					FailureRiskEnabled = false;
				}
			}
		}
		public ZPropertyInfo CountryEnabledForOrganisationInfo => GetZPropertyInfo(nameof(CountryEnabledForOrganisation));

		bool CommercialBureauEnquiryEnabled_ReadOnly => !CountryEnabledForOrganisation;

		bool LatePaymentRiskEnabled_ReadOnly => !CountryEnabledForOrganisation;

		bool ComprehensiveReportEnabled_ReadOnly => !CountryEnabledForOrganisation;

		bool FailureRiskEnabled_ReadOnly => !CountryEnabledForOrganisation;

		#endregion

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CountryCode = new ZString(reader.ReadElementString(Schema.CountryCode));
			Country = new ZString(reader.ReadElementString(Schema.Country));
			CountryEnabledForCompany = new ZBool(reader.ReadElementString(Schema.CountryEnabledForCompany));
			CountryEnabledForOrganisation = new ZBool(reader.ReadElementString(Schema.CountryEnabledForOrganisation));
			ComprehensiveReportEnabled = new ZBool(reader.ReadElementString(Schema.ComprehensiveReportEnabled));
			FailureRiskEnabled = new ZBool(reader.ReadElementString(Schema.FailureRiskEnabled));
			LatePaymentRiskEnabled = new ZBool(reader.ReadElementString(Schema.LatePaymentRiskEnabled));
			CommercialBureauEnquiryEnabled = new ZBool(reader.ReadElementString(Schema.CommercialBureauEnquiryEnabled));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CountryCode, CountryCode);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.CountryEnabledForCompany, CountryEnabledForCompany.ToString());
			writer.WriteElementString(Schema.CountryEnabledForOrganisation, CountryEnabledForOrganisation.ToString());
			writer.WriteElementString(Schema.ComprehensiveReportEnabled, ComprehensiveReportEnabled.ToString());
			writer.WriteElementString(Schema.FailureRiskEnabled, FailureRiskEnabled.ToString());
			writer.WriteElementString(Schema.LatePaymentRiskEnabled, LatePaymentRiskEnabled.ToString());
			writer.WriteElementString(Schema.CommercialBureauEnquiryEnabled, CommercialBureauEnquiryEnabled.ToString());
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditReportItem();
		}
	}
}
