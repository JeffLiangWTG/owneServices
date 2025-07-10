using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchHeader : TransactionHeader, ICashBook, IDocManagerSupport, IDirectDebitBatchComponent, IEDocsParsingSupport
	{
		public event EventHandler NoRowsSelectedEvent;
		public event EventHandler GenerateDDRFileEvent;

		public DirectDebitBatchHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fIsSettingLineBatchNo = false;
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_InvoiceAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_OSTotal), ConcurrencyPolicy.Strict);
		}

		#region Binding List

		AccBankAccountCollection fBankList;
		public AccBankAccountCollection BankList
		{
			get
			{
				if (fBankList == null)
				{
					fBankList = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
				}
				return fBankList;
			}
		}

		#endregion

		#region Batch Line Transaction Headers

		DirectDebitBatchLineCollection fLines;

		[ChildEditable]
		public DirectDebitBatchLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new DirectDebitBatchLineCollection(Factory, this);
					RegisterEditableChildObject(fLines);
					if (IsInDatabase)
					{
						fLines.Load();
					}
				}
				return fLines;
			}
		}

		#endregion

		#region Property Overrirdes

		protected bool AH_OSTotalAmount_ReadOnly => true;

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return true; }
		}

		public bool IsRolledUp
		{
			get { return AH_ReceiptType == ReceiptTypes.DirectDebit; }
		}

		protected override bool AH_PostDate_ReadOnly
		{
			get { return false; }
		}

		[List("BankList")]
		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				base.AH_AB = value;
				LoadNewLines();
			}
		}

		public ZGuid RelatedTransactionPK { get; set; }

		public void LoadNewLines()
		{
			AH_OSExTaxAmount = 0m;
			AH_LocalExTaxAmount = 0m;
			AH_ExchangeRate = 1m;

			if (BankAccount != null)
			{
				AH_RX_NKTransactionCurrency = BankAccount.AB_RX_NKAccountCurrency;
				Lines.LoadNewLines(BankAccount);
				if (Lines.Count == 0)
				{
					RaiseNoTransactionFetchedEvent();
				}
				else
				{
					AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(AH_LocalExTaxAmount, AH_OSExTaxAmount);
				}
			}
		}

		public void RaiseNoTransactionFetchedEvent()
		{
			if (NoRowsSelectedEvent != null)
			{
				NoRowsSelectedEvent(this, new EventArgs());
			}
		}

		protected override void SetTransactionBelongsToGroupFieldCore(ZGuid groupingGuidValue)
		{
			// Do not set this field automatically. Leave it blank
		}

		protected override bool NeedToUpdateTransactionNumberFromFountain
		{
			get { return base.NeedToUpdateTransactionNumberFromFountain && !IsReverseTransaction; }
		}

		public override bool UserAllowedToBackPost => base.UserAllowedToBackPost && !IsReversing;

		#endregion

		#region Abstract TransactionHeader Implmentation

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.DDRBatch; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.DDRBatchNo; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("55edc4d3-ab50-4465-a6a5-d9dc4cae914a", "Direct Debit Batch"); }
		}

		#endregion

		#region FileNumber

		public ZInt FileNumber
		{
			get
			{
				if (!fileNumber.HasValue)
				{
					ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_AB, AH_AB);
					query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
					query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
					query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.EqualToDatePartOnly, AH_PostDate.Date);
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.LessThan, AH_TransactionNum);
					fileNumber = Factory.GetDatabaseCount(typeof(AccTransactionHeader), query) + 1;
				}
				return fileNumber.Value;
			}
		}
		ZInt? fileNumber;

		#endregion

		#region SequenceNumberOffset

		public ZString SequenceNumberOffset
		{
			get
			{
				if (!sequenceNumberOffset.HasValue)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
					query.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DirectPayment);
					query.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
					query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					ZDBOnlySubQuery query2 = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_TransactionNum);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_AB, AH_AB);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.EqualToDatePartOnly, AH_PostDate.Date);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.LessThan, AH_TransactionNum);
					query2.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					query.AddSubQuery(AccTransactionHeaderSchema.AH_ReceiptBatchNo, AccTransactionHeaderSchema.AH_TransactionNum, query2, JoinCondition.And);
					sequenceNumberOffset = Factory.GetDatabaseCount(typeof(AccTransactionHeader), query).ToString();
				}
				return sequenceNumberOffset.Value;
			}
		}
		ZString? sequenceNumberOffset;

		#endregion

		#region Base Overrides

		protected override ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			var additionalPropertiesWithStrictConcurrency = new ZPropertyInfo[] { AH_InvoiceAmountInfo, AH_OSTotalInfo };
			return base.GetPropertiesWithStrictConcurrency().Concat(additionalPropertiesWithStrictConcurrency).ToArray();
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase)
			{
				SetReceiptBatchNo();

				if (BankAccount != null && AH_ReceiptType.IsEmpty)
				{
					AH_ReceiptType = GetDDRReceiptType(BankAccount);
				}

				if (AH_ChequeOrReference.IsEmpty)
				{
					AH_ChequeOrReference = AH_TransactionNum;
				}

				if (BankAccount != null && !fIsSettingLineBatchNo)
				{
					SetReceiptTypeAndBatchNo();
				}

				if (IsReverseTransaction)
				{
					AH_PostDate = ((TransactionHeader)OriginalTransaction.ReverseTransaction).AH_PostDate;
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "AH_TransactionNum is currently 8 characters for batching")]
		void SetReceiptBatchNo()
		{
			AH_ReceiptBatchNo = AH_TransactionNum;
		}

		bool fIsSettingLineBatchNo;
		public void SuspendSettingLineBatchNo()
		{
			fIsSettingLineBatchNo = true;
		}

		public void ResumeSettingLineBatchNo()
		{
			fIsSettingLineBatchNo = false;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				this.ReadOnly = true;
				foreach (var line in Lines)
				{
					var tranHeader = line as TransactionHeader;
					if (tranHeader != null)
					{
						tranHeader.NeedToResetWritableProperties = true;
					}
				}
			}

			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				this.Lines.Load();
				if (BankAccount != null && BankAccount.AB_AllowAutoDDR && !AH_IsCancelled)
				{
					RaiseGenerateDDRFileEvent();
				}
			}
		}

		public string GetDDRReceiptType(AccBankAccount bankAccount)
		{
			return bankAccount.AB_ShowDetailsOnDirectDebits ? ReceiptTypes.NonRolledUpBatch : ReceiptTypes.DirectDebit;
		}

		void SetReceiptTypeAndBatchNo()
		{
			foreach (IDirectDebitBatchTransaction line in Lines)
			{
				if (line.IncludeInTheBatch)
				{
					line.AH_ReceiptBatchNo = AH_TransactionNum;

					if (AH_ReceiptType != ReceiptTypes.eNettDirectDebit &&
						!BankAccount.AB_ShowDetailsOnDirectDebits && !PaymentIsBeingCancelled(line))
					{
						line.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
					}
				}
			}
		}

		bool PaymentIsBeingCancelled(IDirectDebitBatchTransaction line)
		{
			bool cancelled = false;
			TransactionHeader paymentToCheck = line as TransactionHeader;
			if (paymentToCheck != null)
			{
				cancelled = paymentToCheck.AH_IsCancelled;
			}
			return cancelled;
		}

		void RaiseGenerateDDRFileEvent()
		{
			if (GenerateDDRFileEvent != null)
			{
				GenerateDDRFileEvent(this, new EventArgs());
			}
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExTaxAmount_ReadOnly
		{
			get { return true; }
			set { base.AH_OSExTaxAmount_ReadOnly = value; }
		}

		#endregion

		#region IReversing Implementation

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			fIsReversing = true;

			((IReversing)this).SetCancellationFlag(true);

			Lines.SetIncludeBatchFlags(false);

			AH_OSExTaxAmount = 0;
			AH_LocalExTaxAmount = 0;
			AH_LocalTaxAmount = 0;
			AH_OSTaxAmount = 0;
			AH_OutstandingAmount = 0;
			AH_DueDate = ZDateTime.Empty;
			foreach (IDirectDebitBatchTransaction line in Lines)
			{
				line.AH_ReceiptBatchNo = "";
				line.AH_ReceiptType = ReceiptTypes.DirectDebit;
			}

			fReverseTransaction = this;
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;
		}

		#endregion

		#region DDR File generation

		public bool CreateFile(string unmappedFilePath, Func<Stream> getTargetStream)
		{
			bool fileAndLogCreatedSuccessfully = false;

			if (!Lines.HasErrors())
			{
				using (var stream = new MemoryStream())
				{
					var utf8NoBom = new UTF8Encoding(false, true);
					using (StreamWriter writer = new StreamWriter(stream, utf8NoBom, 1024, leaveOpen: true))
					{
						var fileCreator = GetDDRFileGenerator(writer, BankAccount.AB_AutoDDRFormat);
						fileCreator.Create();
						fileAndLogCreatedSuccessfully = true;
					}

#if DEBUG
					CreateFile_ErrorAction_ForTestOnly?.Invoke(unmappedFilePath);
#endif

					AddEventLogAndAttachDDRFileToEDocsAndSave(stream, Path.GetFileName(unmappedFilePath));

					using (var targetStream = getTargetStream())
					{
						stream.Position = 0L;
						stream.CopyTo(targetStream);
					}
				}
			}

			return fileAndLogCreatedSuccessfully;
		}

