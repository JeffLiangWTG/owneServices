using System;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[Immutable]
	public sealed class StringDbRegistryItem : BaseDbRegistryItem<string>
	{
		public StringDbRegistryItem(string name, bool preserveTestValue = false)
			: this(name, defaultValue: null, preserveTestValue)
		{
		}

		public StringDbRegistryItem(string name, string defaultValue, bool preserveTestValue = false)
			 : base(preserveTestValue)
		{
			ItemName = name ?? throw new ArgumentNullException(nameof(name));
			DefaultValue = defaultValue;
		}

		public override string ItemName { get; }

		protected override string DefaultValue { get; }

		protected override string TypeCode
		{
			get { return "STR"; }
		}

		protected override string GetValueFromBytes(byte[] binaryValue)
		{
			string result = Encoding.Unicode.GetString(binaryValue).Trim();
			return result;
		}

		protected override byte[] GetBytesFromValue(string value)
		{
			byte[] result = Encoding.Unicode.GetBytes(value);
			return result;
		}
	}
}
