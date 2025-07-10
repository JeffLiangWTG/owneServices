using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class DV1CertificateWrapper : IDV1Certificate
	{
		public DV1CertificateWrapper(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public DV1CertificateWrapper(CusEntryHeader entryHeader) : this(entryHeader?.Declaration)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		protected JobDeclaration Declaration { get; }
		protected CusEntryHeader EntryHeader { get; }

		IEnumerable<EntryHeaderDataObject> IDV1Certificate.Entries => entries ?? (entries = GetEntries());
		IEnumerable<EntryHeaderDataObject> entries;

		IEnumerable<EntryHeaderDataObject> GetEntries()
		{
			return EntryHeader != null
				? new[] { GetNewEntryHeaderDataObject(EntryHeader) }
				: Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entryHeader => GetNewEntryHeaderDataObject(entryHeader));
		}

		protected virtual EntryHeaderDataObject GetNewEntryHeaderDataObject(CusEntryHeader entryHeader) => new EntryHeaderDataObject(entryHeader);
	}
}
