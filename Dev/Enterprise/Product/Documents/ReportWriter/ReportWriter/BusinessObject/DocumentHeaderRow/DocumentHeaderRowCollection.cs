using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class DocumentHeaderRowCollection : NonPersistentBusinessObjectCollection<DocumentHeaderRow>
	{
		public DocumentHeaderRowCollection(ReportBizObj parent)
			: base(parent.Factory)
		{
			this.parent = parent;
		}

		public readonly ReportBizObj parent;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentHeaderRow(parent);
		}

		#endregion
	}
}
