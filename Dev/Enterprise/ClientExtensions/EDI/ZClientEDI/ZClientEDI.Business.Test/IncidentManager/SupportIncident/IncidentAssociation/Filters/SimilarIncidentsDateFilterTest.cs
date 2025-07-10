using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	[TestedType(typeof(SimilarIncidentsDateFilter))]
	public class SimilarIncidentsDateFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			var filter = new SimilarIncidentsDateFilterForTest("Test");
			Assert(!filter.IsNullable);
			Assert(filter.HideFutureDates);
			Assert(filter.HideOffsetFilters);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SimilarIncidentsDateFilter("Test");
		}
	}

	class SimilarIncidentsDateFilterForTest : SimilarIncidentsDateFilter
	{
		public SimilarIncidentsDateFilterForTest(ZString description) : base(description) { }

		public bool IsNullable => isNullable;
	}
}
