using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(SendOrganizationDataToCertCaptureDataType))]
	public class SendOrganizationDataToCertCaptureDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SendOrganizationDataToCertCaptureDataType>
	{
		protected override SendOrganizationDataToCertCaptureDataType GetNewDataType()
		{
			return new SendOrganizationDataToCertCaptureDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data1 = new SendOrganizationDataToCertCapture { EnableSend = true };
			var data2 = new SendOrganizationDataToCertCapture { EnableSend = false };

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(data1, new SendOrganizationDataToCertCaptureDataType().Serialise(data1)),
				new ValidSampleAndBinaryValueInDB(data2, new SendOrganizationDataToCertCaptureDataType().Serialise(data2)),
			};
		}

		protected override string ExpectedEditorName => "SendOrganizationDataToCertCaptureRegistryEditor";
	}
}
