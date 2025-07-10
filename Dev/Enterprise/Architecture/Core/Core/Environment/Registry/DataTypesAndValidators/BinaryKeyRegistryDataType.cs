using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class BinaryKeyRegistryDataType : StringRegistryDataType
	{
		public BinaryKeyRegistryDataType(int keySize)
			: base()
		{
			KeySize = keySize;
		}

		public int KeySize { get; private set; }

		protected override string DeserialiseCore(byte[] value)
		{
			return ByteArrayToHexString(value);
		}

		protected override byte[] SerialiseCore(string value)
		{
			return HexStringToByteArray(value);
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			ValidateHexString(proposedValue);

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System configuration registry item - no translation required")]
		void ValidateHexString(string value, bool validateLength = true)
		{
			var regex = new Regex("^[0-9A-F]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (!regex.IsMatch(value))
			{
				throw new RegistryValidationException("Input must be valid hexadecimal characters only (0-9, A-F)");
			}

			if (validateLength && value.Length != (KeySize * 2))
			{
				throw new RegistryValidationException(string.Format("Key must be {0} bytes ({1} hexadecimal characters), but was {2} hexadecimal characters.", KeySize, KeySize * 2, value.Length));
			}
		}

		protected byte[] HexStringToByteArray(string value)
		{
			ValidateHexString(value, validateLength: !ValidationSuspended);

			var numChars = value.Length;
			var bytes = new byte[numChars / 2];

			for (int i = 0; i < numChars; i += 2)
			{
				bytes[i / 2] = System.Convert.ToByte(value.Substring(i, 2), 16);
			}

			return bytes;
		}

		protected string ByteArrayToHexString(byte[] value)
		{
			return BitConverter.ToString(value)
				.Replace("-", string.Empty);
		}
	}
}
