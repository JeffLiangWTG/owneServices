using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing
{
	internal class DV1CertificateWrapperTest : TestCaseWithFactory
	{
		public void TestEntriesWhenProviderIsCreatedFromJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dataProvider = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("Entries Count", 0, dataProvider.Entries.Count());

			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			dataProvider = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("Entries Count", 2, dataProvider.Entries.Count());
			AssertType<EntryHeaderDataObject>("EntryHeaderDataObject Type", dataProvider.Entries.ElementAt(0));
		}

		public void TestEntriesWhenProviderIsCreatedFromCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var dataProvider = GetNewWrapperFromEntryHeader(entryHeader);
			AssertEquals("Entries Count", 1, dataProvider.Entries.Count());
			AssertType<EntryHeaderDataObject>("EntryHeaderDataObject Type", dataProvider.Entries.ElementAt(0));
			AssertSame("Same object", entryHeader, dataProvider.Entries.ElementAt(0).EntryHeader);
		}

		EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetNewWrapperFromDeclaration(JobDeclaration declaration) => new DV1CertificateWrapper(declaration);

		EU.Business.Documents.CertificateOfOrigin.IDV1Certificate GetNewWrapperFromEntryHeader(CusEntryHeader entryHeader) => new DV1CertificateWrapper(entryHeader);
	}
}
