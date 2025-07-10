using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.BR.Business
{
	public partial class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		public ActiveCusEntryHeaderCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public IEnumerable<CusEntryLineFee> AllMergedLinesFees => this.Cast<CusEntryHeader>().SelectMany(x => x.AllMergedLinesFees);

		public IEnumerable<CusEntryHeader> FormalEntries => this.Cast<CusEntryHeader>().Where(x => x.IsFormalEntry);

		public IEnumerable<CusEntryHeader> SiscomexUsageFeeEntries => this.Cast<CusEntryHeader>().Where(x => x.CH_MessageType == MessageTypeList.Codes.SUF);
	}
}
