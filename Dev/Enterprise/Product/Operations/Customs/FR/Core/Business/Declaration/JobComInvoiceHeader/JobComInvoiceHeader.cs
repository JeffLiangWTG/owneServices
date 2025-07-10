using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using InvoiceLineDependentCollection = Enterprise.Customs.EU.Business.Declaration.InvoiceLineDependentCollection;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.FR.IJobComInvoiceHeader, IDeltaSupporter
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		public new InvoiceChargeCollection<InvoiceCharge> Charges => (InvoiceChargeCollection<InvoiceCharge>)base.Charges;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

		public new AddInfoJobComInvoiceHeaderLookups AddInfoLookups => (AddInfoJobComInvoiceHeaderLookups)base.AddInfoLookups;

		public new AddInfoJobComInvoiceHeaderValidation AddInfoValidation => (AddInfoJobComInvoiceHeaderValidation)base.AddInfoValidation;

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => JobDeclaration?.ApplicationExtender.GetJobComInvoiceHeaderValidation(this) ?? new DeltaGJobComInvoiceHeaderValidation(this);

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.France;

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => JobDeclaration?.ApplicationExtender.GetJobComInvoiceHeaderLookups(this) ?? new DeltaGJobComInvoiceHeaderLookups(this);

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new JobComInvoiceGroupHeader GroupHeader => (JobComInvoiceGroupHeader)base.GroupHeader;

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var originalEntryInstruction = InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.EntryInstruction != null).Select(x => x.EntryInstruction)
					.Distinct().OrderBy(x => x.CEI_SubStyle).ThenBy(x => x.CEI_Description).FirstOrDefault(x => x.CEI_DateForDuty.IsValid);
				return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
			}
		}

		protected override ZBool AgreedPlaceCodeSupportAndVisibleCore => JobDeclaration?.IsUCC6 ?? base.AgreedPlaceCodeSupportAndVisibleCore;

		IReadOnlyStrategy GetReadOnlyStrategy() => readOnlyStrategy ?? (readOnlyStrategy = JobDeclaration?.ApplicationExtender.GetJobComInvoiceHeaderReadOnlyStrategy(this));
		IReadOnlyStrategy readOnlyStrategy;

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = JobDeclaration?.ApplicationExtender.GetJobComInvoiceHeaderValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		IValuePostProcessingStrategy GetValuePostProcessingStrategy() => valuePostProcessingStrategy ?? (valuePostProcessingStrategy = JobDeclaration?.ApplicationExtender.GetJobComInvoiceHeaderValuePostProcessingStrategy(this));
		IValuePostProcessingStrategy valuePostProcessingStrategy;

		#region SupportingDocuments

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#endregion

		#region PreviousDocuments

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		#endregion

		public ZBool IsDeltaDStepOneSentOK => Entries.Cast<CusEntryHeader>().Any(x => x.IsDeltaDStepOneSentOK);

		public ZBool IsDeltaDStepTwoSentOK => Entries.Cast<CusEntryHeader>().Any(x => x.IsDeltaDStepTwoSentOK);

		public ZBool IsProvisonalAmountAuthorised
		{
			get
			{
				return AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.Equals(FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode))
					|| (JobDeclaration?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.Equals(FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode)) ?? false)
					|| InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.Equals(FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode)))
					|| SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code.Equals(FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode))
					|| (JobDeclaration?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code.Equals(FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode)) ?? false)
					|| InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code.Equals(FRConstants.DocumentCodes.ProvisonalAmountAuthorisedDocumentCode)));
			}
		}

		public bool HasFreeGoods => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasFreeGoods);

		#region Overriden Properties

		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					if (!IsCopying && oldValue != JZ_IncoTerm)
					{
						GetValuePostProcessingStrategy()?.ValuePostProcess(JZ_IncoTermInfo, oldValue);
					}
				}
			}
		}

		protected override bool JZ_IncoTermPlace_ReadOnly => GetReadOnlyStrategy()?.IsReadonly(JZ_IncoTermPlaceInfo) ?? base.JZ_IncoTermPlace_ReadOnly;

		public override ZString JZ_IncoTermPlace
		{
			get => base.JZ_IncoTermPlace;
			set
			{
				base.JZ_IncoTermPlace = value;
				AddInfo.Validation?.ValidateZG_AgreedPlaceCode();
			}
		}

		public bool ZG_IncotermCountry_ReadOnly => GetReadOnlyStrategy()?.IsReadonly(ZG_IncotermCountryInfo) ?? false;

		[ReadOnlyMember(nameof(ZG_IncotermCountry_ReadOnly))]
		public override ZString ZG_IncotermCountry
		{
			get => base.ZG_IncotermCountry;
			set
			{
				base.ZG_IncotermCountry = value;
				if (!IsCopying)
				{
					GetValueSetStrategy()?.ValueSet(ZG_IncotermCountryInfo, ZG_IncotermCountryInfo.Value);
				}
				AddInfo.Validation.ValidateZG_AgreedPlaceCode();
			}
		}

		public bool ZG_AgreedPlaceCode_ReadOnly => GetReadOnlyStrategy()?.IsReadonly(ZG_AgreedPlaceCodeInfo) ?? false;

		[ReadOnlyMember(nameof(ZG_AgreedPlaceCode_ReadOnly))]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.ZG_AgreedPlaceCode))
				{
					var oldValue = ZG_AgreedPlaceCode;
					base.ZG_AgreedPlaceCode = value;
					if (!IsCopying && oldValue != ZG_AgreedPlaceCode)
					{
						GetValueSetStrategy()?.ValueSet(ZG_AgreedPlaceCodeInfo, oldValue);
						GetValuePostProcessingStrategy()?.ValuePostProcess(ZG_AgreedPlaceCodeInfo, oldValue);
					}
				}
			}
		}

		public override ZGuid JZ_JE
		{
			get => base.JZ_JE;
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;
				if (oldValue == ZGuid.Empty && value.IsValid && JZ_OH_Supplier != ZGuid.Empty && SupplierOrgPK == ZGuid.Empty)
				{
					SupplierOrgPK = JZ_OH_Supplier;
				}
			}
		}

		public override ZGuid JZ_OH_Buyer
		{
			get => base.JZ_OH_Buyer;
			set
			{
				var oldValue = JZ_OH_Buyer;
				base.JZ_OH_Buyer = value;

				if (!IsCopying && oldValue != JZ_OH_Buyer)
				{
					GroupHeader?.PopulateCharges();

					if (IsAttachedToPersistentDeclaration)
					{
						PopulateCharges();
					}
				}
			}
		}

		public override ZGuid JZ_OH_Supplier
		{
			get => base.JZ_OH_Supplier;
			set
			{
				var oldValue = JZ_OH_Supplier;
				base.JZ_OH_Supplier = value;

				if (!IsCopying && oldValue != JZ_OH_Supplier)
				{
					GroupHeader?.PopulateCharges();

					if (IsAttachedToPersistentDeclaration)
					{
						PopulateCharges();
					}
				}
			}
		}

		public override ZDecimal JZ_InvoiceAmount
		{
			get => base.JZ_InvoiceAmount;
			set
			{
				var oldValue = JZ_InvoiceAmount;
				base.JZ_InvoiceAmount = value;

				if (!IsCopying && oldValue != JZ_InvoiceAmount)
				{
					GroupHeader?.PopulateCharges();
				}
			}
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				var oldValue = JZ_RX_NKInvoice_Currency;
				base.JZ_RX_NKInvoice_Currency = value;

				if (!IsCopying && oldValue != JZ_RX_NKInvoice_Currency)
				{
					GroupHeader?.PopulateCharges();
				}
			}
		}

		public bool JZ_AdditionalTerms_ReadOnly => GetReadOnlyStrategy()?.IsReadonly(JZ_AdditionalTermsInfo) ?? false;

		[ReadOnlyMember(nameof(JZ_AdditionalTerms_ReadOnly))]
		public override ZString JZ_AdditionalTerms
		{
			get => base.JZ_AdditionalTerms;
			set => base.JZ_AdditionalTerms = value;
		}

		#endregion

		protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		public ZString DeltaMode => JobDeclaration?.JE_DeltaMode ?? ZString.Empty;

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();

		#region Charges

		SupplierBuyerLinkInvoiceChargesPopulator chargesPopulator;
		internal SupplierBuyerLinkInvoiceChargesPopulator ChargesPopulator => chargesPopulator ?? (chargesPopulator = new SupplierBuyerLinkInvoiceChargesPopulator(this));

		public void PopulateCharges()
		{
			ChargesPopulator.PopulateCharges();
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new InvoiceChargeCollection<InvoiceCharge>(this);

#if DEBUG

		public override ZString IncotermEquivalentToCFRForTesting => Core.Constants.IncoTerms.DeliveredDutyUnpaid;

#endif

		#endregion

		protected override bool IsApplicationCodeAllowedForDefaultingSupportingDocumentCore => true;
	}
}
