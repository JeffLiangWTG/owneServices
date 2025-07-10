namespace Enterprise.Customs.KR.Business
{
	public class Import5GWCreator : ExtendedOfficeHoursCreator<Import5GWHeader, Import5GWEntry>
	{
		protected override void PopulateMoreEntryFields(Import5GWEntry entryData, ExtendedHoursRequestLine line)
		{
			base.PopulateMoreEntryFields(entryData, line);
			entryData.ReferenceNumberType = line.CustomsEntryType;
			entryData.HSDescription = line.HSDescription;
			entryData.BondedAreaCode = line.BondedAreaCode;
			entryData.PayerCompanyName = line.PayerCompanyName;
		}
	}
}
