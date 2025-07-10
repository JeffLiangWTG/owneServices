using System;
using System.Xml.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	class CommonTypeBuilder : XmlBuilder
	{
		public CommonTypeBuilder(XNamespace xNamespace) : base(xNamespace)
		{
		}

		public XElement BuildTaxInvoiceFreeTextType(string tagName, string valueText)
		{
			return new XElement(GetTagName(tagName), GetTrimmedText(valueText));
		}

		public XElement BuildTaxInvoiceDateType(string tagName, DateTime value)
		{
			return new XElement(GetTagName(tagName), value.ToString("yyyyMMdd"));
		}

		public XElement BuildExchangedIssueDateTimeType(string tagName, DateTime value)
		{
			return new XElement(GetTagName(tagName), value.ToString("yyyyMMddHHmmss"));
		}

		public XElement BuildTaxInvoicePartyIDType(string tagName, string valueText)
		{
			return new XElement(GetTagName(tagName), valueText);
		}

		public XElement BuildTaxInvoicePartyTextType(string tagName, string valueText)
		{
			return string.IsNullOrEmpty(valueText) ? null : new XElement(GetTagName(tagName), valueText);
		}

		public XElement BuildTaxInvoicePartyNameTextType(string tagName, string valueText)
		{
			return new XElement(GetTagName(tagName), valueText);
		}

		public XElement BuildTaxInvoiceBusinessTypeCodeType(string tagName, string valueText)
		{
			return new XElement(GetTagName(tagName), valueText);
		}

		public XElement BuildTaxInvoicePaidAmountType(string tagName, ZDecimal value)
		{
			return new XElement(GetTagName(tagName), value.ToStringTrimZeros());
		}

		public XElement BuildTaxInvoiceAmountNoFracType(string tagName, ZDecimal value)
		{
			return new XElement(GetTagName(tagName), value.ToStringTrimZeros());
		}

		public XStreamingElement BuildSpecifiedOrganizationType(string tagName, string valueText)
		{
			return string.IsNullOrEmpty(valueText)
				? null
				: new XStreamingElement(GetTagName(tagName)
					, new XElement(GetTagName("TaxRegistrationID"), valueText)
					);
		}

		public XStreamingElement BuildSpecifiedPersonType(string tagName, string valueText)
		{
			return new XStreamingElement(GetTagName(tagName)
				, new XElement(GetTagName("NameText"), GetTrimmedText(valueText)));
		}

		public XStreamingElement BuildSpecifiedAddressType(string tagName, string valueText)
		{
			return string.IsNullOrEmpty(valueText)
				? null
				: new XStreamingElement(GetTagName(tagName)
					, new XElement(GetTagName("LineOneText"), valueText)
					);
		}

		public XStreamingElement BuildDefinedContactType(string tagName, string departmentName, string personName, string telephoneCommunication, string uRICommunication)
		{
			if (string.IsNullOrEmpty(departmentName) && string.IsNullOrEmpty(personName) && string.IsNullOrEmpty(telephoneCommunication) && string.IsNullOrEmpty(uRICommunication))
			{
				return null;
			}
			else
			{
				return new XStreamingElement(GetTagName(tagName)
					, BuildTaxInvoicePartyTextType("DepartmentNameText", departmentName)
					, string.IsNullOrEmpty(personName) ? null : new XElement(GetTagName("PersonNameText"), personName)
					, string.IsNullOrEmpty(telephoneCommunication) ? null : new XElement(GetTagName("TelephoneCommunication"), telephoneCommunication)
					, BuildTaxInvoicePartyTextType("URICommunication", uRICommunication)
				);
			}
		}

		string GetTrimmedText(string valueText, int maxLength = 100)
		{
			if (valueText == null)
			{
				return string.Empty;
			}

			if (valueText.Length > maxLength)
			{
				valueText = $"{valueText.Substring(0, maxLength - 3).TrimEnd()}...";
			}

			return valueText;
		}
	}
}
