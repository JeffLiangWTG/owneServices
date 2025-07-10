using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.CrossTradeDebtorConfigurationRegistryItem;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CrossTradeDebtorConfigurationRegistryItem))]
	public class CrossTradeDebtorConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<CrossTradeDebtorConfigurationHeader>
	{
		protected override StronglyTypedRegistryItem<CrossTradeDebtorConfigurationHeader, CrossTradeDebtorConfigurationHeader> GetNewRegistryItem()
		{
			return new CrossTradeDebtorConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new CrossTradeDebtorConfigurationHeader());
		}
	}

	[TestedType(typeof(CrossTradeDebtorConfigurationRegistryDataType))]
	public class CrossTradeDebtorConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CrossTradeDebtorConfigurationRegistryDataType>
	{
		#region Implementation

		protected override CrossTradeDebtorConfigurationRegistryDataType GetNewDataType()
		{
			return new CrossTradeDebtorConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CrossTradeDebtorConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var header1 = new CrossTradeDebtorConfigurationHeader();
			var config1 = header1.Configurations.AddNew();
			config1.JobType = "ALL";
			config1.DirectionCode = "OTH";
			config1.Mode = "AIR";
			config1.ChargePaymentType = "PPD";
			config1.Debtor = "PBP";

			var header2 = new CrossTradeDebtorConfigurationHeader();
			var config2 = header2.Configurations.AddNew();
			config2.JobType = "SHP";
			config2.DirectionCode = "OTH";
			config2.Mode = "SEA";
			config2.ChargePaymentType = "CCX";
			config2.Debtor = "CBP";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(header1, DataType.Serialise(header1)),
				new ValidSampleAndBinaryValueInDB(header2, DataType.Serialise(header2))
			};
		}

		#endregion
	}
}
