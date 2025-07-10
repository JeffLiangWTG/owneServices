using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class ARAgreedPaymentMethodCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair((NoResString)"Undefined", ResString.GetMultilingualString("bea30d99-00cf-4bea-9c08-dfb64e637771", "Undefined"));
			result.AddRange(OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList());

			return result;
		}
	}
}
