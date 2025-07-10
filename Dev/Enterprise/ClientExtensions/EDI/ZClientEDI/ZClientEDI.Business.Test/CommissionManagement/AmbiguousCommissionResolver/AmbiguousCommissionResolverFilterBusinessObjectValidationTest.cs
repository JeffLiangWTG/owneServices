using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	internal class AmbiguousCommissionResolverFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCompanyPk()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			filterBizObj.IsResolvingFiltered = true;

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			filterBizObj.CompanyPk = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(filterBizObj.CompanyPkInfo);
			AssertMandatoryValidationError(filterBizObj.CompanyPkInfo, false);

			filterBizObj.CompanyPk = Factory.New<GlbCompany>().PK;
			AssertHasError(filterBizObj.CompanyPkInfo, "Must be current login company");
			AssertMandatoryValidationError(filterBizObj.CompanyPkInfo, false);

			filterBizObj.CompanyPk = ZGuid.Empty;
			AssertMandatoryValidationError(filterBizObj.CompanyPkInfo, true);

			filterBizObj.IsResolvingFiltered = false;
			AssertMandatoryValidationError(filterBizObj.CompanyPkInfo, false);

			filterBizObj.IsResolvingFiltered = true;
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			filterBizObj.CompanyPk = Factory.New<GlbCompany>().PK;
			AssertNoErrors(filterBizObj.CompanyPkInfo);
			AssertMandatoryValidationError(filterBizObj.CompanyPkInfo, false);
		}

		public void TestCheckInvoicePkToResolve()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			filterBizObj.IsResolvingFiltered = true;
			filterBizObj.CompanyPk = GlbCompany.CurrentCompany.PK;
			filterBizObj.InvoicePkToResolve = Factory.New<ARInvoice>().PK;
			AssertMandatoryValidationError(filterBizObj.InvoicePkToResolveInfo, false);

			filterBizObj.InvoicePkToResolve = ZGuid.Empty;
			AssertMandatoryValidationError(filterBizObj.InvoicePkToResolveInfo, true);

			filterBizObj.CompanyPk = Factory.New<GlbCompany>().PK;
			filterBizObj.Validation.ValidateInvoicePkToResolve();
			AssertMandatoryValidationError(filterBizObj.InvoicePkToResolveInfo, false);
		}

		public void TestCheckInvoiceNumberToResolve()
		{
			var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(Factory);
			filterBizObj.IsResolvingFiltered = true;
			filterBizObj.CompanyPk = Factory.New<GlbCompany>().PK;
			filterBizObj.InvoiceNumberToResolve = "1111";
			AssertMandatoryValidationError(filterBizObj.InvoiceNumberToResolveInfo, false);

			filterBizObj.InvoiceNumberToResolve = "";
			AssertMandatoryValidationError(filterBizObj.InvoiceNumberToResolveInfo, true);

			filterBizObj.CompanyPk = GlbCompany.CurrentCompany.PK;
			filterBizObj.Validation.ValidateInvoiceNumberToResolve();
			AssertMandatoryValidationError(filterBizObj.InvoiceNumberToResolveInfo, false);
		}
	}
}
