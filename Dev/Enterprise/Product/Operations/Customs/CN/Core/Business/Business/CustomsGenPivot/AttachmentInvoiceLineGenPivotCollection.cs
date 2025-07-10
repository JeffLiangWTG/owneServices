using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AttachmentInvoiceLineGenPivotCollection : CustomsGenPivotCollection<AttachmentInvoiceLineGenPivot, CusStorageDocPivot, JobComInvoiceLine>
	{
		public AttachmentInvoiceLineGenPivotCollection(CusStorageDocPivot attachment) : base(attachment)
		{
		}

		protected override string RelationType => GenPivotTypeDecider.Types.AttachmentInvoiceLineLink;
	}
}
