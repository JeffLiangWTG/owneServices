using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JCJournalHeader : TransactionHeaderWithLines, IJobCosting
	{
		#region Schema

		public new abstract class Schema : TransactionHeaderWithLines.Schema
		{
			public const string AH_ExchangeRateAmount = "AH_ExchangeRateAmount";
			public const string AH_ExchangeRateCurrencyCode = "AH_ExchangeRateCurrencyCode";
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("5dae7d37-6525-4559-9e8a-4d11ff7b81c2", "Job Costing Journal"); }
		}

		public JCJournalHeader(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetReadOnlyIncludingChildren(true);
		}

		#region Reversing

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);

			foreach (JCJournalLine line in this.Lines)
			{
				JCJournalLine reversingLine = ReversingCFXJNL.Lines.AddNew();
				reversingLine.AL_Desc = line.AL_Desc;
				reversingLine.AL_Sequence = line.AL_Sequence;
				reversingLine.AL_Desc = line.AL_Desc;
				reversingLine.AL_RX_NKTransactionCurrency = line.AL_RX_NKTransactionCurrency;
				reversingLine.AL_ExchangeRate = line.AL_ExchangeRate;
				reversingLine.AL_JH = line.AL_JH;
				reversingLine.AL_AC = line.AL_AC;
				reversingLine.AL_GB = line.AL_GB;
				reversingLine.AL_GE = line.AL_GE;
				reversingLine.AL_AG = line.AL_AG;

				reversingLine.AL_LineAmount = -line.AL_LineAmount;
				reversingLine.AL_OSAmount = -line.AL_OSAmount;
			}
		}

		JCJournalHeader ReversingCFXJNL
		{
			get { return (JCJournalHeader)fReverseTransaction; }
		}

		#endregion

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.JobCosting; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.Journal; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(JCJournalLine); }
		}

		public void SetCFXValues(ZDateTime now, IReceivablesPostingChargeCollection charges)
		{
			AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.JCCFX, Res.GetString("34d46029-e2ae-47f8-84f2-fcd1eb031c27", "Job Costing Journal (CFX)"));
			AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.JCCFX, 0);

			AH_InvoiceDate = now;
			AH_DueDate = now;
			AH_PostDate = now;
			AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AH_ExchangeRate = 1M;

			AH_GB = charges.PostedInvoice.AH_GB;
			AH_GE = charges.InvoicePostingDepartment;
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase)
			{
				AH_TransactionNum = AccountingNumberFountainWrapperFactory.Instance.JCJournal.Generate(this);

				foreach (JCJournalLine line in Lines)
				{
					line.UpdateAL_ReverseDate();
				}
			}
		}

		[List("Departments")]
		public override ZGuid AH_GE
		{
			get { return base.AH_GE; }
			set { base.AH_GE = value; }
		}

		[List("Branches")]
		public override ZGuid AH_GB
		{
			get { return base.AH_GB; }
			set { base.AH_GB = value; }
		}

		#region Lookups

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}
				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		#endregion

		#region Journal Lines

		[ChildEditable(true)]
		public new JCJournalLinesCollection Lines
		{
			get
			{
				return (JCJournalLinesCollection)base.Lines;
			}
		}

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new JCJournalLinesCollection(this, null);
		}

		#endregion

		#endregion

		#region ExchangeRate

		public new ZExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZExchangeRate(this, ExchangeRateType.Buy, AH_ExchangeRateInfo, (ZPropertyInfoString)AH_RX_NKTransactionCurrencyInfo);
				}
				return fExchangeRate;
			}
		}

		ZExchangeRate fExchangeRate;

		#endregion

		public override ZBool CanApplyTaxBranch => false;
	}
}
