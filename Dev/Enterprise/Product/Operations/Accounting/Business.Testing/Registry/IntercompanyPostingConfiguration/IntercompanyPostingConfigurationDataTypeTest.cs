using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyPostingConfigurationRegistryDataType))]
	class IntercompanyPostingConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IntercompanyPostingConfigurationRegistryDataType>
	{
		#region Implementation

		protected override IntercompanyPostingConfigurationRegistryDataType GetNewDataType()
		{
			return new IntercompanyPostingConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "IntercompanyPostingConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			IntercompanyPostingConfigurationCollection collection = new IntercompanyPostingConfigurationCollection();
			IntercompanyPostingConfiguration intercompanyPostingConfiguration = collection.AddNew();
			intercompanyPostingConfiguration.MaxCostVarianceApprovalLevel = intercompanyPostingConfiguration.MaxCostVarianceApprovalLevelList[0].Code;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,80,0,111,0,115,0,116,0,105,0,110,
0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,
0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,
0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,
0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,80,0,111,0,115,0,116,0,105,
0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,67,0,111,0,109,0,112,0,97,0,110,0,121,0,32,0,47,0,62,0,60,0,77,0,97,0,120,0,67,0,111,
0,115,0,116,0,86,0,97,0,114,0,105,0,97,0,110,0,99,0,101,0,65,0,112,0,112,0,114,0,111,0,118,0,97,0,108,0,76,0,101,0,118,0,101,0,108,0,62,0,78,0,111,0,110,0,101,0,60,0,47,0,77,0,97,0,120,
0,67,0,111,0,115,0,116,0,86,0,97,0,114,0,105,0,97,0,110,0,99,0,101,0,65,0,112,0,112,0,114,0,111,0,118,0,97,0,108,0,76,0,101,0,118,0,101,0,108,0,62,0,60,0,47,0,73,0,110,0,116,0,101,0,114,
0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,80,0,111,0,115,0,116,0,105,0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,65,0,114,0,114,
0,97,0,121,0,79,0,102,0,73,0,110,0,116,0,101,0,114,0,99,0,111,0,109,0,112,0,97,0,110,0,121,0,80,0,111,0,115,0,116,0,105,0,110,0,103,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,
0,105,0,111,0,110,0,62,0
};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
