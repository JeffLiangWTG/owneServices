using System;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[Immutable]
	public sealed class BoolDbRegistryItem : BaseDbRegistryItem<bool>
	{
		public BoolDbRegistryItem(string name, bool defaultValue, bool preserveTestValue = false)
			 : base(preserveTestValue)
		{
			ItemName = name ?? throw new ArgumentNullException(nameof(name));
			DefaultValue = defaultValue;
		}

		public override string ItemName { get; }

		protected override bool DefaultValue { get; }

		protected override string TypeCode
		{
			get { return "BOL"; }
		}

		protected override bool GetValueFromBytes(byte[] binaryValue)
		{
			string strToCompare = Encoding.Unicode.GetString(binaryValue).Trim();
			bool result = (string.Compare(strToCompare, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0);
			return result;
		}

		protected override byte[] GetBytesFromValue(bool value)
		{
			byte[] result = Encoding.Unicode.GetBytes(value ? bool.TrueString : bool.FalseString);
			return result;
		}
	}
}
