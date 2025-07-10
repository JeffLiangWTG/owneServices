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
using Enterprise.Customs.Common;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.GB.Business.GBCommonConstants;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobComInvoiceLine : EU.Business.Declaration.JobComInvoiceLine
		, Integration.Customs.GB.IJobComInvoiceLine, ITaxAndDocsProvider
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Schema

		public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
		{
			public const string JI_Calc_InstructionDisplaySequence = nameof(JobComInvoiceLine.JI_Calc_InstructionDisplaySequence);
		}

		#endregion

		protected override bool UseUniversalConditionCheck => true;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

		protected override Type InvoiceHeaderType => typeof(JobComInvoiceHeader);

		protected override AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

		protected override EU.Business.Declaration.AddInfoJobComInvoiceLineLookups GetAddInfoJobComInvoiceLineLookupsCore(AddInfoJobComInvoiceLine addInfo)
			=> new AddInfoJobComInvoiceLineLookups(addInfo);

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
			=> Declaration?.ApplicationExtender?.GetJobComInvoiceLineLookups(this) ?? new JobComInvoiceLineLookups(this);

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.UnitedKingdom;
		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public override ZString JI_Procedure
		{
			get { return base.JI_Procedure; }
			set
			{
				base.JI_Procedure = value;
				Declaration?.ApplicationExtender?.GetInvoiceLineHelper(this)?.HandleSettingOfCPC(JI_ProcedureInfo);
			}
		}

		[ResourceStringData("4DDB0B18-964C-4B86-8B54-A16C32DA7AFB", Caption = "[37] CPC", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("F8ECA4F5-246A-4738-8F1A-338C5376996C", Caption = "[UCC 1/10 & 1/11] Procedure")]
		public override ZString JI_FormattedProcedure
		{
			get
			{
				return Declaration?.IsUCCCompliant ?? false ?
					DisplayFormatForProcedure(JI_Procedure) :
					JI_Procedure;
			}
			set => JI_Procedure = FormatForProcedure(value).Left(JI_ProcedureInfo.MaxLength);
		}

		static ZString FormatForProcedure(ZString unformattedProcedure)
		{
			return unformattedProcedure.KeepAlphanumericCharacters().Left(7);
		}

		static ZString DisplayFormatForProcedure(ZString unformattedProcedure)
		{
			ZString newProcedure = unformattedProcedure.KeepAlphanumericCharacters();
			ZString dottedProcedure = newProcedure.IsEmpty ? "" : newProcedure.SubstringSafe(0, 2) + " " + newProcedure.SubstringSafe(2, 2) + " " + newProcedure.SubstringSafe(4, 3).Trim();
			return dottedProcedure.Trim(new char[] { ' ' });
		}

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = JI_JZ;
				base.JI_JZ = value;
				if (oldValue != JI_JZ && !IsCopying)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
					ResetCustomsUnitDefaultingStrategy();
					ExecuteCustomsUnitDefaultingStrategy();
				}
			}
		}

		[ResourceStringData("EDD11C95-3ED1-4B9E-AF80-CB74404C84BA", Caption = "VAT", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("6AA6524C-22CF-49BC-85CE-0EE9559F9980", Caption = "[UCC 6/17] VAT")]
		public override ZString JI_ZZF_NKTaxType
		{
			get { return base.JI_ZZF_NKTaxType; }
			set
			{
				base.JI_ZZF_NKTaxType = value;
				Declaration?.ApplicationExtender?.GetInvoiceLineHelper(this)?.EnsureBox47TaxLineForVatRateWhenSettingTaxType(value);
			}
		}
		protected override bool JI_ZZF_NKTaxTypeReadOnly => false;

		protected override void SetDefaultTaxOrFeeCode()
		{
			if (!IsExport)
			{
				base.SetDefaultTaxOrFeeCode();
			}
		}

		protected override ZString DefaultDataGroupingForTaxOrFee => Core.Constants.CountryCodes.UnitedKingdom;

		public override ZString CustomsUQ => ZString.Empty;

		internal new void ResetCustomsUnitDefaultingStrategy() => base.ResetCustomsUnitDefaultingStrategy();

		protected override bool SetSecondQuantityFromEdiTariffsOwnRecord => false;

		public override ZInt MaxNumberOfAdditionalProcedureCode => Declaration?.ApplicationExtender?.MaxNumberOfAdditionalProcedureCodes ?? 0;

		public const string CfspFsdCPCCode = "0619090";

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected override void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			base.DecorateDocAddressRequirement(requirement, addressType);

			switch (addressType)
			{
				case DocAddressType.CustomsSupervisingOffice:

					if (Declaration != null)
					{
						requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address +=
							delegate  // Make sure to delegate the getting of the ApplicationExtender (inside GetJobComInvoiceLineValidation()) until the validation is invoked, and not at the point of initialising the DocAddress, otherwise we'll wire up one type of validation based on thie initial application code and not change it when the application code changes.
							{
								Declaration.GetJobComInvoiceLineValidation(this).ValidateSupervisingOfficeDocAddress();
							};
					}
					break;
			}
		}

		protected override EU.Business.Declaration.JobComInvoiceLineTaxCollection CreateTaxCollection()
		{
			return new JobComInvoiceLineTaxCollection(this);
		}

		public new JobComInvoiceLineTaxCollection Taxes => (JobComInvoiceLineTaxCollection)base.Taxes;

		BusinessObjectCollection ITaxAndDocsProvider.Taxes => Taxes;

		protected override bool UseUniversalTariffCore => true;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection()
		{
			return new SupportingDocumentCollection(this);
		}

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection()
		{
			return new AdditionalInfoCollection(this);
		}

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection()
		{
			return new PreviousDocumentCollection(this);
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		protected override ZString GetCountryCodeForSupplementaryCodeProviderCore() => Declaration?.GetCountryCodeForSupplementaryCodeProvider() ?? base.GetCountryCodeForSupplementaryCodeProviderCore();

		protected override void SetTariffEtcDataFromProductsPivotCore(BaseCusClassPartPivot pivot)
		{
			base.SetTariffEtcDataFromProductsPivotCore(pivot);

			var gbPivot = (CusClassPartPivot)pivot;

			if (!gbPivot.CI_ZZF_NKTaxType.IsEmpty)
			{
				JI_ZZF_NKTaxType = gbPivot.CI_ZZF_NKTaxType;
			}
			if (!gbPivot.CI_SecondQty.IsEmpty)
			{
				JI_CustomsSecondQuantity = gbPivot.CI_SecondQty;
			}
			if (!gbPivot.CI_FourthQty.IsEmpty)
			{
				JI_CustomsFourthQuantity = gbPivot.CI_FourthQty;
			}
			if (!gbPivot.CI_FifthQty.IsEmpty)
			{
				JI_CustomsFifthQuantity = gbPivot.CI_FifthQty;
			}
			if (IsImport && (Declaration?.IsNorthernIrelandDomestic ?? false) && !gbPivot.CI_GoodsCategory.IsEmpty)
			{
				ZG_GoodsCategory = gbPivot.CI_GoodsCategory;
			}
		}

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return Declaration?.ApplicationExtender.GetJobComInvoiceLineValueSetStrategy(this);
		}

		public ZBool JI_FecORG => CusEntryLine?.Header?.FECChallenges
			.Find(x => x.CY_Code == FECChallengeFields.Codes.JI_ORG && x.CY_ParentID == CusEntryLine.PK && x.CY_IsOverridden).Any() ?? false;

		public ZBool JI_FecQV1_NettMass => CusEntryLine?.Header?.FECChallenges
			.Find(x => (x.CY_Code == FECChallengeFields.Codes.JI_NettMass || x.CY_Code == FECChallengeFields.Codes.JI_NettMassUQ) && x.CY_ParentID == CusEntryLine.PK && x.CY_IsOverridden).Any() ?? false;

		public ZBool JI_FecQV2_Supp => CusEntryLine?.Header?.FECChallenges
			.Find(x => (x.CY_Code == FECChallengeFields.Codes.JI_Supp || x.CY_Code == FECChallengeFields.Codes.JI_SuppUQ) && x.CY_ParentID == CusEntryLine.PK && x.CY_IsOverridden).Any() ?? false;

		protected override string ChargeCodeForOverseasFreight => ChargesProvider.AirFreightCode;

		protected override bool IsValidToApportionToCore => Declaration != null ? NoAdditionalProcedureCodesAreE01orE02(this) : base.IsValidToApportionToCore;

		public static bool NoAdditionalProcedureCodesAreE01orE02(JobComInvoiceLine invoiceLine) =>
			IsValidProcedureToApportion(invoiceLine.JI_Procedure) && invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().All(x => IsValidProcedureToApportion(x.CY_Code));

		public static bool IsValidProcedureToApportion(string procedure) => !procedure.EndsWith("E01", StringComparison.OrdinalIgnoreCase) && !procedure.EndsWith("E02", StringComparison.OrdinalIgnoreCase);

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new InvoiceLineChargeCollection<InvoiceLineCharge>(this);
		}

		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		public new InvoiceLineChargeCollection<InvoiceLineCharge> Charges => (InvoiceLineChargeCollection<InvoiceLineCharge>)base.Charges;

		protected override List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> EffectiveAdditionalInfosCore()
		{
			if (Declaration != null)
			{
				return AdditionalInfos
					.Concat(InvoiceHeader?.AdditionalInfos.Where(x => !x.IsHeaderOnly) ?? Enumerable.Empty<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>())
					.ToList();
			}
			else
			{
				return base.EffectiveAdditionalInfosCore();
			}
		}

		public void RemoveAdditionalInfoByCode(ZString code)
		{
			var addInfos = AdditionalInfos.Cast<AdditionalInfo>().Where(y => y.CSI_Code == code).ToList();
			foreach (var ai in addInfos)
			{
				AdditionalInfos.RemoveAndDelete(ai);
			}
		}

		public AdditionalInfo AddAdditionalInfoByCodeIfNotExists(ZString code)
		{
			if (!AdditionalInfos.Cast<AdditionalInfo>().Any(y => y.CSI_Code == code))
			{
				var addInfo = AdditionalInfos.AddNew();
				addInfo.CSI_Code = code;
				return addInfo;
			}
			return null;
		}

		public bool IsNorthernIrelandDomestic => AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.NIDOM);

		public bool IsNorthernIrelandImportFromRow => AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.NIIMP);

		public bool IsNorthernIrelandImport => IsNorthernIrelandImportFromRow;

		public bool IsNorthernIrelandDeRiskStatement => (IsNorthernIrelandImport || IsNorthernIrelandDomestic) && AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code.In(AdditionalInfoValidation.RiskingAIStatementCodeList));

		public bool IsNorthernIrelandRemainOrQuotaDeRiskStatement
		{
			get
			{
				var numberOfCodesFound = AdditionalInfos.Cast<AdditionalInfo>().Select(x => x.CSI_Code).Count(c => c.Equals(GBCommonConstants.AdditonalInfoCodes.NIREM) || c.Equals(GBCommonConstants.AdditonalInfoCodes.NIQUO));
				return numberOfCodesFound == 1;
			}
		}

		public bool IsAtRisk => !AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.NIREM);

		public bool IsEuTariffToBeUsedForNorthernIreland => IsNorthernIrelandDomestic || (IsNorthernIrelandImportFromRow && IsAtRisk);

		[ResourceStringData("1C5891FA-9234-40C0-BCEB-018014187DD2", Caption = "NI Tariff")]
		public ZString TariffUsedForNorthernIrelandImport => IsImport ? IsEuTariffToBeUsedForNorthernIreland ? WindsorFrameworkNIProtocolTariffCodes.EUN : WindsorFrameworkNIProtocolTariffCodes.GB : null;

		#region JI_Calc_OrderLineNumberAndSubLine

		[ResourceStringData("8d1af733-da43-4193-8efc-48d7733033b2", Caption = "Entry Instruction Display Sequence")]
		public ZShort JI_Calc_InstructionDisplaySequence => EntryInstruction?.CEI_DisplaySequence ?? ZShort.Zero;

		public ZPropertyInfo JI_Calc_InstructionDisplaySequenceInfo => GetZPropertyInfo(Schema.JI_Calc_InstructionDisplaySequence);

		#endregion

		protected override ZString DefaultDataGroupingForTariffsCore => IsEuTariffToBeUsedForNorthernIreland ? (ZString)Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes : Declaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? CustomsCountryCodeCore;

		protected override ZString DutyAmountsAsStringCore => Declaration?.ApplicationExtender.GetDutyAmountsAsString(this, () => base.DutyAmountsAsStringCore) ?? base.DutyAmountsAsStringCore;

		[ResourceStringData("FBFF2A3D-2143-4D2C-9A87-EE8893C4BE89", Caption = "[31] Goods Desc.", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("21218AC1-6C9F-4859-9883-467E9B9D916C", Caption = "[UCC 6/8] Goods Desc.")]
		public override ZString JI_Description
		{
			get => base.JI_Description;
			set => base.JI_Description = value;
		}

		[ResourceStringData("FF02D96C-EC5D-494A-8F23-4542977F69E4", Caption = "[42] Price", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("AE5863A3-B47B-4AF4-845F-FF517DAD393F", Caption = "[UCC 4/14] Price")]
		public override ZDecimal JI_LinePrice
		{
			get => base.JI_LinePrice;
			set => base.JI_LinePrice = value;
		}

		[ResourceStringData("D08C3496-875C-4944-AC64-D67F01B26524", Caption = "[34] Origin", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("BB4B71B5-9988-4469-BE74-BA3DEC3F9221", Caption = "[UCC 5/15,16] Country/Region of (Preferential) Origin", MediumCaption = "[UCC 5/15,16] Ctry/Rgn. of (Preferential) Origin", ShortCaption = "[UCC 5/15,16] C/R of (Preferential) Origin", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("C0C3C58B-723D-4FE1-9DF0-573275F14F9D", Caption = "[UCC 5/15] Origin", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set => base.JI_CountryOfOrigin = value;
		}

		[ResourceStringData("31CE9319-3B8C-4BDC-A6BB-387A793DE4ED", Caption = "[17] Destination", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("DE017220-8B3B-4C00-8708-A24BE37D42E4", Caption = "[UCC 5/8] Destination")]
		public override ZString ZG_CountryOfDestination
		{
			get => base.ZG_CountryOfDestination;
			set => base.ZG_CountryOfDestination = value;
		}

		[ResourceStringData("07D6A75F-EE23-4D98-866C-436D48C572E6", Caption = "[35] GWT", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("4E96B14A-0316-4D92-AF4B-56ECE3C6C98D", Caption = "[UCC 6/5] GWT")]
		public override ZDecimal JI_Weight
		{
			get => base.JI_Weight;
			set => base.JI_Weight = value;
		}

		[ResourceStringData("E145D015-AAD7-476A-97E4-5D615E554C56", Caption = "[36] Pref. Code", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("92C87EAF-CCD1-421E-A47E-BB6C5FE6665D", Caption = "[UCC 4/17] Pref. Code")]
		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set
			{
				base.JI_PrimaryPreference = value;
				Declaration?.ApplicationExtender?.GetInvoiceLineHelper(this)?.HandleSettingOfPREF(JI_PrimaryPreferenceInfo);
			}
		}

		[ResourceStringData("710914C4-03CA-47ED-841F-B2CB2D6A9363", Caption = "[39] Quota", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("1A8F1597-397A-4D80-90FC-258DFF41A989", Caption = "[UCC 8/1] Quota")]
		public override ZString JI_ConcessionOrder
		{
			get => base.JI_ConcessionOrder;
			set => base.JI_ConcessionOrder = value;
		}

		[ResourceStringData("7945B12B-C42F-491D-B07D-D483EC27610A", Caption = "[43] Valuation Method", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("8DA54414-588E-4576-B57C-C2354518C5E2", Caption = "[UCC 4/16] Valuation Method")]
		public override ZString JI_ValuationCode
		{
			get => base.JI_ValuationCode;
			set => base.JI_ValuationCode = value;
		}

		[ResourceStringData("2E77A291-FCE6-4CB0-AF78-DCFAED996606", Caption = "Additional Procedure Codes", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("A2BEBBB7-F18C-4DBF-8E60-CE8CC824AF34", Caption = "[UCC 1/11] Further Additional Procedures")]
		public override ZString AdditionalProcedureCodesAsString => base.AdditionalProcedureCodesAsString;

		[ResourceStringData("4F6E6C9F-7934-4210-84E1-F45356215295", Caption = "[38] Customs Qty", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("79229D27-1BE3-4BCA-8F71-E184EBAAF2FC", Caption = "[UCC 6/1] Customs Qty")]
		public override ZDecimal JI_CustomsQuantity
		{
			get => base.JI_CustomsQuantity;
			set => base.JI_CustomsQuantity = value;
		}

		[ResourceStringData("D9FB62CA-6F9B-445F-9289-1DF530BBEC6F", Caption = "[46] Stat. Value", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("CD572CBD-4A56-4AEB-9D96-A1B0CA9FE3F2", Caption = "[UCC 8/6] Stat. Value")]
		public override ZDecimal ZG_StatisticalValue
		{
			get => base.ZG_StatisticalValue;
			set => base.ZG_StatisticalValue = value;
		}

		[ResourceStringData("FAE6D9F9-778C-463D-9617-5E31799FAB23", Caption = "[41] Supp. Qty", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("F7B25AFD-E58B-46F5-9A83-C6556B5E301F", Caption = "[UCC 6/2] Supp. Qty")]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get => base.JI_CustomsSecondQuantity;
			set => base.JI_CustomsSecondQuantity = value;
		}

		[ResourceStringData("6941830D-C0D2-4D36-B46D-7A784D72352D", Caption = "[44] Third Qty", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("C17282A4-3322-444D-81D4-843C832DD0C2", Caption = "Third Qty")]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get => base.JI_CustomsThirdQuantity;
			set => base.JI_CustomsThirdQuantity = value;
		}

		[ResourceStringData("44615B22-FE55-4FBC-A3F1-1AF6879E7BD9", Caption = "Fourth Qty")]
		public override ZDecimal JI_CustomsFourthQuantity
		{
			get => base.JI_CustomsFourthQuantity;
			set => base.JI_CustomsFourthQuantity = value;
		}

		[ResourceStringData("5677A37A-B176-466C-8BBD-FF7FEC7ED869", Caption = "Fifth Qty")]
		public override ZDecimal JI_CustomsFifthQuantity
		{
			get => base.JI_CustomsFifthQuantity;
			set => base.JI_CustomsFifthQuantity = value;
		}

		[ResourceStringData("16358D3C-28A9-40B9-BE0C-ACF2C76A519A", Caption = "[33] Tariff", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("04FF5D7A-FFCF-47E8-9483-2FDC7BF8B3BD", Caption = "[UCC 6/14 & 6/15] Commodity and TARIC", MultipleKey = JobDeclaration.MultipleKeyCdsImport)]
		[ResourceStringData("F0891A90-088E-4719-919C-6D6EFFECA433", Caption = "[UCC 6/14] Commodity", MultipleKey = JobDeclaration.MultipleKeyCdsExport)]
		public override ZString JI_FormattedTariff
		{
			get => base.JI_FormattedTariff;
			set => base.JI_FormattedTariff = value;
		}

		[ResourceStringData("154D6DD5-47BE-41B0-8783-0B791005C8E4", Caption = "Country of Supply", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("34A9A2EE-EC95-413F-842A-A0AEB6585414", Caption = "[UCC 5/15] Origin Override")]
		public override ZString ZG_CountryOfSupply
		{
			get => base.ZG_CountryOfSupply;
			set => base.ZG_CountryOfSupply = value;
		}

		[ResourceStringData("EEBB34CE-1638-4702-93AD-17A046E4DFC3", Caption = "VAT", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("5125E1BF-1253-4008-8446-1FE712E1BC9C", Caption = "[UCC 6/17] VAT")]
		public override ZPropertyInfo JI_ZZF_NKTaxTypeInfo => base.JI_ZZF_NKTaxTypeInfo;

		[ResourceStringData("98542F5B-C3D4-430D-B2DD-0EB31323BB27", Caption = "Method of Payment", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("AE49A903-AD59-4C1A-90CA-43C5E360E970", Caption = "[UCC 4/8] Method of Payment")]
		public override ZString ZG_MethodOfPayment { get => base.ZG_MethodOfPayment; set => base.ZG_MethodOfPayment = value; }

		[ResourceStringData("EDE3FC03-7719-423B-817B-D0E4E3682579", Caption = "SPIMM Category", ShortCaption = "SPIMM", MediumCaption = "SPIMM Category of Goods",
			FullDescription = "SPIMM (Simplified Procedure for Internal Market Movements) Category of Goods")]
		public override ZString ZG_GoodsCategory { get => base.ZG_GoodsCategory; set => base.ZG_GoodsCategory = value; }

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfExportList))]
		[ResourceStringData("308C9DD7-FD29-410A-8C02-94E35ABD92E2", Caption = "[UCC 5/14] Dispatch/Export Country/Region", MediumCaption = "[UCC 5/14] Dispatch", ShortCaption = "Dispatch")]
		public override ZString JI_RN_NKCountryOfExport { get => base.JI_RN_NKCountryOfExport; set => base.JI_RN_NKCountryOfExport = value; }

		protected override IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatterCore() => new EntryNumberFormatterForNctsAndDeclarationIntegration();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceLineFetchStrategy(this);

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		{
			return new UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>(il =>
			{
				if (il.SupplementaryCodes.Any(sc => sc.CY_Code.StartsWith("X")))
				{
					return il.AllApplicableRates;
				}

				return new[] { il.UniversalDutyRate };
			}, Enumerable.Empty<ZPropertyInfo>(), RateCalcUnitOfMeasureAggregator.IsConvertableFrom);
		}

		protected override IEnumerable<IZZRateSelectionCriteria> GetNationalRateSelectionCriteriaCore()
		{
			return new IZZRateSelectionCriteria[]
			{
				new RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>(this, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise, ZString.Empty)
			};
		}

		public bool IsUpdatingDetailsFromPart { get; private set; }

		protected override IDisposable OnUpdatingDetailsFromPart()
		{
			return new DisposableAction(() => { IsUpdatingDetailsFromPart = true; }, () => { IsUpdatingDetailsFromPart = false; });
		}
	}
}
