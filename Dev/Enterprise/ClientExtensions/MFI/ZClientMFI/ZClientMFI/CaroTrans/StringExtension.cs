using CargoWise.Types;

namespace Enterprise.Client.MFI.CaroTrans
{
	public static class ZStringExtension
	{
		public static int GetStrLengthSafe(this ZString str, int fieldLength)
		{
			return  (str.Length > fieldLength) ?
				fieldLength - (char.IsLowSurrogate(str[fieldLength]) ? 1 : 0)
				: str.Length;
		}
	}
}
