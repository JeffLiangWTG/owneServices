using System;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(RegistryDataTypes.Codes))]

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public abstract class IntDbRegistryItem : BaseDbRegistryItem<int>
	{
		protected override int DefaultValue => 0;

		protected override string TypeCode => RegistryDataTypes.Codes.Int;

		protected override int GetValueFromBytes(byte[] binaryValue)
		{
			return Convert.ToInt32(Encoding.Unicode.GetString(binaryValue));
		}

		protected override byte[] GetBytesFromValue(int value)
		{
			return Encoding.Unicode.GetBytes(value.ToString());
		}
	}
}
