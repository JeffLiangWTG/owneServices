using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobProfitLossRequiringReasonParametersRegistryDataType))]
	class JobProfitLossRequiringReasonParametersRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<JobProfitLossRequiringReasonParametersRegistryDataType>
	{
		#region Implementation

		protected override JobProfitLossRequiringReasonParametersRegistryDataType GetNewDataType()
		{
			return new JobProfitLossRequiringReasonParametersRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "JobProfitLossRequiringReasonParametersRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			JobProfitLossRequiringReasonParameters validSample = new JobProfitLossRequiringReasonParameters();

			validSample.ProfitThreshold = 20M;
			validSample.JobStatusCollection.AddNew().Code = "WRK";
			string stringValue = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><JobProfitLossRequiringReasonParameters><ProfitThreshold>20.00</ProfitThreshold><LossThreshold>0.00</LossThreshold><ArrayOfCodeSelection xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><CodeSelection><Code>WRK</Code></CodeSelection></ArrayOfCodeSelection></JobProfitLossRequiringReasonParameters>";

			byte[] byteArrayValue = System.Text.Encoding.Unicode.GetBytes(stringValue);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(validSample, byteArrayValue)
			};
		}

		#endregion
	}
}
