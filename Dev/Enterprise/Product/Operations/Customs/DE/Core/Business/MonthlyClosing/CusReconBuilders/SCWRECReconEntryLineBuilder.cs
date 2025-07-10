using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class SCWRECReconEntryLineBuilder : CusReconEntryLineBuilder<ISCWRECLine, ISCWPEDLine>
	{
		public SCWRECReconEntryLineBuilder(ISCWRECLine lineProvider, ISCWRECHeader headerProvider, CusReconEntry cusReconEntry)
			: base(lineProvider, headerProvider, cusReconEntry)
		{
		}

		protected override IMonthlyClosingEntryLineSnapshot GetEntryLineSnapshotProvider(ISCWRECLine lineProvider, IImportDecHeader headerProvider)	=> new SCWRECEntryLineSnapshotProvider(lineProvider, (ISCWRECHeader)headerProvider);

		protected override ISCWPEDLine GetMonthlyClosingDecLineProvider(CusReconEntryLine reconEntryLine) => new SCWPEDLineProvider(reconEntryLine, true);

		protected override IEqualityComparer<ISCWPEDLine> LineEqualityComparer => new ISCWPEDLineEqualityComparer();
	}
}
