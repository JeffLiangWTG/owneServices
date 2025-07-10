using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptReason : CusCodeData
	{
		public ReportOfReceiptReason(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(EMCSInvoiceLineCusOutturn));

		protected override ZString HumanReadableNameCore => Res.GetString("8d379774-ee7c-4e6c-8ff3-2f0dbbc50aff", "Report of Receipt Reason");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ReportOfReceiptReason;
		}

		[ResourceStringData("B2A0EE01-FDB5-454B-90E1-20A299DA9E51", Caption = "Code")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[ResourceStringData("843FB156-6B93-4DE9-970F-531F65761567", Caption = "Explanation")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		#region Validation & Lookups

		protected override CusCodeDataLookups GetNewLookups() => new ReportOfReceiptReasonLookups(this);
		public new ReportOfReceiptReasonLookups Lookups => (ReportOfReceiptReasonLookups)base.Lookups;

		protected override CusCodeDataValidation GetNewValidation() => new ReportOfReceiptReasonValidation(this);
		public new ReportOfReceiptReasonValidation Validation => (ReportOfReceiptReasonValidation)base.Validation;

		#endregion

	}
}
