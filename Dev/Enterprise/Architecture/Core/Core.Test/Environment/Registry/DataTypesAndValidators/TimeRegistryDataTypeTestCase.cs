using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(TimeRegistryDataType))]
	sealed class TimeRegistryDataTypeTestCase : RegistryDataTypeTestCase<TimeRegistryDataType>
	{
		protected override TimeRegistryDataType GetNewDataType()
		{
			return new TimeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
					new ValidSampleAndBinaryValueInDB("9:45am", Encoding.Unicode.GetBytes("9:45am")),
					new ValidSampleAndBinaryValueInDB("22:00:33", Encoding.Unicode.GetBytes("22:00:33")),
					new ValidSampleAndBinaryValueInDB("10:35:04 pm", Encoding.Unicode.GetBytes("10:35:04 pm")),
					new ValidSampleAndBinaryValueInDB("21:07", Encoding.Unicode.GetBytes("21:07"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "19:45 am", "07:61 am", "not even a time" };
		}

		protected override object GetNullRepresentation()
		{
			return Encoding.Unicode.GetBytes(StringRegistryDataType.MagicNullString);
		}
	}
}
