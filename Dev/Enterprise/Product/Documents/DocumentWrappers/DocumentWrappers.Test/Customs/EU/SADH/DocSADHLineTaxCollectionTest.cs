using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[TestedType(typeof(DocSADHLineTaxCollection))]
	sealed class DocSADHLineTaxCollectionTest : DocSADHLineTaxCollectionTest<DocSADHLineTaxCollection>
	{
		protected override DocSADHLineTaxCollection GetNewDocumentWrapperCollection() => new DocSADHLineTaxCollection(Supporters, Factory);
	}
}
