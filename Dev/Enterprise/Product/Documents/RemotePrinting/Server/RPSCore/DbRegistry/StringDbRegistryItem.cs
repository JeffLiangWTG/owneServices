using System.Text;
using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public abstract class StringDbRegistryItem : BaseDbRegistryItem<string>
	{
		protected override string TypeCode
		{
			get { return "STR"; }
		}

		protected override string GetValueFromBytes(byte[] binaryValue)
		{
			return Encoding.Unicode.GetString(binaryValue);
		}

		protected override byte[] GetBytesFromValue(string value)
		{
			return Encoding.Unicode.GetBytes(value ?? string.Empty);
		}
	}
}
