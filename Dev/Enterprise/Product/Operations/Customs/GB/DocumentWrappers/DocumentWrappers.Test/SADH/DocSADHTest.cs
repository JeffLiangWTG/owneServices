using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSADH))]
	class DocSADHTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return GetNewDocumentWrapper(entryHeader);
		}

		protected override Enterprise.DocumentWrappers.Customs.EU.DocSADH GetNewDocumentWrapper(EU.Business.Declaration.CusEntryHeader entryHeader) => DocSADH.New(entryHeader, Factory);
	}
}
