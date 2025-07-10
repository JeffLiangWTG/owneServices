using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryHeaderCollection<TCusEntryHeader> : Customs.Business.CusEntryHeaderCollection<TCusEntryHeader>
		where TCusEntryHeader : CusEntryHeader
	{
		public CusEntryHeaderCollection(JobDeclaration parentBO, BusinessObjectFactory factory)
			: base(parentBO, factory)
		{
		}

		public override bool HasAnyEntryWhichMessagesCannotBeChangedCore => Count > 0 && this[0].CH_EntryStatus == EntryStatusList.Codes.AwaitingResponse;
	}
}
