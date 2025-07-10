using System.Text;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public struct ValidSampleAndBinaryValueInDB
	{
		public ValidSampleAndBinaryValueInDB(object validSample, string xmlValue) : this(validSample, Encoding.Unicode.GetBytes(xmlValue))
		{ }

		public ValidSampleAndBinaryValueInDB(object validSample, byte[] binaryValue)
		{
			this.validSample = validSample;
			this.binaryValue = binaryValue;
		}

		public object ValidSample
		{
			get { return validSample; }
		}

		public byte[] BinaryValue
		{
			get { return binaryValue; }
		}

		readonly object validSample;
		readonly byte[] binaryValue;
	}
}
