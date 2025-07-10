using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;

namespace Enterprise.Client.JAS.Business.Utilities.Testing
{
	class JASCsvLineForTest : JASCsvLine
	{
		public JASCsvLineForTest(int numberOfFields) : base(new string[numberOfFields], new bool[numberOfFields])
		{
		}

		public void SetFieldValue(int position, ZString value)
		{
			FieldValues[position] = value;
		}

		public void SetFieldValue(int position, ZDateTime value)
		{
			FieldValues[position] = value.ToString(JXCConstants.DateFormat);
		}

		public ZString ConvertToJXCLine()
		{
			return string.Join(JXCConstants.Delimiter.ToString(), FieldValues);
		}
	}
}
