using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Reports
{
	public class CustomsProfileCodeDescriptionPairProvider : Integration.Customs.FR.ICustomsOfficesProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var query = new ZQuery(OrgCusAccountSchema.CZ_RN_NKCountryCode, Core.Constants.CountryCodes.France);
			query.AddToFilter(OrgCusAccountSchema.CZ_Code, new[] { OrgCusAccountCodeList.Codes.DGI, OrgCusAccountCodeList.Codes.DGE });
			var accounts = Factory.Load<OrgCusAccount>(query);

			var result = new CodeDescriptionPairList();
			foreach (var account in accounts)
			{
				result.AddPair(account.CZ_Account, account.CZ_Issuer);
			}
			return result;
		}
	}
}
