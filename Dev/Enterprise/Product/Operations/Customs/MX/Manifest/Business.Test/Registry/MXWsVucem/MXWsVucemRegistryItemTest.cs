using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(MXWsVucemRegistryItem))]
	public class MXWsVucemRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<MXWsVucem>
	{
		protected override StronglyTypedRegistryItem<MXWsVucem, MXWsVucem> GetNewRegistryItem()
		{
			return new MXWsVucemRegistryItem(
				"", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport,
				new MXWsVucem
				{
					AirModeWSResponse = "http://127.0.0.1/",
					AirModeWSUsername = "ADMINVUCEM1",
					AirModeWSPassword = "9974567891",
					SeaModeWSResponse = "http://127.0.0.1/ws2",
					SeaModeWSUsername = "ADMINVUCEM2",
					SeaModeWSPassword = "9974567890",
				});
		}
	}

	[TestedType(typeof(MXWsVucemRegistryDataType))]
	public class MXWsVucemRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MXWsVucemRegistryDataType>
	{
		protected override MXWsVucemRegistryDataType GetNewDataType() => new MXWsVucemRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample1 = new MXWsVucem { SeaModeWSResponse = "www.exampleendpoint.com", SeaModeWSUsername = "ADMINVUCEM2", SeaModeWSPassword = "9974567890", AirModeWSResponse = "www.exampleendpoint2.com", AirModeWSUsername = "ADMINVUCEM1", AirModeWSPassword = "9974567891" };
			var sample2 = new MXWsVucem { SeaModeWSResponse = "http://127.0.0.1/ws1", SeaModeWSUsername = "ADMINVUCEM2", SeaModeWSPassword = "9974567890", AirModeWSResponse = "http://127.0.0.1/ws2", AirModeWSUsername = "ADMINVUCEM1", AirModeWSPassword = "9974567891" };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(sample1, new MXWsVucemRegistryDataType().Serialise(sample1)),
				new ValidSampleAndBinaryValueInDB(sample2, new MXWsVucemRegistryDataType().Serialise(sample2))
			};
		}

		protected override string ExpectedEditorName => "MXWsVucemRegistryItemEditor";
	}
}
