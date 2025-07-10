using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientAddress : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientAddress>",
				ResString.GetMultilingualString("a438513a-c2e8-4f72-b25e-02ecb7289143", "Returns the Address of the Contact the Report or Document is supposed to be sent to. An optional parameter will exclude the Contact name."),
				new List<(string example, object expectedResult)> { ("<RecipientAddress>", (NoResString)"WISETECH GLOBAL\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY NSW 2000"), ("<RecipientAddress(AddressOnly)>", (NoResString)"ADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY NSW 2000") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var contact = report.MostOfficialContact ?? report.DeliveryContact;

			if (contact != null)
			{
				var addressFormatter = new DocDeliveryContactAddressFormatter(Factory, contact, GlbCompany.CurrentCompany);

				var match = Regex.Match(macro);
				if (string.Equals(match.Groups["AddressOnly"].Value, "AddressOnly", StringComparison.OrdinalIgnoreCase))
				{
					return addressFormatter.PostalAddressWithoutCompanyName();
				}

				return addressFormatter.PostalAddress();
			}

			return string.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*address\s*(?:\((?:[\s]*)(?<AddressOnly>AddressOnly)(?:[\s]*)\)(?:[\s]*))?>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
