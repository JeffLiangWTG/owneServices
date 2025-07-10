using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business
{
	public class SCWPEDLineProvider : MonthlyClosingDecLineProvider, ISCWPEDLine
	{
		public SCWPEDLineProvider(CusReconEntryLine entryLine, bool isModificationMessage) : base(entryLine, isModificationMessage)
		{
		}

		public string RequestedPreferentialTreatment => IsModificationMessage ? LineProvider.RequestedPreferentialTreatment : CurrentSnapshot.PreferentialTreatment?.RequestedPreferentialTreatment;

		public IAmount InwardMovementAmount => IsModificationMessage ? LineProvider.InwardMovementAmount : AmountFromSnapshotProvider.NewOrNull(CurrentSnapshot.InwardMovementAmount);

		public string ForeignTradeImportEarlyClearanceFlag => IsModificationMessage ? HeaderProvider.ForeignTradeImportEarlyClearanceFlag : CurrentSnapshot.ForeignTradeFlag;

		protected override IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader) => new SCWRECHeaderProvider(entryHeader);

		protected override IImportDecLine GetLineProvider(CusEntryLine entryLine) => new SCWRECLineProvider(entryLine);

		new ISCWRECLine LineProvider => (ISCWRECLine)base.LineProvider;

		new ISCWRECHeader HeaderProvider => (ISCWRECHeader)base.HeaderProvider;
	}
}
