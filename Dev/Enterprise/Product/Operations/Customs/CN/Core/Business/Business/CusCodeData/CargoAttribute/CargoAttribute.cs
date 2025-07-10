using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CargoAttribute : CusCodeData
	{
		public CargoAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override CusCodeDataValidation GetNewValidation() => new CargoAttributeValidation(this);

		public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine), typeof(CusClassPartPivot));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
		}

		public bool CanLinkToAttachment => CargoAttributeList.CanLinkToAttachment(CY_Code);

		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldCanLinkToAttachment = InvoiceLine?.CanLinkToAttachment;
				base.CY_Code = value;
				var newCanLinkToAttachment = InvoiceLine?.CanLinkToAttachment;

				if (!IsCopying && oldCanLinkToAttachment != newCanLinkToAttachment)
				{
					ReloadAttachmentLinksOnInvoiceLine(InvoiceLine);
				}
			}
		}

		public void ReloadAttachmentLinksOnInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				if (!invoiceLine.CanLinkToAttachment)
				{
					invoiceLine.EntryInstruction?.CusStorageDocPivots.UnlinkInvoiceLine(invoiceLine);
				}
				invoiceLine.ReloadAttachmentLinksIfLoaded();
			}
		}
	}
}
