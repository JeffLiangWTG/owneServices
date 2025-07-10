using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	sealed class CFCRECReconEntryBuilder : SimplifiedDeclarationReconEntryBuilder
	{
		public CFCRECReconEntryBuilder(CusEntryHeader entryHeader, ICFCRECHeader provider)
			: base(entryHeader, provider)
		{
			this.provider = provider;
		}
		readonly ICFCRECHeader provider;

		protected override void BuildLodgedLines() => provider.Lines.ForEach(x => new CFCRECReconEntryLineBuilder(x, provider, reconEntry).CreateLodgedEntryLineAndSnapshot());

		protected override void BuildCurrentLines() => provider.Lines.ForEach(x => new CFCRECReconEntryLineBuilder(x, provider, reconEntry).CreateCurrentEntryLineAndSnapshot());

		protected internal override bool HasChangesNotInSnapshotProvider()
		{
			var bodyProvider = new CFCPEDBodyProvider(reconEntry, false);
			var persistedBodyProvider = new CFCPEDBodyProvider(PersistedReconEntry, false);
			return !new ICFCPEDBodyEqualityComparer().Equals(bodyProvider, persistedBodyProvider);
		}
	}
}
