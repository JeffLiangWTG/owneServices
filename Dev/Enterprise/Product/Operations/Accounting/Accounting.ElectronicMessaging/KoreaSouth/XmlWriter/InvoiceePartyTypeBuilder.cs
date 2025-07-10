using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class InvoiceePartyTypeBuilder : XmlBuilder
	{
		public InvoiceePartyTypeBuilder(XNamespace xNamespace, IInvoiceePartyAdditionalInfo additionalInfo) : base(xNamespace)
		{
			Argument.NotNull(additionalInfo, nameof(additionalInfo));

			AdditionalInfo = additionalInfo;
			CommonTypeBuilder = new CommonTypeBuilder(xNamespace);
		}

		public XStreamingElement Build(string name)
		{
			return new XStreamingElement(GetTagName(name)
				, BuildID()
				, BuildTypeCode()
				, BuildNameText()
				, BuildClassificationCode()
				, BuildSpecifiedOrganization()
				, BuildSpecifiedPerson()
				, BuildPrimaryDefinedContact()
				, BuildSecondaryDefinedContact()
				, BuildSpecifiedAddress()
			);
		}

		XElement BuildID()
		{
			return CommonTypeBuilder.BuildTaxInvoicePartyIDType("ID", AdditionalInfo.ID);
		}

		XElement BuildTypeCode()
		{
			return CommonTypeBuilder.BuildTaxInvoicePartyTextType("TypeCode", AdditionalInfo.TypeCode);
		}

		XElement BuildNameText()
		{
			return CommonTypeBuilder.BuildTaxInvoicePartyNameTextType("NameText", AdditionalInfo.NameText);
		}

		XElement BuildClassificationCode()
		{
			return CommonTypeBuilder.BuildTaxInvoicePartyTextType("ClassificationCode", AdditionalInfo.ClassificationCode);
		}

		XStreamingElement BuildSpecifiedOrganization()
		{
			return new XStreamingElement(GetTagName("SpecifiedOrganization"),
				string.IsNullOrEmpty(AdditionalInfo.TaxRegistrationID) ? null : new XElement(GetTagName("TaxRegistrationID"), AdditionalInfo.TaxRegistrationID),
				CommonTypeBuilder.BuildTaxInvoiceBusinessTypeCodeType("BusinessTypeCode", AdditionalInfo.BusinessTypeCode));
		}

		XStreamingElement BuildSpecifiedPerson()
		{
			return CommonTypeBuilder.BuildSpecifiedPersonType("SpecifiedPerson", AdditionalInfo.SpecifiedPersonNameText);
		}

		XStreamingElement BuildPrimaryDefinedContact()
		{
			return CommonTypeBuilder.BuildDefinedContactType("PrimaryDefinedContact", string.Empty, AdditionalInfo.PrimaryDefinedContactPersonName, AdditionalInfo.PrimaryDefinedContactTel, AdditionalInfo.PrimaryDefinedContactURICommunication);
		}

		XStreamingElement BuildSecondaryDefinedContact()
		{
			return CommonTypeBuilder.BuildDefinedContactType("SecondaryDefinedContact", string.Empty, AdditionalInfo.SecondaryDefinedContactPersonName, AdditionalInfo.SecondaryDefinedContactTel, AdditionalInfo.SecondaryDefinedContactURICommunication);
		}

		XStreamingElement BuildSpecifiedAddress()
		{
			return CommonTypeBuilder.BuildSpecifiedAddressType("SpecifiedAddress", AdditionalInfo.SpecifiedAddressLineOneText);
		}

		#region Properties

		CommonTypeBuilder CommonTypeBuilder { get; }

		IInvoiceePartyAdditionalInfo AdditionalInfo { get; }

		#endregion
	}
}
