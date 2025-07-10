using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType))]
	sealed class CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType>
	{
		protected override CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType GetNewDataType()
		{
			return new CalculateDeliveryDateWithExceptionsOptionsRegistryItemDataType(new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 12 }, 0, 24);
		}

		protected override string ExpectedEditorName => "CalculateDeliveryDateWithExceptionsOptionsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var option1 = new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 0, UnlimitedDuration = false };
			var option2 = new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 10, UnlimitedDuration = false };
			var option3 = new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = 24, UnlimitedDuration = false };

			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CalculateDeliveryDateWithExceptionsOptions><MaximumDurationHours>0</MaximumDurationHours><UnlimitedDuration>N</UnlimitedDuration></CalculateDeliveryDateWithExceptionsOptions>";
			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CalculateDeliveryDateWithExceptionsOptions><MaximumDurationHours>10</MaximumDurationHours><UnlimitedDuration>N</UnlimitedDuration></CalculateDeliveryDateWithExceptionsOptions>";
			var xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CalculateDeliveryDateWithExceptionsOptions><MaximumDurationHours>24</MaximumDurationHours><UnlimitedDuration>N</UnlimitedDuration></CalculateDeliveryDateWithExceptionsOptions>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(option1, xml1),
				new ValidSampleAndBinaryValueInDB(option2, xml2),
				new ValidSampleAndBinaryValueInDB(option3, xml3)
			};
		}
	}
}
