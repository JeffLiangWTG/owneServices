using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public static class EntryInstructionAutoSplitStrategyProvider
	{
		public enum SplitBy
		{
			LineCount20,
			LineCount50,
			LegalInspection
		}

		public static IEntryInstructionAutoSplitStrategy GetStrategy(CusEntryInstruction instruction, SplitBy splitBy)
		{
			IEntryInstructionAutoSplitStrategy result;
			switch (splitBy)
			{
				case SplitBy.LineCount20:
					result = new EntryInstructionSplitByLineCountStrategy(instruction, 20);
					break;
				case SplitBy.LineCount50:
					result = new EntryInstructionSplitByLineCountStrategy(instruction, 50);
					break;
				case SplitBy.LegalInspection:
					result = new EntryInstructionSplitByLegalInspectionStrategy(instruction);
					break;
				default:
					throw new DeveloperNotificationException("Invalid parameter SplitBy");
			}
			return result;
		}
	}
}
