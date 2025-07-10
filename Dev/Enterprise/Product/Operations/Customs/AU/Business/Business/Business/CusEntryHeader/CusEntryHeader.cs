using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using D99B = Enterprise.Edifact.D99B;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeader : Customs.Business.CusEntryHeader,
		ICMRMessageRespondee,
		ICMRControlMessageRespondee,
		IStatusNeedsRecalculationProvider,
		ICPQAHeaderAttachee,
		IAddInfo,
		IAddInfoManager,
		ICusCodeDataTypeSupporter,
		IDeclarationChargeProvider,
		IDocumentSupportable,
		IMessageAttachee,
		ICMRMessageRespondeeReference,
		Integration.Customs.AU.ICusEntryHeader,
		ISourceIdentifierProvider
	{
		public class CusEntryComparer : BaseCusEntryComparer
		{
			protected override IComparer GetInvoiceLineComparer() => new JobComInvoiceLine.LineComparer();
		}

		#region TypedLists
		#region BaseList
		public class BaseList
		{
			public readonly ZString Code;
			public readonly ZString Description;
			#region Implementation
			protected BaseList(string code, string description)
			{
				Code = code;
				Description = description;
			}
			#endregion
		}
		#endregion

		#region ACSBarrierStatus
		public class ACSBarrierStatus : BaseList
		{
			public static ACSBarrierStatus Get(ZString code)
			{
				ACSBarrierStatus result;
				switch (code)
				{
					case "Y":
						result = Clear;
						break;
					case "N":
						result = NotClear;
						break;
					case "X":
						result = AwaitingReporting;
						break;
					case "M":
						result = RevertToManual;
						break;
					case "P":
						result = PaperClearanceApplies;
						break;
					case "":
					case " ":
						result = None;
						break;
					default:
						result = new ACSBarrierStatus(code, "UNKNOWN");
						break;
				}
				return result;
			}

			public static readonly ACSBarrierStatus Clear = new ACSBarrierStatus("Y", "Clear");
			public static readonly ACSBarrierStatus NotClear = new ACSBarrierStatus("N", "NOT Barrier Clear (Fully Reported)");
			public static readonly ACSBarrierStatus AwaitingReporting = new ACSBarrierStatus("X", "Awaiting Reporting (Reporting Incomplete)");
			public static readonly ACSBarrierStatus RevertToManual = new ACSBarrierStatus("M", "Revert to manual");
			public static readonly ACSBarrierStatus PaperClearanceApplies = new ACSBarrierStatus("P", "Paper clearance applies");
			public static readonly ACSBarrierStatus None = new ACSBarrierStatus(" ", "None");

			#region Implementation
			protected ACSBarrierStatus(string code, string description) : base(code, description) { }
			#endregion
		}
		#endregion

		#region ACSCommercialStatusList
		public class ACSCommercialStatusList : BaseList
		{
			public static ACSCommercialStatusList Get(ZString code)
			{
				ACSCommercialStatusList result;
				switch (code)
				{
					case "Y":
						result = Clear;
						break;
					case "N":
						result = NotClear;
						break;
					case "P":
						result = ClearButNotPaid;
						break;
					case "Q":
						result = NotClearButPaid;
						break;
					case "":
					case " ":
						result = None;
						break;
					default:
						result = new ACSCommercialStatusList(code, "UNKNOWN");
						break;
				}
				return result;
			}

			public static readonly ACSCommercialStatusList Clear = new ACSCommercialStatusList("Y", "Clear");
			public static readonly ACSCommercialStatusList NotClear = new ACSCommercialStatusList("N", "NOT clear");
			public static readonly ACSCommercialStatusList ClearButNotPaid = new ACSCommercialStatusList("P", "ACS Commercial clear but NOT paid");
			public static readonly ACSCommercialStatusList NotClearButPaid = new ACSCommercialStatusList("Q", "NOT ACS Commercial clear (QM outstanding) but paid");
			public static readonly ACSCommercialStatusList None = new ACSCommercialStatusList(" ", "None");

			#region Implementation
			protected ACSCommercialStatusList(string code, string description) : base(code, description) { }
			#endregion
		}
		#endregion

		#region AQISBarrierStatusList
		public class AQISBarrierStatusList : BaseList
		{
			public static AQISBarrierStatusList Get(ZString code)
			{
				AQISBarrierStatusList result;
				switch (code)
				{
					case "Y":
						result = Clear;
						break;
					case "N":
						result = NotClear;
						break;
					case "X":
						result = AwaitingReporting;
						break;
					case "-":
						result = NotSubjectToQuarantine;
						break;
					case "Q":
						result = SubjectToQuarantine;
						break;
					case "":
					case " ":
						result = None;
						break;
					default:
						result = new AQISBarrierStatusList(code, "UNKNOWN");
						break;
				}
				return result;
			}

			public static readonly AQISBarrierStatusList Clear = new AQISBarrierStatusList("Y", "Clear");
			public static readonly AQISBarrierStatusList NotClear = new AQISBarrierStatusList("N", "NOT Barrier Clear");
			public static readonly AQISBarrierStatusList AwaitingReporting = new AQISBarrierStatusList("X", "Awaiting Reporting (Report Incomplete)");
			public static readonly AQISBarrierStatusList NotSubjectToQuarantine = new AQISBarrierStatusList("-", "Not subject to quarantine");
			public static readonly AQISBarrierStatusList SubjectToQuarantine = new AQISBarrierStatusList("Q", "Subject to Quarantine");
			public static readonly AQISBarrierStatusList None = new AQISBarrierStatusList(" ", "None");

			#region Implementation
			protected AQISBarrierStatusList(string code, string description) : base(code, description) { }
			#endregion
		}
		#endregion

		#region AQISCommercialStatusList
		public class AQISCommercialStatusList : BaseList
		{
			public static AQISCommercialStatusList Get(ZString code)
			{
				AQISCommercialStatusList result;
				switch (code)
				{
					case "Y":
						result = Clear;
						break;
					case "N":
						result = NotClear;
						break;
					case "":
					case " ":
						result = None;
						break;
					default:
						result = new AQISCommercialStatusList(code, "UNKNOWN");
						break;
				}
				return result;
			}

			public static readonly AQISCommercialStatusList Clear = new AQISCommercialStatusList("Y", "Clear");
			public static readonly AQISCommercialStatusList NotClear = new AQISCommercialStatusList("N", "NOT clear");
			public static readonly AQISCommercialStatusList None = new AQISCommercialStatusList(" ", "None");

			#region Implementation
			protected AQISCommercialStatusList(string code, string description) : base(code, description) { }
			#endregion
		}
		#endregion

		#region EntryStatusTransmitList
		public class EntryStatusTransmitList : BaseList
		{
			public static EntryStatusTransmitList Get(ZString code)
			{
				EntryStatusTransmitList result;
				switch (code)
				{
					case "Y":
						result = Transmitted;
						break;
					case "-":
						result = NotTransmitted;
						break;
					case "":
					case " ":
						result = None;
						break;
					default:
						result = new EntryStatusTransmitList(code, "UNKNOWN");
						break;
				}
				return result;
			}

			public static readonly EntryStatusTransmitList Transmitted = new EntryStatusTransmitList("Y", "Status has been transmitted to the relavent CTO or Depot");
			public static readonly EntryStatusTransmitList NotTransmitted = new EntryStatusTransmitList("-", "Status has not been transmitted");
			public static readonly EntryStatusTransmitList None = new EntryStatusTransmitList(" ", "None");

			#region Implementation
			protected EntryStatusTransmitList(string code, string description) : base(code, description) { }
			#endregion
		}
		#endregion

		#region EntryStatusConditionsList
		public class EntryStatusConditionsList : BaseList
		{
			public static EntryStatusConditionsList Get(ZString code)
			{
				if (code.Trim().IsEmpty)
				{
					return None;
				}

				var descriptionBuilder = new StringBuilder();
				foreach (var codeChar in code)
				{
					switch (codeChar)
					{
						case 'I':
							descriptionBuilder.Append("Subject to Imported Food Inspection Service (IFIP); ");
							break;
						case 'M':
							descriptionBuilder.Append("Subject to Motor Vehicle Standards Act (MVSA); ");
							break;
						case 'Q':
							descriptionBuilder.Append("Subject to Quarantine; ");
							break;
						case 'O':
							descriptionBuilder.Append("Query Memo Outstanding; ");
							break;
						case 'Y':
							descriptionBuilder.Append("Clear; ");
							break;
						case ' ':
							break;
						default:
							descriptionBuilder.Append("UNKNOWN; ");
							break;
					}
				}
				return new EntryStatusConditionsList(code.Trim(), descriptionBuilder.ToString().Trim(';', ' '));
			}

			public static readonly EntryStatusConditionsList None = new EntryStatusConditionsList(" ", "None");

			#region Implementation
			protected EntryStatusConditionsList(string code, string description) : base(code, description) { }
			#endregion
		}
		#endregion
		#endregion

		public new class Schema : Customs.Business.CusEntryHeader.Schema
		{
			public const string CustomsFactor = "CustomsFactor";
			public const string DutyAmount = "DutyAmount";
			public const string DutyAmountIncludingWHEstimate = "DutyAmountIncludingWHEstimate";
			public const string GSTAmountIncludingWHEstimate = "GSTAmountIncludingWHEstimate";
			public const string IsPrimeEntry = "IsPrimeEntry";
			public const string IsEnclosureEntry = "IsEnclosureEntry";
			public const string WETAmount = "WETAmount";
			public const string WETAmountIncludingWHEstimate = "WETAmountIncludingWHEstimate";
			public const string LCTAmount = "LCTAmount";
			public const string LCTAmountIncludingWHEstimate = "LCTAmountIncludingWHEstimate";
			public const string TAndI = "TAndI";
			public const string CH_StatusDescription = "CH_StatusDescription";
			public const string ImportEntryAdvice = "ImportEntryAdvice";
			public const string TransportLineStatus = "TransportLineStatus";
			public const string PaymentStatus = "PaymentStatus";
			public const string ATDSecurityCode = "ATDSecurityCode";
			public const string WarehouseNumberOfPacks = "WarehouseNumberOfPacks";
			public const string RefundReasonCode = "RefundReasonCode";
			public const string TotalSecurityConcession = "TotalSecurityConcession";
			public const string TotalSecurityLiability = "TotalSecurityLiability";
			public const string EntryFee = "EntryFee";
			public const string MessageFee = "MessageFee";
			public const string TradegateGST = "TradegateGST";
			public const string OtherEntryCharge = "OtherEntryCharge";
			public const string WoodLevy = "WoodLevy";
			public const string WoodLevyIncludingWHEstimate = "WoodLevyIncludingWHEstimate";
			public const string ScreenFreeCharge = "ScreenFreeCharge";
			public const string AQISServicePaymentAmount = "AQISServicePaymentAmount";
			public const string AQISContainerCharges = "AQISContainerCharges";
			public const string AQISProcessingCharge = "AQISProcessingCharge";
			public const string DeclarationProcessingCharge = "DeclarationProcessingCharge";
			public const string TotalPayableAdmin = "TotalPayableAdmin";
		}

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new IMDStatusCalculator(this);
			paymentStatusCalculator = new EntryPaymentStatusCalculator(this);
		}

		public
#if DEBUG
 virtual
