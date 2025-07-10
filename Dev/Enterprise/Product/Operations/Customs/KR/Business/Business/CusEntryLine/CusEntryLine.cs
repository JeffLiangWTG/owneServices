using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[SystemDefinedValues]
	public partial class CusEntryLine : AutoKRCusEntryLine, IEDIMessageCollectionProvider, ILineOrProduct
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusEntryLine.Schema
		{
			public const int NetWeightDecimalPlaces = 3;
		}
		#region GenAddOn
		public static class GenAddOnColumnConstants
		{
			public const string CL_HighestInvoiceLineSequenceNo = "CL_HighestInvoiceLineSequenceNo";
			public const string CL_HighestImmediateDeliveryNo = "CL_HighestImmediateDeliveryNo";
			public const string CL_HighestPreviousExpDecLineNo = "CL_HighestPreviousExpDecLineNo";
			public const string CL_HighestNonGADetailNo = "CL_HighestNonGADetailNo";
		}
		#endregion

		[ResourceStringData("7E9668FC-F14C-4D73-82FD-BD9BA1417F85", Caption = "Net Weight(KG)")]
		[DecimalPlaces(DecimalPlacesConstants.Weight)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZDecimal NetWeightInKG => Math.Round(Core.Constants.Weight.Convert(EffectiveNetWeight.Amount, EffectiveNetWeight.Unit, Core.Constants.Weight.Kilograms), Schema.NetWeightDecimalPlaces);

		public ZDecimal NetWeightInGrams => Core.Constants.Weight.Convert(EffectiveNetWeight.Amount, EffectiveNetWeight.Unit, Core.Constants.Weight.Grams);

		public JobComInvoiceLine FirstInvoiceLine => InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_SequenceNumber).FirstOrDefault();

		public override ZGuid CL_CH
		{
			get => base.CL_CH;
			set
			{
				var oldValue = CL_CH;
				base.CL_CH = value;
				if (oldValue != value)
				{
					CL_FTASequenceNumber = 0;
				}
			}
		}

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("11513971-F2AA-447D-90E1-61A390CB6D40", Caption = "Customs Value (KRW)")]
		[ResourceStringData("4DBB148F-BE3B-4930-AC0D-48275ED2E5A0", Caption = "Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("20572E8E-648C-40ED-BD94-E7F6B7AFD77F", Caption = "Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ResourceStringData("0CA5FB2F-8B27-4373-990B-256B2A6F3651", Caption = "Customs Value (KRW)", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		public override ZDecimal CL_CustomsValue
		{
			get => base.CL_CustomsValue;
			set => base.CL_CustomsValue = value;
		}

		[ResourceStringData("D3CDCBB4-C1F8-47B7-87CE-2868801CBDCA", Caption = "Entry Line No.")]
		[ReadOnly(true)]
		public override ZShort CL_LineNumber
		{
			get => base.CL_LineNumber;
			set => base.CL_LineNumber = value;
		}

		public override ZString CL_CustomsPostedStatus
		{
			get => base.CL_CustomsPostedStatus;
			set
			{
				var oldValue = CL_CustomsPostedStatus;
				base.CL_CustomsPostedStatus = value;
				if (!IsCopying && oldValue != CL_CustomsPostedStatus && !IsValidationSuspended)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("E21B2FDB-AEA2-4550-89AA-E18D23B1C5C8", Caption = "Invoice Description")]
		public override ZString CL_Description { get => base.CL_Description; set => base.CL_Description = value; }

		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		[ResourceStringData("8E12D47A-19B4-4311-9ACE-0E7245DF624D", Caption = "Customs Value (USD)")]
		[ResourceStringData("D3CBEF42-5858-4476-B6E8-BAA3CFE313B2", Caption = "Customs Value (USD)", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ResourceStringData("EEB459DC-56D9-4173-AA78-D34172CECD92", Caption = "Customs Value (USD)", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		public ZDecimal CustomsValueUSD
		{
			get
			{
				var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
				return CurrencyConverter.ConvertExact(CustomsValue, usdCurrency).Amount.Truncate();
			}
		}

		[DecimalPlaces(DecimalPlacesConstants.NoOfPacks)]
		[ResourceStringData("28083960-D2A2-4DAF-A46A-C86828D75296", Caption = "Packages")]
		public ZDecimal NoOfPacks
		{
			get
			{
				if (noOfPacks == null)
				{
					noOfPacks = new CachedProperty<ZDecimal>(Factory, delegate
					{
						return GetNoOfPacks();
					}
					);
				}
				return noOfPacks.Value;
			}
		}
		CachedProperty<ZDecimal> noOfPacks;

		public override int CustomsQuantityDecimalPlaces => 0;

		[ResourceStringData("74B41024-B2FE-4861-AB59-CC98135EF2D7", Caption = "Description")]
		public ZString TariffDescription => RandomLine.UniversalTariff?.ZZ1_Description ?? ZString.Empty;

		[ResourceStringData("3D3777C5-E7E8-4698-97E6-2FC02F67C955", Caption = "Goods Origin")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.Countries))]
		public ZString CountryOfOriginCode => RandomLine.JI_CountryOfOrigin;

		[ResourceStringData("AC43CAA9-5F88-4167-A9AC-CE12FF72371D", Caption = "Pack Type")]
		public ZString PackType => RandomLine.JI_PackType;

		[ResourceStringData("3C468A88-3FCE-470B-9FAF-9C2750E248D4", Caption = "Supporting Document No.")]
		public ZString SupportingDocumentNo => RandomLine?.SupportingDocumentReferenceNumber ?? ZString.Empty;

		[ResourceStringData("EA1DD2EF-03DD-4C83-856D-D69EB69673C3", Caption = "Supporting Document Type")]
		public ZString SupportingDocumentType => RandomLine?.SupportingDocumentCode ?? ZString.Empty;

		[ResourceStringData("397940D6-6563-4941-A5D4-CBA73ABC1865", Caption = "Supporting Document Type Desc.")]
		public ZString SupportingDocumentTypeDescription => Factory.GetCachedValue<LocalExportDocumentTypeList>().GetDescriptionFromCode(SupportingDocumentType);

		[DecimalPlaces(nameof(InvoiceQuantityDecimalPlaces))]
		[ResourceStringData("65A6858D-3650-44FB-BF6F-44291832E646", Caption = "Invoice Qty")]
		public new ZDecimal InvoiceQuantity => Utilities.Round(base.InvoiceQuantity, InvoiceQuantityDecimalPlaces);
		int InvoiceQuantityDecimalPlaces
		{
			get
			{
				var result = 0;
				if (Header != null)
				{
					result = Header.IsLocalExport ? DecimalPlacesConstants.LocalExportInvoiceQuantity : DecimalPlacesConstants.InvoiceQuantity;
				}
				return result;
			}
		}

		[ResourceStringData("85849B5C-1F15-4361-8FDB-63A3CBBCFAD2", Caption = "UQ")]
		public new ZString InvoiceUQ => base.InvoiceUQ;

		public ZShort KR_HighestInvoiceLineSequenceNo
		{
			get { return this.GetSystemDefinedValue<ZShort>(GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo); }
			set
			{
				var oldValue = KR_HighestInvoiceLineSequenceNo;
				this.SetSystemDefinedValue(GenAddOnColumnConstants.CL_HighestInvoiceLineSequenceNo, value);
			}
		}

		public ZShort KR_HighestImmediateDeliveryNo
		{
			get { return this.GetSystemDefinedValue<ZShort>(GenAddOnColumnConstants.CL_HighestImmediateDeliveryNo); }
			set
			{
				var oldValue = KR_HighestImmediateDeliveryNo;
				this.SetSystemDefinedValue(GenAddOnColumnConstants.CL_HighestImmediateDeliveryNo, value);
			}
		}

		public ZShort KR_HighestNonGADetailNo
		{
			get { return this.GetSystemDefinedValue<ZShort>(GenAddOnColumnConstants.CL_HighestNonGADetailNo); }
			set
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.CL_HighestNonGADetailNo, value);
			}
		}

		public ZShort KR_HighestPreviousExpDecLineNo
		{
			get { return this.GetSystemDefinedValue<ZShort>(GenAddOnColumnConstants.CL_HighestPreviousExpDecLineNo); }
			set
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.CL_HighestPreviousExpDecLineNo, value);
			}
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter() => new TariffFormatter();

		protected override ZDecimal GetGSTVATAmountCore() => Fees.GetTotalAmount(Core.Constants.Customs.CusEntryFeeTypes.VAT, false);
		protected override ZDecimal GetDutyAmountCore() => Fees.GetTotalAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, false);

		protected ZDecimal GetNoOfPacks()
		{
			ZDecimal result = 0m;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				result += invoiceLine.JI_NoOfPacks;
			}
			return result;
		}

		protected override ZDecimal GetCustomsQuantity()
		{
			ZDecimal result = 0m;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				result += invoiceLine.JI_CustomsQuantity;
			}
			return Utilities.Round(result, CustomsQuantityDecimalPlaces);
		}

		[ResourceStringData("115201D3-2EEB-4664-B104-8F779725204A", Caption = "Duty Rate Code")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.DutyRateCodeList))]
		public ZString DutyRateCode => RandomLine.DutyRateCode;

		[ResourceStringData("ADC3A5FB-CE62-4F96-A99F-BFBEBE5308BB", Caption = "Duty Rate Code Desc.")]
		public ZString DutyRateCodeDescription => RandomLine.DutyRateCodeDescription;

		[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.PreferenceCodeList))]
		[ResourceStringData("ADED922D-0229-40A2-A493-3AECF5BB8A5C", Caption = "Preference Code")]
		public ZString PreferenceCode => RandomLine.JI_PrimaryPreference;

		[ResourceStringData("5F9A6AA0-435C-45F1-8509-A75551432DD4", Caption = "Preference Code Desc.")]
		public ZString PreferenceCodeDescription => RandomLine.PreferenceCodeDescription;

		[ResourceStringData("76325B97-2245-464C-A63D-05BEE58BA86B", Caption = "Duty Rate")]
		[DecimalPlaces(DecimalPlacesConstants.DutyRate)]
		public ZDecimal DutyRate => Fees.GetElementWithThisCode(ChargeTypeList.Codes.Duty)?.CF_Rate ?? ZDecimal.Zero;

		[ResourceStringData("94A36FC8-FDD9-439E-8680-BD505EB4C505", Caption = "Duty Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal DutyFeeAmount => base.DutyAmount;

		[ResourceStringData("C816859B-3170-4DE2-AD44-52E92C6500BD", Caption = "Duty Reduction Rate")]
		[DecimalPlaces(DecimalPlacesConstants.DutyRate)]
		public ZDecimal DutyReductionRate => RandomLine.DutyReductionRate;

		[ResourceStringData("0BCBDB56-C971-4A1D-931E-DA4F33DD212A", Caption = "Duty Reduction Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public override ZDecimal CL_DutyReductionAmount { get => base.CL_DutyReductionAmount; set => base.CL_DutyReductionAmount = value; }

		[ResourceStringData("39070A4B-D0D2-4FC1-831F-ECF3B8D4EED3", Caption = "VAT Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal VATAmount => base.GSTVATAmount;

		[ResourceStringData("351AE843-F2FE-43A7-A178-A02195B4B8E8", Caption = "Domestic Tax Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal DomesticTaxAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				result += SpecialConsumptionTaxAmount;
				result += LiquorTaxAmount;
				result += TransportationTaxAmount;

				return result;
			}
		}

		[ResourceStringData("8F22EA96-20B7-48BD-8B13-C2EF978AC2E0", Caption = "Education Tax Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal EducationTaxAmount => Fees.GetTotalAmount(ChargeTypeList.Codes.EducationTax, false);

		[ResourceStringData("FB18BF8E-50F7-4A74-BBAC-C2F7416A53C9", Caption = "Agriculture Tax Amount")]
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal AgricultureTaxAmount => Fees.GetTotalAmount(ChargeTypeList.Codes.AgricultureTax, false);
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal SpecialConsumptionTaxAmount => Fees.GetTotalAmount(ChargeTypeList.Codes.SpecialConsumptionTax, false);
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal LiquorTaxAmount => Fees.GetTotalAmount(ChargeTypeList.Codes.LiquorTax, false);
		[DecimalPlaces(DecimalPlacesConstants.DutyTaxFee)]
		public ZDecimal TransportationTaxAmount => Fees.GetTotalAmount(ChargeTypeList.Codes.TransportationTax, false);

		[ResourceStringData("0DFE4ECD-A1A2-4B7B-9584-26D091D6B97C", Caption = "Value For VAT")]
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public override ZDecimal CL_ValueForVAT { get => base.CL_ValueForVAT; set => base.CL_ValueForVAT = value; }

		[ResourceStringData("40B44BF7-6569-4195-9865-30E20B21BFEF", Caption = "VAT Exemption Value")]
		[DecimalPlaces(DecimalPlacesConstants.CustomsValue)]
		public override ZDecimal CL_ValueExemptForVAT { get => base.CL_ValueExemptForVAT; set => base.CL_ValueExemptForVAT = value; }

		public ZDecimal CertificateOfOriginTotalNetWeightInKG => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.CertificateOfOriginTotalNetWeightInKG);

		[ChildEditable(true)]
		public ImmediateDeliveryCollection ImmediateDeliveries
		{
			get
			{
				if (immediateDeliveries == null)
				{
					immediateDeliveries = new ImmediateDeliveryCollection(this);
					immediateDeliveries.Load();
					RegisterEditableChildObject(immediateDeliveries);
				}
				return immediateDeliveries;
			}
		}
		ImmediateDeliveryCollection immediateDeliveries;

		[ChildEditable(true)]
		public NonGADetailCollection NonGADetailCollection
		{
			get
			{
				if (nonGADetailCollection == null)
				{
					nonGADetailCollection = new NonGADetailCollection(this);
					nonGADetailCollection.Load();
					RegisterEditableChildObject(nonGADetailCollection);
				}
				return nonGADetailCollection;
			}
		}
		NonGADetailCollection nonGADetailCollection;

		[ChildEditable(true)]
		public PreviousExpDecLineCollection PreviousExpDecLineCollection
		{
			get
			{
				if (previousExpDecLineCollection == null)
				{
					previousExpDecLineCollection = new PreviousExpDecLineCollection(this);
					previousExpDecLineCollection.Load();
					RegisterEditableChildObject(previousExpDecLineCollection);
				}
				return previousExpDecLineCollection;
			}
		}
		PreviousExpDecLineCollection previousExpDecLineCollection;

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => Header.Messages;

		bool ILineOrProduct.IsExport => Header.IsExport;
		bool ILineOrProduct.IsImport => Header.IsImport;
		ZDateTime ILineOrProduct.DeclarationDate => Header.CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
		bool ILineOrProduct.IsIssueDateRelevant => false;
		bool ILineOrProduct.IsReferenceNumberRelevant => false;
		ZString ILineOrProduct.Tariff => RandomLine.JI_Tariff;
		GAApprovalCollection ILineOrProduct.GAApprovalDataCollection => null;
		bool ILineOrProduct.IsValidationEnabled => false;
		TariffView ILineOrProduct.UniversalTariff => null;
		HSExtensionCodeCollection ILineOrProduct.HSExtensionCodeCollection => null;

		protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var separator = "|";
			base.DoMergeInvoiceLine(baseInvoiceLine);
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var immediateDeliveryNumbers = invoiceLine.ImmediateDeliveries.Select(x => x.CY_Data).Distinct().ToList();
			foreach (var immediateDeliveryNumber in immediateDeliveryNumbers)
			{
				ImmediateDeliveries.AddNewIfRequired(immediateDeliveryNumber);
			}
			var nonGADetails = invoiceLine.NonGADetailCollection.Cast<NonGADetail>().DistinctBy(x => string.Join(separator, x.KeyFields));
			foreach (var nonGADetail in nonGADetails)
			{
				NonGADetailCollection.AddNewIfRequired(nonGADetail);
			}

			var previousExpDecLines = invoiceLine.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>();
			foreach (var previousExpDecLine in previousExpDecLines)
			{
				PreviousExpDecLineCollection.AddNewIfRequired(previousExpDecLine);
			}
		}
		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);
		}
	}
}
