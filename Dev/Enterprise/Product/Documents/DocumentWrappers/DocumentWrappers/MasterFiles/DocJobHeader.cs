using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocJobHeader : DocBaseWrapper
	{
		DocJobHeader(JobHeader jobHeader, BusinessObjectFactory factoryToWrap)
			: base(jobHeader, factoryToWrap)
		{
		}

		public static DocJobHeader New(JobHeader jobHeader, BusinessObjectFactory factoryToWrap) =>
			jobHeader == null
			? null
			: (DocJobHeader)factoryToWrap.GetCachedValue<DocumentWrapper>
				(
					jobHeader.PK.ToStringKey(),
					() => new DocJobHeader(jobHeader, factoryToWrap),
					CacheStalenessPolicy.StaleWhenDataTableChanges(JobChargeSchema.Constants.TableName, jobHeader.Factory)
				);

		public static DocJobHeader New(JobHeader jobHeader1, OrgHeader debtor, BusinessObjectFactory factoryToWrap)
		{
			DocJobHeader jobHeader = DocJobHeader.New(jobHeader1, factoryToWrap);
			if (jobHeader != null)
			{
				jobHeader.Debtor = debtor;
			}
			return jobHeader;
		}

		#region Overrides

		public override string ToString()
		{
			return JobNumber;
		}

		#endregion

		#region Collections

		public DocJobChargeCollection JobCharges =>
			DocJobChargeCollection.GetCollection(this, nameof(JobCharges),
				(collection) => PopulateDocJobChargeCollectionFromJobChargesQuery(collection, new ZQuery(JobChargeSchema.JR_JH, JobHeader.PK))
			);

		public DocJobChargeCollection JobChargesForLocalClient =>
			DocJobChargeCollection.GetCollection(this, nameof(JobChargesForLocalClient),
				(collection) =>
				{
					var localChargesPK = JobHeader.LocalChargesPK;

					if (localChargesPK.IsValid && !localChargesPK.IsEmpty)
					{
						var query = new ZQuery(JobChargeSchema.JR_JH, JobHeader.PK);
						query.AddToFilter(JobChargeSchema.JR_OH_SellAccount, localChargesPK);
						PopulateDocJobChargeCollectionFromJobChargesQuery(collection, query);
					}
					else
					{
						PopulateDocJobChargeCollectionFromJobCharges(collection);
					}
				});

		public DocJobChargeCollection JobChargesForAgentCollect =>
			DocJobChargeCollection.GetCollection(this, nameof(JobChargesForAgentCollect),
				(collection) =>
				{
					var agentCollectPK = JobHeader.AgentCollectPK;

					if (agentCollectPK.IsValid && !agentCollectPK.IsEmpty)
					{
						var query = new ZQuery(JobChargeSchema.JR_JH, JobHeader.PK);
						query.AddToFilter(JobChargeSchema.JR_OH_SellAccount, agentCollectPK);
						PopulateDocJobChargeCollectionFromJobChargesQuery(collection, query);
					}
					else
					{
						PopulateDocJobChargeCollectionFromJobCharges(collection);
					}
				});

		public DocJobChargeCollection JobChargesForDebtorOrLocalClient =>
			DocJobChargeCollection.GetCollection(this, nameof(JobChargesForDebtorOrLocalClient),
				(collection) =>
				{
					var debtorOrLocalClientPK = Debtor != null ? Debtor.PK : JobHeader.LocalChargesPK;

					if (debtorOrLocalClientPK.IsValid && !debtorOrLocalClientPK.IsEmpty)
					{
						var query = new ZQuery(JobChargeSchema.JR_JH, JobHeader.PK);
						query.AddToFilter(JobChargeSchema.JR_OH_SellAccount, debtorOrLocalClientPK);
						PopulateDocJobChargeCollectionFromJobChargesQuery(collection, query);
					}
					else
					{
						PopulateDocJobChargeCollectionFromJobCharges(collection);
					}
				});

		void PopulateDocJobChargeCollectionFromJobChargesQuery(DocJobChargeCollection collection, ZQuery query)
		{
			var charges = JobHeader.Factory.Load<JobCharge>(query);
			PopulateDocJobChargeCollectionFromJobCharges(collection, charges);
		}

		void PopulateDocJobChargeCollectionFromJobCharges(DocJobChargeCollection collection, params JobCharge[] charges)
		{
			foreach (JobCharge currentCharge in charges)
			{
				collection.Add(DocJobCharge.New(currentCharge, Factory));
			}
		}

		public DocExchangeRateCollection ExchangeRates
		{
			get
			{
				if (exchangeRates == null)
				{
					exchangeRates = new DocExchangeRateCollection(Factory);

					foreach (ExchangeRate rate in Factory.Load<ExchangeRate>(new ZQuery(JobExRateSchema.JF_JH, JobHeader.PK)))
					{
						exchangeRates.Add(DocExchangeRate.New(rate, Factory));
					}

					return exchangeRates;
				}

				return exchangeRates;
			}
		}

		DocExchangeRateCollection exchangeRates;

		public DocExchangeRateCollection ExchangeRatesExcludedCreditors
		{
			get
			{
				var collection = new DocExchangeRateCollection(Factory);

				foreach (var rate in ExchangeRates.Cast<DocExchangeRate>().Where(r => r.OrgType == "DEB" || r.OrgType == "ALL"))
				{
					collection.Add(rate);
				}

				return collection;
			}
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime ActualJCL
		{
			get { return JobHeader.JH_A_JCL; }
		}

		public ZDateTime ActualJOP
		{
			get { return JobHeader.JH_A_JOP; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal AgentChargesCFX
		{
			get { return JobHeader.JH_AgentChargesCFX; }
		}

		public ZDecimal LocalChargesCFX
		{
			get { return JobHeader.JH_LocalChargesCFX; }
		}

		#endregion

		#region ZGuid Fields

		public ZGuid ParentID
		{
			get { return JobHeader.JH_ParentID; }
		}

		#endregion

		#region ZString Fields

		public ZString ParentTableCode
		{
			get { return JobHeader.JH_ParentTableCode; }
		}

		public ZString HoldReason
		{
			get { return JobHeader.JH_HoldReason; }
		}

		public ZString JobNumber
		{
			get { return JobHeader.JH_JobNum; }
		}

		public ZString Status
		{
			get { return JobHeader.JH_Status; }
		}

		public ZString QuoteNumber
		{
			get { return JobHeader.JH_TH_NKQuoteNumber; }
		}

		public ZString FormattedTotalLocalSellAmount
		{
			get { return FormatLocalAmount(JobChargesForLocalClient.TotalLocalSellAmount); }
		}

		public ZString FormattedTotalLocalSellAmountIncTax
		{
			get { return FormatLocalAmount(JobChargesForLocalClient.TotalLocalSellAmountIncTax); }
		}

		public ZString FormattedTotalTaxAmount
		{
			get { return FormatLocalAmount(JobChargesForLocalClient.TotalTaxAmount); }
		}

		public ZString ActualJOPString
		{
			get { return JobHeader.JH_A_JOP.ToShortDateString(); }
		}

		public ZString ActualJCLString
		{
			get { return JobHeader.JH_A_JCL.ToShortDateString(); }
		}

		public ZString JobHeaderExportCustomsHandlingNotes
		{
			get
			{
				ZString result = ZString.Empty;
				if (JobHeader.LocalChargesAddr != null
					&& JobHeader.LocalChargesAddr.Header != null
					&& JobHeader.LocalChargesAddr.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.ExportCustomsHandlingNotes.Description).Length > 0)
				{
					result = JobHeader.LocalChargesAddr.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.ExportCustomsHandlingNotes.Description)[0].ST_NoteDataAsText;
				}

				return result;
			}
		}

		public ZString JobHeaderImportCustomsHandlingNotes
		{
			get
			{
				ZString result = ZString.Empty;
				if (JobHeader.LocalChargesAddr != null
					&& JobHeader.LocalChargesAddr.Header != null
					&& JobHeader.LocalChargesAddr.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes.Description).Length > 0)
				{
					result = JobHeader.LocalChargesAddr.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.ImportCustomsHandlingNotes.Description)[0].ST_NoteDataAsText;
				}

				return result;
			}
		}

		public ZString JobHeaderInvoicingPreferences
		{
			get
			{
				ZString result = ZString.Empty;
				if (JobHeader.LocalChargesAddr != null
					&& JobHeader.LocalChargesAddr.Header != null
					&& JobHeader.LocalChargesAddr.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.InvoicingPreferences.Description).Length > 0)
				{
					result = JobHeader.LocalChargesAddr.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.InvoicingPreferences.Description)[0].ST_NoteDataAsText;
				}

				return result;
			}
		}

		public ZString InternalWorkNotes
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.InternalWorkNotes.Description, JobHeader.LocalCharges); }
		}

		#endregion

		#region Wrapper Fields

		public DocBranch Branch
		{
			get { return DocBranch.New(JobHeader.Branch, Factory); }
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(JobHeader.Department, Factory); }
		}

		public DocStaff OpsRep
		{
			get { return DocStaff.New(JobHeader.RepOps, Factory); }
		}

		public DocStaff SalesRep
		{
			get { return DocStaff.New(JobHeader.RepSales, Factory); }
		}

		public DocOrganisation AgentCollect
		{
			get { return DocOrganisation.New(JobHeader.AgentCollect, Factory); }
		}

		public DocAddress AgentCollectAddress
		{
			get { return DocAddress.New(JobHeader.AgentCollectAddr, Factory); }
		}

		public DocOrganisation LocalCharges
		{
			get { return DocOrganisation.New(JobHeader.LocalCharges, Factory); }
		}

		public DocAddress LocalChargesAddress
		{
			get { return DocAddress.New(JobHeader.LocalChargesAddr, Factory); }
		}

		public DocOrganisation DebtorOrLocalCharges
		{
			get
			{
				DocOrganisation result;
				if (Debtor != null)
				{
					result = DocOrganisation.New(Debtor, Factory);
				}
				else
				{
					result = LocalCharges;
				}
				return result;
			}
		}

		protected internal OrgHeader Debtor;

		#endregion

		#region ZBool Fields

		public ZBool SingleAgentsInvoicePerConsol
		{
			get { return JobHeader.JH_SingleAgentsInvoicePerConsol; }
		}

		#endregion

		#region Implementation

		ZString FormatLocalAmount(ZDecimal amount)
		{
			if (Branch != null && Branch.Country != null && Branch.Country.Currency != null)
			{
				return Branch.Country.Currency.FormatMoney(amount);
			}
			else
			{
				return amount.ToString(2);
			}
		}

		internal JobHeader JobHeader
		{
			get { return (JobHeader)WrappedObject; }
		}

		#endregion
	}
}
