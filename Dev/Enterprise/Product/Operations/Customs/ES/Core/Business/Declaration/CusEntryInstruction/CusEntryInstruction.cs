using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration;

[CodeProperty(Schema.CEI_SubStyle)]
[SystemDefinedValues]
public class CusEntryInstruction : AutoCusEntryInstruction, Integration.Customs.ES.ICusEntryInstruction
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Schema
	public new partial class Schema : EU.Business.Declaration.AutoCusEntryInstruction.Schema
	{
		public const string IncludeRoutingSecurityData = "IncludeRoutingSecurityData";
	}
	#endregion

	public new CusEntryHeader EntryHeader => base.EntryHeader as CusEntryHeader;

	protected override EU.Business.CusGoodsLocation GetGoodsLocation() => (CusGoodsLocation)base.GetGoodsLocation();

	protected override bool IsGoodsLocationReadOnly => EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode) && EU.Business.TemporaryStorageHelper.IsLocationManagedInPremises(Factory, GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse)
		&& (IsGoodsLocationReadOnlyForImport || IsGoodsLocationReadOnlyForEXS);

	bool IsStatusForLocationReadOnlyCommon(ZString[] entryStatusList) => EntryHeader != null && (EntryHeader.IsWaitingForResponse || entryStatusList.Contains(EntryHeader.CH_EntryStatus));

	bool IsGoodsLocationReadOnlyForImport => JobDeclaration.IsImport && IsStatusForLocationReadOnlyCommon([EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, EntryStatusCodes.Cleared]);

	bool IsGoodsLocationReadOnlyForEXS => IsEXS && IsStatusForLocationReadOnlyCommon([EntryStatusCodes.Cleared]);

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

	public new AddInfoCusEntryInstruction AddInfo => (AddInfoCusEntryInstruction)base.AddInfo;

	public new AddInfoCusEntryInstructionLookups AddInfoLookups => AddInfo.Lookups;

	protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo() => new AddInfoCusEntryInstruction(CEI_AddInfoInfo);

	protected override ICusSupplyChainActorReferenceCollection<EU.Business.Declaration.CusSupplyChainActorReference> GetNewCusSupplyChainActorReferenceCollection() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.CusEntryInstruction> GetCusAuthorizationUsages() => new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var result = base.GetCusSupportingInfoTypesCore();
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		return result;
	}

	public void AddNewSupportingDocument(ZString code, ZString reference)
	{
		var document = SupportingDocuments.AddNew();
		document.CSI_Code = code;
		document.CSI_ReferenceNumber = reference;
		document.CSI_ParentID = PK;
		document.CSI_DataModel = CountryCode;
	}

	public ZBool IsSubStyleBOrC => CEI_SubStyle == EntrySubStyleList.Codes.B || CEI_SubStyle == EntrySubStyleList.Codes.C;

	public ZBool IsSubStyleAOrBOrC => CEI_SubStyle == EntrySubStyleList.Codes.A || IsSubStyleBOrC;

	public ZBool IsSubStyleBOrZ => CEI_SubStyle == EntrySubStyleList.Codes.B || CEI_SubStyle == EntrySubStyleList.Codes.Z;

	public ZBool IsSubStyleYOrZ => CEI_SubStyle == EntrySubStyleList.Codes.Y || CEI_SubStyle == EntrySubStyleList.Codes.Z;

	public ZBool IsSubStyleBOrCOrZ => CEI_SubStyle == EntrySubStyleList.Codes.B || CEI_SubStyle == EntrySubStyleList.Codes.C || CEI_SubStyle == EntrySubStyleList.Codes.Z;

	public ZBool IsSubStyleCOrYOrZ => CEI_SubStyle == EntrySubStyleList.Codes.C || CEI_SubStyle == EntrySubStyleList.Codes.Y || CEI_SubStyle == EntrySubStyleList.Codes.Z;

	public ZBool IsSubStyleAOrBOrCOrZ => CEI_SubStyle == EntrySubStyleList.Codes.A || CEI_SubStyle == EntrySubStyleList.Codes.B || CEI_SubStyle == EntrySubStyleList.Codes.C || CEI_SubStyle == EntrySubStyleList.Codes.Z;

	public ZBool IsSubStyleAOrBOrCOrYOrZ => IsSubStyleAOrBOrCOrZ || CEI_SubStyle == EntrySubStyleList.Codes.Y;

	public ZBool IsSubStyleAOrBOrCOrXOrYOrZ => IsSubStyleAOrBOrCOrZ || CEI_SubStyle == EntrySubStyleList.Codes.X || CEI_SubStyle == EntrySubStyleList.Codes.Y;

	public ZBool IsSubStyleAOrBOrXOrZ => IsSubStyleBOrZ || CEI_SubStyle == EntrySubStyleList.Codes.A || CEI_SubStyle == EntrySubStyleList.Codes.X;

	public bool IsStyleEmpty => CEI_Style.IsEmpty;

	public bool IsT2C => CEI_SubStyle.Equals(EntrySubStyleList.Codes.T2C);

	public bool IsT2L => CEI_SubStyle.Equals(EntrySubStyleList.Codes.T2L);

	public bool IsEXS => CEI_SubStyle.Equals(ExsEntrySubStyleList.Codes.EXS);

	public bool IsH2 => CEI_Style.Equals(IMPDeclarationTypeList.Codes.H2);

	public bool DJPProcedureAvailable => JobDeclaration.IsImport
		&& (CEI_SubStyle == EntrySubStyleList.Codes.B || CEI_SubStyle == EntrySubStyleList.Codes.Z)
		&& (EntryHeader != null && EntryHeader.CH_EntryStatus == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CEI_SubStyle = ZString.Empty;
	}

		[ReadOnlyMember(nameof(ZG_ActivateByOperatorReadOnly))]
		[ResourceStringData("71F2367E-A0AF-4BF7-991D-34600FE7F7A0", Caption = "Activate Pre Declaration by Operator", MediumCaption = "Active. Pre Dec. by Op.", ShortCaption = "Active. by Op.")]
		public override ZBool ZG_ActivateByOperator { get => base.ZG_ActivateByOperator; set => base.ZG_ActivateByOperator = value; }

		ZBool ZG_ActivateByOperatorReadOnly => !IsSubStyleAOrBOrC;

		public ZBool IncludeRoutingSecurityData
		{
			get => this.GetSystemDefinedValue<ZBool>(Customs.Business.GenAddOnHelper.IncludeRoutingSecurityData);
			set
			{
				var oldValue = IncludeRoutingSecurityData;
				if (value != oldValue)
				{
					this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.IncludeRoutingSecurityData, value);
					IncludeRoutingSecurityDataInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo IncludeRoutingSecurityDataInfo => GetZPropertyInfo(Schema.IncludeRoutingSecurityData);

	public CusAuthorisationRule GetAutorisationRuleFor5018Doc()
	{
		var authorisation = GetAuthorisationHeaderForDocs(SupportingDocumentType.WHLOCAuthorisation);
		var locRules = authorisation?.CusAuthorisationRules.Where(x => x.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && IsValidRuleCode(x.CPR_ValueFrom, authorisation.CPH_RN_NKCountryCode));
		return locRules != null && locRules.Count() == 1 ? locRules.First() : null;
	}

	ZBool IsValidRuleCode(ZString code, ZString countryCode) =>
				!code.IsEmpty
				&& ZZRefCusCodeListCombined.Loader.ExistsWithinEffectiveDate(Factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, code, ZDateTime.Today, false);

	public CusAuthorisationHeader GetAuthorisationHeaderForDocs(string docType)
	{
		CusAuthorisationHeader authorisation = null;
		var expectedAuthorizationUsage = GetCusAuthorizationUsageForDoc(docType);

		if (expectedAuthorizationUsage != null)
		{
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
			var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			query.AddToFilter(endDateQuery);

			if (!expectedAuthorizationUsage.AGC_Code.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, expectedAuthorizationUsage.AGC_Code);
			}

			if (!expectedAuthorizationUsage.AGC_OH_Owner.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, expectedAuthorizationUsage.AGC_OH_Owner);
			}

			if (!expectedAuthorizationUsage.AGC_Number.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, expectedAuthorizationUsage.AGC_Number);
			}

			authorisation = Factory.LoadTop1<CusAuthorisationHeader>(query);
		}
		return authorisation;
	}

	CusAuthorizationUsage GetCusAuthorizationUsageForDoc(string docType)
	{
		Func<ZString, ZBool> typeListContainsDocMethod = null;
		switch (docType)
		{
			case SupportingDocumentType.WHLOCAuthorisation:
				typeListContainsDocMethod = SupDoc5018TypeListContainsCode;
				break;
			case SupportingDocumentType.CentralizedClearance:
				typeListContainsDocMethod = SupDocC513TypeListContainsCode;
				break;
			case SupportingDocumentType.EntryOfDataInDeclarantsRecords:
				typeListContainsDocMethod = SupDocC514TypeListContainsCode;
				break;
		}

		CusAuthorizationUsage authorisationUsage = null;
		if (typeListContainsDocMethod != null && CusAuthorizationUsages.Any(x => typeListContainsDocMethod(x.AGC_Code)))
		{
			authorisationUsage = CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(x => typeListContainsDocMethod(x.AGC_Code));
		}
		return authorisationUsage;
	}

	ZBool SupDocC513TypeListContainsCode(ZString code) => code == CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;

	ZBool SupDocC514TypeListContainsCode(ZString code) => code == CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;

	ZBool SupDoc5018TypeListContainsCode(ZString code)
	{
		var codesList = new ZString[]
		{
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2,
			CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
			ESCusAuthorisationHeaderTypeList.Codes.InAPrivateOtherThanCustomsWarehouse,
			ESCusAuthorisationHeaderTypeList.Codes.InAPublicOtherThanCustomsWarehouseTypeI,
			ESCusAuthorisationHeaderTypeList.Codes.InAPublicOtherThanCustomsWarehouseTypeIi,
			ESCusAuthorisationHeaderTypeList.Codes.InAPublicRefWarehouseOnlyCanaryIslandAdministration,
		};

		return codesList.Contains(code);
	}
}
