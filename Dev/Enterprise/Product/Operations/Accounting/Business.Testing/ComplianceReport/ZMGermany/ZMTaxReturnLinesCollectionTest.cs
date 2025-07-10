using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ComplianceReport.ZMGermany;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.ZMGermany
{
	[TestedType(typeof(ZMTaxReturnLinesCollection))]
	public class ZMTaxReturnLinesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var result = new ZMTaxReturnLinesCollection(Factory);
			return result;
		}
	}
}
