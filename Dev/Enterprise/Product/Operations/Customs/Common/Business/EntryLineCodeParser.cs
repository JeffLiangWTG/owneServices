using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class EntryLineCodeParser
	{
		public EntryLineCodeParser(ZString code)
		{
			if (!code.IsEmpty)
			{
				int separatorIndex = code.LastIndexOf("-");
				if (separatorIndex > 0)
				{
					EntryNumber = code.SubstringSafe(0, separatorIndex);
					LineNumberString = code.SubstringSafe(separatorIndex + 1);
					if (ZShort.TryParse(LineNumberString, out LineNumber))
					{
						IsCompleteCode = true;
					}
				}
				else
				{
					EntryNumber = code;
				}
			}
			IsEmptyCode = code.IsEmpty;
		}

		public readonly bool IsEmptyCode;
		public readonly bool IsCompleteCode;

		public readonly ZString EntryNumber;
		public readonly ZString LineNumberString;
		public readonly ZShort LineNumber;
	}
}
