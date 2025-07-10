using Enterprise.Integration;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.eHub.ScavengingSettingCollectionRegistryItem;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(ScavengingSettingCollectionRegistryItem))]
	sealed class ScavengingSettingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<ScavengingSettingCollection>
	{
		protected override StronglyTypedRegistryItem<ScavengingSettingCollection, ScavengingSettingCollection> GetNewRegistryItem()
		{
			return new ScavengingSettingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached);
		}

		[TestedType(typeof(ScavengingSettingRegistryDataType))]
		class ScavengingSettingRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ScavengingSettingRegistryDataType>
		{
			protected override string ExpectedEditorName => "ScavengingSettingsListControlItemEditor";
			protected override ScavengingSettingRegistryDataType GetNewDataType()
			{
				return new ScavengingSettingRegistryDataType();
			}

			protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
			{
				var collection = new ScavengingSettingCollection();
				var item = collection.AddNew();
				item.TaskName = "abc";

				var collection2 = new ScavengingSettingCollection();
				var item2 = collection2.AddNew();
				item2.TaskName = "Sbc";

				return new ValidSampleAndBinaryValueInDB[]
				{
					new ValidSampleAndBinaryValueInDB(collection, new ScavengingSettingRegistryDataType().Serialise(collection)),
					new ValidSampleAndBinaryValueInDB(collection2, new ScavengingSettingRegistryDataType().Serialise(collection2))
				};
			}
		}
	}
}