#endif
 new CusEntryLineCollection MergedLines => (CusEntryLineCollection)base.MergedLines;

		[ChildEditable(true)]
		public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

		[ChildEditable(true)]
		public new ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection(this, Factory);

		protected override BillCollectionForEntry GetBillsForEntry() => new CMRBillCollectionForEntry(this);

		public bool HasBeenMergedWithMessageErrors => CH_BGMReference.EndsWith("MESSAGEERRORS");

		protected override Customs.Business.WeightUQCalculator GetWeightCalculator() => new WeightUQCalculator(this);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusEntryHeaderFetchStrategy(this);

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			paymentStatusCalculator.DeriveStatusIfRequired();
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override ZGuid CH_JE
		{
			get => base.CH_JE;
			set
			{
				base.CH_JE = value;
				ResetIsDutyDeferredCachedValue();
			}
		}

		#region AuthorityToDealMessage

		public AuthorityToDeal AuthorityToDealMessage
		{
			get
			{
				if (fAuthorityToDealMessage == null)
				{
					var aTDMessage = Messages.GetLastMessage(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.ATD, EDIInterchange.Direction.Receive);

					if (aTDMessage != null)
					{
						var message = (CMRATDMessage)aTDMessage;
						fAuthorityToDealMessage = message.AuthorityToDeal;
					}
				}

				return fAuthorityToDealMessage;
			}
		}
		AuthorityToDeal fAuthorityToDealMessage;

		#endregion

		#region CMR PAYREC/REFACC Messages
		public CMRPAYRECMessage[] CMRPAYRECMessages
		{
			get
			{
				if (fCMRPAYRECMessages == null)
				{
					var result = Messages.GetMatchingMessages(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, new ZString[] { CMRMessage.CMRMessageTypes.PAYREC }, EDIInterchange.Direction.Receive);
					fCMRPAYRECMessages = new CMRPAYRECMessage[result.Length];
					for (var i = 0; i < result.Length; i++)
					{
						fCMRPAYRECMessages[i] = (CMRPAYRECMessage)result[i];
					}
				}

				return fCMRPAYRECMessages;
			}
		}

		public CMRImportDeclarationMessage[] CMRPAYRECandREFACCMessages
		{
			get
			{
				if (fCMRPAYRECandREFACCMessages == null)
				{
					var result = Messages.GetMatchingMessages(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, new ZString[] { CMRMessage.CMRMessageTypes.PAYREC, CMRMessage.CMRMessageTypes.REFACC }, EDIInterchange.Direction.Receive, false, ListSortDirection.Ascending);
					fCMRPAYRECandREFACCMessages = new CMRImportDeclarationMessage[result.Length];
					for (var i = 0; i < result.Length; i++)
					{
						fCMRPAYRECandREFACCMessages[i] = (CMRImportDeclarationMessage)result[i];
					}
				}

				return fCMRPAYRECandREFACCMessages;
			}
		}

		CMRPAYRECMessage[] fCMRPAYRECMessages;
		CMRImportDeclarationMessage[] fCMRPAYRECandREFACCMessages;
		#endregion

		#region Logging
		public string GetImpediments()
		{
			var resultBuilder = new StringBuilder();
			foreach (StmALog impedimentLog in ImpedimentLogs)
			{
				resultBuilder.Append(impedimentLog.SL_Reference + "\r\n");
			}
			return resultBuilder.ToString();
		}

		public void LogImpediment(string impedimentDescription)
		{
			ImpedimentLogs.AddNew(impedimentDescription);
		}

		protected LogsForNominatedEvent fImpedimentLogs;
		public LogsForNominatedEvent ImpedimentLogs
		{
			get
			{
				if (fImpedimentLogs == null)
				{
					fImpedimentLogs = new LogsForNominatedEvent(Logs, AutoEvents.CustomsImpedimentReceived);
				}
				return fImpedimentLogs;
			}
		}

		protected override Registry.Business.Customs.EntryChargeTypeList GetEntryChargeTypeList()
		{
			return Factory.GetCachedValue<CusEntryChargeTypeList>();
		}

		#endregion

		protected override bool IsStatusChangingFromAmendmentPendingToCleared(ZString oldStatus, ZString newStatus)
			=> oldStatus == CustomsEntryStatus.AwaitingAmendment.Code && newStatus == CustomsEntryStatus.ClearAmendment.Code;

		public ZString WarehouseCCP
		{
			get
			{
				foreach (CusEntryLine line in MergedLines)
				{
					if ((Declaration.IsExWarehouse || line.RandomLine.IsGoingIntoBondedWarehouse) && !line.RandomLine.WarehouseCCP.IsEmpty)
					{
						return line.RandomLine.WarehouseCCP;
					}
				}
				return ZString.Empty;
			}
		}

		public Money CustomsValueInAUD => new Money(CustomsValue, JobDeclaration.GetLocalCurrency());

		public ZDateTime EffectiveDutyDate => (RandomHeader != null && RandomHeader.EffectiveDutyDate.IsValid) ? RandomHeader.EffectiveDutyDate : ZDateTime.Today;

		public Money AccumulatedInvoiceAmount
		{
			get
			{
				if (fAccumulatedInvoiceAmount == null)
				{
					AccumulateCustomsValueAndInvoiceAmount();
				}
				return fAccumulatedInvoiceAmount;
			}
		}

		public Money AccumulatedCustomsValue
		{
			get
			{
				if (fAccumulatedCustomsValue == null)
				{
					AccumulateCustomsValueAndInvoiceAmount();
				}
				return fAccumulatedCustomsValue;
			}
		}

		public ZInt NumberOfOutgoingMessages
		{
			get
			{
				var filter = new ZQuery();
				filter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PK);
				filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				var messages = Factory.Load(typeof(EDIMessage), filter);
				return messages.Length;
			}
		}

		public ICurrency NormalisedInvoiceTotalCurrency
		{
			get
			{
				if (invoiceTotalCurrency == null)
				{
					var result = ZGuid.Empty;

					if (IsInProcessOfMerging)
					{
						ErrorReporter.ReportOnce("InvoiceTotalCurrency in CusEntryHeader", "It is still being merged and should not access this until merge is finished");
					}
					else
					{
						var currencies = CollectAllTheCurrenciesThatAffectITOTToSend();
						if (currencies.Count == 1)
						{
							result = currencies[0];
						}
					}

					var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, JobDeclaration.LocalCurrencyConstantCode);
					if (result.IsEmpty)
					{
						result = localCurrency != null ? localCurrency.PK : ZGuid.Empty;
					}

					invoiceTotalCurrency = Factory.Load<RefCurrency>(result);
				}
				return invoiceTotalCurrency;
			}
		}
		ICurrency invoiceTotalCurrency;

		List<ZGuid> CollectAllTheCurrenciesThatAffectITOTToSend()
		{
			var currencies = new List<ZGuid>();

			foreach (var invoice in InvoiceHeaders)
			{
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, invoice.JZ_RX_NKInvoice_Currency);
				if (currency != null)
				{
					if (!currencies.Contains(currency.PK))
					{
						currencies.Add(currency.PK);

						if (currencies.Count > 1)
						{
							break;
						}
					}
				}

				if (currencies.Count == 0 || currencies.Count == 1)
				{
					var charges = new List<BaseJobComInvHeaderCharge>();

					charges.AddRange(new TypedEnumerable<BaseJobComInvHeaderCharge>(invoice.Charges));
					charges.AddRange(new TypedEnumerable<BaseJobComInvHeaderCharge>(invoice.GroupCharges));

					foreach (var charge in charges)
					{
						if (charge.J7_Amount > 0 && charge.Currency != null && !currencies.Contains(charge.Currency.PK) &&
							(
								charge.J7_IsIncludedInITOT && !charge.J7_IsDutiable ||
								!charge.J7_IsIncludedInITOT && charge.J7_IsDutiable ||
								charge.IsDiscount && !charge.J7_IsDutiable
							))
						{
							currencies.Add(charge.Currency.PK);

							if (currencies.Count > 1)
							{
								break;
							}
						}
					}
				}
			}

			return currencies;
		}

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public ZString ITOTIncoTerm => ChargesProvider.ITOTIncoTerm;

		public bool IsNextMessageOriginalForCMR
			=> CH_Status == ""
					|| CH_Status == CustomsEntryStatus.NotSent.Code
					|| CH_Status == CustomsEntryStatus.FailFormalLodge.Code
					|| CH_Status == CustomsEntryStatus.FailPreLodge.Code
					|| CH_Status == CustomsEntryStatus.FailSAC.Code
					|| CH_Status == CustomsEntryStatus.ClearPreLodge.Code;

		public bool IsSAC => Declaration?.IsSAC ?? false;
		public bool IsTransportModeOther => Declaration?.IsTransportModeOther ?? false;
		public bool IsWeeklySettlement => Declaration?.SettlementTypeSelected ?? false;
		bool IsSOFA => Declaration?.IsSOFADeclaration ?? false;

		bool StatusChangedToClearedSinceLoadingCoreForCMR()
		{
			var result = false;

			if (CH_EntryStatus == CMRImportEntryAdvice.ATDReceived.Code && (ZString)CH_EntryStatusInfo.OriginalValue != CMRImportEntryAdvice.ATDReceived.Code)
			{
				result = true;
			}
			else
			{
				var originalVersionAddInfo = new AUAddInfo(this);
				originalVersionAddInfo.LoadPropertiesFromString((ZString)CH_AddInfoInfo.OriginalValue);

				result = !CMREntryPaymentStatusList.IsClearedStatus(originalVersionAddInfo.ZA_PaymentStatus_Hidden) &&
					CMREntryPaymentStatusList.IsClearedStatus(AddInfo.ZA_PaymentStatus_Hidden);
			}

			return result;
		}

		protected override bool HasBeenLodgedAtCustomsForAccIntegration => CH_EntryStatus == CMRImportEntryAdvice.ATDReceived.Code || CMREntryPaymentStatusList.IsClearedStatus(AddInfo.ZA_PaymentStatus_Hidden);

		protected internal bool IsStatusChangingToClearedInternal(ZString originalStatus, ZString newStatus) => IsStatusChangingToCleared(originalStatus, newStatus);
		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
			=> Declaration.IsImportCMR ? StatusChangedToClearedSinceLoadingCoreForCMR() : !EntryHeaderStatus.IsPostPayStatus(originalStatus) && EntryHeaderStatus.IsPostPayStatus(newStatus);

		protected override bool StatusChangedToHeldSinceLoadingCore() => Declaration.IsImportCMR ? StatusChangedToHeldSinceLoadingCoreForCMR() : base.StatusChangedToHeldSinceLoadingCore();

		bool StatusChangedToHeldSinceLoadingCoreForCMR() => CH_EntryStatus == CMRImportEntryAdvice.Held.Code && (ZString)CH_EntryStatusInfo.OriginalValue != CMRImportEntryAdvice.Held.Code;

		public ZString AgencyBranchIdentifier
		{
			get
			{
				var result = ZString.Empty;
				if (HasBeenLodgedAtCustoms)
				{
					result = GetBranchIdentifierFromLastOriginalMessage();
				}
				if (result.IsEmpty)
				{
					result = Env.Registry.AUCustoms.LocalCustomsBranchIdentifier;
				}

				return result;
			}
		}

		ZString GetBranchIdentifierFromLastOriginalMessage()
		{
			var originalCMRMessagesFilter = new ZQuery();
			originalCMRMessagesFilter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
			originalCMRMessagesFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, CMRMessage.MessageSubTypes.Original);
			originalCMRMessagesFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			originalCMRMessagesFilter.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name;

			return (Messages.Find(originalCMRMessagesFilter).LastOrDefault() as CMRCUSDECMessage)?.BranchIdentifier ?? ZString.Empty;
		}

		public ZDateTime ScheduledPaymentDate
		{
			get => AddInfo.ZA_ScheduledPaymentDate_Hidden;
			set => AddInfo.ZA_ScheduledPaymentDate_Hidden = value;
		}

		public ZDecimal CustomsChargeAmountPayableNow
		{
			get => AddInfo.ZA_CustomsPayNow_Hidden;
			set => AddInfo.ZA_CustomsPayNow_Hidden = value;
		}

		public ZDecimal AQISServicePaymentAmountPayableNow
		{
			get => AddInfo.ZA_AQISPayNow_Hidden;
			set => AddInfo.ZA_AQISPayNow_Hidden = value;
		}

		public ZShort ConsolidatedEntryMemberID
		{
			get => AddInfo.ZA_ConsolidatedEntryMemberID_Hidden;
			set => AddInfo.ZA_ConsolidatedEntryMemberID_Hidden = value;
		}

		#region Nature Types Constants

		public abstract class NatureTypes
		{
			public const string Nature10 = "10";
			public const string Nature20 = "20";
			public const string Nature30 = "30";
		}

		public abstract class NatureTypesForImportCMR
		{
			public const string Nature10 = "N10";
			public const string Nature20 = "N20";
			public const string Nature1020 = "N10/N20";
			public const string Nature30 = "N30";
		}

		#endregion

		#region Nature Type

		public bool IsNature10
		{
			get
			{
				var nature = Nature;
				return nature == NatureTypes.Nature10 || nature == NatureTypesForImportCMR.Nature10;
			}
		}

		public bool IsNature20
		{
			get
			{
				var nature = Nature;
				return nature == NatureTypes.Nature20 || nature == NatureTypesForImportCMR.Nature20;
			}
		}

		public bool IsNature1020
		{
			get
			{
				var nature = Nature;
				return nature == NatureTypesForImportCMR.Nature1020;
			}
		}

		public bool IsNature30
		{
			get
			{
				var nature = Nature;
				return nature == NatureTypes.Nature30 || nature == NatureTypesForImportCMR.Nature30;
			}
		}

		public virtual ZString Nature
		{
			get
			{
				if (Declaration != null)
				{
					if (Declaration.IsImportCMR)
					{
						return GetNatureForImportCMR();
					}
					else
					{
						return GetNatureType();
					}
				}
				else
				{
					return NatureTypes.Nature10;
				}
			}
		}

		#region CMR Nature Flags

		public ZBool IsCMRNature10
		{
			get { return Nature == NatureTypesForImportCMR.Nature10; }
		}

		public ZBool IsCMRNature1020
		{
			get { return Nature == NatureTypesForImportCMR.Nature1020; }
		}

		public ZBool IsCMRNature20
		{
			get { return Nature == NatureTypesForImportCMR.Nature20; }
		}

		public ZBool IsCMRNature30
		{
			get { return Nature == NatureTypesForImportCMR.Nature30; }
		}

		#endregion

		protected ZString GetNatureType()
		{
			if (Declaration != null)
			{
				if (Declaration.JE_MessageType == Common.Shared.SharedJobMessageTypeList.Codes.Import)
				{
					if (MergedLines.Count > 0 && MergedLines[0].RandomLine.JI_IsPackToBondForLine)
					{
						return NatureTypes.Nature20;
					}
					else
					{
						return NatureTypes.Nature10;
					}
				}
				else
				{
					return NatureTypes.Nature30;
				}
			}
			else
			{
				return NatureTypes.Nature10;
			}
		}
		public
#if DEBUG
 virtual
