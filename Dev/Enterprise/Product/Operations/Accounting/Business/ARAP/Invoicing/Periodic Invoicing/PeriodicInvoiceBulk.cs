using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceBulk : PeriodicInvoiceBase
	{
		public PeriodicInvoiceBulk(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override PeriodicInvoiceBaseJobFilterBusinessObject GetJobFilterBusinessObject()
		{
			return new PeriodicInvoiceBulkJobsFilterBusinessObject();
		}

		PeriodicInvoiceCollection fPeriodicInvoices;
		public PeriodicInvoiceCollection PeriodicInvoices
		{
			get
			{
				if (fPeriodicInvoices == null)
				{
					fPeriodicInvoices = new PeriodicInvoiceCollection(Factory);
					fPeriodicInvoices.IncludeInThePeriodicInvoiceChanged += PeriodicInvoices_IncludeInThePeriodicInvoiceChanged;
					fPeriodicInvoices.TaxBranchChanged += PeriodicInvoices_TaxBranchChanged;
					RegisterEditableChildObject(fPeriodicInvoices);
				}
				return fPeriodicInvoices;
			}
		}

		public override ZDateTime InvoiceDate
		{
			get { return base.InvoiceDate; }
			set
			{
				base.InvoiceDate = value;
				foreach (PeriodicInvoice invoice in PeriodicInvoices)
				{
					invoice.InvoiceDate = InvoiceDate;
				}
			}
		}

		protected override bool InvoiceDate_ReadOnly => base.InvoiceDate_ReadOnly || !Env.Security.NewReceivablesBulkPeriodicInvoiceDate.IsAllowed;

		public override ZDateTime PostDate
		{
			get { return base.PostDate; }
			set
			{
				base.PostDate = value;
				foreach (PeriodicInvoice invoice in PeriodicInvoices)
				{
					invoice.PostDate = PostDate;
				}
			}
		}

		public PeriodicInvoiceBulkPoster CreatePeriodicInvoiceBulkPoster()
		{
			var poster = new PeriodicInvoiceBulkPoster();

			foreach (PeriodicInvoice periodicInvoice in PeriodicInvoices)
			{
				if (periodicInvoice.IncludeInThePeriodicInvoice)
				{
					poster.AddInvoiceInfo(new PeriodicInvoiceBulkPoster.InvoiceInfo(periodicInvoice));
				}
			}

			return poster;
		}

		void PeriodicInvoices_IncludeInThePeriodicInvoiceChanged(object sender, EventArgs e)
		{
			CalculateTotals();
		}

		void PeriodicInvoices_TaxBranchChanged(object sender, EventArgs e)
		{
			CalculateTotals();
		}

		protected override void OnAfterChangeLines()
		{
			base.OnAfterChangeLines();

			PeriodicInvoices.RemoveAll();

			var periodicInvoicesDictionary = new Dictionary<PeriodicInvoiceKey, PeriodicInvoice>();
			PeriodicInvoice currentPeriodicInvoice;

			foreach (Charge charge in Charges)
			{
				RefCurrency sellCurrency = RefCurrency.LoadFromCurrencyCode(Factory, charge.JR_RX_NKSellCurrency);
				ZString chargePostingCurrency;
				if (InvoiceTypeCalculationProvider.BillInLocalCurrency(charge.JR_InvoiceType))
				{
					chargePostingCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
				else
				{
					if (sellCurrency != null)
					{
						chargePostingCurrency = sellCurrency.RX_Code;
					}
					else
					{
						chargePostingCurrency = ZString.Empty;
					}
				}

				ZString layout = ZString.Empty;

				if (charge.InvoicingJob != null)
				{
					layout = GetLayout(charge.SellAccount, charge.InvoicingJob);
				}

				PeriodicInvoiceKey key = new PeriodicInvoiceKey(charge.JR_OH_SellAccount, chargePostingCurrency, charge.JR_InvoiceType, layout);

				if (!periodicInvoicesDictionary.TryGetValue(key, out currentPeriodicInvoice))
				{
					currentPeriodicInvoice = new PeriodicInvoice(Factory);
					currentPeriodicInvoice.Initialization(this, charge.JR_OH_SellAccount, charge.JR_InvoiceType, true);
					periodicInvoicesDictionary.Add(key, currentPeriodicInvoice);
					currentPeriodicInvoice.JobsFilter.ChargeFilterQueryFromBulkInvoice = JobsFilter.GetChargeQueryWithoutSpecialFilters();
				}

				currentPeriodicInvoice.InitializedCharges.Add(charge);

				if (charge.JR_GB_SellTaxBranch == currentPeriodicInvoice.TaxBranch)
				{
					var selectableJob = currentPeriodicInvoice.AddJobFromCharge(charge);

					if (selectableJob.IncludeInThePeriodicInvoice)
					{
						currentPeriodicInvoice.Charges.Add(charge);
					}
				}
			}

			foreach (InvoicingBase invoice in MiscInvoices)
			{
				string invoiceTypeAsDeferred = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(invoice.AH_TransactionCategory);
				PeriodicInvoiceKey key = new PeriodicInvoiceKey(invoice.AH_OH, invoice.AH_RX_NKTransactionCurrency, invoiceTypeAsDeferred, string.Empty);

				if (!periodicInvoicesDictionary.TryGetValue(key, out currentPeriodicInvoice))
				{
					currentPeriodicInvoice = new PeriodicInvoice(Factory);
					currentPeriodicInvoice.Initialization(this, invoice.AH_OH, invoiceTypeAsDeferred, true);
					periodicInvoicesDictionary.Add(key, currentPeriodicInvoice);
				}

				currentPeriodicInvoice.MiscInvoices.Add(currentPeriodicInvoice.Factory.Load<InvoicingBase>(invoice.PK));
			}

			var suspenders = FunctionalitySuspender.SuspendCollection(periodicInvoicesDictionary.Values,
				periodicInvoice => periodicInvoice.ReloadChargesSuspenderWithoutReloadOnResume.GetSuspender());
			try
			{
				PeriodicInvoices.AddRange(periodicInvoicesDictionary.Values);
			}
			finally
			{
				FunctionalitySuspender.ResumeCollection(suspenders);
			}

			foreach (PeriodicInvoice periodicInvoice in PeriodicInvoices)
			{
				periodicInvoice.UpdateDataAfterLinesChanges();
			}

			((IBindingListView)PeriodicInvoices).ApplySort(((IBindingListView)PeriodicInvoices).SortDescriptions);
			CalculateTotals();
		}

		public override void ReloadChargesByJob(ZGuid jobPK)
		{
			base.ReloadChargesByJob(jobPK);

			foreach (PeriodicInvoice periodicInvoice in PeriodicInvoices)
			{
				periodicInvoice.ReloadChargesByJob(jobPK);
			}

			OnAfterChangeLines();
		}

		internal static string unauthorizedCreditNoteErrorMessage => Res.GetString("b5e47f25-d6e7-4572-b2eb-239dab82a22b", "You do not have sufficient approval level to post this credit note. A user with a higher approval level can post this transaction. Alternatively select New Periodic Invoice for this debtor which will allow on the spot approval by a user with adequate security.");
		internal static string enforceTwoUserApprovalEnabledErrorMessage => Res.GetString("cd384b14-24fe-4f88-bd71-65d23bfd0e20", "Enforce Two Credit Note Approvers registry setting is enabled for this login company. Periodic Credit Notes Cannot be posted from Bulk Periodic Invoices, however they can be posted individually in Single Periodic Invoices.");
		internal static string enforceSequentialUserApprovalEnabledErrorMessage => Res.GetString("7de7c06f-9200-4aa0-bbc8-6c424e6edc59", "The Authorization Mode for credit note approval requires multiple users to review and approve the credit note. This credit note cannot be posted from Bulk Periodic Invoicing. You can approve and post this credit note by creating a Single Periodic Invoice for this Debtor organization.");

		public ZBool CheckLevelSecurityRightsForPeriodicCreditNotes()
		{
			var result = ZBool.True;
			if (MiscInvoices.Any())
			{
				result = ZBool.False;
			}
			else if (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.Value.AuthorizationMode == AuthorizationMode.Codes.TwoApprovers)
			{
				var isAuthorizationRequired = false;
				PeriodicInvoices.Cast<PeriodicInvoice>().Where(x => x.IncludeInThePeriodicInvoice && x.LocalTotalAmount < 0).ForEach(x =>
				{
					isAuthorizationRequired = true;
					x.RemoveRowError(unauthorizedCreditNoteErrorMessage);
					x.AddRowError(enforceTwoUserApprovalEnabledErrorMessage);
				});
				result = isAuthorizationRequired;
			}
			else if (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.Value.AuthorizationMode == AuthorizationMode.Codes.SequentialApprovers)
			{
				var isAuthorizationRequired = false;
				PeriodicInvoices.Cast<PeriodicInvoice>().Where(x => x.IncludeInThePeriodicInvoice && x.LocalTotalAmount < 0).ForEach(x =>
				{
					isAuthorizationRequired = true;
					x.RemoveRowError(unauthorizedCreditNoteErrorMessage);
					x.AddRowError(enforceSequentialUserApprovalEnabledErrorMessage);
				});
				result = isAuthorizationRequired;
			}
			else
			{
				var isAuthorizationRequired = false;
				foreach (PeriodicInvoice periodicInvoice in PeriodicInvoices)
				{
					periodicInvoice.RemoveRowError(unauthorizedCreditNoteErrorMessage);
					if (periodicInvoice.IncludeInThePeriodicInvoice && periodicInvoice.LocalTotalAmount < 0)
					{
						isAuthorizationRequired |= CheckLevelSecurityRightsForSinglePeriodicCreditNote(periodicInvoice, unauthorizedCreditNoteErrorMessage);
					}
				}
				result = isAuthorizationRequired;
			}
			return result;
		}

		bool CheckLevelSecurityRightsForSinglePeriodicCreditNote(PeriodicInvoice periodicInvoice, string errorMessage)
		{
			var result = false;

			var periodicInvoiceInNewFactory = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
			if (periodicInvoiceInNewFactory.CreateTransactions())
			{
				var creditNoteInNewFactory = periodicInvoiceInNewFactory.PostManager.Poster.PostedInvoices.Cast<InvoicingBase>().FirstOrDefault(x => x.AH_TransactionType == TransactionTypes.CreditNote);
				if (creditNoteInNewFactory != null)
				{
					InvoicingPreSaveHelper.PrepareTransactionsForAuthorisationCalculation(creditNoteInNewFactory);
					if (creditNoteInNewFactory.LevelAuthorizationRequired)
					{
						result = true;
						periodicInvoice.AddRowError(errorMessage);
					}
				}
			}
			return result;
		}

		protected override IEnumerable<Charge> SelectedCharges
		{
			get
			{
				var result = new List<Charge>();

				foreach (PeriodicInvoice periodicInvoice in PeriodicInvoices)
				{
					result.AddRange(periodicInvoice.Charges.ToArray<Charge>());
				}

				return result;
			}
		}

		protected override bool ShouldRegisterEditableChildObject
		{
			get { return false; }
		}

		protected override IEnumerable<ZString> GetJobTypesWhichHaveConfiguration()
		{
			return null;
		}

		struct PeriodicInvoiceKey
		{
			public PeriodicInvoiceKey(ZGuid debtorPK, ZString currencyNK, ZString invoiceType, ZString layout)
			{
				DebtorPK = debtorPK;
				CurrencyNK = currencyNK;
				InvoiceType = invoiceType;
				Layout = layout;
			}

			public readonly ZGuid DebtorPK;
			public readonly ZString CurrencyNK;
			public readonly ZString InvoiceType;
			public readonly ZString Layout;

			public override bool Equals(object obj)
			{
				return ToString() == ((PeriodicInvoiceKey)obj).ToString();
			}

			public override int GetHashCode()
			{
				return ToString().GetHashCode();
			}

			public override string ToString()
			{
				return DebtorPK.ToString() + CurrencyNK.ToString() + InvoiceType + Layout;
			}
		}
	}
}
