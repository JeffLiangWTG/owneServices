using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public class ReportDataSourceNonPersistentBusinessObjectCollection : NonPersistentBusinessObjectCollection<ReportDataSourceNonPersistentBusinessObject>
	{
		public ReportDataSourceNonPersistentBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new ReportDataSourceNonPersistentBusinessObject(Factory);
	}
}
