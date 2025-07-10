using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public class ReportDataSourceNonPersistentBusinessObject : AutoReportDataSourceNonPersistentBusinessObject
	{
		public ReportDataSourceNonPersistentBusinessObject()
		{
			collection = new ReportDataSourceNonPersistentBusinessObjectCollection(Factory);
		}

		public ReportDataSourceNonPersistentBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			collection = new ReportDataSourceNonPersistentBusinessObjectCollection(Factory);
		}

		public ReportDataSourceNonPersistentBusinessObjectCollection Collection
			=> collection;

		readonly ReportDataSourceNonPersistentBusinessObjectCollection collection;
	}
}
