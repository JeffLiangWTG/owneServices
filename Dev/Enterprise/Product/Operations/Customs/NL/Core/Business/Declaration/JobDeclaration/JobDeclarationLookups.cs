using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public OrganisationsFindBoxCollection IntracomReceiverList => new(Factory);

	public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaymentPartyList>();

	public CodeDescriptionPairList CustomsOfficesList
	{
		get
		{
			var isImportInterface = Parent.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced && Parent.IsImport;
			var key = isImportInterface ? "IMP_INF" : "CURRENT";
			var cacheKey = "NLDeclarationCustomsOffice_" + key;

			return Factory.GetCachedValue<CodeDescriptionPairList>(cacheKey, () => isImportInterface ? new CustomsOfficesImportInterfaceList() : new CustomsOfficesList());
		}
	}

	public CodeDescriptionPairList LocationQualifierList => Factory.GetCachedValue<LocationQualifierList>();

	public override OrgHeaderCollection ControllingCustomers => controllingCustomers ??= new OrganisationsFindBoxCollection(Factory);
	OrganisationsFindBoxCollection controllingCustomers;

	public override CodeDescriptionPairList TransportMeansList
	{
		get
		{
			var transportModeInland = Parent.JE_TransportModeInland;
			return Factory.GetCachedValue(string.Join("|", "JobDeclarationLookups.TransportMeansList", transportModeInland), () =>
			{
				var result = new CodeDescriptionPairList();

				switch (transportModeInland)
				{
					case Customs.Business.TransportTypeList.Codes.Road:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._30, TransportTypeIdList.Descriptions._30);
						break;
					case Customs.Business.TransportTypeList.Codes.Sea:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._10, TransportTypeIdList.Descriptions._10);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._11, TransportTypeIdList.Descriptions._11);
						break;
					case Customs.Business.TransportTypeList.Codes.Rail:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._20, TransportTypeIdList.Descriptions._20);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._21, TransportTypeIdList.Descriptions._21);
						break;
					case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._80, TransportTypeIdList.Descriptions._80);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._81, TransportTypeIdList.Descriptions._81);
						break;
					case Customs.Business.TransportTypeList.Codes.Air:
						result.AddPairIfNotExist(TransportTypeIdList.Codes._40, TransportTypeIdList.Descriptions._40);
						result.AddPairIfNotExist(TransportTypeIdList.Codes._41, TransportTypeIdList.Descriptions._41);
						break;
					case Customs.Business.TransportTypeList.Codes.Mail:
					case Customs.Business.TransportTypeList.Codes.OwnPropulsion:
					case Customs.Business.TransportTypeList.Codes.FixedTransportInstallations:
						result = new TransportMeansList();
						break;
				}

				return result;
			});
		}
	}

	protected override CodeDescriptionPairList GetEntryPhaseStatusListCore => Factory.GetCachedValue<CustomsEntryPhaseStatusList>();
}
