using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.DeclarationStatusUpdater;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using EventParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), "CustomsEntryHeaders")]
	[VisualizableDocumentsSupportable("CusEntryHeaderEUVisualizableDocumentSupporter")]
	public class CusEntryHeader : TypeSafeCusEntryHeader, Integration.Customs.EU.ICusEntryHeader, IGuaranteeJobParent, IAddInfoChildOverrideTypeSupporter, IUcc6ValueProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : TypeSafeCusEntryHeader.Schema
		{
			public const string EntryTypeFriendlyName = "EntryTypeFriendlyName";
			public const string CRN = nameof(CusEntryHeader.CRN);
			public const string CustomsDocStatus = nameof(CusEntryHeader.CustomsDocStatus);
			public const string CustomsDocStatusDesc = nameof(CusEntryHeader.CustomsDocStatusDesc);
			public const int TotalCustomsQuantityDecimalPlaces = 6;
			public const int TotalNetWeightInKGDecimalPlaces = 6;
			public const int TotalGrossWeightInKGDecimalPlaces = 6;
		}

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly CusEntryHeaderTypeDecider TypeDecider = new CusEntryHeaderTypeDecider();

		protected override ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		[ChildEditable(true)]
		public new ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> Charges => (ICusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		public IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterList() => GetTaxBoxSupporterListCore();

		protected virtual IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => null;

		#region Auto rating stuff
		protected override ZDecimal GetTotalChargeValueFor(Enterprise.Registry.Business.Customs.EntryChargeType chargeTypeElement, ZString methodOfPaymentCode)
		{
			var rateCodes = GetRateCodes(chargeTypeElement.Code).Distinct();
			ZDecimal result = 0m;

			result += GetEntryHeaderChargesValue(rateCodes, methodOfPaymentCode);

			foreach (CusEntryLine entryLine in MergedLines)
			{
				var fees = UseConfirmedFeesForAutoRating ? entryLine.ConfirmedFees.Cast<CusEntryLineFee>().ToArray() : entryLine.Fees.Cast<CusEntryLineFee>().ToArray();
				var nonZeroFees = fees.Where(x => x.CF_ChargeAmount != 0);
				if (nonZeroFees.Any())
				{
					foreach (var rateCode in rateCodes)
					{
						result += nonZeroFees.Where(cf => cf.CF_ChargeType == rateCode
													&& !cf.CF_IsLandedCostOnly
													&& (CanAddDeferredFeeToTotalChargeValue || !TaxFeePaymentCodeIsDeferred(cf.CF_MethodOfPayment))
													&& (methodOfPaymentCode.IsEmpty || cf.CF_MethodOfPayment == methodOfPaymentCode))
												.Sum(x => x.CF_ChargeAmount);
					}
				}
			}
			return result;
		}

		protected bool UseConfirmedFeesForAutoRating => ConfirmedCharges.Any() || HasAnyConfirmedFeesOnAnyMergedLine;

		protected virtual ZDecimal GetEntryHeaderChargesValue(IEnumerable<ZString> rateCodes, ZString methodOfPaymentCode) => 0m;

		protected override IEnumerable<ZString> GetRateCodesCore(ZString rateType)
		{
			var codes = base.GetRateCodesCore(rateType).ToList();
			if (rateType.EqualsIgnoringCase(Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat))
			{
				codes.Add(UniversalReferenceConstants.RefCusRateCodes.Vat);
			}
			return codes;
		}

		public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPaymentCode, ILogger logger)
		{
			var entryFeePaymentPartyUnderstander = Declaration.GetEntryFeePaymentPartyUnderstander(this);
			if (entryFeePaymentPartyUnderstander == null)
			{
				return base.IsFeePaidByBroker(feeCode, methodOfPaymentCode, logger); // The EU country does not implement its own EntryFeePaymentPartyUnderstander and should use base condition
			}
			return entryFeePaymentPartyUnderstander.ShouldBrokerPayThisFee(feeCode, methodOfPaymentCode, logger);
		}

		protected virtual bool CanAddDeferredFeeToTotalChargeValue => true;

		#endregion

		protected override short[] ErrorLineNumbersFromLastResponseMessage()
		{
			List<short> badEntryLines = new List<short>();
			foreach (CusEntryLine entryLine in MergedLines)
			{
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					if (invoiceLine.ZG_HadErrorInLastResponse)
					{
						badEntryLines.Add(entryLine.CL_LineNumber);
						break;
					}
				}
			}
			return badEntryLines.ToArray();
		}
		public ZString CustomsDocStatus => EntryInstruction?.CustomsDocStatus ?? ZString.Empty;
		public ZPropertyInfo CustomsDocStatusInfo => GetWrappedZPropertyInfo(CusEntryInstruction.Schema.CustomsDocStatus, x => EntryInstruction?.CustomsDocStatusInfo ?? GetZPropertyInfo(CusEntryInstruction.Schema.CustomsDocStatus));
		public ZString CustomsDocStatusDesc => EntryInstruction?.CustomsDocStatusDesc ?? ZString.Empty;
		public ZPropertyInfo CustomsDocStatusDescInfo => GetWrappedZPropertyInfo(CusEntryInstruction.Schema.CustomsDocStatusDesc, x => EntryInstruction?.CustomsDocStatusDescInfo ?? GetZPropertyInfo(CusEntryInstruction.Schema.CustomsDocStatusDesc));

		[DecimalPlaces(Schema.TotalCustomsQuantityDecimalPlaces)]
		public ZDecimal TotalCustomsQuantity => TotalCustomsQuantityCore;

		protected virtual ZDecimal TotalCustomsQuantityCore => (ZDecimal)(InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_CustomsQuantity));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.UQList))]
		public ZString TotalCustomsUQ => Core.Constants.Weight.Kilograms;

		public ZDecimal TotalGrossWeight => (ZDecimal)(InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_Weight));

		protected IEnumerable<CusEntryLine> MergedLinesAsList => MergedLines.Cast<CusEntryLine>();

		[DecimalPlaces(Schema.TotalNetWeightInKGDecimalPlaces)]
		public ZDecimal TotalNetWeightInKG => MergedLinesAsList.Sum(x => x.EffectiveNetWeight.InKilogramsSafe);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.UQList))]
		public ZString TotalNetWeightUQ => Core.Constants.Weight.Kilograms;

		[DecimalPlaces(Schema.TotalGrossWeightInKGDecimalPlaces)]
		public ZDecimal TotalGrossWeightInKG => GetTotalGrossWeightInKG();
		protected virtual ZDecimal GetTotalGrossWeightInKG() => MergedLinesAsList.Sum(x => x.GrossWeight.InKilogramsSafe);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.UQList))]
		public ZString TotalGrossWeightUQ => Core.Constants.Weight.Kilograms;

		public ZInt MergedLinesCount => MergedLines.Count;

		public ZString EntryInstructionWarehouseCode
		{
			get
			{
				var orgHeader = EntryInstruction?.CEI_OA_Warehouse2_ZAddress?.OrgHeader as OrgHeader;
				return orgHeader?.OH_Code ?? ZString.Empty;
			}
		}

		public ZString InvoiceCurrency => TotalPrice?.Currency?.Code ?? ZString.Empty;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public ZString ProcedureCodeWithoutConcession => AllEntryLines.Cast<CusEntryLine>().Select(x => x.ProcedureCodeWithoutConcession).Distinct().Count() == 1 ? AllEntryLines.Cast<CusEntryLine>().Select(x => x.ProcedureCodeWithoutConcession).FirstOrDefault() : ZString.Empty;

		public virtual ZString EntryTypeFriendlyName
		{
			get
			{
				var procedureCodeWithoutConcession = ProcedureCodeWithoutConcession;
				return string.Format(CultureInfo.CurrentCulture, "{0} ({1}{2}{3})", EntryInstruction?.CEI_Style, Declaration?.JE_EntryStyle,
					EntryInstruction?.CEI_SubStyle, procedureCodeWithoutConcession.IsEmpty ? string.Empty : string.Format(CultureInfo.CurrentCulture, " / {0}", procedureCodeWithoutConcession));
			}
		}

		/// <summary>
		///  Box S13 on ESS
		/// </summary>
		public List<ZString> CountriesOfRouting => GetCountriesOfRoutingCore();

		protected virtual List<ZString> GetCountriesOfRoutingCore()
		{
			var legs = new List<Transport>();
			if (Declaration != null && Declaration.Shipment != null)
			{
				// Dec is on a shipment, let the shipment pull the countries through from the consol
				foreach (Transport trans in Declaration.Shipment.TransportsInLegOrder)
				{
					legs.Add(trans);
				}
			}
			else if (Declaration.Transports != null & Declaration.Transports.Count > 0)
			{
				// Standalone dec - pull them from the Routing tab
				foreach (Transport trans in Declaration.Transports)
				{
					legs.Add(trans);
				}
			}

			var isOriginAndDestinationRequiredInItinerary = IsOriginAndDestinationRequiredInItinerary;
			if (!isOriginAndDestinationRequiredInItinerary && legs.Count == 0)
			{
				return new List<ZString>();
			}

			string load = GetLoadPortForItinerary();
			string discharge = GetDischargePortForItinerary();

			var unlocos = GetRoutingFromTransports(legs, load, discharge);

			if (IsOriginAndDestinationRequiredInItinerary)
			{
				var copyOfList = new List<ZString>();
				copyOfList.Add(load);
				copyOfList.AddRange(unlocos);
				copyOfList.Add(discharge);
				unlocos = copyOfList;
			}

			var result = new List<ZString>();
			foreach (var unlocoCode in unlocos)
			{
				var country = Declaration.GetDefaultTerritory(unlocoCode);
				if (CanRepeatCountriesInItinerary || !result.Contains(country))
				{
					result.Add(country);
				}
			}

			if (!IsOriginAndDestinationRequiredInItinerary) //Check load and discharge are not duplicated when unlocos processed
			{
				var countryLoad = Declaration.GetDefaultTerritory(load);
				var countryDischarge = Declaration.GetDefaultTerritory(discharge);
				result.Remove(countryLoad);
				result.Remove(countryDischarge);
			}

			return result;
		}

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("827D6335-4BEC-459F-9B8E-3BF23649A0AD", Caption = "Customs Registration Number", ShortCaption = "CRN")]
		public ZString CRN
		{
			get => CRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				crnEntryNumber = CRNEntryNumber;
				if ((crnEntryNumber?.CE_EntryNum ?? ZString.Empty) != value)
				{
					if (crnEntryNumber == null)
					{
						crnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.EU.CustomsRegistrationNumber, CountryCode);
						RegisterEditableChildObject(crnEntryNumber);
					}
					crnEntryNumber.CE_EntryNum = value;
					CRNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CRNInfo => GetZPropertyInfo(Schema.CRN);

		public CusEntryNumber CRNEntryNumber
		{
			get
			{
				if (crnEntryNumber == null || crnEntryNumber.IsDeleted)
				{
					crnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.EU.CustomsRegistrationNumber, CountryCode);
					RegisterEditableChildObject(crnEntryNumber);
				}
				return crnEntryNumber;
			}
		}
		CusEntryNumber crnEntryNumber;

		protected virtual bool IsOriginAndDestinationRequiredInItinerary => false;

		protected virtual bool CanRepeatCountriesInItinerary => false;

		protected virtual string GetLoadPortForItinerary() => Declaration.JE_RL_NKPortOfLoading.ToUpper();

		protected virtual string GetDischargePortForItinerary() => Declaration.JE_RL_NKPortOfArrival.ToUpper();