#endif
 ZString GetNatureForImportCMR()
		{
			var result = ZString.Empty;

			var isNature10 = false;
			var isNature20 = false;

			if (Declaration != null)
			{
				if (Declaration.JE_MessageType == Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse)
				{
					result = NatureTypesForImportCMR.Nature30;
				}
				else
				{
					foreach (CusEntryLine entryLine in MergedLines)
					{
						if (entryLine.RandomLine.AddInfo.ZA_IsPackToBondForLine_Hidden == "Y")
						{
							isNature20 = true;
						}
						else
						{
							isNature10 = true;
						}
					}

					if (isNature10 && isNature20)
					{
						result = NatureTypesForImportCMR.Nature1020;
					}
					else if (isNature20)
					{
						result = NatureTypesForImportCMR.Nature20;
					}
					else
					{
						result = NatureTypesForImportCMR.Nature10;
					}
				}
			}
			else
			{
				result = NatureTypesForImportCMR.Nature10;
			}

			return result;
		}

		bool IncludeLandedCostsOnly
		{
			get { return !IsNature20 || Declaration.EstimateDutyAndTaxOnWHEntries; }
		}

		#endregion

		#region Nature Packages

		public ZInt TotalNumberOfPackages => IsNature30 ? WarehouseNumberOfPacksForMessage : PackingGroups.TotalNumberOfPackages;

		public ZInt CMRTotalNumberOfWarehousePackages => PackingGroups.TotalWarehouseNumberOfPackages;

		public ZInt WarehouseNumberOfPacksForMessage
		{
			get
			{
				var result = WarehouseNumberOfPacks;

				if (Declaration != null && Declaration.CustomsEntryHeaders.Count == 1 && result.IsEmpty)
				{
					result = Declaration.JE_TotalNoOfPacks;
				}

				return result;
			}
		}

		public ZInt Nature10Packages => Nature.Contains("10") ? PackageCountForInvoiceHeaders : (ZInt)0;

		public ZInt Nature20Packages => Nature.Contains("20") ? InvoiceHeaders.Cast<JobComInvoiceHeader>().Sum(x => x.JZ_BondPackCount) : 0;

		#endregion

		#region Nature Pieces

		public ZInt Nature10Pieces => Nature.Contains("10") ? InvoiceHeaders.Cast<JobComInvoiceHeader>().Sum(x => x.JZ_PiecesForRelease) : 0;

		public ZInt Nature20Pieces => Nature.Contains("20") ? InvoiceHeaders.Cast<JobComInvoiceHeader>().Sum(x => x.JZ_PiecesToBond) : 0;

		#endregion

		#region Packages

		public ZInt PackageCountForInvoiceHeaders
		{
			get
			{
				ZInt result = 0;
				foreach (var header in InvoiceHeaders)
				{
					result += header.JZ_Nature10PackCount;
				}
				return result;
			}
		}

		public override ZInt PackagesCount
		{
			get
			{
				ZInt result = 0;

				if (Declaration.IsImportCMR)
				{
					result = Declaration.PackingGroups.TotalNumberOfPackages;
				}
				else
				{
					result = Nature10Packages + Nature20Packages;
				}

				return result;
			}
		}

		#endregion

		#region ErrorLineNumbersFromLastResponseMessage
		protected override short[] ErrorLineNumbersFromLastResponseMessage()
		{
			var errors = new ArrayList();
			var mostRecentMessage = Messages.LastMessage;

			if (mostRecentMessage != null && mostRecentMessage.EM_ReceiveTransmit == EDIInterchange.Direction.Receive)
			{
				var cMRResponseMessage = mostRecentMessage.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new UNOCCMRCharacterSet()) as D99B.Messages.CUSRES.CUSRESMessage;
				if (cMRResponseMessage != null)
				{
					var cmrCUSRESMessage = Factory.Load<CMRCUSRESMessage>(mostRecentMessage.PK);
					errors = ExtractCMRLinesInError(cmrCUSRESMessage, cMRResponseMessage);
				}
			}
			return (short[])errors.ToArray(typeof(short));
		}

		ArrayList ExtractCMRLinesInError(CMRCUSRESMessage cmrCUSRESMessage, D99B.Messages.CUSRES.CUSRESMessage responseMessage)
		{
			var errors = new ArrayList();
			if (cmrCUSRESMessage != null)
			{
				var statusType = cmrCUSRESMessage.GetStatus();
				if (statusType.Contains("REJECTED"))
				{
					foreach (D99B.Messages.CUSRES.SegmentGroup4 group4 in responseMessage.Group4)
					{
						var lineAsString = group4.ERP[0].ErrorPointDetails.MessageSubItemNumber;
						var lineNumber = short.Parse(lineAsString);

						if (!(errors.Contains(lineNumber)))
						{
							errors.Add(lineNumber);
						}
					}
				}
			}
			return errors;
		}

		#endregion

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public bool HasDeferredGST => MergedLines.Cast<CusEntryLine>().Any(x => x.GSTVATDeferred > 0);

		public bool HasGST => MergedLines.Cast<CusEntryLine>().Any(x => x.GSTVATAmount > 0);

		#region ICusEntryLine Arrays

		internal ICusEntryLine[] AmendedLines
		{
			get
			{
				if (fAmendedLines == null)
				{
					var result = new ArrayList();
					result.AddRange(MergedLines);
					result.AddRange(PendingDeletedLinesForAmendment);
					result.AddRange(DeletedLinesWithRefundReasonCode);
					fAmendedLines = (ICusEntryLine[])result.ToArray(typeof(ICusEntryLine));
				}
				return fAmendedLines;
			}
		}
		ICusEntryLine[] fAmendedLines;

		internal ICusEntryLine[] PendingDeletedLinesForAmendment
		{
			get
			{
				var result = new List<ICusEntryLine>();
				foreach (ICusEntryLine line in PendingDeletionEntryLines)
				{
					result.Add(new DeletedLineAmendment(line, this));
				}
				return result.ToArray();
			}
		}

		internal ICusEntryLine[] DeletedLinesWithRefundReasonCode
		{
			get
			{
				var result = new List<ICusEntryLine>();
				foreach (ICusEntryLine line in DeletedEntryLines)
				{
					if (!line.RefundReasonCode.IsEmpty)
					{
						result.Add(new DeletedLineAmendment(line, this));
					}
				}
				return result.ToArray();
			}
		}

		#endregion

		#region IPackingGroup Array

		IEnumerable<IPackingGroup> DeletedHouseBillContainerPacks => DeletedPackingGroups.Cast<DeletedPackingGroupData>().Select(data => new DeletedPackingGroupAmendment(data.Number));

		public IPackingGroup[] AllHouseBillContainerPacks => (IPackingGroup[])PackingGroups.ToArray(typeof(IPackingGroup));

		public IPackingGroup[] AmendedHouseBillContainerPacks => fAmendedHouseBillContainerPacks ?? (fAmendedHouseBillContainerPacks = AllHouseBillContainerPacks.Concat(DeletedHouseBillContainerPacks).OrderBy(pack => pack.HouseContainerNumber).ToArray());
		IPackingGroup[] fAmendedHouseBillContainerPacks;

		#endregion

		#region PackingGroups

		public PackingGroupCollection PackingGroups => Factory.GetValue(ref packingGroups, GetPackingGroups);
		CachedProperty<PackingGroupCollection> packingGroups;

		PackingGroupCollection GetPackingGroups()
		{
			PackingGroupCollection result = null;

			var declaration = Declaration;
			if (declaration != null)
			{
				if (declaration.CustomsEntryHeaders.Count > 1)
				{
					result = new PackingGroupCollection(declaration);

					var relatedHouseBills = InvoiceHeaders.Select(c => c.JZ_CU_RelatedHouseBill).ToArray();
					if (relatedHouseBills.Length > 0)
					{
						var query = new ZQuery(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, relatedHouseBills);
						result.LoadWithMoreFiltering(query);
					}
				}
				else
				{
					result = Declaration.PackingGroups;
				}

				result.Sort(new PackingGroupLineNumberComparer());
			}

			return result;
		}

		internal DeletedPackingGroupCollection DeletedPackingGroups
		{
			get
			{
				if (deletedPackingGroups == null)
				{
					deletedPackingGroups = new DeletedPackingGroupCollection(this);
					deletedPackingGroups.Load();
				}
				return deletedPackingGroups;
			}
		}
		DeletedPackingGroupCollection deletedPackingGroups;

		public ZShort HighHouseContPivotNo => (ZShort)AddInfo.ZA_HighHouseContPivotNo_Hidden;

		public HighHouseContPivotNoManager HighHouseContPivotNoManager => highHouseContPivotNoManager ?? (highHouseContPivotNoManager = new HighHouseContPivotNoManager(this));
		HighHouseContPivotNoManager highHouseContPivotNoManager;

		#endregion

		#region Customs Factor

		[DecimalPlaces(8)]
		public ZDecimal CustomsFactor
		{
			get
			{
				ZDecimal result = 1;    // Default value

				if (!NormalisationConditionChecker.ShouldEntryBeNormalised || AccumulatedInvoiceAmount.Currency.Code != JobDeclaration.GetLocalCurrency().RX_Code)//Normalised --> Customs Factor 1
				{
					if (AccumulatedInvoiceAmount.Amount != 0)
					{
						var factor = AccumulatedCustomsValue.Amount / AccumulatedInvoiceAmount.Amount;
						decimal exchangeRate = CurrencyConverter.GetExchangeRate(AccumulatedInvoiceAmount.Currency);
						if (exchangeRate != 0)
						{
							result = decimal.Round(factor / exchangeRate, 8);
						}
					}
				}

				return result;
			}
		}

		public ZPropertyInfo CustomsFactorInfo => GetZPropertyInfo(Schema.CustomsFactor);

		#endregion

		#region Total Amount Payable

		public override ZDecimal TotalAmountPayable
		{
			get
			{
				var result = CH_TotalPaid;
				if (result.IsEmpty || IsWeeklySettlement)
				{
					if (Declaration != null && Declaration.IsImportCMR)
					{
						result = AllEntryFees + WoodLevy + MergedLines.TotalLinePayable;
						if (IsDutyDeferred)
						{
							result -= UseCustomsValues ? TotalDeferredDutyFromCustoms : EstimatedDeferredDutyAndCharges;
						}
					}
					else
					{
						result = EntryFee + MessageFee + TradegateGST + OtherEntryCharge + DutyAmount + LCTAmount + WETAmount + WoodLevy + GSTAmount;
					}
				}
				return result;
			}
		}

		public Money TotalAmountPayableMoney => new Money(TotalAmountPayable, JobDeclaration.GetLocalCurrency());

		internal ZDecimal CalculatedNetTotalLinesAmountDue
		{
			get
			{
				if (!fCalculatedNetTotalLinesAmountDueDone)
				{
					fCalculatedNetTotalLinesAmountDue = 0;
					var lines = MergedLines.Concat(PendingDeletionEntryLines);
					foreach (var line in lines.Cast<CusEntryLine>())
					{
						fCalculatedNetTotalLinesAmountDue += line.CurrentTotalDutyTax - line.TotalDutyTaxAdvisedInLastClearanceMessage;
					}
					fCalculatedNetTotalLinesAmountDueDone = true;
				}
				return fCalculatedNetTotalLinesAmountDue;
			}
		}
		ZDecimal fCalculatedNetTotalLinesAmountDue;
		ZBool fCalculatedNetTotalLinesAmountDueDone;

		public
#if DEBUG
 virtual
