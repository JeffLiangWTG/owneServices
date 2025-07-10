using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestedType(typeof(XmlDataImporter.ImportedBusinessObjectCollection))]
	sealed class ImportedBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new XmlDataImporter.ImportedBusinessObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<DummyBusinessObject>();
		}
	}
}
