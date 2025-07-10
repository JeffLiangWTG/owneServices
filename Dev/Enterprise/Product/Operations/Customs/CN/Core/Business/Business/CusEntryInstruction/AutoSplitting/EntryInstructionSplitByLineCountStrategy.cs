using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionSplitByLineCountStrategy : BaseEntryInstructionSplitStrategy
	{
		public EntryInstructionSplitByLineCountStrategy(CusEntryInstruction entryInstruction, int byLineCount) : base(entryInstruction)
		{
			splitByLineCount = Argument.GreaterThanOrEqual(byLineCount, 20, nameof(byLineCount));
		}

		readonly int splitByLineCount;

		public override int EstimatedSplitCount => (CusEntryLines.Count() + splitByLineCount - 1) / splitByLineCount;

		protected override ZString CheckBeforeSplitCore()
		{
			return CusEntryLines.Count() > splitByLineCount
				? string.Empty
				: Res.GetString("201517f9-afc8-49a4-9ecc-96e69aa7924b", "The count of Entry Lines linked to this Entry Instruction does not exceed the limit({0}).", splitByLineCount);
		}

		protected override IEnumerable<IEnumerable<CusEntryLine>> GroupEntryLines()
		{
			return CusEntryLines.Batch(splitByLineCount);
		}

		protected override string GetSuffixBySerial(int serial)
		{
			return (serial + 1).ToString(CultureInfo.InvariantCulture);
		}

		protected override void OnAfterSplittingCore(CusEntryInstruction instruction) { }
	}
}
