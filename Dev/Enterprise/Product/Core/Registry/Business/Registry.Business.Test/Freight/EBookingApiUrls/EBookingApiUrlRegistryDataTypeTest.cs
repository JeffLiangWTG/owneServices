using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EBookingApiUrlRegistryDataType))]
	sealed class EBookingApiUrlRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EBookingApiUrlRegistryDataType>
	{
		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override string ExpectedEditorName
		{
			get { return "EBookingApiUrlRegistryItemEditor"; }
		}

		protected override EBookingApiUrlRegistryDataType GetNewDataType()
		{
			return new EBookingApiUrlRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var eBookingApiUrls1 = GetNewEBookingApiUrls(ZString.Empty);
			var eBookingApiUrls2 = GetNewEBookingApiUrls(EBookingApiUrls.Constants.ProdCode);
			var eBookingApiUrls3 = GetNewEBookingApiUrls(EBookingApiUrls.Constants.TestCode);

			byte[] byteArray1 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,69,0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,115,0,62,0,60,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,69,
				0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,67,0,111,0,100,0,101,0,32,0,47,0,62,0,60,0,47,0,69,0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,
				0,85,0,114,0,108,0,115,0,62,0
			};

			byte[] byteArray2 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,69,0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,115,0,62,0,60,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,69,
				0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,67,0,111,0,100,0,101,0,62,0,80,0,82,0,79,0,68,0,60,0,47,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,69,
				0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,69,0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,
				0,108,0,115,0,62,0
			};

			byte[] byteArray3 = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,69,0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,115,0,62,0,60,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,69,
				0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,67,0,111,0,100,0,101,0,62,0,84,0,69,0,83,0,84,0,60,0,47,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,69,
				0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,0,108,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,69,0,66,0,111,0,111,0,107,0,105,0,110,0,103,0,65,0,112,0,105,0,85,0,114,
				0,108,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(eBookingApiUrls1, byteArray1),
				new ValidSampleAndBinaryValueInDB(eBookingApiUrls2, byteArray2),
				new ValidSampleAndBinaryValueInDB(eBookingApiUrls3, byteArray3)
			};
		}

		EBookingApiUrls GetNewEBookingApiUrls(ZString code)
		{
			EBookingApiUrls ebookingApiUrls = new EBookingApiUrls();
			ebookingApiUrls.SelectedEBookingApiUrlCode = code;
			return ebookingApiUrls;
		}
	}
}
