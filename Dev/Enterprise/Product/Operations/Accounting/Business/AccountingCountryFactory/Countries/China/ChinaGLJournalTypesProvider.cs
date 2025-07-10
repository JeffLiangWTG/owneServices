using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China
{
	class ChinaGLJournalTypesProvider : IGLJournalTypesProvider
	{
		public CodeDescriptionPairList GetGLJournalTypes()
		{
			var fTransactionType_List = new CodeDescriptionPairList(OLookUpEditType.GLJournalTypes);
			if (!AccountingConfigurationRegistry.Instance.EnableAJLandRJL.Value)
			{
				fTransactionType_List.RemoveCode(TransactionTypes.GLAutoJournal);
				fTransactionType_List.RemoveCode(TransactionTypes.GLReversingJournal);
			}
			return fTransactionType_List;
		}
	}
}
