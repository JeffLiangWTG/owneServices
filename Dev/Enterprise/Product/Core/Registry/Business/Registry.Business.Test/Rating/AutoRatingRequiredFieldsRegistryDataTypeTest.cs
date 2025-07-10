using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoRatingRequiredFieldsRegistryDataType))]
	sealed class AutoRatingRequiredFieldsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutoRatingRequiredFieldsRegistryDataType>
	{
		protected override AutoRatingRequiredFieldsRegistryDataType GetNewDataType()
		{
			return new AutoRatingRequiredFieldsRegistryDataType();
		}

		protected override bool HasEditor
		{
			get { return false; } // Using an EditorInfo instead of standard RegistryEditor attribute.
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			AutoRatingRequiredFields fields = new AutoRatingRequiredFields();

			fields.RequireServiceLevel = false;
			fields.RequireFrequency = true;
			fields.RequireTransitTime = false;
			fields.RequireCommodityCode = true;
			fields.RequireIncoterm = false;

			byte[] byteArray = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,
				110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,117,0,116,0,111,
				0,82,0,97,0,116,0,105,0,110,0,103,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,100,0,70,0,105,0,101,0,108,0,100,0,115,0,62,0,
				60,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0,78,0,
				60,0,47,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,83,0,101,0,114,0,118,0,105,0,99,0,101,0,76,0,101,0,118,0,101,0,108,0,62,0,
				60,0,82,0,101,0,113,0,117,0,105,0,114,0,101,0,70,0,114,0,101,0,113,0,117,0,101,0,110,0,99,0,121,0,62,0,89,0,60,0,47,0,82,0,
				101,0,113,0,117,0,105,0,114,0,101,0,70,0,114,0,101,0,113,0,117,0,101,0,110,0,99,0,121,0,62,0,60,0,82,0,101,0,113,0,117,0,105,
				0,114,0,101,0,84,0,114,0,97,0,110,0,115,0,105,0,116,0,84,0,105,0,109,0,101,0,62,0,78,0,60,0,47,0,82,0,101,0,113,0,117,0,105,
				0,114,0,101,0,84,0,114,0,97,0,110,0,115,0,105,0,116,0,84,0,105,0,109,0,101,0,62,0,60,0,82,0,101,0,113,0,117,0,105,0,114,0,
				101,0,67,0,111,0,109,0,109,0,111,0,100,0,105,0,116,0,121,0,67,0,111,0,100,0,101,0,62,0,89,0,60,0,47,0,82,0,101,0,113,0,117,0,
				105,0,114,0,101,0,67,0,111,0,109,0,109,0,111,0,100,0,105,0,116,0,121,0,67,0,111,0,100,0,101,0,62,0,60,0,82,0,101,0,113,0,117,
				0,105,0,114,0,101,0,73,0,110,0,99,0,111,0,116,0,101,0,114,0,109,0,62,0,78,0,60,0,47,0,82,0,101,0,113,0,117,0,105,0,114,0,101,
				0,73,0,110,0,99,0,111,0,116,0,101,0,114,0,109,0,62,0,60,0,47,0,65,0,117,0,116,0,111,0,82,0,97,0,116,0,105,0,110,0,103,0,82,0,
				101,0,113,0,117,0,105,0,114,0,101,0,100,0,70,0,105,0,101,0,108,0,100,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(fields, byteArray)
			};
		}
	}
}
