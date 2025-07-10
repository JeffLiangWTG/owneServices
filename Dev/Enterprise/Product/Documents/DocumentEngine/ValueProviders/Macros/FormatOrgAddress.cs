using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FormatOrgAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FormatOrgAddress({OrgAddressPK} [,{HideCountryIfSame}])>",
				ResString.GetMultilingualString("746bbef7-f087-45f3-8317-3faf68a6e390", @"Returns a formatted address for the specified address including the company name.
{0} is the PK of an Address ({1}) you want to get the address from.

Optional Parameters:
{2} is a Microsoft JScript statement that if is evaluated to True will hide the Country in the address if it's the same with the Sender's Company Country.",
"{OrgAddressPK}", "OrgAddress", "{HideCountryIfSame}"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<FormatOrgAddress(Header.OrgAddressID)>", (NoResString)"UNIT 3, 72 O'RIORDAN STREET\nALEXANDRIA NSW\nSYDNEY NSW 2015\nAUSTRALIA"),
					((NoResString)"<FormatOrgAddress(Header.OrgAddressID , '<TransportMode>'=='SEA')>", (NoResString)"UNIT 3, 72 O'RIORDAN STREET\nALEXANDRIA NSW\nSYDNEY NSW 2015") });
		}

		protected override bool NeedCheckForScientificNotation => true;

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			Guid orgAddressPK = Guid.Empty;
			bool hideCountryIfSame = false;
			Match match = Regex.Match(macro);

			object parsedValue = ValueProviderHelper.ParseGuid(report, match.Groups[1].Value);

			if (parsedValue is Guid)
			{
				orgAddressPK = (Guid)parsedValue;
			}
			else
			{
				return parsedValue;
			}

			if (match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[3].Value))
			{
				hideCountryIfSame = ExpressionEvaluator.Evaluate(match.Groups[3].Value, report.UseJsEvaluator);
			}

			if (orgAddressPK != Guid.Empty)
			{
				OrgAddress address = GetAddress(orgAddressPK);
				if (address != null)
				{
					return new AddressFormatter(address.Factory, address, GlbCompany.CurrentCompany, !hideCountryIfSame).PostalAddress();
				}
				return "";
			}

			return string.Empty;
		}

		OrgAddress GetAddress(Guid orgAddressPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.Load<OrgAddress>(orgAddressPK);
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
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)format(?:[\s]*)org(?:[\s]*)Address(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*)(,(.+))?(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
