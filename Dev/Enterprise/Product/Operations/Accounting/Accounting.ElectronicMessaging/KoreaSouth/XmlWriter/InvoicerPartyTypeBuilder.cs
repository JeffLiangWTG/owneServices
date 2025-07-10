using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class InvoicerPartyTypeBuilder : XmlBuilder
	{
		public InvoicerPartyTypeBuilder(XNamespace xNamespace, IInvoicerPartyAdditionalInfo additionalInfo) : base(xNamespace)
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
				, BuildDefinedContact()
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
			return CommonTypeBuilder.BuildSpecifiedOrganizationType("SpecifiedOrganization", AdditionalInfo.TaxRegistrationID);
		}

		XStreamingElement BuildSpecifiedPerson()
		{
			return CommonTypeBuilder.BuildSpecifiedPersonType("SpecifiedPerson", AdditionalInfo.SpecifiedPersonNameText);
		}

		XStreamingElement BuildDefinedContact()
		{
			return CommonTypeBuilder.BuildDefinedContactType("DefinedContact", string.Empty, AdditionalInfo.DefinedContactPersonName, AdditionalInfo.DefinedContactTel, AdditionalInfo.DefinedContactURICommunication);
		}

		XStreamingElement BuildSpecifiedAddress()
		{
			return CommonTypeBuilder.BuildSpecifiedAddressType("SpecifiedAddress", AdditionalInfo.SpecifiedAddressLineOneText);
		}

		#region Properties

		CommonTypeBuilder CommonTypeBuilder { get; }

		IInvoicerPartyAdditionalInfo AdditionalInfo { get; }

		#endregion
	}
}
