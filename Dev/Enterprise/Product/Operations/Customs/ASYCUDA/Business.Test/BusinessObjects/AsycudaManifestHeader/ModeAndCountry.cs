using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public sealed class ModeAndCountry
	{
		public ModeAndCountry(string mode, string country, string type = "NVC", string versionNo = "17.3.29.1")
		{
			Mode = mode;
			Country = country;
			Type = type;
			VersionNo = versionNo;
		}
		public ZString Mode { get; private set; }
		public ZString Country { get; private set; }
		public ZString Type { get; private set; }
		public ZString VersionNo { get; private set; }
	}
}
