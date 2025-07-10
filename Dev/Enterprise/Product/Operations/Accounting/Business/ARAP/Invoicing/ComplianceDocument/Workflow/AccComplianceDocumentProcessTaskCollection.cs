using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentProcessTaskCollection : ProcessTaskCollection
	{
		public AccComplianceDocumentProcessTaskCollection(AccComplianceDocumentHeader header) : base(header) { }

		public new AccComplianceDocumentProcessTask this[int index]
		{
			get { return (AccComplianceDocumentProcessTask)Elements[index]; }
		}

		public new AccComplianceDocumentProcessTask AddNew()
		{
			return (AccComplianceDocumentProcessTask)base.AddNew();
		}
	}
}