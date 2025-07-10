using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class JobComInvoiceHeader : AutoJobComInvoiceHeader, IDocAddresses, IBuyerSupplierRelationshipConsumer, ICurrencyConverterDataProvider, ISynchroniserReadOnlyMembersProvider, ICanDelete, IDisposable, ICustomsCustomLabelsConfigOrgProvider, IAddInfoChildSupporter, Integration.Customs.CA.IJobComInvoiceHeader
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsB2AsAccountForSeededHeader)
			{
				this.ReadOnly = true;
			}

			BuyerSupplierLinksHelper = new BuyerSupplierLinksHelper<JobComInvoiceHeader>(this);
			BuyerSupplierLinksHelper.Register();
		}

		#region Schema

		public new partial class Schema : AutoJobComInvoiceHeader.Schema
		{
			public const string TotalValueForDuty = "TotalValueForDuty";
			public const string GoodsShipmentSequence = "GoodsShipmentSequence";
			public const string FirstPackageNumber = "FirstPackageNumber";
			public const string FirstPackageIsLinked = "FirstPackageIsLinked";
			public const string FirstPackageQty = "FirstPackageQty";
		}

		#endregion

		[ResourceStringData("ef2c753e-393c-4914-a328-08a1348a1c2d", Caption = "First Package Number", ShortCaption = "1st Pkg #")]
		public ZString FirstPackageNumber => FirstCusLinkPackage?.PackageNumber ?? "";

		public ZPropertyInfo FirstPackageNumberInfo => GetZPropertyInfo(Schema.FirstPackageNumber);

		[ReadOnlyMember(nameof(IsLinked_ReadOnly))]
		[ResourceStringData("67ca4ad5-b8ac-4779-ac4c-24d2420f437f", Caption = "First Package Is Linked", ShortCaption = "1st Pkg Is Linked", FullDescription = "First Package Is For Invoice Header?")]
		public ZBool FirstPackageIsLinked
		{
			get
			{
				return FirstCusLinkPackage?.IsLinked ?? false;
			}
			set
			{
				if (FirstCusLinkPackage != null)
				{
					FirstCusLinkPackage.IsLinked = value;
				}
			}
		}

		public ZPropertyInfo FirstPackageIsLinkedInfo => GetZPropertyInfo(Schema.FirstPackageIsLinked);

		bool IsLinked_ReadOnly => FirstCusLinkPackage == null;

		[ReadOnlyMember(nameof(IsPackQty_ReadOnly))]
		[ResourceStringData("aafe7a00-5771-4d7a-9099-dd7e6c59138b", Caption = "First Package Quantity", ShortCaption = "1st Pkg Qty")]
		public ZInt FirstPackageQty
		{
			get
			{
				return FirstCusLinkPackage?.PackQty ?? 0;
			}
			set
			{
				if (FirstCusLinkPackage != null)
				{
					if (!value.IsEmpty && !FirstPackageIsLinked)
					{
						FirstPackageIsLinked = true;
					}
					FirstCusLinkPackage.PackQty = value;
				}
			}
		}

		public ZPropertyInfo FirstPackageQtyInfo => GetZPropertyInfo(Schema.FirstPackageQty);

		bool IsPackQty_ReadOnly => FirstCusLinkPackage == null || !FirstCusLinkPackage.IsLinked;

		public InvoiceHeaderCusLinkPackage FirstCusLinkPackage
		{
			get
			{
				if (firstCusLinkPackage == null)
				{
					((IBindingList)PackagesForInvoicesForBindingOnly).ListChanged -= BaseCusLinkPackageCollection_ListChanged;
					((IBindingList)PackagesForInvoicesForBindingOnly).ListChanged += BaseCusLinkPackageCollection_ListChanged;
					firstCusLinkPackage = new RecalculableCachedValue<InvoiceHeaderCusLinkPackage>(() => (PackagesForInvoicesForBindingOnly.Count > 0 ? PackagesForInvoicesForBindingOnly[0] : null) as InvoiceHeaderCusLinkPackage);
				}

				return firstCusLinkPackage.Value;
			}
		}
		RecalculableCachedValue<InvoiceHeaderCusLinkPackage> firstCusLinkPackage;

		void BaseCusLinkPackageCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshBindingIfFirstCusLinkPackageChanged();
		}

		void RefreshBindingIfFirstCusLinkPackageChanged()
		{
			var oldItem = FirstCusLinkPackage;
			firstCusLinkPackage.InvalidateCache();
			var newItem = FirstCusLinkPackage;
			if (oldItem == null || newItem == null)
			{
				return;
			}

			if (!IsCopying)
			{
				if (!object.ReferenceEquals(oldItem, newItem))
				{
					RefreshBindingForFirstCusLinkPackageProperty(oldItem, newItem);
				}
			}
		}

		void RefreshBindingForFirstCusLinkPackageProperty(BaseCusLinkPackage oldItem, BaseCusLinkPackage newItem)
		{
			RefreshBinding(oldItem, newItem, (x) => x.PackageNumber, () => FirstPackageNumberInfo);
			RefreshBinding(oldItem, newItem, (x) => x.PackQty, () => FirstPackageQtyInfo);
			RefreshBinding(oldItem, newItem, (x) => x.IsLinked, () => FirstPackageIsLinkedInfo);
		}

		void RefreshBinding(BaseCusLinkPackage oldItem, BaseCusLinkPackage newItem, Func<BaseCusLinkPackage, IZType> getValue, Func<ZPropertyInfo> getInfo)
		{
			var oldValue = getValue(oldItem);
			if (oldValue != getValue(newItem))
			{
				getInfo().RefreshBinding(oldValue);
			}
		}

		protected override void SetSupplierFromDeclaration()
		{
			base.SetSupplierFromDeclaration();
			if (JobDeclaration is JobDeclaration declaration)
			{
				if (CA_RL_NKLastPort.IsEmpty && !declaration.JE_RL_NKPortOfLoading.IsEmpty)
				{
					CA_RL_NKLastPort = declaration.JE_RL_NKPortOfLoading;
				}
				if (JZ_ValuationDateOverride.IsEmpty && !declaration.JE_ExportDate.IsEmpty)
				{
					JZ_ValuationDateOverride = declaration.JE_ExportDate;
				}
			}
		}

		public bool IsAttachedToPersistentLVSDeclaration
		{
			get { return IsAttachedToPersistentDeclaration && JobDeclaration.IsLVS; }
		}

		public JobDeclaration PersistentLVXDeclaration => GetPersistentDeclaration<JobDeclaration>(x => x.IsLVX);

		public bool IsAttachedToPersistentLVXDeclaration
		{
			get { return IsAttachedToPersistentDeclaration && JobDeclaration.IsLVX; }
		}

		public bool IsAttachedToPersistentConsolidatedLVS
		{
			get { return IsAttachedToPersistentDeclaration && FirstAdditionalOrOnlyDeclaration.IsConsolidatedLVS; }
		}

		public Money IncludedConstruction
		{
			get { return GetIncludedAmountWithThisChargeType(CAChargeTypeList.Codes.Construction); }
		}

		public ZDecimal InvoiceAmountInCAD
		{
			get { return GetAmountInCanadianDollars(new Money(JZ_InvoiceAmount, Invoice_Currency), false); }
		}

		public ZBool IsSimplifiedLVSMode
		{
			get { return IsAttachedToPersistentDeclaration && JobDeclaration.IsSimplifiedLVSMode; }
		}

		public bool IsGotingToBeDeleted { get; set; }

		public ZBool IsDDPDeductDutyOnlyRequired
		{
			get
			{
				return IsAttachedToPersistentDeclaration && JZ_IncoTerm == Constants.IncoTerms.DeliveredDutyPaid;
			}
		}

		#region Override Properties

		public override bool CanDetach
		{
			get { return base.CanDetach && GoodsShipmentSequence.IsEmpty; }
		}

		public override string ReasonNotToBeAbleToDetach
		{
			get
			{
				return !GoodsShipmentSequence.IsEmpty ? ResString.GetMultilingualString("CADInvoiceNotAbleToDetach", "Invoice not allowed to be detached because there is CAD response for it.") : base.ReasonNotToBeAbleToDetach;
			}
		}

		[ReadOnlyMember(nameof(JZ_NoOfPacks_ReadOnly))]
		public override ZDecimal JZ_NoOfPacks
		{
			get => base.JZ_NoOfPacks;
			set => base.JZ_NoOfPacks = value;
		}

		bool JZ_NoOfPacks_ReadOnly
		{
			get
			{
				var declaration = JobDeclaration;
				return declaration != null && (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking || declaration.SupportsChcPivotBetweenInvoiceLineAndPacking);
			}
		}

		public override bool SupportsRelatedBill
		{
			get { return IsImport && !(JobDeclaration?.IsLVS ?? false); }
		}

		#region JZ_InvoiceNumber

		[ReadOnlyMember(nameof(JZ_InvoiceNumber_ReadOnly))]
		public override ZString JZ_InvoiceNumber
		{
			get { return base.JZ_InvoiceNumber; }
			set
			{
				var setVal = value;
				var declaration = JobDeclaration;
				if (declaration != null && (declaration.IsB2Adjustments || declaration.IsB3X))
				{
					if (setVal == NewSubHeaderPrefix)
					{
						var highestNewSubHeaderSet = ZString.Empty;
						var newSubHeaders = declaration.Invoices.Where(x => x.JZ_InvoiceNumber.StartsWith(NewSubHeaderPrefix, StringComparison.CurrentCulture));
						if (newSubHeaders.Any())
						{
							highestNewSubHeaderSet = newSubHeaders.Max(x => x.JZ_InvoiceNumber.SubstringSafe(2, 1));
						}
						setVal = NewSubHeaderPrefix + (ZInt.ParseSafe(highestNewSubHeaderSet, 0) + 1);
					}
					else if (!setVal.StartsWith(NewSubHeaderPrefix) && !int.TryParse(setVal, out _))
					{
						setVal = System.Text.RegularExpressions.Regex.Replace(setVal, @"[^0-9]+", "");
					}
				}

				base.JZ_InvoiceNumber = setVal;
				EnableAndSynchronise(true);
			}
		}
		const string NewSubHeaderPrefix = "NS";

		bool JZ_InvoiceNumber_ReadOnly
		{
			get
			{
				var declaration = JobDeclaration;
				return declaration != null && (declaration.IsB2Adjustments || declaration.IsB3X) && !JZ_InvoiceNumber.IsEmpty;
			}
		}

		#endregion

		#region JZ_IncoTerm

		public override ZString JZ_IncoTerm
		{
			get { return base.JZ_IncoTerm; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					var newValue = JZ_IncoTerm;
					if (!IsCopying && oldValue != newValue)
					{
						if (newValue == Constants.IncoTerms.DeliveredDutyPaid
							|| oldValue == Constants.IncoTerms.DeliveredDutyPaid)
						{
							ResetLineCalculationMethod();
						}

						if (PersistentLVXDeclaration is JobDeclaration declaration && declaration.JE_ShipmentIncoTerm != newValue)
						{
							declaration.JE_ShipmentIncoTerm = newValue;
						}

						if (newValue != Constants.IncoTerms.DeliveredDutyPaid && CA_DDPDeductDutyOnly)
						{
							CA_DDPDeductDutyOnly = false;
						}
					}
				}
			}
		}

		internal ZString CalculationMethod
		{
			get
			{
				return JZ_IncoTerm == Constants.IncoTerms.DeliveredDutyPaid ? CalculationMethods.Codes.DeliveredDutyPaid : CalculationMethods.Codes.NoRemission;
			}
		}

		internal void ResetLineCalculationMethod()
		{
			var calculationMethod = CalculationMethod;
			foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>().Where(l => !l.IsRemissionRepairLine).ToArray())
			{
				invoiceLine.ResetCalculationMethod(calculationMethod);
			}
		}

		#endregion

		#region JZ_RN_NKDefaultOrigin

		public override ZString JZ_RN_NKDefaultOrigin
		{
			get { return base.JZ_RN_NKDefaultOrigin; }
			set
			{
				var oldValue = JZ_RN_NKDefaultOrigin;
				base.JZ_RN_NKDefaultOrigin = value;
				if (!IsCopying && oldValue != JZ_RN_NKDefaultOrigin)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
					if (JZ_RW_NKOriginState_ReadOnly)
					{
						JZ_RW_NKOriginState = ZString.Empty;
					}

					if (JobDeclaration != null && JobDeclaration.IsLVS)
					{
						foreach (JobComInvoiceLine line in JobComInvoiceLines)
						{
							line.Validation.ValidateJI_CountryOfOrigin();
						}
					}

					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						if (!JZ_RN_NKDefaultOrigin.IsEmpty && line.IsRegulatedByIIDCFIA && line.CA_RN_NKSource.IsEmpty)
						{
							line.CA_RN_NKSource = JZ_RN_NKDefaultOrigin;
						}

						line.JI_CountryOfOriginInfo.RefreshBinding();
						if (line.JI_StateOrRegionOfOrigin_ReadOnly)
						{
							line.JI_StateOrRegionOfOrigin = ZString.Empty;
						}

						if (line.ShouldRefreshSIMAMeasuresFromInvoiceHeader)
						{
							line.RefreshSIMAMeasuresCollection();
						}
					}
				}
			}
		}

		#endregion

		#region JZ_OH_Buyer

		[ReadOnlyMember(nameof(JZ_OH_Buyer_ReadOnly))]
		public override ZGuid JZ_OH_Buyer
		{
			get { return base.JZ_OH_Buyer; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Buyer))
				{
					var oldValue = JZ_OH_Buyer;
					base.JZ_OH_Buyer = value;
					var newValue = JZ_OH_Buyer;
					if (!IsCopying && oldValue != newValue)
					{
						var declaration = JobDeclaration;
						declaration?.MarkAsNeedingValidation();
						foreach (JobComInvoiceLine line in JobComInvoiceLines)
						{
							line.PartSyncManager.Refresh();
							line.MarkAsNeedingValidation();
						}
						DefaultBuyerDocumentaryAddress();
						if ((declaration?.IsPersistent ?? false) && declaration.IsLVX && declaration.JE_OH_Importer != newValue)
						{
							declaration.JE_OH_Importer = newValue;
						}

						CA_ReadyForConsolidation = false;
					}
				}
			}
		}

		bool JZ_OH_Buyer_ReadOnly
		{
			get
			{
				return IsAttachedToPersistentLVSDeclaration && FirstAdditionalOrOnlyDeclaration is JobDeclaration declaration
				&& declaration.JE_MessageSubType != LowValueShipmentsTypes.Codes.TotalConsolidation
				&& declaration.JE_MessageType != JobMessageTypeList.Codes.LVSForConsolidation;
			}
		}

		#endregion

		#region JZ_OH_Consignee

		public override ZGuid JZ_OH_Consignee
		{
			get
			{
				return base.JZ_OH_Consignee;
			}
			set
			{
				var oldValue = JZ_OH_Consignee;
				base.JZ_OH_Consignee = value;
				if (!IsCopying && oldValue != JZ_OH_Consignee)
				{
					DefaultValueforDutyCodeFromSupplierImporterLink();
					DefaultFinalConsigneeAddress();
					if (CA_IsCasualImport)
					{
						DefaultCasualImportDestinationProvinceIfNeeded();
					}
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		#region Implementation

		void DefaultValueforDutyCodeFromSupplierImporterLink()
		{
			if (Consignee != null && Supplier != null && !IsImportingData && CA_ValueForDutyCode.IsEmpty && SupplierConsigneeLink != null && SupplierConsigneeLink.OL_ValuationBasis.IsValid)
			{
				CA_ValueForDutyCode = SupplierConsigneeLink.OL_ValuationBasis.Left(2);
			}
		}

		void DefaultCountryOfOrigin()
		{
			if (IsAttachedToPersistentDeclaration && JobDeclaration.IsImport && (JZ_RN_NKDefaultOrigin.IsEmpty || !this.IsInDatabase))
			{
				var countryCode = ZString.Empty;
				var province = ZString.Empty;
				if (SupplierPickupDeliveryAddress.E2_AddressOverride || SupplierPickupDeliveryAddress.HasRealAddress)
				{
					countryCode = SupplierPickupDeliveryAddress.E2_RN_NKCountryCode;
					province = SupplierPickupDeliveryAddress.E2_State;
				}
				else
				{
					countryCode = SupplierDocumentaryAddress.E2_RN_NKCountryCode;
					province = SupplierDocumentaryAddress.E2_State;
				}

				if (!countryCode.IsEmpty && countryCode != JZ_RN_NKDefaultOrigin)
				{
					JZ_RN_NKDefaultOrigin = countryCode;
				}
				if (!JZ_RW_NKOriginState_ReadOnly && !province.IsEmpty && province != JZ_RW_NKOriginState)
				{
					JZ_RW_NKOriginState = province.SubstringSafe(0, 2);
				}
			}
		}

		void DefaultCountryOfExport()
		{
			if (IsAttachedToPersistentDeclaration && JobDeclaration.IsImport && (CA_RN_NKExport.IsEmpty || !this.IsInDatabase))
			{
				var docAddress = ExporterDocumentaryAddress;
				if (docAddress.IsEmpty)
				{
					docAddress = SupplierDocumentaryAddress;
				}

				if (!docAddress.IsEmpty)
				{
					var countryCode = docAddress.E2_RN_NKCountryCode;
					var province = docAddress.E2_State;

					if (!countryCode.IsEmpty && countryCode != CA_RN_NKExport)
					{
						CA_RN_NKExport = countryCode;
					}

					if (!CA_USStateOfExport_ReadOnly && !province.IsEmpty && province != CA_USStateOfExport)
					{
						CA_USStateOfExport = province.SubstringSafe(0, 2);
					}
				}
			}
		}

		public OrgSupplierBuyerLink SupplierConsigneeLink
		{
			get
			{
				return GetSupplierConsigneeLink(IsAttachedToPersistentDeclaration ? JobDeclaration.FinalDestinationCountryCode : ZString.Empty)
					?? GetSupplierConsigneeLink(IsAttachedToPersistentDeclaration ? JobDeclaration.BranchCompanyCountryCode : ZString.Empty);
			}
		}

		OrgSupplierBuyerLink GetSupplierConsigneeLink(ZString countryCode)
		{
			return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Supplier, Consignee, countryCode);
		}

		#endregion

		#endregion

		#region JZ_OH_Supplier

		public override ZGuid JZ_OH_Supplier
		{
			get
			{
				return base.JZ_OH_Supplier;
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					var oldValue = JZ_OH_Supplier;
					base.JZ_OH_Supplier = value;
					var newValue = JZ_OH_Supplier;
					if (!IsCopying && oldValue != newValue)
					{
						DefaultValueforDutyCodeFromSupplierImporterLink();
						DefaultSupplierDocumentaryAddress();
						if (PersistentLVXDeclaration is JobDeclaration declaration && declaration.JE_OH_Supplier != newValue)
						{
							declaration.JE_OH_Supplier = newValue;
						}
					}
				}
			}
		}

		#endregion

		#region CA_RN_NKExport

		[ResourceStringData("CAAddInfo|CA_RN_NKExport", Caption = "Country/Region of Export", ShortCaption = "Ctry/Rgn. of Export", MediumCaption = "Export Country/Region", FullDescription = "The country/region from which the goods were exported for importation into Canada.")]
		public override ZString CA_RN_NKExport
		{
			get { return base.CA_RN_NKExport; }
			set
			{
				var oldValue = CA_RN_NKExport;
				base.CA_RN_NKExport = value;
				if (!IsCopying && oldValue != CA_RN_NKExport)
				{
					var declaration = JobDeclaration;
					if (CA_RN_NKExport != Constants.CountryCodes.UnitedStates)
					{
						CA_USStateOfExport = ZString.Empty;
						CA_USPortOfExit = ZString.Empty;
					}
					else if (declaration != null && declaration.JE_MessageType == JobMessageTypeList.Codes.Import && !declaration.IsOtherWarehouseEntry && CA_USPortOfExit.IsEmpty && !declaration.PortOfClearanceRelatedUSPortOfExit.IsEmpty)
					{
						CA_USPortOfExit = declaration.PortOfClearanceRelatedUSPortOfExit;
						CA_USPortOfExitInfo.RefreshBinding();
					}

					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.RefreshSIMAMeasuresCollection();

						if (declaration?.IsLVS ?? false)
						{
							line.AddInfoValidation.ValidateCA_RN_NKExport();
						}
					}

					ResetRemissionValues(oldValue);
				}
			}
		}

		void ResetRemissionValues(string countryOfExport)
		{
			if ((this.IsRemissionAll(countryOfExport) && !this.IsRemissionAll())
				|| (this.IsRemissionMexicoAndUSDutyAndTax(countryOfExport) && !this.IsRemissionMexicoAndUSDutyAndTax())
				|| (this.IsRemissionMexicoAndUSDutyOnly(countryOfExport) && !this.IsRemissionMexicoAndUSDutyOnly()))
			{
				this.ApplyCLVSRemission(LVXJobComInvoiceHeaderExtension.NoRemission, (x) => false);
			}
		}

		#endregion

		#region JZ_RW_NKOriginState

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.StatesOfOrigin))]
		[ReadOnlyMember(nameof(JZ_RW_NKOriginState_ReadOnly))]
		[ResourceStringData("90b40496-8e36-4f46-bc41-95eef0eadea5", Caption = "US State of Origin", ShortCaption = "State", MediumCaption = "Org. State", FullDescription = "The US state or origin when the Country/Region of Origin is US.")]
		[MaxLength(2)]
		public override ZString JZ_RW_NKOriginState
		{
			get { return base.JZ_RW_NKOriginState; }
			set
			{
				var oldValue = JZ_RW_NKOriginState;
				var truncatedValue = value.Left(2);
				base.JZ_RW_NKOriginState = truncatedValue;
				if (!IsCopying && oldValue != JZ_RW_NKOriginState)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();

					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						if (!JZ_RW_NKOriginState.IsEmpty && line.IsRegulatedByIIDCFIA && line.CA_StateOfSource.IsEmpty && !line.CA_StateOfSource_ReadOnly)
						{
							line.CA_StateOfSource = truncatedValue;
						}
						line.JI_StateOrRegionOfOriginInfo.RefreshBinding();
					}
				}
			}
		}

		bool JZ_RW_NKOriginState_ReadOnly
		{
			get { return (IsImportIncludingB2 && JZ_RN_NKDefaultOrigin != Core.Constants.CountryCodes.UnitedStates); }
		}

		#endregion

		#region JZ_RX_NKInvoice_Currency

		[BusinessObjectTestExclude]
		public override ZString JZ_RX_NKInvoice_Currency
		{
			get
			{
				var result = ZString.Empty;
				if (base.JZ_RX_NKInvoice_Currency.IsEmpty && IsAttachedToPersistentExportDeclaration)
				{
					var currency = JobDeclaration.DeclaredCurrency;
					if (currency != null)
					{
						result = currency.RX_Code;
					}
				}
				else
				{
					result = base.JZ_RX_NKInvoice_Currency;
				}

				return result;
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_RX_NKInvoice_Currency))
				{
					base.JZ_RX_NKInvoice_Currency = GetInvoiceCurrencyToSet(value);
					SetExchangeRateIfNotUserEntered();
				}
			}
		}

		internal void ResetInvoiceCurrencyIfNeeded()
		{
			var newValue = GetInvoiceCurrencyToSet(JZ_RX_NKInvoice_Currency);

			if (newValue != JZ_RX_NKInvoice_Currency)
			{
				JZ_RX_NKInvoice_Currency = newValue;
				JZ_RX_NKInvoice_CurrencyInfo.RefreshBinding();
			}
		}

		ZString GetInvoiceCurrencyToSet(ZString invoiceCurr)
		{
			var result = invoiceCurr;
			if (!invoiceCurr.IsEmpty && IsAttachedToPersistentExportDeclaration)
			{
				var currency = JobDeclaration.DeclaredCurrency;
				if (currency != null && currency.RX_Code == invoiceCurr)
				{
					result = ZString.Empty;
				}
			}
			return result;
		}

		protected override bool ShouldRefreshExRatesOnApportionment(ICurrencyProvider currencyProvider, ZString localCurrency)
		{
			return true;
		}

		#endregion

		#region CA_TreatmentCode

		[ResourceStringData("CAAddInfo|CA_TreatmentCode", Caption = "Tariff Treatment Code", ShortCaption = "TT", MediumCaption = "Treatment  Code", FullDescription = "A means by which normal rates of duty may be modified according to the Customs Tariff. Refer to the Customs Tariff for information on the applicability of these tariff treatments.")]
		public override ZString CA_TreatmentCode
		{
			get { return base.CA_TreatmentCode; }
			set
			{
				var oldValue = CA_TreatmentCode;
				base.CA_TreatmentCode = value;
				if (!IsCopying && oldValue != CA_TreatmentCode)
				{
					if ((JobDeclaration?.IsLVS ?? false)
						&& TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(oldValue)
						!= TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(CA_TreatmentCode))
					{
						foreach (JobComInvoiceLine line in JobComInvoiceLines)
						{
							line.AddInfoValidation.ValidateCA_RN_NKExport();
							line.Validation.ValidateJI_CountryOfOrigin();
						}
					}
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.CA_TreatmentCodeInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region CA_USStateOfExport

		bool CA_USStateOfExport_ReadOnly
		{
			get { return IsImportIncludingB2 && CA_RN_NKExport != Core.Constants.CountryCodes.UnitedStates; }
		}

		#endregion

		#region CA_StateOfSource

		bool CA_StateOfSource_ReadOnly
		{
			get { return IsImportIncludingB2 && CA_RN_NKSource != Core.Constants.CountryCodes.UnitedStates; }
		}

		public override ZString CA_RN_NKSource
		{
			get => base.CA_RN_NKSource;
			set
			{
				var oldValue = CA_RN_NKSource;
				base.CA_RN_NKSource = value;
				if (!IsCopying && oldValue != CA_RN_NKSource)
				{
					if (!CA_StateOfSource.IsEmpty && CA_StateOfSource_ReadOnly)
					{
						CA_StateOfSource = ZString.Empty;
					}
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.RefreshBindingForDeclaredPGAHeaders();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(CA_StateOfSource_ReadOnly))]
		public override ZString CA_StateOfSource
		{
			get => base.CA_StateOfSource;
			set
			{
				var oldValue = CA_StateOfSource;
				base.CA_StateOfSource = value;
				if (!IsCopying && oldValue != CA_StateOfSource)
				{
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.RefreshBindingForDeclaredPGAHeaders();
					}
				}
			}
		}

		#endregion

		#region ApportionmentDirtyChangedEventHandler

		protected override event EventHandler ApportionmentDirtyChangedEventHandler
		{
			add
			{
				base.ApportionmentDirtyChangedEventHandler += value;
				CA_TreatmentCodeInfo.ValueChanged += value;
				CA_TimeLimitInfo.ValueChanged += value;
				CA_TimeLimitCodeInfo.ValueChanged += value;
				JZ_RN_NKDefaultOriginInfo.ValueChanged += value;
				JZ_RW_NKOriginStateInfo.ValueChanged += value;
				CA_RN_NKExportInfo.ValueChanged += value;
				CA_USStateOfExportInfo.ValueChanged += value;
				CA_CasualImportDestinationProvinceInfo.ValueChanged += value;
				CA_CasualImportCommodityInfo.ValueChanged += value;
				CA_IsCasualImportInfo.ValueChanged += value;
				CA_PortOfClearanceInfo.ValueChanged += value;
			}
			remove { base.ApportionmentDirtyChangedEventHandler -= value; }
		}

		#endregion

		#region CA_PortOfClearance

		[ResourceStringData("CAAddInfo|CA_PortOfClearance", Caption = "Customs Port of Clearance", ShortCaption = "Port", FullDescription = "Port of Clearance")]
		public override ZString CA_PortOfClearance
		{
			get { return base.CA_PortOfClearance; }
			set
			{
				base.CA_PortOfClearance = value.IsEmpty ? value : value.PadLeft(4, '0');
				if (PersistentLVXDeclaration is JobDeclaration declaration)
				{
					var newValue = CA_PortOfClearance;
					if (declaration.JE_CustomsOffice != newValue)
					{
						declaration.JE_CustomsOffice = newValue;
					}
					var newProvinceOfClearance = newValue.IsEmpty ? ZString.Empty : EffectiveImportClearanceProvince;
					if (declaration.CA_ProvinceOfClearance != newProvinceOfClearance)
					{
						declaration.CA_ProvinceOfClearance = newProvinceOfClearance;
					}
				}
			}
		}

		public ZString EffectiveImportClearanceProvince
		{
			get
			{
				var portOfClearance = CA_PortOfClearance.IsEmpty && JobDeclaration is JobDeclaration declaration ? declaration.JE_CustomsOffice : CA_PortOfClearance;
				return Factory.GetCachedValue("EffectiveImportClearanceProvince" + "|" + Core.Constants.CountryCodes.Canada + "|" + portOfClearance, () =>
				{
					var cbsaOfficeCode = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, portOfClearance, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					return cbsaOfficeCode?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province) ?? ZString.Empty;
				});
			}
		}

		#endregion

		#region CA_RL_NKLastPort

		public virtual RefUNLOCO LastPort
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CA_RL_NKLastPort); }
		}

		#endregion

		#region CA_TimeLimit

		[MaxLength(2)]
		[ResourceStringData("CusSCAHouse|CA_TimeLimit", Caption = "Time Limit", ShortCaption = "Limit")]
		public override ZInt CA_TimeLimit
		{
			get { return base.CA_TimeLimit; }
			set { base.CA_TimeLimit = value; }
		}

		public (ZDateTime TimeLimitStart, ZDateTime TimeLimitEnd) TimeLimitRange
		{
			get
			{
				var timeLimitStart = ZDateTime.Empty;
				var timeLimitEnd = ZDateTime.Empty;
				var declaration = JobDeclaration;
				timeLimitStart = declaration.JE_EntryAuthorisationDate.IsEmpty ? declaration.JE_DateOfFirstArrival : declaration.JE_EntryAuthorisationDate;
				if (!timeLimitStart.IsEmpty)
				{
					var timeLimit = CA_TimeLimit;
					var timeLimitCode = CA_TimeLimitCode;
					if (!timeLimit.IsEmpty && !timeLimitCode.IsEmpty)
					{
						var timeLimitCodeStr = timeLimitCode.ToString();
						timeLimitEnd = timeLimitCodeStr switch
						{
							TimeLimitUnitCodes.Codes.Day => timeLimitStart.AddDays(timeLimit),
							TimeLimitUnitCodes.Codes.Week => timeLimitStart.AddDays(timeLimit * 7),
							TimeLimitUnitCodes.Codes.Month => timeLimitStart.AddMonths(timeLimit),
							TimeLimitUnitCodes.Codes.Year => timeLimitStart.AddYears(timeLimit),
							_ => ZDateTime.Empty,
						};
					}
				}
				return (timeLimitStart, timeLimitEnd);
			}
		}

		#endregion

		#region JZ_NetWeight

		public override ZDecimal JZ_NetWeight
		{
			get { return (this.IsAttachedToPersistentDeclaration && JobDeclaration.IsExport) ? 0 : base.JZ_NetWeight; }
			set
			{
				if (JZ_NetWeightUQ.IsEmpty)
				{
					JZ_NetWeightUQ = UnitOfWeightList.Codes.Kilogram;
				}

				base.JZ_NetWeight = value;
			}
		}

		#endregion

		#region CA_CasualImportDestinationProvince

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.CanadianProvinces))]
		[ReadOnlyMember(nameof(CA_IsCasualImportValues_ReadOnly))]
		public override ZString CA_CasualImportDestinationProvince
		{
			get { return base.CA_CasualImportDestinationProvince; }
			set { base.CA_CasualImportDestinationProvince = value; }
		}

		#endregion

		#region CA_CasualImportCommodity

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.CasualImportCommodity))]
		[ReadOnlyMember(nameof(CA_IsCasualImportValues_ReadOnly))]
		public override ZString CA_CasualImportCommodity
		{
			get { return base.CA_CasualImportCommodity; }
			set { base.CA_CasualImportCommodity = value; }
		}

		#endregion

		#region CA_IsCasualImport

		public override ZBool CA_IsCasualImport
		{
			get { return base.CA_IsCasualImport; }
			set
			{
				var oldValue = CA_IsCasualImport;
				base.CA_IsCasualImport = value;
				if (!IsCopying && oldValue != CA_IsCasualImport)
				{
					if (!value)
					{
						CA_CasualImportCommodity = ZString.Empty;
						CA_CasualImportDestinationProvince = ZString.Empty;
					}
					else
					{
						DefaultCasualImportDestinationProvinceIfNeeded();
						foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
						{
							invoiceLine.DefaultCasualImportDestinationProvinceIfNotPreviouslyDefaulted();
						}
					}
				}
			}
		}

		public ZString DefaultCasualImportDestinationProvince
		{
			get
			{
				if (defaultCasualImportDestinationProvince == null)
				{
					defaultCasualImportDestinationProvince = new CachedProperty<ZString>(Factory, () =>
					{
						var destProvince = ZString.Empty;
						var consigneeAddressState = GetCanadianState(FinalConsigneeAddress);
						if (!consigneeAddressState.IsEmpty)
						{
							destProvince = consigneeAddressState;
						}
						else if (JobDeclaration is JobDeclaration dec)
						{
							var deliveryAddressState = GetCanadianState(dec.ImporterDeliveryAddress);
							if (!deliveryAddressState.IsEmpty)
							{
								destProvince = deliveryAddressState;
							}
							else
							{
								var importerAddressState = GetCanadianState(dec?.Importer?.MainAddress);
								if (!importerAddressState.IsEmpty)
								{
									destProvince = importerAddressState;
								}
							}
						}
						return destProvince;
					});
				}
				return defaultCasualImportDestinationProvince.Value;
			}
		}
		CachedProperty<ZString> defaultCasualImportDestinationProvince;

		void DefaultCasualImportDestinationProvinceIfNeeded()
		{
			var destProvince = DefaultCasualImportDestinationProvince;
			if (!destProvince.IsEmpty)
			{
				CA_CasualImportDestinationProvince = destProvince;
			}
		}

		ZString GetCanadianState(JobDocAddress docAddress)
		{
			return GetCanadianState(docAddress != null && !docAddress.E2_AddressOverride && docAddress.E2_OA_Address.IsValid ? docAddress.Address : null);
		}

		ZString GetCanadianState(OrgAddress address)
		{
			return address != null && address.OA_RN_NKCountryCode == Constants.CountryCodes.Canada ? address.OA_State.Left(2) : ZString.Empty;
		}

		#endregion

		#region CA_LVSCarrier

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.CarrierCodes))]
		[ResourceStringData("JobComInvoiceHeader|CA_LVSCarrier", Caption = "Carrier Code", ShortCaption = "Carrier")]
		public virtual ZString CA_LVSCarrier
		{
			get { return IsAttachedToPersistentLVSDeclaration ? CA_CarrierCode : ZString.Empty; }
			set { CA_CarrierCode = value; }
		}

		public virtual ZPropertyInfo CA_LVSCarrierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CA_CarrierCode, x => CA_CarrierCodeInfo); }
		}

		#endregion

		#region Cargo Control Numbers List

		[ChildEditable(true)]
		public JobComInvoiceHeaderCCNsCollection CargoControlNumbersList
		{
			get
			{
				if (cargoControlNumbers == null)
				{
					cargoControlNumbers = new JobComInvoiceHeaderCCNsCollection(this);
					RegisterEditableChildObject(cargoControlNumbers);
				}
				return cargoControlNumbers;
			}
		}
		JobComInvoiceHeaderCCNsCollection cargoControlNumbers;

		#endregion

		public ZBool IsExistingManualDummyCasualImportLine
		{
			get
			{
				if (IsAttachedToPersistentDeclaration)
				{
					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						if (!line.CA_IsAutoDummyHSCodeCasualImportLine)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return JobDeclaration.LocalCurrencyConstantCode; }
		}

		[ResourceStringData("CAAddInfo|CA_OtherReference", Caption = "Other Reference", ShortCaption = "Ref.", MediumCaption = "Reference", FullDescription = "Other Reference to be used for other useful information, comments, etc.")]
		public override ZString CA_OtherReference
		{
			get { return base.CA_OtherReference; }
			set { base.CA_OtherReference = value; }
		}

		[ReadOnlyMember(nameof(CA_USStateOfExport_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_USStateOfExport", Caption = "US State of Export", ShortCaption = "State", MediumCaption = "Exp. State", FullDescription = "The US state code if the Country/Region of Export is US.")]
		public override ZString CA_USStateOfExport
		{
			get { return base.CA_USStateOfExport; }
			set { base.CA_USStateOfExport = value; }
		}

		[ResourceStringData("CusSCAHouse|CA_TimeLimitCode", Caption = "Time Limit Unit", ShortCaption = "TU", MediumCaption = "Time Unit")]
		public override ZString CA_TimeLimitCode
		{
			get { return base.CA_TimeLimitCode; }
			set { base.CA_TimeLimitCode = value; }
		}

		[ResourceStringData("CAAddInfo|CA_ValueForDutyCode", Caption = "Value For Duty Code", ShortCaption = "VFD", MediumCaption = "VFD Code", FullDescription = "A code used to indicate the basis on which the value for duty was determined. The first digit is the relationship, while the second digit is the valuation method used.")]
		public override ZString CA_ValueForDutyCode
		{
			get { return base.CA_ValueForDutyCode; }
			set
			{
				var oldValue = CA_ValueForDutyCode;
				base.CA_ValueForDutyCode = value;
				if (!IsCopying && oldValue != CA_ValueForDutyCode)
				{
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.CA_ValueForDutyCodeInfo.RefreshBinding();
					}
				}
			}
		}

		[ResourceStringData("CusSCAHouse|CA_TradeZone", Caption = "US Trade Zone", ShortCaption = "Zone", MediumCaption = "Trade Zone", FullDescription = "US foreign trade zone.")]
		public override ZString CA_TradeZone
		{
			get { return base.CA_TradeZone; }
			set { base.CA_TradeZone = value; }
		}

		[ReadOnlyMember(nameof(CA_USPortOfExit_ReadOnly))]
		[ResourceStringData("CusSCAHouse|CA_USPortOfExit", Caption = "US Port of Exit", ShortCaption = "US Port", FullDescription = "US port of exit is defined as the \"US Customs port at which or nearest to which the land surface carrier transporting the merchandise crosses the border of the United States into Canada, or in the case of exportation by vessel or air, the US Customs port where the merchandise is loaded on the vessel or aircraft which is to carry the merchandise to Canada.\"")]
		public override ZString CA_USPortOfExit
		{
			get { return base.CA_USPortOfExit; }
			set { base.CA_USPortOfExit = value; }
		}

		bool CA_USPortOfExit_ReadOnly
		{
			get { return IsImport && !IsUSorTerritory; }
		}

		[ResourceStringData("CAAddInfo|CA_DDPDeductDutyOnly", Caption = "Deduct Duty Only", FullDescription = "Deduct Duty only and not GST when calculating with DDP terms")]
		public override ZBool CA_DDPDeductDutyOnly
		{
			get => base.CA_DDPDeductDutyOnly;
			set
			{
				var oldValue = CA_DDPDeductDutyOnly;
				base.CA_DDPDeductDutyOnly = value;
				if (!IsCopying && oldValue != CA_DDPDeductDutyOnly)
				{
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.DutyAndTaxManager.PopulateDutiesAndTaxes();
					}
				}
			}
		}

		public override bool SupportAdditionalDeclarations
		{
			get { return IsAttachedToPersistentLVXDeclaration; }
		}

		[ReadOnlyMember(nameof(CA_ReadyForConsolidation_ReadOnly))]
		public override ZBool CA_ReadyForConsolidation
		{
			get => base.CA_ReadyForConsolidation;
			set
			{
				var hasChanged = base.CA_ReadyForConsolidation != value;
				base.CA_ReadyForConsolidation = value;
				if (!IsCopying && hasChanged)
				{
					Factory.Saved -= DoCreditCheckAfterSaved;
					if (value)
					{
						Factory.Saved += DoCreditCheckAfterSaved;
					}
				}
			}
		}

		void DoCreditCheckAfterSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully && !IsDeleted)
			{
				factory.Saved -= DoCreditCheckAfterSaved;
				var declaration = JobDeclaration;
				var checker = Buyer?.CreditChecker;
				if (CA_ReadyForConsolidation && declaration != null && declaration.IsLVX && checker != null)
				{
					if (checker.IsCreditOnHold() || checker.IsOverGlobalCreditLimit())
					{
						Logs.CreateRecreateOrUpdateEventLog(AutoEvents.CreditCheckFailed, EstimateActual.Actual, ZDateTimeOffset.Now, "Courier LVS Declaration failed credit check.");
					}
					else
					{
						var oldLog = Logs.MostRecentLogByEventTime(AutoEvents.CreditCheckFailed);
						if (oldLog != null)
						{
							oldLog.Cancel();
						}
					}
					try
					{
						factory.Save();
					}
					catch (ZException e)
					{
						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}
		}

		ZBool CA_ReadyForConsolidation_ReadOnly => FirstAdditionalDeclaration != null || (JobDeclaration is JobDeclaration declaration && declaration.LVXInvoiceHeader.JZ_OH_Buyer.IsEmpty);

		#region CA_IsSeeded

		public override ZBool CA_IsSeeded
		{
			get { return base.CA_IsSeeded; }
			set
			{
				base.CA_IsSeeded = value;
				if (IsB2AsAccountForSeededHeader)
				{
					this.ReadOnly = true;
				}
			}
		}

		#endregion

		#endregion

		#region New CommercialInvoiceOriginator

		public JobDocAddress CommercialInvoiceOriginator
		{
			get
			{
				if (fCommercialInvoiceOriginator == null || fCommercialInvoiceOriginator.IsDeleted)
				{
					fCommercialInvoiceOriginator =
						DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CommercialInvoiceOriginator);
				}
				return fCommercialInvoiceOriginator;
			}
		}
		JobDocAddress fCommercialInvoiceOriginator;

		#endregion

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
						fSupplierDocumentaryAddress.DefaultAddressType = ZArchitecture.Business.AddressType.NoDefault;
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
			MarkAsNeedingValidation();
			var newDocAddress = SupplierDocumentaryAddress;
			if (JZ_OH_Supplier != newDocAddress.OrganisationPK)
			{
				JZ_OH_Supplier = newDocAddress.OrganisationPK;
			}
			DefaultCountryOfOrigin();
			DefaultCountryOfExport();
		}

		void DefaultSupplierDocumentaryAddress()
		{
			if (!IsImportingData && !SupplierDocumentaryAddress.E2_AddressOverride)
			{
				if ((IsAttachedToPersistentDeclaration && JobDeclaration.IsImport) || !IsAttachedToPersistentDeclaration)
				{
					SupplierDocumentaryAddress.OrganisationPK = JZ_OH_Supplier;
				}
			}
		}

		#endregion

		#region SupplierPickupDeliveryAddress

		public JobDocAddress SupplierPickupDeliveryAddress
		{
			get
			{
				if (fSupplierPickupDeliveryAddress == null || fSupplierPickupDeliveryAddress.IsDeleted)
				{
					fSupplierPickupDeliveryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierPickupDeliveryAddress);
					if (fSupplierPickupDeliveryAddress != null)
					{
						fSupplierPickupDeliveryAddress.DefaultAddressType = ZArchitecture.Business.AddressType.NoDefault;
						fSupplierPickupDeliveryAddress.DocAddressChanged += fSupplierPickupDeliveryAddress_DocAddressChanged;
					}
				}
				return fSupplierPickupDeliveryAddress;
			}
		}
		JobDocAddress fSupplierPickupDeliveryAddress;

		void fSupplierPickupDeliveryAddress_DocAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			DefaultCountryOfOrigin();
		}

		#endregion

		#region FinalConsigneeAddress

		public JobDocAddress FinalConsigneeAddress
		{
			get
			{
				if (fFinalConsigneeAddress == null || fFinalConsigneeAddress.IsDeleted)
				{
					fFinalConsigneeAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FinalConsigneeAddress);
					if (fFinalConsigneeAddress != null)
					{
						fFinalConsigneeAddress.DocAddressChanged += fFinalConsigneeAddress_DocAddressChanged;
						fFinalConsigneeAddress.E2_OA_AddressInfo.ValueChanged += FinalConsigneeAddress_E2_OA_AddressInfo_ValueChanged;
						if (!fFinalConsigneeAddress.IsInDatabase && fFinalConsigneeAddress.OrganisationPK.IsEmpty)
						{
							fFinalConsigneeAddress.OrganisationPK = JZ_OH_Consignee;
						}
					}
				}
				return fFinalConsigneeAddress;
			}
		}
		JobDocAddress fFinalConsigneeAddress;

		void FinalConsigneeAddress_E2_OA_AddressInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs ve)
			{
				var oldValue = (ZGuid)ve.OldValue;
				var newValue = (ZGuid)ve.NewValue;
				if (oldValue != newValue && FinalConsigneeAddress is JobDocAddress docAddress &&
					!docAddress.E2_AddressOverride && IsImport)
				{
					foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
					{
						invoiceLine.DefaultJI_OA_ConsigneeAddress(oldValue, newValue);
					}
				}
			}
		}

		void fFinalConsigneeAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (IsAttachedToPersistentDeclaration)
			{
				JobDeclaration.MarkAsNeedingValidation();
			}

			var newDocAddress = FinalConsigneeAddress;
			if (JZ_OH_Consignee != newDocAddress.OrganisationPK)
			{
				JZ_OH_Consignee = newDocAddress.OrganisationPK;
			}
		}

		void DefaultFinalConsigneeAddress()
		{
			if (IsAttachedToPersistentDeclaration && JobDeclaration.IsImport && !IsImportingData && !FinalConsigneeAddress.E2_AddressOverride)
			{
				FinalConsigneeAddress.OrganisationPK = JZ_OH_Consignee;
			}
		}

		#endregion

		#region BuyerDocumentaryAddress

		public JobDocAddress BuyerDocumentaryAddress
		{
			get
			{
				if (fBuyerDocumentaryAddress == null || fBuyerDocumentaryAddress.IsDeleted)
				{
					fBuyerDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BuyerDocumentaryAddress);
					if (fBuyerDocumentaryAddress != null)
					{
						fBuyerDocumentaryAddress.DocAddressChanged += fBuyerDocumentaryAddress_DocAddressChanged;
						if (!fBuyerDocumentaryAddress.IsInDatabase)
						{
							fBuyerDocumentaryAddress.OrganisationPK = JZ_OH_Buyer;
						}
					}
				}
				return fBuyerDocumentaryAddress;
			}
		}
		JobDocAddress fBuyerDocumentaryAddress;

		void fBuyerDocumentaryAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (IsAttachedToPersistentDeclaration)
			{
				JobDeclaration.MarkAsNeedingValidation();
			}
			var newDocAddress = BuyerDocumentaryAddress;
			if (JZ_OH_Buyer != newDocAddress.OrganisationPK)
			{
				JZ_OH_Buyer = newDocAddress.OrganisationPK;
			}
		}

		void DefaultBuyerDocumentaryAddress()
		{
			if (!IsImportingData && !BuyerDocumentaryAddress.E2_AddressOverride)
			{
				if (!IsAttachedToPersistentDeclaration || JobDeclaration.IsImport)
				{
					BuyerDocumentaryAddress.OrganisationPK = JZ_OH_Buyer;
				}
			}
		}

		#endregion

		#region ExporterDocumentaryAddress

		public JobDocAddress ExporterDocumentaryAddress
		{
			get
			{
				if (fExporterDocumentaryAddress == null || fExporterDocumentaryAddress.IsDeleted)
				{
					fExporterDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Exporter);
					if (fExporterDocumentaryAddress != null)
					{
						fExporterDocumentaryAddress.DocAddressChanged += fExporterDocumentaryAddress_DocAddressChanged;
					}
				}
				return fExporterDocumentaryAddress;
			}
		}
		JobDocAddress fExporterDocumentaryAddress;

		void fExporterDocumentaryAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (IsAttachedToPersistentDeclaration)
			{
				JobDeclaration.MarkAsNeedingValidation();
			}
			DefaultCountryOfExport();
		}

		#endregion

		#region JZ_OA_ManufacturerAddress

		public override ZGuid JZ_OA_ManufacturerAddress
		{
			get { return base.JZ_OA_ManufacturerAddress; }
			set
			{
				var oldValue = JZ_OA_ManufacturerAddress;
				base.JZ_OA_ManufacturerAddress = value;
				if (!IsCopying && oldValue != JZ_OA_ManufacturerAddress)
				{
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						line.JI_OA_ManufacturerAddressInfo.RefreshBinding();
						line.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override ZAddress GetNewJZ_OA_ManufacturerAddress_ZAddress()
		{
			var address = base.GetNewJZ_OA_ManufacturerAddress_ZAddress();
			address.IsOrgVisible = true;
			address.GetDefaultAddress = (header) => header?.MainAddress?.PK ?? ZGuid.Empty;
			return address;
		}

		#endregion

		#region PiggyBackedDocAddressValidation

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			switch (JZ_MessageType)
			{
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.LowValueShipments:
				case JobMessageTypeList.Codes.LVSForConsolidation:
					return new ImportJobComInvoiceHeaderJobDocAddressValidation(addressToValidate, this);
				default:
					return null;
			}
		}

		#region SupportedAddressTypes

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			return new DocAddressType[]
				{
					DocAddressType.OriginatingConsignorAddress,
					DocAddressType.SupplierDocumentaryAddress,
					DocAddressType.SupplierPickupDeliveryAddress,
					DocAddressType.CommercialInvoiceOriginator,
					DocAddressType.FinalConsigneeAddress,
					DocAddressType.BuyerDocumentaryAddress,
					DocAddressType.Exporter
				};
		}

		#endregion

		#endregion

		#region New Properties

		#region PlaceOfExport

		public ZString PlaceOfExport
		{
			get { return this.CA_TradeZone.IsEmpty ? this.CommonCountryOfExport : this.CA_TradeZone; }
		}

		#endregion

		#region CountryOfOrigin

		public ZString CountryOfOrigin
		{
			get { return JZ_RN_NKDefaultOrigin == Constants.CountryCodes.UnitedStates ? new ZString("U" + JZ_RW_NKOriginState) : JZ_RN_NKDefaultOrigin; }
		}

		#endregion

		public ZDecimal TotalValueForDuty
		{
			get
			{
				if (totalValueForDuty == null)
				{
					totalValueForDuty = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return JobComInvoiceLines.Sum(l => ((JobComInvoiceLine)l).CA_CustomsValue);
					});
				}
				return totalValueForDuty.Value;
			}
		}
		CachedProperty<ZDecimal> totalValueForDuty;

		public ZPropertyInfo TotalValueForDutyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalValueForDuty); }
		}

		public ZShort GoodsShipmentSequence
		{
			get
			{
				if (goodsShipmentSequence == null)
				{
					goodsShipmentSequence = new CachedProperty<ZShort>(Factory, delegate
					{
						return JobComInvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault()?.GoodsShipmentSequence ?? ZShort.Zero;
					});
				}
				return goodsShipmentSequence.Value;
			}
		}
		CachedProperty<ZShort> goodsShipmentSequence;

		public ZPropertyInfo GoodsShipmentSequenceInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsShipmentSequence); }
		}

		public bool IsUSorTerritory
		{
			get
			{
				return CA_RN_NKExport == Constants.CountryCodes.UnitedStates
					|| CA_RN_NKExport == Constants.CountryCodes.PuertoRico
					|| CA_RN_NKExport == Core.Constants.CountryCodes.UnitedStatesMinorIslands
					|| CA_RN_NKExport == Core.Constants.CountryCodes.VirginIslands
					|| !CA_TradeZone.IsEmpty;
			}
		}

		public bool IsUSCountryOfExport
		{
			get
			{
				return IsUSorTerritory && JobDeclaration is JobDeclaration declaration && declaration.TotalCustomsValueInLocalCurrency >= VFDLimit;
			}
		}

		public bool IsUSPlaceOfExport
		{
			get
			{
				var vFD = JobDeclaration?.TotalCustomsValueInLocalCurrency ?? ZDecimal.Zero;
				var placeOfExport = PlaceOfExport;
				return vFD > VFDLimit && (placeOfExport.Length > 2
					|| placeOfExport == Constants.CountryCodes.PuertoRico
					|| placeOfExport == Constants.CountryCodes.UnitedStatesMinorIslands
					|| placeOfExport == Constants.CountryCodes.VirginIslands);
			}
		}

		public const decimal VFDLimit = 2500m;

		public VendorStateAndZipStruct VendorStateAndZip
		{
			get
			{
				var state = ZString.Empty;
				var zip = ZString.Empty;
				var vendor = Vendor;
				if (vendor != null)
				{
					switch (vendor.CountryCode)
					{
						case Constants.CountryCodes.UnitedStates:
							if (!vendor.E2_State.IsEmpty)
							{
								state = "U" + vendor.E2_State.TrimStart();
							}

							zip = vendor.E2_Postcode;
							break;
						case Constants.CountryCodes.PuertoRico:
						case Constants.CountryCodes.UnitedStatesMinorIslands:
						case Constants.CountryCodes.VirginIslands:
							state = IsUSPlaceOfExport ? vendor.CountryCode : ZString.Empty;
							break;
						default:
							var exporterStateAndZip = ExporterStateAndZip;
							state = exporterStateAndZip.State;
							zip = exporterStateAndZip.Zip;
							break;
					}
				}
				return new VendorStateAndZipStruct(state, zip);
			}
		}

		VendorStateAndZipStruct ExporterStateAndZip
		{
			get
			{
				var state = ZString.Empty;
				var zip = ZString.Empty;
				if (IsUSPlaceOfExport)
				{
					var exporter = Exporter;
					if (exporter != null)
					{
						switch (exporter.CountryCode)
						{
							case Constants.CountryCodes.UnitedStates:
								if (!exporter.E2_State.IsEmpty)
								{
									state = "U" + exporter.E2_State.TrimStart();
								}

								zip = exporter.E2_Postcode;
								break;
							case Constants.CountryCodes.PuertoRico:
							case Constants.CountryCodes.UnitedStatesMinorIslands:
							case Constants.CountryCodes.VirginIslands:
								state = exporter.CountryCode;
								break;
						}
					}
				}
				return new VendorStateAndZipStruct(state, zip);
			}
		}

		ZBool CA_IsCasualImportValues_ReadOnly
		{
			get { return !CA_IsCasualImport; }
		}

		#endregion

		#region B2 Adjustments

		public bool IsB2AsAccountForSeededHeader => IsB2AsAccountedForInvoiceHeader && this.CA_IsSeeded;

		public bool IsB2AsAccountedForInvoiceHeader
		{
			get
			{
				var declaration = JobDeclaration;
				var groupHeader = GroupHeader;

				return declaration != null
					&& (declaration.IsB2Adjustments || declaration.IsB3X)
					&& groupHeader != null
					&& groupHeader.JZ_InvoiceNumber == JobComInvoiceGroupHeader.AllInvoices;
			}
		}

		public bool IsB2AsClaimedForSeededHeader => IsB2AsClaimedForInvoiceHeader && CA_IsSeeded;

		bool IsB2AsClaimedForInvoiceHeader
		{
			get
			{
				var declaration = JobDeclaration;
				var groupHeader = GroupHeader;

				return declaration != null
					&& (declaration.IsB2Adjustments || declaration.IsB3X)
					&& groupHeader != null
					&& groupHeader.JZ_InvoiceNumber == JobComInvoiceGroupHeader.AsClaimed;
			}
		}

		bool IsImportIncludingB2
		{
			get { return JobDeclaration != null && JobDeclaration.IsImportIncludingB2; }
		}

		public JobComInvoiceHeader CorrespondingAsAccountedForInvoice
		{
			get
			{
				JobComInvoiceHeader invoice = null;
				if (IsB2AsClaimedForSeededHeader && JobDeclaration is JobDeclaration declaration)
				{
					invoice = declaration.B2AsAccountedForInvoices.Cast<JobComInvoiceHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == this.JZ_InvoiceNumber);
				}
				return invoice;
			}
		}

		public JobComInvoiceHeader CorrespondingAsClaimedForInvoice
		{
			get
			{
				JobComInvoiceHeader invoice = null;
				if (IsB2AsAccountedForInvoiceHeader && JobDeclaration is JobDeclaration declaration)
				{
					invoice = declaration.B2AsClaimedForInvoices.Cast<JobComInvoiceHeader>().FirstOrDefault(x => x.JZ_InvoiceNumber == this.JZ_InvoiceNumber && x.CA_IsSeeded);
				}
				return invoice;
			}
		}

		[ChildEditable(true)]
		public B2JobComInvoiceLineViewCollection AsAccountForFilteredInvoiceLines
		{
			get
			{
				if (fAsAccountForFilteredInvoiceLines == null)
				{
					if (JobDeclaration is JobDeclaration declaration)
					{
						fAsAccountForFilteredInvoiceLines = new B2JobComInvoiceLineViewCollection(this, declaration.InvoiceLines, true);
						RegisterEditableChildObject(fAsAccountForFilteredInvoiceLines);

						foreach (var line in fAsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>())
						{
							line.EnableSynchroniser();
						}
					}
				}
				return fAsAccountForFilteredInvoiceLines;
			}
		}
		B2JobComInvoiceLineViewCollection fAsAccountForFilteredInvoiceLines;

		[ChildEditable(true)]
		public B2JobComInvoiceLineViewCollection AsClaimForFilteredInvoiceLines
		{
			get
			{
				if (fAsClaimForFilteredInvoiceLines == null)
				{
					if (JobDeclaration is JobDeclaration declaration)
					{
						fAsClaimForFilteredInvoiceLines = new B2JobComInvoiceLineViewCollection(this, declaration.InvoiceLines, false);
						RegisterEditableChildObject(fAsClaimForFilteredInvoiceLines);
					}
				}
				return fAsClaimForFilteredInvoiceLines;
			}
		}
		B2JobComInvoiceLineViewCollection fAsClaimForFilteredInvoiceLines;

		public JobComInvoiceLine CreateB2AsAccountForLineIfDoesNotExist(IClassificationLine1 classificationLine)
		{
			var isExist = (from JobComInvoiceLine line in this.AsAccountForFilteredInvoiceLines
						   where line.CA_OriginalLineNo == classificationLine.B3LineNumber.ToString() && line.CA_IsAccountForLine
						   select line).FirstOrDefault() != null;
			if (!isExist)
			{
				var asAccountForLine = this.AsAccountForFilteredInvoiceLines.AddNew();
				asAccountForLine.CA_IsSeeded = true;
				asAccountForLine.CopyB3SubHeaderLineToInvoiceLine(classificationLine);
				return asAccountForLine;
			}

			return null;
		}

		public void GetDetailsFrom(JobComInvoiceHeader asAccountedInvoice)
		{
			this.JZ_InvoiceNumber = asAccountedInvoice.JZ_InvoiceNumber;
			this.JZ_RN_NKDefaultOrigin = asAccountedInvoice.JZ_RN_NKDefaultOrigin;
			this.JZ_RW_NKOriginState = asAccountedInvoice.JZ_RW_NKOriginState;
			this.CA_RN_NKExport = asAccountedInvoice.CA_RN_NKExport;
			this.CA_USStateOfExport = asAccountedInvoice.CA_USStateOfExport;
			this.CA_TreatmentCode = asAccountedInvoice.CA_TreatmentCode;
			this.JZ_ValuationDateOverride = asAccountedInvoice.JZ_ValuationDateOverride;
			this.JZ_RX_NKInvoice_Currency = asAccountedInvoice.JZ_RX_NKInvoice_Currency;
			this.CA_TimeLimit = asAccountedInvoice.CA_TimeLimit;
			this.CA_TimeLimitCode = asAccountedInvoice.CA_TimeLimitCode;
			this.CA_TradeZone = asAccountedInvoice.CA_TradeZone;
		}

		public void EnableAndSynchronise(bool forceSync = false)
		{
			if (ShouldSynchronise && !B2AsClaimedInvoiceSynchroniser.IsEnabled)
			{
				using (GetValidationSuspender())
				{
					B2AsClaimedInvoiceSynchroniser.SetEnabled(ShouldSynchronise, B2AsClaimedInvoiceSynchroniser.DetectEnabled);
					if (forceSync)
					{
						B2AsClaimedInvoiceSynchroniser.Synchronise(forceSync);
					}
					else
					{
						B2AsClaimedInvoiceSynchroniser.Synchronise();
					}
				}
			}
		}

		public void EnableSynchroniser()
		{
			if (ShouldSynchronise)
			{
				B2AsClaimedInvoiceSynchroniser.SetEnabled(ShouldSynchronise, B2AsClaimedInvoiceSynchroniser.DetectEnabled);
			}
		}

		internal B2JobComInvoiceHeaderSynchroniser B2AsClaimedInvoiceSynchroniser
		{
			get
			{
				if (fB2AsClaimedInvoiceSynchroniser == null)
				{
					var destination = JobDeclaration.CreateOrGetB2AsClaimedInvoice(this);
					fB2AsClaimedInvoiceSynchroniser = new B2JobComInvoiceHeaderSynchroniser(destination, this);
				}
				return fB2AsClaimedInvoiceSynchroniser;
			}
		}
		B2JobComInvoiceHeaderSynchroniser fB2AsClaimedInvoiceSynchroniser;

		public void ReEnableAndSynchronise()
		{
			if (fB2AsClaimedInvoiceSynchroniser != null)
			{
				fB2AsClaimedInvoiceSynchroniser.Dispose();
				fB2AsClaimedInvoiceSynchroniser = null;
			}

			EnableAndSynchronise(true);
		}

		internal bool ShouldSynchronise => !JZ_InvoiceNumber.IsEmpty && IsB2AsAccountedForInvoiceHeader;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
		{
			public Strategy(JobComInvoiceHeader invoice)
				: base(invoice)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				ZQuery query = new ZQuery(JobComInvHeaderChargeSchema.J7_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(typeof(Common.JobComInvCharge), query);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			}

			protected new JobComInvoiceHeader BusinessObject
			{
				get { return (JobComInvoiceHeader)base.BusinessObject; }
			}

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(JobComInvoiceHeaderRefsSchema.J2_JZ, BusinessObject.PK);
			}
		}

		#endregion

		#region Multiple Additional Declarations

		public new JobDeclaration FirstAdditionalDeclaration
		{
			get
			{
				return (JobDeclaration)base.FirstAdditionalDeclaration;
			}
		}

		internal JobDeclaration FirstAdditionalOrOnlyDeclaration
		{
			get
			{
				return FirstAdditionalDeclaration ?? JobDeclaration;
			}
		}

		#endregion

		#region Accounting Total Invoiced/Billed Amounts

		public ZDecimal TotalInvoicedAmount
		{
			get { return Accounting_ARInvoiceQueryResultForAllBilled.TotalBilledAmount; }
		}

		public ZDecimal TotalBilledAmount
		{
			get { return Accounting_ARInvoiceQueryResultForDisbusementsBilled.TotalBilledAmount; }
		}

		AP_ARInvoiceQueryResult Accounting_ARInvoiceQueryResultForDisbusementsBilled
		{
			get
			{
				if (!accounting_ARInvoiceQueryResultForDisbursementsBilled.HasValue)
				{
					accounting_ARInvoiceQueryResultForDisbursementsBilled = JobDeclaration.InvoiceQuery.GetTotalInvoicedDetails(JobDeclaration, EntryChargeTypeCodesToMatch, JobDeclaration.TransactionNumber.ToString() + " - " + JZ_InvoiceNumber);
				}
				return accounting_ARInvoiceQueryResultForDisbursementsBilled.Value;
			}
		}
		AP_ARInvoiceQueryResult? accounting_ARInvoiceQueryResultForDisbursementsBilled;

		AP_ARInvoiceQueryResult Accounting_ARInvoiceQueryResultForAllBilled
		{
			get
			{
				if (!accounting_ARInvoiceQueryResultForAllBilled.HasValue)
				{
					accounting_ARInvoiceQueryResultForAllBilled = JobDeclaration.InvoiceQuery.GetTotalInvoicedDetails(JobDeclaration, null, JobDeclaration.TransactionNumber.ToString() + " - " + JZ_InvoiceNumber);
				}
				return accounting_ARInvoiceQueryResultForAllBilled.Value;
			}
		}
		AP_ARInvoiceQueryResult? accounting_ARInvoiceQueryResultForAllBilled;

		public List<ZGuid> EntryChargeTypeCodesToMatch
		{
			get { return Factory.GetCachedValue<EntryChargeTypeList>().GetAllChargeCodePKsOf(Branch.Company.PK).ToList(); }
		}

		#endregion

		#region override

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			var declaration = JobDeclaration;
			if (declaration != null && declaration.IsLVX)
			{
				var readyForConsolidationChanged = IsInDatabase && GetAddInfo().HasChangesSinceLastSaving(CAAddInfoSchema.CA_ReadyForConsolidation);

				if (CA_ReadyForConsolidation && (!IsInDatabase || readyForConsolidationChanged))
				{
					declaration.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.ReadyForConsolidation, EstimateActual.Actual, ZDateTimeOffset.Now, "Ready for Consolidation");
				}
				else if (!CA_ReadyForConsolidation && readyForConsolidationChanged)
				{
					var oldLog = declaration.Logs.MostRecentLogByEventTime(AutoEvents.ReadyForConsolidation);
					if (oldLog != null)
					{
						oldLog.Cancel();
					}
				}
			}
		}

		public override void Delete()
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.RefreshInvoiceLinesWithPGAs();
			}

			CorrespondingAsClaimedForInvoice?.Delete();

			base.Delete();
		}

		public override bool CanDelete
		{
			get
			{
				var declaration = IsAttachedToPersistentLVXDeclaration ? FirstAdditionalDeclaration : IsAttachedToPersistentLVSDeclaration ? JobDeclaration : null;
				return (declaration == null || !declaration.HasAB3AcceptedOrWaiting) && GoodsShipmentSequence.IsEmpty;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return !GoodsShipmentSequence.IsEmpty ? ResString.GetMultilingualString("CADInvoiceNotAbleToDelete", "Invoice not allowed to be deleted because there is CAD response for it.") :
				  ResString.GetMultilingualString("CAJobComInvoiceHeader|ReasonForNotAbleToDelete", "Shipment may not be deleted because this B3 has already been reported, or is waiting for a response.");
			}
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			if (IsB2AsClaimedForSeededHeader && this.CorrespondingAsAccountedForInvoice != null)
			{
				return ResString.GetMultilingualString("9fa588cf-83b4-4e52-943a-5196ad7e1fd5", "Deleting a Sub-header in 'As Claimed' will delete the corresponding Sub-header and  lines in 'As Accounted'.");
			}
			else
			{
				return base.GetWarningBeforeBeingDeleted();
			}
		}

		protected override void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobComInvoiceLine invoiceLine, LinkedToDeclarationData linkedToDeclarationData, bool updatePartSyncManagerAndRefresh)
		{
			base.UpdateWhenAnInvoiceIsLinkedToADeclaration(invoiceLine, linkedToDeclarationData, updatePartSyncManagerAndRefresh);
			var invoiceLineCA = (JobComInvoiceLine)invoiceLine;
			if (invoiceLineCA.IsRegulatedByCFIA || invoiceLineCA.IsRegulatedByIIDCFIA)
			{
				JobDeclaration.AVSEventRequired = true;
			}
		}

		protected override void UpdateWhenAnInvoiceIsDetached(BaseJobComInvoiceLine invoiceLine, bool isImport, bool isAdvanceShippingNotice)
		{
			base.UpdateWhenAnInvoiceIsDetached(invoiceLine, isImport, isAdvanceShippingNotice);
			var invoiceLineCA = (JobComInvoiceLine)invoiceLine;
			if (invoiceLineCA.IsRegulatedByCFIA || invoiceLineCA.IsRegulatedByIIDCFIA)
			{
				JobDeclaration.AVSEventRequired = true;
			}
		}

		public override ZDateTime JZ_ValuationDateOverride
		{
			get { return base.JZ_ValuationDateOverride; }
			set
			{
				var dateOfValue = value.IsValid ? ZDateTime.TruncateToDay(value) : value;
				bool hasChanges = base.JZ_ValuationDateOverride != dateOfValue;
				base.JZ_ValuationDateOverride = dateOfValue;
				if (hasChanges && dateOfValue.IsValid && (!JZ_InvoiceDate.IsValid || JZ_InvoiceDate > dateOfValue))
				{
					JZ_InvoiceDate = dateOfValue;
				}
				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Product Audit

#if DEBUG
		internal ZString ProductAuditActionCoreForTesting() => ProductAuditActionCore();
#endif

		protected override ZString ProductAuditActionCore()
		{
			ZString productAuditAction;

			if (IsImportIncludingB2)
			{
				var productAuditType = JobDeclaration?.ProductAuditType ?? ZString.Empty;

				productAuditAction = GetProductAuditActionFromOrgHeaderPK(JZ_OH_Buyer_Effective, productAuditType);
				if (productAuditAction == ProductAuditActions.Codes.RegistryDefault)
				{
					productAuditAction = GetProductAuditActionFromOrgHeaderPK(JZ_OH_Supplier_Effective, productAuditType);
					if (productAuditAction == ProductAuditActions.Codes.RegistryDefault)
					{
						productAuditAction = GetProductAuditActionFromRegistry(productAuditType);
					}
				}
			}
			else
			{
				productAuditAction = base.ProductAuditActionCore();
			}

			return productAuditAction;
		}

		internal ZString GetProductAuditActionFromOrgHeaderPK(ZGuid orgHeaderPK, ZString productAuditType)
		{
			var result = ProductAuditActions.Codes.RegistryDefault;
			var orgHeader = Factory.Load<OrgHeader>(orgHeaderPK);
			var orgImpAddInfo = OrgImpAddInfo.Get(orgHeader);

			if (orgImpAddInfo != null)
			{
				switch (productAuditType)
				{
					case JobDeclaration.CAProductAuditType.B3High:
						result = orgImpAddInfo.ZO_B3HighValueProductAuditAction;
						break;
					case JobDeclaration.CAProductAuditType.B3Low:
						result = orgImpAddInfo.ZO_B3LowValueProductAuditAction;
						break;
					case JobDeclaration.CAProductAuditType.ACROSSHigh:
						result = orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction;
						break;
					case JobDeclaration.CAProductAuditType.ACROSSLow:
						result = orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction;
						break;
				}
			}

			return result;
		}

		internal ZString GetProductAuditActionFromRegistry(ZString productAuditType)
		{
			ZArchitecture.Environment.CodePairRegistryItem auditActionRegistryItem = null;
			switch (productAuditType)
			{
				case JobDeclaration.CAProductAuditType.B3High:
					auditActionRegistryItem = CACustomsDataRegistry.Instance.EntryHighValueProductAudit;
					break;
				case JobDeclaration.CAProductAuditType.B3Low:
					auditActionRegistryItem = CACustomsDataRegistry.Instance.EntryLowValueProductAudit;
					break;
				case JobDeclaration.CAProductAuditType.ACROSSHigh:
					auditActionRegistryItem = CACustomsDataRegistry.Instance.ReleaseHighValueProductAudit;
					break;
				case JobDeclaration.CAProductAuditType.ACROSSLow:
					auditActionRegistryItem = CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit;
					break;
			}

			var branch = Branch;
			var branchPK = branch?.PK.ToGuid() ?? Guid.Empty;
			var companyPK = branch?.Company?.PK.ToGuid() ?? Guid.Empty;
			return auditActionRegistryItem?.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) ?? ProductAuditActions.Codes.NoAction;
		}

		#endregion

		#region Merge

		public void MarkDeclarationRequiresMerge()
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				declaration.CA_RequiresMerge = true;
			}
		}

		#endregion

		#region Packages

		protected override BaseCusLinkPackageCollection PackagesForInvoicesCore()
		{
			if (packages == null)
			{
				packages = new InvoiceHeaderCusLinkPackageCollection(this);
			}
			return packages;
		}
		BaseCusLinkPackageCollection packages;

		public void RefreshActualTotalPacksCount()
		{
			JZ_NoOfPacks = GetCusPackPivots().Sum(c => c.NumberOfPacks);
		}

		IEnumerable<ICusPackagePivot> GetCusPackPivots()
		{
			foreach (var packagePivot in PackagesPivot.Cast<ICusPackagePivot>())
			{
				if (packagePivot.Package?.IsLowestPackage ?? false)
				{
					yield return packagePivot;
				}
			}

			foreach (var invoiceLine in InvoiceLines.Cast<BaseJobComInvoiceLine>())
			{
				foreach (var packagePivot in invoiceLine.PackagesPivot.Cast<ICusPackagePivot>())
				{
					if (packagePivot.Package?.IsLowestPackage ?? false)
					{
						yield return packagePivot;
					}
				}
			}
		}

		#endregion

		#region PGAs

		public bool HasInvoiceLinesWithCFIAPGA
		{
			get
			{
				if (!hasInvoiceLinesWithCFIAPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithCFIAPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithCNSCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithCNSCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithCNSCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithDFOPGA
		{
			get
			{
				if (!hasInvoiceLinesWithDFOPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithDFOPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithECCCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithECCCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithECCCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithGACPGA
		{
			get
			{
				if (!hasInvoiceLinesWithGACPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithGACPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithHCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithHCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithHCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithNRCanPGA
		{
			get
			{
				if (!hasInvoiceLinesWithNRCanPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithNRCanPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithPHACPGA
		{
			get
			{
				if (!hasInvoiceLinesWithPHACPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithPHACPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithTCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithTCPGA.HasValue)
				{
					CalculateInvoiceLinesWithPGAs();
				}
				return hasInvoiceLinesWithTCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithWENIndOnECCCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithWENIndOnECCCPGA.HasValue)
				{
					calculateInvoiceLinesWithWENIndOnECCCPGA();
				}
				return hasInvoiceLinesWithWENIndOnECCCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithPESIndOnHCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithPESIndOnHCPGA.HasValue)
				{
					calculateInvoiceLinesWithIndOnHCPGA();
				}
				return hasInvoiceLinesWithPESIndOnHCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithCPRIndOnHCPGA
		{
			get
			{
				if (!hasInvoiceLinesWithCPRIndOnHCPGA.HasValue)
				{
					calculateInvoiceLinesWithIndOnHCPGA();
				}
				return hasInvoiceLinesWithCPRIndOnHCPGA.Value;
			}
		}

		public bool HasInvoiceLinesWithPGARequirements => InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => CARefTariffDataLoader.DoesTariffHasPGA(Factory, invoiceLine.JI_Tariff, ZDateTime.Today));

		void CalculateInvoiceLinesWithPGAs()
		{
			hasInvoiceLinesWithCFIAPGA = false;
			hasInvoiceLinesWithCNSCPGA = false;
			hasInvoiceLinesWithDFOPGA = false;
			hasInvoiceLinesWithECCCPGA = false;
			hasInvoiceLinesWithGACPGA = false;
			hasInvoiceLinesWithHCPGA = false;
			hasInvoiceLinesWithNRCanPGA = false;
			hasInvoiceLinesWithPHACPGA = false;
			hasInvoiceLinesWithTCPGA = false;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				hasInvoiceLinesWithCFIAPGA |= invoiceLine.CFIAPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_CFIAInd);
				hasInvoiceLinesWithCNSCPGA |= invoiceLine.CNSCPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_CNSCInd);
				hasInvoiceLinesWithDFOPGA |= invoiceLine.DFOPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_DFOInd);
				hasInvoiceLinesWithECCCPGA |= invoiceLine.ECCCPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_ECCCInd);
				hasInvoiceLinesWithGACPGA |= invoiceLine.GACPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_GACInd);
				hasInvoiceLinesWithHCPGA |= invoiceLine.HCPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_HCInd);
				hasInvoiceLinesWithNRCanPGA |= invoiceLine.NRCanPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_NRCanInd);
				hasInvoiceLinesWithPHACPGA |= invoiceLine.PHACPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_PHACInd);
				hasInvoiceLinesWithTCPGA |= invoiceLine.TCPGAHeader != null && YesNoList.IsYes(invoiceLine.CA_TCInd);
			}
		}

		void calculateInvoiceLinesWithWENIndOnECCCPGA()
		{
			hasInvoiceLinesWithWENIndOnECCCPGA = false;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				hasInvoiceLinesWithWENIndOnECCCPGA |= (invoiceLine.ECCCPGAHeader != null && invoiceLine.ECCCPGAHeader.CA_WENProgramInd == YesNoList.Codes.Yes);
			}
		}

		void calculateInvoiceLinesWithIndOnHCPGA()
		{
			hasInvoiceLinesWithPESIndOnHCPGA = false;
			hasInvoiceLinesWithCPRIndOnHCPGA = false;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				hasInvoiceLinesWithPESIndOnHCPGA |= (invoiceLine.HCPGAHeader != null && invoiceLine.HCPGAHeader.CA_PESProgramInd == YesNoList.Codes.Yes);
				hasInvoiceLinesWithCPRIndOnHCPGA |= (invoiceLine.HCPGAHeader != null && invoiceLine.HCPGAHeader.CA_CPRProgramInd == YesNoList.Codes.Yes);
			}
		}

		public void RefreshInvoiceLinesWithPGAs()
		{
			hasInvoiceLinesWithCFIAPGA = null;
			hasInvoiceLinesWithCNSCPGA = null;
			hasInvoiceLinesWithDFOPGA = null;
			hasInvoiceLinesWithECCCPGA = null;
			hasInvoiceLinesWithGACPGA = null;
			hasInvoiceLinesWithHCPGA = null;
			hasInvoiceLinesWithNRCanPGA = null;
			hasInvoiceLinesWithPHACPGA = null;
			hasInvoiceLinesWithTCPGA = null;
		}

		public void RefreshInvoiceLinesWithWENIndOnECCCPGA()
		{
			hasInvoiceLinesWithWENIndOnECCCPGA = null;
		}

		public void RefreshInvoiceLinesWithPESIndOnHCPGA()
		{
			hasInvoiceLinesWithPESIndOnHCPGA = null;
		}

		public void RefreshInvoiceLinesWithCPRIndOnHCPGA()
		{
			hasInvoiceLinesWithCPRIndOnHCPGA = null;
		}

		bool? hasInvoiceLinesWithCFIAPGA;
		bool? hasInvoiceLinesWithHCPGA;
		bool? hasInvoiceLinesWithPHACPGA;
		bool? hasInvoiceLinesWithTCPGA;
		bool? hasInvoiceLinesWithECCCPGA;
		bool? hasInvoiceLinesWithNRCanPGA;
		bool? hasInvoiceLinesWithDFOPGA;
		bool? hasInvoiceLinesWithCNSCPGA;
		bool? hasInvoiceLinesWithGACPGA;
		bool? hasInvoiceLinesWithWENIndOnECCCPGA;
		bool? hasInvoiceLinesWithPESIndOnHCPGA;
		bool? hasInvoiceLinesWithCPRIndOnHCPGA;

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		public BuyerSupplierLinksHelper<JobComInvoiceHeader> BuyerSupplierLinksHelper;

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Buyer; }
			set { JZ_OH_Buyer = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { JZ_OH_BuyerInfo.ValueChanged += value; }
			remove { JZ_OH_BuyerInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress
		{
			get { return null; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Supplier; }
			set { JZ_OH_Supplier = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { JZ_OH_SupplierInfo.ValueChanged += value; }
			remove { JZ_OH_SupplierInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsignorPickupAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return JZ_RX_NKInvoice_Currency; }
			set { JZ_RX_NKInvoice_Currency = value; }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.IsSettingDefaultValues
		{
			get { return false; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return ZString.Empty; }
			set { }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add { }
			remove { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return JZ_IncoTerm; }
			set { JZ_IncoTerm = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return ZString.Empty; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
		{
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return !IsAttachedToPersistentLVSDeclaration; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreImportBrokerFallback()
		{
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return false; }
		}

		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get { return JobDeclaration != null ? JobDeclaration.JE_TransportMode : ZString.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get
			{
				if (JobDeclaration != null)
				{
					return JobDeclaration.CurrencyConverterMaximumDaysToFallBack;
				}
				else
				{
					return 365;
				}
			}
		}

		#endregion

		#region ISynchroniserReadOnlyMembersProvider

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fB2AsClaimedInvoiceSynchroniser != null)
				{
					fB2AsClaimedInvoiceSynchroniser.SetEnabled(false, fB2AsClaimedInvoiceSynchroniser.DetectEnabled);
					fB2AsClaimedInvoiceSynchroniser.Dispose();
					fB2AsClaimedInvoiceSynchroniser = null;
				}
			}
		}

		#endregion

		public override ZGuid JZ_JE
		{
			get
			{
				return base.JZ_JE;
			}
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;
				if (!IsCopying && oldValue != value)
				{
					var dec = JobDeclaration;
					if (dec != null && dec.IsImport)
					{
						PageNumberCalculator.RecalculatePageNumber(JobDeclaration, JZ_InvoiceDisplaySequence - 1);
						dec.RefreshInvoiceLinesWithPGAs();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(JZ_InvoiceDisplaySequence_ReadOnly))]
		public override ZShort JZ_InvoiceDisplaySequence
		{
			get => base.JZ_InvoiceDisplaySequence;
			set
			{
				var oldValue = JZ_InvoiceDisplaySequence;
				base.JZ_InvoiceDisplaySequence = value;
				if (!IsCopying && oldValue != value)
				{
					var dec = JobDeclaration;
					if (dec != null && dec.IsImport)
					{
						PageNumberCalculator.RecalculatePageNumber(JobDeclaration, Math.Min(oldValue, JZ_InvoiceDisplaySequence));
					}
				}
			}
		}

		ZBool JZ_InvoiceDisplaySequence_ReadOnly => JobDeclaration?.HasAB3AcceptedOrWaiting ?? false;

		public JobCAComInvoiceHeader CAInvoiceHeader => this.LoadOrCreateAddInfoChild(ref caInvoiceHeader);
		JobCAComInvoiceHeader caInvoiceHeader;

		public bool IsRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader
		{
			get
			{
				if (isRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader == null)
				{
					isRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader = new CachedProperty<bool>(Factory, () =>
					{
						return IsAttachedToPersistentLVXDeclaration && (this.IsRemissionAll() || this.IsRemissionMexicoAndUSDutyOnly());
					});
				}
				return isRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader.Value;
			}
		}

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute1 => JobComInvoiceLine.Schema.JI_PartAttrib1;

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute2 => JobComInvoiceLine.Schema.JI_PartAttrib2;

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute3 => JobComInvoiceLine.Schema.JI_PartAttrib3;

		ZString ICustomsCustomLabelsConfigOrgProvider.SerialNumber => JobComInvoiceLine.Schema.JI_SerialNumber;

		#region ICustomLabelsConfigOrgProvider

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get
			{
				var declaration = JobDeclaration;
				if (declaration != null && declaration.IsLVS)
				{
					return Buyer;
				}
				return null;
			}
		}

		CachedProperty<bool> isRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader;

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				JZ_OH_BuyerInfo.ValueChanged += value;
			}
			remove
			{
				JZ_OH_BuyerInfo.ValueChanged -= value;
			}
		}

		BusinessObjectFactory ICustomLabelsConfigOrgProvider.Factory
		{
			get
			{
				return Factory;
			}
		}

		#endregion

		#region IAddInfoChildSupporter Members
		protected override BusinessObject GetAddInfoChild() => CAInvoiceHeader;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobCAComInvoiceHeaderSchema.CAZ_JZ;
		#endregion
	}
}
