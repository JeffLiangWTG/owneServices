using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ABCAnalysisCategoryRegistryDataType))]
	sealed class ABCAnalysisCategoryRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ABCAnalysisCategoryRegistryDataType>
	{
		#region Implementation

		protected override ABCAnalysisCategoryRegistryDataType GetNewDataType()
		{
			return new ABCAnalysisCategoryRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ABCAnalysisCategoryRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ABCAnalysisCategoryCollection();

			var abcCategory = collection.AddNew();
			abcCategory.CategoryName = "A";
			abcCategory.Operator = "<";
			abcCategory.PercentageOfTotal = 50;

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,65,0,66,0,67,0,65,0,110,0,97,0,108,0,121,0,115,0,105,0,115,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,
0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,
0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,
0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,
0,62,0,60,0,65,0,66,0,67,0,65,0,110,0,97,0,108,0,121,0,115,0,105,0,115,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,62,0,60,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,78,0,97,
0,109,0,101,0,62,0,65,0,60,0,47,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,78,0,97,0,109,0,101,0,62,0,60,0,79,0,112,0,101,0,114,0,97,0,116,0,111,0,114,0,62,0,38,0,103,0,116,0,59,
0,60,0,47,0,79,0,112,0,101,0,114,0,97,0,116,0,111,0,114,0,62,0,60,0,80,0,101,0,114,0,99,0,101,0,110,0,116,0,97,0,103,0,101,0,79,0,102,0,84,0,111,0,116,0,97,0,108,0,62,0,53,0,48,0,60,
0,47,0,80,0,101,0,114,0,99,0,101,0,110,0,116,0,97,0,103,0,101,0,79,0,102,0,84,0,111,0,116,0,97,0,108,0,62,0,60,0,47,0,65,0,66,0,67,0,65,0,110,0,97,0,108,0,121,0,115,0,105,0,115,0,67,
0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,65,0,66,0,67,0,65,0,110,0,97,0,108,0,121,0,115,0,105,0,115,0,67,0,97,0,116,0,101,0,103,
0,111,0,114,0,121,0,62,0
};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
