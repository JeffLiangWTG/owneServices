using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPESGLocoMapSystemUsageList : SGLocoMapSystemUsageList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Codes : LocoMapSystemUsageList.Codes
		{
			public const string Ups = UPEOtherLocoMapSystemUsageList.Codes.Ups;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Descriptions : LocoMapSystemUsageList.Descriptions
		{
			public const string Ups = UPEOtherLocoMapSystemUsageList.Descriptions.Ups;
		}

		public UPESGLocoMapSystemUsageList() : base()
		{
			AddPair(Codes.Ups, Descriptions.Ups);
		}
	}
}
