using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class LineNumberAssigner : Customs.Business.LineNumberAssigner
	{
		public LineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected override void ExecuteCore()
		{
			base.ExecuteCore();
			var allEntryLines = EntryHeader.AllEntryLines.Cast<CusEntryLine>();
			if (allEntryLines.Any(x => x.CL_CustomsPostedStatus == Customs.Business.EntryLineStatusList.Codes.DeletePending || x.CL_CustomsPostedStatus == Customs.Business.EntryLineStatusList.Codes.Deleted))
			{
				var sortedEntryLines = allEntryLines.OrderBy(x => x.CL_LineNumber);
				var lastLineNumber = ZShort.Zero;
				foreach (var entryLine in sortedEntryLines)
				{
					var currentLineNumber = entryLine.CL_LineNumber;
					if (entryLine.CL_CustomsPostedStatus == Customs.Business.EntryLineStatusList.Codes.DeletePending || entryLine.CL_CustomsPostedStatus == Customs.Business.EntryLineStatusList.Codes.Deleted)
					{
						if (currentLineNumber != ZShort.Zero)
						{
							entryLine.CL_LineNumber = ZShort.Zero;
						}
					}
					else
					{
						lastLineNumber++;
						if (currentLineNumber != lastLineNumber)
						{
							entryLine.CL_LineNumber = lastLineNumber;
						}
					}
				}
			}
		}

		protected new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;
	}
}
