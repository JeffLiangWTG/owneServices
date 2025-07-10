
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;

namespace Enterprise.Client.UPE.DocumentImaging
{
	static class DocManagerReferenceTypes
	{
		public const string All = "ALL";

		public static ZString AirCargo
		{
			get { return new AUAirCargoHouseData().ReferenceType; }
		}

		public static ZString Declaration
		{
			get { return new JobDeclarationData().ReferenceType; }
		}
	}
}
