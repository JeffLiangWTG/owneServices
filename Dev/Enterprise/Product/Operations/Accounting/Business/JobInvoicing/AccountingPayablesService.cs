using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AccountingPayablesService : IAccountingPayablesService
	{
		public string GetTaxCode(string countryCode)
		{
			var taxCode = Country.GetConsumptionTaxDescription(countryCode);
			return string.IsNullOrEmpty(taxCode) ? "VAT" : taxCode;
		}

		public DateTime GetDueDate(DateTime invoiceDate, ZGuid companyPK, ZGuid creditorPK)
		{
			var factory = new BusinessObjectFactory();

			var zQuery = new ZQuery(OrgCompanyDataSchema.OB_GC, companyPK)
					.AddToFilter(OrgCompanyDataSchema.OB_OH, creditorPK);

			var orgCompanyData = factory.LoadTop1<OrgCompanyData>(zQuery);
			if (orgCompanyData != null)
			{
				var invoiceTerm = orgCompanyData.GetAPTerm();
				return DueDateCalculation.GetDueDate(factory, invoiceTerm, invoiceDate).ToDateTime();
			}

			return invoiceDate;
		}
	}
}
