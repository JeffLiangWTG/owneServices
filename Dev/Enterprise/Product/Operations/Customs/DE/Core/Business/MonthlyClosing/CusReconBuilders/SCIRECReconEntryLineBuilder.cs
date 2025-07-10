using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class SCIRECReconEntryLineBuilder : CusReconEntryLineBuilder<ISCIRECLine, ISCIPEDLine>
	{
		public SCIRECReconEntryLineBuilder(ISCIRECLine lineProvider, ISCIRECHeader headerProvider, CusReconEntry cusReconEntry)
			: base(lineProvider, headerProvider, cusReconEntry)
		{
		}

		protected override IMonthlyClosingEntryLineSnapshot GetEntryLineSnapshotProvider(ISCIRECLine lineProvider, IImportDecHeader headerProvider)	=> new SCIRECEntryLineSnapshotProvider(lineProvider, (ISCIRECHeader)headerProvider);

		protected override ISCIPEDLine GetMonthlyClosingDecLineProvider(CusReconEntryLine reconEntryLine) => new SCIPEDLineProvider(reconEntryLine, true);

		protected override IEqualityComparer<ISCIPEDLine> LineEqualityComparer => new ISCIPEDLineEqualityComparer();
	}
}
