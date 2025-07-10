using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObjectValidation : ZValidation
	{
		public ImportLicenseLoadingObjectValidation(ImportLicenseLoadingObject parent)
			: base(parent)
		{
			Parent = parent;
		}

		public ImportLicenseLoadingObject Parent;

		public override Type AutoValidationType
		{
			get { return typeof(ImportLicenseLoadingObjectValidation); }
		}

		public override void ValidateAll()
		{
			ValidateInvoiceHeaderPK();
		}

		public void ValidateInvoiceHeaderPK()
		{
			ValidateCalculatedProperty(Parent.InvoiceHeaderPKInfo);
		}

		protected void CheckInvoiceHeaderPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.InvoiceHeaderPKInfo);
		}
	}
}
