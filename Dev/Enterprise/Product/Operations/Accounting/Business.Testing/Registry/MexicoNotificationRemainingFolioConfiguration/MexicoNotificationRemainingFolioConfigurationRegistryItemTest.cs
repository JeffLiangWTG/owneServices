using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(MexicoNotificationRemainingFolioConfigurationRegistryItem))]
	class MexicoNotificationRemainingFolioConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<MexicoNotificationRemainingFolioConfiguration>
	{
		protected override StronglyTypedRegistryItem<MexicoNotificationRemainingFolioConfiguration, MexicoNotificationRemainingFolioConfiguration> GetNewRegistryItem()
		{
			return new MexicoNotificationRemainingFolioConfigurationRegistryItem(
						name: string.Empty,
						category: null,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.Company,
						options: RegistryOptions.Default,
						defaultValue: new MexicoNotificationRemainingFolioConfiguration()
						);
		}
	}

	[TestedType(typeof(MexicoNotificationRemainingFolioConfigurationRegistryDataType))]
	class MexicoNotificationRemainingFolioConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MexicoNotificationRemainingFolioConfigurationRegistryDataType>
	{
		#region Implementation

		protected override MexicoNotificationRemainingFolioConfigurationRegistryDataType GetNewDataType()
		{
			return new MexicoNotificationRemainingFolioConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "MexicoNotificationRemainingFolioConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			MexicoNotificationRemainingFolioConfiguration copy1 = new MexicoNotificationRemainingFolioConfiguration
			{
				FoliosQuantity = 0,
				Interval = 0
			};

			MexicoNotificationRemainingFolioConfiguration copy2 = new MexicoNotificationRemainingFolioConfiguration
			{
				FoliosQuantity = 100,
				Interval = 10
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy1, DataType.Serialise(copy1)),
				new ValidSampleAndBinaryValueInDB(copy2, DataType.Serialise(copy2))
			};
		}

		#endregion
	}
}
