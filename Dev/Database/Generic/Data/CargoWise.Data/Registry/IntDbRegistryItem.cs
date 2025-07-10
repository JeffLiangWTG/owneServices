using System;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[Immutable]
	public class IntDbRegistryItem : BaseDbRegistryItem<int>
	{
		public IntDbRegistryItem(string name, int defaultValue, bool preserveTestValue = false)
			 : base(preserveTestValue)
		{
			ItemName = name ?? throw new ArgumentNullException(nameof(name));
			DefaultValue = defaultValue;
		}

		public override string ItemName { get; }

		protected override int DefaultValue { get; }

		protected override string TypeCode => "INT";

		protected override int GetValueFromBytes(byte[] binaryValue)
		{
			int result = Convert.ToInt32(Encoding.Unicode.GetString(binaryValue));
			return result;
		}

		protected override byte[] GetBytesFromValue(int value)
		{
			byte[] result = Encoding.Unicode.GetBytes(value.ToString());
			return result;
		}
	}
}
