using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SystemDefinedValues]
	[UniversalCopyAddInfo(JobComInvoiceHeaderSchema.Constants.Prefix, AUAddInfo.Schema.Prefix)]
	public class JobComInvoiceHeader : TypeSafeJobComInvoiceHeader, IAddInfo, IAQIS, Integration.Customs.AU.IJobComInvoiceHeader, IAddInfoManager, ICusStorageDocPivotParent
	{
		#region Constants
		public new class Schema : BaseJobComInvoiceHeader.Schema
		{
			public const string JZ_CommissionType = "JZ_CommissionType";
			public const string JZ_Nature10PackCount = "JZ_Nature10PackCount";
			public const string JZ_Nature10UnitPack = "JZ_Nature10UnitPack";
			public const string JZ_BondPackCount = "JZ_BondPackCount";
			public const string JZ_BondUnitPack = "JZ_BondUnitPack";
			public const string JZ_ValuationBasis = "JZ_ValuationBasis";
			public const string JZ_PiecesForRelease = "JZ_PiecesForRelease";
			public const string JZ_PiecesToBond = "JZ_PiecesToBond";
			public const string JZ_Calc_RealFOB = "JZ_Calc_RealFOB";
			public const string JZ_Calc_RealCIF = "JZ_Calc_RealCIF";
			public const string JZ_Calc_CustomsValue = "JZ_Calc_CustomsValue";
			public const string ZA_ORG = "ZA_ORG";
			public const string ZA_PRF = "ZA_PRF";
			public const string ZA_GSTE = "ZA_GSTE";
			public const string ZA_PermitNumbers_Hidden = "ZA_PermitNumbers_Hidden";
			public const string JZ_ExporterReference = "JZ_ExporterReference";

			public const int JZ_ExporterReferenceMaxLength = 35;
		}

		public static class NatureString
		{
			public const string Nature10 = "10";
			public const string Nature20 = "20";
			public const string Nature30 = "30";
			public const string NotDetermined = "";
		}

		public static class CommissionType
		{
			public const string Buying = "1";
			public const string Selling = "2";
			public const string Agency = "3";
			public const string Confirming = "4";
			public const string Other = "9";
		}
		#endregion

		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			constructStackTrace = System.Environment.StackTrace;
		}
		internal readonly string constructStackTrace;

		void IAggregatedAddInfo.MarkAsNeedingValidation()
		{
			MarkAsNeedingValidationForMajorDataChange();
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			JobComInvoiceLineViewCollection result = null;
			if (JobDeclaration != null)
			{
				result = new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}
			return result;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			InvoiceLineDependentCollection result = new InvoiceLineDependentCollection(this);
			result.Load();
			return new JobComInvoiceLineViewCollection(this, result);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new JobComInvChargeCollection<InvoiceCharge>(this);
		}

		protected override JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new AUJobComInvoiceHeaderLookups(this);
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return JobDeclaration.LocalCurrencyConstantCode; }
		}

		public Money AggregatedTILV
		{
			get
			{
				Money result = Money.Invalid;

				foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
				{
					if (!invoiceLine.AddInfo.EffectiveTILVString.IsEmpty)
					{
						Money amountToAdd = invoiceLine.AddInfo.TILVMoney;

						if (amountToAdd.IsValid)
						{
							if (!result.IsValid)
							{
								result = Money.Empty;
							}

							result = CurrencyConverter.Add(result, amountToAdd);
						}
					}
				}
				return result;
			}
		}

		public ZDecimal AggregatedTILVInLocalCurrency
		{
			get
			{
				return CurrencyConverter.ConvertExact(AggregatedTILV, JobDeclaration.GetLocalCurrency()).Amount;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZBool IsNEXDOCSActive
		{
			get
			{
				return GetQuarantineExDocHeaderWithoutCreatingNew()?.IsNEXDOCSActive ?? false;
			}
		}

		public ZDateTime EffectiveDutyDate
		{
			get
			{
				if (!isEffectiveDutyDateValid)
				{
					isEffectiveDutyDateValid = true;

					fEffectiveDutyDate = AddInfo.EFD;

					JobDeclaration jobDeclaration = this.JobDeclaration;

					if (fEffectiveDutyDate.IsEmpty && jobDeclaration != null)
					{
						if (jobDeclaration.IsDrawback)
						{
							fEffectiveDutyDate = JZ_InvoiceDate;
						}
						else
						{
							fEffectiveDutyDate = jobDeclaration.EffectiveDutyDate;
						}
					}

					EffectiveDutyDateInfo.RefreshBinding();
				}
				return fEffectiveDutyDate;
			}
		}
		bool isEffectiveDutyDateValid;
		ZDateTime fEffectiveDutyDate;

		public void NotifyEffectiveDutyDateDirty()
		{
			isEffectiveDutyDateValid = false;
			EffectiveDutyDateInfo.RefreshBinding();
		}

		public ZPropertyInfo EffectiveDutyDateInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveDutyDate)); }
		}

		public override ZDateTime JZ_ValuationDateOverride
		{
			get => base.JZ_ValuationDateOverride;
			set
			{
				var oldValue = JZ_ValuationDateOverride;
				base.JZ_ValuationDateOverride = value;
				if (!IsCopying && oldValue != JZ_ValuationDateOverride)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public string Nature
		{
			get
			{
				string result = NatureString.NotDetermined;
				JobDeclaration cachedDeclaration = JobDeclaration;

				if (cachedDeclaration != null)
				{
					if (cachedDeclaration.IsNature30)
					{
						result = NatureString.Nature30;
					}
					else if (cachedDeclaration.IsImport && !cachedDeclaration.IsImportCMR)
					{
						if (JZ_Nature10PackCount > 0 && JZ_BondPackCount == 0)
						{
							result = NatureString.Nature10;
						}
						else if (JZ_Nature10PackCount == 0 && JZ_BondPackCount > 0)
						{
							result = NatureString.Nature20;
						}
					}
				}
				return result;
			}
		}

		[MaxLength(3)]
		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_ValuationBasis_Hidden)]
		public ZString JZ_ValuationBasis
		{
			get { return AddInfo.ZA_ValuationBasis_Hidden; }
			set { AddInfo.ZA_ValuationBasis_Hidden = value; }
		}

		public ZPropertyInfo JZ_ValuationBasisInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetWrappedZPropertyInfo(Schema.JZ_ValuationBasis, x => AddInfo.ZA_ValuationBasis_HiddenInfo); }
		}

		#region JZ_ExporterReference
		[ReadOnly(true)]
		[MaxLength(Schema.JZ_ExporterReferenceMaxLength)]
		[ResourceStringData("AU.Declaration.Business.JobComInvoiceHeader.JZ_ExporterReference", Caption = "Exporter Reference")]
		public ZString JZ_ExporterReference
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.JZ_ExporterReference); }
			set
			{
				CheckMaximumLength(JZ_ExporterReferenceInfo, value);
				this.SetSystemDefinedValue(Schema.JZ_ExporterReference, value);
				JZ_ExporterReferenceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JZ_ExporterReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_ExporterReference); }
		}
		#endregion

		public bool HasAPackCountHasBeenEnteredForThisInvoice
		{
			get
			{
				return !((JZ_Nature10PackCount.IsEmpty || JZ_Nature10PackCount == 0) && (JZ_BondPackCount.IsEmpty || JZ_BondPackCount == 0));
			}
		}

		public bool HasAPiecesCountHasBeenEnteredForThisInvoice
		{
			get
			{
				return AddInfo.ZA_PiecesForBond_Hidden != 0 || AddInfo.ZA_PiecesForRelease_Hidden != 0;
			}
		}

		public void DefaultValuationBasisIfPossible()
		{
			if (!IsCopying)
			{
				OrgSupplierBuyerLink link = SupplierBuyerLink;
				DefaultVALBIfPossible(link);
				DefaultHeaderRELIfPossible(link);
			}
		}

		void DefaultVALBIfPossible(OrgSupplierBuyerLink link)
		{
			if (link != null && !link.OL_ValuationBasis.IsEmpty)
			{
				AddInfo.ZA_VALB_Hidden = GetDefaultVALBforCMR(link.OL_ValuationBasis);
			}
			else
			{
				AddInfo.ZA_VALB_Hidden = CMRValuationBasisList.Codes._1stPref_TransactionValue;
			}
		}

		void DefaultHeaderRELIfPossible(OrgSupplierBuyerLink link)
		{
			if (link != null && !link.OL_RelatedParty.IsEmpty)
			{
				AddInfo.ZA_HeaderREL_Hidden = link.OL_RelatedParty;
			}
			else
			{
				AddInfo.ZA_HeaderREL_Hidden = CMRRelatedTransaction.No.Code;
			}
		}

		ZString GetDefaultVALBforCMR(ZString orgValuationBasis)
		{
			return AddInfo.Lookups.ValuationBasisListForCMR.ContainsCode(orgValuationBasis) ? orgValuationBasis : ZString.Empty;
		}

		public void DefaultORGIfPossible()
		{
			if (!IsCopying && Supplier != null)
			{
				if (Supplier.MiscServ.EXDefaultCntryOfOrigin != null)
				{
					AddInfo.ZA_ORG = Supplier.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin;
				}
				else if (Supplier.ClosestPort != null && Supplier.ClosestPort.Country != null)
				{
					AddInfo.ZA_ORG = Supplier.ClosestPort.RL_RN_NKCountryCode;
				}
			}
		}

		void DefaultSupplierAddressIfPossible()
		{
			var supplier = Supplier;
			if (supplier != null && supplier.MainAddress != null)
			{
				JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			}
		}

		#region IAddInfo Members

		public ZDateTime DateOfValuation
		{
			get { return EffectiveValuationDate; }
		}

		public ZString AggregatedZA_ORG
		{
			get
			{
				if (AddInfo.ZA_ORG != "" || AddInfo.ZA_PRF != "")
				{
					return AddInfo.ZA_ORG;
				}
				else if (Master != null)
				{
					return ((IAddInfo)Master).AggregatedZA_ORG;
				}
				else
				{
					return "";
				}
			}
		}

		public ZString AggregatedZA_PRF
		{
			get
			{
				if (AddInfo.ZA_ORG != "" || AddInfo.ZA_PRF != "")
				{
					return AddInfo.ZA_PRF;
				}
				else if (Master != null)
				{
					return ((IAddInfo)Master).AggregatedZA_PRF;
				}
				else
				{
					return "";
				}
			}
		}

		public IZType AggregatedValue(string propertyName)
		{
			IZType result = (IZType)AddInfo[propertyName];
			if (result.IsEmpty)
			{
				BaseJobComInvoiceGroupHeader master = this.Master;  // Caching
				if (master != null)
				{
					result = ((IAddInfo)master).AggregatedValue(propertyName);
				}
			}
			return result;
		}

		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AUAddInfo(this, JZ_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					using (fAddInfo.SuspendSettingHasChanges())
					{
						((ILightValidationInternals)fAddInfo).IsValid = ((ILightValidationInternals)this).IsValid;
					}
				}
				return fAddInfo;
			}
		}

		public IEnumerable<ZString> Permits
		{
			get { return AddInfo.Permits; }
		}

		public IEnumerable<ZString> EncryptionNumbers
		{
			get { return AddInfo.EncryptionNumbers; }
		}

		bool IAggregatedAddInfo.IsCopying
		{
			get
			{
				return IsCopying;
			}
		}

		#endregion

		#region Overrides

		public void MarkAllChargesIncludingInvoiceLinesOnesAsNeedingValidation()
		{
			if (!IsCopying)
			{
				if (JobDeclaration != null)
				{
					Charges.MarkAsNeedingValidation();
					GroupCharges.MarkAsNeedingValidation();

					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.MarkAllChargesAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				var oldValue = JZ_JE;
				base.JZ_JE = value;

				if (JZ_JE != oldValue)
				{
					LoadOrCreateQuarantineExDocHeader();

					JobComInvoiceLines.CreateContainersForLines();
					JobComInvoiceLines.SynchroniseQuarantineLineProcesses();

					if (!IsDataChangeSuspendedByFakeDeclaration)
					{
						MarkAllChargesIncludingInvoiceLinesOnesAsNeedingValidation();
						JobComInvoiceLines.MarkQuarantineLineAsNeedingValidation();

						var ednNumber = AddInfo.ZA_EDN_Hidden;
						if (!ednNumber.IsEmpty)
						{
							var declaration = JobDeclaration;
							if (declaration != null
								&& declaration.IsPersistent
								&& declaration.IsExport
								&& declaration.EntryType == CANType.CustomsAuthorityNumber.Code
								&& declaration.DeclarationNumber.IsEmpty)
							{
								declaration.DeclarationNumber = ednNumber;
							}
						}
					}
				}
			}
		}

		public override ZString JZ_IncoTerm
		{
			get { return base.JZ_IncoTerm; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					if (!IsCopying && oldValue != JZ_IncoTerm)
					{
						MarkAllChargesIncludingInvoiceLinesOnesAsNeedingValidation();
					}
				}
			}
		}

		public override ZDateTime JZ_InvoiceDate
		{
			get { return base.JZ_InvoiceDate; }
			set
			{
				bool hasChanged = base.JZ_InvoiceDate != value;
				if (hasChanged && JobDeclaration != null && JobDeclaration.IsDrawback)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.ReCalculateClaimAmountIfRepresentativeShipment();
					}
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
				base.JZ_InvoiceDate = value;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (AddInfo.SuspendSettingHasChanges())
			{
				AddInfo.ZA_VALB_Hidden = CMRValuationBasisList.Codes._1stPref_TransactionValue;
				AddInfo.ZA_HeaderREL_Hidden = CMRRelatedTransaction.No.Code;
			}
		}

		protected override void RefreshDefaultsWhenInvoiceAttachedToDeclarationCore()
		{
			var link = SupplierBuyerLink;
			if (AddInfo.ZA_VALB_Hidden.IsEmpty)
			{
				DefaultVALBIfPossible(link);
			}
			if (AddInfo.ZA_HeaderREL_Hidden.IsEmpty)
			{
				DefaultHeaderRELIfPossible(link);
			}
			if (AddInfo.ZA_ORG.IsEmpty)
			{
				DefaultORGIfPossible();
			}
		}

		bool? lastSaveSucceededForDebug;
		bool lastOnSavingIsInDatabaseForDebug;
		bool lastOnSavedIsInDatabaseForDebug;

		public override void OnSaved(bool saveSucceeded)
		{
			lastSaveSucceededForDebug = saveSucceeded;
			lastOnSavedIsInDatabaseForDebug = IsInDatabase;
			base.OnSaved(saveSucceeded);
			AddInfo.OnSaved(saveSucceeded);
		}

		public override void OnSaving()
		{
			lastOnSavingIsInDatabaseForDebug = IsInDatabase;
			base.OnSaving();

			if (JZ_JE.IsValid && IsQuarantine)
			{
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JZ_JE), IsInDatabase ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Default);
			}
		}

		public void CleanUnnecessaryQuarantineValuesOnSaving()
		{
			if (IsQuarantine && QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.SkinsAndHides)
			{
				CleanAQISLoadingEstablishmentLocation();
				QuarantineExDocHeader.QH_LoadingDate = ZDate.Empty;
			}
		}

		public override int PackagesFreeStore
		{
			get { return 0; }
		}

		public override ZDecimal JZ_Calc_TNI
		{
			get
			{
				ZDecimal result = 0m;

				if (!AddInfo.ZA_TILV.IsEmpty)
				{
					result = AddInfo.TILVMoney.Amount;
				}
				else if (JobComInvoiceLines.Count > 0)
				{
					result = AggregatedTransportAndInsurance.Amount;
				}
				else
				{
					result = base.JZ_Calc_TNI;
				}

				return result;
			}
		}

		internal Money AggregatedTransportAndInsurance => Factory.GetValue(ref aggregatedTransportAndInsuranceCached, delegate
		{
			Money result = Money.Empty;

			foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
			{
				result = CurrencyConverter.Add(result, invoiceLine.TransportAndInsurance);
			}

			return result;
		});

		CachedProperty<Money> aggregatedTransportAndInsuranceCached;

		public override ZGuid JZ_RX_Calc_TNICurrency
		{
			get
			{
				var currencyPK = ZGuid.Empty;
				if (!AddInfo.ZA_TILV.IsEmpty)
				{
					currencyPK = AddInfo.TILVMoney.Currency != null ? AddInfo.TILVMoney.Currency.PK : ZGuid.Empty;
				}
				else if (JobComInvoiceLines.Count > 0)
				{
					currencyPK = AggregatedTransportAndInsurance.Currency != null ? AggregatedTransportAndInsurance.Currency.PK : ZGuid.Empty;
				}
				else
				{
					currencyPK = base.JZ_RX_Calc_TNICurrency;
				}

				return currencyPK;
			}
		}

		public Money JZ_Calc_TNIMoney
		{
			get
			{
				RefCurrency tNICurrency = Factory.Load<RefCurrency>(JZ_RX_Calc_TNICurrency);
				return tNICurrency != null ? new Money(JZ_Calc_TNI, tNICurrency) : Money.Empty;
			}
		}

		public ZDecimal JZ_Calc_TNIInLocalCurrency
		{
			get
			{
				RefCurrency tNICurrency = Factory.Load<RefCurrency>(JZ_RX_Calc_TNICurrency);
				return tNICurrency != null ? CurrencyConverter.ConvertExact(new Money(JZ_Calc_TNI, tNICurrency), JobDeclaration.GetLocalCurrency()).Amount : ZDecimal.Zero;
			}
		}

		public override ZDecimal JZ_InvoiceCurrLandedCostExRate
		{
			get { return base.JZ_InvoiceCurrLandedCostExRate; }
			set
			{
				var declaration = JobDeclaration;
				if (declaration != null && declaration.CustomsEntryHeaders.Count > 0)
				{
					#pragma warning disable IDE0001 // Prevent simplification to base class
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
					#pragma warning restore IDE0001 // Prevent simplification to base class
					{
						base.JZ_InvoiceCurrLandedCostExRate = value;
					}
				}
				else
				{
					base.JZ_InvoiceCurrLandedCostExRate = value;
				}
			}
		}

		#endregion

		#region Override FOB

		public ZDecimal EffectiveFOBAmount
		{
			get { return JZ_OverrideFOB ? JZ_FOBValue : JZ_Calc_FOBAmount; }
			set { JZ_FOBValue = value; }
		}

		protected bool EffectiveFOBAmount_ReadOnly
		{
			get { return !JZ_OverrideFOB; }
		}

		public ZPropertyInfo EffectiveFOBAmountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EffectiveFOBAmount), x => JZ_FOBValueInfo); }
		}

		public ZDecimal EffectiveCIFAmount
		{
			get { return EffectiveFOBAmount + JZ_Calc_OFTInInvoiceCurrency + JZ_Calc_ONSInInvoiceCurrency; }
		}

		public ZPropertyInfo EffectiveCIFAmountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EffectiveCIFAmount), x => JZ_Calc_CIFAmountInfo); }
		}

		public Money JZ_FOB
		{
			get { return new Money(EffectiveFOBAmount, Invoice_Currency); }
		}

		public override ZBool JZ_OverrideFOB
		{
			get { return base.JZ_OverrideFOB; }
			set
			{
				base.JZ_OverrideFOB = value;
				if (!IsCopying && value)
				{
					JZ_FOBValue = JZ_Calc_FOBAmount;
				}
			}
		}

		#endregion

		#region JobValidation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			JobValidation.ValidateJZ_Calc_Balance();
			JobValidation.ValidateJZ_Nature10PackCount();
			JobValidation.ValidateJZ_ITOTIncoTerm();
		}

		#endregion

		#region Properties

		public override ZString JZ_AddInfo
		{
			get { return base.JZ_AddInfo; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
				if (JZ_AddInfo != value)
				{
					base.JZ_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(value);
					}
				}
			}
		}

		public ZDecimal ValuationFactor
		{
			get
			{
				ZDecimal result;
				ZDecimal cachedLineTotal = InvoiceLineTotal;

				if (cachedLineTotal != 0)
				{
					result = ZArchitecture.Core.Utilities.Round(JZ_Calc_FOBAmount / cachedLineTotal, 8);
				}
				else
				{
					result = 0;
				}
				return result;
			}
		}

		public ZDecimal TotalLinePriceOfAllExportDrawbackLines
		{
			get
			{
				ZDecimal result = 0;
				if (JobDeclaration != null)
				{
					foreach (JobComInvoiceLine line in JobComInvoiceLines)
					{
						if (line.JI_Drawback)
						{
							result += line.JI_LinePrice;
						}
					}
				}
				return result;
			}
		}

		public void OnDrawbackApportioned()
		{
			foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
			{
				invoiceLine.CalculateClaimAmountIfImputationMethod();
			}
		}

		[ResourceStringData("AUJobComInvoiceHeader|A67F0D9A-A96A-4F4B-B08C-136CD1A1FCF2", Caption = "Supplier", FullDescription = SupplierFullDescription)]
		public override ZGuid JZ_OH_Supplier
		{
			get { return base.JZ_OH_Supplier; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					bool hasChanged = JZ_OH_Supplier != value;
					base.JZ_OH_Supplier = value;
					if (!IsCopying && hasChanged)
					{
						DefaultValuationBasisIfPossible();
						DefaultORGIfPossible();
						DefaultSupplierAddressIfPossible();
						if (IsInvoiceLinesLoaded())
						{
							CommodityCodeDefaulter.AttemptDefaultFromSupplier(Supplier, JobComInvoiceLines);
						}
					}
				}
			}
		}

		[ResourceStringData("AUJobComInvoiceHeader|EFE68CE0-A510-42D0-84F9-43DCFF94D272", Caption = "Supplier Address", FullDescription = SupplierFullDescription)]
		public override ZGuid JZ_OA_SupplierAddress
		{
			get => base.JZ_OA_SupplierAddress;
			set => base.JZ_OA_SupplierAddress = value;
		}

		internal const string SupplierFullDescription = "This address determines what CID code is used in the message for each invoice";

		protected override bool AllowDefaultSupplier => false;

		public override ZGuid JZ_OH_Buyer
		{
			get { return base.JZ_OH_Buyer; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Buyer))
				{
					bool hasChanged = JZ_OH_Buyer != value;
					base.JZ_OH_Buyer = value;
					if (!IsCopying && hasChanged)
					{
						DefaultValuationBasisIfPossible();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsExWarehouse))]
		public override ZString JZ_RX_NKInvoice_Currency
		{
			get
			{
				if (IsExWarehouse)
				{
					return Core.Constants.CurrencyCodes.Australia;
				}
				else
				{
					return base.JZ_RX_NKInvoice_Currency;
				}
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_RX_NKInvoice_Currency))
				{
					var oldValue = JZ_RX_NKInvoice_Currency;
					base.JZ_RX_NKInvoice_Currency = value;
					if (!IsCopying && JZ_RX_NKInvoice_Currency != oldValue)
					{
						JobComInvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		bool IsExWarehouse
		{
			get
			{
				JobDeclaration cachedDeclaration = JobDeclaration;
				return cachedDeclaration != null && cachedDeclaration.IsExWarehouse;
			}
		}

		public bool IsQuarantine => JZ_MessageType == AUJobMessageTypeList.Codes.Quarantine;

		public bool ShouldInvoiceLinesHaveTILV => Factory.GetValue(ref cachedShouldInvoiceLinesHaveTILV, GetShouldInvoiceLinesHaveTILV);

		CachedProperty<bool> cachedShouldInvoiceLinesHaveTILV;

		bool GetShouldInvoiceLinesHaveTILV()
		{
			bool result = false;
			foreach (JobComInvoiceLine currentInvoiceLine in JobComInvoiceLines)
			{
				if (!currentInvoiceLine.AddInfo.EffectiveTILVString.IsEmpty)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#endregion

		#region CodeDescriptionPairList

		//TODO: Remove
		public CodeDescriptionPairList JZ_ValuationBasis_List_ForEDIFICE
		{
			get { return AddInfo.Lookups.HeaderValuationBasisListForEDIFICE; }
		}

		#endregion

		#region New Properties

		public AUJobComInvoiceHeaderLookups AUInvoiceHeaderLookups
		{
			get { return (AUJobComInvoiceHeaderLookups)Lookups; }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_PackCountForNature10_Hidden)]
		public ZInt JZ_Nature10PackCount
		{
			get { return AddInfo.ZA_PackCountForNature10_Hidden; }
			set
			{
				bool hasChanged = JZ_Nature10PackCount != value;
				AddInfo.ZA_PackCountForNature10_Hidden = value;
				if (hasChanged && !IsCopying && JobDeclaration != null)
				{
					JobDeclaration.MarkAsNeedingValidation();
				}
			}
		}

		public ZPropertyInfo JZ_Nature10PackCountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_Nature10PackCount, x => AddInfo.ZA_PackCountForNature10_HiddenInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_PiecesForRelease_Hidden)]
		public ZInt JZ_PiecesForRelease
		{
			get { return AddInfo.ZA_PiecesForRelease_Hidden; }
			set { AddInfo.ZA_PiecesForRelease_Hidden = value; }
		}
		public ZPropertyInfo JZ_PiecesForReleaseInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_PiecesForRelease, x => AddInfo.ZA_PiecesForRelease_HiddenInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_PiecesForBond_Hidden)]
		public ZInt JZ_PiecesToBond
		{
			get { return AddInfo.ZA_PiecesForBond_Hidden; }
			set { AddInfo.ZA_PiecesForBond_Hidden = value; }
		}
		public ZPropertyInfo JZ_PiecesToBondInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_PiecesToBond, x => AddInfo.ZA_PiecesForBond_HiddenInfo); }
		}

		public ZString JZ_Nature10UnitPack
		{
			get
			{
				if (JobDeclaration == null)
				{
					return "";
				}
				else
				{
					return JobDeclaration.JE_TotalNoOfPacksPackType;
				}
			}
		}
		public ZPropertyInfo JZ_Nature10UnitPackInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_Nature10UnitPack); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_PackCountForBond_Hidden)]
		public ZInt JZ_BondPackCount
		{
			get { return AddInfo.ZA_PackCountForBond_Hidden; }
			set
			{
				bool hasChanged = JZ_BondPackCount != value;
				AddInfo.ZA_PackCountForBond_Hidden = value;
				if (hasChanged && !IsCopying && JobDeclaration != null)
				{
					JobDeclaration.MarkAsNeedingValidation();
				}
			}
		}

		public ZPropertyInfo JZ_BondPackCountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_BondPackCount, x => AddInfo.ZA_PackCountForBond_HiddenInfo); }
		}

		public ZString JZ_BondUnitPack
		{
			get { return JobDeclaration == null ? ZString.Empty : JobDeclaration.JE_TotalNoOfPacksPackType; }
		}
		public ZPropertyInfo JZ_BondUnitPackInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_BondUnitPack); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoAUAddInfo.Schema.ZA_CommissionType_Hidden)]
		public ZString JZ_CommissionType
		{
			get { return AddInfo.ZA_CommissionType_Hidden; }
			set
			{
				AddInfo.ZA_CommissionType_Hidden = value;
			}
		}
		public ZPropertyInfo JZ_CommissionTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_CommissionType, x => AddInfo.ZA_CommissionType_HiddenInfo); }
		}

		public CodeDescriptionPairList JZ_CommissionType_List
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairListCustomsCommissionType>(); }
		}

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceApportionedCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionedCharge>)base.GroupCharges;

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceApportionedCharge>(this);
		}

		#endregion

		#region Implementation

		protected AUAddInfo fAddInfo;

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			return JobValidation;
		}

		protected override OrgHeader BuyerForSuppplierLinkCalculation
		{
			get { return IsAttachedToPersistentDeclaration ? JobDeclaration.Importer : Buyer; }
		}

		protected override OrgHeader SupplierForSuppplierLinkCalculation
		{
			get { return Supplier; }
		}

		protected JobComInvoiceHeaderValidation JobValidation
		{
			get
			{
				JobDeclaration jobDec = JobDeclaration;
				if (jobDec != null)
				{
					if (jobDec.IsExport)
					{
						return new ExportJobComInvoiceHeaderValidation(this);
					}
					else if (jobDec.IsSAC)
					{
						return new SACJobComInvoiceHeaderValidation(this);
					}
					else if (jobDec.IsImport)
					{
						return new ImportJobComInvoiceHeaderValidation(this);
					}
				}
				return new JobComInvoiceHeaderValidation(this);
			}
		}

		CommodityCodeDefaulter CommodityCodeDefaulter
		{
			get
			{
				if (fCommodityCodeDefaulter == null)
				{
					fCommodityCodeDefaulter = new CommodityCodeDefaulter(JobDeclaration);
				}
				return fCommodityCodeDefaulter;
			}
		}
		CommodityCodeDefaulter fCommodityCodeDefaulter;

		public sealed class ResetLineValuesFromAddInfoSuspender : IDisposable
		{
			public ResetLineValuesFromAddInfoSuspender(JobComInvoiceHeader invoiceHeader)
			{
				this.invoiceHeader = invoiceHeader;
				invoiceHeader.resetLineValuesFromAddInfoSuspenderCount++;
			}

			readonly JobComInvoiceHeader invoiceHeader;

			public void Dispose()
			{
				invoiceHeader.resetLineValuesFromAddInfoSuspenderCount--;
			}
		}

		public bool IsResetLineValuesFromAddInfoSuspend => resetLineValuesFromAddInfoSuspenderCount > 0;
		int resetLineValuesFromAddInfoSuspenderCount;

		#endregion

		#region Calculated Fields For Document Wrappers

		public Money IncludedBuyingCommission
		{
			get { return GetIncludedAmountWithThisChargeType(AUChargeCodeList.Codes.BuyingCommission); }
		}

		public Money ExcludedBuyingCommission
		{
			get { return GetExcludedAmountWithThisChargeType(AUChargeCodeList.Codes.BuyingCommission); }
		}

		public Money IncludedOtherCommission
		{
			get { return GetIncludedAmountWithThisChargeType(AUChargeCodeList.Codes.OtherCommission); }
		}

		public Money ExcludedOtherCommission
		{
			get { return GetExcludedAmountWithThisChargeType(AUChargeCodeList.Codes.OtherCommission); }
		}

		#endregion

		#region Proxies to AddInfo Properties

		public ZString ZA_ORG
		{
			get { return AddInfo.ZA_ORG; }
			set { AddInfo.ZA_ORG = value; }
		}

		public RefCountryCollection ZA_ORG_List
		{
			get { return AddInfo.ZA_ORG_List; }
		}

		public ZPropertyInfo ZA_ORGInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_ORG, x => AddInfo.ZA_ORGInfo); }
		}

		public ZString ZA_PRF
		{
			get { return AddInfo.ZA_PRF; }
			set { AddInfo.ZA_PRF = value; }
		}

		public CodeDescriptionPairList ZA_PRF_List
		{
			get { return AddInfo.ZA_PRFList; }
		}

		public ZPropertyInfo ZA_PRFInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_PRF, x => AddInfo.ZA_PRFInfo); }
		}

		[List(nameof(AddInfo) + "." + nameof(AUAddInfo.Lookups) + "." + nameof(AUAddInfoLookups.CMRGSTEList))]
		public ZString ZA_GSTE
		{
			get { return AddInfo.ZA_GSTE; }
			set { AddInfo.ZA_GSTE = value; }
		}

		public ZPropertyInfo ZA_GSTEInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ZA_GSTE, x => AddInfo.ZA_GSTEInfo); }
		}

		#endregion

		#region AQIS Collections

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

		#region AQIS Commodity Codes

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

		#endregion

		#region AQIS Entity Ids

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

		#endregion

		#region AQIS Permit Ids

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

		#endregion

		#region AQIS Producer Code

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

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			AddInfo.SetAQISFieldsForSave();
		}

		#endregion

		#region Other Collections

		public CMRTariffRatePeriodCharacteristicCollection GSTExemptTariffRateCharacteristics
		{
			get
			{
				if (fGSTExemptTariffRateCharacteristics == null)
				{
					ZQuery filter = new ZQuery(CMRTariffRatePeriodCharacteristicSchema.TH_CharacteristicCode, SQLComparisonOperator.Equal, ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports));
					fGSTExemptTariffRateCharacteristics = new CMRTariffRatePeriodCharacteristicCollection(Factory, filter);
					fGSTExemptTariffRateCharacteristics.Load();
				}
				return fGSTExemptTariffRateCharacteristics;
			}
		}
		CMRTariffRatePeriodCharacteristicCollection fGSTExemptTariffRateCharacteristics;

		public CMRTreatmentRatePeriodCharacteristicCollection GSTExemptTreatmentRateCharacteristics
		{
			get
			{
				if (fGSTExemptTreatmentRateCharacteristics == null)
				{
					ZQuery filter = new ZQuery(CMRTreatmentRatePeriodCharacteristicSchema.TR_CharacteristicCode, SQLComparisonOperator.Equal, ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports));
					fGSTExemptTreatmentRateCharacteristics = new CMRTreatmentRatePeriodCharacteristicCollection(Factory, filter);
					fGSTExemptTreatmentRateCharacteristics.Load();
				}
				return fGSTExemptTreatmentRateCharacteristics;
			}
		}
		CMRTreatmentRatePeriodCharacteristicCollection fGSTExemptTreatmentRateCharacteristics;

		#endregion

		#region Quarantine Properties

		public QuarantineExDocHeader QuarantineExDocHeader => LoadOrCreateQuarantineExDocHeader();

		QuarantineExDocHeader LoadOrCreateQuarantineExDocHeader()
		{
			if (fQuarantineExDocHeader == null && IsQuarantine)
			{
				fQuarantineExDocHeader = LoadQuarantineExDocHeader(Factory);
				if (fQuarantineExDocHeader == null && !IsDeleted)
				{
					fQuarantineExDocHeader = Factory.New<QuarantineExDocHeader>();
					using (fQuarantineExDocHeader.SuspendSettingHasChanges())
					{
						fQuarantineExDocHeader.QH_JZ = PK;
					}
				}
				RegisterEditableChildObject(fQuarantineExDocHeader);
				RegisterListChangedCalledRefreshBinding(fQuarantineExDocHeader);
			}
			return fQuarantineExDocHeader;
		}
		QuarantineExDocHeader fQuarantineExDocHeader;

		QuarantineExDocHeader GetQuarantineExDocHeaderWithoutCreatingNew() => fQuarantineExDocHeader ?? LoadQuarantineExDocHeader(Factory);

		internal void ResetQuarantineExDocHeaderCache(bool delete = false)
		{
			if (fQuarantineExDocHeader != null)
			{
				UnRegisterListChangedCalledRefreshBinding(fQuarantineExDocHeader);
				UnRegisterEditableChildObject(fQuarantineExDocHeader);
				if (delete)
				{
					fQuarantineExDocHeader.Delete();
				}
				fQuarantineExDocHeader = null;
			}
		}

		public void ResetQuarantineProperties()
		{
			ResetQuarantineExDocHeaderCache(true);

			fAQISResponsiblePerson?.Delete();
			fAQISResponsiblePerson = null;
			fAQISTransitDestination?.Delete();
			fAQISTransitDestination = null;

			JobComInvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.ResetQuarantineExDocLineCache());
		}

		QuarantineExDocHeader LoadQuarantineExDocHeader(BusinessObjectFactory factory) => factory.LoadTop1<QuarantineExDocHeader>(new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

		internal void CreateQuarantineHeaderIfRequired()
		{
			LoadOrCreateQuarantineExDocHeader();
		}

		public override void Delete()
		{
			EDocPivotCollection.RemoveAndDeleteAll();
			this.DeleteChildren<QuarantineExDocHeader>(QuarantineExDocHeaderSchema.QH_JZ);
			fQuarantineExDocHeader = null;
			DocAddresses.RemoveAndDeleteAll();

			JobDeclaration?.ResetQuarantineInvoiceCache();

			try
			{
				base.Delete();
			}
			catch (RowNotInTableException ex)
			{
				ZString message = string.Format("RowNotInTableException. Last save succeeded: {0}, Last OnSaving IsInDatabase: {1}, Last Onsaved IsInDatabase {2}, Current IsInDatabase {3}",
					lastSaveSucceededForDebug == null ? "None" : lastSaveSucceededForDebug.ToString(), lastOnSavingIsInDatabaseForDebug, lastOnSavedIsInDatabaseForDebug, IsInDatabase);
				throw new RowNotInTableException(message, ex);
			}
		}

		public ZString EXDOCExporterNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Supplier_Effective != null)
				{
					result = Supplier_Effective.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, Core.Constants.CountryCodes.Australia);
				}
				return result;
			}
		}

		public ZString NEXDOCExporterNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Supplier_Effective != null)
				{
					result = Supplier_Effective.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, Core.Constants.CountryCodes.Australia);
				}
				return result;
			}
		}
		#endregion

		#region JobDocAddress

		#region AQISResponsiblePerson

		public JobDocAddress AQISResponsiblePerson
		{
			get
			{
				if (fAQISResponsiblePerson == null || fAQISResponsiblePerson.IsDeleted)
				{
					fAQISResponsiblePerson = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AQISResponsiblePerson);
				}
				return fAQISResponsiblePerson;
			}
		}

		JobDocAddress fAQISResponsiblePerson;

		#endregion

		#region AQISTransitDestination

		public JobDocAddress AQISTransitDestination
		{
			get
			{
				if (fAQISTransitDestination == null || fAQISTransitDestination.IsDeleted)
				{
					fAQISTransitDestination = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AQISTransitDestination);
				}
				return fAQISTransitDestination;
			}
		}
		JobDocAddress fAQISTransitDestination;

		#endregion

		#region EUContactPerson

		public JobDocAddress AQISEUContactPerson
		{
			get
			{
				if (fAQISEUContactPerson == null || fAQISEUContactPerson.IsDeleted)
				{
					fAQISEUContactPerson = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AQISEUContactPerson);
				}
				return fAQISEUContactPerson;
			}
		}
		JobDocAddress fAQISEUContactPerson;

		#region IDocAddresses Members

		#region CanDeleteAddress

		protected override bool CanDeleteAddressCore(JobDocAddress docAddress)
		{
			return true;
		}

		#endregion

		protected override SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress)
		{
			return null;
		}
		#region PiggyBackedDocAddressValidation

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobComInvoiceHeaderJobDocAddressValidation(addressToValidate, this);
		}

		#endregion

		#region SupportedAddressTypes

		protected override DocAddressType[] SupportedAddressTypesCore() =>
			JobDeclaration?.IsQuarantine ?? false
				? AQISDocAddressTypes
				: Array.Empty<DocAddressType>();

		public bool IsAQISDocAddressType(DocAddressType docAddressType) => AQISDocAddressTypes.Contains(docAddressType);

		DocAddressType[] AQISDocAddressTypes => aqisDocAddressTypes ??= new[]
		{
			DocAddressType.AQISResponsiblePerson,
			DocAddressType.AQISTransitDestination,
			DocAddressType.AQISEUContactPerson,
			DocAddressType.AQISLoadingEstablishment,
		};
		DocAddressType[] aqisDocAddressTypes;

		#endregion

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return Factory.GetCachedValue($"AU.JobComInvoiceHeader.GetDocAddressRequirement_{addressType}", () =>
			{
				var result = addressType switch
				{
					DocAddressType.AQISLoadingEstablishment => new AQISLoadingEstablishmentAddressRequirement(),
					_ => base.GetDocAddressRequirement(addressType)
				};
				return result;
			});
		}

		#endregion

		#endregion

		#region AQISEUPlaceOfDestination

		public AUJobDocAddress AQISEUPlaceOfDestination
		{
			get
			{
				if (fAQISEUPlaceOfDestination == null || fAQISEUPlaceOfDestination.IsDeleted)
				{
					fAQISEUPlaceOfDestination = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AQISEUPlaceOfDestination) as AUJobDocAddress;
				}
				return fAQISEUPlaceOfDestination;
			}
		}
		AUJobDocAddress fAQISEUPlaceOfDestination;

		#endregion

		#region AQISLoadingEstablishmentLocation

		AUJobDocAddress aqisLoadingEstablishmentLocation;

		[ResourceStringData("Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader|AQISLoadingEstablishmentLocation", Caption = "Location")]
		public AUJobDocAddress AQISLoadingEstablishmentLocation
		{
			get
			{
				if (aqisLoadingEstablishmentLocation == null || aqisLoadingEstablishmentLocation.IsDeleted)
				{
					aqisLoadingEstablishmentLocation = (AUJobDocAddress)DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AQISLoadingEstablishment);
					aqisLoadingEstablishmentLocation.ShouldClearAddressFieldsWhenOverride = true;
				}
				return aqisLoadingEstablishmentLocation;
			}
		}

		void CleanAQISLoadingEstablishmentLocation()
		{
			if (aqisLoadingEstablishmentLocation == null)
			{
				aqisLoadingEstablishmentLocation = (AUJobDocAddress)DocAddresses.FindByDocAddressType(DocAddressType.AQISLoadingEstablishment);
			}
			if (aqisLoadingEstablishmentLocation != null && !aqisLoadingEstablishmentLocation.IsDeleted)
			{
				aqisLoadingEstablishmentLocation.E2_AddressOverride = false;
				aqisLoadingEstablishmentLocation.E2_OA_Address = ZGuid.Empty;
				aqisLoadingEstablishmentLocation.E2_GovRegNum = ZString.Empty;
			}
		}

		#endregion

		protected override JobDocAddressDependentCollection GetDocAddressesCore()
		{
			var docAddresses = new AUJobDocAddressDependentCollection(this);
			docAddresses.Load();
			RegisterEditableChildObject(docAddresses);
			return docAddresses;
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		protected override ShortSequenceNumberGenerator InvoiceLineLineNumberGeneratorCore
		{
			get
			{
				ShortSequenceNumberGenerator result = null;
				if (JobDeclaration?.IsQuarantine ?? false)
				{
					result = new QuarantineSequenceNumberGenerator(this);
				}
				return result ?? new ShortSequenceNumberGenerator(this);
			}
		}

		bool QuarantineExDocHeaderHasMessages => GetQuarantineExDocHeaderWithoutCreatingNew()?.Messages.Any() ?? false;

		public override bool CanDelete => base.CanDelete && !QuarantineExDocHeaderHasMessages;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && QuarantineExDocHeaderHasMessages)
				{
					result = ResString.GetMultilingualString("323DEED0-E656-4E84-8CB4-E64A34A9617F", "Messages exist against the job so this cannot be deleted.");
				}
				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceHeaderFetchStrategy(this);
		}

		#region ICusStorageDocPivotParent Members

		[ChildEditable(true)]
		public CusStorageDocPivotCollection EDocPivotCollection
		{
			get
			{
				if (eDocPivotCollection == null)
				{
					eDocPivotCollection = new CusStorageDocPivotCollection(this);
					eDocPivotCollection.Load();
					RegisterEditableChildObject(eDocPivotCollection);
				}

				return eDocPivotCollection;
			}
		}
		CusStorageDocPivotCollection eDocPivotCollection;

		bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

		CusStorageDocPivotCollection ICusStorageDocPivotParent.EDocPivotCollection => EDocPivotCollection;

		IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections
		{
			get
			{
				foreach (var eDocCollection in EDocsHelper.GetEDocCollections(Entries.OfType<IDocManagerSupport>().ToArray()))
				{
					yield return eDocCollection;
				}

				var declaration = JobDeclaration;

				if (declaration != null)
				{
					if (declaration.IsPersistent)
					{
						foreach (var eDocCollection in EDocsHelper.GetEDocCollections(declaration, declaration.Shipment))
						{
							yield return eDocCollection;
						}
					}
					else
					{
						foreach (var eDocCollection in EDocsHelper.GetEDocCollections(this))
						{
							yield return eDocCollection;
						}
					}
				}
			}
		}

		Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivot);

		void ICusStorageDocPivotTypeSupporter.ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				EDocPivotCollection.Reload(true);
			}
		}

		ZString ICusStorageDocPivotTypeSupporter.HumanReadableName => Res.GetString("4245ace1-ac11-4794-8830-833d3c2a9b31", "invoice {0}", JZ_InvoiceNumber);

		#endregion
	}
}
