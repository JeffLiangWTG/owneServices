using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEUSLocoMapSystemUsageList : USLocoMapSystemUsageList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Codes : USLocoMapSystemUsageList.Codes
		{
			public const string Ups = UPEOtherLocoMapSystemUsageList.Codes.Ups;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Descriptions : USLocoMapSystemUsageList.Descriptions
		{
			public const string Ups = UPEOtherLocoMapSystemUsageList.Descriptions.Ups;
		}

		public UPEUSLocoMapSystemUsageList()
			: base()
		{
			AddPair(Codes.Ups, Descriptions.Ups);
		}
	}
}
