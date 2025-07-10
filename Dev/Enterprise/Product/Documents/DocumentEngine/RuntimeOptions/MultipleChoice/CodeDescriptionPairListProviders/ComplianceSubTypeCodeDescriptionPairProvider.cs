using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ComplianceSubTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCodeInLocalLanguage();
		}
	}
}
