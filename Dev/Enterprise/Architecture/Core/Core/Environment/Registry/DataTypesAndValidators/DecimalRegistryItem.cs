using System;
using System.Globalization;
using System.Text;

namespace Enterprise.ZArchitecture.Environment
{
	public class DecimalRegistryDataType : NumericRegistryDataType<decimal>
	{
		public DecimalRegistryDataType()
			: base(RegistryDataTypes.Codes.Decimal, 0m)
		{
		}

		public DecimalRegistryDataType(double lowerBound, double upperBound)
			: base(RegistryDataTypes.Codes.Decimal, 0m, lowerBound, upperBound)
		{
		}

		public override bool IsDefaultValueImmutable => true;

		protected override bool ValuesAreEqualCore(decimal a, decimal b)
		{
			return a == b;
		}

		protected override byte[] SerialiseCore(decimal value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}

		protected override decimal DeserialiseCore(byte[] value)
		{
			string asString = Encoding.Unicode.GetString(value);
			return Convert.ToDecimal(asString, CultureInfo.InvariantCulture);
		}
	}
}
