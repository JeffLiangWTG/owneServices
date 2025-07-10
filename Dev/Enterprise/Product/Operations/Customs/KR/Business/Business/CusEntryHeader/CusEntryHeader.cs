using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.KR.Business
{
	public partial class CusEntryHeader : AutoKRCusEntryHeader,
		ICusCodeDataTypeSupporter,
		ICurrencyConverterDataProvider,
		ICusEntryNumEntryStatusListProvider,
		IEDIMessageCollectionProviderWithID,
		ISupportMultipleResourceStringData
	{
		public new class Schema : Customs.Business.CusEntryHeader.Schema
		{
			public const string Freight = "Freight";
			public const string Insurance = "Insurance";
			public const string TotalPackages = "TotalPackages";
		}

		public override void Delete()
		{
			base.Delete();
			CustomsOfficers.RemoveAndDeleteAll();
			EntryNumbers.DeleteAll();
		}

		[ChildEditable(true)]
		public CustomsOfficerCollection CustomsOfficers
		{
			get
			{
				if (customsOfficers == null)
				{
					customsOfficers = new CustomsOfficerCollection(this);
					customsOfficers.Load();
					RegisterEditableChildObject(customsOfficers);
				}
				return customsOfficers;
			}
		}
		CustomsOfficerCollection customsOfficers;

		public void AddLog(Event eventType, ZString reference, ZDateTimeOffset? eventTime = null)
		{
			var eventValue = new EventValue(eventType, eventTime: eventTime, reference: reference);
			Logs.AddNew(eventValue);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.CustomsOfficer, typeof(CustomsOfficer) }
			};
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusEntryHeaderFetchStrategy(this);

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		[ChildEditable(true)]
		public CusEntryNumCollection EntryNumbers
		{
			get
			{
				if (entryNumbers == null)
				{
					entryNumbers = new CusEntryNumCollection(this);
					entryNumbers.Load();
					RegisterEditableChildObject(entryNumbers);
				}

				return entryNumbers;
			}
		}
		CusEntryNumCollection entryNumbers;

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.MessageStatusList))]
		public override ZString CH_Status
		{
			get => base.CH_Status;
			set
			{
				var oldValue = CH_Status;
				base.CH_Status = value;
				if (oldValue != CH_Status)
				{
					if (CustomsMessageStatusTypeList.IsMessageAccepted(CH_Status))
					{
						AddLog(Events.MessageAccepted, CH_Status);
						hasBeenLodgedAtCustomsCore = null;
						hasBeenWithdrawn = null;
						SaveHighestSequenceNumbers();
					}
					ManageSnapshots();
				}
			}
		}

		public override ZString EntryNumber
		{
			get
			{
				var entryNumber = CusEntryNumber;
				return entryNumber == null ? ZString.Empty : entryNumber.CE_EntryNum;
			}
			set
			{
				if (EntryNumber != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayEntryNumber();
					}
					else
					{
						CreateCusEntryNumberIfNeeded();
						CusEntryNumber.CE_EntryNum = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateEntryNumber();
					}
				}
				EntryNumberInfo.RefreshBinding();
			}
		}

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			CusEntryNumber result;
			if (IsLocalExport)
			{
				EntryNumbers.Reload(false);
				result = EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == KRJobMessageTypeList.Codes.LocalExport).OrderBy(x => x.CE_EntryLineReference).FirstOrDefault();
			}
			else
			{
				result = base.LoadCusEntryNumber();
			}
			return result;
		}

		protected override CusEntryNumber CreateCusEntryNumber()
		{
			var result = base.CreateCusEntryNumber();
			if (IsLocalExport)
			{
				result.CE_EntryLineReference = (CH_VersionID + 1).ToString();
			}
			return result;
		}

		protected override ZString EntryNumberType => IsLocalExport ? (ZString)Common.KR.KRJobMessageTypeList.Codes.LocalExport : base.EntryNumberType;

		public ZBool IsLocalExport => CH_MessageType == ElectronicDocumentTypeList.Codes._5DP || CH_MessageType == ElectronicDocumentTypeList.Codes._5DQ;
		public ZBool IsMisc => CH_MessageType == ElectronicDocumentTypeList.Codes._008 || CH_MessageType == ElectronicDocumentTypeList.Codes._D87;

		public override void OnSaving()
		{
			base.OnSaving();
			if (EntryNumber.IsEmpty && !IsInDatabase)
			{
				var entryNumberGenerated = ZString.Empty;
				if (IsLocalExport)
				{
					entryNumberGenerated = GenerateEntryNumber(KRJobMessageTypeList.Codes.LocalExport);
				}
				else
				{
					entryNumberGenerated = GenerateEntryNumber(CH_MessageType);
				}
				if (!entryNumberGenerated.IsEmpty)
				{
					EntryNumber = entryNumberGenerated;
				}
			}
		}

		ZString GenerateEntryNumber(string messageType)
		{
			var result = ZString.Empty;
			var uniPassDeclarantID = Declaration?.UNIPASSDeclarantID ?? ZString.Empty;
			var procedureType = Declaration?.JE_ProcedureType ?? ZString.Empty;
			if (!uniPassDeclarantID.IsEmpty)
			{
				var messageInterchange = Enterprise.Core.Constants.CountryCodes.KoreaSouth + messageType;
				var year2digit = ZDate.Today.ToString(Constants.DateFormatType.Year).Substring(2, 2);
				var maxParamValue = NumberFountainMaxValues._6digit;
				var formatDigit = Constants.NumberFormatDigit.D6;
				var checkDigit = ZString.Empty;

				switch (messageType)
				{
					case JobMessageTypeList.Codes.Import:
						checkDigit = GetImpEntryNumberCheckDigit();
						break;
					case ElectronicDocumentTypeList.Codes._5SM:
						checkDigit = Constants.EntryNumberCheckDigit.U;
						maxParamValue = NumberFountainMaxValues._4digit;
						formatDigit = Constants.NumberFormatDigit.D4;
						break;
					case JobMessageTypeList.Codes.Export:
						checkDigit = procedureType == DeclarationProcedureTypeList.Codes.M ? Constants.EntryNumberCheckDigit.R : Constants.EntryNumberCheckDigit.X;
						break;
					case ElectronicDocumentTypeList.Codes._008:
						checkDigit = Constants.EntryNumberCheckDigit.M;
						break;
				}

				var fountainNumber = Env.NumberFountains.KREntryNumberFountain(messageInterchange, uniPassDeclarantID, year2digit, maxParamValue).GetNextFormatted(Factory);
				var formattedNumber = ZInt.ParseSafe(fountainNumber, 0).ToString(formatDigit);

				result = uniPassDeclarantID + year2digit + formattedNumber + checkDigit;
			}
			return result;
		}

		ZString GetImpEntryNumberCheckDigit()
		{
			var procedureType = Declaration?.JE_ProcedureType ?? ZString.Empty;
			var tradeType = Declaration?.JE_TradeType ?? ZString.Empty;
			return DeclarationProcedureTypeCodeList.ImpEntryNumberCheckDigit(procedureType, tradeType);
		}

		public ZString GetEntryNumberForLEXMessage()
		{
			var result = ZString.Empty;
			if (IsLocalExport)
			{
				var latestCusEntryNumber = EntryNumbers.Cast<CusEntryNumber>().MaxBySafe(x => ZInt.ParseSafe(x.CE_EntryLineReference, 0));
				if (ZInt.ParseSafe(latestCusEntryNumber.CE_EntryLineReference, 0) <= CH_VersionID)
				{
					var cusEntryNumberCreated = CreateCusEntryNumber();
					var entryNumberGenerated = GenerateEntryNumber(KRJobMessageTypeList.Codes.LocalExport);
					if (!entryNumberGenerated.IsEmpty)
					{
						result = cusEntryNumberCreated.CE_EntryNum = entryNumberGenerated;
					}
				}
				else
				{
					result = latestCusEntryNumber.CE_EntryNum;
				}
			}
			return result;
		}

		public ZString GetRefundNumber(string customsDisbursementBillNumber)
		{
			var entryNumber5UL = EntryNumbers.Cast<CusEntryNumber>().OrderByDescending(x => x.CE_SystemCreateTimeUtc).FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL && x.CE_EntryLineReference == customsDisbursementBillNumber && x.CE_EntryStatus != CustomsMessageStatusTypeList.Codes.OriginalAccepted);
			if (entryNumber5UL != null && entryNumber5UL.CE_EntryNum.IsEmpty)
			{
				entryNumber5UL.Generate5ULEntryNumber(Branch.GB_GC);
				newRefundNumber = entryNumber5UL.CE_EntryNum;
			}
			return entryNumber5UL?.CE_EntryNum ?? ZString.Empty;
		}
		ZString newRefundNumber;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				EntryNumber = ZString.Empty;
				if (!newRefundNumber.IsEmpty)
				{
					var entryNumber5UL = EntryNumbers.Cast<CusEntryNumber>().Single(x => x.CE_EntryNum == newRefundNumber && x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);
					entryNumber5UL.CE_EntryNum = ZString.Empty;
				}
			}
			newRefundNumber = ZString.Empty;
		}

		protected override bool IsStatusClear(string status) => CustomsMessageStatusTypeList.IsMessageAccepted(status);

		public override bool IsWaitingForResponse
		{
			get
			{
				return CustomsMessageStatusTypeList.IsWaitingForResponse(CH_Status) || EntryNumbers.Cast<CusEntryNumber>().Any(x => CustomsMessageStatusTypeList.IsWaitingForResponse(x.CE_EntryStatus));
			}
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_EntryStatusList))]
		[ResourceStringData("6062B378-B254-4DCA-A2A1-FB309302808A", Caption = "Entry status", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public override ZString CH_EntryStatus
		{
			get => base.CH_EntryStatus;
			set
			{
				if (CH_EntryStatus != value && !IsCESLoggingSuspended)
				{
					AddLog(Events.CustomsEntryStatus, value);
				}
				base.CH_EntryStatus = value;
			}
		}

		public IDisposable SuspendCESLog() => new CESLoggingSuspender(this);

		class CESLoggingSuspender : IDisposable
		{
			public CESLoggingSuspender(CusEntryHeader entry)
			{
				this.entry = entry;
				entry.cesLoggingSuspenderIndex++;
			}
			readonly CusEntryHeader entry;

			public void Dispose()
			{
				entry.cesLoggingSuspenderIndex--;
			}
		}

		byte cesLoggingSuspenderIndex;
		public bool IsCESLoggingSuspended => cesLoggingSuspenderIndex > 0;

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.Parcel, typeof(Parcel) }
			};
			return result;
		}

		public override bool HasBeenLodgedAtCustoms
		{
			get
			{
				if (!hasBeenLodgedAtCustomsCore.HasValue)
				{
					hasBeenLodgedAtCustomsCore = CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CH_Status);
				}
				return hasBeenLodgedAtCustomsCore.Value;
			}
		}
		bool? hasBeenLodgedAtCustomsCore;

		public override bool HasBeenWithdrawn
		{
			get
			{
				if (!hasBeenWithdrawn.HasValue)
				{
					hasBeenWithdrawn = HasMessageAcceptedEvent(CustomsMessageStatusTypeList.Codes.CancellationAccepted);
				}
				return hasBeenWithdrawn.Value;
			}
		}
		bool? hasBeenWithdrawn;

		bool HasMessageAcceptedEvent(string status) => Logs.GetAllLogs().Cast<StmALog>().Any(x => !x.SL_IsCancelled && !x.SL_IsEstimate && x.SL_SE_NKEvent == Events.MessageAcceptedCode && x.SL_Reference == status);

		void SaveHighestFTAEntryLineNumber(ZString messageType)
		{
			var snapshot = Snapshots.GetLatestSnapshotIn(messageType, EntrySnapshotStatus.Current);
			if (snapshot != null)
			{
				var header = snapshot.DataProviderObject as IImportFTAHeader;
				if (header != null && header.EntryLines.Any())
				{
					CH_HighestFTASequenceNumber = (ZShort)header.EntryLines.Select(x => x.SequenceNo).Max();
				}
			}
		}

		void SaveHighestSequenceNumbers()
		{
			var snapshot = Snapshots.GetLatestSnapshotIn(GetDeclarationSnapshotType(), EntrySnapshotStatus.Current);
			if (snapshot != null)
			{
				IEntryHeaderWithEntryLines header = snapshot.DataProviderObject as IEntryHeaderWithEntryLines;
				if (IsImport || IsExport)
				{
					foreach (CusEntryLine entryLine in MergedLines)
					{
						var entryLineInSnapshot = header.EntryLines.FirstOrDefault(x => x.EntryLineNo == entryLine.CL_LineNumber);
						if (entryLineInSnapshot != null)
						{
							entryLine.KR_HighestInvoiceLineSequenceNo = Math.Max(entryLine.KR_HighestInvoiceLineSequenceNo, (ZShort)entryLineInSnapshot.InvoiceLines.Select(x => x.InvoiceLineNo).Max());
						}
					}

					if (IsImport)
					{
						var importEntryHeader = header as ImportEntryHeader;
						if (importEntryHeader != null)
						{
							foreach (CusEntryLine entryLine in MergedLines)
							{
								var entryLineInSnapshot = importEntryHeader.EntryLines.FirstOrDefault(x => Convert.ToInt32(x.EntryLineNo) == entryLine.CL_LineNumber);
								if (entryLineInSnapshot != null)
								{
									entryLine.KR_HighestImmediateDeliveryNo = entryLineInSnapshot.ImmediateDeliveries.Any() ? (ZShort)entryLineInSnapshot.ImmediateDeliveries.Select(x => Convert.ToInt32(x.SequenceNo)).Max() : ZShort.Zero;
									entryLine.KR_HighestNonGADetailNo = entryLineInSnapshot.NonGADetails.Any() ? (ZShort)entryLineInSnapshot.NonGADetails.Select(x => Convert.ToInt32(x.SequenceNo)).Max() : ZShort.Zero;
									entryLine.KR_HighestPreviousExpDecLineNo = entryLineInSnapshot.PreviousExpDecLines.Any() ? (ZShort)entryLineInSnapshot.PreviousExpDecLines.Select(x => Convert.ToInt32(x.SequenceNumber)).Max() : ZShort.Zero;
								}
							}
						}
					}
					else if (IsExport)
					{
						var exportEntryHeader = header as ExportEntryHeader;
						if (exportEntryHeader != null)
						{
							CH_HighestContainerNumber = exportEntryHeader.Containers.Any() ? (ZShort)exportEntryHeader.Containers.Select(x => Convert.ToInt32(x.SequenceNo)).Max() : ZShort.Zero;
							foreach (CusEntryLine entryLine in MergedLines)
							{
								var entryLineInSnapshot = exportEntryHeader.EntryLines.FirstOrDefault(x => Convert.ToInt32(x.EntryLineNo) == entryLine.CL_LineNumber);
								foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
								{
									var invoiceLineInSnapshot = entryLineInSnapshot.InvoiceLines.FirstOrDefault(x => Convert.ToInt32(x.InvoiceLineNo) == invoiceLine.JI_SequenceNumber);
									if (invoiceLineInSnapshot != null)
									{
										invoiceLine.KR_HighestGAApprovalSeqNo = invoiceLineInSnapshot.GAApprovalDocuments.Any() ? (ZShort)invoiceLineInSnapshot.GAApprovalDocuments.Select(x => Convert.ToInt32(x.SequenceNo)).Max() : ZShort.Zero;
										invoiceLine.KR_HighestVehicleSeqNo = invoiceLineInSnapshot.VehicleNumbers.Any() ? (ZShort)invoiceLineInSnapshot.VehicleNumbers.Select(x => Convert.ToInt32(x.SequenceNo)).Max() : ZShort.Zero;
									}
								}
							}
						}
					}
				}
				else if (IsLocalExport)
				{
					var localExportEntryHeader = header as LocalExportEntryHeader;
					if (localExportEntryHeader != null)
					{
						if (CH_MessageType == ElectronicDocumentTypeList.Codes._5DQ)
						{
							CH_HighestTransportMeansNo = localExportEntryHeader.OtherTransportMeans.Any() ? (ZShort)localExportEntryHeader.OtherTransportMeans.Select(x => x.SequenceNo).Max() : ZShort.Zero;
						}
					}
				}
			}
		}

		protected override ZShort MaxLineNumberToAssignAsHighestLineNumber
		{
			get
			{
				ZShort maxNumber = 0;
				var snapshot = Snapshots.GetLatestSnapshotIn(GetDeclarationSnapshotType(), EntrySnapshotStatus.Current);
				if (snapshot != null)
				{
					var entryHeader = snapshot.DataProviderObject as IEntryHeaderWithEntryLines;
					if (entryHeader != null)
					{
						maxNumber = !entryHeader.EntryLines.Any() ? ZShort.Zero : (ZShort)entryHeader.EntryLines.Select(x => x.EntryLineNo).Max();
					}
				}

				return maxNumber;
			}
		}

		void ManageSnapshots()
		{
			if (!CustomsMessageStatusTypeList.IsCancellationStatus(CH_Status))
			{
				if (CustomsMessageStatusTypeList.IsMessageAccepted(CH_Status))
				{
					AccumulativeAmendmentManager.AcceptCurrentSnapshot(this, GetDeclarationSnapshotType());
				}
				else if (CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CH_Status))
				{
					AccumulativeAmendmentManager.DeleteCurrentSnapshot(this, GetDeclarationSnapshotType());
				}
			}
			else if (CustomsMessageStatusTypeList.IsMessageAccepted(CH_Status) && CH_Status != CustomsMessageStatusTypeList.Codes.CancellationAccepted)
			{
				foreach (CusEntrySnapshot snapshot in Snapshots)
				{
					snapshot.CES_Status = EntrySnapshotStatus.Deleted;
				}
				Snapshots.Load();
			}
		}
		ZString GetDeclarationSnapshotType()
		{
			var snapshotType = ZString.Empty;
			if (IsLocalExport)
			{
				snapshotType = CH_MessageType;
			}
			else if (IsExport)
			{
				snapshotType = ElectronicDocumentTypeList.Codes._830;
			}
			else if (IsImport)
			{
				snapshotType = ElectronicDocumentTypeList.Codes._929;
			}
			return snapshotType;
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => Messages;
		CodeDescriptionPairList ICusEntryNumEntryStatusListProvider.EntryStatusList => Factory.GetCachedValue<CustomsMessageStatusTypeList>();
		void ICusEntryNumEntryStatusListProvider.OnEntryStatusSet(ZString entryType, ZString newValue)
		{
			if (Factory.GetCachedValue<CustomsEntryStatusTypeList>().ContainsCode(newValue))
			{
				ErrorReporter.ReportOnce(string.Format("EntryStatus to be set '{0}'.", newValue));
			}

			if (ElectronicDocumentTypeList.GenerateSnapshot(entryType) && ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(entryType))
			{
				if (CustomsMessageStatusTypeList.IsMessageAccepted(newValue))
				{
					SaveHighestFTAEntryLineNumber(entryType);
					AccumulativeAmendmentManager.AcceptCurrentSnapshot(this, entryType);
				}
				else if (CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(newValue))
				{
					AccumulativeAmendmentManager.DeleteCurrentSnapshot(this, entryType);
				}
			}
		}

		public void MarkLodgedSnapshotAsDeleted(ZString entryType)
		{
			AccumulativeAmendmentManager.MarkLodgedSnapshotAsDeleted(this, entryType);
		}

		public ZString FormattedTotalEntryLineCount => MergedLines.Count.ToString("D3");
		public ZString FormattedCargoManagementNo
		{
			get
			{
				if (RandomHeader.JZ_ImportCargoManagementNumber.Length == 15)
				{
					return MessageFunctions.GetFormattedNumber(RandomHeader.JZ_ImportCargoManagementNumber, new int[] { 0, 11 });
				}
				else
				{
					return MessageFunctions.GetFormattedNumber(RandomHeader.JZ_ImportCargoManagementNumber, new int[] { 0, 11, 15 });
				}
			}
		}

		public Bill RelatedBill => Factory.Load<Bill>(RandomHeader.JZ_CU_RelatedHouseBill);

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get
			{
				var result = CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				return result.IsEmpty ? ZDateTime.Today : result;
			}
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => ((ICurrencyConverterDataProvider)Declaration).MaximumDaysToFallback;
		ExchangeRateType ICurrencyConverterDataProvider.RateType => ((ICurrencyConverterDataProvider)Declaration).RateType;
		GlbCompany ICurrencyConverterDataProvider.Company => ((ICurrencyConverterDataProvider)Declaration).Company;
		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride => ((ICurrencyConverterDataProvider)Declaration).LocalCurrencyCodeOverride;
		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride => ((ICurrencyConverterDataProvider)Declaration).IsReciprocalOverride;

		protected override CurrencyConverter CurrencyConverterCore
		{
			get { return currencyConverter ?? (currencyConverter = new CurrencyConverterWithDataProvider(Factory, this)); }
		}
		CurrencyConverter currencyConverter;
		#endregion

		[ResourceStringData("5C45EA84-59A1-421A-A179-F718F41FDF21", Caption = "Entry Number", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("5893E908-E242-4A0F-987C-4E1CBA2F9CB1", Caption = "Entry Number", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("2359FF9A-F28C-4430-A74D-8839D8F4CB6E", Caption = "Entry Number", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ResourceStringData("76582BA4-ABDA-498E-BCF0-993EA7EEA4AA", Caption = "Entry Number", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public ZString FormattedEntryNumber => MessageFunctions.DeclarationNumberFormat(EntryNumber);

		public CusStatementHeader MonthlyDisbursementBill
		{
			get
			{
				if (cusStatement == null)
				{
					var cusStatementQuery = new ZDBOnlyQuery(typeof(CusStatementHeader));
					cusStatementQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementType, StatementHeaderTypeList.Codes.Invoice);

					var cusStatementLineQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
					cusStatementLineQuery.AddToFilter(CusStatementLineSchema.B3_EntryNum, EntryNumber);
					cusStatementLineQuery.AddToFilter(CusStatementLineSchema.B3_EntryType, SharedJobMessageTypeList.Codes.Import);
					cusStatementQuery.AddSubQuery(CusStatementHeaderSchema.PK, CusStatementLineSchema.B3_B2, cusStatementLineQuery, JoinCondition.And);

					cusStatement = Factory.Load<CusStatementHeader>(cusStatementQuery).OrderBy(x => x.B2_SystemCreateTimeUtc).LastOrDefault();
				}
				return cusStatement;
			}
		}
		CusStatementHeader cusStatement;

		public CusStatementHeader Statement929 => statement929 ??= StatementLine929?.StatementHeader;
		CusStatementHeader statement929;
		public ZBool IsValidDueDate => Statement929 != null && Statement929.B2_DueDate.IsValid;

		public CusStatementLine StatementLine929
		{
			get
			{
				if (statementLine929 == null)
				{
					var query = CusStatementLine.Loader.GetQuery(EntryNumber, SharedJobMessageTypeList.Codes.Import, Declaration.JE_GC, new string[] { StatementHeaderTypeList.Codes.Invoice, StatementHeaderTypeList.Codes.CustomsDisbursementBill });
					var statementLines = Factory.Load<CusStatementLine>(query);
					statementLine929 = statementLines.Cast<CusStatementLine>().OrderBy(x => x.StatementHeader.B2_ProcessDate).FirstOrDefault();
				}
				return statementLine929;
			}
		}
		CusStatementLine statementLine929;

		[ResourceStringData("337EB880-4EB9-43AC-A1C3-FD2B2A9CAECD", Caption = "Msg. Status Desc.")]
		public override ZString MessageStatusDescription => base.MessageStatusDescription;

		public override ZString DefaultStatusDescription => Res.GetString("13FED9F4-EBAE-42D2-AF3F-EF4A27336B40", "Unknown");

		[ResourceStringData("F2DF7329-588B-4131-AB12-5B066096D440", Caption = "Total Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("F55BD361-B0BE-4225-B167-156EE3E9B9C0", Caption = "Total Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("3E58C10B-7BCD-4A7C-B4D5-7BC2D7ACCBC5", Caption = "Total Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public override ZDecimal CustomsValue => base.CustomsValue;

		[ResourceStringData("02C5A8F3-3ED0-4171-953E-F3D14A8964F1", Caption = "Total Customs Value (USD)")]
		[ResourceStringData("FC3E1DCD-E186-487F-8A50-A5C5C477FACE", Caption = "Total Customs Value (USD)", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZDecimal CustomsValueUSD
		{
			get
			{
				if (customsValueUSD.IsEmpty)
				{
					var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
					var value = CurrencyConverter.ConvertExact(new Money(CustomsValue, LocalCurrency), usdCurrency).Amount;
					if (value > 0)
					{
						customsValueUSD = Math.Round(value, 0);
					}
				}
				return customsValueUSD;
			}
		}
		ZDecimal customsValueUSD;

		[ResourceStringData("8FAAF2CA-E876-488B-80E0-4DD9EAC48EC9", Caption = "Entry Submitted Date")]
		[ReadOnly(true)]
		public override ZDateTime CH_EntrySubmittedDate
		{
			get => base.CH_EntrySubmittedDate;
			set => base.CH_EntrySubmittedDate = value;
		}

		[ResourceStringData("D6CBF064-C6D4-4F79-866A-0C1DFD0BC59E", Caption = "Cleared Date")]
		[ReadOnly(true)]
		public override ZDateTime CH_EntryReleaseDate
		{
			get => base.CH_EntryReleaseDate;
			set => base.CH_EntryReleaseDate = value;
		}

		[ResourceStringData("FCC2DB18-A645-4605-8836-63118B3647C6", Caption = "Actual Date of Loading", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("CEFE0AD6-9E4F-471D-8D20-1663E0640032", Caption = "Declaration/Loading", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		public ZDateTime KR_ActualDateOfLoading => Declaration.JE_EntryDate;

		[ResourceStringData("92A59495-3B91-4467-8E53-8333DF9EA05B", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate => CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

		[ResourceStringData("52526EEA-717E-4508-896D-0AA01990C4F9", Caption = "Inspection Date", FullDescription = "Preferred Inspection Date")]
		public ZDateTime FixedInspectionDate
		{
			get
			{
				ZDateTime possibleInspectionDate = AcceptedDate.IsEmpty ? ZDateTime.Today : AcceptedDate;
				return possibleInspectionDate.AddYears(-30);
			}
		}

		public bool IsFixedInspectionDateRelevant => IsExport && !Declaration.IsInspectionDateRelevant;

		[ResourceStringData("F960C90D-02FB-4C86-A721-A06848B7249F", Caption = "Due Date of Loading")]
		[ResourceStringData("C8604F93-CAA4-45B8-9DE8-B63DD9D7DF6F", Caption = "Effective To Date", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public ZDateTime DueDateofLoading => CusEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_MessageTypeList))]
		[ResourceStringData("076E851F-BEEB-4535-B145-D7D3960E7E93", Caption = "Message Type")]
		public override ZString CH_MessageType => base.CH_MessageType;

		[ResourceStringData("4E21296D-5C09-42F9-9298-93ED71581DAF", Caption = "Message Type Desc.")]
		public ZString MessageTypeDescription => base.CH_MessageTypeDescription;

		[ResourceStringData("0572F328-E05E-4EF0-9A59-DB439250123E", Caption = "Incoterm")]
		public ZString Incoterm => RandomHeader.JZ_IncoTerm;

		int TotalInvoiceAmountDecimalPlaces => IsImport ? DecimalPlacesConstants.ImportTotalInvoiceAmount : DecimalPlacesConstants.TotalInvoiceAmount;

		[ResourceStringData("48CE7CA7-3F3B-4260-A5E3-905FB156C94F", Caption = "Total Invoice Amount")]
		[DecimalPlaces(nameof(TotalInvoiceAmountDecimalPlaces))]
		public ZDecimal TotalInvoiceAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				if (CH_MessageType == JobMessageTypeList.Codes.Export)
				{
					foreach (var invoice in InvoiceHeaders())
					{
						result += invoice.JZ_InvoiceAmount;
					}
				}
				else if (CH_MessageType == JobMessageTypeList.Codes.Import)
				{
					foreach (var invLine in InvoiceLines)
					{
						result += invLine.JI_LinePrice;
						result += invLine.GetImportTotalInvoiceAmountInLocalCurrency(currencyConverter);
					}
				}
				return result;
			}
		}

		[ResourceStringData("0A7D8479-F1CB-40AD-834D-CD8972DBEA9A", Caption = "Curr.")]
		public ZString InvoiceAmountCurrency => RandomHeader.JZ_RX_NKInvoice_Currency;

		[ResourceStringData("B7DA5A85-761F-4508-B277-34EF7E0B3A4A", Caption = "Additional Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal AdditionalAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var invoiceLine in InvoiceLines)
				{
					result += invoiceLine.GetImportAdditionalAmountInLocalCurrency(CurrencyConverter, invoiceLine.InvoiceHeader.JZ_ValuationCode);
				}
				return result;
			}
		}

		[ResourceStringData("A2AD4D25-2DC9-4444-A1B3-F19840B644CA", Caption = "Deducted Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal DeductedAmount
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var invoiceLine in InvoiceLines)
				{
					result += invoiceLine.GetImportDeductionAmountInLocalCurrency(CurrencyConverter, invoiceLine.InvoiceHeader.JZ_ValuationCode);
				}
				return result;
			}
		}

		[ResourceStringData("E4B8363D-A490-44F9-9B28-70012753532E", Caption = "Total VAT")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalVAT => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.VAT);

		[ResourceStringData("58312210-1643-4A97-AFC3-ED4B1EA98BFA", Caption = "Total Value For VAT")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalValueForVAT => MergedLines.Sum(x => x.CL_ValueForVAT);

		[ResourceStringData("E8F26294-A0FB-4C1A-BC8D-06042B3949B3", Caption = "Total VAT Exemption Value")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalVATExemptionValue => MergedLines.Sum(x => x.CL_ValueExemptForVAT);

		[ResourceStringData("CCAAE5FB-9988-4E17-BB87-6378E27588AB", Caption = "Total Special Consumption Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalSpecialConsumptionTax => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.SpecialConsumptionTax);

		[ResourceStringData("F44BCE45-F7E3-43F3-B27F-E6FF5004D460", Caption = "Total Transportation Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalTransportationTax => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.TransportationTax);

		[ResourceStringData("C9DB30B1-9632-49E7-B09F-24082DEC6032", Caption = "Total Liquor Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalLiquorTax => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.LiquorTax);

		[ResourceStringData("36D76573-36D2-47F0-8801-A1324D40B677", Caption = "Total Education Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalEducationTax => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.EducationTax);

		[ResourceStringData("CE9414B0-8048-46C2-974B-FAF138ABBE64", Caption = "Total Agriculture Tax")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TotalAgricultureTax => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.AgricultureTax);

		[ResourceStringData("AE68D57F-35FE-47A5-8DD4-292775FC4854", Caption = "Penalty For Late Declaration")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal PenaltyForLateDeclaration => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.PenaltyForLateDeclaration);

		[ResourceStringData("42BB0277-3088-4945-9005-4015FAA48CC8", Caption = "Penalty For Missed Declaration")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal PenaltyForMissedDeclaration => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.PenaltyForMissedDeclaration);

		[ResourceStringData("F042CAA4-36A9-4E66-BA74-51CAA9A680BC", Caption = "Freight (KRW)")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal Freight
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var invoiceLine in InvoiceLines)
				{
					result += CH_MessageType == JobMessageTypeList.Codes.Import ? invoiceLine.GetImportFreightInLocalCurrency(CurrencyConverter, invoiceLine.InvoiceHeader.JZ_ValuationCode) : invoiceLine.GetTotalAmountInLocalCurrency(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, CurrencyConverter);
				}
				return result;
			}
		}

		public ZPropertyInfo FreightInfo => GetZPropertyInfo(nameof(Freight));

		[ResourceStringData("660646FC-0E6C-47E5-AAE2-611A8C15E1C2", Caption = "Insurance (KRW)")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal Insurance
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var invoiceLine in InvoiceLines)
				{
					result += CH_MessageType == JobMessageTypeList.Codes.Import ? invoiceLine.GetImportInsuranceInLocalCurrency(CurrencyConverter, invoiceLine.InvoiceHeader.JZ_ValuationCode) : invoiceLine.GetTotalAmountInLocalCurrency(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, CurrencyConverter);
				}
				return result;
			}
		}

		public ZPropertyInfo InsuranceInfo => GetZPropertyInfo(nameof(Insurance));

		[ResourceStringData("BF6368B3-B91B-4F00-BD09-8E960408892F", Caption = "Total Gross Weight (KG)")]
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		public ZDecimal TotalGrossWeightInKG
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (var invoice in InvoiceHeaders())
				{
					var grossWeightInKG = Core.Constants.Weight.Convert(invoice.JZ_Weight, invoice.JZ_WeightUQ, Core.Constants.Weight.Kilograms);
					result += grossWeightInKG;
				}
				return result;
			}
		}

		[ResourceStringData("8E3A8150-C01C-4E3E-9913-8404E166DE69", Caption = "Total Packages")]
		public ZDecimal TotalPackages
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsImport)
				{
					var packQty = EntryInstruction?.CEI_PackQty ?? ZInt.Zero;
					result = (ZDecimal)packQty;
				}
				else
				{
					foreach (var invoice in InvoiceHeaders())
					{
						result += invoice.JZ_NoOfPacks;
					}
				}

				return result;
			}
		}
		public ZPropertyInfo TotalPackagesInfo => GetZPropertyInfo(nameof(TotalPackages));

		[ResourceStringData("F52BB688-4523-40A7-B0B9-EB2B909C7A13", Caption = "Pack Type")]
		public ZString PackagesUQ => Declaration.JE_TotalNoOfPacksPackType;

		[ResourceStringData("190BB4D8-673E-4945-9E8C-77EAD58EBB09", Caption = "Customer Officer")]
		public ZString ResponsibleCustomsOfficer => CustomsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer)?.OrderBy(x => x.CY_Date)?.LastOrDefault()?.CY_Data ?? ZString.Empty;

		[ReadOnly(true)]
		[ResourceStringData("E13B2A5A-0BC0-449A-A2F4-1545E2E2AA00", Caption = "Customs Remarks")]
		[ResourceStringData("E828ED5F-7681-4B91-87CC-7A6C82605CA3", Caption = "Result Reason", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public override ZString CH_CustomsMessageRemarks => base.CH_CustomsMessageRemarks;

		[ResourceStringData("D6FB74E2-6FDF-4383-B074-8CDC671965B1", Caption = "Total Value For VAT")]
		public override ZDecimal ValueForVAT => base.ValueForVAT;

		[ResourceStringData("0DF6A0A7-3334-43FF-BA16-8299BC9FDDFD", Caption = "Customs Confirmation No.", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("0849CCFB-AC63-475D-8821-C302BB14E5DD", Caption = "Approval No.", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public ZString FormattedRefNumber
		{
			get
			{
				switch (CH_MessageType)
				{
					default:
					case ElectronicDocumentTypeList.Codes._5DP:
					case ElectronicDocumentTypeList.Codes._5DQ:
						return CH_BGMReference.Length == 14 ? MessageFunctions.GetFormattedNumber(CH_BGMReference, new int[] { 0, 3, 5, 7, 13 }) : CH_BGMReference;
					case ElectronicDocumentTypeList.Codes._5SM:
						return MessageFunctions.GetFormattedNumber(CH_BGMReference, new int[] { 0, 3, 5, 7 });
				}
			}
		}

		[ResourceStringData("5FF295E2-9FCF-44F1-9421-DA9E08497B58", Caption = "Customs Review Date")]
		[ResourceStringData("6A5C51A9-38E0-4ABC-AAB8-DED9C4234520", Caption = "Approval Date", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public ZDateTime MostRecentCustomsReviewDate => MostRecentCESLog?.SL_EventTime ?? ZDateTime.Empty;

		public StmALog MostRecentCESLog => mostRecentCESLog ?? (mostRecentCESLog = Logs.GetAllLogs().Cast<StmALog>().Where(s => s.SL_SE_NKEvent == Events.CustomsEntryStatus.Code).OrderByDescending(x => x.SL_PostedTimeUtc).FirstOrDefault());
		StmALog mostRecentCESLog;

		[ResourceStringData("B56694B3-8D9F-47C0-93F6-C97F1AC7D83E", Caption = "Ent. Status Desc.")]
		public override ZString EntryHeaderStatusDescription => base.EntryHeaderStatusDescription;

		[ResourceStringData("2B7ACFC6-B62B-4BB6-8C24-00F3E5B93361", Caption = "Total Packages")]
		public ZInt PackageQuantity => EntryInstruction?.CEI_PackQty ?? 0;

		public CusStatementHeader[] IndividualStatements => CustomsDisbursementBills?.Cast<CusStatementHeader>().Where(x => x.B2_StatementType == StatementHeaderTypeList.Codes.CustomsDisbursementBill)?.ToArray();
		bool HasOriginCountryReqDetailedFTADeclaration => InvoiceLines.Any(x => Lookups.CountryOfOriginsReqDHRList.Cast<Universal.ZZRefCusCodeListCombined>().Any(y => x.JI_CountryOfOrigin == y.ZZD_Code));

		[ResourceStringData("16AABDA9-3754-44B1-8488-3BE635F147E0", Caption = "Total Payable Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public override ZDecimal TotalAmountPayable => FormattedTotalDutyAmount + TotalVAT + TotalSpecialConsumptionTax + TotalTransportationTax + TotalLiquorTax + TotalEducationTax + TotalAgricultureTax;

		[ResourceStringData("06E3BE55-DF42-46DF-AE12-76DBD4812C53", Caption = "Total Duty Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal FormattedTotalDutyAmount => this.GetTotalDutyOrTaxAmount(ChargeTypeList.Codes.Duty);

		ZString IEDIMessageCollectionProviderWithID.IDNumber => EntryNumber;
		void IEDIMessageCollectionProviderWithID.MarkAsFailed()
		{
			var failedStatus = CustomsMessageStatusTypeList.GetErrorStatus(CH_Status);
			if (!string.IsNullOrEmpty(failedStatus))
			{
				CH_Status = failedStatus;
			}
		}

		public CusStatementHeaderCollection CustomsDisbursementBills
		{
			get
			{
				if (cusStatements == null)
				{
					var cusStatementQuery = new ZDBOnlyQuery(typeof(CusStatementHeader));
					cusStatementQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementType, StatementHeaderTypeList.EntryDisbursementBillTypes);

					var cusStatementLineQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
					cusStatementLineQuery.AddToFilter(CusStatementLineSchema.B3_EntryNum, EntryNumber);
					cusStatementLineQuery.AddToFilter(CusStatementLineSchema.B3_EntryType, SharedJobMessageTypeList.Codes.Import);
					cusStatementQuery.AddSubQuery(CusStatementHeaderSchema.PK, CusStatementLineSchema.B3_B2, cusStatementLineQuery, JoinCondition.And);

					cusStatements = new CusStatementHeaderCollection(Factory, cusStatementQuery);
					cusStatements.ApplySort(CusStatementHeaderSchema.Constants.B2_ProcessDate, ListSortDirection.Ascending);
				}
				return cusStatements;
			}
		}
		CusStatementHeaderCollection cusStatements;

		public IEnumerable<CusStatementLine> PaidStatementLines => StatementLines.Where(x => x.StatementHeader.B2_PaymentStatus == StatementHeaderPaymentStatusList.Codes.PYC);

		public IEnumerable<CusStatementLine> StatementLines
		{
			get
			{
				var query = CusStatementLine.Loader.GetQuery(EntryNumber, SharedJobMessageTypeList.Codes.Import, Declaration.JE_GC, StatementHeaderTypeList.EntryDisbursementBillTypes);
				return Factory.GetCachedValue($"StatementLines For {EntryNumber}", () => Factory.Load<CusStatementLine>(query).Cast<CusStatementLine>());
			}
		}

		public ExportAmendmentDetailsCollection ExportAmendmentDetailsCollection
		{
			get
			{
				if (IsExport)
				{
					if (exportAmendmentDetailsCollection == null)
					{
						exportAmendmentDetailsCollection = new ExportAmendmentDetailsCollection(this);
					}
				}
				return exportAmendmentDetailsCollection;
			}
		}
		ExportAmendmentDetailsCollection exportAmendmentDetailsCollection;

		public SubsequentMessageDetails SubsequentMessageDetails
		{
			get
			{
				if (IsImport)
				{
					if (subsequentMessageDetails == null)
					{
						subsequentMessageDetails = new SubsequentMessageDetails(this);
					}
				}
				return subsequentMessageDetails;
			}
		}
		SubsequentMessageDetails subsequentMessageDetails;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { Declaration?.JE_MessageType };

		[BusinessObjectTestExclude]
		public LocalExportAmendmentDetailsCollection LocalExportAmendmentDetailsCollection
		{
			get
			{
				if (IsLocalExport)
				{
					if (localExportAmendmentDetailsCollection == null)
					{
						localExportAmendmentDetailsCollection = new LocalExportAmendmentDetailsCollection(this);
					}
				}
				return localExportAmendmentDetailsCollection;
			}
		}
		LocalExportAmendmentDetailsCollection localExportAmendmentDetailsCollection;

		[BusinessObjectTestExclude]
		public ImportAmendmentDetailsCollection ImportAmendmentDetailsCollection
		{
			get
			{
				if (IsImport)
				{
					if (importAmendmentDetailsCollection == null)
					{
						importAmendmentDetailsCollection = new ImportAmendmentDetailsCollection(this);
					}
				}
				return importAmendmentDetailsCollection;
			}
		}
		ImportAmendmentDetailsCollection importAmendmentDetailsCollection;

		public ZString GetOriginalFTAType()
		{
			var result = ZString.Empty;
			var ftaTypes = ElectronicDocumentTypeList.GetOriginalFTAMessageTypes().ToList();
			var snapshot = GetFTASnapshot(ftaTypes, EntrySnapshotStatus.Lodged) ?? GetFTASnapshot(ftaTypes, EntrySnapshotStatus.Current);

			if (snapshot != null)
			{
				result = snapshot.CES_MessageType;
			}
			else
			{
				result = HasOriginCountryReqDetailedFTADeclaration ? ElectronicDocumentTypeList.Codes._DHR : ElectronicDocumentTypeList.Codes._5SC;
			}
			return result;

			CusEntrySnapshot GetFTASnapshot(List<string> ftaTypes, string statusToMatch)
			{
				return Snapshots.Cast<CusEntrySnapshot>().Where(x => ftaTypes.Contains(x.CES_MessageType) && x.CES_Status == statusToMatch).OrderBy(x => x.CES_SystemCreateTimeUtc).LastOrDefault();
			}
		}

		[ResourceStringData("6981A0B5-D5FC-429A-A9A8-70B998E2A4B3", Caption = "Carnet Certificate No.", MultipleKey = ElectronicDocumentTypeList.Codes._D87)]
		public ZString CarnetCertificateNo => Declaration.JE_AgentsReference;

		public ZBool HasAny5FNRejection => EntryNumbers.Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN)?.Any(x => x.CE_EntryStatus == CustomsMessageStatusTypeList.Codes.OriginalRejected) ?? false;

		protected override ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);
	}
}
