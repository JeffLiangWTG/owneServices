using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DisbursementJobsClosureConfigurationRegistryDataType))]
	class DisbursementJobsClosureConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DisbursementJobsClosureConfigurationRegistryDataType>
	{
		protected override DisbursementJobsClosureConfigurationRegistryDataType GetNewDataType()
		{
			return new DisbursementJobsClosureConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DisbursementJobsClosureConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var configuration = new DisbursementJobsClosureConfiguration();

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,68,0,105,0,115,0,98,0,117,0,114,0,115,0,101,0,109,0,101,0,110,0,116,0,74,0,111,0,98,0,115,0,67,0,108,0,111,0,115,0,117,0,114,0,101,0,67,0,111,0,110,
				0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,74,0,111,0,98,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,104,0,111,0,114,0,116,0,102,0,97,0,108,0,108,0,85,0,112,
				0,84,0,111,0,62,0,48,0,60,0,47,0,74,0,111,0,98,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,104,0,111,0,114,0,116,0,102,0,97,0,108,0,108,0,85,0,112,0,84,0,111,0,62,0,60,0,74,0,111,
				0,98,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,117,0,114,0,112,0,108,0,117,0,115,0,85,0,112,0,84,0,111,0,62,0,48,0,60,0,47,0,74,0,111,0,98,0,76,0,101,0,118,0,101,0,108,0,79,0,102,
				0,83,0,117,0,114,0,112,0,108,0,117,0,115,0,85,0,112,0,84,0,111,0,62,0,60,0,65,0,103,0,103,0,114,0,101,0,103,0,97,0,116,0,101,0,100,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,104,0,111,
				0,114,0,116,0,102,0,97,0,108,0,108,0,85,0,112,0,84,0,111,0,62,0,48,0,60,0,47,0,65,0,103,0,103,0,114,0,101,0,103,0,97,0,116,0,101,0,100,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,104,
				0,111,0,114,0,116,0,102,0,97,0,108,0,108,0,85,0,112,0,84,0,111,0,62,0,60,0,65,0,103,0,103,0,114,0,101,0,103,0,97,0,116,0,101,0,100,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,117,0,114,
				0,112,0,108,0,117,0,115,0,85,0,112,0,84,0,111,0,62,0,48,0,60,0,47,0,65,0,103,0,103,0,114,0,101,0,103,0,97,0,116,0,101,0,100,0,76,0,101,0,118,0,101,0,108,0,79,0,102,0,83,0,117,0,114,0,112,
				0,108,0,117,0,115,0,85,0,112,0,84,0,111,0,62,0,60,0,47,0,68,0,105,0,115,0,98,0,117,0,114,0,115,0,101,0,109,0,101,0,110,0,116,0,74,0,111,0,98,0,115,0,67,0,108,0,111,0,115,0,117,0,114,0,101,
				0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(configuration, byteArrayValue)
			};
		}
	}
}
