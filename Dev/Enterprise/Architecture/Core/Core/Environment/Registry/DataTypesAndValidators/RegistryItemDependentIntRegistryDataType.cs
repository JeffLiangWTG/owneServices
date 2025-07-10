using System;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryItemDependentIntRegistryDataType : RegistryItemDependentNumericRegistryDataType<int>
	{
		public RegistryItemDependentIntRegistryDataType(IRegistryItem lowerBoundRegistryItem, IRegistryItem upperBoundRegistryItem)
			: this(lowerBoundRegistryItem, upperBoundRegistryItem, true, true)
		{
		}

		public RegistryItemDependentIntRegistryDataType(IRegistryItem lowerBoundRegistryItem, IRegistryItem upperBoundRegistryItem, bool lowerBoundInclusive, bool upperBoundInclusive)
			: base(RegistryDataTypes.Codes.Int, lowerBoundRegistryItem, upperBoundRegistryItem, lowerBoundInclusive, upperBoundInclusive, 0)
		{
		}

		protected override byte[] SerialiseCore(int value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}

		protected override int DeserialiseCore(byte[] value)
		{
			string str = Encoding.Unicode.GetString(value);
			return Convert.ToInt32(str);
		}

		public override bool IsDefaultValueImmutable => true;
	}
}
