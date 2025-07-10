using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Business
{
	public partial class JobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.A;
		}

		[ChildEditable(true)]
		public ComprehensiveValuationCollection ComprehensiveValuations
		{
			get
			{
				if (comprehensiveValuations == null)
				{
					comprehensiveValuations = new ComprehensiveValuationCollection(this);
					comprehensiveValuations.Load();

					RegisterEditableChildObject(comprehensiveValuations);
				}
				return comprehensiveValuations;
			}
		}

		ComprehensiveValuationCollection comprehensiveValuations;

		[List(nameof(JobComInvoiceHeader.Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationTypeCodeList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_ValuationCode", Caption = "Valuation Type")]
		public override ZString JZ_ValuationCode { get => base.JZ_ValuationCode; set => base.JZ_ValuationCode = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_IncoTerm", Caption = "Incoterm")]
		public override ZString JZ_IncoTerm { get => base.JZ_IncoTerm; set => base.JZ_IncoTerm = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_ValuationDateOverride", Caption = "Valuation Date", MediumCaption = "Valuation Date", ShortCaption = "Val. Date", FullDescription = "If not empty, this value will be used by the system to determine exchange rates.")]
		public override ZDateTime JZ_ValuationDateOverride
		{
			get => base.JZ_ValuationDateOverride;
			set
			{
				base.JZ_ValuationDateOverride = value;
				if (!IsCopying && !IsValidationSuspended)
				{
					InvoiceLines.ForEach(x => x.MarkAsNeedingValidation());
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_ElectronicInvoiceReceiptNumber", Caption = "Electronic Invoice Reception Number")]
		public override ZString JZ_ElectronicInvoiceReceiptNumber { get => base.JZ_ElectronicInvoiceReceiptNumber; set => base.JZ_ElectronicInvoiceReceiptNumber = value; }

		[List(nameof(JobComInvoiceHeader.Lookups) + "." + nameof(JobComInvoiceHeaderLookups.InvoiceTypes))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_InvoiceType", Caption = "Invoice Type")]
		public override ZString JZ_InvoiceType { get => base.JZ_InvoiceType; set => base.JZ_InvoiceType = value; }

		[List(nameof(JobComInvoiceHeader.Lookups) + "." + nameof(JobComInvoiceHeaderLookups.InvoiceAmountType))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_InvoiceAmountType", Caption = "Invoice Amount Type", MediumCaption = "Inv. Amt. Type", ShortCaption = "Amt. Type")]
		public override ZString JZ_InvoiceAmountType { get => base.JZ_InvoiceAmountType; set => base.JZ_InvoiceAmountType = value; }

		[List(nameof(JobComInvoiceHeader.Lookups) + "." + nameof(JobComInvoiceHeaderLookups.FreightTypes))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_FreightType", Caption = "Freight Type")]
		public override ZString JZ_FreightType { get => base.JZ_FreightType; set => base.JZ_FreightType = value; }

		[List(nameof(JobComInvoiceHeader.Lookups) + "." + nameof(JobComInvoiceHeaderLookups.InsuranceTypes))]
		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_InsuranceType", Caption = "Insurance Type", ShortCaption = "Ins. Type")]
		public override ZString JZ_InsuranceType { get => base.JZ_InsuranceType; set => base.JZ_InsuranceType = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_ComprehensiveInsuranceNumber", Caption = "Comprehensive Insurance Number", MediumCaption = "Com. Insurance Number", ShortCaption = "Com. Ins. No.")]
		public override ZString JZ_ComprehensiveInsuranceNumber { get => base.JZ_ComprehensiveInsuranceNumber; set => base.JZ_ComprehensiveInsuranceNumber = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.JobComInvoiceHeader|JZ_InvoiceAmount", Caption = "Invoice Amount", MediumCaption = "Inv. Amount", ShortCaption = "Inv. Amt.")]
		[DecimalPlaces(nameof(InvoiceAmountDecimalPlaces))]
		public override ZDecimal JZ_InvoiceAmount { get => base.JZ_InvoiceAmount; set => base.JZ_InvoiceAmount = value; }

		[ResourceStringData("BA4AF3E1-0275-4286-A888-C3CBDD53C6E0", ShortCaption = "ARV 1", Caption = "Advance Ruling on Valuation 1")]
		public override ZString JZ_AdvanceRulingOnValuation1
		{
			get => base.JZ_AdvanceRulingOnValuation1;
			set => base.JZ_AdvanceRulingOnValuation1 = value;
		}

		[ResourceStringData("BF1849D8-E1A0-4F47-B412-64D0246BADC8", ShortCaption = "ARV 2", Caption = "Advance Ruling on Valuation 2")]
		public override ZString JZ_AdvanceRulingOnValuation2
		{
			get => base.JZ_AdvanceRulingOnValuation2;
			set => base.JZ_AdvanceRulingOnValuation2 = value;
		}

		int InvoiceAmountDecimalPlaces => JZ_RX_NKInvoice_Currency == CurrencyCodes.Japan ? 0 : 2;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => this.GetCountryContext();

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var entryInstructionsWithValidDateForDuty = CusEntryInstructions.Where(x => !x.CEI_DateForDuty.IsEmpty && x.CEI_DateForDuty.IsValid);
				if (entryInstructionsWithValidDateForDuty.Any())
				{
					return entryInstructionsWithValidDateForDuty.FirstOrDefault().CEI_DateForDuty;
				}
				else
				{
					return base.EffectiveValuationDateCore;
				}
			}
		}

		public ImmutableHashSet<ZString> DeclarationTypes => Factory.GetValue(ref declarationTypes, () => CusEntryInstructions.Select(e => e.CEI_Style).ToImmutableHashSet());
		CachedProperty<ImmutableHashSet<ZString>> declarationTypes;
	}
}
