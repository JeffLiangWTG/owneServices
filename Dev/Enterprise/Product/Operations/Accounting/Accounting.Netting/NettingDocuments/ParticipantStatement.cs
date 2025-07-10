using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class ParticipantStatement : NettingStatement
	{
		public ParticipantStatement(BusinessObjectFactory factory, ZGuid nettingPeriod)
			: this(factory, nettingPeriod, ZString.Empty, StatementType.Trial)
		{
		}

		public ParticipantStatement(BusinessObjectFactory factory, ZGuid nettingPeriod, StatementType statementType)
			: this(factory, nettingPeriod, ZString.Empty, statementType)
		{
		}

		public ParticipantStatement(BusinessObjectFactory factory, ZGuid nettingPeriod, ZString companyCode, StatementType statementType)
			: base(factory, nettingPeriod, statementType)
		{
			CompanyCode = companyCode;

			if (AccountingConfigurationRegistry.Instance.NettingModeOption.Value == AccountingConstants.NettingModeOption.CompanyLevel.Code &&
				!CompanyCode.IsEmpty && NettingOrganization == null)
			{
				throw new IncorrectDataSetupException(Res.GetString("c842012f-ba8c-4a23-9de1-feeca5f624ea", "Netting Participant not found. The Organization Proxy of company '{0}' is not a Netting Participant. Kindly add the same as the participant and try again.", CompanyCode));
			}
		}

		public ZString OrgCode => Organization != null ? Organization.OH_Code : ZString.Empty;

		public readonly ZString CompanyCode;
		internal OrgHeader Organization
		{
			get
			{
				if (organization == null && !CompanyCode.IsEmpty)
				{
					organization = OrgHeader.GetCompanyOrgProxyFromCompanyCode(Factory, CompanyCode);
				}

				return organization;
			}
		}

		OrgHeader organization;

		NettingOrganisation NettingOrganization
		{
			get
			{
				if (nettingOrganization == null && Organization != null)
				{
					var query = new ZQuery();
					query.AddToFilter(NettingOrganisationSchema.NSO_OH_Organisation, Organization.PK.ToGuid());
					query.AddToFilter(NettingOrganisationSchema.NSO_NS_NettingSystem, Period?.NSP_NS_NettingSystem);

					nettingOrganization = Factory.LoadTop1<NettingOrganisation>(query);
				}

				return nettingOrganization;
			}
		}

		NettingOrganisation nettingOrganization;

		public ZString ReportingCurrency
		{
			get { return NettingOrganization != null ? NettingOrganization.NSO_RX_NKReportingCurrency : ZString.Empty; }
		}

		public override NettingTransaction LocalCurrency
		{
			get
			{
				if (localCurrency == null)
				{
					if (Transactions.Any())
					{
						if (exchangeRates == null || !exchangeRates.Any())
						{
							exchangeRates = NettingHelper.ReadExchangeRatesFromDatabase(Period, ExchangeRateType, new BusinessObjectFactory());
						}

						localCurrency = new NettingTransaction(this)
						{
							Currency = ReportingCurrency,
							ParticipantCurrency = ReportingCurrency,
							ParticipantExchangeRate = GetExchangeRate(ReportingCurrency),
							NettingCurrency = Transactions.First().NettingCurrency,
							NettingSystemExchangeRate = GetExchangeRate(ReportingCurrency)
						};
					}
				}

				return localCurrency;
			}
		}
		NettingTransaction localCurrency;

		public override IEnumerable<NettingTransaction> Transactions
		{
			get
			{
				return transactions ?? (transactions = base.Transactions.Where(x => x.CompanyCode == CompanyCode));
			}
		}
		IEnumerable<NettingTransaction> transactions;

		public override IEnumerable<NettingTransaction> FXOffers
		{
			get
			{
				return fxOffers ?? (fxOffers = base.FXOffers.Where(x => x.CompanyCode == CompanyCode));
			}
		}
		IEnumerable<NettingTransaction> fxOffers;

		public override IEnumerable<NettingTransaction> FXRequests
		{
			get
			{
				return fXRequests ?? (fXRequests = base.FXRequests.Where(x => x.CompanyCode == CompanyCode));
			}
		}
		IEnumerable<NettingTransaction> fXRequests;

		public override IEnumerable<NettingMovement> GetNettingMovements()
		{
			return nettingMovements ?? (nettingMovements = base.GetNettingMovements().Where(x => x.CompanyCode == CompanyCode));
		}
		IEnumerable<NettingMovement> nettingMovements;

		public override IEnumerable<NettingMovement> GetReceivableNettingMovements()
		{
			return receivableNettingMovements ?? (receivableNettingMovements = GetNettingMovements().Where(x => x.Direction == "OUT"));
		}
		IEnumerable<NettingMovement> receivableNettingMovements;

		public override IEnumerable<NettingMovement> GetPayableNettingMovements()
		{
			return payableNettingMovements ?? (payableNettingMovements = GetNettingMovements().Where(x => x.Direction == "IN"));
		}
		IEnumerable<NettingMovement> payableNettingMovements;

		public override IEnumerable<NettingMatchedInvoice> ReceivableMatchedInvoices
		{
			get
			{
				return receivableMatchedInvoices ?? (receivableMatchedInvoices = GetSplitMatchedInvoices().Where(x => x.Issuer == OrgCode));
			}
		}
		IEnumerable<NettingMatchedInvoice> receivableMatchedInvoices;

		public override IEnumerable<NettingMatchedInvoice> PayableMatchedInvoices
		{
			get
			{
				return payableMatchedInvoices ?? (payableMatchedInvoices = GetSplitMatchedInvoices().Where(x => x.Recipient == OrgCode));
			}
		}
		IEnumerable<NettingMatchedInvoice> payableMatchedInvoices;

		public override IEnumerable<NettingClearingJournal> NettingClearingJournals
		{
			get
			{
				return nettingClearingJournals ?? (nettingClearingJournals = base.NettingClearingJournals.Where(x => x.CompanyCode == CompanyCode || x.ParticipatingCompanyCode == CompanyCode));
			}
		}
		IEnumerable<NettingClearingJournal> nettingClearingJournals;

		public override DocumentSupporter DocumentSupporter
		{
			get { return new ParticipantStatementDocumentSupporter(this); }
		}
	}

	public class ParticipantStatementDocumentSupporter : NettingStatementDocumentSupporter
	{
		public ParticipantStatementDocumentSupporter(ParticipantStatement participantStatement)
			: base(participantStatement)
		{
		}

		ParticipantStatement ParticipantStatement
		{
			get { return (ParticipantStatement)BusinessObject; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(ParticipantStatement.Organization, null);
		}
	}
}
