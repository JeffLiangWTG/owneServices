using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CSARevenueSummaryFormModuleCollection))]
	sealed class CSARevenueSummaryFormModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CSARevenueSummaryFormModuleCollection(Factory);

		protected override Type GetExpectedCollectionType() => typeof(CSARevenueSummaryFormModuleCollection);
	}
}
