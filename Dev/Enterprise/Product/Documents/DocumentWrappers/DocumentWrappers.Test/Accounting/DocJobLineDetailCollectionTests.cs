using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobLineDetailCollection))]
	public class DocJobLineDetailCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJobLineDetailCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			return DocJobLineDetail.New(line, Factory);
		}

		protected override DocJobLineDetailCollection GetCollectionToTest()
		{
			return new DocJobLineDetailCollection(Factory);
		}
	}
}
