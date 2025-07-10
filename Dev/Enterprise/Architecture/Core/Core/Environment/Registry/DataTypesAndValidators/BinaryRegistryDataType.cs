
namespace Enterprise.ZArchitecture.Environment
{
	public class BinaryRegistryDataType : RegistryDataType<byte[]>
	{
		public BinaryRegistryDataType()
			: this(null)
		{
		}

		public override bool IsNullDataRepresentation(object value)
		{
			return value is byte[] && IsNullBytes((byte[])value);
		}

		protected BinaryRegistryDataType(byte[] defaultValue)
			: base(RegistryDataTypes.Codes.Binary, defaultValue)
		{
		}

		protected override bool AllowNullCore
		{
			get { return true; }
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override byte[] SerialiseCore(byte[] value)
		{
			return value ?? MagicNullBytes;
		}

		protected override byte[] DeserialiseCore(byte[] value)
		{
			return IsNullBytes(value) ? null : value;
		}

		protected override byte[] CloneValue(byte[] value)
		{
			return (byte[])value.Clone();
		}

		bool IsNullBytes(byte[] bytes)
		{
			bool result = (bytes.Length == MagicNullBytes.Length);
			if (result)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					result = true;
					if (bytes[i] != MagicNullBytes[i])
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		static readonly byte[] MagicNullBytes = { 34, 104, 23, 82, 126 };
	}
}
