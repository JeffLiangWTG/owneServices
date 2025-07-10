using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class SiscomexUsageFeeCollection : BusinessObjectCollection<CusEntryLineFee>
	{
		public SiscomexUsageFeeCollection(CusEntryHeader master) : base(master.Factory)
		{
			entryHeader = master;
		}

		readonly CusEntryHeader entryHeader;

		public override bool ReadOnly => true;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = ZQuery.NoResultQuery;

			if (entryHeader.CH_MessageType == MessageTypeList.Codes.CDI && entryHeader.CH_CEI_Instruction.IsValid)
			{
				var sufEntryHeader = entryHeader.Declaration.ActiveEntryHeaders.SiscomexUsageFeeEntries.FirstOrDefault(h => h.CH_CEI_Instruction == entryHeader.CH_CEI_Instruction);
				var entryLinePKs = sufEntryHeader?.MergedLines.Select(l => l.PK).ToArray() ?? System.Array.Empty<ZGuid>();

				if (entryLinePKs.Length > 0)
				{
					result = new ZQuery(CusEntryLineFeeSchema.CF_ChargeType, MessageTypeList.Codes.SUF);
					result.FetchOnlyFromLocalCache = !entryHeader.IsInDatabase && !sufEntryHeader.IsInDatabase;
					result.AddToFilter(CusEntryLineFeeSchema.CF_CL, entryLinePKs);
				}
			}

			return result;
		}
	}
}
