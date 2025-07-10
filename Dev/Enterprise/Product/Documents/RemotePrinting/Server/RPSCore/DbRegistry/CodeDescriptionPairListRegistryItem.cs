using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public abstract class CodeDescriptionPairListRegistryItem : BaseDbRegistryItem<ICodeDescriptionPairList>
	{
		protected override ICodeDescriptionPairList DefaultValue => new CodeDescriptionPairList();

		protected override string TypeCode => RegistryDataTypes.Codes.Binary;

		protected override ICodeDescriptionPairList GetValueFromBytes(byte[] binaryValue)
		{
			return new ReadOnlyCodeDescriptionPairList(binaryValue);
		}

		protected override byte[] GetBytesFromValue(ICodeDescriptionPairList value)
		{
			var list = (value as ReadOnlyCodeDescriptionPairList) ?? new ReadOnlyCodeDescriptionPairList(value);
			return list.ToXMLByteArray();
		}
	}
}
