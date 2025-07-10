using System;
using System.Text;

namespace Enterprise.ZArchitecture.Environment
{
	public class IntRegistryDataType : NumericRegistryDataType<int>
	{
		public IntRegistryDataType()
			: base(RegistryDataTypes.Codes.Int, 0)
		{
		}

		public IntRegistryDataType(int lowerBound, int upperBound)
			: base(RegistryDataTypes.Codes.Int, 0, lowerBound, upperBound)
		{
		}

		protected override byte[] SerialiseCore(int value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}

		protected override int DeserialiseCore(byte[] value)
		{
			return Convert.ToInt32(Encoding.Unicode.GetString(value));
		}

		public override bool IsDefaultValueImmutable => true;
	}
}
