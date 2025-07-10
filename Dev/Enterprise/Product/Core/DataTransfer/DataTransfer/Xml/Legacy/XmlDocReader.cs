using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class XmlDocReader
	{
		public XmlDocReader(XmlDocument xmlDoc)
		{
			this.fXmlDoc = xmlDoc;
		}

		public XmlDocument XmlDoc
		{
			get { return fXmlDoc; }
		}
		protected XmlDocument fXmlDoc;

		#region Get Values

		public ZDateTime GetValueAsDateTime(XmlNode parentNode, string xPath)
		{
			ZDateTime result = ZDateTime.Empty;

			string dateTimeAsString = GetValue(parentNode, xPath);
			ZDateTime parsedDateTime;
			if (ZDateTime.TryParseISO8601Date(dateTimeAsString, out parsedDateTime))
			{
				result = parsedDateTime;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		public ZDateTimeOffset GetValueAsDateTimeOffset(XmlNode parentNode, string xPath)
		{
			ZDateTimeOffset result = ZDateTimeOffset.Empty;

			string dateTimeOffsetAsString = GetValue(parentNode, xPath);
			ZDateTimeOffset parsedDateTimeOffset;
			if (ZDateTimeOffset.TryParse(dateTimeOffsetAsString, out parsedDateTimeOffset))
			{
				result = parsedDateTimeOffset;
			}

			return result;
		}

		public ZTime GetValueAsTime(XmlNode parentNode, string xPath)
		{
			ZTime result = ZTime.Empty;

			string dateTimeAsString = GetValue(parentNode, xPath);
			ZTime parsedTime;
			if (ZTime.TryParseExact(dateTimeAsString, out parsedTime, ZTime.TimeFormat))
			{
				result = parsedTime;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		public ZGeography GetValueAsGeography(XmlNode parentNode, string xPath)
		{
			var result = ZGeography.Empty;

			string geographyAsString = GetValue(parentNode, xPath);
			ZGeography parsedGeography;
			if (ZGeography.TryParse(geographyAsString, out parsedGeography))
			{
				result = parsedGeography;
			}

			return result;
		}

		public string GetValue(XmlNode parentNode, string xPath)
		{
			string result = "";

			if (parentNode != null)
			{
				XmlNode node = parentNode.SelectSingleNode(xPath);
				if (node != null)
				{
					result = node.InnerText;
				}
			}

			return result;
		}

		public string GetAttributeValue(XmlNode parentNode, string xPath, string attributeName)
		{
			string result = "";

			if (parentNode != null)
			{
				XmlNode node = parentNode.SelectSingleNode(xPath);
				if (node != null)
				{
					result = GetAttributeValue(node, attributeName);
				}
			}

			return result;
		}

		public string GetAttributeValue(XmlNode node, string attributeName)
		{
			string result = "";

			if (node != null)
			{
				XmlAttribute attribute = node.Attributes[attributeName];
				if (attribute != null)
				{
					result = attribute.Value;
				}
			}

			return result;
		}

		public string GetValue(XmlNode parentNode, string xPath, string attributeName, string attributeValue)
		{
			string result = "";

			if (parentNode != null)
			{
				XmlNode node = parentNode.SelectSingleNode(xPath + "[@" + attributeName + " = '" + attributeValue + "']");
				if (node != null)
				{
					result = node.InnerText;
				}
			}

			return result;
		}

		#endregion

		#region Organisation Creation and Matching

		public OrgHeader GetOrganisation(BusinessObjectFactory factory, OrgHeader eDIOrg, XmlNode orgNode)
		{
			OrgHeader result = null;

			ZString eDIOrgCode = GetAttributeValue(orgNode, ElementsXsd.Attributes.EDICode);
			ZString foreignCode = GetAttributeValue(orgNode, ElementsXsd.Attributes.OwnerCode);

			OrgHeader temporaryOrganisation = (OrgHeader)factory.New(typeof(OrgHeader));
			temporaryOrganisation.OH_IsTempAccount = true;
			temporaryOrganisation.OH_FullName = GetValue(orgNode, ElementsXsd.XPath.Organisation.Name);
			temporaryOrganisation.OH_RL_NKClosestPort = GetValue(orgNode, ElementsXsd.XPath.Organisation.Location);
			temporaryOrganisation.MainAddress.OA_Address1 = GetValue(orgNode, ElementsXsd.XPath.Organisation.AddressLine1);
			temporaryOrganisation.MainAddress.OA_Address2 = GetValue(orgNode, ElementsXsd.XPath.Organisation.AddressLine2);
			temporaryOrganisation.MainAddress.OA_City = GetValue(orgNode, ElementsXsd.XPath.Organisation.CityOrSuburb);
			temporaryOrganisation.MainAddress.OA_State = GetValue(orgNode, ElementsXsd.XPath.Organisation.StateOrProvince);
			temporaryOrganisation.MainAddress.OA_PostCode = GetValue(orgNode, ElementsXsd.XPath.Organisation.PostCode);
			SetTelephoneNumbers(temporaryOrganisation, orgNode);
			temporaryOrganisation.MainAddress.OA_Email = GetValue(orgNode, ElementsXsd.XPath.Organisation.Email);
			temporaryOrganisation.MainWebURL.PU_URL = GetValue(orgNode, ElementsXsd.XPath.Organisation.WebAddress);
			SetRegistrationNumbers(factory, temporaryOrganisation, orgNode);
			//TemporaryOrganisation.OH_Code = EDIOrgCode;

			LegacyOrganisationMatching organisationMatching = new LegacyOrganisationMatching(factory, Xsd.XmlInterchange.Empty, new NotificationBuffer());
			if (!eDIOrgCode.IsEmpty)
			{
				result = organisationMatching.Match(eDIOrgCode, temporaryOrganisation);
			}
			else if (!foreignCode.IsEmpty)
			{
				result = organisationMatching.MatchForeignCodeOrMatchOnTempAndRegisterForeignCode(foreignCode, eDIOrg, temporaryOrganisation);
			}
			else
			{
				result = organisationMatching.Match(temporaryOrganisation);
			}

			if (result != null || temporaryOrganisation.OH_Code == "")
			{
				temporaryOrganisation.Delete();
			}
			else
			{
				result = temporaryOrganisation;
			}

			return result;
		}

		protected void SetTelephoneNumbers(OrgHeader org, XmlNode orgNode)
		{
			org.MainAddress.OA_Phone = GetValue(orgNode, ElementsXsd.XPath.Organisation.TelephoneNumbers, ElementsXsd.Attributes.NumberType, ElementsXsd.NumberTypes.Business);
			org.MainAddress.OA_Fax = GetValue(orgNode, ElementsXsd.XPath.Organisation.TelephoneNumbers, ElementsXsd.Attributes.NumberType, ElementsXsd.NumberTypes.Fax);
			org.MainAddress.OA_Mobile = GetValue(orgNode, ElementsXsd.XPath.Organisation.TelephoneNumbers, ElementsXsd.Attributes.NumberType, ElementsXsd.NumberTypes.Mobile);
		}

		//TODO: if for existing org then need to think about need to update or not
		protected void SetRegistrationNumbers(BusinessObjectFactory factory, OrgHeader org, XmlNode orgNode)
		{
			XmlNodeList registrationNumberNodes = orgNode.SelectNodes(ElementsXsd.XPath.Organisation.RegistrationNumbers);
			foreach (XmlNode registrationNumberNode in registrationNumberNodes)
			{
				OrgCusCode cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = GetValue(registrationNumberNode, ElementsXsd.RegistrationNumber.NumberType);
				string countryCode = GetValue(registrationNumberNode, ElementsXsd.RegistrationNumber.CountryOfRegistration);
				RefCountry country = RefCountry.LoadFromCountryCode(factory, countryCode);
				if (country != null)
				{
					cusCode.OK_RN_NKCodeCountry = country.Code;
				}
				cusCode.OK_CustomsRegNo = GetValue(registrationNumberNode, ElementsXsd.RegistrationNumber.Number);
			}
		}

		class LegacyOrganisationMatching : OrganisationMatching
		{
			public LegacyOrganisationMatching(BusinessObjectFactory factory, Xsd.XmlInterchange interchange, INotifications notifications) : base(new BusinessObjectFactoryProvider(factory), interchange, notifications)
			{
			}

			public OrgHeader Match(OrgHeader temporaryOrganisation)
			{
				return base.Match(temporaryOrganisation);
			}

			public OrgHeader Match(ZString orgCode, OrgHeader temporaryOrganisation)
			{
				return base.Match(orgCode, temporaryOrganisation);
			}

			public OrgHeader MatchForeignCodeOrMatchOnTempAndRegisterForeignCode(ZString foreignOrgCode, OrgHeader mappingOrg, OrgHeader temporaryOrganisation)
			{
				return base.MatchForeignCodeOrMatchOnTempAndRegisterForeignCode(foreignOrgCode, mappingOrg, temporaryOrganisation);
			}
		}

		#endregion
	}
}
