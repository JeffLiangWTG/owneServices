using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobComInvoiceHeader : AutoCNJobComInvoiceHeader, Integration.Customs.CN.IJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString LocalCurrencyCodeCore => JobDeclaration.LocalCurrencyConstantCode;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JZ_SpecialRelationshipConfirm = ConfirmationTypeList.Codes.No;
			JZ_PriceAffectConfirm = ConfirmationTypeList.Codes.No;
			JZ_PaymentOfRoyaltyConfirm = ConfirmationTypeList.Codes.No;
			JZ_Calc_FormulaPricingConfirm = JZ_Calc_TemporaryPricingConfirm = ConfirmationTypeList.Codes.Uncertain;
		}

		#region Schema
		public new partial class Schema : BaseJobComInvoiceHeader.Schema
		{
			public const string ContractNumbersAsString = "ContractNumbersAsString";
			public const string JZ_Calc_FormulaPricingConfirm = nameof(JZ_Calc_FormulaPricingConfirm);
			public const string JZ_Calc_TemporaryPricingConfirm = nameof(JZ_Calc_TemporaryPricingConfirm);
		}
		#endregion

		#region Override Properties

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.JZ_IncoTerm_List))]
		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					base.JZ_IncoTerm = value;
				}
			}
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

		public override ZDateTime JZ_InvoiceDate
		{
			get => base.JZ_InvoiceDate;
			set
			{
				var oldValue = JZ_InvoiceDate;
				base.JZ_InvoiceDate = value;
				if (!IsCopying && oldValue != JZ_InvoiceDate && JobDeclaration != null)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override void RefreshDefaultsWhenInvoiceAttachedToDeclarationCore()
		{
			base.RefreshDefaultsWhenInvoiceAttachedToDeclarationCore();
			if (JobDeclaration.CustomsEntryInstructions.Count == 1)
			{
				var pk = JobDeclaration.CustomsEntryInstructions[0].PK;
				foreach (var line in InvoiceLines)
				{
					((JobComInvoiceLine)line).JI_CEI = pk;
				}
			}
		}

		#endregion

		#region Confirmations

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|JZ_PaymentOfRoyaltyConfirm", Caption = "Payment of Royalty Confirm")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ConfirmationTypeList))]
		public override ZString JZ_PaymentOfRoyaltyConfirm
		{
			get => base.JZ_PaymentOfRoyaltyConfirm;
			set
			{
				var oldValue = JZ_PaymentOfRoyaltyConfirm;
				base.JZ_PaymentOfRoyaltyConfirm = value;
				if (!IsCopying && JZ_PaymentOfRoyaltyConfirm != oldValue)
				{
					if (JZ_PaymentOfRoyaltyConfirm == ConfirmationTypeList.Codes.Yes &&
						GroupHeader != null &&
						JobDeclaration != null &&
						JobDeclaration.IsImport &&
						!JobDeclaration.WillGenerateBothEntries &&
						!this.FindRoyaltyChargesOnInvoiceOrGroup().Any())
					{
						GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Royalty);
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|JZ_PriceAffectConfirm", Caption = "Price Affect Confirm")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ConfirmationTypeList))]
		public override ZString JZ_PriceAffectConfirm
		{
			get => base.JZ_PriceAffectConfirm;
			set => base.JZ_PriceAffectConfirm = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|JZ_SpecialRelationshipConfirm", Caption = "Special Relationship Confirm")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ConfirmationTypeList))]
		public override ZString JZ_SpecialRelationshipConfirm
		{
			get => base.JZ_SpecialRelationshipConfirm;
			set => base.JZ_SpecialRelationshipConfirm = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ConfirmationTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|JZ_Calc_FormulaPricingConfirm", Caption = "Formula Pricing Confirm")]
		public ZString JZ_Calc_FormulaPricingConfirm
		{
			get => GetPricingConfirmFromValuationCode(0);
			set
			{
				SetPricingConfirmToValuationCode(value, JZ_Calc_TemporaryPricingConfirm);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFormulaPricingConfirm();
					Validation.ValidateTemporaryPricingConfirm();
				}
				JZ_Calc_FormulaPricingConfirmInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo JZ_Calc_FormulaPricingConfirmInfo => GetZPropertyInfo(Schema.JZ_Calc_FormulaPricingConfirm);

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ConfirmationTypeList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|JZ_Calc_TemporaryPricingConfirm", Caption = "Temporary Pricing Confirm")]
		public ZString JZ_Calc_TemporaryPricingConfirm
		{
			get => GetPricingConfirmFromValuationCode(1);
			set
			{
				SetPricingConfirmToValuationCode(JZ_Calc_FormulaPricingConfirm, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateTemporaryPricingConfirm();
				}
				JZ_Calc_TemporaryPricingConfirmInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo JZ_Calc_TemporaryPricingConfirmInfo => GetZPropertyInfo(Schema.JZ_Calc_TemporaryPricingConfirm);

		ZString GetPricingConfirmFromValuationCode(int index) => JZ_ValuationCode.SubstringSafe(index, 1).Trim();

		void SetPricingConfirmToValuationCode(ZString formulaPricingConfirm, ZString temporaryPricingConfirm)
		{
			JZ_ValuationCode = formulaPricingConfirm.Left(1).PadRight(1, ' ') + temporaryPricingConfirm.Left(1);
		}

		#endregion

		#region Contract Numbers

		[BusinessObjectTestExclude]
		public ZString ContractNumbersAsString
		{
			get => ContractNumbers.ContractNumbersAsString;
			set => ContractNumbers.ContractNumbersAsString = value;
		}

		public ZPropertyInfo ContractNumbersAsStringInfo => GetZPropertyInfo(nameof(ContractNumbersAsString));

		[ChildEditable]
		public JobComInvoiceHeaderContractCollection ContractNumbers
		{
			get
			{
				if (fContractNumbers == null)
				{
					fContractNumbers = new JobComInvoiceHeaderContractCollection(this);
					fContractNumbers.ContractNumbersChanged += FContractNumbers_ContractNumebrsChanged;
					RegisterEditableChildObject(fContractNumbers);
				}
				return fContractNumbers;
			}
		}

		JobComInvoiceHeaderContractCollection fContractNumbers;

		void FContractNumbers_ContractNumebrsChanged(object sender, EventArgs e)
		{
			ContractNumbersAsStringInfo.RefreshBinding();
			if (!IsValidationSuspended)
			{
				Validation.ValidateContractNumbersAsString();
			}
		}

		#endregion

		#region IncoTermConverter
		public new IncoTermConverter IncoTermConverter => (IncoTermConverter)base.IncoTermConverter;

		protected override Customs.Business.IncoTermConverter CreateNewIncoTermConverter()
		{
			return new IncoTermConverter();
		}
		#endregion

		public ZString EffectiveMarksAndNumbers => JZ_MarksAndNumbers.IsEmpty && JobDeclaration != null ? JobDeclaration.JE_MarksAndNumbers : JZ_MarksAndNumbers;

		public bool AnyLineHasFormulaPricingRecordNumber => Factory.GetCached(ref fAnyLineHasFormulaPricingRecordNumber, () => InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.FormulaPricingRecordNumber.IsEmpty));
		CachedProperty<bool> fAnyLineHasFormulaPricingRecordNumber;

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new Strategy(this);

		class Strategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
		{
			public Strategy(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(JobComInvoiceHeaderRefsSchema.J2_JZ, BusinessObject.PK);
				Factory.AddRefCusCodeListFetchHintIfNotEmpty(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, BusinessObject.JZ_RX_NKInvoice_Currency, BusinessObject.EffectiveValuationDate);
			}
		}

		#endregion
	}
}
