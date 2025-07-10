using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsTadEdocSaverOptions
	{
		public NctsTadEdocSaverOptions(string language = null)
		{
			Language = language ?? ZString.Empty;
		}

		public ZString Language { get; }
	}
}
