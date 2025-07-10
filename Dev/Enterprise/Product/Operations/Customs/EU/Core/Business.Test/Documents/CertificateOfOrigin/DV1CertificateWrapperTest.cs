using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	class DV1CertificateWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEntriesWhenProviderIsCreatedFromJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.Entries.Count(), NUnit.Framework.Is.EqualTo(0), "Entries Count");

			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.Entries.Count(), NUnit.Framework.Is.EqualTo(2), "Entries Count");
			NUnit.Framework.Assert.That(wrapper.Entries.ElementAt(0), NUnit.Framework.Is.TypeOf<EntryHeaderDataObject>(), "EntryHeaderDataObject Type");
		}

		[ExpectNoExceptions]
		public void TestEntriesWhenProviderIsCreatedFromCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			var wrapper = GetNewWrapperFromEntryHeader(entryHeader);
			NUnit.Framework.Assert.That(wrapper.Entries.Count(), NUnit.Framework.Is.EqualTo(1), "Entries Count");
			NUnit.Framework.Assert.That(wrapper.Entries.ElementAt(0), NUnit.Framework.Is.TypeOf<EntryHeaderDataObject>(), "EntryHeaderDataObject Type");
			NUnit.Framework.Assert.That(wrapper.Entries.ElementAt(0).EntryHeader, NUnit.Framework.Is.SameAs(entryHeader), "Same object");
		}

		IDV1Certificate GetNewWrapperFromDeclaration(JobDeclaration declaration) => new DV1CertificateWrapper(declaration);

		IDV1Certificate GetNewWrapperFromEntryHeader(CusEntryHeader entryHeader) => new DV1CertificateWrapper(entryHeader);
	}
}
