using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public static class ZStringExtension
	{
		public static string GetNullIfEmpty(this ZString value)
		{
			return value.IsEmpty ? null : (string)value;
		}
	}
}
