using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Startup.DeveloperFeatureControlOverrideForm;

namespace CargoWise.Main.Test.Startup.MainForm
{
	[TestedType(typeof(FeatureData))]
	sealed class FeatureDataTest : NonPersistentBusinessObjectTestCase
	{
	}

	[TestedType(typeof(FeatureDataCollection))]
	sealed class FeatureDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FeatureDataCollection>
	{
		protected override FeatureDataCollection GetCollectionToTest()
		{
			return new FeatureDataCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FeatureData();
		}
	}
}
