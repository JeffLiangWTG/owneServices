using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRImportEntryAdviceList : CodeDescriptionPairList
	{
		public CMRImportEntryAdviceList()
		{
			Add(CMRImportEntryAdvice.Processing);
			Add(CMRImportEntryAdvice.Held);
			Add(CMRImportEntryAdvice.Clear);
			Add(CMRImportEntryAdvice.Finalised);
			Add(CMRImportEntryAdvice.Withdrawn);
			Add(CMRImportEntryAdvice.Rejected);
			Add(CMRImportEntryAdvice.MultiStatus);
			Add(CMRImportEntryAdvice.DeclarationWorkComplete);
			Add(CMRImportEntryAdvice.ATDReceived);
		}

		public static bool IsEntryStatusClearOrFinalisedOrATDReceived(string entryStatus)
		{
			return entryStatus == CMRImportEntryAdvice.Clear.Code ||
				entryStatus == CMRImportEntryAdvice.Finalised.Code ||
				entryStatus == CMRImportEntryAdvice.ATDReceived.Code;
		}
	}
}
