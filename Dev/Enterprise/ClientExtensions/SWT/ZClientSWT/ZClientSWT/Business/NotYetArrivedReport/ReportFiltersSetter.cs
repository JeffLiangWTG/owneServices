using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.SWT
{
	class ReportFiltersSetter
	{
		public ReportFiltersSetter(ZString keyType, OrgHeader org)
		{
			KeyType = keyType;
			Org = org;
		}

		public void UpdateReportFilters(CollectionOfIFilter filters)
		{
			foreach (FilterField filter in filters)
			{
				if (filter.DisplayName == "Importer" && KeyType == "Importer")
				{
					((LookupField)filter).Value = Org.PK.ToGuid();
				}
				else if (filter.DisplayName == "Supplier" && KeyType == "Supplier")
				{
					((LookupField)filter).Value = Org.PK.ToGuid();
				}
			}
		}

		readonly string KeyType;
		readonly OrgHeader Org;
	}
}
