using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryPriorityRegistryDataType))]
	sealed class HBLDeliveryPriorityRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<HBLDeliveryPriorityRegistryDataType>
	{
		protected override string ExpectedEditorName => "HBLDeliveryPriorityRegistryItemEditor";

		protected override HBLDeliveryPriorityRegistryDataType GetNewDataType()
			=> new HBLDeliveryPriorityRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			#region First

			var defaultItems = new HBLDeliveryPriorityConfigCollection();
			var configuration = defaultItems.AddNew();
			configuration.ContainerMode = "FCL";
			configuration.HBLDeliveryMode = "DOOR/DOOR";

			var setting = configuration.Settings.AddNew();
			setting.HBLDeliveryModePriority = "DOOR/DOOR";

			var setting1 = configuration.Settings.AddNew();
			setting1.HBLDeliveryModePriority = "DOOR/CFS";

			var first = defaultItems;
			var firstXml = @"<?xml version=""1.0"" encoding=""utf-16""?><HBLDeliveryPriorities xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><HBLDeliveryPriorityConfig><ContainerMode>FCL</ContainerMode><HBLDeliveryMode>DOOR/DOOR</HBLDeliveryMode><HBLDeliveryFallBackPriorities><HBLDeliveryPrioritySetting><HBLDeliveryModePriority>DOOR/DOOR</HBLDeliveryModePriority></HBLDeliveryPrioritySetting><HBLDeliveryPrioritySetting><HBLDeliveryModePriority>DOOR/CFS</HBLDeliveryModePriority></HBLDeliveryPrioritySetting></HBLDeliveryFallBackPriorities></HBLDeliveryPriorityConfig></HBLDeliveryPriorities>";

			#endregion

			#region Second

			defaultItems = new HBLDeliveryPriorityConfigCollection();
			configuration = defaultItems.AddNew();
			configuration.ContainerMode = "FCL";
			configuration.HBLDeliveryMode = "DOOR/DOOR";

			setting = configuration.Settings.AddNew();
			setting.HBLDeliveryModePriority = "DOOR/DOOR";

			setting1 = configuration.Settings.AddNew();
			setting1.HBLDeliveryModePriority = "DOOR/CFS";

			var configuration1 = defaultItems.AddNew();
			configuration1.ContainerMode = "BCN";
			configuration1.HBLDeliveryMode = "CFS/DOOR";

			var setting3 = configuration1.Settings.AddNew();
			setting3.HBLDeliveryModePriority = "CFS/DOOR";

			var second = defaultItems;
			var secondXml = @"<?xml version=""1.0"" encoding=""utf-16""?><HBLDeliveryPriorities xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><HBLDeliveryPriorityConfig><ContainerMode>FCL</ContainerMode><HBLDeliveryMode>DOOR/DOOR</HBLDeliveryMode><HBLDeliveryFallBackPriorities><HBLDeliveryPrioritySetting><HBLDeliveryModePriority>DOOR/DOOR</HBLDeliveryModePriority></HBLDeliveryPrioritySetting><HBLDeliveryPrioritySetting><HBLDeliveryModePriority>DOOR/CFS</HBLDeliveryModePriority></HBLDeliveryPrioritySetting></HBLDeliveryFallBackPriorities></HBLDeliveryPriorityConfig><HBLDeliveryPriorityConfig><ContainerMode>BCN</ContainerMode><HBLDeliveryMode>CFS/DOOR</HBLDeliveryMode><HBLDeliveryFallBackPriorities><HBLDeliveryPrioritySetting><HBLDeliveryModePriority>CFS/DOOR</HBLDeliveryModePriority></HBLDeliveryPrioritySetting></HBLDeliveryFallBackPriorities></HBLDeliveryPriorityConfig></HBLDeliveryPriorities>";

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(first, firstXml),
				new ValidSampleAndBinaryValueInDB(second, secondXml),
			};
		}
	}
}
