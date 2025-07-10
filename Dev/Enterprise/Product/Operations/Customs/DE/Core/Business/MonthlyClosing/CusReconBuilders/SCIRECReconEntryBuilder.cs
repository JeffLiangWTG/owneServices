using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	sealed class SCIRECReconEntryBuilder : SimplifiedDeclarationReconEntryBuilder
	{
		public SCIRECReconEntryBuilder(CusEntryHeader entryHeader, ISCIRECHeader provider) : base(entryHeader, provider)
		{
			this.provider = provider;
		}
		readonly ISCIRECHeader provider;

		protected override void BuildLodgedLines() => provider.Lines.ForEach(x => new SCIRECReconEntryLineBuilder(x, provider, reconEntry).CreateLodgedEntryLineAndSnapshot());

		protected override void BuildCurrentLines() => provider.Lines.ForEach(x => new SCIRECReconEntryLineBuilder(x, provider, reconEntry).CreateCurrentEntryLineAndSnapshot());

		protected internal override bool HasChangesNotInSnapshotProvider()
		{
			var bodyProvider = new SCIPEDBodyProvider(reconEntry, false);
			var persistedBodyProvider = new SCIPEDBodyProvider(PersistedReconEntry, false);
			return !new ISCIPEDBodyEqualityComparer().Equals(bodyProvider, persistedBodyProvider);
		}
	}
}
