using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDeclarationDeadlineWarningThresholdCollection))]
	class CNDeclarationDeadlineWarningThresholdCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CNDeclarationDeadlineWarningThresholdCollection>
	{
		public void TestGetDefault()
		{
			var collection = CNDeclarationDeadlineWarningThresholdCollection.GetDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, collection.Count);
				var item = collection[0];
				AssertEquals("TransportMode", CNDeclarationDeadlineWarningThreshold.ALL, item.TransportMode);
				AssertEquals("FirstLevelThreshold", 1, item.FirstLevelThreshold);
				AssertEquals("FirstLevelWarningColor", "255,000,000", item.FirstLevelWarningColor);
				AssertEquals("SecondLevelThreshold", 3, item.SecondLevelThreshold);
				AssertEquals("SecondLevelWarningColor", "255,160,122", item.SecondLevelWarningColor);
				AssertEquals("ThirdLevelThreshold", 7, item.ThirdLevelThreshold);
				AssertEquals("ThirdLevelWarningColor", "255,255,224", item.ThirdLevelWarningColor);
				AssertEquals("DelayedWarningColor", "", item.DelayedWarningColor);
			});
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override CNDeclarationDeadlineWarningThresholdCollection GetCollectionToTest() => new CNDeclarationDeadlineWarningThresholdCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CNDeclarationDeadlineWarningThreshold();
	}
}
