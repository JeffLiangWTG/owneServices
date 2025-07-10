using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class CFCRECReconEntryLineBuilder : CusReconEntryLineBuilder<ICFCRECLine, ICFCPEDLine>
	{
		public CFCRECReconEntryLineBuilder(ICFCRECLine lineProvider, ICFCRECHeader headerProvider, CusReconEntry cusReconEntry)
			: base(lineProvider, headerProvider, cusReconEntry)
		{
		}

		protected override IMonthlyClosingEntryLineSnapshot GetEntryLineSnapshotProvider(ICFCRECLine lineProvider, IImportDecHeader headerProvider)	=> new CFCRECEntryLineSnapshotProvider(lineProvider, (ICFCRECHeader)headerProvider);

		protected override ICFCPEDLine GetMonthlyClosingDecLineProvider(CusReconEntryLine reconEntryLine) => new CFCPEDLineProvider(reconEntryLine, true);

		protected override IEqualityComparer<ICFCPEDLine> LineEqualityComparer => new ICFCPEDLineEqualityComparer();
	}
}
