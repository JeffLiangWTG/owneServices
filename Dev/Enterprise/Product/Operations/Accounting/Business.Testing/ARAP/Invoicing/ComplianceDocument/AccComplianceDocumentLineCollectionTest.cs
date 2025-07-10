using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentLineCollection))]
	public class AccComplianceDocumentLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AccComplianceDocumentLineCollection(Factory);
	}
}
