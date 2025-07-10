using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Country = Enterprise.Core.Constants.CountryCodes;
using CusCodeListType = Enterprise.Customs.IT.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using TransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclarationLookups
{
	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public override CodeDescriptionPairList AuthorisationNumberList => AuthorizationsProvider.GetAuthorizations(new JobDeclarationAuthorizationDataProvider(Declaration));

	protected override CodeDescriptionPairList GetExportCommunityTransitStatusIDList() => Factory.GetCachedValue<ITExportCommunityTransitStatusList>();

	protected override CodeDescriptionPairList GetImportCommunityTransitStatusIDList() => new CodeDescriptionPairList();

	protected override CodeDescriptionPairList GetOthersCommunityTransitStatusIDList() => new CodeDescriptionPairList();

	public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => GetSpecificCircumstanceIndicatorList();

	public override CodeDescriptionPairList BorderTransportMeansList => GetBorderTransportMeansList();

	protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetExportInlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration)
		=> new InlandTransportCodeDescriptionPairListBuilder(declaration);

	protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetImportInlandTransportCodeDescriptionPairListBuilder(EU.Business.Declaration.JobDeclaration declaration)
		=> new InlandTransportCodeDescriptionPairListBuilder(declaration);

	#region Implementation

	AuthorizationListProvider AuthorizationsProvider => authorizationsProvider ?? (authorizationsProvider = new AuthorizationListProvider(Factory));
	AuthorizationListProvider authorizationsProvider;

	CodeDescriptionPairList GetSpecificCircumstanceIndicatorList()
	{
		if (Declaration.IsUCC6AndIsExport)
		{
			return RefCusCodeListTypes.GetCachedList(Factory, Country.Italy, CusCodeListType.SpecificCircumstanceIndicatorCode, ZDateTime.Today);
		}

		return base.SpecificCircumstanceIndicatorList;
	}

	CodeDescriptionPairList GetBorderTransportMeansList()
	{
		var declaration = Declaration;
		if (declaration.IsUCC6AndIsExport)
		{
			var transportMode = declaration.JE_TransportMode;
			return Factory.GetCachedValue(string.Join("|", "IT.JobDeclarationLookups.BorderTransportMeansList", declaration.JE_MessageType, transportMode), () => GetBorderTransportMeansListForUcc6Export(transportMode));
		}

		return base.BorderTransportMeansList;
	}

	CodeDescriptionPairList GetBorderTransportMeansListForUcc6Export(ZString transportMode)
	{
		var result = new CodeDescriptionPairList();

		switch (transportMode)
		{
			case TransportTypeListCodes.Air:
				result.AddPair(ExportBorderTransportMeansList.Codes._40, ExportBorderTransportMeansList.Descriptions._40);
				result.AddPair(ExportBorderTransportMeansList.Codes._41, ExportBorderTransportMeansList.Descriptions._41);
				result.DefaultCode = ExportBorderTransportMeansList.Codes._40;
				break;
			case TransportTypeListCodes.FixedTransportInstallations:
			case TransportTypeListCodes.OwnPropulsion:
			case TransportTypeListCodes.Mail:
			case "":
				result.AddRange(new ExportBorderTransportMeansList());
				result.DefaultCode = null;
				break;
			case TransportTypeListCodes.InlandWaterwayTransport:
				result.AddPair(ExportBorderTransportMeansList.Codes._10, ExportBorderTransportMeansList.Descriptions._10);
				result.AddPair(ExportBorderTransportMeansList.Codes._11, ExportBorderTransportMeansList.Descriptions._11);
				result.AddPair(ExportBorderTransportMeansList.Codes._80, ExportBorderTransportMeansList.Descriptions._80);
				result.AddPair(ExportBorderTransportMeansList.Codes._81, ExportBorderTransportMeansList.Descriptions._81);
				result.DefaultCode = ExportBorderTransportMeansList.Codes._81;
				break;
			case TransportTypeListCodes.Rail:
				result.AddPair(ExportBorderTransportMeansList.Codes._21, ExportBorderTransportMeansList.Descriptions._21);
				result.DefaultCode = ExportBorderTransportMeansList.Codes._21;
				break;
			case TransportTypeListCodes.Road:
				result.AddPair(ExportBorderTransportMeansList.Codes._30, ExportBorderTransportMeansList.Descriptions._30);
				result.DefaultCode = ExportBorderTransportMeansList.Codes._30;
				break;
			case TransportTypeListCodes.Sea:
				result.AddPair(ExportBorderTransportMeansList.Codes._10, ExportBorderTransportMeansList.Descriptions._10);
				result.AddPair(ExportBorderTransportMeansList.Codes._11, ExportBorderTransportMeansList.Descriptions._11);
				result.AddPair(ExportBorderTransportMeansList.Codes._80, ExportBorderTransportMeansList.Descriptions._80);
				result.AddPair(ExportBorderTransportMeansList.Codes._81, ExportBorderTransportMeansList.Descriptions._81);
				result.DefaultCode = ExportBorderTransportMeansList.Codes._11;
				break;
			default:
				break;
		}

		return result;
	}

	#endregion
}
