using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class IncotermsPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new IncoTermsCodeDescriptionPairList();
			result.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));
			return result;
		}
	}
}
