using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class SCIPEDLineProvider : MonthlyClosingDecLineProvider, ISCIPEDLine
	{
		public SCIPEDLineProvider(CusReconEntryLine entryLine, bool isModificationMessage) : base(entryLine, isModificationMessage)
		{
		}

		public string RequestedPreferentialTreatment => IsModificationMessage ? LineProvider.RequestedPreferentialTreatment : CurrentSnapshot.PreferentialTreatment?.RequestedPreferentialTreatment;

		public IAmount InwardMovementAmount => IsModificationMessage ? LineProvider.InwardMovementAmount : AmountFromSnapshotProvider.NewOrNull(CurrentSnapshot.InwardMovementAmount);

		protected override IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader) => new SCIRECHeaderProvider(entryHeader);

		protected override IImportDecLine GetLineProvider(CusEntryLine entryLine) => new SCIRECLineProvider(entryLine);

		new ISCIRECLine LineProvider => (ISCIRECLine)base.LineProvider;
	}
}
