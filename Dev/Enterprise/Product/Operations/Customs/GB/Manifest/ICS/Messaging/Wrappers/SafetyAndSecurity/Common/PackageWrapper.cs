using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class PackageWrapper : IPackage
	{
		public PackageWrapper(AsycudaPack pack)
		{
			this.pack = pack;
		}
		readonly AsycudaPack pack;

		public string KindOfPackages => pack.APA_PackUQ;

		public string MarksAndNumbersOfPackages => pack.APA_MarksAndNumbers;

		public string MarksAndNumbersOfPackagesLNG => string.Empty; // Do not send.

		public ZBool IsBulk => pack.IsBulk;

		public ZBool IsUnpacked => pack.IsUnpacked;

		public string NumberOfPackages => CachedValueHelper.GetValue(ref numberOfPackages, () => !IsBulk && !IsUnpacked ? pack.APA_PackQty.ToString() : string.Empty);
		CachedValue<string> numberOfPackages;

		public string NumberOfPieces => CachedValueHelper.GetValue(ref numberOfPieces, () => !IsBulk && IsUnpacked ? pack.APA_PackQty.ToString() : string.Empty);
		CachedValue<string> numberOfPieces;

		public int NumberOfPackagesInt => int.TryParse(NumberOfPackages, out int result) ? result : 0;

		public int NumberOfPiecesInt => int.TryParse(NumberOfPieces, out int result) ? result : 0;
	}
}
