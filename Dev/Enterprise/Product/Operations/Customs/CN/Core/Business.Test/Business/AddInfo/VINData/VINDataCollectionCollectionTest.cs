using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(VINDataCollection))]
	class VINDataCollectionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<JobDeclaration>().InvoiceLines.AddNew().VINDataCollection;
		}
	}
}
