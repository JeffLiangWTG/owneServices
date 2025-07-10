using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityClosedReasonsCollection))]
	sealed class OpportunityLostReasonsCollectionTest : RegistryBusinessObjectCollectionTestCase<OpportunityClosedReasonsCollection>
	{
		public void TestGetActiveCodeDescriptionPairList()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add("A01", (NoResString)"A01");
			collection.Add("A02", (NoResString)"A02").Bool = false;
			collection.Add("A03", (NoResString)"A03").StatusRules.AddNew().Code = "S03";
			collection.Add("A04", (NoResString)"A04").StatusRules.AddNew().Code = "S04";

			AssertContainsExactElementsInAnyOrder(
				new[] { "A01", "A03", "A04" },
				collection.GetActiveCodeDescriptionPairList().Cast<ICodeDescription>().Select(x => x.Code));

			AssertContainsExactElementsInAnyOrder(
				new[] { "A03" },
				collection.GetActiveCodeDescriptionPairList("S03").Cast<ICodeDescription>().Select(x => x.Code));

			AssertContainsExactElementsInAnyOrder(
				new[] { "A04" },
				collection.GetActiveCodeDescriptionPairList("S04").Cast<ICodeDescription>().Select(x => x.Code));

			AssertContainsExactElementsInAnyOrder(
				Array.Empty<string>(),
				collection.GetActiveCodeDescriptionPairList("S@@").Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestFallbackLevel()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add(new OpportunityClosedReasons());
			collection.Add(new OpportunityClosedReasons());
			AssertEquals(collection.CurrentFallbackLevel, collection[0].CurrentFallbackLevel);
			AssertEquals(collection.CurrentFallbackLevel, collection[1].CurrentFallbackLevel);

			var companyPK = Guid.NewGuid();
			var fallbackLevel = new FallbackLevel(companyPK, Guid.Empty, Guid.Empty);
			collection.CurrentFallbackLevel = fallbackLevel;
			AssertEquals(fallbackLevel, collection.CurrentFallbackLevel);
			AssertEquals(fallbackLevel, collection[0].CurrentFallbackLevel);
			AssertEquals(fallbackLevel, collection[1].CurrentFallbackLevel);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override OpportunityClosedReasonsCollection GetCollectionToTest()
		{
			return new OpportunityClosedReasonsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OpportunityClosedReasons();
		}
	}
}
