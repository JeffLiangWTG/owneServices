using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IGLJournalTypesProvider
	{
		CodeDescriptionPairList GetGLJournalTypes();
	}
}
