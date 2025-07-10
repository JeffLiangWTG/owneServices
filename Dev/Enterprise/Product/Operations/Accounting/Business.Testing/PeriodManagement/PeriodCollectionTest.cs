using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	[TestedType(typeof(PeriodCollection))]
	class PeriodCollectionTest : AccPeriodManagementCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PeriodCollection(Factory, new PeriodManager(Factory));
		}
	}
}
