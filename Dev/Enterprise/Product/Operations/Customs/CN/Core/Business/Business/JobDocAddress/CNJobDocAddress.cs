using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDocAddress : JobDocAddress
	{
		public CNJobDocAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			E2_AddressOverrideInfo.ValueChanged += E2_AddressOverrideInfo_ValueChanged;
			GetDefaultCountryCodeIfEmpty = () => string.Empty;
		}

		#region Schema

		public new class Schema : JobDocAddress.Schema
		{
			public const string ChineseCompanyName = "ChineseCompanyName";
			public const string EnglishCompanyName = "EnglishCompanyName";
			public const string SocialCreditCode = "SocialCreditCode";
			public const string CustomsCode = "CustomsCode";
			public const string CIQCode = "CIQCode";
			public const string OverseasPartyCodeType = "OverseasPartyCodeType";
			public const string OverseasPartyCode = "OverseasPartyCode";

			public const int ChineseCompanyNameMaxLength = 80;
			public const int SocialCreditCodeMaxLength = 18;
			public const int CustomsCodeMaxLength = 10;
			public const int CIQCodeMaxLength = 10;
		}

		#endregion

		JobDeclaration Declaration => Parent as JobDeclaration;

		#region Properties

		public override bool SupportsDocAddressNumbers => true;

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|OverridenAddress", Caption = "Address", ShortCaption = "Addr.")]
		public override ZString E2_Address1 { get => base.E2_Address1; set => base.E2_Address1 = value; }

		public override ZBool E2_AddressOverride
		{
			get => base.E2_AddressOverride;
			set
			{
				var oldValue = base.E2_AddressOverride;
				base.E2_AddressOverride = value;
				if (!IsCopying && oldValue != base.E2_AddressOverride)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		[LightValidationTestExempt]
		public override ZGuid E2_ParentID { get => base.E2_ParentID; set => base.E2_ParentID = value; }

		[LightValidationTestExempt]
		public override ZString E2_ParentTableCode { get => base.E2_ParentTableCode; set => base.E2_ParentTableCode = value; }

		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set
			{
				var oldValue = base.E2_OA_Address;
				base.E2_OA_Address = value;
				if (!IsCopying && oldValue != base.E2_OA_Address)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		#region CompanyName chs/eng

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|ChineseCompanyName", Caption = "Company Name", ShortCaption = "Co. Name", FullDescription = "Company Name in Chinese")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member AddressNotOverridden")]
		[ReadOnlyMember("AddressNotOverridden")]
		[MaxLength(Schema.ChineseCompanyNameMaxLength)]
		public ZString ChineseCompanyName
		{
			get => E2_AddressOverride ? E2_CompanyName : Address?.GetChineseCompanyName().Left(Schema.ChineseCompanyNameMaxLength) ?? ZString.Empty;
			set
			{
				CheckMaximumLength(ChineseCompanyNameInfo, value);
				E2_CompanyName = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateChineseCompanyName();
				}
				ChineseCompanyNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ChineseCompanyNameInfo => GetZPropertyInfo(Schema.ChineseCompanyName);

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|EnglishCompanyName", Caption = "English Company Name", MediumCaption = "English Co.", ShortCaption = "En Co.", FullDescription = "Company Name in English")]
		public ZString EnglishCompanyName => E2_AddressOverride ? E2_CompanyName : Address?.GetEnglishCompanyName() ?? ZString.Empty;

		public ZPropertyInfo EnglishCompanyNameInfo => GetZPropertyInfo(Schema.EnglishCompanyName);

		#endregion

		#region Registration Codes

		ZString GetOrganisationRegNum(string type, OrgHeader orgHeader)
		{
			var result = ZString.Empty;
			if (orgHeader != null)
			{
				result = IsAeo(type)
					? orgHeader.GetAEONumber(Declaration?.DateOfValuation ?? ZDateTime.Today)
					: orgHeader.CustomsCodes.GetCustomsRegNo(type, Core.Constants.CountryCodes.China);
			}
			return result;
		}

		bool ShouldAutoFillCompanyByCode(string codeType) => E2_AddressOverride && ChineseCompanyName.IsEmpty && DocAddressNumbers.Cast<JobDocAddressNumber>().Where(x => x.E2N_NumberType != codeType).All(x => x.E2N_Number.IsEmpty);

		void SetRegNumber(ZPropertyInfo propertyInfo, ZString codeType, ZString regNo)
		{
			if (E2_AddressOverride)
			{
				SetDocAddressNumber(codeType, regNo.Left(propertyInfo.MaxLength));
			}

			if (!regNo.IsEmpty)
			{
				if (!E2_AddressOverride && !OrganisationPK.IsValid)
				{
					SetOrganisationPKByRegNo(codeType, regNo);
				}
				if (ShouldAutoFillCompanyByCode(codeType))
				{
					AutoFillCompanyNameAndRefreshOtherCodes(codeType, regNo);
				}
			}
		}
		JobDocAddressNumber GetDocAddressNumber(ZString codeType) => DocAddressNumbers.FindFirstByNumberType(codeType);

		void SetDocAddressNumber(ZString codeType, ZString regNo)
		{
			SetDocAddressNumber(GetDocAddressNumber(codeType), codeType, regNo);
		}

		void SetDocAddressNumber(JobDocAddressNumber number, string codeType, ZString regNo)
		{
			var isAeo = IsAeo(codeType);
			var numberToStore = isAeo ? regNo.SubstringSafe(2) : regNo;
			var countryCodeToStore = isAeo ? regNo.Left(2).ToString() : Core.Constants.CountryCodes.China;

			var jobDocAddressNumToStore = number ?? DocAddressNumbers.AddNew();
			jobDocAddressNumToStore.E2N_NumberType = codeType;
			jobDocAddressNumToStore.E2N_Number = numberToStore.Left(jobDocAddressNumToStore.E2N_NumberInfo.MaxLength);
			jobDocAddressNumToStore.E2N_RN_NKCountryCode = countryCodeToStore;
		}

		internal bool IsAeo(ZString codeType) => codeType == OrgCusCode.ChinaCodeTypes.AEO;

		void AutoFillCompanyNameAndRefreshOtherCodes(string codeType, ZString regNo)
		{
			var isAeo = IsAeo(codeType);
			var e2Number = isAeo ? regNo.SubstringSafe(2) : regNo;
			var countryCodeForAddressWithSameNum = isAeo ? (string)regNo.Left(2) : Core.Constants.CountryCodes.China;

			var scriptForAddressWithSameNum = FormattableString.Invariant($@"
E2_PK IN
(
	SELECT TOP 1 E2_PK FROM dbo.JobDocAddress 
	JOIN dbo.JobDeclaration ON JE_PK = E2_ParentID
	JOIN dbo.GlbBranch ON GB_PK = JE_GB
	JOIN dbo.GlbCompany ON GC_PK = GB_GC
	WHERE GC_PK = @GC_PK
	AND E2_PK <> @E2_PK
	AND JE_IsCancelled = 0
	AND E2_AddressOverride = 1
	AND E2_PK IN (SELECT E2N_E2 FROM dbo.JobDocAddressNumber WHERE E2N_NumberType = @CodeType AND E2N_Number = @RegNo AND E2N_RN_NKCountryCode = @CountryCode)
	ORDER BY JE_EntrySubmittedDate DESC, JE_SystemLastEditTimeUtc DESC
)");

			var queryForAddressWithSameNum = new ZDBOnlyQuery(typeof(JobDocAddress));
			queryForAddressWithSameNum.AddFilterAndZSQLParameterCollection(
				scriptForAddressWithSameNum,
				new ZSqlParameterCollection
				{
					{ "@GC_PK", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK },
					{ "@E2_PK", PK, JobDocAddressSchema.PK },
					{ "@CodeType", codeType, JobDocAddressNumberSchema.E2N_NumberType },
					{ "@RegNo", e2Number, JobDocAddressNumberSchema.E2N_Number },
					{ "@CountryCode", countryCodeForAddressWithSameNum, JobDocAddressNumberSchema.E2N_RN_NKCountryCode }
				}
			);
			var addressWithSameNum = Factory.LoadTop1<CNJobDocAddress>(queryForAddressWithSameNum);
			if (addressWithSameNum != null)
			{
				ChineseCompanyName = addressWithSameNum.ChineseCompanyName;
				if (RequiresDomesticOrg)
				{
					ChinaRegNumTypes.Where(x => x != codeType).ForEach(x =>
					{
						SetDocAddressNumber(x, addressWithSameNum.GetRegNumber(x));
						RefreshCodeBinding(x);
					});
				}
			}
		}

		void RefreshCodeBinding(ZString codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.ChinaCodeTypes.USC:
					SocialCreditCodeInfo.RefreshBinding();
					break;
				case OrgCusCode.CodeTypes.CustomsClientCode:
					CustomsCodeInfo.RefreshBinding();
					break;
				case OrgCusCode.ChinaCodeTypes.CIQ:
					CIQCodeInfo.RefreshBinding();
					break;
				default:
					OverseasPartyCodeInfo.RefreshBinding();
					break;
			}
		}

		ZString GetRegNumber(string codeType)
		{
			return E2_AddressOverride ? GetRegNumber(GetDocAddressNumber(codeType)) : GetOrganisationRegNum(codeType, Organisation);
		}

		ZString GetRegNumber(JobDocAddressNumber addressNumber)
		{
			var result = addressNumber?.E2N_Number ?? ZString.Empty;
			if (addressNumber != null && IsAeo(addressNumber.E2N_NumberType))
			{
				result = addressNumber.E2N_RN_NKCountryCode + result;
			}
			return result;
		}

		void SetOrganisationPKByRegNo(ZString codeType, ZString regNo)
		{
			if (!codeType.IsEmpty && !regNo.IsEmpty)
			{
				var orgCusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.China);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeType);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, regNo);

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				query.AddSubQuery(orgCusCodeQuery, JoinCondition.And);
				query.MaximumRows = 2;

				var orgHeaders = Factory.Load<OrgHeader>(query);
				if (orgHeaders.Length == 1)
				{
					OrganisationPK = orgHeaders.First().PK;
				}
			}
		}

		bool IsChinaCodesReadOnly => !E2_AddressOverride && OrganisationPK.IsValid;

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|CustomsCode", Caption = "China Customs Code", ShortCaption = "CCD", FullDescription = "China Customs Client Code")]
		[ReadOnlyMember(nameof(IsChinaCodesReadOnly))]
		[BusinessObjectTestExclude]
		[MaxLength(Schema.CustomsCodeMaxLength)]
		public ZString CustomsCode
		{
			get => GetRegNumber(OrgCusCode.CodeTypes.CustomsClientCode);
			set => SetRegNumber(CustomsCodeInfo, OrgCusCode.CodeTypes.CustomsClientCode, value);
		}

		public ZPropertyInfo CustomsCodeInfo => GetZPropertyInfo(Schema.CustomsCode);

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|SocialCreditCode", Caption = "Unified Social Credit Code", ShortCaption = "USC", FullDescription = "China Unified Social Credit Identifier")]
		[ReadOnlyMember(nameof(IsChinaCodesReadOnly))]
		[BusinessObjectTestExclude]
		[MaxLength(Schema.SocialCreditCodeMaxLength)]
		public ZString SocialCreditCode
		{
			get => GetRegNumber(OrgCusCode.ChinaCodeTypes.USC);
			set => SetRegNumber(SocialCreditCodeInfo, OrgCusCode.ChinaCodeTypes.USC, value);
		}

		public ZPropertyInfo SocialCreditCodeInfo => GetZPropertyInfo(Schema.SocialCreditCode);

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|CIQCode", Caption = "Inspection and Quarantine Code", ShortCaption = "CIQ", FullDescription = "China Import-Export Inspection and Quarantine Code")]
		[ReadOnlyMember(nameof(IsChinaCodesReadOnly))]
		[BusinessObjectTestExclude]
		[MaxLength(Schema.CIQCodeMaxLength)]
		public ZString CIQCode
		{
			get => GetRegNumber(OrgCusCode.ChinaCodeTypes.CIQ);
			set => SetRegNumber(CIQCodeInfo, OrgCusCode.ChinaCodeTypes.CIQ, value);
		}

		public ZPropertyInfo CIQCodeInfo => GetZPropertyInfo(Schema.CIQCode);

		internal bool IsOverseasParty(ZString codeType) => Lookups.OverseasPartyCodes.ContainsCode(codeType);

		internal JobDocAddressNumber OverseasPartyNumber
		{
			get
			{
				if (fOverseasPartyNumber == null || fOverseasPartyNumber.IsDeleted)
				{
					fOverseasPartyNumber = DocAddressNumbers.Cast<JobDocAddressNumber>().FirstOrDefault(x => IsOverseasParty(x.E2N_NumberType));
				}
				return fOverseasPartyNumber;
			}
		}
		JobDocAddressNumber fOverseasPartyNumber;

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|OverseasPartyCodeType", Caption = "Overseas Party Code Type", ShortCaption = "Code", FullDescription = "Code Type of Overseas Party")]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CNJobDocAddressLookups.OverseasPartyCodes))]
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[MaxLength(JobDocAddressNumber.Schema.E2N_NumberTypeMaxLength)]
		public ZString OverseasPartyCodeType
		{
			get => E2_AddressOverride ? (OverseasPartyNumber?.E2N_NumberType ?? ZString.Empty) : GetOverseasPartyCodeTypeAndCode(Organisation).CodeType;
			set
			{
				if (E2_AddressOverride && (OverseasPartyNumber != null || IsOverseasParty(value)))
				{
					SetDocAddressNumber(OverseasPartyNumber, value, OverseasPartyCode);
					ZString defaultCode = overseasPartyTypeCodeList?.GetDescriptionFromCode(value) ?? ZString.Empty;
					if (!defaultCode.IsEmpty)
					{
						OverseasPartyCode = defaultCode;
					}
				}

				OverseasPartyCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo OverseasPartyCodeTypeInfo => GetZPropertyInfo(Schema.OverseasPartyCodeType);

		[ResourceStringData("Enterprise.Customs.CN.Business.DocAddressWrapper|OverseasPartyCode", Caption = "Overseas Party Code", ShortCaption = "Code", FullDescription = "Code of Overseas Party")]
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[BusinessObjectTestExclude]
		[MaxLength(JobDocAddressNumber.Schema.E2N_NumberMaxLength)]
		public ZString OverseasPartyCode
		{
			get
			{
				return E2_AddressOverride ? GetRegNumber(OverseasPartyNumber) : GetOverseasPartyCodeTypeAndCode(Organisation).Code;
			}
			set
			{
				SetRegNumber(OverseasPartyCodeInfo, OverseasPartyCodeType, value);
			}
		}
		public ZPropertyInfo OverseasPartyCodeInfo => GetZPropertyInfo(Schema.OverseasPartyCode);

		(ZString CodeType, ZString Code) GetOverseasPartyCodeTypeAndCode(OrgHeader organisation)
		{
			foreach (var numberType in OverseasPartyTypesInOrder)
			{
				var number = GetOrganisationRegNum(numberType, organisation);
				if (!number.IsEmpty)
				{
					return (numberType, number);
				}
			}
			return (ZString.Empty, ZString.Empty);
		}

		string[] OverseasPartyTypesInOrder => (Declaration?.IsImport ?? true)
			? new[] { OrgCusCode.ChinaCodeTypes.MMR, OrgCusCode.ChinaCodeTypes.SMR, OrgCusCode.ChinaCodeTypes.AEO }
			: new[] { OrgCusCode.ChinaCodeTypes.AEO, OrgCusCode.ChinaCodeTypes.MMR, OrgCusCode.ChinaCodeTypes.SMR };

		#endregion

		#endregion

		#region Codes Avaliabilities

		public bool RequiresTradeOrg => Declaration != null
			&& (E2_AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress && Declaration.WillGenerateEnteringEntry
			|| E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && Declaration.WillGenerateExitingEntry);

		public bool RequiresOwnerOrg => Declaration != null
			&& (E2_AddressType == DocAddressTypes.Codes.BuyerDocumentaryAddress && Declaration.WillGenerateEnteringEntry
			|| E2_AddressType == DocAddressTypes.Codes.Manufacturer && Declaration.WillGenerateExitingEntry);

		public bool RequiresDomesticOrg => RequiresTradeOrg || RequiresOwnerOrg;

		public bool RequiresOverseasOrg => Declaration != null
			&& (E2_AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress && !Declaration.WillGenerateEnteringEntry
			|| E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && !Declaration.WillGenerateExitingEntry);

		#endregion

		#region E2_AddressOverride Changed

		void E2_AddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			if (E2_AddressOverride)
			{
				ResetChineseCompanyNameFromDocAddress();
				ResetGovRegTypeAndNumbers();
			}

			if (Requirement != null)
			{
				Requirement.CanOverride = E2_AddressOverride;
			}
		}

		void ResetChineseCompanyNameFromDocAddress()
		{
			ZString chsName;
			if (Address != null && !(chsName = Address.GetChineseCompanyName()).IsEmpty)
			{
				ChineseCompanyName = chsName.Left(Schema.ChineseCompanyNameMaxLength);
			}
		}

		void ResetGovRegTypeAndNumbers()
		{
			if (RequiresDomesticOrg)
			{
				var orgHeader = Factory.Load<OrgHeader>(OrganisationPK);
				if (orgHeader != null)
				{
					SocialCreditCode = GetOrganisationRegNum(OrgCusCode.ChinaCodeTypes.USC, orgHeader);
					CustomsCode = GetOrganisationRegNum(OrgCusCode.CodeTypes.CustomsClientCode, orgHeader);
					CIQCode = GetOrganisationRegNum(OrgCusCode.ChinaCodeTypes.CIQ, orgHeader);
				}
			}
			if (RequiresOverseasOrg)
			{
				var orgHeader = Factory.Load<OrgHeader>(OrganisationPK);
				BuildOverseasPartyTypeCodeList(orgHeader);

				var overseasPartyCodePair = overseasPartyTypeCodeList.Cast<CodeDescriptionPair>().FirstOrDefault();
				if (overseasPartyCodePair != null)
				{
					OverseasPartyCodeType = overseasPartyCodePair.Code;
					OverseasPartyCode = overseasPartyCodePair.Description;
				}
			}
		}

		void BuildOverseasPartyTypeCodeList(OrgHeader organisation)
		{
			overseasPartyTypeCodeList = new CodeDescriptionPairList();

			if (organisation != null)
			{
				foreach (var codeType in OverseasPartyTypesInOrder)
				{
					ZString code = GetOrganisationRegNum(codeType, organisation);
					if (!code.IsEmpty)
					{
						overseasPartyTypeCodeList.AddPair(codeType, code);
					}
				}
			}
		}
		CodeDescriptionPairList overseasPartyTypeCodeList;

		#endregion

		public static IEnumerable<string> ChinaRegNumTypes
		{
			get
			{
				yield return OrgCusCode.ChinaCodeTypes.USC;
				yield return OrgCusCode.CodeTypes.CustomsClientCode;
				yield return OrgCusCode.ChinaCodeTypes.CIQ;
			}
		}

		public static IEnumerable<string> OverseasPartyTypes
		{
			get
			{
				yield return OrgCusCode.ChinaCodeTypes.AEO;
				yield return OrgCusCode.ChinaCodeTypes.MMR;
				yield return OrgCusCode.ChinaCodeTypes.SMR;
			}
		}

		public static IEnumerable<string> AllRegNumTypes => ChinaRegNumTypes.Union(OverseasPartyTypes);

		#region Validation & Lookups

		public new CNJobDocAddressValidation Validation => (CNJobDocAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation() => new CNJobDocAddressValidation(this);

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new CNJobDocAddressLookups(this);
		}

		public new CNJobDocAddressLookups Lookups => (CNJobDocAddressLookups)base.Lookups;

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (E2_AddressOverride && !RequiresDomesticOrg)
			{
				ChinaRegNumTypes.ForEach(x => GetDocAddressNumber(x)?.Delete());
			}
			if (!RequiresOverseasOrg || OverseasPartyCode.IsEmpty || OverseasPartyCodeType.IsEmpty)
			{
				OverseasPartyNumber?.Delete();
			}
		}
	}
}
