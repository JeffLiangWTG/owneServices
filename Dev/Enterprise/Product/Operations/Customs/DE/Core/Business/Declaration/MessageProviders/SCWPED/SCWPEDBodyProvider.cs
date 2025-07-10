using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWPEDBodyProvider : MonthlyClosingDecBodyProvider, ISCWPEDBody
	{
		public SCWPEDBodyProvider(CusReconEntry entry, bool isModificationMessage)
			: base(entry, isModificationMessage)
		{
		}

		public IReadOnlyCollection<ISCWPEDLine> Lines => lines ?? (lines = GetLinesQuery().Select(l => new SCWPEDLineProvider(l, IsModificationMessage)).ToArray());
		IReadOnlyCollection<ISCWPEDLine> lines;

		protected override bool IsModificationStatus(ZString type)
		{
			return base.IsModificationStatus(type) || (type == UniversalReferenceConstants.EntryStatus.TX5);
		}
	}
}
