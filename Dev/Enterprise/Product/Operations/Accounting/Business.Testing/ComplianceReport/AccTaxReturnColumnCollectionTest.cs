using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccTaxReturnColumnCollection))]
	public class AccTaxReturnColumnCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var taxReturn = Factory.New<AccTaxReturn>();
			return new AccTaxReturnColumnCollection(taxReturn);
		}
	}
}
