using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public static class CommonLookups
{
	public static ZZRefCusCodeListCombinedCollection CustomsOfficeList<TParent>(TParent parent)
		where TParent : BusinessObject, IDateOfValuationProvider
		=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(parent.Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CHCustomsOffice, parent.DateOfValuation);

	public static CodeDescriptionPairList DeclarationTimeCodeList<TParent>(TParent parent)
		where TParent : BusinessObject, IDateOfValuationProvider
		=> RefCusCodeListTypes.GetCachedList(parent.Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, parent.DateOfValuation);

	public static CodeDescriptionPairList NextProcedureList<TParent>(TParent parent)
		where TParent : BusinessObject, IDateOfValuationProvider
		=> RefCusCodeListTypes.GetCachedList(parent.Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.NextProcedure, parent.DateOfValuation);

	public static CodeDescriptionPairList TransportModeList(BusinessObjectFactory factory) => factory.GetCachedValue<TransportMeansList>();

	public static CodeDescriptionPairList TransportTypeList(BusinessObjectFactory factory) => factory.GetCachedValue<TransportTypeList>();

	public static CodeDescriptionPairList TransportationTypeList<TParent>(TParent parent)
		where TParent : BusinessObject, IDateOfValuationProvider
		=> RefCusCodeListTypes.GetCachedList(parent.Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportationType, parent.DateOfValuation);

	public static CodeDescriptionPairList ExportAuthorizationsList<TParent>(TParent parent, bool isEdec, ZGuid holderOrgPK)
		where TParent : BusinessObject, IDateOfValuationProvider
	{
		var date = parent.DateOfValuation.Date;
		return parent.Factory.GetCachedValue($"CH.CommonLookups.ExportAuthorizationsList_{holderOrgPK}_{parent.DateOfValuation.Date}_{isEdec}", () =>
		{
			var authorizationList = new CodeDescriptionPairList();
			var authorizations = new CusAuthorisationHeaderCollection(parent.Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, holderOrgPK, date);
			foreach (var authorization in authorizations)
			{
				authorizationList.AddPair(authorization.CPH_Number, isEdec ? authorization.AppliesTo?.Header?.OH_Code : authorization.CPH_PermitDescription);
			}
			return authorizationList;
		});
	}

	public static CodeDescriptionPairList CommunicationLanguageList(BusinessObjectFactory factory) => factory.GetCachedValue<SwissCustomsLanguageList>();

	public static CodeDescriptionPairList ActivationTypeList(BusinessObjectFactory factory) => factory.GetCachedValue<ActivationTypeList>();

	public static CodeDescriptionPairList MessageStatusList(BusinessObjectFactory factory) => factory.GetCachedValue<CHLogicalStatusList>();

	public static CodeDescriptionPairList CustomsStatusList(BusinessObjectFactory factory)
		=> factory.GetCachedValue("CH.CommonLookups.CustomsStatusList." + GlbStaff.CurrentUser.GS_WorkingLanguage, delegate
		{
			var entryStatusCodeList = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EntryStatus, ZDateTime.Now);
			var codeDescriptionPairList = new AdditionalCHEntryStatusList();
			codeDescriptionPairList.AddRange(entryStatusCodeList);
			codeDescriptionPairList.Sort();
			return codeDescriptionPairList;
		});
}
