using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	sealed class SCWRECReconEntryBuilder : SimplifiedDeclarationReconEntryBuilder
	{
		public SCWRECReconEntryBuilder(CusEntryHeader entryHeader, ISCWRECHeader provider) : base(entryHeader, provider)
		{
			this.provider = provider;
		}
		readonly ISCWRECHeader provider;

		protected override void BuildLodgedLines() => provider.Lines.ForEach(x => new SCWRECReconEntryLineBuilder(x, provider, reconEntry).CreateLodgedEntryLineAndSnapshot());

		protected override void BuildCurrentLines() => provider.Lines.ForEach(x => new SCWRECReconEntryLineBuilder(x, provider, reconEntry).CreateCurrentEntryLineAndSnapshot());

		protected internal override bool HasChangesNotInSnapshotProvider()
		{
			var bodyProvider = new SCWPEDBodyProvider(reconEntry, false);
			var persistedBodyProvider = new SCWPEDBodyProvider(PersistedReconEntry, false);
			return !new IMonthlyClosingDecBodyEqualityComparer().Equals(bodyProvider, persistedBodyProvider);
		}
	}
}
