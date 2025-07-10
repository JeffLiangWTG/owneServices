using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ComplementaryLogisticInvoice : CusSupportingInfo
	{
		public ComplementaryLogisticInvoice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(44)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ComplementaryLogisticInvoice|CSI_ReferenceNumber", Caption = "NF-E Key")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(4)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ComplementaryLogisticInvoice|CSI_LineNo", Caption = "NF-E Item Number")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		public ZString SupplierCNPJ
		{
			get
			{
				var result = ZString.Empty;
				var supplier = Parent.InvoiceHeader?.Supplier_Effective;
				if (supplier != null)
				{
					result = supplier.GetCNPJOrCPF();
				}
				return result;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ComplementaryLogisticInvoice;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;
			if (Parent?.IsExport ?? ZBool.False)
			{
				result = new ComplementaryLogisticInvoiceValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
