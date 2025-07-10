namespace Enterprise.Customs.EU.EMCS.Business
{
	public class PackageProviderHelper
	{
		public PackageProviderHelper(EMCSJobComInvoiceLine emcsInvoiceLine, EMCSPackage emcsPackage)
		{
			this.emcsInvoiceLine = emcsInvoiceLine;
			this.emcsPackage = emcsPackage;
		}
		readonly EMCSJobComInvoiceLine emcsInvoiceLine;
		readonly EMCSPackage emcsPackage;

		public string KindOfPackages => emcsPackage.B5_UnitType;

		public string SealNumber => emcsPackage.B5_SealNumber;

		public long? NumberOfPackages => emcsInvoiceLine.ZG_IsMainPack ? (emcsPackage.IsCountable() ? emcsPackage.B5_UnitCount : null) : 0;

		public string ShippingMarks => emcsPackage.IsCountable() ? emcsPackage.B5_MarksAndNumbers : null;
	}
}
