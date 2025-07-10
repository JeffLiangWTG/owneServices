using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentSendingObjectCollection : NonPersistentBusinessObjectCollection<DocumentSendingObject>
	{
		public DocumentSendingObjectCollection(CusExitReport exitReport) : base(exitReport.Factory)
		{
			cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
		}

		readonly CusExitReport cusExitReport;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DocumentSendingObject(cusExitReport);
	}
}
