using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APComplianceDocumentHeaderCollection))]
	public class APComplianceDocumentHeaderCollectionTest : AccComplianceDocumentHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new APComplianceDocumentHeaderCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APComplianceDocumentHeader));
		}
	}
}
