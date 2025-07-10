using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.Snapshot;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[DescriptionProperty(nameof(CH_BGMReference))]
	[VisualizableDocumentsSupportable("CusEntryHeaderFRVisualizableDocumentSupporter")]
	public class CusEntryHeader : AutoFRCusEntryHeader,
		Integration.Customs.FR.ICusEntryHeader,
		ICorrelationIDProvider,
		IHeaderFeeData,
		ICINMessage745,
		ICINMessage755,
		Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter,
		IFRMessagesOwner,
		ITemporaryStorageRegisterTransactionDataProvider
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoFRCusEntryHeader.Schema
		{
			public const string CINLevelREF = "REF";
			public const string CINLevelHWB = "HWB";
			public const string CINLevelAWB = "AWB";
			public const string CINOACICode = "CIN";
			public const string SequenceNumber = "CH_SequenceNumber";
			public const string IsCancelledWithCustoms = "IsCancelledWithCustoms";
			public const string CorrelationID = "CorrelationID";
			public const string SpecialMentionForFallbackProcedureDeSecours = "50000";
			public const int CorrelationMaxLength = 19;
			public const string FallbackEntryType = "FBK";
			public const int FRCustomsFallbackEntryNumberMaxLength = 10;
			public const string FRCustomsFallbackNumber = "FRCustomsFallbackNumber";
			public const string AI2 = "AI2";
			public const string CH_CalcTotalInvoicedAmountInLocalCurrency = "CH_CalcTotalInvoicedAmountInLocalCurrency";
		}

		#endregion

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		#region AddInfo

		protected override EU.Business.Declaration.AddInfoCusEntryHeader GetNewAddInfoCusEntryHeader() => new AddInfoCusEntryHeader(CH_AddInfoInfo);

		internal new AddInfoCusEntryHeader AddInfo => (AddInfoCusEntryHeader)base.AddInfo;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			CorrelationIdGenerator.InitCorrelationID(IsUniqueCorrelationID);
			LogADCEventIfSavingFirstTimeOrTriggeringPointForValidationUpdated();
		}

		protected override bool IsStatusChangedToClearedForLoggingCLREvent => StatusChangedToClearedSinceLoading;

		protected override bool ShouldUpdateHeldUniversalShipmentEntryStatus(UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader entryXml, ZString newEntryStatus) => base.ShouldUpdateHeldUniversalShipmentEntryStatus(entryXml, newEntryStatus) || !newEntryStatus.IsEmpty;

		protected override bool ShouldUpdateHeldUniversalShipmentMessageStatus(UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader entryXml, ZString newMessageStatus) => base.ShouldUpdateHeldUniversalShipmentEntryStatus(entryXml, newMessageStatus) || !newMessageStatus.IsEmpty;

		void LogADCEventIfSavingFirstTimeOrTriggeringPointForValidationUpdated()
		{
			if (!IsInDatabase || CH_TriggeringPointForValidationInfo.HasChanges)
			{
				Logs.AddNew(Events.ArrivalDetailsChanged, $"VAA Trigger = {CH_TriggeringPointForValidation}", ZDateTimeOffset.Now);
			}
		}

		void LogCOEEventIfNeeded()
		{
			if (Logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.ConfirmationOfExit) != null)
			{
				return;
			}

			var keys = new HashSet<string>()
			{
				ExportControlStatusList.Codes.APE,ExportControlStatusList.Codes.SOR,ExportControlStatusList.Codes.ESO
			};

			if (keys.Contains(CH_ExitedStatus))
			{
				var log = Logs.AddNew(AutoEvents.ConfirmationOfExit, CH_ExitedStatus).WithRecipients(new[] { RecipientRoleType.DTW });
				PublishEventsOnSaved.Add(log);
			}
		}

		bool IsUniqueCorrelationID(ZString correlationID)
		{
			return NumberGeneratorHelper.IsUniqueEntryNumber(Factory, TableName, correlationID, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			CorrelationIdGenerator.ClearCorrelationIDOnSaved(saveSucceeded);
			base.OnSaved(saveSucceeded);
		}

		#region CorrelationID

		[ReadOnly(true)]
		[MaxLength(Schema.CorrelationMaxLength)]
		public ZString CorrelationID
		{
			get { return CorrelationIDEntryNumber.CE_EntryNum; }
			set { CorrelationIDEntryNumber.CE_EntryNum = value; }
		}

		public ZPropertyInfo CorrelationIDInfo { get { return GetWrappedZPropertyInfo(Schema.CorrelationID, x => CorrelationIDEntryNumber.CE_EntryNumInfo); } }

		public CusEntryNumber CorrelationIDEntryNumber
		{
			get
			{
				if (correlationIDEntryNumber == null)
				{
					correlationIDEntryNumber = new CachedProperty<CusEntryNumber>(Factory, delegate
					{
						var correlationEntryNumberInternal = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode);
						if (correlationEntryNumberInternal == null)
						{
							correlationEntryNumberInternal = CusEntryNumber.New(this, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode);
							correlationEntryNumberInternal.CE_EntryIsSystemGenerated = true;
							correlationEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
						}
						return correlationEntryNumberInternal;
					});
				}
				return correlationIDEntryNumber.Value;
			}
		}
		CachedProperty<CusEntryNumber> correlationIDEntryNumber;

		public CorrelationIDGenerator CorrelationIdGenerator => correlationIdGenerator ?? (correlationIdGenerator = new CorrelationIDGenerator(this, this?.Declaration));
		CorrelationIDGenerator correlationIdGenerator;

		public ZString CorrelationIDPrefix => Declaration?.ApplicationExtender.GetCorrelationIDPrefix() ?? ZString.Empty;
		#endregion

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new ICusEntryLineCollection<CusEntryLine> MergedLines
		{
			get { return (ICusEntryLineCollection<CusEntryLine>)base.MergedLines; }
		}

		public IEnumerable<CusEntryLine> T2LApplicableEntryLines => MergedLines.Where(line => line.IsT2LApplicable);

		public IEnumerable<CusEntryLine> T2LFApplicableEntryLines => MergedLines.Where(line => line.IsT2LFApplicable);

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
		{
			return new CusEntryLineCollection<CusEntryLine>(this);
		}

		[ChildEditable(true)]
		public new FREDIMessageCollection Messages => (FREDIMessageCollection)base.Messages;

		protected override EDIMessageCollection GetNewMessageCollection()
		{
			return new FREDIMessageCollection(this);
		}

		ZString GetEntryStatusDescription()
		{
			return Declaration.ApplicationExtender.GetEntryStatusDescription(Declaration, CH_EntryStatus);
		}

		public override ZString EntryHeaderStatusDescription => GetEntryStatusDescription();

		ZString GetExitStatusDescription()
		{
			var result = ZString.Empty;

			if (CH_ExitedStatus == ExportControlStatusList.Codes.APE)
			{
				result = $"/ESO-PA";
			}
			else if (CH_ExitedStatus != ZString.Empty)
			{
				result = $"/{CH_ExitedStatus}";
			}

			return result;
		}

		public ZString EntryHeaderEcsStatusDescription => GetExitStatusDescription();

		ZString GetMessageStatusDescription()
		{
			return Factory.GetCachedValue<MessageStatusCodeList>().GetDescriptionFromCode(CH_Status) ?? ZString.Empty;
		}

		public override ZString MessageStatusDescription => GetMessageStatusDescription();

		public TaxStructCollection ChargesAsTaxes()
		{
			var result = new TaxStructCollection();

			var taxes = new Dictionary<MergeKey, FRTaxStruct>();
			foreach (EU.Business.Declaration.CusEntryHeaderCharges charge in Charges)
			{
				var mergeKey = new MergeKey();
				mergeKey.Add(charge.C1_ChargeType);

				FRTaxStruct frTax;
				if (!taxes.TryGetValue(mergeKey, out frTax))
				{
					frTax = new FRTaxStruct();
					result.Add(frTax);
					taxes[mergeKey] = frTax;
				}
				frTax?.AddTax(charge);
			}
			return result;
		}

		#region FRCustomsFallbackEntryNumber

		public ZString DeltaGFallbackStatus
		{
			get { return FRCustomsFallbackEntryNumber?.CE_EntryStatus ?? ZString.Empty; }
			set
			{
				if (FRCustomsFallbackEntryNumber != null)
				{
					FRCustomsFallbackEntryNumber.CE_EntryStatus = value;
				}
			}
		}

		public ZDateTime DeltaGFallbackIssueDate
		{
			get { return FRCustomsFallbackEntryNumber?.CE_IssueDate ?? ZDateTime.Empty; }
			set
			{
				if (FRCustomsFallbackEntryNumber != null)
				{
					FRCustomsFallbackEntryNumber.CE_IssueDate = value;
				}
			}
		}

		[ReadOnly(true)]
		[MaxLength(Schema.FRCustomsFallbackEntryNumberMaxLength)]
		public ZString FRCustomsFallbackNumber
		{
			get { return FRCustomsFallbackEntryNumber?.CE_EntryNum ?? ZString.Empty; }
			set
			{
				if (FRCustomsFallbackEntryNumber.CE_EntryNum != value)
				{
					CheckMaximumLength(FRCustomsFallbackNumberInfo, value);
					FRCustomsFallbackEntryNumber.CE_EntryNum = value;
					FRCustomsFallbackNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FRCustomsFallbackNumberInfo => GetZPropertyInfo(Schema.FRCustomsFallbackNumber);

		CusEntryNumber FRCustomsFallbackEntryNumber
		{
			get
			{
				if (frCustomsFallbackEntryNumber == null)
				{
					frCustomsFallbackEntryNumber = new CachedProperty<CusEntryNumber>(Factory, delegate
					{
						var frCustomsFallbackEntryNumberInternal = CusEntryNumber.Load(this, Schema.FallbackEntryType, CountryCode);
						if (frCustomsFallbackEntryNumberInternal == null)
						{
							frCustomsFallbackEntryNumberInternal = CusEntryNumber.New(this, Schema.FallbackEntryType, CountryCode);
							frCustomsFallbackEntryNumberInternal.CE_EntryIsSystemGenerated = true;
							frCustomsFallbackEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
						}
						return frCustomsFallbackEntryNumberInternal;
					});
				}
				return frCustomsFallbackEntryNumber.Value;
			}
		}
		CachedProperty<CusEntryNumber> frCustomsFallbackEntryNumber;

		#endregion

		#region CIN Message
		ZString UniqueMessageNumber => ZDateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture) + FRConstants.MessageSpecialCharacter.MessageID;

		ZString BGMReference => ZDateTime.Now.DayOfYear.ToString("000", CultureInfo.InvariantCulture) + FRConstants.MessageSpecialCharacter.MessageID;

		ZDecimal GetCINGrossWeight()
		{
			var weightInKilos = 0m;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				var grossWeight = new ZWeight(invoiceLine.JI_Weight, invoiceLine.JI_WeightUQ);
				weightInKilos += grossWeight.InKilogramsSafe;
			}
			return weightInKilos;
		}

		ZString GetMagasin()
		{
			var magasin = ZString.Empty;
			var addressDepot = Declaration.DepotDocAddress?.Address;
			var addressCTO = Declaration.ContainerTerminalOperatorDocAddress?.Address;

			if (addressDepot != null)
			{
				var customsRegNo = addressDepot.Header?.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, addressDepot.PK) ?? ZString.Empty;
				if (customsRegNo.IsEmpty)
				{
					if (addressCTO != null)
					{
						customsRegNo = addressCTO.Header?.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, addressCTO.PK) ?? ZString.Empty;

						if (customsRegNo.IsEmpty)
						{
							customsRegNo = addressDepot.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, ZGuid.Empty) ?? ZString.Empty;

							if (customsRegNo.IsEmpty)
							{
								customsRegNo = addressCTO.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, ZGuid.Empty) ?? ZString.Empty;
							}
						}
					}
					else
					{
						customsRegNo = addressDepot.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, ZGuid.Empty) ?? ZString.Empty;
					}
				}

				magasin = customsRegNo;
			}
			else
			{
				if (addressCTO != null)
				{
					var customsRegNo = addressCTO.Header?.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, addressCTO.PK) ?? ZString.Empty;
					magasin = customsRegNo.IsEmpty ? (addressCTO.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.CIN, Core.Constants.CountryCodes.France, ZGuid.Empty) ?? ZString.Empty) : customsRegNo;
				}
			}
			return magasin;
		}

		#region CIN Message 745

		(ZString masterbill, ZString homeBill) GetCIN745NameAndLevel()
		{
			var name = CH_BGMReference;
			var level = Schema.CINLevelREF;
			if (!Declaration.JE_MasterBill.IsEmpty)
			{
				name = Declaration.JE_MasterBill;
				level = Schema.CINLevelAWB;
			}
			else if (!Declaration.JE_HouseBill.IsEmpty)
			{
				name = Declaration.JE_HouseBill;
				level = Schema.CINLevelHWB;
			}
			return (name, level);
		}
		ZInt ICINMessage745.MrnQuantity => Declaration.JE_TotalNoOfPacks;
		ZDecimal ICINMessage745.MrnWeight => GetCINGrossWeight();
		ZString ICINMessage745.Name => GetCIN745NameAndLevel().masterbill;
		ZString ICINMessage745.Level => GetCIN745NameAndLevel().homeBill;
		ZString ICINMessage745.ExitOfOffice => Declaration?.OfficeOfExitCode ?? ZString.Empty;
		ZString ICINMessage745.MrnNumber => LoadCusEntryNumber(false)?.CE_EntryNum ?? ZString.Empty;
		ZString ICINMessage745.OACI_Shipper => Declaration?.Supplier?.CustomsCodes.GetCustomsRegNo(Schema.CINOACICode, Core.Constants.CountryCodes.France) ?? ZString.Empty;
		ZString ICINMessage745.OACI_Carrier => Declaration?.ShippingLine?.CustomsCodes.GetCustomsRegNo(Schema.CINOACICode, Core.Constants.CountryCodes.France) ?? ZString.Empty;
		ZString ICINMessage745.BGMReference => BGMReference;
		ZString ICINMessage745.TransactionIDCIN745 => CH_BGMReference;
		ZString ICINMessage745.Magasin => GetMagasin();

		#endregion

		#region CIN Message755

		public ZString CustomOffice
		{
			get
			{
				return (Declaration.CustomsOffices?.Cast<EuOfficeCode>().FirstOrDefault(co => co.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit
													&& co.CY_Type == EU.Business.CusCodeDataTypeList.Codes.OfficeCode
													&& co.CY_Data != ZString.Empty))?.CY_Data ?? ZString.Empty;
			}
		}

		ZString ICINMessage755.UniqueMessageNumber => UniqueMessageNumber;

		ZString ICINMessage755.BGMReference => BGMReference;

		ZString ICINMessage755.Date => ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);

		ZString ICINMessage755.NumLTA => Declaration?.JE_MasterBill ?? ZString.Empty;

		ZString ICINMessage755.OACI_Shipper => FRCustomsDataRegistry.Instance.CINSenderID.Value;
		ZString ICINMessage755.OACI_Carrier => GetCINCarrier();

		ZString ICINMessage755.RefDos => this.CH_BGMReference;

		ZString ICINMessage755.Hwb => Declaration?.JE_HouseBill ?? ZString.Empty;

		ZString ICINMessage755.StoreCode => Declaration?.JE_SubLocationOfGoods ?? ZString.Empty;

		ZString ICINMessage755.PackagesCount => PackagesCount.ToString();

		ZString ICINMessage755.GrossWeight
		{
			get
			{
				var weight = GetCINGrossWeight();
				return weight.IsEmpty ? string.Empty : weight.ToZInt().ToString();
			}
		}

		ZString ICINMessage755.MrnNumber => LoadCusEntryNumber(false)?.CE_EntryNum ?? ZString.Empty;

		ZString ICINMessage755.CustomOffice => CustomOffice;

		ZString ICINMessage755.BAEDate => LoadCusEntryNumber(false)?.CE_IssueDate.ToString("yyyyMMdd") ?? ZString.Empty;

		ZString GetCINCarrier()
		{
			var masterBillPrefix = Declaration.JE_MasterBill.Left(3);
			var result = RefAirline.LoadFromAirlinePrefix(Factory, masterBillPrefix)?.RM_ThreeLetterCode ?? ZString.Empty;
			return result != ZString.Empty ? result : masterBillPrefix;
		}

		ZString ICINMessage755.Magasin => GetMagasin();

		#endregion
		#endregion

		#region SequenceNumber (Numéro de séquence)

		[ResourceStringData("241E8F14-BC8A-4E60-A948-FE32913A5218", Caption = "Sequence Number")]
		public ZInt CH_SequenceNumber
		{
			get { return AddInfo.ZG_SequenceNumber; }
			set
			{
				AddInfo.ZG_SequenceNumber = value;
			}
		}

		public ZPropertyInfo CH_SequenceNumberInfo => GetWrappedZPropertyInfo(Schema.SequenceNumber, x => AddInfo.ZG_SequenceNumberInfo);

		#endregion

		#region CusEntryNumber
		protected override ZString EntryNumberType => Declaration?.JE_MessageType ?? ZString.Empty;

		protected CusEntryNumber LoadCusEntryNumber(bool create)
		{
			if (cusEntryNumber == null)
			{
				cusEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);

				if (cusEntryNumber == null && create)
				{
					cusEntryNumber = CusEntryNumber.New(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
					cusEntryNumber.CE_ParentID = PK;
				}
				RegisterEditableChildObject(cusEntryNumber);
			}
			return cusEntryNumber;
		}
		CusEntryNumber cusEntryNumber;
		#endregion

		public ZBool IsCancelledWithCustoms => !CH_EntryStatus.IsEmpty && CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES090;

		#region IHeaderFeeData Members

		IEnumerable<ILineDutyData> IHeaderFeeData.Lines
		{
			get { return new TypedEnumerable<ILineDutyData>(MergedLines); }
		}

		void IHeaderFeeData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
			Charges.SetAmount(feeType, feeAmount);
		}

		#endregion

		public ZBool IsBAE => CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES100;

		public ZBool IsVALOrBAEOrComplete => CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES060 || CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES100 || CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES130;

		public ZBool IsDeltaDStepOneSentOK => (Declaration?.IsDeltaD ?? false) && CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES100;

		public ZBool IsDeltaDStepTwoSentOK => (Declaration?.IsDeltaD ?? false) && CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES130;

		public ZBool IsDeltaDStepTwoSentOKButZeroLiquidation => IsDeltaDStepTwoSentOK && SumLineConfirmedFees(x => true) == 0m;

		public ZBool IsDeltaGFallbackInactiveAndNotRegularised => !FRCustomsDataRegistry.DeltaGFallbackIsActive && !DeltaGFallbackStatus.IsEmpty
															&& DeltaGFallbackStatus != DeltaGFallbackStatusList.Codes.RGA && DeltaGFallbackStatus != DeltaGFallbackStatusList.Codes.RGM;

		public bool IsAwaitingResponse => CH_Status == MessageStatusCodeList.Codes.AWR;

		public bool IsPromotionalProductToDROM => MergedLines.All(x => x.IsPromotionalProductToDROM);

		public bool IsProductOfNegligibleValueToDROM => MergedLines.All(x => x.IsProductOfNegligibleValueToDROM);

		public IEnumerable<SupportingDocument> SupportingDocumentsDTP => SupportingDocuments.OfType<SupportingDocument>().Where(x => x.CSI_IsDTP);

		[ChildEditable(true)]
		public new EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		protected override ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new EU.Business.Declaration.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		public new IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> ConfirmedCharges => (ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.ConfirmedCharges;

		protected override IConfirmedCusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewConfirmedCusEntryHeaderChargesCollection() => new ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override IEnumerable<IDocSADHLineTaxBoxSupporter> GetTaxBoxSupporterListCore() => ConfirmedCharges.Cast<IDocSADHLineTaxBoxSupporter>();

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("6903E3C6-605D-4F38-AD6D-D05AAB8F588E", Caption = "MRN #")]
		public ZString MRN
		{
			get => MRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (MRN != value)
				{
					if (MRNEntryNumber == null)
					{
						mrnEntryNumber = CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
					}
					MRNEntryNumber.CE_EntryNum = value;
					RegisterEditableChildObject(mrnEntryNumber);
					MRNInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(nameof(MRN));

		public CusEntryNumber MRNEntryNumber
		{
			get
			{
				if (mrnEntryNumber == null || mrnEntryNumber.IsDeleted)
				{
					mrnEntryNumber = CusEntryNumber.Load<CusEntryNumber>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
				}
				return mrnEntryNumber;
			}
		}
		CusEntryNumber mrnEntryNumber;

		public override ZGuid CH_JE
		{
			get => base.CH_JE;
			set
			{
				base.CH_JE = value;
				Charges.MarkAsNeedingValidation();
			}
		}

		public ZDecimal Ai2Amount
		{
			get
			{
				ZDecimal amount = 0;
				if (Declaration.ZG_VATDeferType == VATProcedureList.Codes._2)
				{
					amount = EntryLineFeesAi2Amount;
				}
				return amount;
			}
		}

		ZDecimal EntryLineFeesAi2Amount
		{
			get
			{
				ZDecimal amount = 0;
				foreach (CusEntryLine entryLine in AllEntryLines)
				{
					foreach (CusEntryLineFee entryFee in entryLine.Fees)
					{
						amount += entryFee.Ai2Amount;
					}
				}
				return amount;
			}
		}

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CusEntryHeader|CH_Calc_GuaranteeAmount", Caption = "Guarantee Amount")]
		public ZDecimal CH_Calc_GuaranteeAmount
		{
			get
			{
				if (calc_GuaranteeAmountCached == null)
				{
					calc_GuaranteeAmountCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						ZDecimal amount = 0;
						foreach (CusEntryLine entryLine in MergedLines)
						{
							amount += entryLine.CL_Calc_GuaranteeAmount;
						}
						return amount.Round(0);
					});
				}

				return calc_GuaranteeAmountCached.Value;
			}
		}

		CachedProperty<ZDecimal> calc_GuaranteeAmountCached;

		protected override ZString ComplementaryJobPreviousDocumentCodeTypeCore => PreviousDocumentCodeList.Codes.IM;

		protected override ZString ComplementaryJobPreviousDocumentEntryReferenceTypeCore => IsImport ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;

		protected override ZString SadBoxATextCore
		{
			get
			{
				var sb = new ZStringBuilder();
				sb.AppendIfNotEmpty(base.SadBoxATextCore);
				ZString officeDesc = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Declaration.JE_CustomsOffice, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today)?.ZZD_Description ?? ZString.Empty;
				sb.AppendFormat((NoResString)"Etat de la declaration: {0}{1}", EntryHeaderStatusDescription, EntryHeaderEcsStatusDescription);
				sb.AppendFormat((NoResString)"Bureau de présentation: {0}", officeDesc.IsEmpty ? Declaration.JE_CustomsOffice : new ZString(Declaration.JE_CustomsOffice + " - " + officeDesc));

				var officeOfDeclaration = Declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep);

				if (officeOfDeclaration != null)
				{
					sb.AppendFormat((NoResString)"Bureau de declaration: {0}", officeOfDeclaration.CY_Data + (officeOfDeclaration.CY_OfficeDescription.IsEmpty ? "" : " - " + officeOfDeclaration.CY_OfficeDescription));
				}

				if (FRCustomsDataRegistry.DeltaGFallbackIsActive)
				{
					sb.AppendFormat((NoResString)"No douane: {0}", FRCustomsFallbackNumber);
				}

				return sb.ToStringWithNewLineBetweenAppends();
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusEntryHeaderFetchStrategy(this);
		}

		public ZDecimal TotalD48Amount => AllEntryLines.Cast<CusEntryLine>().Sum(x => x.D48Amount);

		protected override ZString[] GetMethodsOfPaymentThatCanInfluenceAutoRatingCore()
			=> new ZString[] { FRConstants.MethodOfPayment._1, FRConstants.MethodOfPayment._2, FRConstants.MethodOfPayment._6 };

		protected override ZDecimal GetEntryHeaderChargesValue(IEnumerable<ZString> rateCodes, ZString methodOfPaymentCode)
		{
			ZDecimal result = 0m;
			var entryHeaderCharges = UseConfirmedFeesForAutoRating ? ConfirmedCharges.Cast<CusEntryHeaderCharges>().ToArray() : Charges.Cast<CusEntryHeaderCharges>().ToArray();
			var nonZeroCharge = entryHeaderCharges.Where(x => x.C1_ChargeAmount != 0);
			if (nonZeroCharge.Any())
			{
				foreach (var rateCode in rateCodes)
				{
					result += nonZeroCharge.Where(c1 => c1.C1_ChargeType == rateCode
																&& !c1.C1_IsLandedCostOnly
																&& (CanAddDeferredFeeToTotalChargeValue || !TaxFeePaymentCodeIsDeferred(c1.C1_MethodOfPayment))
																&& (methodOfPaymentCode.IsEmpty || c1.C1_MethodOfPayment == methodOfPaymentCode))
											.Sum(x => x.C1_ChargeAmount);
				}
			}

			return result;
		}

		protected override IList<PermitRecord> GetPermitRecordsCore()
		{
			var permitRecords = new List<PermitRecord>();

			var ai2Permit = Declaration.Ai2Permit;
			if (ai2Permit != null)
			{
				permitRecords.Add(new PermitRecord
				{
					PermitHeader = ai2Permit,
					Value = Ai2Amount,
					Quantity = 0,
					Procedure = Schema.AI2
				});
			}

			var guarantee = Declaration.CustomsGuarantee;
			if (guarantee != null)
			{
				foreach (var pair in AmountAndTypeToBeGuaranteeds)
				{
					if (pair.AmountInDeclarationCurrency > 0)
					{
						permitRecords.Add(new PermitRecord
						{
							PermitHeader = guarantee,
							Value = pair.AmountInDeclarationCurrency,
							Quantity = 0,
							Procedure = pair.Procedure
						});
					}
				}
			}
			return permitRecords;
		}

		protected override void ReCalculateStatusDetails()
		{
			base.ReCalculateStatusDetails();
			if (CH_EntryStatus == MessageStatusList.Codes.AwaitingResponse)
			{
				CH_EntryStatus = Declaration.ApplicationExtender.ReCalculateStatusDetails(this);
			}
		}

		public ZDecimal CustomsPackageCount
		{
			get
			{
				if (customsPackageCountCached == null)
				{
					customsPackageCountCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						ZDecimal result = 0;

						foreach (CusEntryLine entryLine in MergedLines)
						{
							var package = entryLine.Package;
							if (package != null)
							{
								result += package.IsUnpacked ? package.ItemsCount : (ZDecimal)package.Count;
							}
						}

						return result;
					});
				}

				return customsPackageCountCached.Value;
			}
		}

		CachedProperty<ZDecimal> customsPackageCountCached;

		public override bool ShouldLogEntryStatus => true;

		protected override bool ShouldLogCustomsReleaseNumberEnteredEvent => EntryActionHelper.IsVariousBAEStatus(CH_EntryStatus);

		protected override bool IsCustomsNumberEnteredEventSupported => true;

		protected override bool IsCustomsReleaseNumberEnteredEventSupported => true;

		public ZString LastNonIntermediateEntryStatus => Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code))
														.Where(x => new EntryStatusDescriptionCodeList().ContainsCode(x.SL_Reference) && !EntryActionHelper.IsIntermediateStatus(x.SL_Reference)).OrderByDescending(x => x.SL_EventTime).FirstOrDefault()?.SL_Reference.Left(3) ?? ZString.Empty;

		bool Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter.SupportModificationState => true;

		public ZString FallbackStampTitle => FRCustomsDataRegistry.DeltaGFallbackIsActive ? (ZString)string.Format(CultureInfo.InvariantCulture, (NoResString)"PROCÉDURE DE SECOURS / FALLBACK PROCEDURE") : ZString.Empty;
		public ZString FallbackStampText => FRCustomsDataRegistry.DeltaGFallbackIsActive ? (ZString)string.Format(CultureInfo.InvariantCulture, (NoResString)"AUCUNE DONNÉE DISPONIBLE DANS LE SYSTÈME\r\nENGAGÉE LE {0}", FRCustomsDataRegistry.DeltaGFallbackInvocationDate.ToString("dd/MM/yyyy A HH:mm", CultureInfo.CurrentCulture)) : ZString.Empty;

		protected override ZString RepresentativeOrDeclarantEoriOfMainOfficeCore => Declaration?.Declarant.GetEuIdentificationNumber() ?? ZString.Empty;

		protected override ZString SupplierEoriOfMainOfficeCore
		{
			get
			{
				var result = ZString.Empty;

				var fiscalRepresentative = EntryInstruction?.FiscalReferences?.Cast<CusFiscalReference>().FirstOrDefault(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
				if (Declaration.IsExport && fiscalRepresentative != null && SupplierUsesFiscalRepresentative)
				{
					result = fiscalRepresentative.CFR_Reference;
				}
				else
				{
					result = Supplier?.Address?.GetEORI() ?? ZString.Empty;
				}

				return result;
			}
		}

		protected override ZString ImporterEoriOfMainOfficeCore => Importer?.Address?.GetEORI() ?? ZString.Empty;

		public bool SupplierUsesFiscalRepresentative => OrgUsesFiscalRepresentative(Declaration.Supplier);

		public bool ImporterUsesFiscalRepresentative => OrgUsesFiscalRepresentative(Declaration.Importer);

		bool OrgUsesFiscalRepresentative(OrgHeader orgHeader)
		{
			var result = false;

			var euAddInfo = EUOrgImpAddInfo.Get(orgHeader, Declaration.CountryCode);
			if (euAddInfo != null)
			{
				euAddInfo.Deserialise();
				result = euAddInfo.ZO_UseFr3FiscalRepresentation;
			}

			return result;
		}

		#region FRCustomsFallbackEntryNumber

		// JEG: Remember to rollback everything in method "RollbackFallbackSpecialMention"
		public bool AddFallbackSpecialMention()
		{
			var fallbackSpecialMentionCreated = false;
			if (FRCustomsDataRegistry.DeltaGFallbackIsActive)
			{
				var existingFBKEntryNum = CusEntryNumber.Load(this, CusEntryHeader.Schema.FallbackEntryType, CountryCode)?.CE_EntryNum ?? ZString.Empty;
				if (existingFBKEntryNum == ZString.Empty && (CH_EntryStatus != EntryStatusDescriptionCodeList.Codes.ES100 || CH_EntryStatus != EntryStatusDescriptionCodeList.Codes.ES130))
				{
					if (!Declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours))
					{
						var specialMention = Declaration.AdditionalInfos.AddNew();
						specialMention.CSI_Code = CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours;
						fallbackSpecialMentionCreated = true;
					}
				}
			}

			return fallbackSpecialMentionCreated;
		}

		public void RollbackFallbackSpecialMention(bool rollbackFallbackSpecialMention)
		{
			if (FRCustomsDataRegistry.DeltaGFallbackIsActive)
			{
				if (rollbackFallbackSpecialMention)
				{
					var specialMentions = Declaration.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.CSI_Code == CusEntryHeader.Schema.SpecialMentionForFallbackProcedureDeSecours).ToList();
					specialMentions.ForEach(Declaration.AdditionalInfos.RemoveAndDelete);
				}
			}
		}

		public void InitFRCustomsFallbackEntryNumber()
		{
			var newFRCustomsFallbackEntryNumber = GetUniqueEntryFallbackEntryNumberReference(Factory);
			if (!newFRCustomsFallbackEntryNumber.IsEmpty && FRCustomsFallbackNumber.IsEmpty)
			{
				FRCustomsFallbackNumber = newFRCustomsFallbackEntryNumber;
			}
		}

		ZString GetUniqueEntryFallbackEntryNumberReference(BusinessObjectFactory factory)
		{
			var corel = GetEntryIsUniqueFallbackEntryNumberReference(factory);

			while (!NumberGeneratorHelper.IsUniqueEntryNumber(factory, CusEntryHeader.Schema.TableName, corel, CusEntryHeader.Schema.FallbackEntryType, CountryCode))
			{
				corel = GetEntryIsUniqueFallbackEntryNumberReference(factory);
			}

			return corel;
		}

		static ZString GetEntryIsUniqueFallbackEntryNumberReference(BusinessObjectFactory factory)
		{
			var target = new FRCustomsFallbackEntryNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.GetFRCustomsFallbackEntryNumberSendCounter(),
				FountainGetter = Env.NumberFountains.GetFRCustomsFallbackEntryNumberGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		#endregion

		#region Snapshot

		public ZString GetEntrySnapshotData()
		{
			var snapshots = new List<SnapshottedCusEntryLine>();
			foreach (CusEntryLine entryLine in this.MergedLines)
			{
				var childDatas = new List<ChildData>();

				GetLineDocuments(entryLine, childDatas);

				GetLineCanas(entryLine, childDatas);

				GetLineCacos(entryLine, childDatas);

				var lineSnapshot = new SnapshottedCusEntryLine(entryLine.CL_LineNumber, childDatas.ToArray());
				snapshots.Add(lineSnapshot);
			}

			return new CusEntryLineSnapshotDataBuilder(snapshots.ToArray()).GetSnapshot();
		}

		void GetLineDocuments(CusEntryLine entryLine, List<ChildData> childDatas)
		{
			foreach (SupportingDocument supDoc in this.SupportingDocuments)
			{
				childDatas.Add(new ChildData(ChildData.DOC, supDoc.CSI_Code, supDoc.CSI_ReferenceNumber));
			}

			var key = new List<string>();

			foreach (SupportingDocument supDoc in entryLine.SupportingDocuments)
			{
				if (!supDoc.CSI_IsDTP && (!supDoc.IsCodeAPermitType || !key.Contains(supDoc.CSI_Code + supDoc.CSI_ReferenceNumber)))
				{
					childDatas.Add(new ChildData(ChildData.DOC, supDoc.CSI_Code, supDoc.CSI_ReferenceNumber));
					key.Add(supDoc.CSI_Code + supDoc.CSI_ReferenceNumber);
				}
				else if (supDoc.CSI_IsDTP)
				{
					childDatas.Add(new ChildData(ChildData.DOC, supDoc.CSI_Code, ZString.Empty));
				}
			}
		}

		static void GetLineCanas(CusEntryLine entryLine, List<ChildData> childDatas)
		{
			foreach (var frAdditionalCode in entryLine.RandomLine.FRAdditionalCodes)
			{
				childDatas.Add(new ChildData(ChildData.CAN, frAdditionalCode, ZString.Empty));
			}
		}

		static void GetLineCacos(CusEntryLine entryLine, List<ChildData> childDatas)
		{
			foreach (var ceAdditionalCode in entryLine.RandomLine.CEAdditionalCodes)
			{
				childDatas.Add(new ChildData(ChildData.CAC, ceAdditionalCode, ZString.Empty));
			}
		}

		#endregion

		public IEnumerable<TemporaryStorageRegisterTransactionData> GetTemporaryStorageRegisterTransactionData()
		{
			var isDeltaIE = Declaration.IsDeltaIE;
			var groupedEntryLines = MergedLines
			.Where(x => x.RandomLine.PreviousISTDocument != null)
			.GroupBy(x => new
			{
				PreviousISTNumber = x.RandomLine.PreviousISTNumber,
				PreviousISTLineNo = isDeltaIE ? x.RandomLine.PreviousISTDocument.CSI_ItemNumber : x.RandomLine.PreviousISTDocument.CSI_LineNo
			});

			foreach (var entryLineGroup in groupedEntryLines)
			{
				var entryLine = entryLineGroup.First();
				var converter = entryLine.RandomLine.CustomsQuantityConverter;
				var transaction = new TemporaryStorageRegisterTransactionData(Factory);

				var grossMass = isDeltaIE
					? entryLineGroup.Sum(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Sum(inv => new ZWeight(inv.PreviousISTDocument.CSI_Quantity, converter.GetEffectiveWeightUnit(inv.PreviousISTDocument.CSI_UnitOfQuantity)).InKilogramsSafe))
					: entryLineGroup.Sum(x => x.EffectiveGrossWeight.InKilograms);

				var packageQty = isDeltaIE
					? entryLineGroup.Sum(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Sum(inv => inv.PreviousISTDocument.CSI_PackQty))
					: entryLineGroup.Sum(x => x.Package?.Count ?? 0);

				transaction.PreviousRegisterHeader = entryLine.PreviousISTHeader?.RegisterHeader;
				transaction.CustomsReferenceNumber = EntryNumber;
				transaction.InternalReferenceNumber = CH_BGMReference;
				transaction.GrossMass = grossMass;
				transaction.PackageQuantity = packageQty;
				transaction.ReferenceType = TempStorageTransactionRefTypeList.Codes.EntryHeader;
				transaction.Comments = $"Entry line {entryLine.CL_LineNumber}";
				transaction.RegisterLineNo = entryLineGroup.Key.PreviousISTLineNo;

				yield return transaction;
			}
		}

		#region AddInfos

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CusEntryHeader|CH_TriggeringPointForValidation", Caption = "VAA Trig. Point")]
		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.TriggeringPointForValidationList))]
		public override ZString CH_TriggeringPointForValidation { get => base.CH_TriggeringPointForValidation; set => base.CH_TriggeringPointForValidation = value; }

		#endregion

		public ZDateTime AssessmentDate => EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CusEntryHeader|CH_EntryReleaseDate", Caption = "First BAE Date")]
		public override ZDateTime CH_EntryReleaseDate { get => base.CH_EntryReleaseDate; set => base.CH_EntryReleaseDate = value; }

		[ResourceStringData("Enterprise.Customs.FR.Business.Declaration.CusEntryHeader|CH_ExitedStatus", Caption = "Export Control Status", ShortCaption = "ECS Status")]
		public override ZString CH_ExitedStatus { get => base.CH_ExitedStatus; set => base.CH_ExitedStatus = value; }

		[ResourceStringData("1B9518C5-90A3-4E51-AA5F-99F18CEA5F38", Caption = "Total Paid")]
		public override ZDecimal CH_TotalPaid { get => base.CH_TotalPaid; set => base.CH_TotalPaid = value; }

		public ZBool CusEntryLinesConfirmedValueHasBeenPopulated
		{
			get
			{
				if (cusEntryLinesConfirmedValueHasBeenPopulatedCached == null)
				{
					cusEntryLinesConfirmedValueHasBeenPopulatedCached = new CachedValue<ZBool>(() =>
					{
						var result = true;
						if (DataUtils.ObjectExists(Db.Connection, "ClientFRCusEntryLineWithResponseMessage"))
						{
							var dynamicBOs = new DynamicBusinessObjectCollection(Factory);
							dynamicBOs.Load(string.Format("SELECT TOP 1 FR2_CH_PK FROM ClientFRCusEntryLineWithResponseMessage WHERE FR2_CH_PK = '{0}'", PK));
							if (dynamicBOs.Count > 0)
							{
								result = false;
							}
						}
						return result;
					});
				}
				return cusEntryLinesConfirmedValueHasBeenPopulatedCached.Value;
			}
		}
		CachedValue<ZBool> cusEntryLinesConfirmedValueHasBeenPopulatedCached;

		public ZString PreviousECSStatusToAPE
		{
			get
			{
				if (previousECSStatusToAPE == null)
				{
					previousECSStatusToAPE = new CachedProperty<ZString>(Factory, delegate
					{
						var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, AutoEvents.CustomsEntryStatusCode);
						query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "ECS=");
						var orderedECSStatuses = Logs.Find(query)
							.OrderByDescending(log => log.SL_EventTime)
							.Select(log => log.SL_Reference.SubstringSafe("ECS=".Length))
							.ToArray();

						var firstIndexOfAPE = Array.IndexOf(orderedECSStatuses, ExportControlStatusList.Codes.APE);
						return orderedECSStatuses.Skip(firstIndexOfAPE + 1).FirstOrDefault(s => s != ExportControlStatusList.Codes.APE);
					});
				}
				return previousECSStatusToAPE.Value;
			}
		}
		CachedProperty<ZString> previousECSStatusToAPE;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			LogExitedStatusIfRequired();
			LogCOEEventIfNeeded();
			PrintSADDocumentIfRequired();
		}

		void PrintSADDocumentIfRequired()
		{
			var validCodes = new HashSet<string>
			{
				$"ECS={ExportControlStatusList.Codes.SOR}",
				$"ECS={ExportControlStatusList.Codes.ESO}",
				$"ECS={ExportControlStatusList.Codes.APE}",
				$"ECS={ExportControlStatusList.Codes.ESD}"
			};

			if (!PK.IsEmpty
				&& Logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsEntryStatus, new ZQuery(StmALogSchema.SL_Reference, validCodes)) is StmALog log
				&& !log.IsInDatabase
				&& FRCustomsDataRegistry.Instance.SADGenerationOnConfirmedExitEnabled.Value)
			{
				new SADEDocsSaver(this).RenderDocumentAndSaveInEDocs();
			}
		}

		void LogExitedStatusIfRequired()
		{
			if (!PK.IsEmpty)
			{
				if ((ZString)CH_ExitedStatusInfo.OriginalValue != CH_ExitedStatus)
				{
					var reference = $"ECS={CH_ExitedStatus}";
					var unsavedCESEvents = Logs.Find((StmALog log) => log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && !log.IsInDatabase && log.SL_Reference == reference);

					if (unsavedCESEvents == null || !unsavedCESEvents.Any())
					{
						Logs.AddNew(Events.CustomsEntryStatus, reference, ZDateTimeOffset.Now);
					}
				}
			}
		}

		public void SetEntryAsAmending()
		{
			var previousEntryStatus = CH_EntryStatus;
			var previousMessageStatus = CH_Status;

			CH_EntryStatus = DeltaIEImportCusEntryStatusList.Codes.Amending;
			CH_Status = ZString.Empty;

			var logTextForEntryStatus = FormattableString.Invariant($"Entry status set to AMG, original status: {previousEntryStatus}");
			var logTextForMessageStatus = FormattableString.Invariant($"Entry message status reset, original status: {previousMessageStatus}");
			Logs.AddNew(Events.CustomsStatusOverride, logTextForEntryStatus, ZDateTimeOffset.Now);
			Logs.AddNew(Events.CustomsStatusOverride, logTextForMessageStatus, ZDateTimeOffset.Now);
		}

		public ZDateTime CustomsLastEntryStatusDate
		{
			get
			{
				if (customsLastEntryStatusDateCache == null)
				{
					customsLastEntryStatusDateCache = new CachedProperty<ZDateTime>(Factory, () =>
					{
						var result = ZDateTime.Empty;
						if (Declaration.IsDeltaG)
						{
							result = Messages.LastIncomingMessageWithValuedEntryStatus?.EM_MessageDateTime ?? ZDateTime.Empty;
						}
						else if (Declaration.IsDeltaIE)
						{
							result = Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code).MaxOrDefault(x => x.SL_EventTime);
						}
						return result;
					});
				}
				return customsLastEntryStatusDateCache.Value;
			}
		}

		CachedProperty<ZDateTime> customsLastEntryStatusDateCache;

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			return newStatus != originalStatus && EntryActionHelper.IsStatusClear(newStatus);
		}

		protected override ZString PreviousStatus => (ZString)CH_EntryStatusInfo.OriginalValue;
		protected override ZString CurrentStatus => CH_EntryStatus;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentData = documentDataLoader.Load(this);
				if (documentData != null)
				{
					result.AddRange(documentData.Cast<BusinessObject>());
				}
				return result.ToArray();
			}
		}

		#region ITRCDetails Members

		public CusEntryHeaderTRCDetailsProvider TRCDetailsProvider => tRCDetailsProvider ?? (tRCDetailsProvider = new CusEntryHeaderTRCDetailsProvider(this));
		CusEntryHeaderTRCDetailsProvider tRCDetailsProvider;

		#endregion

		protected override Customs.Business.AmendmentSnapshotManager GetNewAmendmentSnapshotManagerCore() => new AmendmentSnapshotManager(this, Declaration.ApplicationExtender.AmendmentSnapshotMessageType);

		public bool CanBeRevertedToLastBAE => Factory.GetCached(ref canBeRevertedToLastBAECache, () => Declaration.ApplicationExtender.CanBeRevertedToLastBAE(this));
		CachedProperty<bool> canBeRevertedToLastBAECache;

		[ResourceStringData("FR.Business.Declaration.CusEntryHeader.CH_CalcTotalInvoicedAmountInLocalCurrency", Caption = "Total Invoiced Amount (EUR)", MediumCaption = "Tot.Inv.Amount (EUR)", ShortCaption = "Tot.Inv.Amt. (EUR)")]
		public ZDecimal CH_CalcTotalInvoicedAmountInLocalCurrency => Factory.GetCached(ref totalInvoicedAmountInLocalCurrencyCache, () => MergedLines.Sum(x => x.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency));
		CachedProperty<ZDecimal> totalInvoicedAmountInLocalCurrencyCache;

		public ZPropertyInfo CH_CalcTotalInvoicedAmountInLocalCurrencyInfo => GetZPropertyInfo(Schema.CH_CalcTotalInvoicedAmountInLocalCurrency);

		protected override List<ZString> GetCountriesOfRoutingCore()
		{
			var list = base.GetCountriesOfRoutingCore();

			if (IsExport && list.IsNullOrEmpty())
			{
				var officeOfExitCountry = Declaration.OfficeOfExit.Left(2);
				var provider = new EuropeanUnionCustomsMembersProvider();
				if (officeOfExitCountry != Core.Constants.CountryCodes.France && provider.IsMemberOfEU(officeOfExitCountry))
				{
					list.Add(officeOfExitCountry);
				}
			}

			return list;
		}

		public FREDIMessage IDDMessage => (FREDIMessage)Messages
			.Where(m => m.EM_Status == EDIMessageStatusList.Codes.ProcessedOK && m.EM_MessageSubType.EqualsAny(new ZString[] { DeltaIEResponseMessageSubTypeList.Codes.PrelodgedDeclarationAcceptance, DeltaIEResponseMessageSubTypeList.Codes.DeclarationAcceptance, DeltaIEResponseMessageSubTypeList.Codes.ReleaseNotification }))
			.OrderByDescending(m => m.EM_MessageDateTime)
			.FirstOrDefault();

		public ZString IDDMessageType => IDDMessage?.EM_MessageSubType ?? ZString.Empty;

		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetSupportingDocumentsToProcess()
		{
			var documents = base.GetSupportingDocumentsToProcess();
			if (EntryInstruction is CusEntryInstruction instruction)
			{
				documents = documents.Union(instruction.SupportingDocuments);
			}
			foreach (var document in documents)
			{
				yield return document;
			}
		}

		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetPreviousDocumentsToProcess()
		{
			var documents = base.GetPreviousDocumentsToProcess();
			if (EntryInstruction is CusEntryInstruction instruction)
			{
				documents = documents.Union(instruction.PreviousDocuments);
			}
			foreach (var document in documents)
			{
				yield return document;
			}
		}

		protected override IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> GetAdditionalInfosToProcess()
		{
			var additionalInfos = base.GetAdditionalInfosToProcess();
			if (EntryInstruction is CusEntryInstruction instruction)
			{
				additionalInfos = additionalInfos.Union(instruction.AdditionalInfos);
			}
			foreach (var additionalInfo in additionalInfos)
			{
				yield return additionalInfo;
			}
		}

		public override bool IsBondedWarehousingDisabled => base.IsBondedWarehousingDisabled || (EntryInstruction != null && EntryInstruction.CusAuthorizationUsages.Select(usage => usage.AuthorisationHeader).Cast<CusAuthorisationHeader>().Any(header => header != null && header.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing && header.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.CON && rule.CPR_ValueFrom == RuleCodeCONValueFromList.Codes.SUC)));

		public override bool IsEntryStatusCleared => Declaration.ApplicationExtender.IsEntryStatusCleared(this) || base.IsEntryStatusCleared;
	}
}
