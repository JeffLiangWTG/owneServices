using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class RecipientNameAndAddressFollowingByContactName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<RecipientNameAndAddressFollowingByContactName>",
				ResString.GetMultilingualString("623de84b-d396-478f-a8ce-6bd268c5dd77", "Returns the Address of the Contact the Report or Document is supposed to be sent to following by 'ATTENTION:' prefix showing the Contact Name it's being sent to, based on the document's Address Category."),
				new List<(string example, object expectedResult)> { ("<RecipientNameAndAddressFollowingByContactName>", (NoResString)"WISETECH GLOBAL\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY NSW 2000\nATTENTION: JOHN DOE") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			DocDeliveryContact contact = report.MostOfficialContact ?? report.DeliveryContact;
			string result = "";

			if (contact != null)
			{
				//PostalAddress: this is *NOT* necessarily an OrgAddress with the Postal Address capability. It is just *a* postal adress, which could have any capability
				ZString postalAddress = new DocDeliveryContactAddressFormatter(Factory, contact, GlbCompany.CurrentCompany).PostalAddress();
				ZString contactName = report.DeliveryContact.Name;
				if (DocumentsDataRegistry.Instance.ContactNameUpperCase.Value)
				{
					contactName = contactName.ToUpper();
				}

				ZString contactNameLine = "";
				if (!contactName.IsEmpty)
				{
					contactNameLine = Res.GetString("5fdb8ae4-9773-419f-b74a-e0ddc53fde55", "ATTENTION: {0}", contactName);
				}

				result = postalAddress + "\n" + contactNameLine;
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*recipient\s*name\s*and\s*address\s*following\s*by\s*contact\s*name\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
