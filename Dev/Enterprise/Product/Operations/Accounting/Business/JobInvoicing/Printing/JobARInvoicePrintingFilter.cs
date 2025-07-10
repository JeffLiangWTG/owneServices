using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobARInvoicePrintingFilter : JobInvoicePrintingFilter, IObsoleteValidation
	{
		readonly ZGuid jobCompany;

		public JobARInvoicePrintingFilter(IBusiness hostBusinessObject, ZGuid jobHeaderPK)
			: base(hostBusinessObject, jobHeaderPK)
		{
		}

		public JobARInvoicePrintingFilter(ForwardingConsol hostBusinessObject, ZGuid[] jobHeaderPKs)
			: base(hostBusinessObject, jobHeaderPKs)
		{
		}

		public JobARInvoicePrintingFilter(IBusiness hostBusinessObject, ZGuid jobHeaderPK, BusinessObjectFactory factory, ZGuid jobCompany)
			: base(hostBusinessObject, jobHeaderPK, factory)
		{
			this.jobCompany = jobCompany;
		}

#if DEBUG
		public JobARInvoicePrintingFilter(IBusiness hostBusinessObject, Job jobHeader, ZDateTime from, ZDateTime to)
			: base(hostBusinessObject, jobHeader.PK, jobHeader.Factory, from, to)
		{
		}
#endif

		protected override OrganisationsFindBoxCollection GetDebtorOrCreditorFindBoxCollectionCore()
		{
			return FindboxLookupCollections.GetDebtorCollection(Factory);
		}

		protected override IEnumerable<string> Ledger
		{
			get { yield return LedgerTypes.AccountsReceivable; }
		}

		protected override ZGuid GetCompanyPK()
		{
			return jobCompany.IsValid ? jobCompany : base.GetCompanyPK();
		}

		public bool EnableAccTransactionHeaderIndexHints { get; set; }

		protected override void AddAccTransactionHeaderIndexHints(ZDBOnlyQuery query, Guid jobPK, ZString ledgerType)
		{
			if (jobPK != Guid.Empty && EnableAccTransactionHeaderIndexHints)
			{
				query.TableIndexHints.Add(new TableIndexHint(AccTransactionHeader.Schema.Index_FK_RX__AH_JH));
				query.TableIndexHints.Add(new TableIndexHint(AccTransactionHeaderSchema.Constants.PkIndex));
				query.IsForceSeek = true;
			}
		}
	}
}
