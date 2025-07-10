using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	static class ZBoolExtensions
	{
		public static string ToYN(this ZBool input)
		{
			return input ? "Y" : "N";
		}
	}
}
