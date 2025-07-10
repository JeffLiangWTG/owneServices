using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommissionPeriodCollection))]
	sealed class CommissionPeriodCollectionTest : RegistryBusinessObjectCollectionTestCase<CommissionPeriodCollection>
	{
		#region Add

		public void TestSetDefaultsForNewChild()
		{
			var collection = new CommissionPeriodCollection();
			var period = collection.AddNew();
			AssertEquals(true, period.IsEnabled);
		}

		#endregion

		#region GetEnabledCodeDescriptionPairList

		public void TestGetEnabledCodeDescriptionPairList()
		{
			var collection = new CommissionPeriodCollection();
			collection.AddNew("12", (NoResString)"12", 1, 2).IsEnabled = true;
			collection.AddNew("23", (NoResString)"23", 2, 3).IsEnabled = false;
			collection.AddNew("34", (NoResString)"34", 3, 4).IsEnabled = true;

			AssertContainsExactElementsInAnyOrder(
				new[] { "12", "34" },
				collection.GetEnabledCodeDescriptionPairList().Cast<ICodeDescription>().Select(x => x.Code));
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CommissionPeriodCollection GetCollectionToTest()
		{
			return new CommissionPeriodCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CommissionPeriod();
		}

		#endregion
	}
}
