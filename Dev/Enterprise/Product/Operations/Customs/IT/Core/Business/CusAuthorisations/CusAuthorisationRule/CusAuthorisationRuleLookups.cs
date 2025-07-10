using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class CusAuthorisationRuleLookups : Customs.Business.CusAuthorisationRuleLookups
{
	public CusAuthorisationRuleLookups(CusAuthorisationRule parent) : base(parent)
	{
	}

	protected CusAuthorisationHeaderProvider Provider => (CusAuthorisationHeaderProvider)AuthorisationHeader?.Provider;

	public override CodeDescriptionPairList RuleCodeList => Factory.GetCachedValue("CusAuthorisationRuleCodeList|IT|" + AuthorisationHeader.CPH_Type, () => GetRuleCodeList());

	protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
	{
		var result = base.GetValueListFromRuleCodeCore();
		result.Add(ITCusAuthorisationRuleTypeList.Codes.Document, () => ValueListAuthorizationDocument);
		result.Add(ITCusAuthorisationRuleTypeList.Codes.Use, () => Factory.GetCachedValue<CusAuthorisationRuleUseValueList>());
		return result;
	}

	public ZZRefCusCodeListCombinedCollection ValueListAuthorizationDocument
	{
		get
		{
			var targetRefCusCodeListType = AuthorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode();
			var supportedDocCusCollection = !targetRefCusCodeListType.IsEmpty ? new ZString[] { targetRefCusCodeListType } : allSupportedDocRefCusCodeListTypes;
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, AuthorisationHeader.CPH_RN_NKCountryCode, supportedDocCusCollection, ZDateTime.Today, Enumerable.Empty<RefCusCodeListAttributeFilter>());
		}
	}

	#region Implementation

	CusAuthorisationHeader AuthorisationHeader => Parent?.AuthorisationHeader;

	CodeDescriptionPairList GetRuleCodeList()
	{
		if (AuthorisationHeader.CPH_Type == CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent)
		{
			return GetDeclarationOfIntentRuleCodeList();
		}

		var ruleCodeList = new CodeDescriptionPairList(base.RuleCodeList);
		AppendDocumentRuleCode(ruleCodeList);
		ruleCodeList.Sort();
		return ruleCodeList;
	}

	void AppendDocumentRuleCode(CodeDescriptionPairList ruleCodeList)
	{
		if (AuthorisationHeader.SupportImportSupportingDocumentsRule() || AuthorisationHeader.SupportExportSupportingDocumentsRule() || AuthorisationHeader.SupportNctsSupportingDocumentsRule())
		{
			ruleCodeList.AddPair(ITCusAuthorisationRuleTypeList.Codes.Document, ITCusAuthorisationRuleTypeList.Descriptions.Document);
		}
	}

	CodeDescriptionPairList GetDeclarationOfIntentRuleCodeList()
	{
		var ruleCodeList = new CodeDescriptionPairList();
		ruleCodeList.AddPair(ITCusAuthorisationRuleTypeList.Codes.Use, ITCusAuthorisationRuleTypeList.Descriptions.Use);
		return ruleCodeList;
	}

	readonly ZString[] allSupportedDocRefCusCodeListTypes = new ZString[]
	{
		Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection,
		Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection,
		EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
	};

	#endregion
}
