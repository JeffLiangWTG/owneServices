using CargoWise.Types;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	static class TestHelper
	{
		public static bool StringEquals(this ZString? expected, string actual)
		{
			return expected == new ZString?(actual);
		}
	}
}
