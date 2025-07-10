
using System.Text;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class BooleanRegistryDataType : RegistryDataType<bool>
	{
		public BooleanRegistryDataType()
			: base(RegistryDataTypes.Codes.Bool, false)
		{
		}

		public override bool IsDefaultValueImmutable => true;

		protected override bool ValuesAreEqualCore(bool a, bool b)
		{
			return a == b;
		}

		protected override byte[] SerialiseCore(bool value)
		{
			return Encoding.Unicode.GetBytes(value ? bool.TrueString : bool.FalseString);
		}

		protected override bool DeserialiseCore(byte[] value)
		{
			bool result = false;

			string strToCompare = Encoding.Unicode.GetString(value).ToUpperInvariant().Trim();
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

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new BooleanRegistryEditorInfo();
		}
	}
}
