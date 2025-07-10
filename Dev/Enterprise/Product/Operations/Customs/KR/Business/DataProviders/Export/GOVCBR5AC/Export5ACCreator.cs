namespace Enterprise.Customs.KR.Business
{
	public class Export5ACCreator : ExtendedOfficeHoursCreator<Export5ACHeader, Export5ACEntry>
	{
		protected override void PopulateMoreEntryFields(Export5ACEntry entryData, ExtendedHoursRequestLine line)
		{
			base.PopulateMoreEntryFields(entryData, line);
			entryData.SupplierName = line.SupplierName;
		}
	}
}
