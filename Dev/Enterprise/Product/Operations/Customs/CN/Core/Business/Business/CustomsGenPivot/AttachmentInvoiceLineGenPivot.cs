using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AttachmentInvoiceLineGenPivot : Customs.Business.CustomsGenPivot, Integration.Customs.CN.IAttachmentInvoiceLineGenPivot
	{
		public AttachmentInvoiceLineGenPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = GenPivotTypeDecider.Types.AttachmentInvoiceLineLink;
		}
	}
}
