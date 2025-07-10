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
	public class JobComInvoiceLineJobDocAddressValidation : JobDocAddressValidation
	{
		public JobComInvoiceLineJobDocAddressValidation(JobDocAddress address, JobComInvoiceLine invoiceLine)
			: base(address)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		bool IsManufacturerAddressEditable => invoiceLine.IsImportLicense && Parent.DocAddressType == DocAddressType.Manufacturer && !invoiceLine.ManufacturerAddress_ReadOnly;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (IsManufacturerAddressEditable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, Res.GetString("335646ED-98D4-4ED5-B391-A14C4244AFF4", "Manufacturer"));
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (IsManufacturerAddressEditable && Parent.E2_OA_Address.IsValid)
			{
				if (invoiceLine.InvoiceHeader?.SupplierDocAddressPK == invoiceLine.ManufacturerDocAddressPK)
				{
					Parent.E2_OA_AddressInfo.AddMessageError(Res.GetString("211CBD10-195A-4E7F-B95C-378328F0D913", "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"{0} - {1}\"", ManufacturerIndicatorList.Codes._2, ManufacturerIndicatorList.Descriptions._2));
				}

				AddressValidationHelper.CheckAddressStatusAndStreetNumber(Parent.E2_OA_AddressInfo, Parent.Address);
			}
		}
	}
}
