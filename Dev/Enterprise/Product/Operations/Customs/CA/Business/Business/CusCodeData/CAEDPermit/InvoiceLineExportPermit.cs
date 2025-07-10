using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineExportPermit : CusCodeData
	{
		public InvoiceLineExportPermit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
			set { base.Parent = value; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CusCodeDataLookups(this);
		}

		public new InvoiceLineExportPermitValidation Validation
		{
			get { return (InvoiceLineExportPermitValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new InvoiceLineExportPermitValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.Permit;
			CY_Code = CusCodeDataTypeList.Codes.Permit;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}
	}
}
