using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentPivotCollection))]
	public class AccComplianceDocumentPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AccComplianceDocumentPivotCollection(Factory);
	}
}
