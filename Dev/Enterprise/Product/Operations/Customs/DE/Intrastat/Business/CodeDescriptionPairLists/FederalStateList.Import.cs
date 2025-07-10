namespace Enterprise.Customs.DE.Intrastat.Business
{
	partial class FederalStateList
	{
		public static FederalStateList Import => GetImportFederalStateList();

		static FederalStateList GetImportFederalStateList()
		{
			var list = new FederalStateList();
			list.RemoveCode(FederalStateList.Codes.ForeignCountry);
			return list;
		}
	}
}
