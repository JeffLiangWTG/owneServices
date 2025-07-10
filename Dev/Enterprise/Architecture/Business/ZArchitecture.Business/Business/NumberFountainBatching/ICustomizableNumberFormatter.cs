using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomizableNumberFormatter
	{
		INumberFountainProxy GetFountainProxy();

		string GetFormattedNumber(long seed, INumberFountainProxy fountain);
	}
}
