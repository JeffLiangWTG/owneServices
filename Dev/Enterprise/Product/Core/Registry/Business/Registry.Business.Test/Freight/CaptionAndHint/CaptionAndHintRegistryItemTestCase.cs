using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CaptionAndHintRegistryItem.CaptionAndHintRegistryDataType))]
	sealed class CaptionAndHintRegistryItemTestCase : NonPersistentBusinessObjectRegistryDataTypeTestCase<CaptionAndHintRegistryItem.CaptionAndHintRegistryDataType>
	{
		protected override CaptionAndHintRegistryItem.CaptionAndHintRegistryDataType GetNewDataType()
		{
			return new CaptionAndHintRegistryItem.CaptionAndHintRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CaptionAndHintRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CaptionAndHint captionAndHint = new CaptionAndHint("Caption", "Hint");

			byte[] value = new byte[]
			{
					255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,
					34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,
					0,62,0,60,0,67,0,97,0,112,0,116,0,105,0,111,0,110,0,65,0,110,0,100,0,72,0,105,0,110,0,116,0,62,0,60,0,67,0,
					97,0,112,0,116,0,105,0,111,0,110,0,62,0,67,0,97,0,112,0,116,0,105,0,111,0,110,0,60,0,47,0,67,0,97,0,112,0,
					116,0,105,0,111,0,110,0,62,0,60,0,72,0,105,0,110,0,116,0,62,0,72,0,105,0,110,0,116,0,60,0,47,0,72,0,105,0,
					110,0,116,0,62,0,60,0,47,0,67,0,97,0,112,0,116,0,105,0,111,0,110,0,65,0,110,0,100,0,72,0,105,0,110,0,116,0,
					62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(captionAndHint, value)
			};
		}
	}
}
