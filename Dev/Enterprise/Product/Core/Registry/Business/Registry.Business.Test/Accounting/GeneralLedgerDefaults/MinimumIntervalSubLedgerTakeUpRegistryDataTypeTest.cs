using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.MinimumIntervalSubLedgerTakeUpRegistryItem;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MinimumIntervalSubLedgerTakeUpRegistryDataType))]
	sealed class MinimumIntervalSubLedgerTakeUpRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MinimumIntervalSubLedgerTakeUpRegistryDataType>
	{
		#region Implementation

		protected override MinimumIntervalSubLedgerTakeUpRegistryDataType GetNewDataType()
		{
			return new MinimumIntervalSubLedgerTakeUpRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "MinimumIntervalSubLedgerTakeUpRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			MinimumIntervalSubLedgerTakeUp obj1 = new MinimumIntervalSubLedgerTakeUp();
			MinimumIntervalSubLedgerTakeUp obj2 = new MinimumIntervalSubLedgerTakeUp();
			obj2.Interval = 30;
			obj2.IntervalType = MinimumIntervalSubLedgerTakeUp.IntervalTypes.Hours;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(obj1, DataType.Serialise(obj1)),
				new ValidSampleAndBinaryValueInDB(obj2, DataType.Serialise(obj2))
			};
		}

		#endregion
	}
}
