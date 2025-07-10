using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionSplitByLegalInspectionStrategy : BaseEntryInstructionSplitStrategy
	{
		public EntryInstructionSplitByLegalInspectionStrategy(CusEntryInstruction instruction) : base(instruction) { }

		public override int EstimatedSplitCount => 2;

		protected override ZString CheckBeforeSplitCore()
		{
			ZString result = ZString.Empty;

			if (CusEntryLines.AllSame(line => line.RequiresLegalInspection))
			{
				if (CusEntryLines.FirstOrDefault().RequiresLegalInspection)
				{
					result = Res.GetString("46CBA025-33D0-40C4-90B0-5B07139DC979", "All entry lines require Legal Inspection.");
				}
				else
				{
					result = Res.GetString("37D65052-CC49-416B-A973-0D5207DD1126", "All entry lines do not require Legal Inspection.");
				}
			}

			return result;
		}

		protected override IEnumerable<IEnumerable<CusEntryLine>> GroupEntryLines()
		{
			return CusEntryLines.GroupBy(line => line.RequiresLegalInspection).OrderByDescending(group => group.Key);
		}

		protected override string GetSuffixBySerial(int serial)
		{
			string result;
			switch (serial)
			{
				case 0:
					result = "LIR";
					break;
				case 1:
					result = "LIN";
					break;
				default:
					throw new DeveloperNotificationException("Splitting by Legal Inspection only makes 2 Instructions.");
			}
			return result;
		}

		protected override void OnAfterSplittingCore(CusEntryInstruction instruction)
		{
			SetCIQRequires(instruction);
		}

		void SetCIQRequires(CusEntryInstruction instruction)
		{
			instruction.CEI_CIQRequires = instruction == EntryInstruction;
		}
	}
}
