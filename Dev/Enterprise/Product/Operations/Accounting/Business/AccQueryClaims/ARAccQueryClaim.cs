using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class ARAccQueryClaim : AccQueryClaimBase, IARAccQueryClaim, IDocManagerSupport, IEDocsParsingSupport
	{
		public ARAccQueryClaim(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			adjustPostedInvoiceHelper_constructorInitializedOnly = new AdjustPostedInvoiceHelper();
		}

		IAdjustPostedInvoiceHelper AdjustPostedInvoiceHelper => adjustPostedInvoiceHelper_constructorInitializedOnly;
		IAdjustPostedInvoiceHelper adjustPostedInvoiceHelper_constructorInitializedOnly;

		#region Overrides

		public override ZString Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override ContactType DefaultContactType
		{
			get { return ContactType.Receivables; }
		}

		protected override INumberFountainProxy NumberFountainNo
		{
			get { return Env.NumberFountains.QueryClaimNo; }
		}

		public override bool AY_HoldOption_ReadOnly
		{
			get { return true; }
		}

		#region Approving

		public override void Approve()
		{
			base.Approve();

			if (TransactionHeader != null)
			{
				RelatedUnapprovedCreditNote = GetRelatedUnapprovedCreditNote();
				if (RelatedUnapprovedCreditNote != null)
				{
					if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsReceivable, GlbCompany.CurrentCompany.PK))
					{
						throw new InvalidOperationException(Res.GetString("9AC67885-4674-47C2-90E3-0E08347DAB5D", "This claim is not allowed to be approved. A Credit Note was going to be created as a result of this claim approval. ") + AccountingMasterFilesUtils.ARCreditNoteDisallowedMessage);
					}

					if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, RelatedUnapprovedCreditNote.AH_GC))
					{
						throw new InvalidOperationException(Res.GetString("7C5D94F5-280A-4068-8622-5FBCADA889CD", "This claim is not allowed to be approved. This would create a Credit Note in the Claiming company ({0}). Posting of Credit Notes is not allowed in {0}.", RelatedUnapprovedCreditNote.Company.GC_Code));
					}

					// below is the part that creates the AR credit note in this login company
					IReceivablesPostingChargeCollection chargesCollection = new IReceivablesPostingChargeCollection();
					OrgHeader arCreditNoteDebtor = GetARCreditNoteDebtor();
					RefCurrency arCreditNoteCurrency = RelatedUnapprovedCreditNote.TransactionCurrency;
					ZDecimal currentARRate = GetARCreditNoteExchangeRate(arCreditNoteCurrency.RX_Code);
					if (arCreditNoteDebtor == null || arCreditNoteCurrency == null || currentARRate == 0M)
					{
						if (currentARRate == 0M)
						{
							throw new InvalidOperationException("Please add a Buy rate for the claim currency."
								+ System.Environment.NewLine +
								"Approving this claim requires the creation of a foreign currency transaction using today’s exchange rate. Currently there is no buy rate for this date.");
						}
						else
						{
							throw new InvalidOperationException(string.Format("System can't define {0} for approving.",
								arCreditNoteDebtor == null ? "Debtor" : "Currency"));
						}
					}
					else
					{
						ZString chargeInvoiceType = InvoiceTypesList.Codes.FinalInvoice;
						if (arCreditNoteCurrency.RX_Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							chargeInvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
						}
						foreach (UACreditNoteLine line in RelatedUnapprovedCreditNote.Lines)
						{
							Job loginCompanyJob = GetLoginCompanyJob(line.Job);
							if (loginCompanyJob != null)
							{
								if (loginCompanyJob.IsReadyForFinancialClosureWithoutPostSecurity)
								{
									throw new InvalidOperationException(Res.GetString("51723815-5094-4FF0-90A4-9D1F7936A066", "Can not approve this claim as related job has Jobs Ready for Financial Closure status."));
								}
								if (loginCompanyJob.IsClosed)
								{
									throw new InvalidOperationException(Res.GetString("70EF711F-5FC5-451F-9CBD-AA7F432391ED", "This claim is not allowed to be approved as it is linked to closed jobs. If you need to approve this claim, please reopen the Closed job then approve again."));
								}

								Charge charge = loginCompanyJob.Charges.AddNew();
								if (line.ChargeCode != null)
								{
									var loginCompanyChargeCode = GetLoginCompanyChargeCode(line.ChargeCode)
										?? throw new InvalidOperationException(
											Res.GetString("F9281059-2009-4102-A3F3-D29CE01F3070", "Charge code '{0}' does not exist in current company, please create charge code before approving claim.", line.ChargeCode.AC_Code));
									charge.JR_AC = loginCompanyChargeCode.PK;
								}
								using (charge.InvoiceSellCurrencyDefaultingSuspender.GetSuspender())
								{
									charge.JR_OH_SellAccount = arCreditNoteDebtor.PK;
									charge.JR_InvoiceType = chargeInvoiceType;
								}
								charge.JR_RX_NKSellCurrency = arCreditNoteCurrency.RX_Code;
								charge.JR_OSSellAmt = -line.AL_OSExTaxAmount; // we don't set the local currency amount here, because we want to use the charge's rate
								charge.JR_OSCostAmt = 0m;
								chargesCollection.Add(charge);
							}
							else
							{
								throw new InvalidOperationException("System can't find a Job for Claim Charges.");
							}
						}

						InvoicingBase arCreditNote = null;
						ChargePoster poster = new ChargePoster(Factory);
						PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(chargesCollection);
						// should only be one 'collection' here
						if (distributedCharges.Count == 1)
						{
							arCreditNote = poster.Post(distributedCharges.GetCharges(arCreditNoteDebtor, arCreditNoteCurrency));
							if (arCreditNote != null)
							{
								AdjustPostedInvoiceHelper.AdjustPostedInvoice(arCreditNote);
								arCreditNote.TransactionNumberSet += new EventHandler(arCreditNote_TransactionNumberSet);
							}
							else
							{
								throw new InvalidOperationException("System can't create AR Credit Note.");
							}
						}
						else
						{
							// system must stop and advise user of problem here
							throw new InvalidOperationException("Multiple AR Credit Notes can't be created during approving. All charges must be posted as in one AR Credit Note.");
						}

						// this is the part that converts the UA credit note to an actual AP credit note
						// not sure if we have to actually login to the company that the credit note was created in ...
						using (Env.SetTemporaryUserContext(RelatedUnapprovedCreditNote.CreatingUserID, RelatedUnapprovedCreditNote.Branch.PK.ToGuid(), RelatedUnapprovedCreditNote.Department.PK.ToGuid()))
						{
							UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
							InvoicingBase approvedCreditNote = converter.ConvertToAPUnsafe(RelatedUnapprovedCreditNote, Factory, false, true);
							approvedCreditNote.AH_Desc = Res.GetString("c0e39f32-96c2-43d2-865a-b9c6d0298a5d", "Approved Credit Note relating to Claim ID {0}", AY_QueryClaimReference);
						}
					}
				}
			}

			AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed;
		}

		void arCreditNote_TransactionNumberSet(object sender, EventArgs e)
		{
			ARCreditNote creditNote = sender as ARCreditNote;
			if (creditNote != null && RelatedUnapprovedCreditNote != null)
			{
				creditNote.AH_PostedInternal = ZBool.True;
				TransactionHeader.AH_TransactionBelongsToGroup = ZGuid.Empty;
				RelatedUnapprovedCreditNote.AH_TransactionBelongsToGroup = ZGuid.Empty;
				RelatedUnapprovedCreditNote.AH_ChequeOrReference = RelatedUnapprovedCreditNote.AH_TransactionNum;

				if (!RelatedUnapprovedCreditNote.HasContext(BusinessContext.PostUnapprovedCreditNoteForApproveClaim))
				{
					RelatedUnapprovedCreditNote.SetContext(BusinessContext.PostUnapprovedCreditNoteForApproveClaim);
				}

				RelatedUnapprovedCreditNote.AH_TransactionNum = ((ARCreditNote)sender).AH_TransactionNum;
			}
		}
		
		#endregion

		public override void Reject()
		{
			base.Reject();

			AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus4RejectedNotClosed;
		}

		protected override AccQueryClaimLookups GetNewLookups()
		{
			return new ARAccQueryClaimLookups(this);
		}

		#endregion

		#region Implementation

		Job GetLoginCompanyJob(JobHeader jobHeader)
		{
			Job result = null;
			if (jobHeader != null)
			{
				ZQuery jobQuery = new ZQuery(JobHeaderSchema.JH_JobNum, jobHeader.JH_JobNum);
				jobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				result = Factory.LoadTop1<Job>(jobQuery);
				if (result != null)
				{
					result.InitializeParentFromGenericJobWithSettingDefaults();
				}
			}
			return result;
		}

		AccChargeCode GetLoginCompanyChargeCode(AccChargeCode chargeCode)
		{
			AccChargeCode result = null;
			if (chargeCode != null)
			{
				ZQuery chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode.AC_Code);
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				result = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
			}
			return result;
		}

		OrgHeader GetARCreditNoteDebtor()
		{
			return TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Branch.Company.OrgProxy;
		}

		ZDecimal GetARCreditNoteExchangeRate(ZString currencyNK)
		{
			ARCreditNote creditNote = Factory.GetCachedReadOnlyFactory().New<ARCreditNote>();
			creditNote.ExchangeRate.Currency = currencyNK;
			if (!currencyNK.IsEmpty && currencyNK != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				creditNote.ExchangeRate.Rate = ExchangeRateReader.GetReaderInstance().GetRate(GlbCompany.CurrentCompany.PK.ToGuid(), Core.Constants.ExchangeRateTypes.Code.BuyRate, currencyNK, ZDateTime.Now.ToDateTime());
				//Note - we use the BUY rate here because jobs also use the 'BUY' rate.
			}
			return creditNote.ExchangeRate.Rate;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ARClaimsAndQueries);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

#if DEBUG

		public void SubstituteAdjustPostedInvoiceHelper_ForTestOnly(IAdjustPostedInvoiceHelper replacement) => adjustPostedInvoiceHelper_constructorInitializedOnly = replacement;
		public IAdjustPostedInvoiceHelper AdjustPostedInvoiceHelper_ExposedForTestOnly => AdjustPostedInvoiceHelper;

		#endif
	}
}
