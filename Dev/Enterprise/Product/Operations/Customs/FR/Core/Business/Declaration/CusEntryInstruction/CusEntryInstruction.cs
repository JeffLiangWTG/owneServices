using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[CodeProperty(Schema.CEI_Style), DescriptionProperty(Schema.CEI_Description)]
	public partial class CusEntryInstruction : AutoCusEntryInstruction, Integration.Customs.FR.ICusEntryInstruction
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal new AddInfoCusEntryInstruction AddInfo => (AddInfoCusEntryInstruction)base.AddInfo;

		protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(CEI_AddInfoInfo);

		public new AddInfoCusEntryInstructionValidation AddInfoValidation => (AddInfoCusEntryInstructionValidation)AddInfo.Validation;

		internal IValueSetStrategy ValueSetStrategy => GetValueSetStrategy();

		protected override IValueSetStrategy GetValueSetStrategy() => JobDeclaration?.ApplicationExtender.GetCusEntryInstructionValueSetStrategy(this) ?? new CusEntryInstructionValueSetStrategy(this);

		public new AddInfoCusEntryInstructionLookups AddInfoLookups
		{
			get
			{
				return JobDeclaration?.ApplicationExtender.GetAddInfoCusEntryInstructionLookups(AddInfo) ?? new DeltaGAddInfoCusEntryInstructionLookups(AddInfo);
			}
		}

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => JobDeclaration?.ApplicationExtender.GetCusEntryInstructionValidation(this) ?? new CusEntryInstructionValidation(this);

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override bool IsLookupsCachedInBase => false;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => JobDeclaration?.ApplicationExtender.GetCusEntryInstructionLookups(this) ?? new CusEntryInstructionLookups(this);

		new public CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		public override ZString CEI_SubStyle
		{
			get => base.CEI_SubStyle;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_SubStyle))
				{
					if (base.CEI_SubStyle != value)
					{
						base.CEI_SubStyle = value;
						JobDeclaration?.MarkInvoicesAsNeedingValidation();

						if (!IsValidationSuspended)
						{
							Validation.ValidateCEI_OA_Warehouse();
							Validation.ValidateCEI_OA_Warehouse2();
						}
					}
				}
			}
		}

		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set
			{
				var oldValue = CEI_DateForDuty;
				base.CEI_DateForDuty = value;
				if (!IsCopying && oldValue != CEI_DateForDuty)
				{
					JobDeclaration?.ResumeApportionment();
					JobDeclaration?.MarkInvoicesAsNeedingValidation();
				}
			}
		}

		[MaxLength(Schema.CEI_DescriptionMaxLength)]
		public override ZString CEI_Description
		{
			get => base.CEI_Description;
			set
			{
				CheckMaximumLength(CEI_DescriptionInfo, value);
				base.CEI_Description = value;
				JobDeclaration?.MarkInvoicesAsNeedingValidation();
			}
		}

		public override ZGuid CEI_OA_Warehouse
		{
			get => base.CEI_OA_Warehouse;
			set
			{
				if (base.CEI_OA_Warehouse != value)
				{
					base.CEI_OA_Warehouse = value;
					JobDeclaration?.MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CPCList))]
		public override ZString CEI_Procedure
		{
			get => base.CEI_Procedure;
			set
			{
				var oldValue = base.CEI_Procedure;
				base.CEI_Procedure = value;

				if (oldValue != value && !IsValidationSuspended)
				{
					Validation.ValidateCEI_OA_Warehouse();
					Validation.ValidateCEI_OA_Warehouse2();
				}
			}
		}

		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					if (CEI_Style != value)
					{
						base.CEI_Style = value;
						JobDeclaration?.CustomsOffices?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public CusAuthorisationHeader FromWarehouseAuthorisation => (CusAuthorisationHeader)(GetAuthorisationUsageForWarehousing()?.AuthorisationHeader) ?? Warehouse?.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(CountryCode).FirstOrDefault();

		public CusAuthorisationHeader ToWarehouseAuthorisation => (CusAuthorisationHeader)(GetAuthorisationUsageForWarehousing()?.AuthorisationHeader) ?? Warehouse2?.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(CountryCode).FirstOrDefault();

		protected override ZString GetFromWarehouseCode() => FromWarehouseAuthorisation?.CPH_Number ?? ZString.Empty;

		protected override ZString GetToWarehouseCode() => ToWarehouseAuthorisation?.CPH_Number ?? ZString.Empty;

		protected override ZString GetFromWarehouseType() => FromWarehouseAuthorisation?.CPH_Type ?? ZString.Empty;

		protected override ZString GetToWarehouseType() => ToWarehouseAuthorisation?.CPH_Type ?? ZString.Empty;

		CusAuthorizationUsage GetAuthorisationUsageForWarehousing()
		{
			var validAuthorizationTypes = FRCusAuthorizationUsageUpdater.GetAuthorizationTypesForCustomsWarehousing();
			return CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(p => p.AuthorisationHeader != null && validAuthorizationTypes.Contains<string>(p.AuthorisationHeader.CPH_Type));
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockGoodsLocationManagementMutex();
			}
		}

		public override void Delete()
		{
			GoodsLocation?.Delete();
			base.Delete();
			DisposeMutex();
		}

		protected override bool IsDescriptionDefaultedFromStyle => true;

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public ZBool IsPrelodgedSubstyle => (JobDeclaration.IsDeltaC && CEI_SubStyle == EntrySubstyleCodePairList.Codes.D) || (JobDeclaration.IsDeltaD && CEI_SubStyle == EntrySubstyleCodePairList.Codes.F);
		public ZBool IsLodgedSubstyle => (JobDeclaration.IsDeltaC && CEI_SubStyle == EntrySubstyleCodePairList.Codes.A) || (JobDeclaration.IsDeltaD && CEI_SubStyle == EntrySubstyleCodePairList.Codes.C);
		public ZBool IsNeitherPrelodgedNorLodged => CEI_SubStyle == EntrySubstyleCodePairList.Codes.Y || CEI_SubStyle == EntrySubstyleCodePairList.Codes.Z;

		public ZBool IsValidSubStyle
		{
			get
			{
				if (JobDeclaration.IsDeltaC)
				{
					return CEI_SubStyle == EntrySubstyleCodePairList.Codes.D || CEI_SubStyle == EntrySubstyleCodePairList.Codes.A || CEI_SubStyle == EntrySubstyleCodePairList.Codes.Z;
				}

				if (JobDeclaration.IsDeltaD)
				{
					return CEI_SubStyle == EntrySubstyleCodePairList.Codes.F || CEI_SubStyle == EntrySubstyleCodePairList.Codes.C || CEI_SubStyle == EntrySubstyleCodePairList.Codes.Y || CEI_SubStyle == EntrySubstyleCodePairList.Codes.Z;
				}

				return false;
			}
		}

		public new CusAuthorizationUsageEntryInstructionCollection CusAuthorizationUsages => (CusAuthorizationUsageEntryInstructionCollection)base.CusAuthorizationUsages;

		protected override ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new CusAuthorizationUsageEntryInstructionCollection(this, Factory);

		public CusAuthorizationUsage SpecificRegimeAuthorisationUsage => GetSpecificRegimeAuthorisationUsage();

		CusAuthorizationUsage GetSpecificRegimeAuthorisationUsage()
		{
			var validAuthorizationTypes = FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes(CEI_Style);
			return validAuthorizationTypes.IsNullOrEmpty() ? null : GetAuthorisationUsage(validAuthorizationTypes);

			CusAuthorizationUsage GetAuthorisationUsage(IEnumerable<string> authorizationTypes)
			{
				var authorizationUsagesWithType = CusAuthorizationUsages.Find(p => authorizationTypes.Contains<string>(p.AGC_Code));

				return authorizationUsagesWithType.FirstOrDefault();
			}
		}

		public static ZString GetComplementarySubstyle(string substyle)
		{
			string result;
			switch (substyle)
			{
				case EntrySubstyleCodePairList.Codes.D:
					result = EntrySubstyleCodePairList.Codes.A;
					break;
				case EntrySubstyleCodePairList.Codes.A:
					result = EntrySubstyleCodePairList.Codes.D;
					break;
				case EntrySubstyleCodePairList.Codes.F:
					result = EntrySubstyleCodePairList.Codes.C;
					break;
				case EntrySubstyleCodePairList.Codes.C:
					result = EntrySubstyleCodePairList.Codes.F;
					break;
				default:
					result = substyle;
					break;
			}
			return result;
		}

		public ZString SpecificRegimeNumber => SpecificRegimeAuthorizationNotTemporaryExportation ? SpecificRegimeAuthorisation.CusAuthorisationRules?.FirstOrDefault(r => r.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.AUT)?.CPR_ValueFrom ?? SpecificRegimeAuthorisation.CPH_Number : ZString.Empty;

		public ZString SpecificRegimeDescription => SpecificRegimeAuthorisation?.CPH_PermitDescription ?? ZString.Empty;

		public ZString SpecificRegimeCountryCode => SpecificRegimeAuthorizationNotTemporaryExportation && SpecificRegimeAuthorisation.PermitHolder != null ? SpecificRegimeAuthorisation.PermitHolder.CountryCode : ZString.Empty;

		public ZString SpecificRegimeNature => GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.NAT);

		public ZString SpecificRegimeCondition => GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.CON);

		public ZString SpecificRegimeOffice => GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.OFC);

		public ZString SpecificRegimeLocation => GetAuthorisationRuleValue(Enterprise.Customs.Business.CusAuthorisationRuleTypeList.Codes.Location);

		public ZString SpecificRegimeProcedure => GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.TRA);

		public ZString SpecificRegimeProcedureDescription => GetAuthorisationRuleDescription(CusAuthorisationRuleTypeList.Codes.TRA);

		public ZString SpecificRegimeInformation => GetAuthorisationRuleValue(CusAuthorisationRuleTypeList.Codes.INF);

		public CusAuthorisationHeader SpecificRegimeAuthorisation => SpecificRegimeAuthorisationUsage?.RelatedAuthorisationHeader;

		bool SpecificRegimeAuthorizationNotTemporaryExportation => SpecificRegimeAuthorisation != null && SpecificRegimeAuthorisation.CPH_Type != CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;

		// Using CusAuthorisationHeaderExtensions.GetAuthorisationRuleValueWithCode would be inefficient since it extracts all of the values.  We would then select just the first of those.
		internal ZString GetAuthorisationRuleValue(string code) => SpecificRegimeAuthorisation?.CusAuthorisationRules.Cast<CusAuthorisationRule>()?.FirstOrDefault(x => x.CPR_RuleCode == code)?.CPR_ValueFrom ?? ZString.Empty;

		internal ZString GetAuthorisationRuleDescription(string code) => SpecificRegimeAuthorisation?.CusAuthorisationRules.Cast<CusAuthorisationRule>()?.FirstOrDefault(x => x.CPR_RuleCode == code)?.CPR_Description ?? ZString.Empty;

		internal ZDecimal GetAuthorisationRuleDecimalValue(string code)
		{
			var value = GetAuthorisationRuleValue(code);
			return value.IsEmpty ? ZDecimal.Zero : (ZDecimal)Convert.ToDecimal(value);
		}

		public bool IsSimplified => CEI_Style == DeltaIEImportDeclarationTypeList.Codes.I1 && (CEI_SubStyle == EntrySubstyleCodePairList.Codes.C || CEI_SubStyle == EntrySubstyleCodePairList.Codes.F);

		public bool HasSimplifiedAuthorisationAdditionalInfo => AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._00100);

		#region Guarantees

		public new GuaranteeForEntryInstructionCollection Guarantees => (GuaranteeForEntryInstructionCollection)base.Guarantees;

		protected override EU.Business.Declaration.GuaranteeForEntryInstructionCollection GetNewGuaranteeForEntryInstructionCollection()
		{
			return new GuaranteeForEntryInstructionCollection(this);
		}

		#endregion

		#region ZG_BypassCode

		[UniversalCopyAddInfoPropertyMapping(AutoEUAddInfo.Schema.ZG_BypassCode)]
		public override ZString ZG_BypassCode
		{
			get { return base.ZG_BypassCode; }
			set
			{
				if (ZG_BypassCode != value)
				{
					base.ZG_BypassCode = value;
					ZG_BypassCodeInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region ZG_BypassReason

		[UniversalCopyAddInfoPropertyMapping(AutoEUAddInfo.Schema.ZG_BypassReason)]
		public override ZString ZG_BypassReason
		{
			get { return base.ZG_BypassReason; }
			set
			{
				if (ZG_BypassReason != value)
				{
					base.ZG_BypassReason = value;
					ZG_BypassReasonInfo.RefreshBinding();
				}
			}
		}

		#endregion

		public bool IsEntryStyleOutOfInward => JobDeclaration.ApplicationExtender.IsEntryInstructionOutOfInward(this);

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}
	}
}
