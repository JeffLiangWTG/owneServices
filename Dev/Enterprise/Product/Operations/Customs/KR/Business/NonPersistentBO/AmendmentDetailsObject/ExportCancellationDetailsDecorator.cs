using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public static class ExportCancellationDetailsDecorator
	{
		public static void Decorate(this ExportCancellationDetails details, CusEntryHeader entry)
		{
			if (entry != null)
			{
				details.DeclarationDate = entry.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				details.ReleaseDate = entry.CH_EntryReleaseDate;
				details.Declarant = new OrganizationDocWrapper(entry.Declaration.BrokerAddress);
				details.Supplier = new OrganizationDocWrapper(entry.Declaration.SupplierAddress);
			}
		}
	}
}
