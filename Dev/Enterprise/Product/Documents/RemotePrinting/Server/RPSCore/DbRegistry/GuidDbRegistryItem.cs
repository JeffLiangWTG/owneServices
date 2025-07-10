using System;
using System.Text;

using CargoWise.Data;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
#if DEBUG
	public
#endif
	abstract class GuidDbRegistryItem : BaseDbRegistryItem<Guid>
	{
		protected override string TypeCode
		{
			get { return "GID"; }
		}

		protected override Guid GetValueFromBytes(byte[] binaryValue)
		{
			return new Guid((Encoding.Unicode.GetString(binaryValue)));
		}

		protected override byte[] GetBytesFromValue(Guid value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}
	}
}
