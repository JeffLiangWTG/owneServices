using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase
	{
		partial class Schema
		{
			public const string AH_Calc_AmendStatusCode = nameof(AH_Calc_AmendStatusCode);
			public const string ReversalStatusCode = nameof(ReversalStatusCode);
		}

		#region ReversalStatusCode

		[List("Lookups.ReversalStatusCodeList")]
		[ResourceStringData("A1765B71-C0F6-445C-AB06-8735106470FF", Caption = "Reversal Code")]
		[MaxLength(AutoAccTransactionHeaderReference.Schema.AH1_ReferenceMaxLength)]
		public override ZString ReversalStatusCode
		{
			get => ReversalStatusCode_TransactionHeaderReference?.AH1_Reference ?? ZString.Empty;
			set
			{
				if (ReversalStatusCode != value)
				{
					if (!ReversalStatusCodeReferenceType.IsEmpty)
					{
						if (ReversalStatusCode_TransactionHeaderReference == null)
						{
							reversalStatusCode_TransactionHeaderReference = CreateTransactionHeaderReference(ReversalStatusCodeReferenceType);
						}

						ReversalStatusCode_TransactionHeaderReference.AH1_Reference = value;
					}

					if (TransactionReversalValidation != null)
					{
						TransactionReversalValidation.ValidateReversalStatusCode();
					}
				}

				ReversalStatusCodeInfo.RefreshBinding();
			}
		}

		AccTransactionHeaderReference ReversalStatusCode_TransactionHeaderReference
		{
			get
			{
				if (reversalStatusCode_TransactionHeaderReference == null && IsInDatabase)
				{
					reversalStatusCode_TransactionHeaderReference = GetTransactionHeaderReference(ReversalStatusCodeReferenceType);
				}

				return reversalStatusCode_TransactionHeaderReference;
			}
		}
		AccTransactionHeaderReference reversalStatusCode_TransactionHeaderReference;

		#region ITransaction Members

		bool ITransaction.ReversalStatusCode_ReadOnly => !IsReversalStatusCodeAllowed;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => Lookups.ReversalStatusCodeList;

		#endregion

		ZString ReversalStatusCodeReferenceType => ReversalStatusCodeConfiguration.GetReversalStatusCodeReferenceType();

		#endregion

		#region AH_Calc_AmendStatusCode

		[List("Lookups.AmendStatusCodeList")]
		[ResourceStringData("84D5F3F9-B043-4610-BF38-A81337E3F489", Caption = "Amend Status Code")]
		[MaxLength(2)]
		[BusinessObjectTestExclude]
		public ZString AH_Calc_AmendStatusCode
		{
			get => TransactionHeaderReference?.AH1_Reference ?? ZString.Empty;
			set
			{
				if (!AmendStatusCodeReferenceType.IsEmpty)
				{
					if (!value.IsEmpty && TransactionHeaderReference == null)
					{
						transactionHeaderReference = CreateTransactionHeaderReference(AmendStatusCodeReferenceType);
					}

					if (AH_Calc_AmendStatusCode != value)
					{
						CheckMaximumLength(AH_Calc_AmendStatusCodeInfo, value);

						TransactionHeaderReference.AH1_Reference = value;

						if (!IsValidationSuspended && Validation != null)
						{
							if (Validation is InvoiceBaseValidation invoiceBaseValidation)
							{
								invoiceBaseValidation.ValidateAH_Calc_AmendStatusCode();
							}
							else if (Validation is InvoicingBaseReversalValidation invoicingBaseReversalValidation)
							{
								invoicingBaseReversalValidation.ValidateAH_Calc_AmendStatusCode();
							}
						}
					}
				}
				AH_Calc_AmendStatusCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_Calc_AmendStatusCodeInfo => GetZPropertyInfo(Schema.AH_Calc_AmendStatusCode);

		ZString AmendStatusCodeReferenceType
		{
			get
			{
				if (amendStatusCodeReferenceType.IsEmpty)
				{ 
					var amendStatusCodeInstanceProvider = AccountingCountryFactory as IInstanceProvider<IAmendStatusCodeProvider>;
					var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
					amendStatusCodeReferenceType = amendStatusCodeProvider?.AmendStatusCodeReferenceType ?? ZString.Empty;
				}
				return amendStatusCodeReferenceType;
			}
		}
		ZString amendStatusCodeReferenceType;

		AccTransactionHeaderReference TransactionHeaderReference
		{
			get
			{
				if (transactionHeaderReference == null && IsInDatabase)
				{
					transactionHeaderReference = GetTransactionHeaderReference(AmendStatusCodeReferenceType);
				}
				return transactionHeaderReference;
			}
		}
		AccTransactionHeaderReference transactionHeaderReference;

		protected bool AH_Calc_AmendStatusCode_ReadOnly => !IsAllowModifyAmendStatusCode;

		public virtual bool IsAllowModifyAmendStatusCode => false;

		public override ZString AmendStatusCodeAndDescription => AH_Calc_AmendStatusCode.IsEmpty ? ZString.Empty : new ZString(AH_Calc_AmendStatusCode + " - " + Lookups.AmendStatusCodeList.GetDescriptionFromCode(AH_Calc_AmendStatusCode));

		#endregion

		#region DeleteCountrySpecificReferences

		void DeleteCountrySpecificReferences()
		{
			TransactionHeaderReference?.Delete();
			ReversalStatusCode_TransactionHeaderReference?.Delete();
		}

		#endregion

		#region Implementation

		public ZBool IsReversalStatusCodeAllowed => ReversalStatusCodeConfiguration?.GetIsReversalStatusCodeAllowed(Ledger) ?? false;

		IReversalStatusCodeConfiguration ReversalStatusCodeConfiguration => (AccountingCountryFactory as IInstanceProvider<IReversalStatusCodeConfiguration>)?.Get() ?? new GlobalReversalStatusCodeConfiguration();

		IAccountingCountryFactory AccountingCountryFactory => ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Company.GC_RN_NKCountryCode);

		#endregion
	}
}
