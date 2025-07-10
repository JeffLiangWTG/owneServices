using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_OA_SupplierAddress()
		{
			var message = "There is no CID code that matches this address or any ABN code in the Supplier. Please update the Supplier Organisation";
			var orgHeader = Factory.New<OrgHeader>();
			var address1 = orgHeader.Addresses.AddNew();
			var cusCode1 = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456", Core.Constants.CountryCodes.Australia);
			cusCode1.OK_OA_PremisesAddress = address1.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_OH_Supplier = orgHeader.PK;
			InvoiceHeader.JZ_OA_SupplierAddress = address1.PK;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertNoMessageError("Supplier has CCID with matched address", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			var address2 = orgHeader.Addresses.AddNew();
			InvoiceHeader.JZ_OA_SupplierAddress = address2.PK;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertHasMessageError("Supplier doesn't have CCID with matched address", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "13579", Core.Constants.CountryCodes.Australia);
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertNoMessageError("Supplier doesn't have CCID with matched address but there's an CCID with blank address", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			InvoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertNoMessageError("Supplier has CCID with blank address when JZ_OA_SupplierAddress is blank", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			cusCode2.OK_OA_PremisesAddress = address2.PK;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertHasMessageError("Supplier doesn't have CCID with blank address when JZ_OA_SupplierAddress is blank", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertHasMessageError("Supplier doesn't have any CCID or ABN code", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "123456", Core.Constants.CountryCodes.Australia);
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertNoMessageError("Supplier only has ABN code", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertNoMessageError("There should be no validation error on JZ_OA_SupplierAddress for Export declaration", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertNoMessageError("There should be no validation error on JZ_OA_SupplierAddress for ExWarehouse declaration", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertHasMessageError("There should be validation error on JZ_OA_SupplierAddress for Quarantine declaration", InvoiceHeader.JZ_OA_SupplierAddressInfo, message);
		}

		#region Implementation

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
			set { base.declaration = value; }
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.invoiceHeader; }
			set { base.invoiceHeader = value; }
		}

		protected JobComInvoiceGroupHeader invoiceGroupHeader;
		protected RefCurrency expiredCurrency;
		protected abstract JobComInvoiceHeaderValidation GetNewValidationProvider(JobComInvoiceHeader invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();

			Declaration = Factory.New<JobDeclaration>();
			Declaration.JE_ExportDate = ZDateTime.Today;
			invoiceGroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
			InvoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();

			expiredCurrency = Factory.New<RefCurrency>();
			expiredCurrency.RX_Code = "USD";    // Must be a valid currency for Exit 1
			expiredCurrency.RX_Desc = "Description";
			expiredCurrency.RX_SubUnitName = "SubUnit";
			expiredCurrency.RX_SubUnitRatio = 100;
			expiredCurrency.RX_Symbol = "X";
			expiredCurrency.RX_UnitName = "XXX";
		}
		#endregion
	}
}
