using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDeclarationDeadlineWarningThresholdRegistryDataType))]
	class CNDeclarationDeadlineWarningThresholdRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CNDeclarationDeadlineWarningThresholdRegistryDataType>
	{
		protected override CNDeclarationDeadlineWarningThresholdRegistryDataType GetNewDataType() => new CNDeclarationDeadlineWarningThresholdRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CNDeclarationDeadlineWarningThresholdCollection();
			var deadlineWarning1 = collection1.AddNew();

			deadlineWarning1.TransportMode = "ALL";
			deadlineWarning1.FirstLevelThreshold = 0;
			deadlineWarning1.FirstLevelWarningColor = "255000000";
			deadlineWarning1.SecondLevelThreshold = 3;
			deadlineWarning1.SecondLevelWarningColor = "255160122";
			deadlineWarning1.ThirdLevelThreshold = 7;
			deadlineWarning1.ThirdLevelWarningColor = "255255224";

			var collection2 = new CNDeclarationDeadlineWarningThresholdCollection();
			var deadlineWarning2 = collection2.AddNew();

			deadlineWarning2.TransportMode = "AIR";
			deadlineWarning2.FirstLevelThreshold = 1;
			deadlineWarning2.FirstLevelWarningColor = "255000030";
			deadlineWarning2.SecondLevelThreshold = 4;
			deadlineWarning2.SecondLevelWarningColor = "255160132";
			deadlineWarning2.ThirdLevelThreshold = 8;
			deadlineWarning2.ThirdLevelWarningColor = "255255234";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new CNDeclarationDeadlineWarningThresholdRegistryDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new CNDeclarationDeadlineWarningThresholdRegistryDataType().Serialise(collection2))
			};
		}

		protected override string ExpectedEditorName => "CNDeclarationDeadlineWarningThresholdEditor";
	}
}
