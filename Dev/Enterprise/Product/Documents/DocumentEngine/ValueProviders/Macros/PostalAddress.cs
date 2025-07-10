using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class PostalAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<PostalAddress({companyid})>",
				ResString.GetMultilingualString("980deed1-c355-42af-ab0a-1fea780f290b", @"Returns a formatted postal address for the specified company including the company name. 
{0} can either be 'COMPANYCODE' which will return the postal address for the Organization Proxy to the Current Company, or it can be the PK of an Organization ({1}) you want to get the postal address from. ",
"{companyid}", "OrgHeader"),
				new List<(string example, object expectedResult)>
				{
					("<PostalAddress(COMPANYCODE)>", (NoResString)"EDI CUSTOMS BROKERS\n10 HUTCHESON STREET\nALBION QLD\n4010"),
					((NoResString)"<PostalAddress(Header.BillToID)>", (NoResString)"UNIT 3, 72 O'RIORDAN STREET\nALEXANDRIA NSW\nSYDNEY NSW 2015") });
		}

		#region GetReplacement

		protected override object GetReplacementCore(string macro, Report report)
		{
			OrgHeader recipientOrg = null;
			Match match = Regex.Match(macro);

			if (match.Groups[1].Value.Equals("COMPANYCODE", StringComparison.OrdinalIgnoreCase))
			{
				recipientOrg = GlbCompany.CurrentCompany.OrgProxy;
			}
			else
			{
				object parsedValue = ParseNonCompanyCode(report, match.Groups[1].Value);

				if (parsedValue is Guid)
				{
					recipientOrg = Factory.Load<OrgHeader>((Guid)parsedValue);
				}
				else
				{
					return parsedValue;
				}
			}

			string contactType = (report.TypeOfContact != null) ? report.TypeOfContact.Code : "";

			if (recipientOrg != null)
			{
				var orgAddress = GetContactAddress(recipientOrg, contactType);
				if (orgAddress != null)
				{
					return new AddressFormatter(Factory, orgAddress, GlbCompany.CurrentCompany, false).PostalAddress();
				}

				orgAddress = GetContactAddress(recipientOrg, nameof(PrintCopyType.ALL));
				if (orgAddress != null)
				{
					return new AddressFormatter(Factory, orgAddress, GlbCompany.CurrentCompany, false).PostalAddress();
				}

				return new AddressFormatter(Factory, recipientOrg, GlbCompany.CurrentCompany, false).PostalAddress();
			}

			return string.Empty;
		}

		OrgAddress GetContactAddress(OrgHeader org, string addressType)
		{
			if (org.Addresses.Count > 0)
			{
				foreach (OrgAddress address in org.Addresses)
				{
					if (address.AddressCapability.GetCapabilityEnabled(addressType) && address.AddressCapability.GetIsMainAddress(addressType))
					{
						return address;
					}
					if (address.AddressCapability.GetCapabilityEnabled(addressType))
					{
						return org.Addresses[0];
					}
				}
			}
			return null;
		}

		protected object ParseNonCompanyCode(Report report, string matchedValue)
		{
			object value = report.MacroTranslator.GetValue("<" + matchedValue + ">", Passes.FirstPass);
			object parsedValue = null;

			if (value == DBNull.Value)
			{
				parsedValue = "";
			}
			else if (value is ZGuid || value is Guid)
			{
				parsedValue = ToGuid(value);
			}
			else
			{
				try
				{
					// Allow strings like "12345678-9abc-def0-1234-56789abcdef0"
					parsedValue = new Guid(value.ToString());
				}
				catch (FormatException)
				{
					parsedValue = string.Empty;
				}
			}

			return parsedValue;
		}

		protected Guid ToGuid(object value)
		{
			Guid result = Guid.Empty;

			if (value is Guid)
			{
				result = (Guid)value;
			}
			else if (value is ZGuid)
			{
				ZGuid zGuid = (ZGuid)value;
				if (zGuid.IsValid)
				{
					result = zGuid.ToGuid();
				}
			}

			return result;
		}
		#endregion

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)PostalAddress(?:[\s]*)\((?:[\s]*)([^>]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
