using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Reports;

public class MethodOfPaymentListProvider : Integration.Customs.IT.IMethodOfPaymentListProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
{
	public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
	{
		var factory = new BusinessObjectFactory();
		return factory.GetCachedValue("Enterprise.Customs.IT.Business.MethodOfPaymentListProvider", () => GetMethodsOfPayment(factory));
	}

	ReadOnlyCodeDescriptionPairList GetMethodsOfPayment(BusinessObjectFactory factory)
	{
		var result = new CodeDescriptionPairList();
		const string codeTypeMOP = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment;

		var methodsOfPayment = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Italy, codeTypeMOP, ZDateTime.Today, Enumerable.Empty<RefCusCodeListAttributeFilter>()).OrderBy(x => x.ZZD_Code);

		foreach (var mop in methodsOfPayment)
		{
			result.AddPair(mop.ZZD_Code, ZString.Format("{0} - {1}", mop.ZZD_Code, mop.ZZD_Description));
		}

		return result;
	}
}