#if DEBUG
		public bool IsOriginAndDestinationRequiredInItinerary_ForTest => IsOriginAndDestinationRequiredInItinerary;
		public bool CanRepeatCountriesInItinerary_ForTest => CanRepeatCountriesInItinerary;
#endif
		List<ZString> GetRoutingFromTransports(List<Transport> legs, ZString load, ZString discharge)
		{
			var unlocos = new List<ZString>();
			foreach (var trans in legs)
			{
				if (trans.LoadPort != null && trans.LoadPort.Country != null)
				{
					var depCode = trans.LoadPort.RL_Code;
					if (!depCode.IsEmpty && !unlocos.Contains(depCode))
					{
						unlocos.Add(depCode.ToUpper());
					}
				}
				if (trans.DiscPort != null && trans.DiscPort.Country != null)
				{
					var arrCode = trans.DiscPort.RL_Code;
					if (!arrCode.IsEmpty && !unlocos.Contains(arrCode))
					{
						unlocos.Add(arrCode.ToUpper());
					}
				}
			}

			// Snip load and disch from the list even for those countroies that want them, to ensure we put them in the first and last positions exactly
			unlocos.Remove(load);
			unlocos.Remove(discharge);

			return unlocos;
		}

		public override bool HasBeenWithdrawn
		{
			get { return false; }
		}

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			return CH_EntryStatus != EntryStatusList.Codes.Cancelled;
		}

		public override ZGuid CH_JE
		{
			get { return base.CH_JE; }
			set
			{
				JobDeclaration originalDeclaration = Declaration;
				bool hasChanged = CH_JE != value;
				base.CH_JE = value;
				if (hasChanged && !IsCopying)
				{
					ReCalculateStatusDetails();
					if (Declaration != null)
					{
						Declaration.DeriveDeclarationStatus();
						Declaration.InvoiceLines.MarkAsNeedingValidation();
						AllEntryLines.MarkAsNeedingValidationIncludingChildren();
					}
					if (originalDeclaration != null)
					{
						originalDeclaration.DeriveDeclarationStatus();
					}
				}
			}
		}

		[ResourceStringData("D4FE2A26-305F-4B04-B4D8-5768ADFCE583", Caption = "Message Status")]
		public override ZString CH_Status
		{
			get { return base.CH_Status; }
			set
			{
				bool hasChanged = CH_Status != value;
				base.CH_Status = value;
				if (hasChanged && !IsCopying)
				{
					ReCalculateStatusDetails();
					if (Declaration != null)
					{
						Declaration.DeriveDeclarationStatus();
					}
					UpdateHighestLineNumber();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_EntryStatusList))]
		[ResourceStringData("510F7465-73C5-450E-844B-FBAC40F34513", Caption = "Entry Status")]
		public override ZString CH_EntryStatus
		{
			get { return base.CH_EntryStatus; }
			set
			{
				var hasChanged = CH_EntryStatus != value;
				base.CH_EntryStatus = value;
				if (hasChanged && !IsCopying)
				{
					if (Declaration != null)
					{
						Declaration.DeriveDeclarationStatus();
					}
					UpdateHighestLineNumber();
				}

				if (ShouldConfirmTemporaryStorageGoodsConsumption)
				{
					ConfirmTemporaryStorageGoodsConsumptionIfNeeded(Factory,
						EntryInstruction?.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty,
						CountryCode,
						CH_EntryStatus,
						TemporaryStorageTransactionInternalReferenceNumber,
						TemporaryStorageTransactionInternalReferenceType,
						CH_BGMReference,
						PreviousDocumentCodeForDataToReserveTemporaryStorageGoods,
						GetEntryLineDataDeclaredToReserveTSGoods,
						MovementReferenceNumber,
						TemporaryStorageTransactionCommentPrefix,
						Declaration?.JE_DeclarationReference ?? ZString.Empty,
						MovementReferenceNumberIssueDate,
						CH_EntryReleaseDate,
						TemporaryStorageWriteOffTransactionCommentReferenceNumber,
						Logs,
						CustomsStatusToCancelTemporaryStoragePendingTransactions,
						CustomsStatusToNotCreateTemporaryStorageTransactions,
						CustomsStatusToConfirmTemporaryStoragePendingTransactions,
						isLAME: IsLAMETemporaryStorage,
						formatDocRef: ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation ? GetDocumentNumberFormat : null);
				}
			}
		}

		void UpdateHighestLineNumber()
		{
			if (LockNumberOfEntryLines)
			{
				if (CH_HighestLineNumber == 0)
				{
					CH_HighestLineNumber = (short)(!MergedLines.Any() ? 0 : MergedLines.OfType<CusEntryLine>().Max(x => x.CL_LineNumber));
				}
			}
			else if (Declaration?.Configuration.LockNumberOfEntryLinesForRegisteredEntry ?? false)
			{
				CH_HighestLineNumber = 0;
			}
		}

		#region Confirm/Reserve Temporary Storage Goods

		protected virtual IReadOnlyList<ZString> PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => Array.Empty<ZString>();

		protected virtual ZBool ShouldConfirmTemporaryStorageGoodsConsumption => false;

		protected virtual ZBool IsLAMETemporaryStorage => false;

		public ZString TemporaryStorageTransactionInternalReferenceNumber => TemporaryStorageTransactionInternalReferenceNumberCore;
		protected virtual ZString TemporaryStorageTransactionInternalReferenceNumberCore => ZString.Empty;

		public ZString TemporaryStorageTransactionInternalReferenceType => TemporaryStorageTransactionInternalReferenceTypeCore;
		protected virtual ZString TemporaryStorageTransactionInternalReferenceTypeCore => ZString.Empty;

		protected virtual IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => Array.Empty<ZString>();

		protected virtual ZString TemporaryStorageTransactionCommentPrefix => ZString.Empty;

		protected virtual ZString TemporaryStorageWriteOffTransactionCommentReferenceNumber => ZString.Empty;

		public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoods() => GetEntryLineDataDeclaredToReserveTSGoodsCore();
		protected virtual (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsCore() => (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), ZString.Empty);

		protected virtual ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => false;

		protected virtual ZString GetDocumentNumberFormat(ZString dsdtMRN) => dsdtMRN;

		#endregion

		protected override bool ShouldCompletelyReassignNumbersCore => !LockNumberOfEntryLines && base.ShouldCompletelyReassignNumbersCore;

		public override ZString CH_MessageType
		{
			get { return base.CH_MessageType; }
			set
			{
				var hasChanged = CH_MessageType != value;
				base.CH_MessageType = value;
				if (hasChanged && !IsCopying)
				{
					if (Declaration != null)
					{
						Declaration.InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("D05017D2-9853-4202-8DD2-EB1E2B38202F", ShortCaption = "Duties + Taxes", Caption = "Total Duties and Taxes Amount")]
		public override ZDecimal CH_TotalPaid { get => base.CH_TotalPaid; set => base.CH_TotalPaid = value; }

		[ResourceStringData("06E0DEB3-7DBE-4BCB-82EE-6E1AA3B10303", Caption = "MRN")]
		public override ZString MovementReferenceNumber => base.MovementReferenceNumber;

		protected virtual bool IsMrnEntryNumberTheOneWeWantToShow => false;

		protected override ZString EntryNumberType => IsMrnEntryNumberTheOneWeWantToShow ? CusEntryNumberTypes.Standard.MovementReferenceNumber : base.EntryNumberType;

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			return CusEntryNumber.Load(this, EntryNumberType, CountryCode);
		}

		public ZString OfficeOfExit => Declaration.OfficeOfExit;

		public ZString OfficeOfEntry => Declaration.OfficeOfEntry;

		public ZString TransportChargesMoP => RandomHeader?.ZG_TransportChargesMethodOfPayment ?? ZString.Empty;

		public void SetAsFailedFromTransmission()
		{
			var previousCH_Status = CH_Status;
			CH_Status = MessageStatusList.Codes.FailedFromTransmission;
			if (ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction)
			{
				CH_EntryStatus = MessageStatusList.Codes.FailedFromTransmission;
				DeclarationEntryStatusUpdater.Update(Declaration);
			}
			AfterSetAsFailedFromTransmission(previousCH_Status);
		}

		protected virtual void AfterSetAsFailedFromTransmission(ZString previousCH_Status) { }

		protected virtual ZBool ShouldSetAsFailedEntryStatusInSetFailedFromTrasmissionAction => ZBool.True;

		protected override ZDateTime GetCustomsClearedDateWithoutUsingLoggedEvent()
		{
			return CH_EntryReleaseDate;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (ShouldSetUCRinBGMReferenceNumber)
			{
				FillInBGMReferenceAndMasterUCR();
			}
			ReCalculateStatusDetails();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			LogCENEventIfApplicable();
			LogCRNEventIfApplicable();
			RenderSADInEDocsIfApplicable();
			LogStatusIfRequired(ShouldLogStatus, CH_StatusInfo, CH_Status, Events.MessageStatusChange);
			LogStatusIfRequired(ShouldLogPhaseStatus, CH_PhaseStatusInfo, CH_PhaseStatus, Events.PhaseStatusChange);
		}

		protected virtual bool ShouldLogStatus => false;

		protected virtual bool ShouldLogPhaseStatus => false;

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (IsInDatabase)
				{
					CH_EntryStatus = (ZString)CH_EntryStatusInfo.OriginalValue;
					CH_EntrySubmittedDate = (ZDateTime)CH_EntrySubmittedDateInfo.OriginalValue;
				}
				else
				{
					CH_EntryStatus = ZString.Empty;
					CH_EntrySubmittedDate = ZDateTime.Empty;
				}
			}
			base.OnSaved(saveSucceeded);
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded && PublishEventsOnSaved.Any(l => l != null))
			{
				foreach (var log in PublishEventsOnSaved.Where(l => l != null))
				{
					JobDeclarationWarehouseExtensions.PublishUniversalEvent(log.RecipientRoleTypes.ToArray(), this, new[] { Declaration.Company.OrgProxy }, log.EventBO);
				}
				PublishEventsOnSaved.Clear();
			}
			base.OnFactorySaved(saveSucceeded);
		}

		public const string UCRReferencePlaceHolder = "<<UCRREF>>";
		public const string UCRPartPlaceHolder = "<<UCRPART>>";
		public const string BGMReferencePlaceHolderXmlFriendly = "BGMREFERENCEPLACEHOLDER415A6A0C9690475B8AA2D2AE9D237BE4";
		public const string UCRReferencePlaceHolderXmlFriendly = "UCRREFERENCEPLACEHOLDER3A3C3059CEBF44D08B4E1DE9048AA854";
		public const string UCRPartPlaceHolderXmlFriendly = "UCRPARTPLACEHOLDERCFF7FF233A9B469B933AA91E2F6BA6AC";

		#region Implementation

		public bool IsFailedFromTransmission => IsFailedFromTransmissionCore;
		protected virtual bool IsFailedFromTransmissionCore => CH_EntryStatus == MessageStatusList.Codes.FailedFromTransmission;

		protected override bool IsStatusChangedToClearedForLoggingCLREvent
		{
			get { return CH_EntryStatus == EntryStatusList.Codes.Clear; }
		}

		// This is generally for CH_Status, and by 'clear' base means 'accepted'.
		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			return originalStatus != MessageStatusList.Codes.OK && newStatus == MessageStatusList.Codes.OK;
		}

		protected override bool IsStatusChangingFromAmendmentPendingToCleared(ZString originalStatus, ZString newStatus)
		{
			return newStatus == MessageStatusList.Codes.OK;
		}

		protected override bool IsStatusChangingFromNotAcceptedToAccepted(string oldStatus, string newStatus)
		{
			bool result = false; // stop update CH_HighestLineNumber for the countries which not allow to add or delete entry lines after registed.
			if (!(Declaration?.Configuration.LockNumberOfEntryLinesForRegisteredEntry ?? false))
			{
				result = oldStatus != MessageStatusList.Codes.OK && newStatus == MessageStatusList.Codes.OK;
			}
			return result;
		}

		protected virtual void ReCalculateStatusDetails()
		{
			if (CH_Status != MessageStatusList.Codes.NotSent)
			{
				if (CH_EntrySubmittedDate.IsEmpty)
				{
					CH_EntrySubmittedDate = ZDateTime.Now;
				}
			}
			if (CH_Status == MessageStatusList.Codes.AwaitingResponse && (CH_EntryStatus == EntryStatusList.Codes.NotSent || CH_EntryStatus == EntryStatusList.Codes.SentAndInitiallyRejected || CH_EntryStatus == MessageStatusList.Codes.FailedFromTransmission))
			{
				CH_EntryStatus = MessageStatusList.Codes.AwaitingResponse;
			}
		}

		void LogCENEventIfApplicable()
		{
			if (IsCustomsNumberEnteredEventSupported && ShouldLogCustomsNumberEnteredEvent && Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsNumberEntered) == null)
			{
				var log = Logs.AddNew(Events.CustomsNumberEntered, CusEntryNumber.CE_IssueDate.ToOffset(), CRNAndCENEventParameters.ToArray()).WithRecipients(RecipientRoleTypesForCENOrCRNEvent);
				if (ShouldPublishUMXLEventForTransitWarehouse)
				{
					PublishEventsOnSaved.Add(log);
				}
			}
		}

		void LogCRNEventIfApplicable()
		{
			if (IsCustomsReleaseNumberEnteredEventSupported && ShouldLogCustomsReleaseNumberEnteredEvent && Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReleaseNumberEntered) == null)
			{
				var log = Logs.AddNew(Events.CustomsReleaseNumberEntered, CH_EntryReleaseDate.ToOffset(), CRNAndCENEventParameters.ToArray()).WithRecipients(RecipientRoleTypesForCENOrCRNEvent);
				if (ShouldPublishUMXLEventForTransitWarehouse)
				{
					PublishEventsOnSaved.Add(log);
				}
			}
		}

		void RenderSADInEDocsIfApplicable()
		{
			if (!PK.IsEmpty && Logs.MostRecentLogByEventTimeExcludingEstimated(Declaration.CustomsClearedEventType) is StmALog log && !log.IsInDatabase && EUCustomsDataRegistry.Instance.SADGenerationOnClearanceEnabled.Value)
			{
				new SADEDocsSaver(this).RenderDocumentAndSaveInEDocs();
			}
		}

		protected List<StmALogWithRecipients> PublishEventsOnSaved
		{
			get { return publishEventsOnSaved ?? (publishEventsOnSaved = new List<StmALogWithRecipients>()); }
		}
		List<StmALogWithRecipients> publishEventsOnSaved;

		IDictionary<string, string> CRNAndCENEventParameters => new Dictionary<string, string>() { { EventParameterCodes.CustomsReferenceNumber, EntryNumber }, { EventParameterCodes.HouseBill, Declaration.JE_HouseBill }, { EventParameterCodes.OuterPackQuantity, PackagesCount.ToString() }, { EventParameterCodes.InnerPackQuantity, (EntryInstruction?.CEI_TotalInnerPackages ?? ZInt.Zero).ToString() } };

		protected virtual bool IsCustomsNumberEnteredEventSupported => false;

		protected virtual bool ShouldLogCustomsNumberEnteredEvent => !EntryNumber.IsEmpty;

		protected virtual bool IsCustomsReleaseNumberEnteredEventSupported => false;

		protected virtual bool ShouldLogCustomsReleaseNumberEnteredEvent => CH_EntryStatus == EntryStatusList.Codes.Clear;

		bool ShouldPublishUMXLEventForTransitWarehouse => Factory.GetCached(ref shouldPublishUMXLEventForTransitWarehouseCache, () =>
		{
			return MergedLines.Cast<CusEntryLine>().Any(line =>
			{
				var result = false;
				if (line.CusProcedure is RefCusProcedure procedure)
				{
					result = !(procedure.IsIntoRegime() && procedure.IsOutOfRegime());
				}
				return result;
			});
		});
		CachedProperty<bool> shouldPublishUMXLEventForTransitWarehouseCache;

		RecipientRoleType[] RecipientRoleTypesForCENOrCRNEvent => IsImport ? new[] { RecipientRoleType.ATW } : (IsExport ? new[] { RecipientRoleType.DTW } : Array.Empty<RecipientRoleType>());

		internal void FillInBGMReferenceAndMasterUCR()
		{
			if (!IsInDatabase && CH_BGMReference.IsEmpty)
			{
				Declaration.PopulateUCRIfNeeded();
				var ucrMaybeWithTraingSuffix = Declaration.JE_UCR + TrainingEnvironmentSuffix;
				if (LoadForBGMReference(Factory, ucrMaybeWithTraingSuffix) != null)
				{
					var maxAllow = CusEntryHeaderSchema.CH_BGMReference.MaxLength - ucrMaybeWithTraingSuffix.Length - 1;
					for (var i = 1; i < Math.Pow(10, maxAllow); i++)
					{
						ucrMaybeWithTraingSuffix = Declaration.JE_UCR + TrainingEnvironmentSuffix + "/" + i.ToString();
						if (LoadForBGMReference(Factory, ucrMaybeWithTraingSuffix) == null)
						{
							break;
						}
					}
				}
				CH_BGMReference = ucrMaybeWithTraingSuffix;
			}
			else
			{
				UpdateBgmAndCusEntryNumToMatchNewUCRIfNotLodged(Declaration.JE_UCRInfo);
			}

			InitialiseMasterUCR();
		}

		public virtual void InitialiseMasterUCR()
		{
		}

		void UpdateBgmAndCusEntryNumToMatchNewUCRIfNotLodged(ZPropertyInfo ucrInfo)
		{
			if (!HasBeenLodgedAtCustoms && CH_Status != MessageStatusList.Codes.AwaitingResponse && (ucrInfo.HasChanges || Declaration.ZG_IsTrainingDeclarationInfo.HasChanges))
			{
				var oldPartSuffix = DeclarationUCRPartSuffix;
				var ducr = (ZString)(ucrInfo.Value + TrainingEnvironmentSuffix);
				var newBgm = oldPartSuffix.IsEmpty ? ducr : ZString.Format("{0}/{1}", ducr, oldPartSuffix);
				CH_BGMReference = newBgm; // this will also update the CE_EntryNum.
			}
		}

		public bool TaxFeePaymentCodeIsDeferred(ZString cF_MethodOfPayment)
		{
			return TaxFeePaymentCodeIsDeferredCore(cF_MethodOfPayment);
		}

		protected virtual bool TaxFeePaymentCodeIsDeferredCore(ZString cF_MethodOfPayment)
		{
			return false;
		}

		ZString TrainingEnvironmentSuffix
		{
			get { return Declaration != null && Declaration.ZG_IsTrainingDeclaration ? new ZString("T") : ZString.Empty; }
		}

		#endregion

		public new JobComInvoiceHeader RandomHeader
		{
			get { return base.RandomHeader as JobComInvoiceHeader; }
		}

		public override ZString CH_BGMReference
		{
			get { return base.CH_BGMReference; }
			set
			{
				base.CH_BGMReference = value;
				LoadOrCreateUCRNumber(value);
			}
		}

		#region TotalPrice

		public Money TotalPrice => Factory.GetValue(ref totalPriceCache, GetTotalPrice);
		CachedProperty<Money> totalPriceCache;

		Money GetTotalPrice()
		{
			var result = Money.Empty;
			foreach (CusEntryLine entryLine in MergedLines)
			{
				result = CurrencyConverter.Add(result, entryLine.Price);
			}
			return result;
		}

		public ZDecimal TotalPriceAmount => TotalPrice.Amount;

		#endregion

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return Declaration.GetCusEntryHeaderDocumentSupporter(this) ?? base.CreateNewDocumentSupporter();
		}

		#region Organization

		#region Supplier
		public JobDocAddress Supplier => Declaration?.SupplierDocumentaryAddress;

		public ZString SupplierEoriOfMainOffice => SupplierEoriOfMainOfficeCore;
		protected virtual ZString SupplierEoriOfMainOfficeCore => Supplier.GetEuIdentificationNumber();
		#endregion

		#region Importer
		public JobDocAddress Importer => Declaration.ImporterDocumentaryAddress;

		public ZString ImporterEoriOfMainOffice => ImporterEoriOfMainOfficeCore;
		protected virtual ZString ImporterEoriOfMainOfficeCore => Importer.GetEuIdentificationNumber();
		#endregion

		#region Declarant
		public OrgHeader DeclarantOrganisation => Declaration?.Declarant?.Header;

		public ZString RepresentativeOrDeclarantEoriOfMainOffice => RepresentativeOrDeclarantEoriOfMainOfficeCore;

		protected virtual ZString RepresentativeOrDeclarantEoriOfMainOfficeCore => DeclarantOrganisation.GetEuIdentificationNumber();
		#endregion

		#region Seller
		public OrgHeader SellerOrganisation => Declaration?.Seller;

		public ZString SellerEoriOfMainOffice => SellerOrganisation.GetEuIdentificationNumber();
		#endregion

		#region Buyer
		public JobDocAddress Buyer => Declaration?.BuyerDocAddress;

		public ZString BuyerEoriOfMainOffice => Buyer.GetEuIdentificationNumber();
		#endregion

		#region Representative
		public OrgHeader RepresentativeOrganisation => Declaration?.Representative?.Header;

		public ZString RepresentativeEoriOfMainOffice => RepresentativeOrganisation.GetEuIdentificationNumber();
		#endregion

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("c7e8afc3-5171-4f3b-bb6b-e7e40c825160", "Customs Entry {0}", CH_BGMReference);
			}
		}

		public bool DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries { get; set; }

		public ISet<ZString> DutyCodeSet => DutyCodeSetCore();

		protected virtual ISet<ZString> DutyCodeSetCore()
		{
			return new HashSet<ZString> { DutyCode };
		}

		public ISet<ZString> TaxCodeSet => TaxCodeSetCore();

		protected virtual ISet<ZString> TaxCodeSetCore()
		{
			return new HashSet<ZString> { TaxCode };
		}

		public new ZDecimal Duty
		{
			get { return SumLineFees(DutyCodeSet); }
		}

		protected override string GetDutyCode()
		{
			return UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		}

		protected internal virtual IReadOnlyList<ZString> ExcludedFeeCodesForAllOtherFees => new ZString[] { DutyCode, TaxCode, // Don't inlcude these in the "other" section as they'll be covered in their own calculations
			Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, Core.Constants.Customs.CusEntryFeeTypes.TotalAmountPayable }; // Exclude these Aussie codes.  New jobs wont have them but very old ones will have them (e.g. a copy of the A00 value in DTY, etc)

		public ZDecimal DutyImmediate => SumLineFees(fee => DutyCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsImmediate(fee.CF_MethodOfPayment));

		public ZDecimal DutyDeferred => SumLineFees(fee => DutyCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment));

		public ZDecimal VATImmediate => SumLineFees(fee => TaxCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsImmediate(fee.CF_MethodOfPayment));

		public ZDecimal VATDeferred => SumLineFees(fee => TaxCodeSet.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment));

		public ZDecimal AllOtherFeesImmediate => SumLineFees(fee => !ExcludedFeeCodesForAllOtherFees.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsImmediate(fee.CF_MethodOfPayment));

		public ZDecimal AllOtherFeesDeferred => SumLineFees(fee => !ExcludedFeeCodesForAllOtherFees.Contains(fee.CF_ChargeType) && TaxFeePaymentCodeIsDeferred(fee.CF_MethodOfPayment));

		public ZDecimal AllOtherFees => SumLineFees(fee => !ExcludedFeeCodesForAllOtherFees.Contains(fee.CF_ChargeType));

		protected virtual ZBool TaxFeePaymentCodeIsImmediate(ZString methodOfPayment) => !TaxFeePaymentCodeIsDeferred(methodOfPayment);

		public new ZDecimal VAT
		{
			get { return SumLineFees(TaxCodeSet); }
		}

		decimal SumLineFees(ISet<ZString> feeCodeSet)
		{
			if (HasAnyConfirmedFeesOnAnyMergedLine)
			{
				return (from CusEntryLine line in MergedLines from CusEntryLineFee fee in line.ConfirmedFees where feeCodeSet.Contains(fee.CF_ChargeType) select (decimal)fee.CF_ChargeAmount).Sum();
			}
			else
			{
				return (from CusEntryLine line in MergedLines from CusEntryLineFee fee in line.Fees where feeCodeSet.Contains(fee.CF_ChargeType) select (decimal)fee.CF_ChargeAmount).Sum();
			}
		}

		public ZDecimal DutyTotalUnion
		{
			get
			{
				return SumLineFees(DutyTotalUnionList());
			}
		}

		public ISet<ZString> DutyTotalUnionList()
		{
			var listRateCode = Factory.GetCachedRatesByType(Declaration.GetDefaultDataGroupingCode(), new ZString[] { Constants.RateTypes.Duty, Constants.RateTypes.AntiDumping, Constants.RateTypes.Countervailing }).Select(x => x.ZY1_RateCode);
			return listRateCode.ToArray().ToHashSet();
		}

		public decimal SumLineFees(string feeCode)
		{
			return (from CusEntryLine line in MergedLines from CusEntryLineFee fee in line.Fees where fee.CF_ChargeType == feeCode select (decimal)fee.CF_ChargeAmount).Sum();
		}

		public decimal SumLineFees(Func<CusEntryLineFee, bool> customFilterFunc)
		{
			CargoWise.Common.Argument.NotNull(customFilterFunc, nameof(customFilterFunc));
			return (from CusEntryLine line in MergedLines from CusEntryLineFee fee in line.Fees where customFilterFunc.Invoke(fee) select (decimal)fee.CF_ChargeAmount).Sum();
		}

		public decimal SumLineConfirmedFees(Func<CusEntryLineFee, bool> customFilterFunc)
		{
			CargoWise.Common.Argument.NotNull(customFilterFunc, nameof(customFilterFunc));

			return (from CusEntryLine line in MergedLines from CusEntryLineFee fee in line.ConfirmedFees.Any() ? line.ConfirmedFees.Cast<CusEntryLineFee>().ToArray() : line.Fees.Cast<CusEntryLineFee>().ToArray() where customFilterFunc.Invoke(fee) select (decimal)fee.CF_ChargeAmount).Sum();
		}

		protected override string GetTaxCode()
		{
			return UniversalReferenceConstants.RefCusRateCodes.Vat;
		}

		public static string BondedWarehouseAddressShouldBeInsideDeclarationRegionEU(string importerCode, string entryDetail, string country)
		{
			return Res.GetString("{38558EF6-E7B6-4428-AEE5-6FFA3EA6CC91}", "Inventory recording/Bonded Warehouse Integration is active for Importer '{0}'. Please enter a Bonded Warehouse on Entry ({1}) that is located in {2} area (EU).", importerCode, entryDetail, country);
		}

		#region AdditionalInfos

		public IEnumerable<AdditionalInfo> AdditionalInfos => Factory.GetCachedAggregatedData(ref additionalInfosCached, () => Declaration?.AdditionalInfoKeys.ToArray() ?? Array.Empty<string>(), GetAdditionalInfosToProcess);
		CachedProperty<IEnumerable<AdditionalInfo>> additionalInfosCached;

		protected virtual IEnumerable<AdditionalInfo> GetAdditionalInfosToProcess()
		{
			if (Declaration is JobDeclaration declaration)
			{
				foreach (AdditionalInfo document in declaration.AdditionalInfos)
				{
					yield return document;
				}

				foreach (JobComInvoiceHeader invoiceHeader in InvoiceHeaders)
				{
					foreach (var document in invoiceHeader.AdditionalInfos.Cast<AdditionalInfo>().Where(_ => _.IsHeaderOnly))
					{
						yield return document;
					}
				}
			}
		}
		#endregion

		#region SupportingDocuments
		public IEnumerable<SupportingDocument> SupportingDocuments => Factory.GetCachedAggregatedData(ref supportingDocumentsCached, () => Declaration?.CreateEntryCreationStrategy().GetSupportingDocumentKeys() ?? Array.Empty<string>(), GetSupportingDocumentsToProcess);
		CachedProperty<IEnumerable<SupportingDocument>> supportingDocumentsCached;

		protected virtual IEnumerable<SupportingDocument> GetSupportingDocumentsToProcess()
		{
			if (Declaration is JobDeclaration declaration)
			{
				foreach (SupportingDocument document in declaration.SupportingDocuments)
				{
					yield return document;
				}
				foreach (JobComInvoiceHeader invoiceHeader in InvoiceHeaders)
				{
					foreach (var document in invoiceHeader.EffectiveSupportingDocuments())
					{
						yield return document;
					}
				}
			}
		}
		#endregion

		#region PreviousDocuments
		public IEnumerable<PreviousDocument> PreviousDocuments => Factory.GetCachedAggregatedData(ref previousDocumentsCached, () => Declaration?.PreviousDocumentKeys.ToArray() ?? Array.Empty<string>(), GetPreviousDocumentsToProcess);
		CachedProperty<IEnumerable<PreviousDocument>> previousDocumentsCached;

		protected virtual IEnumerable<PreviousDocument> GetPreviousDocumentsToProcess()
		{
			if (SupportsPreviousDocumentsAtEntryHeaderLevel)
			{
				foreach (PreviousDocument document in Declaration.PreviousDocuments)
				{
					yield return document;
				}
			}
			else
			{
				if (InvoiceHeaders.Any(x => x.GroupHeader != null))
				{
					foreach (PreviousDocument document in Declaration.PreviousDocuments)
					{
						yield return document;
					}
				}
				foreach (JobComInvoiceHeader invoiceHeader in InvoiceHeaders)
				{
					foreach (PreviousDocument document in invoiceHeader.PreviousDocuments)
					{
						yield return document;
					}
				}
			}
		}

		public bool SupportsPreviousDocumentsAtEntryHeaderLevel => SupportsPreviousDocumentsAtEntryHeaderLevelCore;
		protected virtual bool SupportsPreviousDocumentsAtEntryHeaderLevelCore => false;

		public virtual bool IsIndirectExport => Declaration.IsExport && OfficeOfExit != ZString.Empty && !OfficeOfExit.StartsWith(Declaration.CountryCode, StringComparison.OrdinalIgnoreCase);
		#endregion

		public ComplementaryJob ComplementaryJob
		{
			get
			{
				ComplementaryJob result = null;
				if (!ComplementaryJobPreviousDocumentCodeType.IsEmpty && !ComplementaryJobPreviousDocumentEntryReferenceType.IsEmpty)
				{
					var number = PreviousDocuments.FirstOrDefault(x => x.CSI_Code == ComplementaryJobPreviousDocumentCodeType)?.CSI_ReferenceNumber ?? ZString.Empty;

					if (!number.IsEmpty)
					{
						var entryNumber = CusEntryNumber.Load(Factory, ComplementaryJobPreviousDocumentEntryReferenceType, number, CountryCode).FirstOrDefault();

						var parent = entryNumber != null ? Factory.Load<CusEntryHeader>(entryNumber.CE_ParentID) : null;

						result = new ComplementaryJob
						{
							Reference = number,
							ParentTableCode = entryNumber?.CE_ParentTable ?? null,
							Parent = parent
						};
					}
				}

				return result;
			}
		}

		public ZString ComplementaryJobPreviousDocumentCodeType => ComplementaryJobPreviousDocumentCodeTypeCore;

		protected virtual ZString ComplementaryJobPreviousDocumentCodeTypeCore => EUCommonConstants.PreviousDocumentCodeList.Cleared;

		public ZString ComplementaryJobPreviousDocumentEntryReferenceType => ComplementaryJobPreviousDocumentEntryReferenceTypeCore;

		protected virtual ZString ComplementaryJobPreviousDocumentEntryReferenceTypeCore => Common.CusEntryNumberTypes.Standard.MovementReferenceNumber;

		public ZString SadBoxAText => SadBoxATextCore;

		protected virtual ZString SadBoxATextCore => ZString.Empty;

		public IEnumerable<AmountAndTypeToBeGuaranteed> AmountAndTypeToBeGuaranteeds
		{
			get
			{
				return Factory.GetCached(ref amountAndTypeToBeGuaranteedsCache, () =>
				{
					var groupingAmountAndTypeToBeGuaranteeds = MergedLines.SelectMany(x => x.AmountAndTypeToBeGuaranteeds).GroupBy(x => x.DebitType);

					var list = new List<AmountAndTypeToBeGuaranteed>();
					foreach (var groupingAmountAndTypeToBeGuaranteed in groupingAmountAndTypeToBeGuaranteeds)
					{
						var sum = groupingAmountAndTypeToBeGuaranteed.Sum(x => x.AmountInDeclarationCurrency);
						list.Add(new AmountAndTypeToBeGuaranteed
						{
							DebitType = groupingAmountAndTypeToBeGuaranteed.Key,
							AmountInDeclarationCurrency = sum
						});
					}

					return list;
				});
			}
		}
		CachedProperty<IEnumerable<AmountAndTypeToBeGuaranteed>> amountAndTypeToBeGuaranteedsCache;

		protected override bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivot => true;

		ZInt IGuaranteeJobParent.PackageCount => PackagesCount;

		ZDecimal IGuaranteeJobParent.AmountToBeGuaranteedInDeclarationCurrency => AmountAndTypeToBeGuaranteeds.Sum(x => x.AmountInDeclarationCurrency);

		public virtual ZBool ShouldSetUCRinBGMReferenceNumber => true;

		public ZBool LockNumberOfEntryLines => lockNumberOfEntryLinesCore;
		protected virtual ZBool lockNumberOfEntryLinesCore => (Declaration?.Configuration.LockNumberOfEntryLinesForRegisteredEntry ?? false) && (HasBeenLodgedAtCustoms || IsWaitingForResponse);

		protected override bool ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked => !LockNumberOfEntryLines;

		#region IAddInfoChildSupporter Members

		protected override BusinessObject GetAddInfoChild() => AddInfoChild;

		protected override SchemaGuidColumn GetChildForeignKeyColumn() => CusEUEntryHeaderSchema.EUH_CH;

		#endregion

		Type IAddInfoChildOverrideTypeSupporter.AddInfoChildType => new CusEUEntryHeaderTypeDecider().GetTypeForCountryCode(Declaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		public override ZString ReferenceNumber
		{
			get
			{
				var entryNumber = EntryNumber;
				return entryNumber.IsEmpty ? CH_BGMReference : entryNumber;
			}
		}

		protected override ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore => new EuCommonGoodsItemsIntegrator(this);

		protected override bool GetSupportsBondedWarehousingForEntry(Customs.Business.CusEntryInstruction entryInstruction)
		{
			return ((entryInstruction.ClientIsBondedWarehousing || entryInstruction.Warehouse2IsBondedWarehousing) && entryInstruction.HasIntoVATWarehouseProcedure)
				|| base.GetSupportsBondedWarehousingForEntry(entryInstruction);
		}

		#region IUcc6ValueProvider

		bool IUcc6ValueProvider.IsUCC6 => Declaration?.IsUCC6 ?? false;

		#endregion

		#region SimplifiedDeclarationMRN
		public ZString SimplifiedDeclarationMRN => SIMEntryNumber?.CE_EntryNum ?? ZString.Empty;

		public void SetSimplifiedDeclarationMRN(ZString mrn)
		{
			var simEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.EU.SimplifiedDeclarationMRN, CountryCode);
			simEntryNumber.CE_EntryIsSystemGenerated = true;
			simEntryNumber.CE_EntryNum = mrn;
		}

		protected CusEntryNumber SIMEntryNumber
		{
			get
			{
				if (simEntryNumber == null)
				{
					simEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.SimplifiedDeclarationMRN, CountryCode);
				}
				return simEntryNumber;
			}
		}
		CusEntryNumber simEntryNumber;
		#endregion

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.ExportExitStatusList))]
		public override ZString CH_ExitedStatus { get => base.CH_ExitedStatus; set => base.CH_ExitedStatus = value; }
	}
}
