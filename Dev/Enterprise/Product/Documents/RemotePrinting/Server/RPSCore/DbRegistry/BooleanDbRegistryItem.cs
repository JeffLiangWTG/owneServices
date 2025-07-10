using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(RegistryDataTypes.Codes))]

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public abstract class BooleanDbRegistryItem : BaseDbRegistryItem<bool>
	{
		protected override bool DefaultValue => false;

		protected override string TypeCode => RegistryDataTypes.Codes.Bool;

		protected override bool GetValueFromBytes(byte[] binaryValue)
		{
			bool result = false;

			string strToCompare = Encoding.Unicode.GetString(binaryValue).ToUpperInvariant().Trim();
			if (strToCompare == bool.TrueString.ToUpperInvariant() ||
				strToCompare == "Y" ||
				strToCompare == "1" ||
				strToCompare == "TRUE" ||
				strToCompare == "YES" ||
				strToCompare == "HAI" ||
				strToCompare == "YA")
			{
				result = true;
			}

			return result;
		}

		protected override byte[] GetBytesFromValue(bool value)
		{
			return Encoding.Unicode.GetBytes(value ? bool.TrueString : bool.FalseString);
		}
	}
}
