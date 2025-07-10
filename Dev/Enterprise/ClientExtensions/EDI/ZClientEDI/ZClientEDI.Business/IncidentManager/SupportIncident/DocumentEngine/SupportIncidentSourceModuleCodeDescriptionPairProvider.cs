using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentSourceModuleCodeDescriptionPairProvider : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return SupportIncidentLookups.GetNewSearchableSourceModuleList();
		}
	}
}

