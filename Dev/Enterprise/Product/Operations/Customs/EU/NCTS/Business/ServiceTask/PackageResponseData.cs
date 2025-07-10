using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class PackageResponseData
	{
		public ZString MarksAndNumbers { get; set; }
		public ZString PackageType { get; set; }
		public ZString NumberOfPackages { get; set; }
		public ZString NumberOfPieces { get; set; }
	}
}
