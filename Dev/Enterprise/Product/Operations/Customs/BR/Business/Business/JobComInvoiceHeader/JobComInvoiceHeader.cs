using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class JobComInvoiceHeader : AutoBRJobComInvoiceHeader, Integration.Customs.ICusSupportingInfoTypeSupporter, ICanDelete
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			JZ_ValuationDateOverrideInfo.ValueChanged += JZ_ValuationDateOverrideInfo_ValueChanged;
			IsJZ_InvoiceCurrExRateUserEnterableInfo.ValueChanged += IsJZ_InvoiceCurrExRateUserEnterableInfo_ValueChanged;
		}

		public new class Schema : AutoBRJobComInvoiceHeader.Schema
		{
			public const string ExchangeHedgeType = nameof(JobComInvoiceHeader.ExchangeHedgeType);
			public const string ExchangeHedgePaymentMethod = nameof(JobComInvoiceHeader.ExchangeHedgePaymentMethod);
			public const string ExchangeHedgePaymentDeadline = nameof(JobComInvoiceHeader.ExchangeHedgePaymentDeadline);
			public const string ExchangeHedgeReason = nameof(JobComInvoiceHeader.ExchangeHedgeReason);
			public const string ExchangeHedgeValue = nameof(JobComInvoiceHeader.ExchangeHedgeValue);
			public const string ExchangeHedgeROFBACENNumber = nameof(JobComInvoiceHeader.ExchangeHedgeROFBACENNumber);
			public const string ExchangeHedgeFinancialInstitution = nameof(JobComInvoiceHeader.ExchangeHedgeFinancialInstitution);
			public const string ExchangeRateDate = nameof(JobComInvoiceHeader.ExchangeRateDate);
			public const string SupplierDocOrgPK = nameof(JobComInvoiceHeader.SupplierDocOrgPK);
			public const string SupplierDocAddressPK = nameof(JobComInvoiceHeader.SupplierDocAddressPK);
			public const int ExchangeHedgeTypeMaxLength = 1;
			public const int AdditionalTermsMaxLength = 250;
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return Core.Constants.CurrencyCodes.Brazil; }
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
		}

		public ZBool IsImportOnly => Factory.GetCached(ref isImportOnlyCached, () => PersistentDeclaration is JobDeclaration declaration
					? declaration.IsImportOnly : IsImportForStandAloneInvoice);
		CachedProperty<ZBool> isImportOnlyCached;

		public ZBool IsImportLicense => JobDeclaration?.IsImportLicense ?? ZBool.False;

		public ZBool IsImportSiscomex => JobDeclaration?.IsImportSiscomex ?? ZBool.False;

		public ZBool IsImportExcludingLicense => !IsImportLicense && IsImport;

		public ZBool IsLPCO => JobDeclaration?.IsLPCO ?? ZBool.False;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationCodeList))]
		[ResourceStringData("F090848C-EAD4-42C7-A2F0-DB4083D35AE4", Caption = "Valuation Code")]
		public override ZString JZ_ValuationCode { get => base.JZ_ValuationCode; set => base.JZ_ValuationCode = value; }

		[ResourceStringData("27B04E61-5BEB-4254-83B5-91C96053AF99", Caption = "Buy-Seller Relationship")]
		public override ZString JZ_RelatedIndicator { get => base.JZ_RelatedIndicator; set => base.JZ_RelatedIndicator = value; }

		#region Supplier

		void PopulateValuesFromSupplierLink()
		{
			if (SupplierBuyerLink != null && IsImportExcludingLicense)
			{
				if (!SupplierBuyerLink.OL_RelatedParty.IsEmpty && JZ_RelatedIndicator.IsEmpty)
				{
					JZ_RelatedIndicator = SupplierBuyerLink.OL_RelatedParty;
				}
			}
		}

		void PopulateValuesFromForeignOperator()
		{
			JZ_SupplierAuthorityIdentifier = IsImportOnly && ForeignOperator != null ? ForeignOperator.BFR_AuthorityIdentifier : ZString.Empty;
			JZ_SupplierAuthorityVersion = IsImportOnly && ForeignOperator != null ? ForeignOperator.BFR_AuthorityVersion : ZString.Empty;
		}

		public override ZGuid JZ_OH_Supplier
		{
			get { return base.JZ_OH_Supplier; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					var oldValue = JZ_OH_Supplier;
					base.JZ_OH_Supplier = value;

					if (!IsCopying && oldValue != JZ_OH_Supplier)
					{
						PopulateValuesFromSupplierLink();
						PopulateValuesFromForeignOperator();
						InvoiceLines?.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override void DefaultSupplierAddressFromSupplier(ZGuid newSupplierPK)
		{
			if (SupplierAddress?.OA_OH != newSupplierPK)
			{
				var newAddressPK = Supplier?.MainAddress.PK ?? ZGuid.Empty;
				if (newAddressPK != JZ_OA_SupplierAddress)
				{
					JZ_OA_SupplierAddress = newAddressPK;
					JZ_OA_SupplierAddress_ZAddress.OrgPK = newSupplierPK;
				}
			}
		}

		public override ZString SupplierName => SupplierAddress?.EffectiveCompanyNameTruncated ?? base.SupplierName;

		protected override ZAddress GetNewJZ_OA_SupplierAddress_ZAddress()
		{
			var address = base.GetNewJZ_OA_SupplierAddress_ZAddress();
			address.IsOrgVisible = true;
			address.OrgPKValidation = Validation.ValidateSupplierOrgPK;
			return address;
		}

		public static ResourceStringData SupplierCaption => Res.GetData("6F07EDFD-CF3E-4921-82BA-BA4CB13221F7", "Supplier");

		public bool SupplierAddressIsAvailable => IsAttachedToPersistentDeclaration && IsImportExcludingLicense;

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|BR_SupplierAuthorityIdentifier", Caption = "Supplier Authority")]
		public override ZString JZ_SupplierAuthorityIdentifier { get => base.JZ_SupplierAuthorityIdentifier; set => base.JZ_SupplierAuthorityIdentifier = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|BR_SupplierAuthorityVersion", Caption = "Supplier Version")]
		public override ZString JZ_SupplierAuthorityVersion { get => base.JZ_SupplierAuthorityVersion; set => base.JZ_SupplierAuthorityVersion = value; }

		#region SupplierDocumentaryAddress

		public JobDocAddress SupplierDocumentaryAddress
		{
			get
			{
				if (fSupplierDocumentaryAddress == null || fSupplierDocumentaryAddress.IsDeleted)
				{
					fSupplierDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
					if (fSupplierDocumentaryAddress != null)
					{
						fSupplierDocumentaryAddress.DefaultAddressType = AddressType.NoDefault;
						fSupplierDocumentaryAddress.DocAddressChanged += SupplierDocumentaryAddress_DocAddressChanged;
						if (!fSupplierDocumentaryAddress.IsInDatabase)
						{
							fSupplierDocumentaryAddress.OrganisationPK = JZ_OH_Supplier;
						}
					}
				}
				return fSupplierDocumentaryAddress;
			}
		}

		JobDocAddress fSupplierDocumentaryAddress;

		void SupplierDocumentaryAddress_DocAddressChanged(object sender, EventArgs e)
		{
			var newDocAddressPK = SupplierDocumentaryAddress.E2_OA_Address;
			if (IsImportLicense && JZ_OA_SupplierAddress != newDocAddressPK)
			{
				JZ_OA_SupplierAddress = newDocAddressPK;
			}
			MarkAsNeedingValidation();
		}

		[ReadOnlyMember(nameof(AnyLineHasLinkedInvoiceLine))]
		public override ZGuid JZ_OA_SupplierAddress
		{
			get => base.JZ_OA_SupplierAddress;
			set
			{
				var oldValue = JZ_OA_SupplierAddress;
				base.JZ_OA_SupplierAddress = value;
				if (!IsCopying && JZ_OA_SupplierAddress != oldValue)
				{
					if (IsImportLicense && JZ_OA_SupplierAddress != SupplierDocumentaryAddress.E2_OA_Address)
					{
						SupplierDocumentaryAddress.E2_OA_Address = value;
					}
					if (IsImport)
					{
						InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_ManufacturerIndicator = ZString.Empty);
					}
				}
			}
		}

		[ReadOnlyMember(nameof(AnyLineHasLinkedInvoiceLine))]
		public override ZString JZ_RX_NKInvoice_Currency { get => base.JZ_RX_NKInvoice_Currency; set => base.JZ_RX_NKInvoice_Currency = value; }

		[ReadOnlyMember(nameof(AnyLineHasLinkedInvoiceLine))]
		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set
			{
				var oldValue = base.JZ_IncoTerm;
				base.JZ_IncoTerm = value;
				if (!IsCopying && oldValue != JZ_IncoTerm && JobDeclaration != null)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(Schema.AdditionalTermsMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|JZ_AdditionalTerms", Caption = "Complement", FullDescription = "The complement related to the choice of the OCV - Other Condition of Sale Incoterm or the reason for the charges.")]
		public override ZString JZ_AdditionalTerms { get => base.JZ_AdditionalTerms; set => base.JZ_AdditionalTerms = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|SupplierDocOrgPK", Caption = "Supplier")]
		public ZGuid SupplierDocOrgPK
		{
			get => SupplierDocumentaryAddress.OrganisationPK;
			set
			{
				SupplierDocumentaryAddress.OrganisationPK = value;
				SupplierDocOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SupplierDocOrgPKInfo => GetWrappedZPropertyInfo(Schema.SupplierDocOrgPK, x => SupplierDocumentaryAddress.OrganisationPKInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|SupplierDocAddressPK", Caption = "Supplier Address")]
		public ZGuid SupplierDocAddressPK
		{
			get => SupplierDocumentaryAddress.E2_OA_Address;
			set
			{
				SupplierDocumentaryAddress.E2_OA_Address = value;
				SupplierDocAddressPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SupplierDocAddressPKInfo => GetWrappedZPropertyInfo(Schema.SupplierDocAddressPK, x => SupplierDocumentaryAddress.E2_OA_AddressInfo);

		#endregion

		#endregion

		#region ExchangeRateDate

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				if (IsImportExcludingLicense)
				{
					return JZ_ValuationDateOverride.IsValid ? JZ_ValuationDateOverride : LatestCustomsExchangeRateDate;
				}
				else
				{
					return base.EffectiveValuationDateCore;
				}
			}
		}

		[BusinessObjectTestExclude()]
		[ResourceStringData("74C4D9AD-F5C2-4A7A-B08B-D9FE011E1CDF", Caption = "Exchange Rate Date")]
		[ReadOnlyMember(nameof(ExchangeRateDate_ReadOnly))]
		public ZDateTime ExchangeRateDate
		{
			get => EffectiveValuationDateCore;
			set
			{
				JZ_ValuationDateOverride = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateExchangeRateDate();
				}
				ExchangeRateDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeRateDateInfo
		{
			get { return GetZPropertyInfo(Schema.ExchangeRateDate); }
		}

		bool ExchangeRateDate_ReadOnly => !IsJZ_InvoiceCurrExRateUserEnterable;

		protected override bool JZ_InvoiceCurrExRate_ReadOnly => true;

		void JZ_ValuationDateOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			JZ_InvoiceCurrExRate = (CurrencyConverter as CurrencyConverterWithFixedExchangeRatesDataProvider)?.GetExchangeRateToDefault(Invoice_Currency) ?? 0m;
		}

		void IsJZ_InvoiceCurrExRateUserEnterableInfo_ValueChanged(object sender, EventArgs e)
		{
			JZ_ValuationDateOverride = IsJZ_InvoiceCurrExRateUserEnterable ? LatestCustomsExchangeRateDate : ZDateTime.Empty;
		}

		public ZDateTime LatestCustomsExchangeRateDate => Factory.GetCached(ref fLatestCustomsExchangeRateDate, GetLatestCustomsExchangeRateDate);
		CachedProperty<ZDateTime> fLatestCustomsExchangeRateDate;

		ZDateTime GetLatestCustomsExchangeRateDate()
		{
			var foundRateDate = ZDateTime.Empty;
			var exchangeRate = Invoice_Currency?.GetRateForDate(ExchangeRateType.Customs, ZDateTime.Today, 300, out foundRateDate) ?? 0m;
			return exchangeRate > 0 ? foundRateDate : JobDeclaration?.DateOfValuation ?? ZDateTime.Today;
		}

		#endregion

		#region Fetch Hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceHeaderFetchStrategy(this);
		}

		class JobComInvoiceHeaderFetchStrategy : BaseJobComInvoiceHeaderFetchStrategy
		{
			public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoice)
				: base(invoice)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.JZ_OA_SupplierAddress);
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
				var ownerPK = BusinessObject.JobDeclaration?.JE_OH_Importer ?? ZGuid.Empty;
				var supplierPK = BusinessObject.JZ_OH_Supplier;
				if (ownerPK.IsValid && supplierPK.IsValid)
				{
					Factory.AddFetchHint(CusBRForeignOperatorSchema.Instance,
						new ZQuery(CusBRForeignOperatorSchema.BFR_OH_Owner, ownerPK),
						new ZQuery(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, supplierPK));
				}
			}
		}

		#endregion

		#region ExchangeHedge

		public bool IsExchangeHedgeAppliable => IsImport;

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ExchangeHedgeCollection ExchangeHedgeCollection
		{
			get
			{
				if (fExchangeHedgeCollection == null)
				{
					fExchangeHedgeCollection = new ExchangeHedgeCollection(this);
					fExchangeHedgeCollection.Load();
					RegisterEditableChildObject(fExchangeHedgeCollection);
				}
				if (IsExchangeHedgeAppliable && !fExchangeHedgeCollection.Any())
				{
					fExchangeHedgeCollection.AddNew();
				}
				return fExchangeHedgeCollection;
			}
		}
		ExchangeHedgeCollection fExchangeHedgeCollection;

		public ExchangeHedge ExchangeHedge => ExchangeHedgeCollection.FirstOrDefault() ?? ExchangeHedgeCollection.AddNew();

		[MaxLength(Schema.ExchangeHedgeTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ExchangeHedgeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgeType", ShortCaption = "Type", Caption = "Exchange Hedge Type", FullDescription = "The Exchange Hedge Type.")]
		public ZString ExchangeHedgeType
		{
			get => ExchangeHedge.CSI_Code;
			set
			{
				ExchangeHedge.CSI_Code = value;
				ExchangeHedgeTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgeTypeInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgeType, x => ExchangeHedge.CSI_CodeInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ExchangeHedgePaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgePaymentMethod", ShortCaption = "Payment Method", Caption = "Exchange Hedge Payment Method")]
		public ZString ExchangeHedgePaymentMethod
		{
			get => ExchangeHedge.CSI_SubType;
			set
			{
				ExchangeHedge.CSI_SubType = value;
				ExchangeHedgePaymentMethodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgePaymentMethodInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgePaymentMethod, x => ExchangeHedge.CSI_SubTypeInfo);

		[MaxLength(3)]
		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgePaymentDeadline", ShortCaption = "Payment Deadline", Caption = "Exchange Hedge Payment Deadline", FullDescription = "The Payment Deadline, stated in days.")]
		public ZDecimal ExchangeHedgePaymentDeadline
		{
			get => ExchangeHedge.CSI_Quantity;
			set
			{
				ExchangeHedge.CSI_Quantity = value;
				ExchangeHedgePaymentDeadlineInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgePaymentDeadlineInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgePaymentDeadline, x => ExchangeHedge.CSI_QuantityInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ReasonTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgeReason", ShortCaption = "Reason", Caption = "Exchange Hedge Reason", FullDescription = "When there is no Exchange Hedge, a Reason must be informed.")]
		public ZString ExchangeHedgeReason
		{
			get => ExchangeHedge.CSI_AdditionalDescription;
			set
			{
				ExchangeHedge.CSI_AdditionalDescription = value;
				ExchangeHedgeReasonInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgeReasonInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgeReason, x => ExchangeHedge.CSI_AdditionalDescriptionInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.FinancialInstitutionList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgeFinancialInstitution", ShortCaption = "Financial Institution", Caption = "Exchange Hedge Financial Institution")]
		public ZString ExchangeHedgeFinancialInstitution
		{
			get => ExchangeHedge.CSI_IssuerType;
			set
			{
				ExchangeHedge.CSI_IssuerType = value;
				ExchangeHedgeFinancialInstitutionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgeFinancialInstitutionInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgeFinancialInstitution, x => ExchangeHedge.CSI_IssuerTypeInfo);

		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgeValue", ShortCaption = "Value", Caption = "Exchange Hedge Value", FullDescription = "The financed amount.")]
		public ZDecimal ExchangeHedgeValue
		{
			get => ExchangeHedge.CSI_Value;
			set
			{
				ExchangeHedge.CSI_Value = value;
				ExchangeHedgeValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgeValueInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgeValue, x => ExchangeHedge.CSI_ValueInfo);

		[MaxLength(8)]
		[ResourceStringData("Enterprise.Customs.BR.Business.JobComInvoiceHeader|ExchangeHedgeROFBACENNumber", ShortCaption = "ROF/BACEN Number", Caption = "Exchange Hedge ROF/BACEN Number", FullDescription = "The registration number of the financial operation assigned by BACEN.")]
		public ZString ExchangeHedgeROFBACENNumber
		{
			get => ExchangeHedge.CSI_ReferenceNumber;
			set
			{
				ExchangeHedge.CSI_ReferenceNumber = value;
				ExchangeHedgeROFBACENNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExchangeHedgeROFBACENNumberInfo => GetWrappedZPropertyInfo(Schema.ExchangeHedgeROFBACENNumber, x => ExchangeHedge.CSI_ReferenceNumberInfo);

		#endregion

		#region IDocAddresses Members

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobComInvoiceHeaderJobDocAddressValidation(addressToValidate, this);
		}

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			return new DocAddressType[] { DocAddressType.SupplierDocumentaryAddress };
		}

		#endregion

		#region CanDelete

		public bool AnyLineAttachedToImportLicenseLine => Factory.GetCached(ref anyLineAttachedToImportLicenseLine,
			() => JobComInvoiceLine.AttachedToImportLicenseLines(Factory, InvoiceLines.Cast<JobComInvoiceLine>().ToArray()));
		CachedProperty<bool> anyLineAttachedToImportLicenseLine;

		public bool AnyLineIsImportLicenseGeneratedFromImportSiscomexLine => Factory.GetCached(ref anyLineIsImportLicenseGeneratedFromImportSiscomexLine,
			() => InvoiceLines.Count > 0 && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsImportLicenseGeneratedFromImportSiscomexLine));
		CachedProperty<bool> anyLineIsImportLicenseGeneratedFromImportSiscomexLine;

		bool ICanDelete.CanDelete => InvoiceLines.Count == 0 || (!AnyLineAttachedToImportLicenseLine && !AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				if (IsImportLicense && AnyLineIsImportLicenseGeneratedFromImportSiscomexLine)
				{
					return ResString.GetMultilingualString("D25849BE-9BC9-4553-BFEB-15F41FD6912E", "The Invoice Header cannot be deleted, because there is Import License line attached to some Import Entries.");
				}
				else if (AnyLineAttachedToImportLicenseLine)
				{
					return ResString.GetMultilingualString("7db25b0c-f000-468b-835e-28b419764089", "The Invoice Header cannot be deleted, because there is Import License line reference some Invoice Lines on it.");
				}

				return base.ReasonForNotAbleToDelete;
			}
		}

		#endregion

		public CusBRForeignOperator ForeignOperator
		{
			get
			{
				var ownerPK = JobDeclaration?.JE_OH_Importer ?? ZGuid.Empty;
				return new CusBRForeignOperator.Loader(Factory).LoadByOwnerAndForeignOperator(ownerPK, JZ_OH_Supplier);
			}
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.ExchangeHedge, typeof(ExchangeHedge) },
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public bool AnyLineHasLinkedInvoiceLine => IsImport && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.HasLinkedInvoiceLine);

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!IsDeleted)
			{
				if (!IsExchangeHedgeAppliable)
				{
					ExchangeHedgeCollection.RemoveAndDeleteAll();
				}

				if (!IsImportLicense)
				{
					DocAddresses.FindByDocAddressType(DocAddressType.SupplierDocumentaryAddress)?.Delete();
				}
			}
		}

		public void UpdateTotalValuesFromInvoiceLines()
		{
			JZ_Weight = InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_Weight, x.JI_WeightUQ, JZ_WeightUQ));
			JZ_NetWeight = InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_NetWeight, x.JI_NetWeightUQ, JZ_NetWeightUQ));
			JZ_InvoiceAmount = InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_InvAmount);
		}

		protected override ExchangeRateType RateTypeCore => IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

		protected override JobComInvoiceHeaderDeepCopyStrategy GetTemplateCopyStrategy(CloneType cloneType)
		{
			return new JobComInvoiceHeaderDeepCloneStrategy(this, cloneType, JobDeclaration, null);
		}

		protected override ZString GetMessageTypeFromDeclaration(BaseJobDeclaration declaration)
		{
			switch (declaration.JE_MessageType)
			{
				case BRJobMessageTypeList.Codes.ImportSiscomex:
				case BRJobMessageTypeList.Codes.ImportLicense:
					return BRJobMessageTypeList.Codes.Import;
				case BRJobMessageTypeList.Codes.LPCO:
					return BRJobMessageTypeList.Codes.Export;
				default:
					return base.GetMessageTypeFromDeclaration(declaration);
			}
		}
	}
}