#if DEBUG
		public Action<string> CreateFile_ErrorAction_ForTestOnly;
#endif

		public void AddEventLogAndAttachDDRFileToEDocsAndSave(MemoryStream data, string eDocsFilename)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, FormattableString.Invariant($"DDR File Generated."));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			DocManagerInfo.AddFileOrDocument(data.ToArray(), eDocsFilename, "DDR", false);

			DocManagerInfo.Save();
			Logs.Factory.Save();
		}

		DDRFileGenerator GetDDRFileGenerator(TextWriter writer, string format)
		{
			if (format == Core.Constants.DDRFileFormat.BNZ)
			{
				return new BNZDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.ANZ)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
				{
					return new ANZNewZealandFileGenerator(writer, this);
				}
				else
				{
					return new DDRFileGenerator(writer, this);
				}
			}
			else if (format == Core.Constants.DDRFileFormat.ASB)
			{
				return new ASBDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.BBL)
			{
				return new BBLDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.BCS)
			{
				return new BCSDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.WNZ)
			{
				return new WNZDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.BTM)
			{
				return new YusenDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.HSB)
			{
				return new HSBCDDRFileGenerator(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.CUS)
			{
				throw new NotSupportedException("Custom DDR File must be created from DirectDebitBatchExportAdapter");
			}
			else if (format == Core.Constants.DDRFileFormat.AB1)
			{
				return new ABAFileGeneratorWithDetailLine(writer, this);
			}
			else if (format == Core.Constants.DDRFileFormat.AB2)
			{
				return new ABAFileGenerator(writer, this);
			}
			else
			{
				return new DDRFileGeneratorWithDetailLine(writer, this);
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateBeforePosting();
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			DirectDebitBatchHeaderValidation result = null;
			if (IsValidatingBatchForFileGeneration)
			{
				result = new DirectDebitBatchHeaderValidationForFileGeneration(this);
			}
			else
			{
				result = new DirectDebitBatchHeaderValidation(this);
			}
			return result;
		}

		protected override TransactionHeaderValidation GetNewReversalValidation()
		{
			return new DirectDebitBatchHeaderValidation(this);
		}

		public bool ShouldValidateDirectDebitBatchComponent
		{
			get { return IsValidatingBatchForFileGeneration || !(this.IsInDatabase && this.IsTransactionInDatabaseReadOnly); }
		}

		public void ValidateBeforePosting()
		{
			if (DDRValidation != null)
			{
				DDRValidation.ValidateBeforePosting();
			}
			else if (Validation is TransactionHeaderEmptyValidation)
			{
				Validation.ValidateAll();
			}
		}

		bool IsValidatingBatchForFileGeneration;

		public void ValidateBeforeFileGeneration()
		{
			IsValidatingBatchForFileGeneration = true;
			try
			{
				if (DDRValidation != null)
				{
					DDRValidation.ValidateAH_AB();
					if (!AH_ABInfo.HasErrors())
					{
						DDRValidation.ValidateBatchLines();
					}
				}
				else if (Validation is TransactionHeaderEmptyValidation)
				{
					Validation.ValidateAll();
				}
			}
			finally
			{
				IsValidatingBatchForFileGeneration = false;
			}
		}

		DirectDebitBatchHeaderValidation DDRValidation
		{
			get { return Validation as DirectDebitBatchHeaderValidation; }
		}

		#endregion

		#region Generate DDR Batch from Payment

		public static DirectDebitBatchHeader ReverseDDRBatch(IDirectDebitBatchTransaction originalPayment)
		{
			if (!originalPayment.AH_ReceiptBatchNo.IsEmpty)
			{
				DirectDebitBatchHeader reversedDDRBatch = originalPayment.Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

				reversedDDRBatch.IsReverseTransaction = true;
				reversedDDRBatch.OriginalTransaction = originalPayment as TransactionHeader;
				reversedDDRBatch.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				reversedDDRBatch.AH_PostDate = ZDateTime.Now;
				reversedDDRBatch.AH_InvoiceDate = ZDateTime.Now;
				reversedDDRBatch.AH_FullyPaidDate = ZDateTime.Now;
				reversedDDRBatch.AH_AB = originalPayment.AH_AB;
				reversedDDRBatch.AH_OH = originalPayment.AH_OH;
				reversedDDRBatch.AH_Desc = Res.GetString("664f7382-d9b6-46d6-8fd6-09a60525786b", "Cancellation of {0}", originalPayment.AH_ReceiptBatchNo);
				reversedDDRBatch.AH_OSTotal = -originalPayment.AH_OSTotalAmount;
				reversedDDRBatch.AH_InvoiceAmount = -(originalPayment.AH_LocalExTaxAmount + originalPayment.AH_LocalTaxAmount);
				if (reversedDDRBatch.AH_OSTotal != reversedDDRBatch.AH_InvoiceAmount)
				{
					reversedDDRBatch.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(reversedDDRBatch.AH_InvoiceAmount, reversedDDRBatch.AH_OSTotal);
				}

				if (originalPayment.AH_ReceiptType == ReceiptTypes.DirectDebitLine)
				{
					reversedDDRBatch.AH_ReceiptType = ReceiptTypes.DirectDebit;
				}
				else if (originalPayment.AH_ReceiptType == ReceiptTypes.DirectDebit)
				{
					reversedDDRBatch.AH_ReceiptType = ReceiptTypes.NonRolledUpBatch;
				}
				else if (originalPayment.AH_ReceiptType == ReceiptTypes.eNettDirectDebit)
				{
					reversedDDRBatch.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
				}

				reversedDDRBatch.SuspendSettingLineBatchNo();

				return reversedDDRBatch;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DirectDebitBatchHeaderDocManagerInfo(this, Core.Constants.DocManagerCodes.DirectDebitBatch)); }
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

		#region DocumentSupporter

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DirectDebitBatchHeaderDocumentSupporter(this); }
		}

		class DirectDebitBatchHeaderDocumentSupporter : TransactionHeaderDocumentSupporter
		{
			public DirectDebitBatchHeaderDocumentSupporter(DirectDebitBatchHeader directDebitBatchHeader)
				: base(directDebitBatchHeader)
			{
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.DirectDebitBatch; }
			}
		}

		#endregion
	}
}
