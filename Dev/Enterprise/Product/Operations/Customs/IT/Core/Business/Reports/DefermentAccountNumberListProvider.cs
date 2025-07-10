using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusAuthorisationHeader;

namespace Enterprise.Customs.IT.Business.Reports;

public class DefermentAccountNumberListProvider : Integration.Customs.IT.IDefermentAccountNumberProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
{
	public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
	{
		var factory = new BusinessObjectFactory();
		return factory.GetCachedValue("Enterprise.Customs.IT.Business.DefermentAccountNumberListProvider", () => GetDefermentAccountNumber(factory));
	}

	ReadOnlyCodeDescriptionPairList GetDefermentAccountNumber(BusinessObjectFactory factory)
	{
		var query = new CusAuthorisationHeaderQueryBuilder(Core.Constants.CountryCodes.Italy)
				.AddTypeFilter(CusAuthorizationHeaderTypeList.Codes.DeferredPayment)
				.AddTransactionDateFilter(ZDate.Today)
				.Build();

		var result = factory.Load<CusAuthorisationHeader>(query).Aggregate(new CodeDescriptionPairList(), (list, auth) =>
		{
			list.AddPair(auth.CPH_Number, ZString.Format("{0}{1}{2}", auth.PermitHolder?.OH_Code ?? ZString.Empty, auth.CPH_PermitDescription.IsEmpty ? ZString.Empty : " - ", auth.CPH_PermitDescription));
			return list;
		});
		result.Sort();
		return result;
	}
}
