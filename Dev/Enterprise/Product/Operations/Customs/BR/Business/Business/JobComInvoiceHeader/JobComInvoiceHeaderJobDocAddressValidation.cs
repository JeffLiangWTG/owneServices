using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceHeaderJobDocAddressValidation : JobDocAddressValidation
	{
		public JobComInvoiceHeaderJobDocAddressValidation(JobDocAddress address, JobComInvoiceHeader invoiceHeader)
			: base(address)
		{
			this.invoiceHeader = invoiceHeader;
		}

		readonly JobComInvoiceHeader invoiceHeader;

		bool IsSupplierAddressEditable => invoiceHeader.IsImportLicense && Parent.DocAddressType == DocAddressType.SupplierDocumentaryAddress;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (IsSupplierAddressEditable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, JobComInvoiceHeader.SupplierCaption.Caption);
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (IsSupplierAddressEditable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_OA_AddressInfo, Res.GetString("5273ecd0-ad60-4a70-b227-de06e75ab629", "Supplier Address"));
				AddressValidationHelper.CheckAddressStatusAndStreetNumber(Parent.E2_OA_AddressInfo, Parent.Address);
			}
		}
	}
}
