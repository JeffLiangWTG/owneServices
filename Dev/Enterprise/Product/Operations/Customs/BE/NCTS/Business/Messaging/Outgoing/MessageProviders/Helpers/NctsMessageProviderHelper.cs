using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public static class NctsMessageProviderHelper
	{
		public static int? ConvertStringToNullableInt(ZString stringValue)
		{
			int? result = null;
			if (!stringValue.IsEmpty && int.TryParse(stringValue, out var value))
			{
				result = value;
			}
			return result;
		}
	}
}
