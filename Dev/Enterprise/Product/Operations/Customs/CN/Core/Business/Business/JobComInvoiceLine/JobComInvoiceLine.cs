using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Integration.Customs;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobComInvoiceLine : AutoCNJobComInvoiceLine,
		Integration.Customs.CN.IJobComInvoiceLine,
		ICusCodeDataTypeSupporter,
		IChargeApportionee,
		ICusSupportingInfoTypeSupporter,
		ICusAddInfoTypeSupporter,
		IAdditionalBusinessObjectFetchStrategyProvider,
		IAdditionalInformationWrapperParent
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoCNJobComInvoiceLine.Schema
		{
			public const string JI_CEI_Description = "JI_CEI_Description";
			public const string JI_CEI_StyleDescription = "JI_CEI_StyleDescription";
			public const string CIQIngredient = "CIQIngredient";
			public const string JI_CIQCountryOfOrigin = "JI_CIQCountryOfOrigin";
			public const string DangerousGoodsDGSubs = "DangerousGoodsDGSubs";
			public const string CargoAttributesAsString = "CargoAttributesAsString";
			public const string UniversalDutyRateFormula = "UniversalDutyRateFormula";
			public const string TradeAgreementCode = "TradeAgreementCode";
			public const string CertificateOfOrigin = "CertificateOfOrigin";
			public const string CertificateOfOriginCountry = "CertificateOfOriginCountry";
			public const string CertificateOfOriginType = "CertificateOfOriginType";
			public const string ItemNoOnCertOfOrigin = "ItemNoOnCertOfOrigin";
			public const string TradeUnitPrice = "TradeUnitPrice";
			public const string XC_GoodsSpecModel = "XC_GoodsSpecModel";
			public const string XC_GoodsSpecModel2 = "XC_GoodsSpecModel2";
			public const string BatchNumbersAsString = "BatchNumbersAsString";
			public const string ManufactureDatesAsString = "ManufactureDatesAsString";
			public const string FormulaPricingRecordNumber = "FormulaPricingRecordNumber";
			public const string CIQSpecificationMaxLength = "100";
		}

		#endregion

		#region Implementation

		#region GetTariffDescription - to be overridden once the Tariff is setup for a new country

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return ZString.Empty;
		}

		#endregion

		#region Factory Save

		protected override void OnFactorySaving()
		{
			if (Declaration != null)
			{
				if (!Declaration.IsImport)
				{
					VINDataCollection.DeleteAll();
				}
				if (!Declaration.WillGenerateBothEntries)
				{
					JI_NameOfGoods2 = ZString.Empty;
					XC_GoodsSpecModel2 = ZString.Empty;
				}
			}

			if (!IsCertificateOfOriginApplicable)
			{
				CertificateOfOriginDocument?.Delete();
			}

			base.OnFactorySaving();
		}

		public override void Delete()
		{
			if (fAttachmentLinks != null)
			{
				AttachmentLinks.DeleteAll();
			}

			EntryInstruction?.CusStorageDocPivots.UnlinkInvoiceLine(this);

			this.DeleteHiddenNotes();
			base.Delete();
		}

		#endregion

		#region JI_CEI

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_CEI", Caption = "Entry Instruction")]
		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				var oldEntryInstruction = EntryInstruction;
				base.JI_CEI = value;
				var newEntryInstruction = EntryInstruction;

				if (!IsCopying && oldEntryInstruction != newEntryInstruction)
				{
					if (CanLinkToAttachment)
					{
						oldEntryInstruction?.CusStorageDocPivots.UnlinkInvoiceLine(this);
						ReloadAttachmentLinksIfLoaded();
					}

					InvoiceHeader?.MarkAsNeedingValidation();
					JI_CEI_DescriptionInfo.RefreshBinding();
				}
				RefreshBinding();
			}
		}

		#endregion

		#region JI_CEI_Description

		public ZString JI_CEI_Description => EntryInstruction?.DescriptionWithBillOfLading ?? ZString.Empty;

		public ZPropertyInfo JI_CEI_DescriptionInfo => GetZPropertyInfo(Schema.JI_CEI_Description);

		#endregion

		#region JI_CEI_StyleDescription

		public ZString JI_CEI_StyleDescription => EntryInstruction?.CEI_StyleDescription ?? ZString.Empty;

		public ZPropertyInfo JI_CEI_StyleDescriptionInfo => GetZPropertyInfo(Schema.JI_CEI_StyleDescription);

		#endregion

		#region JI_Tariff

		[MaxLength(10)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					var oldValue = JI_Tariff;
					base.JI_Tariff = value;
					if (!IsCopying && oldValue != JI_Tariff)
					{
						if (CIQRequires && !SetterSuspender.IsSetterSuspended(AutoCNJobComInvoiceLine.Schema.JI_CIQTariff))
						{
							var childTariffs = UniversalTariff?.GetEffectiveChildTariffs(EffectiveAssessmentDate, Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff);
							if (childTariffs != null)
							{
								if (childTariffs.Length == 1)
								{
									JI_CIQTariff = childTariffs.FirstOrDefault().ZZ1_TariffCode.Left(AutoCNJobComInvoiceLine.Schema.JI_CIQTariffMaxLength);
								}
								if (!childTariffs.Any(x => x.ZZ1_TariffCode == JI_CIQTariff))
								{
									JI_CIQTariff = ZString.Empty;
								}
							}
						}

						if (UniversalTariff != null && EntryInstruction != null && EntryInstruction.IsGeneralTrade && !SetterSuspender.IsSetterSuspended(CusSupportingDocument.Schema.CSI_Code))
						{
							var cusConditions = ConditionChecker.GetApplicableConditions(Factory, UniversalTariff, RequiredDocumentsConditionSelectionCriteria);

							foreach (var condition in cusConditions)
							{
								var conditionValuesCount = condition.ConditionValues.Count;
								if (conditionValuesCount == 1)
								{
									var documentType = condition.ConditionValues.First().ZX3_Value;
									DefaultCusSupportingDocumentsFromTariff((code) => documentType == code, documentType);
								}
								else if (conditionValuesCount > 1)
								{
									var conditionDocumentTypes = condition.ConditionValues.Select(x => x.ZX3_Value);
									DefaultCusSupportingDocumentsFromTariff((code) => conditionDocumentTypes.Contains(code), conditionDocumentTypes.First());
								}
							}
						}

						if (UniversalTariff != null && !AreClassificationDetailsBeingUpdated)
						{
							DefaultTradeUQFromCustomsUQ();
						}

						UniversalDutyRateFormulaInfo.RefreshBinding();
					}
				}
			}
		}

		void DefaultCusSupportingDocumentsFromTariff(Func<ZString, ZBool> codeMatcher, ZString defaultCode)
		{
			if (!this.CusSupportingDocuments.Cast<CusSupportingDocument>().Any(x => codeMatcher(x.CSI_Code)))
			{
				CusSupportingDocuments.SuspendValidation();
				try
				{
					var firstDocument = EntryInstruction?.CusSupportingDocuments.FirstOrDefault(x => codeMatcher(x.CSI_Code));
					if (firstDocument != null)
					{
						CusSupportingDocuments.AddNew(firstDocument.CSI_Code, firstDocument.CSI_ReferenceNumber);
					}
					else
					{
						CusSupportingDocuments.AddNew(defaultCode, ZString.Empty);
					}
				}
				finally
				{
					CusSupportingDocuments.ResumeValidation();
				}
			}
		}

		#endregion

		public override ZDateTime EffectiveAssessmentDate =>
			EntryInstruction != null && !EntryInstruction.CEI_DateForDuty.IsEmpty ? EntryInstruction.CEI_DateForDuty : base.EffectiveAssessmentDate;

		#region Override for Caption

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DutyModes))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_DutyMode", Caption = "Duty Mode")]
		public override ZString JI_DutyMode
		{
			get => base.JI_DutyMode;
			set => base.JI_DutyMode = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_ProductManualNo", Caption = "Manual Item No")]
		public override ZInt JI_ProductManualNo
		{
			get => base.JI_ProductManualNo;
			set => base.JI_ProductManualNo = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_ProductManualNo2", Caption = "Manual Item No")]
		public override ZInt JI_ProductManualNo2
		{
			get => base.JI_ProductManualNo2;
			set => base.JI_ProductManualNo2 = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_ProductVersion", Caption = "Product Version")]
		public override ZString JI_ProductVersion
		{
			get => base.JI_ProductVersion;
			set => base.JI_ProductVersion = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|FormulaPricingRecordNumber", Caption = "Formula Pricing Record No.", ShortCaption = "Formula Pricing No.")]
		[MaxLength(JobComInvLineRefs.Schema.JG_ReferenceNumberMaxLength)]
		public ZString FormulaPricingRecordNumber
		{
			get
			{
				return FormulaPricingRecordNumberLineRef?.JG_ReferenceNumber ?? ZString.Empty;
			}
			set
			{
				var oldValue = FormulaPricingRecordNumber;
				var lineRef = FormulaPricingRecordNumberLineRef;
				value = value.TrimEndSpaceTab();
				if (value.IsEmpty)
				{
					lineRef?.Delete();
				}
				else if (lineRef == null)
				{
					InvoiceLineRefs.AddNew(Constants.JobComInvLineRefType.FormulaPricingRecordNumber, value);
				}
				else
				{
					lineRef.JG_ReferenceNumber = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateFormulaPricingRecordNumber();
					if (!IsCopying && FormulaPricingRecordNumber != oldValue)
					{
						InvoiceHeader.Validation.ValidateFormulaPricingConfirm();
						InvoiceHeader.Validation.ValidateTemporaryPricingConfirm();
					}
				}
				FormulaPricingRecordNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FormulaPricingRecordNumberInfo => GetZPropertyInfo(Schema.FormulaPricingRecordNumber);

		JobComInvLineRefs FormulaPricingRecordNumberLineRef => InvoiceLineRefs.GetFirstJobComInvLineRefs(Constants.JobComInvLineRefType.FormulaPricingRecordNumber);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DestDistrictList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_DestinationDistrict", Caption = "Domestic Destination", ShortCaption = "Domestic Dest.")]
		public override ZString JI_DestinationDistrict
		{
			get => base.JI_DestinationDistrict;
			set => base.JI_DestinationDistrict = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OrigDistrictList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_OriginDistrict", Caption = "Domestic Origin")]
		public override ZString JI_OriginDistrict
		{
			get => base.JI_OriginDistrict;
			set => base.JI_OriginDistrict = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.DestRegionList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_DestinationRegion", Caption = "Domestic Destination", ShortCaption = "Domestic Dest.")]
		public override ZString JI_DestinationRegion
		{
			get => base.JI_DestinationRegion;
			set => base.JI_DestinationRegion = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OrigRegionList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_OriginRegion", Caption = "Domestic Origin")]
		public override ZString JI_OriginRegion
		{
			get => base.JI_OriginRegion;
			set => base.JI_OriginRegion = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_RN_NKCountryOfExport", Caption = "Final Destination")]
		public override ZString JI_RN_NKCountryOfExport
		{
			get => base.JI_RN_NKCountryOfExport;
			set => base.JI_RN_NKCountryOfExport = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_PrimaryPreference", Caption = "Preference")]
		[MaxLength(8)]
		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set
			{
				var oldValue = JI_PrimaryPreference;
				base.JI_PrimaryPreference = value;
				if (!IsCopying && JI_PrimaryPreference != oldValue)
				{
					UniversalDutyRateFormulaInfo.RefreshBinding();

					if (!IsDefaultingByPrimaryPreferenceSuspended)
					{
						DefaultJI_SecondaryPreference();
						DefaultCertificateOfOrigin();
					}

					CertificateOfOriginTypeInfo.RefreshBinding();
					CertificateOfOriginInfo.RefreshBinding();
					CertificateOfOriginCountryInfo.RefreshBinding();
					TradeAgreementCodeInfo.RefreshBinding();
				}
			}
		}

		void DefaultJI_SecondaryPreference()
		{
			if (TradeAgreementCode_ReadOnly)
			{
				TradeAgreementCode = ZString.Empty;
			}
			else
			{
				var applicableRates = Lookups.TradeAgreementCodeList;
				if (applicableRates.Count == 1)
				{
					TradeAgreementCode = applicableRates[0].Code;
				}
				else if (!applicableRates.ContainsCode(TradeAgreementCode))
				{
					TradeAgreementCode = ZString.Empty;
				}
			}
		}

		void DefaultCertificateOfOrigin()
		{
			if (!IsCertificateOfOriginApplicable)
			{
				CertificateOfOriginDocument?.Delete();
			}
			else if (IsCertificateOfOriginRequired && CertificateOfOriginDocument == null)
			{
				var instructionCertificateOfOrigin = EntryInstruction?.CusSupportingDocuments.FirstOrDefault(doc => doc.IsCertificateOfOrigin);
				if (instructionCertificateOfOrigin != null && !instructionCertificateOfOrigin.CSI_ReferenceNumber.IsEmpty)
				{
					var newDoc = CreateNewCertificateOfOrigin(instructionCertificateOfOrigin.CSI_ReferenceNumber);
					newDoc.CSI_RN_NKCountryCode = instructionCertificateOfOrigin.CSI_RN_NKCountryCode;
					newDoc.CSI_SubType = instructionCertificateOfOrigin.CSI_SubType;
					JI_SecondaryPreference = instructionCertificateOfOrigin.Parent.JI_SecondaryPreference;
				}
			}
		}

		#region Suspend DefaultingByPrimaryPreference

		protected bool IsDefaultingByPrimaryPreferenceSuspended => defaultingByPrimaryPreferenceSuspenderIndex > 0;
		int defaultingByPrimaryPreferenceSuspenderIndex;

		public IDisposable SuspendDefaultingByPrimaryPreference()
		{
			return new DefaultingByPrimaryPreferenceSuspender(this);
		}

		class DefaultingByPrimaryPreferenceSuspender : IDisposable
		{
			public DefaultingByPrimaryPreferenceSuspender(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.invoiceLine.defaultingByPrimaryPreferenceSuspenderIndex++;
			}

			readonly JobComInvoiceLine invoiceLine;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.defaultingByPrimaryPreferenceSuspenderIndex--;
			}

			#endregion
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_OA_ManufacturerAddress", Caption = "Manufacturer")]
		public override ZGuid JI_OA_ManufacturerAddress
		{
			get => base.JI_OA_ManufacturerAddress;
			set => base.JI_OA_ManufacturerAddress = value;
		}

		public ZString ManufacturerCIQNum => ManufacturerAddress?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.ChinaCodeTypes.CIQ, Core.Constants.CountryCodes.China) ?? ZString.Empty;

		public ZString ManufacturerChineseName => ManufacturerAddress?.GetChineseCompanyName() ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_StateOrRegionOfOrigin", Caption = "Origin State")]
		public override ZString JI_StateOrRegionOfOrigin
		{
			get => base.JI_StateOrRegionOfOrigin;
			set
			{
				var oldValue = JI_StateOrRegionOfOrigin;
				base.JI_StateOrRegionOfOrigin = value;
				if (!IsCopying && oldValue != value)
				{
					DefaultOriginState(value);
				}
			}
		}

		void DefaultOriginState(ZString originState)
		{
			if (!JI_CountryOfOrigin.IsEmpty)
			{
				var result = CNRefCusMapper.MapCW1StateCodeToCustomsCode(Factory, JI_CountryOfOrigin + originState);
				JI_CIQOriginState = result.IsEmpty ? CountryOfOrigin.RN_IsoNumericUNM49Code : result;
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_NDescription", Caption = "Specification")]
		[MaxLength(Schema.CIQSpecificationMaxLength)]
		public override ZString JI_NDescription
		{
			get => base.JI_NDescription;
			set => base.JI_NDescription = value;
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CIQTariffList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_CIQTariff", Caption = "CIQ Tariff")]
		public override ZString JI_CIQTariff
		{
			get => base.JI_CIQTariff;
			set
			{
				var oldValue = JI_CIQTariff;
				base.JI_CIQTariff = value;
				if (!IsCopying && oldValue != JI_CIQTariff)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_BrandName", Caption = "Brand")]
		public override ZString JI_BrandName
		{
			get => base.JI_BrandName;
			set => base.JI_BrandName = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CIQOriginStateList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_CIQOriginState", Caption = "Origin State")]
		public override ZString JI_CIQOriginState
		{
			get => base.JI_CIQOriginState;
			set => base.JI_CIQOriginState = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.EndUseList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_CIQEndUse", Caption = "End Use")]
		public override ZString JI_CIQEndUse
		{
			get => base.JI_CIQEndUse;
			set
			{
				var oldValue = JI_CIQEndUse;
				base.JI_CIQEndUse = value;
				if (!IsCopying && oldValue != JI_CIQEndUse)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_Model", Caption = "Model")]
		public override ZString JI_Model
		{
			get => base.JI_Model;
			set => base.JI_Model = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_CountryOfOrigin", Caption = "Goods Origin")]
		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set
			{
				var oldValue = JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;
				if (!IsCopying && oldValue != JI_CountryOfOrigin)
				{
					UniversalDutyRateFormulaInfo.RefreshBinding();
				}
			}
		}

		public ZString JI_CIQCountryOfOrigin => RefCountry.LoadFromCountryCode(Factory, JI_CountryOfOrigin)?.RN_IsoAlpha3Code ?? ZString.Empty;

		public ZPropertyInfo JI_CIQCountryOfOriginInfo => GetZPropertyInfo(Schema.JI_CIQCountryOfOrigin);

		#region CusSupportingDocuments

		[ChildEditable(true)]
		public CusSupportingDocumentCollection CusSupportingDocuments
		{
			get
			{
				if (cusSupportingDocuments == null)
				{
					cusSupportingDocuments = new CusSupportingDocumentCollection(this);
					cusSupportingDocuments.Load();
					RegisterEditableChildObject(cusSupportingDocuments);
				}
				return cusSupportingDocuments;
			}
		}
		CusSupportingDocumentCollection cusSupportingDocuments;

		public CusSupportingDocumentCollectionView FilteredCusSupportingDocuments
		{
			get
			{
				if (filteredCusSupportingDocuments == null)
				{
					filteredCusSupportingDocuments = new CusSupportingDocumentCollectionView(CusSupportingDocuments);
				}
				return filteredCusSupportingDocuments;
			}
		}
		CusSupportingDocumentCollectionView filteredCusSupportingDocuments;

		#endregion

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.China;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.JobComInvoiceLineFetchStrategy(this);
		}

		protected override ZString GetPartPivotTypeCore()
		{
			return Declaration.WillGenerateBothEntries ? (ZString)ClassificationTypeList.Codes.HTB : base.GetPartPivotTypeCore();
		}

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();

			var pivot = Pivot as CusClassPartPivot;
			if (pivot != null)
			{
				UpdateFromPivot(pivot, Declaration.WillGenerateExitingEntry, Declaration.WillGenerateEnteringEntry);
				if (JI_Tariff == pivot.CI_TariffNum)
				{
					UpdateAdditionalInfomationsFromPivot(pivot);
				}
			}
		}

		void UpdateAdditionalInfomationsFromPivot(CusClassPartPivot pivot)
		{
			var nameOfGoods = pivot.CNC_NameOfGoods;
			var goodsSpecModel = pivot.CNC_GoodsSpecModel;

			if (!nameOfGoods.IsEmpty)
			{
				JI_NameOfGoods = nameOfGoods.Left(JI_NameOfGoodsInfo.MaxLength);
			}
			if (!goodsSpecModel.IsEmpty)
			{
				XC_GoodsSpecModel = pivot.CNC_GoodsSpecModel.Left(XC_GoodsSpecModelInfo.MaxLength);
				AdditionalInformationHelper.CorrectAdditionalInfoValues();
			}

			if (Declaration?.WillGenerateBothEntries ?? false)
			{
				if (!nameOfGoods.IsEmpty)
				{
					JI_NameOfGoods2 = nameOfGoods.Left(JI_NameOfGoods2Info.MaxLength);
				}
				if (!goodsSpecModel.IsEmpty)
				{
					XC_GoodsSpecModel2 = pivot.CNC_GoodsSpecModel.Left(XC_GoodsSpecModel2Info.MaxLength);
					AdditionalInformation2Helper.CorrectAdditionalInfoValues();
				}
			}
		}

		void UpdateFromPivot(CusClassPartPivot pivot, bool willGenerateExitingEntry, bool willGenerateEnteringEntry)
		{
			if (!pivot.CI_RN_NKCountryOfOrigin.IsEmpty)
			{
				JI_CountryOfOrigin = pivot.CI_RN_NKCountryOfOrigin;
			}

			if (!pivot.CI_RN_NKCountryOfExport.IsEmpty)
			{
				JI_RN_NKCountryOfExport = pivot.CI_RN_NKCountryOfExport;
			}

			if (!pivot.CNC_OriginDistrict.IsEmpty && willGenerateExitingEntry)
			{
				JI_OriginDistrict = pivot.CNC_OriginDistrict;
			}

			if (!pivot.CNC_OriginRegion.IsEmpty && willGenerateExitingEntry)
			{
				JI_OriginRegion = pivot.CNC_OriginRegion;
			}

			if (!pivot.CI_RW_NKOriginState.IsEmpty && willGenerateEnteringEntry)
			{
				JI_StateOrRegionOfOrigin = pivot.CI_RW_NKOriginState;
			}

			if (!pivot.CNC_OriginState.IsEmpty && willGenerateEnteringEntry)
			{
				JI_CIQOriginState = pivot.CNC_OriginState;
			}

			if (!pivot.CNC_DestinationDistrict.IsEmpty && willGenerateEnteringEntry)
			{
				JI_DestinationDistrict = pivot.CNC_DestinationDistrict;
			}

			if (!pivot.CNC_DestinationRegion.IsEmpty && willGenerateEnteringEntry)
			{
				JI_DestinationRegion = pivot.CNC_DestinationRegion;
			}

			if (!pivot.CNC_CIQTariff.IsEmpty)
			{
				JI_CIQTariff = pivot.CNC_CIQTariff;
			}

			var pivotCargoAtts = pivot.CargoAttributesAsString;
			if (!pivotCargoAtts.IsEmpty && pivotCargoAtts != CargoAttributesAsString)
			{
				CargoAttributes.RemoveAndDeleteAll();
				pivot.CargoAttributes.Cast<CargoAttribute>().ForEach(att => CargoAttributes.AddNew(att.CY_Code));
			}

			if (!pivot.CNC_OA_ManufacturerAddress.IsEmpty)
			{
				JI_OA_ManufacturerAddress = pivot.CNC_OA_ManufacturerAddress;
			}

			if (!pivot.CI_NDescription.IsEmpty)
			{
				JI_NDescription = pivot.CI_NDescription.Left(JI_NDescriptionInfo.MaxLength);
			}

			if (!pivot.CNC_Model.IsEmpty)
			{
				JI_Model = pivot.CNC_Model.Left(JI_ModelInfo.MaxLength);
			}

			if (!pivot.CNC_Brand.IsEmpty)
			{
				JI_BrandName = pivot.CNC_Brand.Left(JI_BrandNameInfo.MaxLength);
			}

			if (!pivot.CNC_EndUse.IsEmpty)
			{
				JI_CIQEndUse = pivot.CNC_EndUse;
			}

			if (!pivot.CIQIngredient.IsEmpty)
			{
				CIQIngredient = pivot.CIQIngredient;
			}

			if (!pivot.CNC_UNPackageMarking.IsEmpty)
			{
				JI_PackageTypeOfUNDG = pivot.CNC_UNPackageMarking.Left(JI_PackageTypeOfUNDGInfo.MaxLength);
			}

			if (!pivot.CNC_NonDangerousChemicalFlag.IsEmpty)
			{
				JI_NonDangerousChemicalFlag = pivot.CNC_NonDangerousChemicalFlag;
			}

			if (!pivot.CNC_QualityGuaranteePeriod.IsEmpty)
			{
				JI_CIQQualityGuaranteePeriod = pivot.CNC_QualityGuaranteePeriod;
			}

			if (!pivot.CNC_TradeUnitQty.IsEmpty)
			{
				JI_TradeUnitQty = pivot.CNC_TradeUnitQty;

				var pivotUnitPrice = pivot.CNC_TradeUnitPrice;
				if (pivotUnitPrice > 0 && JI_LinePrice.IsEmpty && JI_RX_NKLinePriceCurr == pivot.CNC_RX_NKTradeUnitPriceCurrency)
				{
					TradeUnitPrice = pivotUnitPrice;
				}
			}
			else
			{
				DefaultTradeUQFromInvoiceUQ();
				DefaultTradeUQFromCustomsUQ();
			}

			var defaultUNDG = Part.UNDGs.FirstOrDefault();
			if (defaultUNDG != null && !defaultUNDG.DI_DG.IsEmpty)
			{
				DangerousGoodsDGSubs = defaultUNDG.DI_DG;
			}
		}

		protected override bool ShouldRecalculatePercentageChargeAmountBasedOnLinePrice => false;

		public TariffView CIQTariff => CNRefTariffDataLoader.GetCIQTariff(Factory, JI_CIQTariff, EffectiveAssessmentDate);

		#endregion

		#region IChargeApportionee members

		void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
		{
			if (LinePriceRefCurrency != null)
			{
				if (charge.J7_ChargeType == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance)
				{
					var invoice = this.InvoiceHeader;
					var incoTermAndChargeFactory = invoice?.IncoTermAndChargeFactory;

					ZDecimal cFRAmount = 0m;
					if (incoTermAndChargeFactory != null && !invoice.IncoTerm.IsEmpty && incoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoice.IncoTerm, charge.ChargeCode))
					{
						cFRAmount = (JI_Calc_CIF / (100 + charge.J7_Percentage)) * 100;
					}
					else
					{
						cFRAmount = JI_Calc_FOB + JI_Calc_FreightInInvoiceCurr;
					}

					charge.J7_Amount = charge.J7_Percentage > 0m ? cFRAmount * charge.J7_Percentage / 100m : 0m;

					if (charge.J7_Amount > 0m)
					{
						charge.J7_RX_NKCurrency = LinePriceRefCurrency.RX_Code;
					}
				}
				else
				{
					if (charge.J7_Percentage > 0)
					{
						charge.J7_Amount = JI_LinePrice * charge.J7_Percentage / 100;
						charge.J7_RX_NKCurrency = LinePriceRefCurrency.RX_Code;
					}
				}
			}
		}

		public ZString UniversalTariffRateType
		{
			get
			{
				var isImport = Declaration?.IsImport ?? true;
				return isImport ? Universal.Constants.RateTypes.Duty : Universal.Constants.RateTypes.ExportDuty;
			}
		}

		protected override ZString UniversalTariffDutyRateCode => Constants.UniversalReferenceConstants.RefCusRateCodes.CustomsDuty;

		public ZString UniversalDutyRateFormula
		{
			get
			{
				var isImport = Declaration?.IsImport ?? true;
				return isImport ? UniversalDutyRate?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty : ZString.Empty;
			}
		}

		public ZPropertyInfo UniversalDutyRateFormulaInfo => GetZPropertyInfo(Schema.UniversalDutyRateFormula);

		public override RateView UniversalDutyRate => UniversalTariff?.GetApplicableRates(DutyRateSelectionCriteria).OrderBy(x => x.RateFormulaNumber).FirstOrDefault();

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Constants.CusCodeDataTypes.Codes.CIQ, typeof(ProductionBatch));
			result.Add(Constants.CusCodeDataTypes.Codes.CargoAttribute, typeof(CargoAttribute));
			return result;
		}

		#endregion

		#region Cargo Attributes

		[ChildEditable(true)]
		public CargoAttributeCollection CargoAttributes
		{
			get
			{
				if (cargoAttributes == null)
				{
					cargoAttributes = new CargoAttributeCollection(this);
					cargoAttributes.Load();
					RegisterEditableChildObject(cargoAttributes);
				}
				return cargoAttributes;
			}
		}
		CargoAttributeCollection cargoAttributes;

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|CargoAttributesAsString", Caption = "Cargo Attributes")]
		public ZString CargoAttributesAsString => CargoAttributes.GetSelectedOptionDescAsString();

		public ZPropertyInfo CargoAttributesAsStringInfo => GetZPropertyInfo(Schema.CargoAttributesAsString);

		public ZBool HasDangerousGoodsAttribute => CargoAttributes.IsAnyDangerousGoodsAttributeSelected();

		public ZBool NonDangerousChemicalFlagVisible => HasDangerousGoodsAttribute || !DangerousGoodsDGSubs.IsEmpty;

		public ZPropertyInfo NonDangerousChemicalFlagVisibleInfo => GetZPropertyInfo(nameof(NonDangerousChemicalFlagVisible));

		#endregion

		#region AttachmentLinks

		public bool CanLinkToAttachment => CargoAttributes.Where(x => CargoAttributeList.CanLinkToAttachment(x.CY_Code)).Any();

		[ChildEditable(true)]
		public AttachmentInvoiceLineLinkCollection AttachmentLinks
		{
			get
			{
				if (fAttachmentLinks == null)
				{
					fAttachmentLinks = new AttachmentInvoiceLineLinkCollection(this);
					RegisterEditableChildObject(fAttachmentLinks);
					fAttachmentLinks.Load();
				}
				return fAttachmentLinks;
			}
		}
		AttachmentInvoiceLineLinkCollection fAttachmentLinks;

		public void ReloadAttachmentLinksIfLoaded()
		{
			if (fAttachmentLinks != null)
			{
				AttachmentLinks.Load();
			}
		}

		public void RemoveAttachmentLinkIfLoaded(CusStorageDocPivot storageDoc)
		{
			if (fAttachmentLinks != null)
			{
				AttachmentLinks.Where(x => x.CusStorageDocPivot == storageDoc).ToList().ForEach(x => AttachmentLinks.RemoveAndDelete(x));
			}
		}

		public ZString[] LinkedAttachmentTypes => Factory.GetValue(ref cachedLinkedAttachmentTypes, GetLinkedAttachmentTypes);
		CachedProperty<ZString[]> cachedLinkedAttachmentTypes;

		ZString[] GetLinkedAttachmentTypes()
		{
			return CanLinkToAttachment && EntryInstruction != null
				? EntryInstruction.CusStorageDocPivots.FindByInvoiceLine(this).Select(x => x.CSD_DocType).ToArray()
				: Array.Empty<ZString>();
		}

		#endregion

		#region ProductionBatch

		[ChildEditable(true)]
		public ProductionBatchCollection ProductionBatch
		{
			get
			{
				if (productionBatch == null)
				{
					productionBatch = new ProductionBatchCollection(this);
					productionBatch.Load();
					RegisterEditableChildObject(productionBatch);
				}
				return productionBatch;
			}
		}

		ProductionBatchCollection productionBatch;

		[ChildEditable(true)]
		public CIQProductQualificationCollection CIQProductQualifications
		{
			get
			{
				if (cIQProductQualifications == null)
				{
					cIQProductQualifications = new CIQProductQualificationCollection(this);
					cIQProductQualifications.Load();
					RegisterEditableChildObject(cIQProductQualifications);
				}
				return cIQProductQualifications;
			}
		}

		CIQProductQualificationCollection cIQProductQualifications;

		public bool SupportAdditionalInfoUsage(ZString usage)
		{
			var result = false;
			switch (usage)
			{
				case AdditionalInfoUsageTypeList.Codes.CustomsEntry:
					result = Declaration?.WillGenerateCustomsEntry ?? true;
					break;
				case AdditionalInfoUsageTypeList.Codes.RecordListing:
					result = Declaration?.WillGenerateRecordListing ?? true;
					break;
			}
			return result;
		}

		#endregion

		#region ICusSupportingInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ Constants.CusSupportingInfoTypes.CusSupportingDocument, typeof(CusSupportingDocument) },
				{ Constants.CusSupportingInfoTypes.CIQProductQualification, typeof(CIQProductQualification) }
			};
			return result;
		}

		#endregion

		#region VIN Data Collection

		VINDataCollection fVINDataCollection;

		[ChildEditable(true)]
		public VINDataCollection VINDataCollection
		{
			get
			{
				if (fVINDataCollection == null)
				{
					fVINDataCollection = new VINDataCollection(this);
					fVINDataCollection.Load();
					RegisterEditableChildObject(fVINDataCollection);
				}
				return fVINDataCollection;
			}
		}

		#endregion

		#region Implementation of ICusAddInfoTypeSupporter

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusAddInfoTypeAttribute.Codes.CNVINData, typeof(VINData) }
			};
		}

		#endregion

		#region New Properties

		public ZBool CIQRequires => EntryInstruction?.CEI_CIQRequires ?? false;

		#region Notes

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|CIQIngredient", Caption = "Customs Quarantine Ingredient", ShortCaption = "Ingredient")]
		public ZString CIQIngredient
		{
			get => CustomsQuarantineIngredientNote.Text;
			set => CustomsQuarantineIngredientNote.SetNoteText(this, CIQIngredientInfo, value, () => Validation.ValidateCIQIngredient());
		}

		HiddenTextNote CustomsQuarantineIngredientNote => customsQuarantineIngredientNote ?? (customsQuarantineIngredientNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.CustomsQuarantineIngredient.Description));
		HiddenTextNote customsQuarantineIngredientNote;

		public ZPropertyInfo CIQIngredientInfo => GetZPropertyInfo(Schema.CIQIngredient);

		#endregion

		internal CusEntryInstruction CustomsEntryOrOnlyInstruction
		{
			get
			{
				var instruction = EntryInstruction;
				if ((Declaration?.WillGenerateBothEntries ?? false) && (instruction?.WillGenerateRecordListing ?? false))
				{
					instruction = instruction.ChildInstruction;
				}
				return instruction;
			}
		}

		internal CusEntryInstruction ChildInstruction => EntryInstruction?.ChildInstruction;

		public EnteringOrExiting IsEnteringOrExiting(bool forChildInstruction = false)
		{
			var result = EnteringOrExiting.Both;

			if ((Declaration?.WillGenerateBothEntries ?? false))
			{
				result = forChildInstruction ? EnteringOrExiting.Exiting : EnteringOrExiting.Entering;
			}
			else
			{
				result = IsImport ? EnteringOrExiting.Entering : EnteringOrExiting.Exiting;
			}
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TradeAgreementCodeList))]
		[BusinessObjectTestExclude()]
		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|TradeAgreementCode", ShortCaption = "PTA Code", MediumCaption = "Pref. Code", Caption = "Preferential Code", FullDescription = "Preferential Trade Agreement Code")]
		public ZString TradeAgreementCode
		{
			get
			{
				if (IsImport)
				{
					switch (JI_PrimaryPreference)
					{
						case Constants.PrimaryPreferenceCodes.FreeTradeAgreement:
							return JI_SecondaryPreference;
						case Constants.PrimaryPreferenceCodes.LeastDevelopedCountries:
							return Constants.TradeAgreementCodes.Codes.LDC;
						default:
							return ZString.Empty;
					}
				}
				else
				{
					return JI_SecondaryPreference;
				}
			}
			set
			{
				JI_SecondaryPreference = value;
				TradeAgreementCodeInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateTradeAgreementCode();
					Validation.ValidateJI_Tariff();
					Validation.ValidateJI_PrimaryPreference();
				}
			}
		}

		public ZPropertyInfo TradeAgreementCodeInfo => GetZPropertyInfo(Schema.TradeAgreementCode);

		public bool TradeAgreementCode_ReadOnly => IsImport && JI_PrimaryPreference != Constants.PrimaryPreferenceCodes.FreeTradeAgreement;

		#region Certificate Of Origin

		internal bool CertificateOfOriginTypeIsX => CertificateOfOriginDocument?.IsCertificateOfOriginX ?? false;

		[MaxLength(32)]
		[ReadOnlyMember(nameof(CertificateOfOrigin_Readonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|CertificateOfOrigin", ShortCaption = "COO", MediumCaption = "COO No.", Caption = "Certificate Of Origin")]
		public ZString CertificateOfOrigin
		{
			get => CertificateOfOriginDocument?.CSI_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (!IsCopying && CertificateOfOrigin != value)
				{
					var certificate = CertificateOfOriginDocument;
					if (certificate == null && !value.IsEmpty)
					{
						certificate = CreateNewCertificateOfOrigin();
					}
					if (certificate != null)
					{
						certificate.CSI_ReferenceNumber = value;
						if (!value.IsEmpty)
						{
							DefaultJI_SecondaryPreference();
						}
					}
					CertificateOfOriginInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateCertificateOfOrigin();
					}
				}
			}
		}
		public ZPropertyInfo CertificateOfOriginInfo => GetZPropertyInfo(Schema.CertificateOfOrigin);
		bool CertificateOfOrigin_Readonly => !IsCertificateOfOriginApplicable || CertificateOfOriginTypeIsX;

		[MaxLength(2)]
		[ReadOnlyMember(nameof(CertificateOfOriginCountry_Readonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|CertificateOfOriginCountry", Caption = "Origin Under FTA", FullDescription = "Certificate of Origin Country under Free Trade Agreement")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateOfOriginCountryList))]
		[BusinessObjectTestExclude]
		public ZString CertificateOfOriginCountry
		{
			get => (IsCertificateOfOriginRequired || !CertificateOfOrigin.IsEmpty) ? CertificateOfOriginDocument?.CSI_RN_NKCountryCode.FallbackIfEmpty(JI_CountryOfOrigin) ?? JI_CountryOfOrigin : ZString.Empty;
			set
			{
				if (!IsCopying && CertificateOfOriginCountry != value)
				{
					var certificate = CertificateOfOriginDocument;
					if (certificate == null && !value.IsEmpty)
					{
						certificate = CreateNewCertificateOfOrigin();
					}
					if (certificate != null)
					{
						certificate.CSI_RN_NKCountryCode = value;
					}
					CertificateOfOriginCountryInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateCertificateOfOriginCountry();
					}

					if (IsImport)
					{
						DefaultJI_SecondaryPreference();
					}
				}
			}
		}
		public ZPropertyInfo CertificateOfOriginCountryInfo => GetZPropertyInfo(Schema.CertificateOfOriginCountry);
		bool CertificateOfOriginCountry_Readonly => !IsCertificateOfOriginApplicable;

		[MaxLength(1)]
		[ReadOnlyMember(nameof(CertificateOfOriginType_Readonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|CertificateOfOriginType", ShortCaption = "COO Type", Caption = "Certificate of Origin Type")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CertificateOfOriginTypeList))]
		public ZString CertificateOfOriginType
		{
			get => CertificateOfOriginDocument?.CSI_SubType ?? ZString.Empty;
			set
			{
				if (!IsCopying && CertificateOfOriginType != value)
				{
					CheckMaximumLength(CertificateOfOriginTypeInfo, value);

					var certificate = CertificateOfOriginDocument;
					if (certificate == null && !value.IsEmpty)
					{
						certificate = CreateNewCertificateOfOrigin();
					}
					if (certificate != null)
					{
						certificate.CSI_SubType = value;

						if (CertificateOfOriginTypeList.IsSmallAmountGoods(value))
						{
							certificate.CSI_ReferenceNumber = ZString.Empty;
							certificate.CSI_LineNo = ZShort.Zero;
						}
					}
					CertificateOfOriginTypeInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateCertificateOfOriginType();
						Validation.ValidateItemNoOnCertOfOrigin();
						Validation.ValidateTradeAgreementCode();
					}
				}
			}
		}
		public ZPropertyInfo CertificateOfOriginTypeInfo => GetZPropertyInfo(Schema.CertificateOfOriginType);
		bool CertificateOfOriginType_Readonly => !IsCertificateOfOriginApplicable;

		[ReadOnlyMember(nameof(ItemNoOnCertOfOrigin_Readonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|ItemNoOnCertOfOrigin", ShortCaption = "Item No", Caption = "Item No on Certificate Of Origin")]
		public ZInt ItemNoOnCertOfOrigin
		{
			get => CertificateOfOriginTypeIsX ? (CusEntryLine?.EntryLineNo ?? ZShort.Zero) : CertificateOfOriginDocument?.CSI_LineNo ?? ZInt.Zero;
			set
			{
				if (!IsCopying)
				{
					var certificate = CertificateOfOriginDocument;
					if (certificate == null && !value.IsEmpty)
					{
						certificate = CreateNewCertificateOfOrigin();
					}
					if (certificate != null)
					{
						certificate.CSI_LineNo = value;
					}
					ItemNoOnCertOfOriginInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateItemNoOnCertOfOrigin();
				}
			}
		}
		public ZPropertyInfo ItemNoOnCertOfOriginInfo => GetZPropertyInfo(Schema.ItemNoOnCertOfOrigin);
		bool ItemNoOnCertOfOrigin_Readonly => !IsCertificateOfOriginApplicable || CertificateOfOriginTypeIsX;

		internal CusSupportingDocument CertificateOfOriginDocument => CusSupportingDocuments.Cast<CusSupportingDocument>().FirstOrDefault(document => !document.IsDeleted && document.IsCertificateOfOrigin);

		internal CusSupportingDocument CreateNewCertificateOfOrigin(string number = null)
		{
			return CusSupportingDocuments.AddNew(Constants.DocumentCodes.CertificateOfOrigin, number ?? ZString.Empty);
		}

		public ZBool IsCertificateOfOriginRequired => IsImport && !JI_PrimaryPreference.IsEmpty && JI_PrimaryPreference != Constants.PrimaryPreferenceCodes.Normal && JI_PrimaryPreference != Constants.PrimaryPreferenceCodes.MostFavouredNations;

		public ZBool IsCertificateOfOriginApplicable => IsCertificateOfOriginRequired || IsExport;

		#endregion

		#endregion

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UNDGPackageTypes))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_PackageTypeOfUNDG", Caption = "UN Markings for Packaging", ShortCaption = "UN Pack. Marking", FullDescription = "UN Marking for the Packaging of Dangerous Goods")]
		public override ZString JI_PackageTypeOfUNDG
		{
			get => base.JI_PackageTypeOfUNDG;
			set => base.JI_PackageTypeOfUNDG = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_NonDangerousChemicalFlag", Caption = "Non Dangerous Goods")]
		public override ZBool JI_NonDangerousChemicalFlag
		{
			get => base.JI_NonDangerousChemicalFlag;
			set => base.JI_NonDangerousChemicalFlag = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_CIQExpiryDate", Caption = "Expiry Date")]
		public override ZDateTime JI_CIQExpiryDate
		{
			get => base.JI_CIQExpiryDate;
			set => base.JI_CIQExpiryDate = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_CIQQualityGuaranteePeriod", Caption = "QGP(days)")]
		public override ZInt JI_CIQQualityGuaranteePeriod
		{
			get => base.JI_CIQQualityGuaranteePeriod;
			set => base.JI_CIQQualityGuaranteePeriod = value;
		}

		public override ZDecimal JI_CustomsValue => IsExport ? JI_Calc_FOB_InLocalCurrency : JI_Calc_CIF_InLocalCurrency;

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_NameOfGoods", Caption = "Name of Goods")]
		public override ZString JI_NameOfGoods
		{
			get => base.JI_NameOfGoods;
			set
			{
				var oldValue = JI_NameOfGoods;
				base.JI_NameOfGoods = value;

				if (!IsCopying && oldValue != JI_NameOfGoods)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		public bool NameOfGoodsSeemsTobeUsedProduct => JI_NameOfGoods.Contains((NoResString)"旧", StringComparison.OrdinalIgnoreCase);

		internal int GoodsSpecModelMaxLength => AdditionalInformationHelper.GoodsSpecModelMaxLength;

		[MaxLength(nameof(GoodsSpecModelMaxLength))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_GoodsSpecModel", Caption = "Specification & Model", ShortCaption = "Spec & Model")]
		public ZString XC_GoodsSpecModel
		{
			get => GoodsSpecModelNote.Text;
			set => GoodsSpecModelNote.SetNoteText(this, XC_GoodsSpecModelInfo, value, () =>
			{
				Validation.ValidateXC_GoodsSpecModel();
				_ = AdditionalInformationHelper.AdditionalElementValues;
				Validation.ValidateManufactureDatesAsString();
			});
		}

		HiddenTextNote GoodsSpecModelNote => goodsSpecModelNote ?? (goodsSpecModelNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.GoodsSpecificationAndModel.Description));
		HiddenTextNote goodsSpecModelNote;

		public ZPropertyInfo XC_GoodsSpecModelInfo => GetZPropertyInfo(Schema.XC_GoodsSpecModel);

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_NameOfGoods2", Caption = "Name of Goods")]
		public override ZString JI_NameOfGoods2
		{
			get => base.JI_NameOfGoods2;
			set => base.JI_NameOfGoods2 = value;
		}

		[MaxLength(nameof(GoodsSpecModelMaxLength))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_GoodsSpecModel2", Caption = "Specification & Model", ShortCaption = "Spec & Model")]
		public ZString XC_GoodsSpecModel2
		{
			get => GoodsSpecModelNote2.Text;
			set => GoodsSpecModelNote2.SetNoteText(this, XC_GoodsSpecModel2Info, value, () => Validation.ValidateXC_GoodsSpecModel2());
		}

		HiddenTextNote GoodsSpecModelNote2 => goodsSpecModelNote2 ?? (goodsSpecModelNote2 = new HiddenTextNote(this, PredefinedNoteTypes.Instance.GoodsSpecificationAndModel2.Description));
		HiddenTextNote goodsSpecModelNote2;

		public ZPropertyInfo XC_GoodsSpecModel2Info => GetZPropertyInfo(Schema.XC_GoodsSpecModel2);

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConfirmationTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_OrigContainerFlag", Caption = "Original Container Loading")]
		public override ZString JI_OrigContainerFlag
		{
			get => base.JI_OrigContainerFlag;
			set => base.JI_OrigContainerFlag = value;
		}

		protected override ZString AdditionalCode => JI_SecondaryPreference;

		public override bool ShouldWipeNKTaxType => false;

		protected override ZString EffectiveCountryOfOriginCore => IsCertificateOfOriginRequired ? CertificateOfOriginCountry : JI_CountryOfOrigin;

		#endregion

		#region UNDG

		public UNDGDataItem DangerousGoods => UNDGs.FirstItemForBinding[0];

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UNDGSubs))]
		public ZGuid DangerousGoodsDGSubs
		{
			get
			{
				return (dangerousGoodsDGSubsCached
						?? (dangerousGoodsDGSubsCached =
							new CachedProperty<ZGuid>(Factory, () => GetUNDGValue(undg => undg.DI_DG)))).Value;
			}
			set
			{
				SetUNDGValue(x => x.DI_DG = value);
				DangerousGoodsDGSubsInfo.RefreshBinding();
			}
		}
		CachedProperty<ZGuid> dangerousGoodsDGSubsCached;

		T GetUNDGValue<T>(Func<UNDGDataItem, T> valueGetter)
		{
			return UNDGs.Count > 1 ? default(T) : UNDGs.Select(valueGetter).FirstOrDefault();
		}

		void SetUNDGValue(Action<UNDGDataItem> valueSetter)
		{
			if (UNDGs.Count <= 1)
			{
				var undg = (UNDGs.FirstOrDefault() ?? UNDGs.AddNew());
				var oldDI_DGValue = undg.DI_DG;
				valueSetter(undg);
				if (undg.DI_DG.IsEmpty && undg.DI_OC_DGContact.IsEmpty)
				{
					undg.Delete();
				}
				else
				{
					var dgClass = undg.UNDGSubstance?.DG_Class ?? ZString.Empty;
					if (undg.DI_IMOClass != dgClass)
					{
						undg.DI_IMOClass = dgClass;
					}

					if (!IsCopying && oldDI_DGValue.IsEmpty)
					{
						JI_NonDangerousChemicalFlag = !undg.DI_DG.IsEmpty && !GoodsIsDangerousChemical;
					}
				}
			}
		}

		public ZPropertyInfo DangerousGoodsDGSubsInfo =>
			UNDGs.Count == 1 ? GetWrappedZPropertyInfo(Schema.DangerousGoodsDGSubs, x => UNDGs.First().DI_DGInfo) : GetZPropertyInfo(Schema.DangerousGoodsDGSubs);

		public bool GoodsIsDangerousChemical => DangerousGoodsHelper.IsDangerousChemical(Factory, AdditionalInformationHelper, EffectiveAssessmentDate);

		#endregion

		#region Quantities & Quantities Calculating

		public override ZString JI_InvoiceUQ
		{
			get => base.JI_InvoiceUQ;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					var oldValue = JI_InvoiceUQ;
					base.JI_InvoiceUQ = value;

					if (!IsCopying && oldValue != JI_InvoiceUQ && !AreClassificationDetailsBeingUpdated)
					{
						DefaultTradeUQFromInvoiceUQ();
					}
				}
			}
		}

		void DefaultTradeUQFromInvoiceUQ()
		{
			if (JI_TradeUnitQty.IsEmpty && !JI_InvoiceUQ.IsEmpty)
			{
				using (CalculateCustomsQtyFromTradeQtySuspender.SuspendCalculateCustomsQtyFromTradeQty(this))
				{
					JI_TradeUnitQty = this.GetBestMatchingCustomsUnit(JI_InvoiceUQ);
				}
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(invoiceLine => invoiceLine.UniversalTariff != null ? new TariffWrapper(invoiceLine) : null);

		void DefaultTradeUQFromCustomsUQ()
		{
			if (JI_TradeUnitQty.IsEmpty && !JI_CustomsUnitQty.IsEmpty)
			{
				using (CalculateCustomsQtyFromTradeQtySuspender.SuspendCalculateCustomsQtyFromTradeQty(this))
				{
					JI_TradeUnitQty = UniversalTariff.GetSpecificUOM(Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType).Left(3);
				}
			}
		}

		protected override void CalculateCustomsFactorAndQtyCore()
		{
			base.CalculateCustomsFactorAndQtyCore();

			using (CalculateCustomsQtyFromTradeQtySuspender.SuspendCalculateCustomsQtyFromTradeQty(this))
			{
				TradeQuantityConverter.CalculateCustomsFactorAndQty();
			}
		}

		#region Customs Qty & UQ

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString JI_CustomsUnitQty
		{
			get => base.JI_CustomsUnitQty;
			set => base.JI_CustomsUnitQty = value;
		}

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return (UniversalTariff != null);
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return JI_CustomsUnitQty.IsEmpty;
		}

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new CustomsQuantityConverter(this, JI_CustomsQuantityInfo, JI_CustomsUnitQtyInfo);
		}

		public ZString JI_CustomsUnitQtyDescription => Lookups.CustomsUQList.GetDescriptionFromCode(JI_CustomsUnitQty);

		#endregion

		#region Customs Second Qty & UQ

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[MaxLength(3)]
		public override ZString JI_CustomsSecondUnitQty
		{
			get => base.JI_CustomsSecondUnitQty;
			set => base.JI_CustomsSecondUnitQty = value;
		}

		public bool JI_CustomsSecondUnitQty_ReadOnly => UniversalTariff != null;

		public ZString JI_CustomsSecondUnitQtyDescription => Lookups.CustomsUQList.GetDescriptionFromCode(JI_CustomsSecondUnitQty);

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|JI_CustomsSecondQuantity", Caption = "Second Qty")]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get => base.JI_CustomsSecondQuantity;
			set => base.JI_CustomsSecondQuantity = value;
		}

		public bool JI_CustomsSecondQuantity_ReadOnly => JI_CustomsSecondUnitQty.IsEmpty;

		protected override BaseCustomsQuantityConverter GetCustomsQuantity2Converter()
		{
			return new CustomsQuantityConverter(this, JI_CustomsSecondQuantityInfo, JI_CustomsSecondUnitQtyInfo);
		}

		#endregion

		#region Trade Qty & UQ

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_TradeQuantity", ShortCaption = "Trade Qty", Caption = "Trade Quantity")]
		public override ZDecimal JI_TradeQuantity
		{
			get => base.JI_TradeQuantity;
			set
			{
				var oldValue = JI_TradeQuantity;
				base.JI_TradeQuantity = value;
				if (!IsCopying && oldValue != JI_TradeQuantity)
				{
					CalculateCustomsQuantitiesFromTradeQuantity();

					var tradeUnitPrice = TradeUnitPrice;
					if (!JI_LinePrice.IsEmpty)
					{
						UpdateTradeUnitPriceIfChangedFromLinePriceChanges();
					}
					else if (!tradeUnitPrice.IsEmpty)
					{
						JI_LinePrice = CalculateLinePrice(tradeUnitPrice, JI_TradeQuantity);
					}

					MarkAsNeedingValidation();
				}
			}
		}

		void CalculateCustomsQuantitiesFromTradeQuantity()
		{
			if (!IsCalculateCustomsQtyFromTradeQtySuspended && !JI_TradeQuantity.IsEmpty && !JI_TradeUnitQty.IsEmpty)
			{
				if (!JI_CustomsQuantity_ReadOnly && !JI_CustomsUnitQty.IsEmpty)
				{
					var quantity = UnitConverter.Convert(JI_TradeQuantity, JI_TradeUnitQty, JI_CustomsUnitQty);
					if (quantity > 0)
					{
						JI_CustomsQuantity = quantity;
					}
				}

				if (!JI_CustomsSecondQuantity_ReadOnly && !JI_CustomsSecondUnitQty.IsEmpty)
				{
					var quantity = UnitConverter.Convert(JI_TradeQuantity, JI_TradeUnitQty, JI_CustomsSecondUnitQty);
					if (quantity > 0)
					{
						JI_CustomsSecondQuantity = quantity;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_TradeUnitQtyDescription", ShortCaption = "Trade Qty", Caption = "Trade Quantity")]
		public ZString JI_TradeUnitQtyDescription => Lookups.TradeUnitQtyList.GetDescriptionFromCode(JI_TradeUnitQty);

		public ZPropertyInfo JI_TradeUnitQtyDescriptionInfo => GetZPropertyInfo(nameof(JI_TradeUnitQtyDescription));

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TradeUnitQtyList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|XC_TradeUnitQty", ShortCaption = "UQ", Caption = "Trade UQ")]
		public override ZString JI_TradeUnitQty
		{
			get => base.JI_TradeUnitQty;
			set
			{
				var oldValue = base.JI_TradeUnitQty;
				base.JI_TradeUnitQty = value;
				if (!IsCopying && JI_TradeUnitQty != oldValue)
				{
					CalculateTradeQtyFromInvoiceQtyOrNetWeight();
					CalculateCustomsQuantitiesFromTradeQuantity();
				}
			}
		}

		void CalculateTradeQtyFromInvoiceQtyOrNetWeight()
		{
			if (CanConvertFromNetWeightToCustomsUnit(JI_TradeUnitQty))
			{
				TradeQuantityConverter.CalculateFromNetWeightToCustomsQty();
			}
			else
			{
				TradeQuantityConverter.CalculateCustomsFactorAndQty();
			}
		}

		BaseCustomsQuantityConverter fTradeQuantityConverter;
		BaseCustomsQuantityConverter TradeQuantityConverter => fTradeQuantityConverter ?? (fTradeQuantityConverter = new CustomsQuantityConverter(this, JI_TradeQuantityInfo, JI_TradeUnitQtyInfo));

		#endregion

		#region CalculateCustomsQtyFromTradeQtySuspender

		protected bool IsCalculateCustomsQtyFromTradeQtySuspended => calculateCustomsQtyFromTradeQtySuspenderIndex > 0;
		int calculateCustomsQtyFromTradeQtySuspenderIndex;

		class CalculateCustomsQtyFromTradeQtySuspender : IDisposable
		{
			public static IDisposable SuspendCalculateCustomsQtyFromTradeQty(JobComInvoiceLine invoiceLine)
			{
				return new CalculateCustomsQtyFromTradeQtySuspender(invoiceLine);
			}

			CalculateCustomsQtyFromTradeQtySuspender(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.invoiceLine.calculateCustomsQtyFromTradeQtySuspenderIndex++;
			}

			readonly JobComInvoiceLine invoiceLine;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.calculateCustomsQtyFromTradeQtySuspenderIndex--;
			}

			#endregion
		}

		#endregion

		#region Net Weight

		public override ZDecimal JI_NetWeight
		{
			get => base.JI_NetWeight;
			set
			{
				var oldValue = JI_NetWeight;
				base.JI_NetWeight = value;
				if (!IsCopying && JI_NetWeight != oldValue)
				{
					using (CalculateCustomsQtyFromTradeQtySuspender.SuspendCalculateCustomsQtyFromTradeQty(this))
					{
						TradeQuantityConverter.CalculateFromNetWeightToCustomsQty();
					}
				}
			}
		}

		public override ZString JI_NetWeightUQ
		{
			get => base.JI_NetWeightUQ;
			set
			{
				var oldValue = JI_NetWeightUQ;
				base.JI_NetWeightUQ = value;
				if (!IsCopying && JI_NetWeightUQ != oldValue)
				{
					using (CalculateCustomsQtyFromTradeQtySuspender.SuspendCalculateCustomsQtyFromTradeQty(this))
					{
						TradeQuantityConverter.CalculateFromNetWeightToCustomsQty();
					}
				}
			}
		}

		public override bool CanConvertFromNetWeightToCustomsUnit(ZString customsUnit)
		{
			return JI_NetWeight > 0m && CustomsUnitOfMeasurementListHelper.IsWeightUnit(customsUnit);
		}

		#endregion

		#endregion

		#region Prices

		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|UnitPrice", Caption = "Unit Price")]
		public override ZDecimal UnitPrice
		{
			get => base.UnitPrice;
			set => base.UnitPrice = value;
		}

		public override ZDecimal JI_LinePrice
		{
			get => base.JI_LinePrice;
			set
			{
				var oldValue = JI_LinePrice;
				base.JI_LinePrice = value;

				using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.LinePrice))
				{
					if (!IsCopying && oldValue != JI_LinePrice)
					{
						UpdateTradeUnitPriceIfChangedFromLinePriceChanges();
					}
				}
			}
		}

		protected virtual void UpdateTradeUnitPriceIfChangedFromLinePriceChanges()
		{
			if (!IsSettingTradeUnitPrice && !JI_LinePrice.IsEmpty)
			{
				var newUnitPrice = CalculateTradeUnitPrice();
				if (TradeUnitPrice != newUnitPrice)
				{
					TradeUnitPrice = newUnitPrice;
				}
			}
		}

		ZDecimal CalculateTradeUnitPrice()
		{
			return JI_TradeQuantity.IsEmpty ? ZDecimal.Zero : new ZDecimal(decimal.Round(JI_LinePrice / JI_TradeQuantity, 4));
		}

		ZDecimal? tradeUnitPrice;

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|TradeUnitPrice", Caption = "Trade Unit Price")]
		public ZDecimal TradeUnitPrice
		{
			get
			{
				if (tradeUnitPrice == null)
				{
					tradeUnitPrice = CalculateTradeUnitPrice();
				}
				return tradeUnitPrice.Value;
			}
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(CNLinePriceCalculationFieldSettingType.TradeUnit))
				{
					var oldValue = TradeUnitPrice;
					tradeUnitPrice = value;

					if (!IsCopying && oldValue != TradeUnitPrice)
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateTradeUnitPrice();
						}
						TradeUnitPriceInfo.RefreshBinding();

						if (!IsSettingLinePrice)
						{
							var newLinePrice = CalculateLinePrice(TradeUnitPrice, JI_TradeQuantity);
							if (newLinePrice != JI_LinePrice)
							{
								JI_LinePrice = newLinePrice;
							}
						}
					}
				}
			}
		}

		ZDecimal CalculateLinePrice(ZDecimal unitPrice, ZDecimal tradeQuantity)
		{
			return decimal.Round(unitPrice * tradeQuantity, 4);
		}

		public ZPropertyInfo TradeUnitPriceInfo => GetZPropertyInfo(Schema.TradeUnitPrice);

		bool IsSettingTradeUnitPrice => IsFieldSettingInProgress(CNLinePriceCalculationFieldSettingType.TradeUnit);

		#endregion

		#region Implementation of IAdditionalInformationWrapperParent

		TariffView IAdditionalInformationWrapperParent.UniversalTariff => UniversalTariff;

		ZString IAdditionalInformationWrapperParent.CountryOfOrigin => JI_CountryOfOrigin;

		bool IAdditionalInformationWrapperParent.ElementValueAllowEmpty => false;

		AdditionalInformationCollection IAdditionalInformationWrapperParent.AdditionalInformationCodes => null;

		ValidationModes IValidationModeProvider.ValidationMode => Declaration?.ValidationMode ?? ValidationModes.Full;

		public AdditionalInformationHelper AdditionalInformationHelper => fAdditionalInformationHelper ?? (fAdditionalInformationHelper = new AdditionalInformationHelper(this, JI_NameOfGoodsInfo, XC_GoodsSpecModelInfo, () => IsEnteringOrExiting()));
		AdditionalInformationHelper fAdditionalInformationHelper;

		public AdditionalInformationHelper AdditionalInformation2Helper => fAdditionalInformation2Helper ?? (fAdditionalInformation2Helper = new AdditionalInformationHelper(this, JI_NameOfGoods2Info, XC_GoodsSpecModel2Info, () => IsEnteringOrExiting(true)));
		AdditionalInformationHelper fAdditionalInformation2Helper;

		#endregion

		public void SyncCIQDetails()
		{
			var helper = AdditionalInformationHelper;
			var ciqIngredient = helper.ExtractedIngredient;
			if (!ciqIngredient.IsEmpty)
			{
				CIQIngredient = ciqIngredient;
			}

			var ciqSpecification = helper.ExtractedSpecification;
			if (!ciqSpecification.IsEmpty)
			{
				JI_NDescription = ciqSpecification;
			}

			var ciqBrand = helper.ExtractedBrand;
			if (!ciqBrand.IsEmpty)
			{
				JI_BrandName = ciqBrand;
			}

			var ciqModel = helper.ExtractedModel;
			if (!ciqModel.IsEmpty)
			{
				JI_Model = ciqModel;
			}

			foreach (var date in helper.ExtractedManufactureDates)
			{
				if (!ProductionBatch.Cast<ProductionBatch>().Any(x => x.CY_Date == date))
				{
					ProductionBatch.AddNew().CY_Date = date;
				}
			}

			foreach (ProductionBatch batch in ProductionBatch.Cast<ProductionBatch>().Where(x => x.CY_Data.IsEmpty).ToArray())
			{
				if (!helper.ExtractedManufactureDates.Contains(batch.CY_Date))
				{
					batch.Delete();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|BatchNumbersAsString", ShortCaption = "Batch Nums.", Caption = "Batch Numbers")]
		public ZString BatchNumbersAsString => ProductionBatch.Cast<ProductionBatch>().Select(x => x.CY_Data).DistinctSortAndJoinForDisplay();

		public ZPropertyInfo BatchNumbersAsStringInfo => GetZPropertyInfo(Schema.BatchNumbersAsString);

		[ResourceStringData("Enterprise.Customs.CN.Business.JobComInvoiceLine|ManufactureDatesAsString", ShortCaption = "Manuf. Date", Caption = "Manufacture Dates")]
		public ZString ManufactureDatesAsString => ProductionBatch.Cast<ProductionBatch>().Select(x => x.CY_Date).DistinctSortAndJoinForDisplay(format: x => x.ToString("yyyyMMdd"));

		public ZPropertyInfo ManufactureDatesAsStringInfo => GetZPropertyInfo(Schema.ManufactureDatesAsString);

		public bool TariffIsDangerousChemical => UniversalTariff.HasCommodityTypeDGC();

		public IZZRateSelectionCriteria ExciseRateSelectionCriteria => (exciseRateSelectionCriteria ?? (exciseRateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetExciseRateSelectionCriteriaCore))).Value;
		CachedProperty<IZZRateSelectionCriteria> exciseRateSelectionCriteria;

		protected IZZRateSelectionCriteria GetExciseRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, Universal.Constants.RateTypes.Excise, ZString.Empty);

		public IZZRateSelectionCriteria ExportDutyRateSelectionCriteria => (exportDutyRateSelectionCriteria ?? (exportDutyRateSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetExportDutyRateSelectionCriteriaCore))).Value;
		CachedProperty<IZZRateSelectionCriteria> exportDutyRateSelectionCriteria;

		protected IZZRateSelectionCriteria GetExportDutyRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, Universal.Constants.RateTypes.ExportDuty, ZString.Empty);

		public IZZRateSelectionCriteria GetSpecificRateSelectionCriteria(ZString rateType, ZString rateCode, ZString preference, ZString overrideAdditionalCode)
			=> new SpecificRateSelectionCriteria(this, rateType, rateCode, preference, overrideAdditionalCode);

		public class SpecificRateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
		{
			public SpecificRateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode, ZString preference, ZString overrideAdditionalCode)
				: base(invoiceLine, rateType, rateCode)
			{
				this.PrimaryPreference = preference;
				this.AdditionalCodes = new HashSet<ZString>() { overrideAdditionalCode };
			}
		}

		protected override bool UseUniversalConditionCheck => true;

		public IZZConditionSelectionCriteria RequiredDocumentsConditionSelectionCriteria => Factory.GetValue(ref requiredDocumentsConditionSelectionCriteriaCached, GetRequiredDocumentsConditionSelectionCriteria);
		CachedProperty<IZZConditionSelectionCriteria> requiredDocumentsConditionSelectionCriteriaCached;

		protected IZZConditionSelectionCriteria GetRequiredDocumentsConditionSelectionCriteria() => new CNRequiredDocumentsConditionSelectionCriteria(this);

		public override ConditionChecker.EvaluateConditionValue EvaluateConditionValue => this.CheckConditionForInvoiceLine;

		public override ConditionChecker.GetFriendlyConditionValue GetFriendlyConditionValue => (conditionType, valueType, inputValue) =>
		{
			var result = ZString.Empty;

			if (valueType == RefCusConditionValueTypes.Codes.PresentationOfSupportingDoc)
			{
				result = CNRefCusCodeListLoader.GetRequiredDocuments(Factory, inputValue, EffectiveAssessmentDate)?.ZZD_Description ?? ZString.Empty;
			}
			return result;
		};

		public ValidationModes ValidationMode => throw new NotImplementedException();

		public class CNRequiredDocumentsConditionSelectionCriteria : ZZConditionSelectionCriteria<JobComInvoiceLine>
		{
			public CNRequiredDocumentsConditionSelectionCriteria(JobComInvoiceLine invoiceLine)
				: base(invoiceLine, RefCusConditionTypes.ConditionClass.Control, RefCusCodeListTypes.Codes.CNRequiredDocuments)
			{
			}

			protected override ZString GetTradeGroupCountry(JobComInvoiceLine invoiceLine) => invoiceLine.IsImport ? invoiceLine.EffectiveCountryOfOrigin : invoiceLine.JI_RN_NKCountryOfExport;
			protected override ConditionChecker.ConditionDirection GetDirection(JobComInvoiceLine invoiceLine) => invoiceLine.IsImport ? ConditionChecker.ConditionDirection.Import : ConditionChecker.ConditionDirection.Export;
		}
	}
}
