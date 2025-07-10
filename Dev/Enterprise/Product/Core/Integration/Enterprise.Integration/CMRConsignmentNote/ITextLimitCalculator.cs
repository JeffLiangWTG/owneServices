using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ITextLimitCalculator
	{
		ZString CalculateLimits(ZString text, int maxLines, int maxLineLength);
	}
}
