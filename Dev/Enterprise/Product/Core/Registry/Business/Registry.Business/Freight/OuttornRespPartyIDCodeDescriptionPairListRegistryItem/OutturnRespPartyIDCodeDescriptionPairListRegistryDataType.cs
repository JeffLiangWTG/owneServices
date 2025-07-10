using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OutturnRespPartyIDCodeDescriptionPairListRegistryDataType : CodeDescriptionPairListRegistryDataType
	{
		public OutturnRespPartyIDCodeDescriptionPairListRegistryDataType(int codeMaxLength)
			: base(codeMaxLength)
		{
			base.AllowDuplicateCodes = false;
			base.AllowEmptyCodes = false;
		}

		protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			foreach (CodeDescriptionPair pair in proposedValue)
			{
				ValidateDescription(pair.Description);
				ValidateCode(pair.Code);
			}
		}

		#region Validate Code

		void ValidateCode(ZString code)
		{
			if (!code.IsEmpty)
			{
				if (CodeIsNNNNA(code) || CodeIsANNNA(code) || CodeIsAANNA(code))
				{
					char checkSum;
					if ((CodeIsNNNNA(code) && !EstablishmentCodeValidNNNNA(code, out checkSum)) ||
						(CodeIsANNNA(code) && !EstablishmentCodeValidANNNA(code, out checkSum)) ||
						(CodeIsAANNA(code) && !EstablishmentCodeValidAANNA(code, out checkSum)))
					{
						throw new RegistryValidationException(Res.GetString("38a4464a-394f-4a90-9915-fe74e8f1ceae", "Invalid Establishment Code, expected checksum is: {0}", checkSum));
					}
				}
				else
				{
					throw new RegistryValidationException(Res.GetString("0ce92ba0-0fe4-431b-a754-d0517055486b", "Invalid establishment code format. The format could be any of NNNNA, ANNNA or AANNA ('A' means alphabetical and 'N' indicates numeric)."));
				}
			}
		}

		protected bool CodeIsNNNNA(ZString code)
		{
			bool result = false;
			if (code.Length == 5)
			{
				result = (Char.IsDigit(code[0]) && Char.IsDigit(code[1]) && Char.IsDigit(code[2]) && Char.IsDigit(code[3]) && Char.IsLetter(code[4]));
			}
			return result;
		}

		protected bool CodeIsANNNA(ZString code)
		{
			bool result = false;
			if (code.Length == 5)
			{
				result = (Char.IsLetter(code[0]) && Char.IsDigit(code[1]) && Char.IsDigit(code[2]) && Char.IsDigit(code[3]) && Char.IsLetter(code[4]));
			}
			return result;
		}

		protected bool CodeIsAANNA(ZString code)
		{
			bool result = false;
			if (code.Length == 5)
			{
				result = (Char.IsLetter(code[0]) && Char.IsLetter(code[1]) && Char.IsDigit(code[2]) && Char.IsDigit(code[3]) && Char.IsLetter(code[4]));
			}
			return result;
		}

		protected bool EstablishmentCodeValidNNNNA(ZString code, out char correctCheckSum)
		{
			int value1 = int.Parse(code[0].ToString());
			int value2 = int.Parse(code[1].ToString());
			int value3 = int.Parse(code[2].ToString());
			int value4 = int.Parse(code[3].ToString());
			correctCheckSum = GetCheckSum(value1, value2, value3, value4);
			return correctCheckSum == code[4];
		}

		protected bool EstablishmentCodeValidANNNA(ZString code, out char correctCheckSum)
		{
			int value1 = GetANNNAValueForChar(code[0]);
			int value2 = int.Parse(code[1].ToString());
			int value3 = int.Parse(code[2].ToString());
			int value4 = int.Parse(code[3].ToString());
			correctCheckSum = GetCheckSum(value1, value2, value3, value4);
			return correctCheckSum == code[4];
		}

		protected bool EstablishmentCodeValidAANNA(ZString code, out char correctCheckSum)
		{
			int value1 = GetAANNAValueForChar(code[0]);
			int value2 = GetAANNAValueForChar(code[1]);
			int value3 = int.Parse(code[2].ToString());
			int value4 = int.Parse(code[3].ToString());
			correctCheckSum = GetCheckSum(value1, value2, value3, value4);
			return correctCheckSum == code[4];
		}

		protected char GetCheckSum(int value1, int value2, int value3, int value4)
		{
			int weightedValue = ((value1 * 10) + (value2 * 9) + (value3 * 8) + (value4 * 7));
			weightedValue %= 11;
			weightedValue++;
			return GetChecksumChar(weightedValue);
		}

		protected int GetANNNAValueForChar(char c)
		{
			switch (c)
			{
				case 'A':
					return 11;
				case 'B':
					return 12;
				case 'C':
					return 13;
				case 'D':
					return 14;
				case 'E':
					return 15;
				case 'F':
					return 16;
				case 'G':
					return 17;
				case 'H':
					return 18;
				case 'I':
					return 19;
				case 'J':
					return 20;
				case 'K':
					return 21;
				case 'L':
					return 22;
				case 'M':
					return 23;
				case 'N':
					return 24;
				case 'O':
					return 25;
				case 'P':
					return 26;
				case 'Q':
					return 27;
				case 'R':
					return 28;
				case 'S':
					return 29;
				case 'T':
					return 30;
				case 'U':
					return 31;
				case 'V':
					return 32;
				case 'W':
					return 33;
				case 'X':
					return 34;
				case 'Y':
					return 35;
				case 'Z':
					return 36;
			}
			return -1;
		}

		protected int GetAANNAValueForChar(char c)
		{
			switch (c)
			{
				case 'A':
					return 1;
				case 'B':
					return 2;
				case 'C':
					return 3;
				case 'D':
					return 4;
				case 'E':
					return 5;
				case 'F':
					return 6;
				case 'G':
					return 7;
				case 'H':
					return 8;
				case 'I':
					return 9;
				case 'J':
					return 0;
				case 'K':
					return 1;
				case 'L':
					return 2;
				case 'M':
					return 3;
				case 'N':
					return 4;
				case 'O':
					return 5;
				case 'P':
					return 6;
				case 'Q':
					return 7;
				case 'R':
					return 8;
				case 'S':
					return 9;
				case 'T':
					return 0;
				case 'U':
					return 1;
				case 'V':
					return 2;
				case 'W':
					return 3;
				case 'X':
					return 4;
				case 'Y':
					return 5;
				case 'Z':
					return 6;
			}
			return -1;
		}

		protected char GetChecksumChar(int value)
		{
			switch (value)
			{
				case 1:
					return 'A';
				case 2:
					return 'B';
				case 3:
					return 'C';
				case 4:
					return 'D';
				case 5:
					return 'E';
				case 6:
					return 'H';
				case 7:
					return 'J';
				case 8:
					return 'K';
				case 9:
					return 'M';
				case 10:
					return 'N';
				case 11:
					return 'P';
			}
			return 'Z';
		}

		#endregion

		#region Validate Description

		void ValidateDescription(ZString description)
		{
			if (!description.IsEmpty)
			{
				if (!CheckValidABN(description))
				{
					throw new RegistryValidationException(Res.GetString("e85eed8d-c3aa-4dbf-99d8-024f2ba85083", "The entered ABN is not valid.\r\nAn ABN must be 11 or 14 digits with a valid check-digit."));
				}
			}
			else
			{
				throw new RegistryValidationException(Res.GetString("328ab558-1ca7-4802-8fa9-4e62d6c6f6d1", "ABN cannot be empty."));
			}
		}

#if DEBUG
		protected virtual
#endif
		bool CheckValidABN(ZString aBNRaw)
		{
			ZString aBN = aBNRaw.KeepChars("0123456789");

			if (aBN.Length != 11 && aBN.Length != 14)
			{
				return false;
			}
			else
			{
				return ((int.Parse(aBN[0].ToString()) - 1) * 10 +
					int.Parse(aBN[1].ToString()) +
					int.Parse(aBN[2].ToString()) * 3 +
					int.Parse(aBN[3].ToString()) * 5 +
					int.Parse(aBN[4].ToString()) * 7 +
					int.Parse(aBN[5].ToString()) * 9 +
					int.Parse(aBN[6].ToString()) * 11 +
					int.Parse(aBN[7].ToString()) * 13 +
					int.Parse(aBN[8].ToString()) * 15 +
					int.Parse(aBN[9].ToString()) * 17 +
					int.Parse(aBN[10].ToString()) * 19) % 89 == 0;
			}
		}

		#endregion
	}
}
