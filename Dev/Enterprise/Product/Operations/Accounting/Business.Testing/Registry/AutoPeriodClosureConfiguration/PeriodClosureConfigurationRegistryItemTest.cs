using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(PeriodClosureConfigurationRegistryItem))]
	class PeriodClosureConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<PeriodClosureConfiguration>
	{
		protected override StronglyTypedRegistryItem<PeriodClosureConfiguration, PeriodClosureConfiguration> GetNewRegistryItem()
		{
			return new PeriodClosureConfigurationRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new PeriodClosureConfiguration());
		}
	}

	[TestedType(typeof(PeriodClosureConfigurationRegistryDataType))]
	class PeriodClosureConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PeriodClosureConfigurationRegistryDataType>
	{
		#region Implementation

		protected override PeriodClosureConfigurationRegistryDataType GetNewDataType()
		{
			return new PeriodClosureConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PeriodClosureConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			PeriodClosureConfiguration copy1 = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 1, 1);
			PeriodClosureConfiguration copy2 = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Hours, 2, 2, 2);
			PeriodClosureConfiguration copy3 = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Days, 3, 3, 3);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy1, DataType.Serialise(copy1)),
				new ValidSampleAndBinaryValueInDB(copy2, DataType.Serialise(copy2)),
				new ValidSampleAndBinaryValueInDB(copy3, DataType.Serialise(copy3))
			};
		}

		#endregion
	}
}
