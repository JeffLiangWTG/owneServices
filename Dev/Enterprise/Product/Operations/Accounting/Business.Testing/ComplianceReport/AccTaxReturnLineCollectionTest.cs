using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccTaxReturnLineCollection))]
	public class AccTaxReturnLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var taxReturn = Factory.New<AccTaxReturn>();
			return new AccTaxReturnLineCollection(taxReturn);
		}
	}
}
