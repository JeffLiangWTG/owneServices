namespace Enterprise.Customs.DE.Intrastat.Business
{
	partial class FederalStateList
	{
		public static FederalStateList Export => GetExportFederalStateList();

		static FederalStateList GetExportFederalStateList()
		{
			var list = new FederalStateList();
			list.RemoveCode(FederalStateList.Codes.Ursprungsausland);
			return list;
		}
	}
}
