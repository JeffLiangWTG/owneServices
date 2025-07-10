//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientCompanyLookups
//
//    This class should be used for overriding collections in AutoClientCompanyLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientCompanyLookups : AutoClientCompanyLookups
	{
		public ClientCompanyLookups(AutoClientCompany parent) : base(parent)
		{
		}

		public LicenceDatabaseNonDependentCollection Databases
		{
			get { return new LicenceDatabaseNonDependentCollection(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public static CodeDescriptionPairList BuildCompanyCategoryCodeDescriptionList(IEnumerable<ClientCompany> companies, OrgHeader org)
		{
			var result = new CodeDescriptionPairList();

			foreach (var companyGroup in companies
				.GroupBy(x => (org == null || x.LCC_OH == org.PK) ? 0 : 1)
				.OrderBy(x => x.Key))
			{
				result.AddPair("");
				var keyDescription = (companyGroup.Key == 0 ? "Client Company" : "Other Companies");
				result.Add(new CategoryCodeDescriptionPair(keyDescription, ""));
				foreach (var company in companyGroup.OrderBy(x => x.RelatedOrgName))
				{
					result.AddPair(company.LCC_Code, company.RelatedOrgName);
				}
			}

			return result;
		}
	}
}

