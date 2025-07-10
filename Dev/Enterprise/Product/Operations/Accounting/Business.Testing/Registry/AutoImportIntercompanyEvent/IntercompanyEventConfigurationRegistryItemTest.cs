using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.IntercompanyEventConfigurationRegistryItem;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyEventConfigurationRegistryItem))]
	public class IntercompanyEventConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<IntercompanyEventConfiguration>
	{
		protected override StronglyTypedRegistryItem<IntercompanyEventConfiguration, IntercompanyEventConfiguration> GetNewRegistryItem()
		{
			return new IntercompanyEventConfigurationRegistryItem(string.Empty, null, null, null
				, Enterprise.Integration.RegistryStorageFlags.System | Enterprise.Integration.RegistryStorageFlags.Company, Enterprise.Integration.RegistryOptions.IsOnlyForCargoWise, new IntercompanyEventConfiguration());
		}
	}

	[TestedType(typeof(IntercompanyEventConfigurationRegistryItemDataType))]
	public class IntercompanyEventConfigurationRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IntercompanyEventConfigurationRegistryItemDataType>
	{
		protected override IntercompanyEventConfigurationRegistryItemDataType GetNewDataType()
		{
			return new IntercompanyEventConfigurationRegistryItemDataType();
		}

		protected override string ExpectedEditorName => "IntercompanyEventConfigurationRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			IntercompanyEventConfiguration sample1 = new IntercompanyEventConfiguration();

			var settings1 = new IntercompanyEventSetting();
			settings1.StmEventCode = Events.CustomisableEvent00.Code;
			settings1.StartDate = ZDate.BrettsBirthday;

			var settings2 = new IntercompanyEventSetting();
			settings2.StmEventCode = Events.CustomisableEvent01.Code;
			settings2.StartDate = ZDate.Today;

			sample1.EnableEventConfiguration = true;

			sample1.IntercompanyEventSettingCollection.Add(settings1);
			sample1.IntercompanyEventSettingCollection.Add(settings2);

			IntercompanyEventConfiguration sample2 = new IntercompanyEventConfiguration();

			var settings3 = new IntercompanyEventSetting();
			settings3.StmEventCode = Events.CustomisableEvent02.Code;
			settings3.StartDate = ZDate.Today.AddDays(78);

			sample2.EnableEventConfiguration = true;

			sample2.IntercompanyEventSettingCollection.Add(settings3);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, DataType.Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, DataType.Serialise(sample2))
			};
		}
	}
}
