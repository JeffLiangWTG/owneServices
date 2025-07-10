using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	/// <summary>
	/// Summary description for GLJournalLineValidation.
	/// </summary>
	public class GLJournalLineValidation : DependentTransactionLineValidation
	{
		public GLJournalLineValidation(GLJournalLine parent)
			: base(parent)
		{
		}

		new GLJournalLine Parent
		{
			get { return (GLJournalLine)base.Parent; }
		}

		protected override void CheckAL_PostDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckAL_ReverseDateIsValidZDateTimeRange()
		{
		}

		#region CheckAL_AG
		protected override void CheckAL_AG()
		{
			base.CheckAL_AG();

			MandatoryValidation.CheckEntered(Parent.AL_AGInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AL_AGInfo, Parent.GLHeaderCollection);

			if (Parent.GLHeader != null)
			{
				if (Parent.IsNoteJournal)
				{
					if (Parent.GLHeader.AG_AccountType != AccountType.Note)
					{
						Parent.AL_AGInfo.AddError(Res.GetString("f2a7090e-8487-4ba1-b5c0-0450e0ba37a8", "For NJL journal type, you can only post to Note Accounts."));
					}
				}
				else
				{
					if (Parent.GLHeader.AG_AccountType == AccountType.Note)
					{
						Parent.AL_AGInfo.AddError(Res.GetString("e62fb2fb-a851-43f7-aaa1-6633be958f0b", "GL Account with 'NTE' account type cannot be used for the creation of GJL, RJL and AJL journal. Please select another GL Account."));
					}
					else
					{
						var isFCBAdjustmentJournalLine = Parent.GetType() == typeof(FCBAdjustmentJournalLine);
						var balanceSheetAccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
						var pandLAccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

						if (Parent.AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							if (Parent.GLHeader.AG_AccountType != balanceSheetAccountType && Parent.GLHeader.AG_AccountType != pandLAccountType)
							{
								Parent.AL_AGInfo.AddError(isFCBAdjustmentJournalLine ?
									InvalidGLAccountError.ToString() :
									Res.GetString("a140f909-38cb-4e7e-8b6a-c03f5f3e5e7e", "For GJL, RJL and AJL journal types, you can only post to Balance Sheet or Profit & Loss Accounts."));
							}
						}
						else
						{
							if (Parent.GLHeader.AG_ControlAccount || GLJournalLineHelper.IsGlAccountConfiguredAsPlAppropriationAccount(Parent.GLHeader.PK.ToGuid()) || GLJournalLineHelper.IsGlAccountConfiguredAsControlOrLinkAccount(Parent.GLHeader.PK.ToGuid()))
							{
								Parent.AL_AGInfo.AddError(Res.GetString("af2a8e7f-885b-457c-89a7-f49a4085396f", "For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry"));
							}

							if (Parent.GLHeader.AG_AccountType != balanceSheetAccountType)
							{
								if (isFCBAdjustmentJournalLine)
								{
									Parent.AL_AGInfo.AddError(Res.GetString("5D166E71-0885-4904-AC31-273C5618713E", "For a foreign currency line you can only post to a Balance Sheet account"));
								}
								else if (Parent.GLHeader.AG_AccountType != pandLAccountType)
								{
									Parent.AL_AGInfo.AddError(Res.GetString("0D58B749-0B50-4EFE-8FB6-A7C217E07948", "For a foreign currency line you can only post to a Balance Sheet or Profit & Loss account"));
								}
							}
						}
					}
				}

				if (!Parent.GLHeader.AG_IsActive)
				{
					Parent.AL_AGInfo.AddError(InactiveGLAccountError);
				}

				if (Parent.GLHeader.AG_ControlAccount && !Env.Security.GeneralLedgerJournalPostToControlAccounts.IsAllowed)
				{
					string error = Res.GetString("1a2dfc3c-e275-4b74-b345-2c470eaa0421", "This account is flagged as a control account. You do not have security rights to post to control accounts.\r\n{0}", Env.Security.GeneralLedgerJournalPostToControlAccounts.ErrorMessageForNotAllowed);
					Parent.AL_AGInfo.AddError(error);
				}
			}
		}

		#endregion

		#region CheckAL_Desc
		protected override void CheckAL_Desc()
		{
			base.CheckAL_Desc();
			MandatoryValidation.CheckEntered(Parent.AL_DescInfo);
		}
		#endregion

		#region CheckAL_OH

		protected override void CheckAL_OH()
		{
#if DEBUG
			if (Parent.JournalHeader == null)
			{
				return;
			}
#endif
			if (!Parent.AL_OH.IsEmpty)
			{
				if (!Parent.JournalHeader.IsEliminationJournal)
				{
					Parent.AL_OHInfo.AddError(Res.GetString("a20446e9-ffa7-42ef-bb95-14612decc8ae", "Organization can only be set for Elimination Journal."));
				}
				else
				{
					base.CheckAL_OH();

					ZDBOnlySubQuery branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
					branchSubQuery.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, Parent.AL_OH);

					ZDBOnlyQuery companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
					companyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, Parent.AL_OH);
					companyQuery.AddSubQuery(GlbCompanySchema.PK, branchSubQuery, JoinCondition.Or);

					GlbCompany company = Parent.Factory.LoadTop1<GlbCompany>(companyQuery);

					AccConsolidationMember consolidationMember = null;

					if (company == null && Parent.AL_OH.IsValid)
					{
						consolidationMember = Parent.Factory.LoadTop1<AccConsolidationMember>(new ZQuery(Enterprise.ZArchitecture.Schema.AccConsolidationMemberSchema.YM_OH_Organisation, SQLComparisonOperator.Equal, Parent.AL_OH.ToGuid()));
					}

					if (company == null && consolidationMember == null)
					{
						Parent.AL_OHInfo.AddError(Res.GetString("9c5f3ebe-6565-49be-8333-fa87d41f5534", "Organization must be a branch or company proxy or should belong to a consolidation group."));
					}

					var lines =
						from GLJournalLine line in Parent.JournalHeader.Lines
						where line.AL_OH == Parent.AL_OH
						select line;

					ZDecimal sumOfLines = 0m;
					foreach (var line in lines)
					{
						sumOfLines += (ZDecimal)line[GLJournalLine.Schema.AL_OSExTaxAmount];
					}

					if (sumOfLines != 0)
					{
						Parent.AL_OHInfo.AddError(Res.GetString("d2e2d93f-b4a2-46a8-b57d-e5016c60cca5", "When posting elimination journals and setting the notional organization, all postings must balance to zero by organization."));
					}
				}
			}
		}

		#endregion

		#region CheckAL_ExchangeRate

		protected override void CheckAL_ExchangeRate()
		{
			base.CheckAL_ExchangeRate();

			if (Parent.AL_ExchangeRate == 0 && Parent.GLHeader != null)
			{
				var rateTypeCode = AccountingUtils.GetGLJournalExchangeRateType(Parent.AL_GC.ToGuid(), Parent.GLHeader.AG_AccountType);
				var rateTypePair = RefExchangeRateLookups.GetExchangeRateTypeList().ToArray().FirstOrDefault(x => x.Code == rateTypeCode);
				Parent.AL_ExchangeRateInfo.AddWarning(Res.GetString("d38708d7-9fab-484a-a060-b1856098be86", "{0} - {1} exchange rate not found.", rateTypePair.Code, rateTypePair.Description));
			}
			MandatoryValidation.CheckNotNegative(Parent.AL_ExchangeRateInfo);
			MandatoryValidation.CheckNotZero(Parent.AL_ExchangeRateInfo);
		}

		#endregion

		#region CheckAL_RX_NKTransactionCurrency

		protected override void CheckAL_RX_NKTransactionCurrency()
		{
			base.CheckAL_RX_NKTransactionCurrency();

			MandatoryValidation.CheckEntered(Parent.AL_RX_NKTransactionCurrencyInfo);
		}

		#endregion

		#region CheckAL_LocalTaxAmount

		protected override void CheckAL_LocalTaxAmount()
		{
			//Journal lines do not have tax
		}

		#endregion

		#region CheckAL_SupplyType

		protected override void CheckAL_SupplyType()
		{
			//empty validation because GL Journal Line don't have supply type
		}

		#endregion

		#region CheckAL_OSExTaxAmount

		protected override void CheckAL_OSExTaxAmount()
		{
			if (!IsValidateNoteJournal)
			{
				base.CheckAL_OSExTaxAmount();
			}
		}

		#endregion

		#region CheckAL_LocalExTaxAmount

		protected override void CheckAL_LocalExTaxAmount()
		{
			//Journal Lines don't use AL_LocalExTaxAmount in GUI Form, they use UnsignedLocalLineAmount
		}

		#endregion

		#region Calculated Properties Validation

		#region ValidateUnsignedLineAmount

		public void ValidateUnsignedOSLineAmount()
		{
			ValidateCalculatedProperty(Parent.UnsignedOSLineAmountInfo);
		}

		protected virtual void CheckUnsignedOSLineAmount()
		{
			if (!IsValidateNoteJournal)
			{
				MandatoryValidation.CheckEntered(Parent.UnsignedOSLineAmountInfo);
			}
		}

		public void ValidateUnsignedLocalLineAmount()
		{
			ValidateCalculatedProperty(Parent.UnsignedLocalLineAmountInfo);
		}

		protected virtual void CheckUnsignedLocalLineAmount()
		{
			if (!IsValidateNoteJournal)
			{
				MandatoryValidation.CheckEntered(Parent.UnsignedLocalLineAmountInfo);

				if (!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(Parent.AL_RX_NKTransactionCurrency, Parent.UnsignedOSLineAmount, Parent.UnsignedLocalLineAmount))
				{
					Parent.UnsignedLocalLineAmountInfo.AddError(Res.GetString("e0c5fbbb-7679-46c2-bc03-28c3a35c2c61", "The Local Amount should be equal to OS Amount when Local Currency is used"));
				}

				if (!Parent.UnsignedLocalLineAmountInfo.HasErrors())
				{
					GLJournal.AuthorisationRequiredType authorithationRequiredType = Parent.AuthorizationRequiredType;
					if (authorithationRequiredType == GLJournal.AuthorisationRequiredType.AuthorisationRequired)
					{
						Parent.UnsignedLocalLineAmountInfo.AddWarning(Res.GetString("7B7A33B9-562D-408F-92FF-B614171D7222", "The total amount on all lines for this GL account requires approval on posting because it exceeds the registry defined approval threshold, or the registry has been set as 'ANY' that requires all journals to be approved."));
					}
					else if (authorithationRequiredType == GLJournal.AuthorisationRequiredType.HasAuthorisationRights)
					{
						Parent.UnsignedLocalLineAmountInfo.AddWarning(Res.GetString("119d9a5a-4abf-4636-a78c-01e901555581", "This line will be automatically approved when you post because you already have the necessary authorization security right."));
					}
				}
			}
		}

		#endregion

		#region ValidateDebitCreditSign

		public void ValidateDebitCreditSign()
		{
			ValidateCalculatedProperty(Parent.DebitCreditSignInfo);
		}

		protected virtual void CheckDebitCreditSign()
		{
			MandatoryValidation.CheckEntered(Parent.DebitCreditSignInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DebitCreditSignInfo, Parent.DebitCreditSign_List);
		}

		#endregion

		#region Unit Quantity

		public void ValidateUnitQuantity()
		{
			ValidateCalculatedProperty(Parent.UnitQuantityInfo);
		}

		protected void CheckUnitQuantity()
		{
			if (IsValidateNoteJournal)
			{
				MandatoryValidation.CheckEntered(Parent.UnitQuantityInfo);
			}
		}

		bool IsValidateNoteJournal => Parent.IsNoteJournal;

		#endregion

		#endregion

		#region PreSaveValidation

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateUnsignedOSLineAmount();
			ValidateUnsignedLocalLineAmount();
			ValidateDebitCreditSign();
			ValidateUnitQuantity();
		}

		#endregion

		#region Validation Providers

		protected override PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new GLPeriodValidationProvider(Parent.Factory);
		}

		#endregion
	}
}
