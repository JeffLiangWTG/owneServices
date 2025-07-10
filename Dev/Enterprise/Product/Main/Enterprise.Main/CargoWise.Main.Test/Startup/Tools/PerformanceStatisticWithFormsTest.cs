using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(PerformanceStatisticWithForms))]
	sealed class PerformanceStatisticWithFormsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PerformanceStatisticWithForms();
		}
	}
}
