using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.GB.ICS.Messaging.IE315
{
	internal class PackageWrapper : IPackage
	{
		readonly AsycudaPack pack;

		public PackageWrapper(AsycudaPack pack)
		{
			this.pack = pack;
		}

		ZString IPackage.KindOfPackages => pack.APA_PackUQ;

		ZInt IPackage.NumberOfPackages => pack.APA_PackQty;

		ZInt IPackage.NumberOfPieces => pack.APA_PackQty;

		ZString IPackage.MarksAndNumbersOfPackages => pack.APA_MarksAndNumbers;

		ZString IPackage.MarksAndNumbersOfPackagesLNG => ZString.Empty; // Do not send.
	}
}
