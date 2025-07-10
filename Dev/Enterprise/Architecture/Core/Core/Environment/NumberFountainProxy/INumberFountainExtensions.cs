using Enterprise.NumberFountain;

namespace Enterprise.ZArchitecture.Environment
{
	public static class INumberFountainExtensions
	{
		public static INumberFountainProxy Wrap(this INumberFountain inner)
		{
			return new NumberFountainProxy(inner);
		}
	}
}
