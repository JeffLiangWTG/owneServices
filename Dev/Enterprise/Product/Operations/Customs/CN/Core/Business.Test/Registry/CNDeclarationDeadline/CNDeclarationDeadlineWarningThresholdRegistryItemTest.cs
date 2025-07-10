using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDeclarationDeadlineWarningThresholdRegistryItem))]
	class CNDeclarationDeadlineWarningThresholdRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CNDeclarationDeadlineWarningThresholdCollection>
	{
		protected override StronglyTypedRegistryItem<CNDeclarationDeadlineWarningThresholdCollection, CNDeclarationDeadlineWarningThresholdCollection> GetNewRegistryItem()
			=> new CNDeclarationDeadlineWarningThresholdRegistryItem("", null, null, null, RegistryStorageFlags.Company, CNDeclarationDeadlineWarningThresholdCollection.GetDefault());

		protected override CNDeclarationDeadlineWarningThresholdCollection ValidValue
		{
			get
			{
				var collection = new CNDeclarationDeadlineWarningThresholdCollection();
				var deadlineWarning = collection.AddNew();

				deadlineWarning.TransportMode = CNDeclarationDeadlineWarningThreshold.ALL;
				deadlineWarning.FirstLevelThreshold = 0;
				deadlineWarning.FirstLevelWarningColor = "255000000";
				deadlineWarning.SecondLevelThreshold = 3;
				deadlineWarning.SecondLevelWarningColor = "255160122";
				deadlineWarning.ThirdLevelThreshold = 7;
				deadlineWarning.ThirdLevelWarningColor = "255255224";

				return collection;
			}
		}
	}
}