#endif
 ZBool IsARefundDue => CalculatedNetTotalLinesAmountDue < -Env.Registry.AUCustomsRefundToleranceAmount;

		#endregion

		#region Duty Amount

		public ZDecimal DutyAmount => TotalDutyAmount;

		public ZPropertyInfo DutyAmountInfo => GetZPropertyInfo(Schema.DutyAmount);

		public ZDecimal DutyAmountIncludingWHEstimate => MergedLines.Cast<CusEntryLine>().Sum(x => x.DutyAmountIncludingWHEstimate);

		public ZPropertyInfo DutyAmountIncludingWHEstimateInfo => GetZPropertyInfo(Schema.DutyAmountIncludingWHEstimate);

		public ZDecimal FlatDutyPortion => MergedLines.Cast<CusEntryLine>().Sum(x => x.FlatDutyPortion);

		public ZDecimal AllOtherDuties => MergedLines.Cast<CusEntryLine>().Sum(x => x.AllOtherDuty);

		public ZDecimal PayableDuty => DutyAmount - DeferredDuty;

		public ZDecimal DeferredDuty
		{
			get
			{
				var result = decimal.Zero;
				if (IsDutyDeferred)
				{
					if (UseCustomsValues)
					{
						if (TotalDeferredDutyFromCustoms > 0m)
						{
							result = TotalDeferredDutyFromCustoms - DeferrableFeesAndCharges;
						}
						else
						{
							result = DutyAmount;
						}
					}
					else
					{
						result = DeferrableDuty;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalDeferredDutyFromCustoms => Charges.GetAmount(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount); // Not always from Customs - is also set by Merge

		public ZDecimal EstimatedDeferredDutyAndCharges => DeferrableDuty + DeferrableFeesAndCharges;

		public ZDecimal DeferrableDuty => IsDutyDeferred ? MergedLines.Cast<CusEntryLine>().Where(x => !x.IsExciseEquivalentGoods).Sum(x => x.DutyAmount) : 0m;

		ZDecimal DeferrableFeesAndCharges
		{
			get
			{
				return Factory.GetCachedValue("AU.CusEntryHeader.DeferrableFeesAndCharges|" + PK, () =>
				{
					var result = ZDecimal.Zero;

					foreach (var charge in Charges)
					{
						if (charge.C1_ChargeType != Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount && charge.IsDeferrable)
						{
							result += charge.C1_ChargeAmount;
						}
					}

					foreach (CusEntryLine entryLine in MergedLines)
					{
						if (!entryLine.IsExciseEquivalentGoods)
						{
							result += entryLine.DeferrableFeesAndCharges;
						}
					}

					return result;
				}, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		#region IsDutyDeferred

		public bool IsDutyDeferred
			=> Factory.GetCachedValue(IsCalculatingDuty ? IsDutyDeferredForCalcCacheKey : IsDutyDeferredCacheKey,
				() =>
					{
						return UseCustomsValues ? TotalDeferredDutyFromCustoms > 0m : (bool)(Declaration?.Consignee?.AUIsDutyDeferred ?? false);
					},
				CacheStalenessPolicy.StaleOnFactorySave);

		string IsDutyDeferredCacheKey => isDutyDeferredCacheKey ?? (isDutyDeferredCacheKey = "AU.CusEntryHeader.IsDutyDeferred|" + PK);
		string isDutyDeferredCacheKey;

		string IsDutyDeferredForCalcCacheKey => IsDutyDeferredCacheKey + "_CALC";

		void ResetIsDutyDeferredCachedValue()
		{
			Factory.ClearCachedValue<ZBool>(IsDutyDeferredCacheKey);
			Factory.ClearCachedValue<ZBool>(IsDutyDeferredForCalcCacheKey);
		}

		#endregion

		#region UseCustomsValues

		bool UseCustomsValues => !IsCalculatingDuty && HasValidIMDR;

		public bool IsCalculatingDuty { get; set; }

		bool HasValidIMDR
		{
			get
			{
				return Factory.GetCachedValue("AU.CusEntryHeader.HasValidIMDR|" + PK, () =>
				{
					var messages = ((IStatusNeedsRecalculationProvider)this).Messages;
					var imdrMessage = messages.OfType<CMRIMDRMessage>()
											  .Where(msg => msg.EM_ReceiveTransmit == EDIInterchange.Direction.Receive)
											  .OrderBy(msg => msg.EM_SystemCreateTimeUtc)
											  .LastOrDefault();
					return imdrMessage != null && imdrMessage.EM_Status == EDIMessageStatusList.Codes.Received;
				}, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		#endregion
		#endregion

		#region GST Amount

		public virtual ZDecimal GSTAmountIncludingWHEstimate => MergedLines.Cast<CusEntryLine>().Sum(x => x.GSTVATAmountIncludingWHEstimate);

		public virtual ZPropertyInfo GSTAmountIncludingWHEstimateInfo => GetZPropertyInfo(Schema.GSTAmountIncludingWHEstimate);

		#endregion

		#region Other Charges

		public ZDecimal PayableOtherCMRCharges
		{
			get
			{
				var result = OtherCMRCharges;
				if (IsDutyDeferred)
				{
					result -= (EntryFee + WoodLevy + AQISProcessingCharge);
				}
				return result;
			}
		}

		public ZDecimal OtherCMRCharges
			=> Convert.ToDecimal(EntryFee
					+ MessageFee + TradegateGST + ScreenFreeCharge
					+ WoodLevy + OtherEntryCharge + AQISProcessingCharge
					+ TotalPayableAdmin + AQISContainerCharges);

		#endregion

		public ZDecimal CountervailingDuty => MergedLines.Cast<CusEntryLine>().Sum(x => x.CountervailingDuty);

		public ZDecimal DumpingDuty => MergedLines.Cast<CusEntryLine>().Sum(x => x.DumpingDuty);

		#region LCT Amount

		public ZDecimal PayableLCT => IsDutyDeferred ? ZDecimal.Zero : LCTAmount;

		public ZDecimal LCTAmount => MergedLines.Cast<CusEntryLine>().Sum(x => x.LCTAmount);

		public ZPropertyInfo LCTAmountInfo => GetZPropertyInfo(Schema.LCTAmount);

		public ZDecimal LCTAmountIncludingWHEstimate => MergedLines.Cast<CusEntryLine>().Sum(x => x.LCTAmountIncludingWHEstimate);

		public ZPropertyInfo LCTAmountIncludingWHEstimateInfo => GetZPropertyInfo(Schema.LCTAmountIncludingWHEstimate);

		#endregion

		#region WET Amount

		public ZDecimal PayableWET => IsDutyDeferred ? ZDecimal.Zero : WETAmount;

		public ZDecimal WETAmount => MergedLines.Cast<CusEntryLine>().Sum(x => x.WETAmount);

		public ZPropertyInfo WETAmountInfo => GetZPropertyInfo(Schema.WETAmount);

		public ZDecimal WETAmountIncludingWHEstimate => MergedLines.Cast<CusEntryLine>().Sum(x => x.WETAmountIncludingWHEstimate);

		public ZPropertyInfo WETAmountIncludingWHEstimateInfo => GetZPropertyInfo(Schema.WETAmountIncludingWHEstimate);

		#endregion

		#region TotalSecurity

		public ZDecimal TotalSecurityConcession => MergedLines.Cast<CusEntryLine>().Sum(x => x.SecurityConcessionAmount);

		public ZPropertyInfo TotalSecurityConcessionInfo => GetZPropertyInfo(Schema.TotalSecurityConcession);

		public ZDecimal TotalSecurityLiability => MergedLines.Cast<CusEntryLine>().Sum(x => x.SecurityLiabilityAmount);

		public ZPropertyInfo TotalSecurityLiabilityInfo => GetZPropertyInfo(Schema.TotalSecurityLiability);

		#endregion

		#region Tolerance Amount

		public ZDecimal ToleranceAmount
		{
			get
			{
				if (Env.Registry.AUCustomsToleranceAmount == 0)
				{
					return (Env.Registry.AUCustomsTolerancePercentage) * TotalAmountPayable / 100.0m;
				}
				else if (Env.Registry.AUCustomsTolerancePercentage == 0)
				{
					return Env.Registry.AUCustomsToleranceAmount;
				}
				else
				{
					return Math.Min(Env.Registry.AUCustomsToleranceAmount, (Env.Registry.AUCustomsTolerancePercentage * TotalAmountPayable) / 100);
				}
			}
		}

		public Money ToleranceAmountMoney => new Money(ToleranceAmount, JobDeclaration.GetLocalCurrency());

		#endregion

		#region Supplier Code

		public virtual ZString JZ_SupplierCode => (RandomHeader == null || RandomHeader.Supplier == null) ? ZString.Empty : RandomHeader.Supplier.LocalCustomsSupplierCode;

		public ZString JZ_SupplierName => (RandomHeader == null || RandomHeader.Supplier == null) ? ZString.Empty : RandomHeader.Supplier.OH_FullNameTruncated;

		#endregion

		#region Is Prime Entry

		public ZBool IsPrimeEntry
		{
			get
			{
				if (IsEnclosureEntry)
				{
					return false;
				}

				foreach (var header in Declaration.CustomsEntryHeaders)
				{
					if (header.IsEnclosureEntry)
					{
						return true;
					}
				}
				return false;
			}
		}
		public ZPropertyInfo IsPrimeEntryInfo => GetZPropertyInfo(Schema.IsPrimeEntry);

		public CusEntryHeader PrimeEntry => (CusEntryHeader)Factory.Load(typeof(CusEntryHeader), CH_CH_PrimeEntry);

		#endregion

		#region Is Encolsure Entry

		public ZBool IsEnclosureEntry => PrimeEntry != null;

		public ZPropertyInfo IsEnclosureEntryInfo => GetZPropertyInfo(Schema.IsEnclosureEntry);

		#endregion

		#region Agent Reference

		public ZString AgentReference
		{
			get
			{
				var result = Declaration.JE_AgentsReference;

				switch (Env.Registry.AUCustoms.AgentsReferenceDefaulting)
				{
					case Core.Constants.AgentsReferenceDefaulting.FAR:
						result = GetAgentReferenceWithBGMReference();
						break;
					case Core.Constants.AgentsReferenceDefaulting.DEF:
						var allowedLengthForReference = Declaration.IsImportCMR ? cMRLengthForAgentReference : legacyLengthForAgentReference;

						if (allowedLengthForReference - (CH_BGMReference.Length + 1) >= Declaration.JE_AgentsReference.Length)
						{
							result = GetAgentReferenceWithBGMReference();
						}
						break;
				}

				return Declaration.IsImportCMR ? result.Substring(0, cMRLengthForAgentReference) : result.Substring(0, legacyLengthForAgentReference);
			}
		}

		readonly ZInt legacyLengthForAgentReference = 15;
		readonly ZInt cMRLengthForAgentReference = 20;

		ZString GetAgentReferenceWithBGMReference()
		{
			PopulateCH_BGMReferenceIfNeeded();

			var result = CH_BGMReference;
			if (!Declaration.JE_AgentsReference.IsEmpty)
			{
				result += " " + Declaration.JE_AgentsReference;
			}

			return result;
		}

		#endregion

		public ZBool IsNormalEntry => (!IsPrimeEntry && !IsEnclosureEntry);

		public ZBool NumberOfPackagesHasBeenSet
		{
			get
			{
				foreach (var invoiceHeader in InvoiceHeaders)
				{
					if (invoiceHeader.AddInfo.ZA_PackCountForNature10_Hidden.IsEmpty)
					{
						return false;
					}
				}
				return true;
			}
		}

		public const string ReferenceNumberSeparator = "/";
		public void PopulateCH_BGMReferenceIfNeeded()
		{
			if (CH_BGMReference.IsEmpty)
			{
				var declaration = Declaration;
				if (declaration != null)
				{
					declaration.PopulateJE_DeclarationReferenceIfNeeded();

					var reference = declaration.JE_DeclarationReference + ReferenceNumberSeparator + CalculateHeaderID();
					if (!declaration.IsEXPDeclaration && !declaration.IsImportCMR && declaration.HasMessageErrors)
					{
						reference += "MESSAGEERRORS";
					}

					CH_BGMReference = reference;
				}
			}
		}

		int CalculateHeaderID()
		{
			var result = -1;

			var declaration = Declaration;
			if (declaration != null)
			{
				var numberInDeclarationHeaderCollection = -1;
				var headerNumber = 0;
				foreach (var header in declaration.CustomsEntryHeaders)
				{
					headerNumber++;
					if (header == this)
					{
						numberInDeclarationHeaderCollection = headerNumber;
						break;
					}
				}

				var originalCMRMessagesFilter = new ZQuery();
				originalCMRMessagesFilter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
				originalCMRMessagesFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, CMRMessage.MessageSubTypes.Original);
				var numberOfDiscardedOriginalCMRMessages = declaration.Messages.Find(originalCMRMessagesFilter).Length;

				result = numberInDeclarationHeaderCollection + numberOfDiscardedOriginalCMRMessages;
			}

			return result;
		}

		public ZString Details
		{
			get
			{
				ZString result = "Reference Number: " + CH_BGMReference + "\r\n";
				if (Declaration != null)
				{
					result += Declaration.Details;
				}

				return result;
			}
		}

		public ZString ShortDescription => Declaration != null ? Declaration.ShortDescription : ZString.Empty;

		public TAndITransmitConditionChecker TAndITransmitConditionChecker => fTAndITransmitConditionChecker ?? (fTAndITransmitConditionChecker = new TAndITransmitConditionChecker(this));
		TAndITransmitConditionChecker fTAndITransmitConditionChecker;

		public bool HasALineWithPUPIndicator => MergedLines.HasALineWithPUPIndicator;

		#region Status

		#region Import Entry Advice

		public ZString ImportEntryAdviceCodeStatus => ZString.Empty;

		public ZString ImportEntryAdvice => StatusProvider != null ? StatusProvider.CargoStatusDescription : ZString.Empty;

		public ZPropertyInfo ImportEntryAdviceInfo => GetZPropertyInfo(Schema.ImportEntryAdvice);

		#endregion

		#region Payment Status

		public ZString PaymentStatus => CMRStatusProvider.PaymentStatusDescription;

		public ZPropertyInfo PaymentStatusInfo => GetZPropertyInfo(Schema.PaymentStatus);

		#endregion

		#region ATD Code

		public ZString ATDSecurityCode => CMRStatusProvider.ATDSecurityCode;

		public ZPropertyInfo ATDSecurityCodeInfo => GetZPropertyInfo(Schema.ATDSecurityCode);

		#endregion

		internal ICusEntryHeaderStatusProvider StatusProvider => Declaration != null && Declaration.IsImportCMR ? CMRStatusProvider as ICusEntryHeaderStatusProvider : null;

		internal CMRStatusProvider CMRStatusProvider => fCMRStatusProvider ?? (fCMRStatusProvider = new CMRStatusProvider(this));
		CMRStatusProvider fCMRStatusProvider;

		ZString ICMRControlMessageRespondee.UpdateStatusWhenControlMessageSyntaxError(EDIMessage incomingMessage, EDIMessage outgoingMessage)
		{
			var logText = ZString.Empty;
			var newStatus = ZString.Empty;
			switch (outgoingMessage.EM_MessageSubType)
			{
				case CMRMessage.MessageSubTypes.Original:
					if (CH_Status == CustomsEntryStatus.AwaitingFormalLodge.Code)
					{
						newStatus = CustomsEntryStatus.FailFormalLodge.Code;
					}
					else if (CH_Status == CustomsEntryStatus.AwaitingPreLodge.Code)
					{
						newStatus = CustomsEntryStatus.FailPreLodge.Code;
					}
					else if (CH_Status == CustomsEntryStatus.AwaitingPayment.Code)
					{
						newStatus = CustomsEntryStatus.FailPayment.Code;
					}
					else if (CH_Status == CustomsEntryStatus.AwaitingSAC.Code)
					{
						newStatus = CustomsEntryStatus.FailSAC.Code;
					}
					break;
				case CMRMessage.MessageSubTypes.Change:
				case CMRMessage.MessageSubTypes.Amendment:
					if (CH_Status == CustomsEntryStatus.AwaitingAmendment.Code)
					{
						newStatus = CustomsEntryStatus.FailAmendment.Code;
					}
					break;
				case CMRMessage.MessageSubTypes.Withdraw:
					if (CH_Status == CustomsEntryStatus.AwaitingWithdrawal.Code)
					{
						newStatus = CustomsEntryStatus.FailWithdrawal.Code;
					}
					break;
			}

			if (!newStatus.IsEmpty)
			{
				CH_Status = newStatus;
				Declaration.JE_MessageStatus = newStatus;
				logText = "Import JobDeclaration to: " + newStatus;
			}

			return logText;
		}

		public override bool HasBeenWithdrawn => Factory.GetValue(ref hasBeenWithdrawnCache, delegate
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeclarationCancellationApproved.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, PK);
			return IsWithdrawn || Factory.LoadTop1<StmALog>(query) != null;
		});

		CachedProperty<bool> hasBeenWithdrawnCache;

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore() => !HasBeenWithdrawn;

		public ZString ZA_DetailsNotToBeAmended
		{
			get { return AddInfo.ZA_DetailsNotToBeAmended_Hidden; }
			set { AddInfo.ZA_DetailsNotToBeAmended_Hidden = value; }
		}

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked => !Declaration?.SubmitWeeklyNilReturnN30 ?? base.ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked;

		public override bool IsActive
		{
			//even though an entry is withdrawn, it should stay as active so that subsequent merge links invoices to the entry
			get { return !fIsDeactivated && (!HasBeenWithdrawn || RandomHeader != null); }//is it still attached to invoices?
			set
			{
				if (!value)
				{
					var houseContainerNumbersCleared = new ZStringBuilder();

					if (packingGroupsForEntryPreRecycle != null)
					{
						AddInfo.ZA_HighHouseContPivotNo_Hidden = 0;

						foreach (var currentPack in packingGroupsForEntryPreRecycle.Cast<PackingGroup>())
						{
							if (currentPack.CR_HouseContainerNumber != 0)
							{
								houseContainerNumbersCleared.Append(currentPack.CR_HouseContainerNumber.ToString());
								currentPack.CR_HouseContainerNumber = 0;
							}
						}
					}

					if (!IsWaitingForResponse && !HasBeenLodgedAtCustoms)
					{
						Delete();
						//validation adds an error and users are not able to save for an entry with active transactions about to be deactivated
					}
					else if (!houseContainerNumbersCleared.IsEmpty && Declaration != null)
					{
						ErrorReporter.ReportOnce("CusEntryHeader CR_HouseContainerNumber",// this is just a constant
													string.Format("CR_HouseContainerNumber(s) ({2}) set to 0 when existing value > 0 and entry recycled, but entry not deleted. Job: {0}, Entry: {1}",// this is just a constant
													Declaration.JE_DeclarationReference, EntryNumber, houseContainerNumbersCleared.ToStringWithDelimiterBetweenAppends("/")));
					}
				}
				fIsDeactivated = !value;
			}
		}
		bool fIsDeactivated;

		ZString CurrentValueForNonAmendableDetails => Nature;

		protected override bool HasNonAmendableChangesCore => !ZA_DetailsNotToBeAmended.IsEmpty && !CurrentValueForNonAmendableDetails.EqualsIgnoringCase(ZA_DetailsNotToBeAmended);

		#region IsStatusPostLodge
		public bool IsStatusPostLodge
		=> CH_Status != CMRBaseStatuses.Codes.NotSent && //"NOT"
			CH_Status != CustomsEntryStatus.NotSent.Code && //""
			CH_Status != CustomsEntryStatus.AwaitingFormalLodge.Code &&
			CH_Status != CustomsEntryStatus.AwaitingSAC.Code &&
			CH_Status != CustomsEntryStatus.AwaitingPreLodge.Code &&
			CH_Status != CustomsEntryStatus.ClearPreLodge.Code &&
			CH_Status != CustomsEntryStatus.FailPreLodge.Code &&
			CH_Status != CustomsEntryStatus.FailFormalLodge.Code &&
			CH_Status != CustomsEntryStatus.FailSAC.Code;

		public bool IsStatusPostLodgeAndNoFailedAmendment
		=> IsStatusPostLodge &&
			CH_Status != CustomsEntryStatus.AwaitingAmendment.Code &&
			CH_Status != CustomsEntryStatus.FailAmendment.Code;

		public bool CMREntryMayHaveChangedPostLodge
		=> Declaration != null && Declaration.IsImportCMR && !(
									IsStatusPostLodgeAndNoFailedAmendment &&
									!Declaration.HasOutstandingAmendment &&
									!Declaration.HasOutstandingAmendmentNotQueued
								);

		#endregion

		public override ZString CH_WarehouseTransactionStatus
		{
			get { return base.CH_WarehouseTransactionStatus; }
			set
			{
				var oldValue = CH_WarehouseTransactionStatus;
				base.CH_WarehouseTransactionStatus = value;
				if (!IsCopying && oldValue != CH_WarehouseTransactionStatus)
				{
					Declaration?.InvoiceLines?.MarkAsNeedingValidation();
				}
			}
		}
		#region EntryHeaderStatusDescription
		public override ZString EntryHeaderStatusDescription
		{
			get
			{
				var result = base.EntryHeaderStatusDescription;
				var additionalStatusInfo = AdditionalEntryHeaderStatusInformation;
				if (additionalStatusInfo.Length > 0)
				{
					additionalStatusInfo = additionalStatusInfo.Replace("\t", "").Replace("\r", "").Replace("\n", "");
					result += ":" + additionalStatusInfo;
				}

				return result;
			}
		}

		public ZString AdditionalEntryHeaderStatusInformation
		{
			get
			{
				var resultBuilder = new StringBuilder();
				if (Declaration.IsDeclarationWorkFinished)
				{
					resultBuilder.Append(Declaration.GetDeclarationWorkCompleteReason());
				}
				ZString impediments = GetImpediments();
				if (impediments.Length > 0 && impediments.Replace("\t", "").Replace("\r", "").Replace("\n", "") != "")
				{
					resultBuilder.Append(" Impediment(s):");
					resultBuilder.Append(impediments);
				}
				return resultBuilder.ToString();
			}
		}
		#endregion

		public bool IsWithdrawn => CH_Status == CustomsEntryStatus.ClearWithdrawal.Code;

		#endregion

		#region Overrides

		public override ZString CH_Status
		{
			get { return base.CH_Status; }
			set
			{
				if (CH_Status != value && Declaration != null)
				{
					if (!IsStatusClear(CH_Status) && IsStatusClear(value))
					{
						HighHouseContPivotNoManager.Update();
						UpdateDetailsNotToBeAmended();
						DeletedPackingGroups.RemoveAndDeleteAll();
						Declaration.OutstandingAmendmentLogManger.CancelAllOutstandingAmendments();
						Declaration.OutstandingAmendmentLogManger.AllAmendmentRejectedLogs.CancelAll();
					}

					if (value == CustomsEntryStatus.AwaitingAmendment.Code)
					{
						Declaration.OutstandingAmendmentLogManger.CancelAllOutstandingAmendments();
					}
					else if (value == CustomsEntryStatus.FailAmendment.Code)
					{
						Declaration.OutstandingAmendmentLogManger.AddRejectedAmendmentLog();
					}
					else if (value == CustomsEntryStatus.ClearWithdrawal.Code)
					{
						Logs.AddNew(AutoEvents.DeclarationCancellationApproved, CustomsEntryStatus.ClearWithdrawal.MultilingualDescription.GetUnresolvedString());
					}

					if (!IsCopying)
					{
						foreach (JobComInvoiceHeader header in Declaration.Invoices)
						{
							header.InvoiceLines.MarkAsNeedingValidation();
						}
					}
				}

				base.CH_Status = value;
			}
		}

		protected override bool IsStatusClear(string status) => status == CustomsEntryStatus.ClearFormalLodge.Code || status == CustomsEntryStatus.ClearAmendment.Code;

		void UpdateDetailsNotToBeAmended()
		{
			//once this column is filled in, it stays forever as it reflects what Customs has at their site for this entry
			if (ZA_DetailsNotToBeAmended.IsEmpty)
			{
				ZA_DetailsNotToBeAmended = CurrentValueForNonAmendableDetails;
			}

			MergedLines.UpdateDetailsNotToBeAmendedAsEntryIsCleared();
		}

		public override bool IsFeePaidByBroker(string feeCode, ZString methdOfPayment, ILogger logger)
			=> Declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Broker && !IsGSTDeferredFee(feeCode) && !IsDutyDeferredFee(feeCode);

		bool IsGSTDeferredFee(string feeCode) => feeCode == Registry.Business.Customs.AU.EntryChargeTypeList.Codes.GSTDeferred;

		bool IsDutyDeferredFee(string feeCode)
		{
			var result = feeCode == Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount;

			if (!result && IsDutyDeferred)
			{
				if (feeCode == Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount)
				{
					result = PayableDuty.IsEmpty;
				}
				else
				{
					result = CusEntryHeaderCharges.IsChargeTypeDeferrable(feeCode) || CusEntryLineFee.IsFeeTypeDeferrable(feeCode);
				}
			}

			return result;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				PopulateCH_BGMReferenceIfNeeded();
			}
		}

		#region AutoRateASPOnSaved

		public void AutoRateASPOnSavedIfNecessary()
		{
			if ((!Declaration?.IsDeclarationIntegrated ?? false) && Declaration.IsQuarantineChargeRatingSeparated)
			{
				new InvoicePostingAccountingIntegrator(Declaration.Logger).IntegrateIfNecessary(new AUASPJobDeclarationIAccIntegrationDataProvider(Declaration, PK, false));
			}
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && IsFormalEntry && NeedsAutoRateASPOnSaved && Declaration != null)
			{
				AutoRateASPOnSavedIfNecessary();
			}
			ClearNeedsAutoRateASP();
		}

		public override void Delete()
		{
			Questions.RemoveAndDeleteAll();
			DeletedPackingGroups.RemoveAndDeleteAll();
			base.Delete();
		}

		// this is used in the bonded warhouse interface, transactions are not allowed if true
		// we could add the new failed amendment checking here, but that would further restrict when bonded wh transactions can be done.
		public override bool HaveAmendmentsBeenMadeAndNotYetClearedByCustoms
		=> CH_Status == CustomsEntryStatus.FailAmendment.Code ||
			CH_Status == CustomsEntryStatus.AwaitingAmendment.Code ||
			(Declaration != null && Declaration.HasOutstandingAmendment);

		protected override ZDecimal GetTotalChargeValueFor(Registry.Business.Customs.EntryChargeType chargeTypeElement, ZString methodOfPaymentCode)
		{
			var result = base.GetTotalChargeValueFor(chargeTypeElement, methodOfPaymentCode);
			if (chargeTypeElement.Code == ChargeTypesList.Codes.DTY)
			{
				// if duty is partially deferred, we should substract the deferred amount from original duty amount.
				result -= DeferredDuty;
			}
			else if (chargeTypeElement.Code == Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyDeferredAmount)
			{
				result = DeferredDuty;
			}
			else if (chargeTypeElement.Code == Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount && !Declaration.IsQuarantineChargeRatingSeparated)
			{
				result -= TotalAmountOnSeperatedASPCharges();
			}
			return result;
		}

		protected override ZString GetUniqueNumberForAccountingIntegrationCore()
		{
			var result = base.GetUniqueNumberForAccountingIntegrationCore();
			if (!result.IsEmpty && ConsolidatedDeclaration != null)
			{
				result += $"-{ConsolidatedEntryMemberID}";
			}
			return result;
		}

		ZDecimal TotalAmountOnSeperatedASPCharges()
		{
			var result = ZDecimal.Zero;

			var job = Declaration?.Job;
			if (job != null)
			{
				var uniqueNumberForASP = new AUASPEntryHeaderIAccInvoiceDataProvider(this).UniqueNumber;
				var allCharges = job.Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
				result = allCharges.Where(c => c.JR_APInvoiceNum.StartsWith(uniqueNumberForASP)).Sum(c => c.JR_LocalCostAmt);
			}

			return result;
		}

		protected override ZString EntryNumberType
		{
			get
			{
				var declaration = Declaration;
				if (declaration != null && declaration.IsEXPDeclaration)
				{
					return CusEntryNumberTypes.Australia.CAN;
				}

				return base.EntryNumberType;
			}
		}

		public override ZString CH_MessageType
		{
			get
			{
				string entryType = null;
				var declaration = Declaration;
				if (declaration != null && declaration.IsEXPDeclaration)
				{
					entryType = CusEntryNumber?.CE_EntryType;
				}

				return entryType ?? base.CH_MessageType;
			}
		}

		#endregion

		#region New Properties

		public bool IsEntryHeld => Declaration != null && Declaration.IsImportCMR && CH_EntryStatus == CMRImportEntryAdvice.Held.Code;

		public bool ShouldEntryBeNormalised => NormalisationConditionChecker.ShouldEntryBeNormalised;

		public override bool IsWaitingForResponse => CMRImportMessageStatusList.IsAwaitingResponse(CH_Status);

		public bool IsSubjectToDutyAndTax => (IsGoodsValueOfEntryOverThreshold || IsPayDutyOnLowValue || IsNature30 || IsSACWithLine) && !IsSACWithoutLine;

		internal bool IsGoodsValueOfEntryOverThreshold
		{
			get
			{
				var deminimus = UniversalReferenceHelper.GetDeminimus(Factory);
				return CustomsValueInAUD.Amount > deminimus;
			}
		}

		bool IsPayDutyOnLowValue => Factory.GetValue(ref isPayDutyOnLowValueCached, delegate
		{
			var payDutyOnLowValue = Declaration.IsUPEDeclaration;
			if (!payDutyOnLowValue)
			{
				var decQuestion = Questions.GetQuestionWithID(375);
				payDutyOnLowValue = decQuestion != null && decQuestion.IsAnswered && decQuestion.IsYes;
			}

			return payDutyOnLowValue;
		});

		CachedProperty<bool> isPayDutyOnLowValueCached;

		internal bool IsSACWithoutLine => Declaration != null && Declaration.IsSACWithoutLines;

		internal bool IsSACWithLine => Declaration != null && Declaration.IsSACWithLines;

		public CusEntryNumber ExportCusEntryNumber
		{
			get
			{
				var entryNumbers = AUCusEntryNumber.LoadEntryNumber(Factory, PK, AutoCusEntryHeader.Schema.TableName, CountryCode, !IsInDatabase);
				return entryNumbers.FirstOrDefault(x => x.CE_EntryType == CusEntryNumberTypes.Australia.ECN || x.CE_EntryType == CANType.CustomsAuthorityNumber.Code);
			}
		}

		#region Charges
		protected override Money GetFOB()
		{
			var result = Money.Empty;
			if (Declaration != null && Declaration.IsImportCMR)//Under Edifice, there can be two entries from an invoice header
			{
				foreach (var invoice in InvoiceHeaders)
				{
					if (invoice.JZ_OverrideFOB)
					{
						result = CurrencyConverter.Add(result, invoice.JZ_FOB);
					}
					else
					{
						foreach (var invoiceLine in invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>())
						{
							result = CurrencyConverter.Add(result, invoiceLine.JI_FOB);
						}
					}
				}
			}
			else
			{
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.FOB);
				}
			}
			return result;
		}

		protected override Money GetFOBInLocalCurrency()
		{
			var result = Money.Empty;
			if (Declaration != null && Declaration.IsImportCMR)//Under Edifice, there can be two entries from an invoice header
			{
				foreach (var invoice in InvoiceHeaders)
				{
					if (invoice.JZ_OverrideFOB)
					{
						result = CurrencyConverter.Add(result, CurrencyConverter.ConvertRounded(invoice.JZ_FOB, JobDeclaration.GetLocalCurrency()));
					}
					else
					{
						foreach (var invoiceLine in invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>())
						{
							result = CurrencyConverter.Add(result, new Money(invoiceLine.JI_Calc_FOB_InLocalCurrency, JobDeclaration.GetLocalCurrency()));
						}
					}
				}
			}
			else
			{
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, CurrencyConverter.ConvertRounded(cusLine.FOB, JobDeclaration.GetLocalCurrency()));
				}
			}
			return result;
		}

		/// <summary>
		/// This is the value coming from AddInfo, TILV
		/// </summary>
		public ZDecimal TAndI => Factory.GetValue(ref fTAndI, GetTAndI);

		CachedProperty<ZDecimal> fTAndI;

		public ZPropertyInfo TAndIInfo => GetZPropertyInfo(Schema.TAndI);
		ZDecimal GetTAndI()
		{
			var result = AddInfo.TILVInAUD;
			if (result.IsEmpty)
			{
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result += cusLine.TILV;
				}
			}
			return result;
		}

		public void UpdateTILV(string tilv)
		{
			AddInfo.ZA_TILV = tilv;
			ResetCachedValuesForTransportAndInsurance();
			TAndIInfo.RefreshBinding();
		}

		public Money TransportAndInsurance
		{
			get
			{
				if (fTransportAndInsurance == null)
				{
					fTransportAndInsurance = Money.Empty;

					foreach (CusEntryLine entryLine in MergedLines)
					{
						fTransportAndInsurance = CurrencyConverter.Add(fTransportAndInsurance, entryLine.TransportAndInsurance);
					}
				}
				return fTransportAndInsurance;
			}
		}
		Money fTransportAndInsurance;

		/// <summary>
		/// Transport and Insurance in a right currency
		/// </summary>
		public Money TransportAndInsuranceForMessage
		{
			get
			{
				var result = Money.Invalid;

				if (TAndITransmitConditionChecker.ShouldTransmitTAndIForHeader && TransportAndInsuranceCurrencyForMessage != null)
				{
					result = CurrencyConverter.ConvertExact(TransportAndInsurance, TransportAndInsuranceCurrencyForMessage);
				}
				return result;
			}
		}

		public bool DoesTILVExist
		{
			get
			{
				foreach (CusEntryLine cusLine in MergedLines)
				{
					if (cusLine.DoesTILVExist)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal RefCurrency TransportAndInsuranceCurrencyForMessage
		{
			get
			{
				if (fTransportAndInsuranceCurrencyForMessage == null)
				{
					var currenciesUsed = new List<ZString>();

					foreach (CusEntryLine cusLine in MergedLines)
					{
						var tAndICurrencyCode = cusLine.TransportAndInsurance.Currency == null ? "" : cusLine.TransportAndInsurance.Currency.Code;
						if (!string.IsNullOrEmpty(tAndICurrencyCode) && !currenciesUsed.Contains(tAndICurrencyCode))
						{
							currenciesUsed.Add(tAndICurrencyCode);
							if (currenciesUsed.Count > 1)
							{
								break;
							}
						}
					}

					var result = JobDeclaration.LocalCurrencyConstantCode;
					if (currenciesUsed.Count == 1)
					{
						result = currenciesUsed[0];//if more than one currency is used, then local currency
					}

					fTransportAndInsuranceCurrencyForMessage = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, result);
				}
				return fTransportAndInsuranceCurrencyForMessage;
			}
		}
		RefCurrency fTransportAndInsuranceCurrencyForMessage;

		/// <summary>
		/// TILV amount in a local currency if it exists in AddInfo, Otherwise, OFT + ONS in a local currency
		/// </summary>
		public Money TransportAndInsuranceInLocalCurrency => Factory.GetValue(ref fTransportAndInsuranceInLocalCurrency, GetTransportAndInsuranceInLocalCurrency);

		CachedProperty<Money> fTransportAndInsuranceInLocalCurrency;

		Money GetTransportAndInsuranceInLocalCurrency()
		{
			var result = AddInfo.TILVMoney;
			if (result.IsEmpty)
			{
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.CustomsCalculatedTransportAndInsuranceInLocalCurrency);
				}
			}
			return CurrencyConverter.ConvertExact(result, JobDeclaration.GetLocalCurrency());
		}

		public Money Commission => ChargesProvider.Commission;

		public Money BuyingCommission
		{
			get
			{
				var result = Money.Empty;
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.BuyingCommission);
				}
				return result;
			}
		}

		public Money OtherCommission
		{
			get
			{
				var result = Money.Empty;
				foreach (CusEntryLine cusLine in MergedLines)
				{
					result = CurrencyConverter.Add(result, cusLine.OtherCommission);
				}
				return result;
			}
		}

		public Money OtherCharges1 => ChargesProvider.OtherCharges1;

		public Money OtherCharges2
		{
			get
			{
				var result = ChargesProvider.OtherCharges2;
				var amount = result.Amount;
				if (Declaration != null && Declaration.IsImportEdifice)
				{
					amount = result.Amount * -1;
				}
				return new Money(amount, result.Currency);
			}
		}

		public Money InvoiceTotal => ChargesProvider.InvoiceTotal;

		public Money LandingCharges => ChargesProvider.LandingCharges;

		public Money Discount => ChargesProvider.Discount;

		public Money PackingCosts => ChargesProvider.PackingCosts;

		public Money ForeignInlandFreight => ChargesProvider.ForeignInlandFreight;

		public ICurrency[] UsedCurrencies
		{
			get
			{
				if (fUsedCurrencies == null)
				{
					var usedCurrs = new ArrayList();
					if (InvoiceTotal.Currency != null)
					{
						usedCurrs.Add(InvoiceTotal.Currency.Code);
					}

					if (Declaration != null)
					{
						foreach (BaseJobComInvHeaderCharge groupCharge in Declaration.JobComInvoiceGroupHeaders[0].Charges)
						{
							if (groupCharge.Currency != null && !usedCurrs.Contains(groupCharge.Currency.RX_Code.ToString()))
							{
								usedCurrs.Add(groupCharge.Currency.RX_Code.ToString());
							}
						}
					}
					foreach (var invHeader in InvoiceHeaders)
					{
						foreach (BaseJobComInvHeaderCharge charge in invHeader.Charges)
						{
							if (charge.Currency != null && !usedCurrs.Contains(charge.Currency.RX_Code.ToString()))
							{
								usedCurrs.Add(charge.Currency.RX_Code.ToString());
							}
						}

						foreach (var line in invHeader.JobComInvoiceLines.Cast<JobComInvoiceLine>())
						{
							var priceAdjustment = line.JI_PriceAdjustment;
							if (!priceAdjustment.IsEmpty && priceAdjustment.Currency != null && !usedCurrs.Contains(priceAdjustment.Currency.Code))
							{
								usedCurrs.Add(priceAdjustment.Currency.Code);
							}

							var dumpingExportAmount = line.JI_DumpingExportPrice;
							if (!dumpingExportAmount.IsEmpty && dumpingExportAmount.Currency != null && !usedCurrs.Contains(dumpingExportAmount.Currency.Code))
							{
								usedCurrs.Add(dumpingExportAmount.Currency.Code);
							}

							foreach (var charge in line.Charges)
							{
								if (charge.Currency != null && !usedCurrs.Contains(charge.Currency.RX_Code.ToString()))
								{
									usedCurrs.Add(charge.Currency.RX_Code.ToString());
								}
							}
						}
					}

					var usedCurrenciesArrayList = new ArrayList();
					foreach (string currencyCode in usedCurrs)
					{
						var currencyLoaded = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
						if (currencyLoaded != null)
						{
							usedCurrenciesArrayList.Add(currencyLoaded);
						}
					}
					fUsedCurrencies = (ICurrency[])usedCurrenciesArrayList.ToArray(typeof(ICurrency));
				}
				return fUsedCurrencies;
			}
		}
		internal ICurrency[] fUsedCurrencies;

		#endregion

		#region CH_CargoStatus
		public ZString CH_CargoStatus
		{
			get
			{
				var resultBuilder = new StringBuilder();
				var aCSOutput = false;
				if (BarrierStatus != ACSBarrierStatus.None)
				{
					if (!aCSOutput)
					{
						resultBuilder.Append("ACS Status: ");
					}

					resultBuilder.Append("Barrier=" + BarrierStatus.Code + ", ");
					aCSOutput = true;
				}
				if (ACSCommercialStatus != ACSCommercialStatusList.None)
				{
					if (!aCSOutput)
					{
						resultBuilder.Append("ACS Status: ");
					}

					resultBuilder.Append("Commercial=" + ACSCommercialStatus.Code + ", ");
					aCSOutput = true;
				}
				var aQISOutput = false;
				if (AQISBarrierStatus != AQISBarrierStatusList.None)
				{
					if (!aQISOutput)
					{
						resultBuilder.Append("Quarantine Status: ");
					}

					resultBuilder.Append("Barrier=" + AQISBarrierStatus.Code + ", ");
					aQISOutput = true;
				}
				if (AQISCommercialStatus != AQISCommercialStatusList.None)
				{
					if (!aQISOutput)
					{
						resultBuilder.Append("Quarantine Status: ");
					}

					resultBuilder.Append("Commercial=" + AQISCommercialStatus.Code + ", ");
					aQISOutput = true;
				}
				var entryOutput = false;
				if (EntryStatusConditions != EntryStatusConditionsList.None)
				{
					if (!entryOutput)
					{
						resultBuilder.Append("Entry: ");
					}

					resultBuilder.Append("Conditions=" + EntryStatusConditions.Code + ", ");
					entryOutput = true;
				}
				if (EntryStatusTransmit != EntryStatusTransmitList.None)
				{
					if (!entryOutput)
					{
						resultBuilder.Append("Entry: ");
					}

					resultBuilder.Append("Transmit=" + EntryStatusTransmit.Code + ", ");
					entryOutput = true;
				}
				return resultBuilder.ToString().TrimEnd(',', ' ');
			}
		}

		public ZString CH_CargoStatusLongDescription
		{
			get
			{
				var resultBuilder = new StringBuilder();
				if (BarrierStatus != ACSBarrierStatus.None)
				{
					resultBuilder.Append("ACS Barrier Status: '" + BarrierStatus.Code + "' - " + BarrierStatus.Description + "\r\n");
				}
				if (ACSCommercialStatus != ACSCommercialStatusList.None)
				{
					resultBuilder.Append("ACS Commercial Status: '" + ACSCommercialStatus.Code + "' - " + ACSCommercialStatus.Description + "\r\n");
				}
				if (AQISBarrierStatus != AQISBarrierStatusList.None)
				{
					resultBuilder.Append("Quarantine Barrier Status: '" + AQISBarrierStatus.Code + "' - " + AQISBarrierStatus.Description + "\r\n");
				}
				if (AQISCommercialStatus != AQISCommercialStatusList.None)
				{
					resultBuilder.Append("Quarantine Commercial Status: '" + AQISCommercialStatus.Code + "' - " + AQISCommercialStatus.Description + "\r\n");
				}
				if (EntryStatusConditions != EntryStatusConditionsList.None)
				{
					resultBuilder.Append("Entry Status Conditions: '" + EntryStatusConditions.Code + "' - " + EntryStatusConditions.Description + "\r\n");
				}
				if (EntryStatusTransmit != EntryStatusTransmitList.None)
				{
					resultBuilder.Append("Entry Status Transmit: '" + EntryStatusTransmit.Code + "' - " + EntryStatusTransmit.Description + "\r\n");
				}
				return resultBuilder.ToString();
			}
		}

		public ACSBarrierStatus BarrierStatus
		{
			get => ACSBarrierStatus.Get(CusEntryNumber != null ? PaddedSubString(CusEntryNumber.CE_EntryLineReference, 0, 1) : new ZString(" "));
			set
			{
				CreateCusEntryNumberIfNeeded();
				CusEntryNumber.CE_EntryLineReference = PadAndStuff(CusEntryNumber.CE_EntryLineReference, 0, value.Code);
			}
		}

		public ACSCommercialStatusList ACSCommercialStatus
		{
			get => ACSCommercialStatusList.Get(CusEntryNumber != null ? PaddedSubString(CusEntryNumber.CE_EntryLineReference, 1, 1) : new ZString(" "));
			set
			{
				CreateCusEntryNumberIfNeeded();
				CusEntryNumber.CE_EntryLineReference = PadAndStuff(CusEntryNumber.CE_EntryLineReference, 1, value.Code);
			}
		}

		public AQISBarrierStatusList AQISBarrierStatus
		{
			get => AQISBarrierStatusList.Get(CusEntryNumber != null ? PaddedSubString(CusEntryNumber.CE_EntryLineReference, 2, 1) : new ZString(" "));
			set
			{
				CreateCusEntryNumberIfNeeded();
				CusEntryNumber.CE_EntryLineReference = PadAndStuff(CusEntryNumber.CE_EntryLineReference, 2, value.Code);
			}
		}

		public AQISCommercialStatusList AQISCommercialStatus
		{
			get => AQISCommercialStatusList.Get(CusEntryNumber != null ? PaddedSubString(CusEntryNumber.CE_EntryLineReference, 3, 1) : new ZString(" "));
			set
			{
				CreateCusEntryNumberIfNeeded();
				CusEntryNumber.CE_EntryLineReference = PadAndStuff(CusEntryNumber.CE_EntryLineReference, 3, value.Code);
			}
		}

		public EntryStatusTransmitList EntryStatusTransmit
		{
			get => EntryStatusTransmitList.Get(CusEntryNumber != null ? PaddedSubString(CusEntryNumber.CE_EntryLineReference, 4, 1) : new ZString(" "));
			set
			{
				CreateCusEntryNumberIfNeeded();
				CusEntryNumber.CE_EntryLineReference = PadAndStuff(CusEntryNumber.CE_EntryLineReference, 4, value.Code);
			}
		}

		public EntryStatusConditionsList EntryStatusConditions
		{
			get => EntryStatusConditionsList.Get(CusEntryNumber != null ? PaddedSubString(CusEntryNumber.CE_EntryLineReference, 5, 5) : new ZString(" "));
			set
			{
				CreateCusEntryNumberIfNeeded();
				CusEntryNumber.CE_EntryLineReference = PadAndStuff(CusEntryNumber.CE_EntryLineReference, 5, value.Code);
			}
		}

		protected ZString PadAndStuff(ZString input, int index, ZString toStuff)
		{
			var leftString = input.SubstringSafe(0, index).PadRight(index, ' ');
			var rightString = input.SubstringSafe(index + toStuff.Length);
			return leftString + toStuff + rightString;
		}

		protected ZString PaddedSubString(ZString input, int index, int length) => input.SubstringSafe(index, length).PadRight(length, ' ');

		#endregion

		#region AQISServicePaymentAmount

		public ZDecimal AQISServicePaymentAmount
		{
			get => Charges.GetAmount(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount);
			set
			{
				Charges[Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount].C1_ChargeAmount = value;
				AQISServicePaymentAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AQISServicePaymentAmountInfo => GetZPropertyInfo(Schema.AQISServicePaymentAmount);

		#endregion

		#region Subject To Red Line Processing

		public ZString SubjectToRedLineProcessing => AddInfo.ZA_IsSubjectToRedLine_Hidden ? "Subject to Red Line" : String.Empty;

		public ZPropertyInfo SubjectToRedLineProcessingInfo => GetZPropertyInfo(nameof(SubjectToRedLineProcessing));

		#endregion

		#region Warehouse Number of Packs

		public ZInt WarehouseNumberOfPacks
		{
			get { return AddInfo.ZA_WarehouseNumberOfPacks_Hidden; }
			set { AddInfo.ZA_WarehouseNumberOfPacks_Hidden = value; }
		}

		public ZPropertyInfo WarehouseNumberOfPacksInfo => GetWrappedZPropertyInfo(Schema.WarehouseNumberOfPacks, x => AddInfo.ZA_WarehouseNumberOfPacks_HiddenInfo);

		#endregion

		#region Is Goods Delivered
		public
#if DEBUG
 virtual
#endif
 bool IsGoodsDelivered => Factory.GetValue(ref isGoodsDeliveredCached, delegate
		{
			var decQuestion = Questions.GetQuestionWithID(14);
			return decQuestion != null && decQuestion.IsAnswered && decQuestion.IsYes;
		});

		CachedProperty<bool> isGoodsDeliveredCached;
		#endregion

		#endregion

		#region Validation

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		#endregion

		#region Total Payable Advised in Last Clearance Message

		public ZDecimal TotalAmountPayableForThisSession => IsStatusPostLodge ? TotalPayableAdvisedInLastClearanceMessage : TotalAmountPayable;

		/// <summary>
		/// Depending on status, this will return 0m if it is already paid. Otherwise it shows Due Amount.
		/// </summary>
		public virtual ZDecimal TotalPayableDueAdvisedInLastClearanceMessage => !CMREntryPaymentStatusList.IsPaymentMessageSentOrCleared(AddInfo.ZA_PaymentStatus_Hidden) ? TotalPayableAdvisedInLastClearanceMessage : ZDecimal.Zero;

		/// <summary>
		/// Total Payable Advised in Last Clearance Amount
		/// </summary>
		public virtual ZDecimal TotalPayableAdvisedInLastClearanceMessage
		{
			get
			{
				if (!isTotalPayableAdvisedInLastClearanceMessageCalculated)
				{
					var iMDOrSACSuccessfulResponses = (this as IStatusNeedsRecalculationProvider).Messages.GetMatchingMessages(EDIInterchange.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.IMD, CMRMessage.CMRMessageTypes.SAC }, EDIInterchange.Direction.Receive);

					CMRMessage latestSuccessfulMessage = null;
					foreach (var message in iMDOrSACSuccessfulResponses.Cast<CMRMessage>())
					{
						if (!message.IsRejected && (latestSuccessfulMessage == null || latestSuccessfulMessage.EM_SystemCreateTimeUtc < message.EM_SystemCreateTimeUtc))
						{
							latestSuccessfulMessage = message;
						}
					}

					if (latestSuccessfulMessage != null)
					{
						var provider = latestSuccessfulMessage as IOutstandingPaymentInfoProvider;
						if (provider != null)
						{
							fTotalPayableAdvisedInLastClearanceMessage = new OutstandingAmountRetriever(provider).OutstandingAmount;
						}
					}
					isTotalPayableAdvisedInLastClearanceMessageCalculated = true;
				}
				return fTotalPayableAdvisedInLastClearanceMessage;
			}
		}
		ZDecimal fTotalPayableAdvisedInLastClearanceMessage;
		internal bool isTotalPayableAdvisedInLastClearanceMessageCalculated;

		#endregion

		#region CP Dec Questions Collections

		[MaxLength(3)]
		public ZString CPDecQuestionViewType
		{
			get { return fCPDecQuestionViewType; }
			set
			{
				CheckMaximumLength(CPDecQuestionViewTypeInfo, value);
				SetNonPersistentPropertyValue(CPDecQuestionViewTypeInfo, ref fCPDecQuestionViewType, value);
				if (Lookups.CPDecQuestionViewTypeList.ContainsCode(value))
				{
					CPDecQuestionsViewCollection.Rebuild();
				}
			}
		}
		ZString fCPDecQuestionViewType = CPDecQuestionViewTypeList.Codes.All;

		public ZPropertyInfo CPDecQuestionViewTypeInfo => GetZPropertyInfo(nameof(CPDecQuestionViewType));

		public AllCPDecQuestionsViewCollection CPDecQuestionsViewCollection => fAllCPDecQuestions ?? (fAllCPDecQuestions = new AllCPDecQuestionsViewCollection(AllCPDecQuestions));
		AllCPDecQuestionsViewCollection fAllCPDecQuestions;

		public AllEntryLineCPDecQuestion AllCPDecQuestions
		{
			get
			{
				if (fAllCPDecQuestionsComplete == null)
				{
					fAllCPDecQuestionsComplete = new AllEntryLineCPDecQuestion(this);
					fAllCPDecQuestionsComplete.Load();
				}
				return fAllCPDecQuestionsComplete;
			}
		}
		AllEntryLineCPDecQuestion fAllCPDecQuestionsComplete;

		#endregion

		#region New Proxy Properties for CusEntryHeaderCharges

		public ZDecimal AllEntryFees
		{
			get
			{
				ZDecimal result = 0m;
				if (Declaration != null && Declaration.IsImportCMR)
				{
					result = AllAQISCharges + DeclarationProcessingCharge + TotalPayableAdmin + OtherEntryCharge;
				}
				else
				{
					result = EntryFee + MessageFee + TradegateGST + ScreenFreeCharge + OtherEntryCharge;
				}
				return result;
			}
		}

		public ZDecimal AllEntryFeesExcludingAQISServiceFee => AllEntryFees - AQISServicePaymentAmount;

		public ZDecimal EntryFeeIncludingTradegate => EntryFee + MessageFee + TradegateGST;

		public ZDecimal EntryFee
		{
			get
			{
				ZDecimal result;
				if (Declaration != null && Declaration.IsImportCMR)
				{
					result = IsWeeklySettlement ? ZDecimal.Zero : GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DeclarationProcessingCharge);
				}
				else
				{
					result = GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.EntryFee);
				}
				return result;
			}
		}

		public ZPropertyInfo EntryFeeInfo => GetZPropertyInfo(Schema.EntryFee);

		public ZDecimal MessageFee => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.MessageFee);

		public ZPropertyInfo MessageFeeInfo => GetZPropertyInfo(Schema.MessageFee);

		public ZDecimal TradegateGST => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.TradegateGST);

		public ZPropertyInfo TradegateGSTInfo => GetZPropertyInfo(Schema.TradegateGST);

		public ZDecimal WoodLevy => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.Woodlevy);

		public ZPropertyInfo WoodLevyInfo => GetZPropertyInfo(Schema.WoodLevy);

		public ZDecimal WoodLevyIncludingWHEstimate => GetTotalChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.Woodlevy, IncludeLandedCostsOnly);

		public ZPropertyInfo WoodLevyIncludingWHEstimateInfo => GetZPropertyInfo(Schema.WoodLevyIncludingWHEstimate);

		public ZDecimal ScreenFreeCharge => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.ScreenFree);

		public ZPropertyInfo ScreenFreeChargeInfo => GetZPropertyInfo(Schema.ScreenFreeCharge);

		public ZDecimal OtherEntryCharge => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.OtherCharges);

		public ZPropertyInfo OtherEntryChargeInfo => GetZPropertyInfo(Schema.OtherEntryCharge);

		public ZDecimal DeclarationProcessingCharge => IsWeeklySettlement || IsSOFA ? ZDecimal.Zero : GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DeclarationProcessingCharge);

		public ZPropertyInfo DeclarationProcessingChargeInfo => GetZPropertyInfo(Schema.DeclarationProcessingCharge);

		public ZDecimal TotalPayableAdmin => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.TotalPayableAdmin);

		public ZPropertyInfo TotalPayableAdminInfo => GetZPropertyInfo(Schema.TotalPayableAdmin);

		public ZDecimal AQISProcessingCharge => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISProcessingCharge);

		public ZPropertyInfo AQISProcessingChargeInfo => GetZPropertyInfo(Schema.AQISProcessingCharge);

		public ZDecimal AQISContainerCharges => GetChargeFromHeaderCharges(Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISContainerCharges);

		public ZPropertyInfo AQISContainerChargesInfo => GetZPropertyInfo(Schema.AQISContainerCharges);

		public ZDecimal AllAQISCharges => AQISProcessingCharge + AQISServicePaymentAmount + AQISContainerCharges;

		public ZDecimal PayableAQISCharges
		{
			get
			{
				var result = AQISServicePaymentAmount + AQISContainerCharges;
				if (!IsDutyDeferred)
				{
					result += AQISProcessingCharge;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected override void AddExtraRequiredFieldsMessageError(ZStringBuilder messageErrors, bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			base.AddExtraRequiredFieldsMessageError(messageErrors, checkProduct, checkQuantity, checkEntryDetails);
			var declaration = Declaration;
			if (declaration != null && declaration.IsExWarehouse)
			{
				if (declaration.GetInvoiceLinesMarkedForBondedWarehousingWithDifferentWarehouseAddressToDeclaration().Length > 0)
				{
					messageErrors.Append(JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);
				}
			}
		}

		protected override string GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage() => JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresWRNAndWRL;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		public readonly IMDStatusCalculator Calculator;

		readonly EntryPaymentStatusCalculator paymentStatusCalculator;

		protected JobComInvoiceHeader fRandomHeader;

		protected Money fAccumulatedInvoiceAmount;
		protected Money fAccumulatedCustomsValue;
		protected void AccumulateCustomsValueAndInvoiceAmount()
		{
			fAccumulatedInvoiceAmount = Money.Empty;
			fAccumulatedCustomsValue = Money.Empty;
			foreach (CusEntryLine entryLine in MergedLines)
			{
				fAccumulatedInvoiceAmount = CurrencyConverter.Add(fAccumulatedInvoiceAmount, entryLine.Price);
				fAccumulatedCustomsValue = CurrencyConverter.Add(fAccumulatedCustomsValue, entryLine.FOB);
			}
		}

		public NormalisationConditionChecker NormalisationConditionChecker => fNormalisationConditionChecker ?? (fNormalisationConditionChecker = new NormalisationConditionChecker(this));
		NormalisationConditionChecker fNormalisationConditionChecker;

		protected ICommercialChargesProvider ChargesProvider => NormalisationConditionChecker.ShouldEntryBeNormalised ? NormalisedProvider : CommercialChargesProvider;

		protected EntryHeaderNormalisedChargesProvider NormalisedProvider => fNormalisedProvider ?? (fNormalisedProvider = new EntryHeaderNormalisedChargesProvider(this, NormalisedInvoiceTotalCurrency));
		EntryHeaderNormalisedChargesProvider fNormalisedProvider;

		protected EntryHeaderCommercialChargesProvider CommercialChargesProvider => fCommercialChargesProvider ?? (fCommercialChargesProvider = new EntryHeaderCommercialChargesProvider(this));
		EntryHeaderCommercialChargesProvider fCommercialChargesProvider;

		protected override ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override void ResetCachedValues()
		{
			base.ResetCachedValues();
			fNormalisationConditionChecker = null;
			fAccumulatedCustomsValue = null;
			fAccumulatedInvoiceAmount = null;
			ResetCachedValuesForTransportAndInsurance();
			fTransportAndInsurance = null;
			fTransportAndInsuranceCurrencyForMessage = null;
			TAndITransmitConditionChecker.RefreshCalculation();
			fAmendedLines = null;
			fAmendedHouseBillContainerPacks = null;
			fUsedCurrencies = null;
			ResetCachedValuesForDutyCalculation();
			packingGroupsForEntryPreRecycle = GetPackingGroups();
			invoiceTotalCurrency = null;
			lodgementQuestionKeys = null;
			isCustomsValuesByNatureSet = false;
		}
		PackingGroupCollection packingGroupsForEntryPreRecycle;

		public void ResetCachedValuesForDutyCalculation()
		{
			fCalculatedNetTotalLinesAmountDueDone = false;
			foreach (var cPDec in Questions.Cast<CMRCusEntryCPDec>())
			{
				cPDec.ResetCachedValues();
			}
		}

		public void ResetCachedValuesForTransportAndInsurance()
		{
			fTAndI = null;
			fTransportAndInsuranceInLocalCurrency = null;
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		bool IStatusNeedsRecalculationProvider.StatusNeedsRecalculation
		{
			get
			{
				var result = !IsDeleted;
				if (result)
				{
					var declaration = Declaration;
					result = declaration != null && !declaration.IsDeleted && declaration.IsImportCMR
						&& ConsolidatedDeclaration == null
						&& MessagesAreLoaded && Messages.HasChanges;
				}
				return result;
			}
		}

		EDIMessageCollection IStatusNeedsRecalculationProvider.Messages => Factory.GetCachedValue("AU|CusEntryHeader|IStatusNeedsRecalculationProvider.Messages|" + PK,
			() => ConsolidatedDeclaration?.Messages ?? Messages);

		#endregion

		#region ConsolidatedDeclaration

		public void DeriveConsolidatedStatus()
		{
			Calculator.DeriveStatusNow();
			paymentStatusCalculator.DeriveStatusNow();
		}

		public ConsolidatedDeclaration ConsolidatedDeclaration => Factory.GetCachedValue("AU|CusEntryHeader|ConsolidatedDeclaration|" + PK,
			() => Declaration == null ? null : ConsolidatedDeclaration.GetConsolidatedDeclaration(Declaration) as ConsolidatedDeclaration,
			CacheStalenessPolicy.StaleOnFactorySave);

		#endregion

		#region ISourceIdentifierProvider

		ZGuid ISourceIdentifierProvider.SourceIdentifier => ConsolidatedDeclaration == null  ? PK : ConsolidatedDeclaration.PK;

		#endregion

		#region ICPQAHeaderAttachee Members

		LodgementQuestionKeys ICPQAHeaderAttachee.LodgementQuestionKey
		{
			get
			{
				if (!lodgementQuestionKeys.HasValue)
				{
					var result = new LodgementQuestionKeys();
					result.HasFCLOrFCXLines = HasContainerOfType(Core.Constants.ContainerModes.FCL, Containers) || HasContainerOfType(Core.Constants.ContainerModes.FCLMixedShipper, Containers);
					result.HasLCLLines = HasContainerOfType(Core.Constants.ContainerModes.LCL, Containers);
					result.IsABNQuotedForLCTAndWET = MergedLines.IsABNQuotedForLCTAndWET;
					result.IsPaid = IsCustomsChargePaid;
					result.IsPaidUnderProtest = HasALineWithPUPIndicator;
					result.IsNature30 = IsNature30;
					result.IsNature20 = IsNature20;
					result.IsSAC = Declaration.IsSAC;
					result.IsSACWithLine = Declaration.IsSACWithLines;
					result.IsSea = Declaration.IsSea;
					result.TotalCustomsValue = CustomsValueInAUD.Amount;
					result.IsGSTDeferred = Declaration.Importer != null && Declaration.Importer.MiscServ.IsGSTVATDeferred;
					result.IsRefundAmendment = HasRefundReason;
					result.HasSecurityTreatment = HasSecurityTreatment;
					result.IsUPEDeclaration = Declaration.IsUPEDeclaration;
					result.IsSOFADeclaration = Declaration.IsSOFADeclaration;
					result.HasRemissionOnBunkerFuels = HasRemissionOnBunkerFuels;
					lodgementQuestionKeys = result;
				}
				return lodgementQuestionKeys.Value;
			}
		}
		LodgementQuestionKeys? lodgementQuestionKeys;

		public bool HasSecurityTreatment
		{
			get
			{
				foreach (ICusEntryLine entryLine in MergedLines)
				{
					var treatmentCode = entryLine.TreatmentCode;
					if (!treatmentCode.IsEmpty && SecurityRequiredTreatmentCodes.Contains((string)treatmentCode))
					{
						return true;
					}
					if (entryLine.SCN.IsEmpty &&
						(CMRInstrumentCharacteristic.IsInstrumentCharacteristic(entryLine.TCI_InstrumentNo, 3, Factory) ||
						CMRInstrumentCharacteristic.IsInstrumentCharacteristic(entryLine.TI2_InstrumentNo, 3, Factory) ||
						CMRInstrumentCharacteristic.IsInstrumentCharacteristic(entryLine.PRI_InstrumentNo, 3, Factory) ||
						CMRInstrumentCharacteristic.IsInstrumentCharacteristic(entryLine.InstrumentCode, 3, Factory)))
					{
						return true;
					}
				}
				return false;
			}
		}

		internal string[] SecurityRequiredTreatmentCodes => _securityRequiredTreatmentCodes ?? (_securityRequiredTreatmentCodes = AUCustomsDataRegistry.Instance.SecurityRequiredTreatmentCodes.Value.Split(','));
		string[] _securityRequiredTreatmentCodes;

		string[] NoProcessingChargeTreatmentCodes => _noProcessingChargeTreatmentCodes ?? (_noProcessingChargeTreatmentCodes = AUCustomsDataRegistry.Instance.NoProcessingChargeTreatmentCodes.Value.Split(','));
		string[] _noProcessingChargeTreatmentCodes;

		public ZBool HasRemissionOnBunkerFuels
		{
			get
			{
				foreach (ICusEntryLine entryLine in MergedLines)
				{
					var treatmentCode = entryLine.TreatmentCode;
					if (!treatmentCode.IsEmpty && treatmentCode == "142")
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasRefundReason
		{
			get
			{
				if (!EntryNumber.IsEmpty)
				{
					foreach (var entryLine in AmendedLines)
					{
						if (!entryLine.RefundReasonCode.IsEmpty)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public ZString RefundReasonCode
		{
			get => AddInfo.ZA_RRC_Hidden;
			set { AddInfo.ZA_RRC_Hidden = value; }
		}

		public ZPropertyInfo RefundReasonCodeInfo => GetWrappedZPropertyInfo(Schema.RefundReasonCode, x => AddInfo.ZA_RRC_HiddenInfo);

		protected override bool EntryNumber_ReadOnly => Declaration == null || !Declaration.IsDeclarationByExternalBroker;

		public override ZDateTime DeclarationDate
		{
			get
			{
				var result = base.DeclarationDate;
				if (!result.IsValid)
				{
					result = Declaration.ManualClearanceDate;
				}

				return result;
			}
		}

		public bool IsCustomsChargePaid => CMREntryPaymentStatusList.IsClearedStatus(AddInfo.ZA_PaymentStatus_Hidden) || AddInfo.ZA_IsPAYRECAck_Hidden || TotalDeferredDutyFromCustoms > 0;

		bool HasContainerOfType(string containerMode, BaseCusContainer[] containers) => containers.Cast<BaseCusContainer>().Any(x => x.CO_FCL_LCL_AIR == containerMode);

		SchemaGuidColumn ICPQAAttachee.FKColumnInCusEntryCPDecTable => CusEntryCPDecSchema.ON_CH;

		ZDateTime ICPQAAttachee.SelectionDate => ZDateTime.Today;

		[ChildEditable(true)]
		public CMRCusEntryCPDecCollection Questions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = new CMRCusEntryCPDecCollection(this);
					fQuestions.Load();
					RegisterEditableChildObject(fQuestions);
				}
				return fQuestions;
			}
		}
#if DEBUG
		public
#endif
 CMRCusEntryCPDecCollection fQuestions;

		#endregion

		#region IAddInfo Members

		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CusEntryHeaderAddInfo(this, CH_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AUAddInfo fAddInfo;

		#endregion

		#region IAggregatedAddInfo Members

		ZDateTime IAggregatedAddInfo.DateOfValuation => EffectiveValuationDate;

		ZString IAggregatedAddInfo.AggregatedZA_ORG
		{
			get
			{
				var randomHeader = RandomHeader;
				return randomHeader != null ? randomHeader.AddInfo.ZA_ORG : ZString.Empty;
			}
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get
			{
				var randomHeader = RandomHeader;
				return randomHeader != null ? randomHeader.AddInfo.ZA_PRF : ZString.Empty;
			}
		}

		IZType IAggregatedAddInfo.AggregatedValue(string propertyName) => (IZType)AddInfo[propertyName];

		bool IAggregatedAddInfo.IsCopying => IsCopying;

		#endregion

		#region IDeclarationChargeProvider Members

		ZDate IDeclarationChargeProvider.EffectiveDutyDate => EffectiveDutyDate.Date;

		bool IDeclarationChargeProvider.IsS162ATemporaryImport
		{
			get
			{
				var result = false;
				foreach (var line in InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (!NoProcessingChargeTreatmentCodes.Contains((string)line.AddInfo.ZA_TreatmentCode_Hidden))
					{
						result = false;
						break;
					}
					result = true;
				}
				return result;
			}
		}

		bool IDeclarationChargeProvider.IsSOFADeclaration => IsSOFA;

		TransportModeEnum IDeclarationChargeProvider.TransportMode => Declaration?.TransportModeEnum ?? TransportModeEnum.Undefined;

		int IDeclarationChargeProvider.NumberOfFCLContainers => GetNumberOfContainersWithTypes(Core.Constants.ContainerModes.FCL);

		int IDeclarationChargeProvider.NumberOfFCXContainers => GetNumberOfContainersWithTypes(Core.Constants.ContainerModes.FCLMixedShipper);

		int IDeclarationChargeProvider.NumberOfLCLContainers => GetNumberOfContainersWithTypes(Core.Constants.ContainerModes.LCL);

		int GetNumberOfContainersWithTypes(string containerMode)
		{
			var result = 0;
			if (Declaration != null && Declaration.IsSea)
			{
				foreach (var container in Containers)
				{
					if (container.CO_FCL_LCL_AIR == containerMode)
					{
						result++;
					}
				}
			}
			return result;
		}

		ZDecimal IDeclarationChargeProvider.N10CustomsValue
		{
			get
			{
				SetCustomsValuesByNature();
				return n10CustomsValue;
			}
		}

		ZDecimal IDeclarationChargeProvider.N20CustomsValue
		{
			get
			{
				SetCustomsValuesByNature();
				return n20CustomsValue;
			}
		}

		ZDecimal IDeclarationChargeProvider.N30CustomsValue => IsNature30 ? CustomsValue : ZDecimal.Zero;

		bool IDeclarationChargeProvider.IsExemptedFromCustomsAndQuarantineFees => Declaration != null && Declaration.IsUPEDeclaration && Declaration.IsImporterDiplomat;

		void SetCustomsValuesByNature()
		{
			if (!isCustomsValuesByNatureSet)
			{
				n10CustomsValue = ZDecimal.Zero;
				n20CustomsValue = ZDecimal.Zero;
				if (IsNature10)
				{
					n10CustomsValue = CustomsValue;
				}
				else if (IsNature20)
				{
					n20CustomsValue = CustomsValue;
				}
				else if (IsNature1020)
				{
					foreach (CusEntryLine line in MergedLines)
					{
						if (line.IsNature20)
						{
							n20CustomsValue += line.CL_CustomsValue;
						}
						else
						{
							n10CustomsValue += line.CL_CustomsValue;
						}
					}
				}
				isCustomsValuesByNatureSet = true;
			}
		}
		ZDecimal n10CustomsValue;
		ZDecimal n20CustomsValue;
		bool isCustomsValuesByNatureSet;

		#endregion

		#region IDocumentSupportable Members

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

		public ZBool IsAllowedToPrintATD => AuthorityToDealMessage != null && !ATDSecurityCode.IsEmpty;

		public const string ATDNotPrintableMessageText = "Only Entries that contain ATD Messages and ATD Code are allowed to generate Authority To Deal document.";

		#endregion

		#region IMessageAttachee Members

		ZString IMessageAttachee.UserFriendlyCode
		{
			get
			{
				var result = new ZStringBuilder();
				result.Append("Entry No:");
				result.Append(EntryNumber);
				result.Append("/");
				result.Append("Ref No:");
				result.Append(CH_BGMReference);
				return result.ToString();
			}
		}

		ZBool IMessageAttachee.IsValidToSendThisMessageType(MessageAttacheeMessageType messageType)
		{
			var result = false;
			if (!IsWaitingForResponse && !IsWithdrawn)
			{
				if (messageType == MessageAttacheeMessageType.Original)
				{
					result = !IsStatusPostLodge;
				}
				else if (messageType == MessageAttacheeMessageType.Amend || messageType == MessageAttacheeMessageType.Withdraw)
				{
					result = IsStatusPostLodge;
				}
			}
			return result;
		}

		#endregion

		#region IHeaderFeeData Members

		IEnumerable<ILineDutyData> IHeaderFeeData.Lines => new TypedEnumerable<ILineDutyData>(MergedLines);

		void IHeaderFeeData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
			Charges.SetAmount(feeType, feeAmount);
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo => AddInfo;

		#endregion

		#region ICusCodeDataTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.DeletedPackingGroupData, typeof(DeletedPackingGroupData));
			return result;
		}

		#endregion

		#region ICMRMessageRespondeeReference Members

		ZString ICMRMessageRespondeeReference.GetHTMLFormatDetailsIfNeeded(ZString details, bool isForHtml)
		{
			ICMRMessageRespondeeReference declaration = Declaration;
			return declaration != null ? declaration.GetHTMLFormatDetailsIfNeeded(details, isForHtml) : details;
		}

		#endregion
	}
}
