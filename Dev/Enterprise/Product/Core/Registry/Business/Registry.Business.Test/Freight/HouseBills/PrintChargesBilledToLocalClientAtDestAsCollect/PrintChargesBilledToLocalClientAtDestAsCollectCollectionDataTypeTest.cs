using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataType))]
	class PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataType>
	{
		#region Implementation
		protected override string ExpectedEditorName
		{
			get
			{
				return "PrintChargesBilledToLocalClientAtDestAsCollectRegistryItemEditor";
			}
		}

		protected override PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataType GetNewDataType()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataType(new PrintChargesBilledToLocalClientAtDestAsCollectCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			var transportMode = collection.AddNew();
			transportMode.TransportMode = Core.Constants.TransportModes.Sea;
			transportMode.ExportCountry = Core.Constants.CountryCodes.Belgium;
			transportMode.ImportCountry = Core.Constants.CountryCodes.Australia;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,80,0,114,0,105,0,110,0,116,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,66,0,105,0,108,0,108,0,101,0,100,0,84,0,111,0,76,0,111,0,99,0,97,0,108,0,67,0,108,0,105,0,101,0,110,0,116,0,65,0,116,0,68,0,101,0,115,0,116,0,65,0,115,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,80,0,114,0,105,0,110,0,116,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,66,0,105,0,108,0,108,0,101,0,100,0,84,0,111,0,76,0,111,0,99,0,97,0,108,0,67,0,108,0,105,0,101,0,110,0,116,0,65,0,116,0,68,0,101,0,115,0,116,0,65,0,115,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,62,0,60,0,84,0,114,0,97,0,110,0,115,0,112,0,111,0,114,0,116,0,77,0,111,0,100,0,101,0,62,0,83,0,69,0,65,0,60,0,47,0,84,0,114,0,97,0,110,0,115,0,112,0,111,0,114,0,116,0,77,0,111,0,100,0,101,0,62,0,60,0,69,0,120,0,112,0,111,0,114,0,116,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,66,0,69,0,60,0,47,0,69,0,120,0,112,0,111,0,114,0,116,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,60,0,73,0,109,0,112,0,111,0,114,0,116,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,65,0,85,0,60,0,47,0,73,0,109,0,112,0,111,0,114,0,116,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,62,0,60,0,47,0,80,0,114,0,105,0,110,0,116,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,66,0,105,0,108,0,108,0,101,0,100,0,84,0,111,0,76,0,111,0,99,0,97,0,108,0,67,0,108,0,105,0,101,0,110,0,116,0,65,0,116,0,68,0,101,0,115,0,116,0,65,0,115,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,80,0,114,0,105,0,110,0,116,0,67,0,104,0,97,0,114,0,103,0,101,0,115,0,66,0,105,0,108,0,108,0,101,0,100,0,84,0,111,0,76,0,111,0,99,0,97,0,108,0,67,0,108,0,105,0,101,0,110,0,116,0,65,0,116,0,68,0,101,0,115,0,116,0,65,0,115,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}
		#endregion
	}
}
