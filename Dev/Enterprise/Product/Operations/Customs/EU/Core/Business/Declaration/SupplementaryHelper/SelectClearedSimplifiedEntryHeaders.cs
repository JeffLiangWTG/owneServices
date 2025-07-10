using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.EU.Business.Declaration.SupplementaryHelper
{
	public class SelectClearedSimplifiedEntryHeaders : IEntryHeaderFilter
	{
		public SelectClearedSimplifiedEntryHeaders(JobDeclaration jobDeclaration)
		{
			declaration = jobDeclaration;
		}

		public IEnumerable<CusEntryHeader> EntryHeaders => declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(
			x => !x.MovementReferenceNumber.IsEmpty &&
			x.EntryInstruction.IsSimplifiedEntryInstruction &&
			x.IsEntryStatusCleared
		);

		readonly JobDeclaration declaration;
	}
}
