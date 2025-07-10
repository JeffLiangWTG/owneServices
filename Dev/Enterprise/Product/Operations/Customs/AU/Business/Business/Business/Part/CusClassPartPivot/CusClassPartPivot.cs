using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassPartPivot : BaseCusClassPartPivot
		, IAggregatedAddInfo
		, IDutyDataWrapper
		, IAddInfoManager
		, IAddInfo
		, ITariffNumberProvider
		, IAQIS
		, Integration.Customs.AU.ICusClassPartPivot
		, ICusCodeDataTypeSupporter
		, ISelfHoldingLineAttachee
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fToday = ZDateTime.Today;
		}

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				Factory.ForcePublishForDataRefreshByTableName(this);
			}
		}

		#endregion

		public ZBool IsImport => this.CI_ChildType == ClassificationTypeList.Codes.HTI;
		public ZBool IsExport => this.CI_ChildType == ClassificationTypeList.Codes.HTE;

		public CusClassPartPivotAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CusClassPartPivotAddInfo(this, CI_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
				}
				return fAddInfo;
			}
		}
		protected CusClassPartPivotAddInfo fAddInfo;

		AUAddInfo IAddInfo.AddInfo => AddInfo;

		AUAddInfo IAQIS.AddInfo => AddInfo;

		public AUAddInfo EffectiveAddInfo
		{
			get
			{
				if (fEffectiveAddInfo == null)
				{
					fEffectiveAddInfo = this.AddInfo;
				}

				var ccAddInfoString = Classification?.CC_AddInfo ?? ZString.Empty;
				var addInfoString = CI_AddInfo;
				var keyValue = ccAddInfoString + CI_AddInfo;

				if (lastKey != keyValue)
				{
					lastKey = keyValue;

					using (fEffectiveAddInfo.SuspendSettingHasChanges())
					{
						if (!ccAddInfoString.IsEmpty)
						{
							fEffectiveAddInfo.LoadPropertiesFromString(ccAddInfoString);
						}

						fEffectiveAddInfo.LoadPropertiesFromString(addInfoString, false);
					}
				}

				return fEffectiveAddInfo;
			}
		}
		AUAddInfo fEffectiveAddInfo;
		string lastKey;

		#region Proxy Classification AddInfo
		public AUAddInfo ClassificationAddInfo => Classification?.AddInfo;

		public ZString ClassificationAddInfoString => ClassificationAddInfo?.AddInfoLine ?? ZString.Empty;

		public ZString ImportClassTreatmentCode
		{
			get { return ClassificationAddInfo?.ZA_TreatmentCode_Hidden ?? ZString.Empty; }
		}

		public ZPropertyInfo ImportClassTreatmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ImportClassTreatmentCode)); }
		}

		public ZString ImportClassInstrumentType
		{
			get { return ClassificationAddInfo?.ZA_InstrumentType_Hidden ?? ZString.Empty; }
		}

		public ZPropertyInfo ImportClassInstrumentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ImportClassInstrumentType)); }
		}

		public ZString ImportClassInstrumentCode
		{
			get { return ClassificationAddInfo?.ZA_InstrumentCode_Hidden ?? ZString.Empty; }
		}

		public ZPropertyInfo ImportClassInstrumentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ImportClassInstrumentCode)); }
		}
		#endregion

		public new Classification Classification
		{
			get { return base.Classification as Classification; }
		}

		public new AUOrgSupplierPart Part
		{
			get { return (AUOrgSupplierPart)base.Part; }
		}

		public CodeDescriptionPairList TreatmentCodeList
		{
			get { return AddInfo.Lookups.TreatmentCodeList; }
		}

		public CodeDescriptionPairList InstrumentTypeList
		{
			get { return AddInfo.Lookups.ZA_InstrumentType_List; }
		}

		#region Override

		public override ZGuid CI_OP
		{
			get
			{
				return base.CI_OP;
			}

			set
			{
				bool hasChanged = CI_OP != value;
				if (hasChanged && value == ZGuid.Empty && (this.Classification?.IsImport ?? false))
				{
					Questions.RemoveAndDeleteAll();
				}
				base.CI_OP = value;
			}
		}

		public override ZString CI_ChildType
		{
			get { return base.CI_ChildType; }
			set
			{
				var oldValue = CI_ChildType;
				base.CI_ChildType = value;
				if (oldValue != value)
				{
					Questions.RemoveAndDeleteAll();
					if (IsImport)
					{
						LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
					}
				}
				Part?.MarkAsNeedingValidation();
			}
		}

		protected override ZString DefaultChildType
		{
			get { return ClassificationTypeList.Codes.HTI; }
		}

		public override ZGuid CI_CC
		{
			get { return base.CI_CC; }
			set
			{
				base.CI_CC = value;
				fEffectiveAddInfo = null;
				if (IsImport)
				{
					LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
				}
				Part?.MarkAsNeedingValidation();
			}
		}

		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				base.CI_TariffNum = value;
				if (IsImport)
				{
					LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
				}
			}
		}

		protected override bool UseUniversalTariff => false;

		public override ZString CI_AddInfo
		{
			get { return base.CI_AddInfo; }
			set
			{
				value = value.Trim(SerialisableAUAddInfo.SeperationCharacter);
				if (CI_AddInfo != value)
				{
					base.CI_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(value);
					}
					if (IsImport)
					{
						LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
					}
				}
			}
		}

		protected override ZString GetDutyRateForCurrentCountry()
		{
			var result = ZString.Empty;
			if (!ImportTariffCode.IsEmpty && CI_ChildType != ClassificationTypeList.Codes.HTE)
			{
				var dutyData = new DutyDataWrapper(this);
				var rate = new CMRDutyCalculator(dutyData).Duty.Percent;
				result = rate.ToString(5);
			}
			return result;
		}

		#region Validation
		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		public new CusClassPartPivotValidation Validation
		{
			get { return (CusClassPartPivotValidation)GetNewValidation(); }
		}
		#endregion

		#region Lookups
		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		public new CusClassPartPivotLookups Lookups
		{
			get { return (CusClassPartPivotLookups)GetNewLookups(); }
		}
		#endregion

		#endregion

		#region IAQIS

		[ChildEditable(false)]
		public AQISDocumentCollection AQISDocuments
		{
			get
			{
				if (fAQISDocuments == null)
				{
					fAQISDocuments = new AQISDocumentCollection(Factory, AddInfo);
					fAQISDocuments.SplitAndAddAQISElements(AddInfo.ZA_AQISDocuments_Hidden);
					RegisterEditableChildObject(fAQISDocuments);
				}

				return fAQISDocuments;
			}
		}
		AQISDocumentCollection fAQISDocuments;

		[ChildEditable(false)]
		public AQISPremisesIdAndProcessingTypeCollection AQISPremisesIdAndProcessingTypes
		{
			get
			{
				if (fAQISPremisesIdAndProcessingTypes == null)
				{
					fAQISPremisesIdAndProcessingTypes = new AQISPremisesIdAndProcessingTypeCollection(Factory, AddInfo);
					fAQISPremisesIdAndProcessingTypes.SplitAndAddAQISElements(AddInfo.ZA_AQISPremIdProcessType_Hidden);
					RegisterEditableChildObject(fAQISPremisesIdAndProcessingTypes);
				}

				return fAQISPremisesIdAndProcessingTypes;
			}
		}
		AQISPremisesIdAndProcessingTypeCollection fAQISPremisesIdAndProcessingTypes;

		[ChildEditable(true)]
		public AQISCommodityCodeCollection AQISCommodityCodes
		{
			get
			{
				if (fAQISCommodityCodes == null)
				{
					fAQISCommodityCodes = new AQISCommodityCodeCollection(Factory);
					RegisterEditableChildObject(fAQISCommodityCodes);
				}

				return fAQISCommodityCodes;
			}
		}
		AQISCommodityCodeCollection fAQISCommodityCodes;

		[ChildEditable(true)]
		public AQISEntityIdCollection AQISEntityIds
		{
			get
			{
				if (fAQISEntityIds == null)
				{
					fAQISEntityIds = new AQISEntityIdCollection(Factory);
					RegisterEditableChildObject(fAQISEntityIds);
				}

				return fAQISEntityIds;
			}
		}
		AQISEntityIdCollection fAQISEntityIds;

		[ChildEditable(true)]
		public AQISPermitIdCollection AQISPermitIds
		{
			get
			{
				if (fAQISPermitIds == null)
				{
					fAQISPermitIds = new AQISPermitIdCollection(Factory);
					RegisterEditableChildObject(fAQISPermitIds);
				}

				return fAQISPermitIds;
			}
		}
		AQISPermitIdCollection fAQISPermitIds;

		[ChildEditable(true)]
		public AQISProducerCodeCollection AQISProducerCodes
		{
			get
			{
				if (fAQISProducerCodes == null)
				{
					fAQISProducerCodes = new AQISProducerCodeCollection(Factory);
					RegisterEditableChildObject(fAQISProducerCodes);
				}

				return fAQISProducerCodes;
			}
		}
		AQISProducerCodeCollection fAQISProducerCodes;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			AddInfo.SetAQISFieldsForSave();
		}

		#endregion

		[ChildEditable(true)]
		public ICSPermitCollection ICSPermits
		{
			get
			{
				if (icsPermits == null)
				{
					icsPermits = new ICSPermitCollection(this);
					icsPermits.Load();
					RegisterEditableChildObject(icsPermits);
				}
				return icsPermits;
			}
		}
		ICSPermitCollection icsPermits;

		#region IAggregatedAddInfo Members

		public ZDateTime EffectiveDutyDate
		{
			get { return ZDateTime.Today; }
		}

		public ZDateTime DateOfValuation
		{
			get { return fToday; }
		}
		protected ZDateTime fToday;

		ZString IAggregatedAddInfo.AggregatedZA_ORG
		{
			get { return AddInfo.ZA_ORG; }
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get { return AddInfo.ZA_ORG; }
		}

		public IZType AggregatedValue(string propertyName)
		{
			return (IZType)AddInfo[propertyName];
		}

		public CollectionCache CollectionCache
		{
			get
			{
				if (fCollectionCache == null)
				{
					fCollectionCache = new CollectionCache(Factory);
				}
				return fCollectionCache;
			}
		}
		protected CollectionCache fCollectionCache;

		bool IAggregatedAddInfo.IsCopying
		{
			get
			{
				return IsCopying;
			}
		}

		#endregion

		#region ITariffNumberProvider Members

		public ZString TariffAndStatNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Classification != null)
				{
					result = Classification.CC_TariffNum;
				}

				return result;
			}
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => ((IAUTariffFormatter)CurrentTariffFormatter).FormatDotted(unformattedTariff);

		protected override TariffFormatter GetTariffFormatter()
		{
			if (IsExport)
			{
				return new AUExportTariffUniversalFormatter();
			}
			else
			{
				return new AUImportTariffUniversalFormatter();
			}
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region ISelfHoldingLineAttachee

		#region supporting BOs

		internal CPQASelfHoldingLineAttacheeWrapper LineAttacheeHolderWrapper
		{
			get
			{
				if (fLineAttacheeHolderWrapper == null)
				{
					fLineAttacheeHolderWrapper = new CPQASelfHoldingLineAttacheeWrapper(this);
				}
				return fLineAttacheeHolderWrapper;
			}
		}
		CPQASelfHoldingLineAttacheeWrapper fLineAttacheeHolderWrapper;

		CPQAAttacheeWrapper CPQAAttacheeWrapper
		{
			get
			{
				if (fCPQAAttacheeWrapper == null)
				{
					fCPQAAttacheeWrapper = new CPQAAttacheeWrapper(this);
				}
				return fCPQAAttacheeWrapper;
			}
		}
		CPQAAttacheeWrapper fCPQAAttacheeWrapper;

		[ChildEditable(true)]
		public CMRCusEntryCPDecCollection Questions
		{
			get { return CPQAAttacheeWrapper.Questions; }
		}

		public
#if DEBUG
		virtual
#endif
void GenerateQuestion()
		{
			LineAttacheeHolderWrapper.GenerateQuestion();
		}

		public void RefreshQuestions()
		{
			LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
			GenerateQuestion();
		}

		#endregion

		ICPQAHeaderAttachee[] ICPQAAttacheeHolder.Headers => LineAttacheeHolderWrapper.Headers;

		ICPQALineAttachee[] ICPQAAttacheeHolder.Lines => LineAttacheeHolderWrapper.Lines;

		CachedAnsweredQuestions ICPQAAttacheeHolder.CachedQuestions => LineAttacheeHolderWrapper.CachedQuestions;

		CPQuestionKeys ICPQALineAttachee.CPQuestionKey
		{
			get
			{
				CPQuestionKeys result = new CPQuestionKeys();

				var tariffCode = this.Classification?.CC_TariffNum ?? CI_TariffNum;
				if (this.IsImport && !tariffCode.IsEmpty)
				{
					ZString tariffAndStat = tariffCode.Replace(".", "").Replace(" ", "");
					result.TariffNumber = tariffAndStat.SubstringSafe(0, 8);
					result.StatCode = tariffAndStat.SubstringSafe(8, 2);
					result.OriginCode = EffectiveAddInfo.ZA_ORG;
					result.HasValidOriginOrNatureOrModeOfTransport = false;
				}
				return result;
			}
		}

		ICPQALineAttachee[] ICPQALineAttachee.SourcesToDefault
		{
			get
			{
				ICPQALineAttachee[] result;
				if (this.IsImport && Classification != null)
				{
					result = new ICPQALineAttachee[] { Classification };
				}
				else
				{
					result = Array.Empty<ICPQALineAttachee>();
				}
				return result;
			}
		}

		LineDefaultQuestions ICPQALineAttachee.DefaultUniqueQuestions => LineAttacheeHolderWrapper.DefaultUniqueQuestions;

		ZString ICPQALineAttachee.TableCode => Enterprise.ZArchitecture.Schema.CusClassPartPivotSchema.Constants.Prefix;

		ZBool ICPQALineAttachee.IsRiskCalculatedFromTariff => true;

		ZBool ICPQALineAttachee.IsRiskHistorySupported => true;

		SchemaGuidColumn ICPQAAttachee.FKColumnInCusEntryCPDecTable => CPQAAttacheeWrapper.FKColumnInCusEntryCPDecTable;

		CMRCusEntryCPDecCollection ICPQAAttachee.Questions => CPQAAttacheeWrapper.Questions;

		ZDateTime ICPQAAttachee.SelectionDate => CPQAAttacheeWrapper.SelectionDate;

		#endregion

		#region TariffCustomsUniqureQuantity

		public ZString TariffCustomsUnitQuantity
		{
			get
			{
				var result = ZString.Empty;
				if (IsExport)
				{
					result = Classification?.ExportTariff?.ZZ1_ZZ8_UQ1 ?? result;
				}
				else if (IsImport)
				{
					var importTariff = Classification?.CC_TariffNum.Trim() ?? ZString.Empty;
					if (!importTariff.IsEmpty)
					{
						var dutyDate = ZDateTime.Today;
						result = Factory.GetCachedValue($"CusClassPartPivot|{importTariff}{dutyDate}", () =>
						{
							var unitQuantity = ZString.Empty;
							var splittedTariff = importTariff.Split(' ');
							if (splittedTariff.Length == 2)
							{
								var wrapper = ClassificationPeriodSnapshotWrapper.Load(Factory, splittedTariff[0], splittedTariff[1], dutyDate);
								unitQuantity = wrapper?.QuantityUnit ?? unitQuantity;
							}
							return unitQuantity;
						});
					}
				}
				return result;
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.ICSPermit, typeof(ICSPermit) }
			};
		}

		#endregion

		#region IDutyDataWrapper

		public ZDecimal ImportDutyPercentage
		{
			get
			{
				ICMRDutyData dutyData = new DutyDataWrapper(this);
				return new CMRDutyCalculator(dutyData).Duty.Percent;
			}
		}

		public ZString Preference { get { return EffectiveAddInfo?.ZA_PRF ?? ZString.Empty; } }
		public ZString ImportTariffCode { get { return Classification?.CC_TariffNum ?? CI_TariffNum; } }
		public ZString TreatmentCode { get { return EffectiveAddInfo?.ZA_TreatmentCode_Hidden ?? ZString.Empty; } }

		#endregion
	}
}
