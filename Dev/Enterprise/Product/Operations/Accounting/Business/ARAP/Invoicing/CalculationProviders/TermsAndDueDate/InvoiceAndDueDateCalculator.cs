using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	/// <summary>
	/// Calculates invoice terms, date and due date base on IJobInvoicingPlugIn and organisation term settings.
	/// </summary>
	public class InvoiceAndDueDateCalculator
	{
		public InvoiceAndDueDateCalculator(IJobInvoicingPlugIn plugIn, ZDateTime initialInvoiceDate, OrgHeader organisation, string ledger = LedgerTypes.AccountsPayable, string invoiceType = "", JobHeader job = null)
				: this(plugIn, initialInvoiceDate, organisation, null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, ledger, invoiceType, job)
		{
		}

		public InvoiceAndDueDateCalculator(IJobInvoicingPlugIn plugIn, ZDateTime initialInvoiceDate, OrgHeader organisation, JobInvoicingConsumerType jobType, ZString direction, ZString transportMode, ZGuid branchPK, ZGuid departmentPK, string ledger, string invoiceType, JobHeader job = null)
		{
			plugIn_cached = plugIn;
			initialInvoiceDate_cached = initialInvoiceDate;
			this.jobHeader = job;

			invoiceTerm_cached = new InvoiceTerm();
			if (organisation != null)
			{
				this.orgCode_cached = organisation.OH_Code;

				var companyData = organisation.CompanyDataLoadOnly;
				if (companyData != null)
				{
					invoiceTerm_cached = ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable ? companyData.GetAPTerm() : companyData.GetARTerm(jobType, direction, transportMode, branchPK, departmentPK, invoiceType, null);
				}
			}
		}

		public InvoiceAndDueDateCalculator(IJobInvoicingPlugIn plugIn, ZDateTime initialInvoiceDate, InvoiceTerm invoiceTerm)
		{
			plugIn_cached = plugIn;
			initialInvoiceDate_cached = initialInvoiceDate;

			invoiceTerm_cached = invoiceTerm;
			overriddenInvoiceTerm = ZString.Empty;
		}

		readonly IJobInvoicingPlugIn plugIn_cached;
		readonly InvoiceTerm invoiceTerm_cached;
		readonly ZDateTime initialInvoiceDate_cached;
		readonly string orgCode_cached;

		public ZDateTime DueDate
		{
			get
			{
				if (fDueDate.IsEmpty)
				{
					ZDateTime initialDueDate = GetDateByInvoiceTerm();
					if (initialDueDate.IsEmpty)
					{
						initialDueDate = initialInvoiceDate_cached;
					}
					else
					{
						initialDueDate = initialDueDate.AddDays(invoiceTerm_cached.Days);
						if (initialDueDate < initialInvoiceDate_cached)
						{
							initialDueDate = initialInvoiceDate_cached;
						}
					}

					fDueDate = DueDateCalculation.GetDueDate(Factory, invoiceTerm_cached, initialDueDate, overriddenInvoiceTerm, jobHeader: jobHeader);
				}
				return fDueDate;
			}
		}
		ZDateTime fDueDate;

		public ZDateTime InvoiceDate
		{
			get
			{
				if (fInvoiceDate.IsEmpty)
				{
					ZDateTime dateByInvoiceTerm = GetDateByInvoiceTerm();
					fInvoiceDate = dateByInvoiceTerm.Date > initialInvoiceDate_cached.Date ? dateByInvoiceTerm : initialInvoiceDate_cached;
					if (new string[] { ARInvoiceTermsList.FromShipmentDate.Code, ARInvoiceTermsList.FromCustomsClearanceDate.Code, ARInvoiceTermsList.LaterOfShipmentOrInvoiceDate.Code }.Contains(InvoiceTerm.ToString()))
					{
						fInvoiceDate = fInvoiceDate > ZDateTime.Now ? ZDateTime.Now : fInvoiceDate;
					}
				}
				return fInvoiceDate;
			}
		}
		ZDateTime fInvoiceDate;

		public ZString InvoiceTerm
		{
			get { return IsInvoiceTermOverridden ? new ZString(overriddenInvoiceTerm) : invoiceTerm_cached.Term; }
		}
		string overriddenInvoiceTerm;

		public bool IsInvoiceTermOverridden
		{
			get { return !string.IsNullOrEmpty(overriddenInvoiceTerm); }
		}

		public ZString MessageForInvoiceTermOverriding
		{
			get { return fMessageForInvoiceTermOverriding; }
		}
		ZString fMessageForInvoiceTermOverriding;

		public ZByte InvoiceTermDays
		{
			get { return invoiceTerm_cached.Days; }
		}

		protected ZDateTime GetDateByInvoiceTerm()
		{
			return GetDateByInvoiceTerm(invoiceTerm_cached.Term);
		}

		ZDateTime GetDateByInvoiceTerm(ZString term)
		{
			ZDateTime dateByInvoiceTerm = ZDateTime.Empty;

			if (plugIn_cached != null)
			{
				if (term == Constants.InvoiceTerms.FromCustomsClearanceDate)
				{
					dateByInvoiceTerm = plugIn_cached.InvoicingSupporter.GetCustomsClearanceDate();
					if (dateByInvoiceTerm.IsEmpty)
					{
						dateByInvoiceTerm = GetDateByInvoiceTerm(Constants.InvoiceTerms.FromShipmentDate);
						if (!dateByInvoiceTerm.IsEmpty)
						{
							overriddenInvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
							var clientName = orgCode_cached != null ? Res.GetString("07297917-1e03-44cd-8751-1d8da15aa81b", "[{0}] :", orgCode_cached) : string.Empty;
							fMessageForInvoiceTermOverriding = Res.GetString("70f90560-58cc-444b-9d2d-f4c1b62de79a", "{0} Invoice terms are CUS - {1} Days from Customs Clearance date. As there is no CLR event on this job, Invoice Term bases on Shipment date", clientName, invoiceTerm_cached.Days);
						}
					}
				}
				else if (term == Constants.InvoiceTerms.FromShipmentDate)
				{
					dateByInvoiceTerm = plugIn_cached.InvoicingSupporter.GetShipmentDate();
				}
				else if (term == Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate)
				{
					var shipmentDate = plugIn_cached.InvoicingSupporter.GetShipmentDate();
					var invoiceDate = fInvoiceDate.IsEmpty ? initialInvoiceDate_cached.Date : fInvoiceDate;
					dateByInvoiceTerm = shipmentDate > invoiceDate ? shipmentDate : invoiceDate;
				}

				if (dateByInvoiceTerm.IsEmpty && new ZString[] { Constants.InvoiceTerms.FromCustomsClearanceDate, Constants.InvoiceTerms.FromShipmentDate, Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate }.Contains(invoiceTerm_cached.Term))
				{
					dateByInvoiceTerm = initialInvoiceDate_cached;
				}
			}

			return dateByInvoiceTerm;
		}

		BusinessObjectFactory Factory
		{
			get { return plugIn_cached != null ? plugIn_cached.Factory : (factory ?? (factory = new BusinessObjectFactory())); }
		}
		BusinessObjectFactory factory;
		readonly JobHeader jobHeader;
	}
}
