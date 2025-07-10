using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(FaxPriceDataType))]
	class FaxPriceDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FaxPriceDataType>
	{
		#region Implementation

		protected override FaxPriceDataType GetNewDataType()
		{
			return new FaxPriceDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "FaxPriceRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			FaxPriceCollection collection = new FaxPriceCollection();
			FaxPrice item1 = collection.AddNew();
			item1.Code = "AUD";
			item1.Price = 0.20m;

			FaxPrice item2 = collection.AddNew();
			item2.Code = "NZD";
			item2.Price = 0.12m;

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,70,0,97,0,120,0,80,0,114,0,105,0,99,0,101,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,
				0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,
				0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,
				0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,70,0,97,0,120,0,80,0,114,0,105,0,99,0,101,
				0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,0,85,0,68,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,80,0,114,0,105,0,99,0,101,0,62,0,48,0,46,0,50,0,48,0,60,0,47,0,80,0,114,0,105,
				0,99,0,101,0,62,0,60,0,47,0,70,0,97,0,120,0,80,0,114,0,105,0,99,0,101,0,62,0,60,0,70,0,97,0,120,0,80,0,114,0,105,0,99,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,78,0,90,0,68,
				0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,80,0,114,0,105,0,99,0,101,0,62,0,48,0,46,0,49,0,50,0,60,0,47,0,80,0,114,0,105,0,99,0,101,0,62,0,60,0,47,0,70,0,97,0,120,0,80,0,114,
				0,105,0,99,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,70,0,97,0,120,0,80,0,114,0,105,0,99,0,101,0,62,0
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
