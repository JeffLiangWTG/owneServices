using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class TextLimitCalculator : ITextLimitCalculator
	{
		public ZString CalculateLimits(ZString text, int maxLines, int lineMaxLength)
		{
			var transferredCharacters = 0;
			var transferredCharactersLine = 0;

			if (!text.IsEmpty)
			{
				for (int i = 0; i < text.Length; i++)
				{
					if (text.Substring(i, 1) == System.Environment.NewLine)
					{
						transferredCharacters += (lineMaxLength - transferredCharactersLine);
					}
					else
					{
						if (lineMaxLength >= transferredCharactersLine)
						{
							transferredCharactersLine++;
						}
						else
						{
							transferredCharactersLine = 1;
						}
						transferredCharacters++;
					}
					if (transferredCharacters >= (maxLines * lineMaxLength))
					{
						break;
					}
				}
			}
			return text.Substring(0, transferredCharacters);
		}
	}
}
