using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ARComplianceDocumentHeaderCollection))]
	public class ARComplianceDocumentHeaderCollectionTest : AccComplianceDocumentHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new ARComplianceDocumentHeaderCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ARComplianceDocumentHeader));
		}
	}
}
