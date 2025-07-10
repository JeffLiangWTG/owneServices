using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGLAccountCollection))]
	public class DocGLAccountCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocGLAccountCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AccGLHeader>();
			return DocGLAccount.New(header, Factory);
		}

		protected override DocGLAccountCollection GetCollectionToTest()
		{
			return new DocGLAccountCollection(Factory);
		}
	}
}
