using System;
using System.Text;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[Immutable]
	public class DatabaseVersionRegistryItem : IntDbRegistryItem
	{
		public DatabaseVersionRegistryItem(string itemName, bool preserveForTest = false)
			: base(itemName, defaultValue: 0, preserveForTest)
		{
		}

		protected override int GetValueFromBytes(byte[] binaryValue)
		{
			try
			{
				return base.GetValueFromBytes(binaryValue);
			}
			catch (FormatException ex)
			{
				var valueAsString = binaryValue == null ? "<null>" : Encoding.Unicode.GetString(binaryValue);
				var message = FormattableString.Invariant($"Format exception converting '{valueAsString}' to integer when loading registry item {ItemName}.");

				ErrorReporter.ReportOnce(message, ex);
				throw new FormatException(message, ex);
			}
		}
	}
}
