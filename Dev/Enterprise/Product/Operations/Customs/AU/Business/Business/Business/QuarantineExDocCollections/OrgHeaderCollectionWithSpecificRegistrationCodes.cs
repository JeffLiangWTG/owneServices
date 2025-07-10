using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OrgHeaderCollectionWithSpecificRegistrationCodes : OrgHeaderCollection
	{
		public OrgHeaderCollectionWithSpecificRegistrationCodes(BusinessObjectFactory factory, params string[] codes)
			: base(factory)
		{
			registrationCodes = codes;
		}

		readonly string[] registrationCodes;

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			var orgCusCodesQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, registrationCodes);
			orgCusCodesQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);

			var orgCusCodes = Factory.Load<OrgCusCode>(orgCusCodesQuery);
			result.AddToFilter(new ZQuery(OrgHeaderSchema.PK, orgCusCodes.Select(x => x.OK_OH)));
			return result;
		}

		public static string OrgDoesNotHaveRegNo(string message) => ZString.Format("Organisation does not contain a registration number / code for: {0}", message);
	}
}
