using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.CH.Business;

public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public ZZRefCusCodeListCombinedCollection CustomsOffices => CommonLookups.CustomsOfficeList(Parent);

	public override CodeDescriptionPairList CargoIdTypeList
	{
		get
		{
			return Factory.GetCachedValue("CH.JobDeclaration.Lookups.CargoIdTypeList." + Parent.JE_TransportMode, delegate
			{
				var list = FreightCodePairLists.JS_PackingModeList(Parent.JE_TransportMode);
				if (Parent.JE_TransportMode != Enterprise.Core.Constants.TransportModes.Air)
				{
					list.AddPairIfNotExist(ContainerModes.Containerised, Enterprise.Core.Constants.ContainerModeDescriptions.Containerised);
				}
				list.AddPairIfNotExist(ContainerModes.NonContainerised, Enterprise.Core.Constants.ContainerModeDescriptions.NonContainerised);
				list.RemoveCode(ContainerModes.BuyersConsol);
				list.RemoveCode(ContainerModes.ShippersConsol);
				list.RemoveCode(ContainerModes.AgentConsol);
				return list;
			});
		}
	}

	public OrgHeaderCollection RepresentativeList => new OrgHeaderCollection(Factory);

	public ConsignorCollection ConsignorList => new ConsignorCollection(Factory);

	public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<DeclarationPayerList>();

	public override CodeDescriptionPairList DeclarationLanguageList => CommonLookups.CommunicationLanguageList(Factory);

	public override CodeDescriptionPairList MessageStatusList => CommonLookups.MessageStatusList(Factory);

	public override CodeDescriptionPairList MessageSubTypeList => CommonLookups.ActivationTypeList(Factory);

	public override CodeDescriptionPairList TransportMeansList => CommonLookups.TransportModeList(Factory);

	public CodeDescriptionPairList AuthorizationsList => Parent.IsImport ? GetImportAuthorizationsList() : GetExportAuthorizationsList();

	CodeDescriptionPairList GetImportAuthorizationsList()
	{
		var holderPK = Parent.Representative?.Header?.PK ?? ZGuid.Empty;
		var date = Parent.DateOfValuation.Date;
		return Factory.GetCachedValue($"CH.JobDeclaration.Lookups.AuthorizationsList_Import_{holderPK}_{Parent.DateOfValuation.Date}", () =>
		{
			var authorizationList = new CodeDescriptionPairList();
			var authorizations = new CusAuthorisationHeaderCollection(Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec, holderPK, date);
			foreach (var rule in authorizations.SelectMany(x => x.CusAuthorisationRules.Where(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location)))
			{
				authorizationList.AddPair(rule.PK, rule.CPR_ValueFrom, rule.CPR_Description);
			}
			return authorizationList;
		});
	}

	CodeDescriptionPairList GetExportAuthorizationsList() => CommonLookups.ExportAuthorizationsList(Parent, Parent.IsExportActivationEdec, Parent.DeclarantAddress?.Header?.PK ?? ZGuid.Empty);

	protected override CodeDescriptionPairList GetEntryPhaseStatusListCore => Factory.GetCachedValue<PassarDeclarationPhaseList>();

	public CodeDescriptionPairList SelectionResultList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult, ZDateTime.Today);

	public override CodeDescriptionPairList EntryStatusList => CommonLookups.CustomsStatusList(Factory);

	public CodeDescriptionPairList SpecificCircumstanceIndicatorList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.N0296, ZDateTime.Today);

	public CodeDescriptionPairList TransportationTypeList => CommonLookups.TransportationTypeList(Parent);

	public CodeDescriptionPairList ClearanceLocationList
	{
		get
		{
			var declaration = Parent;
			if (declaration.IsImport)
			{
				return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ClearanceLocation, declaration.DateOfValuation, false, UniversalReferenceConstants.RefCusCodeList.Attributes.IsImports, new ZString[] { UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes });
			}
			if (declaration.IsExportOrExportDeclarationActivation)
			{
				return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ClearanceLocation, declaration.DateOfValuation, false, UniversalReferenceConstants.RefCusCodeList.Attributes.IsExports, new ZString[] { UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes });
			}
			return new CodeDescriptionPairList();
		}
	}
}
