using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.HotCheque
{
	[CodeProperty(AccHotChequeSchema.Constants.AQ_ChequeNumber)]
	public partial class AccHotCheque : AutoAccHotCheque, Integration.IAccHotCheque, IDocManagerSupport, IDocumentSupportable, IChequeNumberAutoAllocation, IEDocsParsingSupport
	{
		public static string CANCELLED
		{
			get { return Res.GetString("d861d6b0-ec22-4ff0-a43c-5e0a8f397358", "Canceled"); }
		}
		public static string POSTED
		{
			get { return Res.GetString("11f8879a-7c4d-4c2d-b111-29e869b4d14d", "Posted"); }
		}
		public static string ACTIVE
		{
			get { return Res.GetString("8cb71709-69d1-4e2a-8c70-3c1903210647", "Active"); }
		}

		public AccHotCheque(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business object overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AQ_ChequeDate = (ZDateTime)Env.Time.CurrentLocalDate;
			AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Actual;
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				throw new NotSupportedException("You cannot delete Hot Check in Database.");
			}
			else
			{
				base.Delete();
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Property Overrides

		#region AQ_OH

		[List("Creditors")]
		public override ZGuid AQ_OH
		{
			get { return base.AQ_OH; }
			set
			{
				base.AQ_OH = value;
				if (Header != null)
				{
					AQ_ChequePayee = Header.OH_FullNameTruncated;
				}
			}
		}

		protected bool AQ_OH_ReadOnly
		{
			get { return AQ_Printed; }
		}

		#endregion

		#region AQ_AK

		[List("AccChequeBooks")]
		public override ZGuid AQ_AK
		{
			get { return base.AQ_AK; }
			set
			{
				base.AQ_AK = value;
				if (ChequeBook != null)
				{
					if (!IsChequeNumberAutoAllocated)
					{
						AQ_ChequeNumber = ChequeBook.AK_CurrentNo.ToString();
						AccChequeBook.UpdateCurrentNumber(ChequeBook.PK, ChequeBook.AK_CurrentNo++);
					}
				}
				CheckNumberIsAutoAllocated();
			}
		}

		protected bool AQ_AK_ReadOnly
		{
			get { return AQ_Printed; }
		}

		#endregion

		#region AQ_ChequeDate

		protected bool AQ_ChequeDate_ReadOnly
		{
			get { return AQ_Printed; }
		}

		#endregion

		#region AQ_AH

		[List("TransactionHeaders")]
		public override ZGuid AQ_AH
		{
			get { return base.AQ_AH; }
			set
			{
				base.AQ_AH = value;
				if (AQ_AH != ZGuid.Empty) // if (AQ_Calc_ChequeStatus == POSTED)
				{
					SetReadOnlyIncludingChildren(true);
				}
			}
		}
		#endregion

		#region AQ_Cancelled

		public override ZBool AQ_Cancelled
		{
			get { return base.AQ_Cancelled; }
			set
			{
				base.AQ_Cancelled = value;
				if (value) // if (AQ_Calc_ChequeStatus == CANCELLED)
				{
					SetReadOnlyIncludingChildren(true);
				}
			}
		}
		#endregion

		#region AQ_ChequeNumber

		public override ZString AQ_ChequeNumber
		{
			get { return base.AQ_ChequeNumber; }
			set
			{
				if (ChequeBook != null && ChequeBook.BankAccount != null)
				{
					value = Validation.ValidationHelper.PadChequeDigitsWithLeadingZeros(ChequeBook.BankAccount, value);
				}
				base.AQ_ChequeNumber = value;
			}
		}

		protected bool AQ_ChequeNumber_ReadOnly
		{
			get { return IsChequeNumberAutoAllocated || AQ_Printed; }
		}

		internal ZBool IsChequeNumberAutoAllocated
		{
			get { return ChequeBook != null && ChequeBook.IsAutoPrint; }
		}

		#endregion

		#region AQ_ChequePayee

		protected bool AQ_ChequePayee_ReadOnly
		{
			get { return AQ_Printed; }
		}

		#endregion

		#region AQ_Amount

		[DecimalPlaces(nameof(AmountDecimals))]
		public override ZDecimal AQ_Amount
		{
			get { return base.AQ_Amount; }
			set { base.AQ_Amount = value; }
		}

		protected bool AQ_Amount_ReadOnly
		{
			get { return AQ_Printed; }
		}

		#endregion

		#region AQ_GS_NKResponsibleStaff

		protected bool AQ_GS_NKResponsibleStaff_ReadOnly
		{
			get { return AQ_Printed; }
		}

		#endregion

		#region AQ_JH

		[List("JobHeaders")]
		public override ZGuid AQ_JH
		{
			get { return base.AQ_JH; }
			set { base.AQ_JH = value; }
		}

		#endregion

		#endregion

		#region New Bound Properties

		#region AQ_Calc_ActualAmountIndicator

		public ZPropertyInfo AQ_Calc_ActualAmountIndicatorInfo
		{
			get { return GetZPropertyInfo(nameof(AQ_Calc_ActualAmountIndicator)); }
		}

		public ZBool AQ_Calc_ActualAmountIndicator
		{
			get { return AQ_ActualOrMaxIndicator == "ACT"; }
			set
			{
				if (value)
				{
					AQ_ActualOrMaxIndicator = "ACT";
				}
				else
				{
					AQ_ActualOrMaxIndicator = "MAX";
				}
				AQ_Calc_ActualAmountIndicatorInfo.RefreshBinding();
			}
		}

		#endregion

		#region AQ_Calc_MaximumAmountIndicator

		public ZPropertyInfo AQ_Calc_MaximumAmountIndicatorInfo
		{
			get { return GetZPropertyInfo(nameof(AQ_Calc_MaximumAmountIndicator)); }
		}

		public ZBool AQ_Calc_MaximumAmountIndicator
		{
			get { return AQ_ActualOrMaxIndicator == "MAX"; }
			set
			{
				if (value)
				{
					AQ_ActualOrMaxIndicator = "MAX";
				}
				else
				{
					AQ_ActualOrMaxIndicator = "ACT";
				}
				AQ_Calc_MaximumAmountIndicatorInfo.RefreshBinding();
			}
		}

		#endregion

		#region AQ_Calc_RX_NK

		public ZPropertyInfo AQ_Calc_RX_NKInfo
		{
			get { return GetZPropertyInfo(nameof(AQ_Calc_RX_NK)); }
		}

		[MaxLength(3)]
		[List("Currencies")]
		public ZString AQ_Calc_RX_NK
		{
			get
			{
				if (ChequeBook != null && ChequeBook.BankAccount != null)
				{
					return ChequeBook.BankAccount.AB_RX_NKAccountCurrency;
				}
				else
				{
					return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
			}
		}

		#endregion

		#region AmountDecimals

		public int AmountDecimals => Currency?.Decimals ?? LocalDecimals;

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		protected RefCurrency Currency => RefCurrency.LoadFromCurrencyCode(Factory, AQ_Calc_RX_NK);

		#endregion

		#region AQ_Calc_ChequeStatus
		public ZPropertyInfo AQ_Calc_ChequeStatusInfo
		{
			get { return GetZPropertyInfo(nameof(AQ_Calc_ChequeStatus)); }
		}

		public ZString AQ_Calc_ChequeStatus
		{
			get
			{
				ZString status = "";
				if (AQ_Cancelled)
				{
					status = CANCELLED;
				}
				else if (AQ_AH != ZGuid.Empty)
				{
					status = POSTED;
				}
				else
				{
					status = ACTIVE;
				}
				return status;
			}
		}
		#endregion

		#region Creating User and Created Date

		public ZString CreatingUser
		{
			get
			{
				return (CreateLog != null && CreateLog.User != null) ? CreateLog.User.GS_FullName : ZString.Empty;
			}
		}

		public ZPropertyInfo CreatingUserInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CreatingUser));
			}
		}

		public ZDateTime CreatedDate
		{
			get
			{
				return (CreateLog == null) ? ZDateTime.Empty : CreateLog.SL_EventTime;
			}
		}

		public ZPropertyInfo CreatedDateInfo
		{
			get { return GetZPropertyInfo(nameof(CreatedDate)); }
		}

		StmALog fCreateLog;
		protected StmALog CreateLog
		{
			get
			{
				if (fCreateLog == null)
				{
					LoadCreateLog();
				}
				return fCreateLog;
			}
		}

		protected void LoadCreateLog()
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, PK);
			filter.MaximumRows = 1;
			StmALog[] retrievedLogs = (StmALog[])Factory.Load(typeof(StmALog), filter);
			if (retrievedLogs.Length > 0)
			{
				fCreateLog = retrievedLogs[0];
			}
		}

		#endregion

		#region Calc_ChequeNumberIsAutoAllocatedLabel

		public ZString Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated && !IsInDatabase ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		#endregion

		#region Calc_ChequeIsAutoPrintedLabel

		public ZString Calc_ChequeIsAutoPrintedLabel
		{
			get { return IsChequeNumberAutoAllocated && !IsInDatabase ? AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeIsAutoPrintedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeIsAutoPrintedLabel)); }
		}

		#endregion

		#endregion

		#region List Properties

		#region Creditors
		protected CreditorCollection fCreditors;
		public CreditorCollection Creditors
		{
			get
			{
				if (fCreditors == null)
				{
					fCreditors = new CreditorCollection(Factory);
				}
				return fCreditors;
			}
		}
		#endregion

		#region AccChequeBooks

		protected AccChequeBookCollection fAccChequeBooks;
		public AccChequeBookCollection AccChequeBooks
		{
			get
			{
				if (fAccChequeBooks == null)
				{
					// get all cheque books in the current branch
					ZQuery filter = new ZQuery(AccChequeBookSchema.AK_GB, GlbBranch.CurrentBranch.PK);
					fAccChequeBooks = new ActiveChequeBookCollection(Factory, filter);
					fAccChequeBooks.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("a40ca931-e198-46aa-bf62-515977f36e54", "This cheque book cannot be chosen because it is inactive or/and belongs to another Branch. Please choose another cheque book."));
				}
				return fAccChequeBooks;
			}
		}
		#endregion

		#region Currencies

		protected RefCurrencyCollection fCurrencies;
		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}

		#endregion

		#region JobHeaders

		public JobHeaderCollection JobHeaders => jobHeaders ?? (jobHeaders = new JobHeaderCollection(Factory, new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)));
		JobHeaderCollection jobHeaders;

		#endregion

		#region TransactionHeaders

		protected AccTransactionHeaderCollection fTransactionHeaders;
		public AccTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				if (fTransactionHeaders == null)
				{
					fTransactionHeaders = new AccTransactionHeaderCollection(Factory);
				}
				return fTransactionHeaders;
			}
		}

		#endregion

		#endregion

		#region Implementation

		internal AccChequeBookAutoAllocationValidation AutoAllocationValidation
		{
			get
			{
				if (fAutoAllocationValidation == null)
				{
					fAutoAllocationValidation = new AccChequeBookAutoAllocationValidation();
				}
				return fAutoAllocationValidation;
			}
		}
		AccChequeBookAutoAllocationValidation fAutoAllocationValidation;

		void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			UpdatePrintedFlag(ZBool.True, true);
		}

		protected virtual void UpdatePrintedFlag(ZBool value, bool postNow)
		{
			HotChequeWasPrintedOnThisSave = value;
		}

		ZBool HotChequeWasPrintedOnThisSave;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && HotChequeWasPrintedOnThisSave)
			{
				HotChequeWasPrintedOnThisSave = ZBool.False;
				UpdateAQ_PrintedFlagInNewFactory();
			}
		}
		void UpdateAQ_PrintedFlagInNewFactory()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccHotCheque hotCheque = newFactory.Load<AccHotCheque>(PK);
			hotCheque.AQ_Printed = ZBool.True;
			newFactory.Save();
		}

		void CheckNumberIsAutoAllocated()
		{
			Calc_ChequeIsAutoPrintedLabelInfo.RefreshBinding();
			Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
			if (IsChequeNumberAutoAllocated)
			{
				AQ_ChequeNumber = ZString.Empty;
			}
		}

		#endregion

		#region Printing Utilities

		public string GetChequeTemplateName()
		{
			string templateName = "";

			if (!AQ_Cancelled && ChequeBook.AK_AutoPrintCheque && !ChequeBook.BankAccount.AB_SO_ChequeTemplate.IsEmpty)
			{
				StmTemplate template = Factory.Load(typeof(StmTemplate), ChequeBook.BankAccount.AB_SO_ChequeTemplate) as StmTemplate;
				templateName = template.SO_Name;
			}

			return templateName;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.HotCheque);
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

		#region IChequeNumberAutoAllocation Members

		AccChequeBook IChequeNumberAutoAllocation.ChequeBook
		{
			get { return ChequeBook; }
		}

		ZBool IChequeNumberAutoAllocation.IsAutoAllocationEnabled
		{
			get
			{
				return IsChequeNumberAutoAllocated;
			}
		}

		void IChequeNumberAutoAllocation.AssignChequeNumber(string autoGeneratedChequeNumber)
		{
			AQ_ChequeNumber = autoGeneratedChequeNumber;
		}

		ZBool IChequeNumberAutoAllocation.IsAllocationPerformed
		{
			get
			{
				return !AQ_ChequeNumber.IsEmpty;
			}
		}

		Guid IChequeNumberAutoAllocation.Printing_ObjectPK
		{
			get
			{
				return PK.ToGuid();
			}
		}

		ZGuid IChequeNumberAutoAllocation.Printing_PrinterPK
		{
			get
			{
				return (ChequeBook != null) ? ChequeBook.AK_SQ : ZGuid.Empty;
			}
		}

		ZBool IChequeNumberAutoAllocation.ChequeIsAutoPrinted
		{
			get
			{
				return AQ_Printed;
			}
			set
			{
				AQ_Printed = value;
			}
		}

		void IChequeNumberAutoAllocation.AllocationOrPrintingFailed()
		{
			ChequeBook.Reload();
			AQ_ChequeNumber = ZString.Empty;
		}

		#endregion

		#region IDocumentSupportable Members
		public DocumentSupporter DocumentSupporter
		{
			get { return new AccHotChequeDocumentSupporter(this); }
		}
		#endregion

		#region Document Supporter

		public class AccHotChequeDocumentSupporter : DocumentSupporter
		{
			public AccHotChequeDocumentSupporter(AccHotCheque hotCheque)
				: base(hotCheque)
			{
			}

			protected AccHotCheque HotCheque
			{
				get { return (AccHotCheque)BusinessObject; }
			}

			#region IDocumentSupport Members

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.HotCheque; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.HotCheque, HotCheque) };
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.Cheques };
			}

			public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
			{
				return new TitleCopyCountPair(Res.GetString("d9fa96cf-190a-4022-91eb-2dbd23f274e3", "Check"));
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.HotChequeCustomiseDocuments; }
			}

			protected override void InitialiseCore(IDocumentEvents documentEventSource)
			{
				base.InitialiseCore(documentEventSource);
				documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(HotCheque.DocumentEventSource_DocumentPrinted);
			}

			#endregion
		}

		#endregion
	}
}
