using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageHeaderLookups : EU.Business.CusTempStorage.TemporaryStorageHeaderLookups
{
	public TemporaryStorageHeaderLookups(TemporaryStorageHeader parent) : base(parent)
	{
	}

	protected new TemporaryStorageHeader Parent => (TemporaryStorageHeader)base.Parent;

	protected override CodeDescriptionPairList TransportMeansList => Factory.GetCachedValue<TemporaryStorageMeansOfTransportList>();

	public CodeDescriptionPairList CustomsProfileList
	{
		get
		{
			var customsProfileListProvider = GetCustomsProfileListProvider();
			return customsProfileListProvider.GetAccountDetails(true);
		}
	}

	public CodeDescriptionPairList RepresentativeQualificationList => GetCachedRepresentativeQualificationList();

	protected override CodeDescriptionPairList GetPNTSMessageStatusListCore() => Factory.GetCachedValue<PNTSMessageStatusList>();

	ICustomsProfileListProvider GetCustomsProfileListProvider()
	{
		var supportingDataAdapter = new TemporaryStorageHeaderCustomsProfileListLoaderSupportingDataAdapter(Parent);
		return new AccountCustomsProfileListProvider(Factory, supportingDataAdapter);
	}

	CodeDescriptionPairList GetCachedRepresentativeQualificationList()
	{
		return Factory.GetCachedValue("IT.TemporaryStorage.TemporaryStorageHeaderLookups.RepresentativeQualificationList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(RepresentationTypeList.Codes._2Direct, RepresentationTypeList.Descriptions._2Direct);
			list.AddPair(RepresentationTypeList.Codes._3Indirect, RepresentationTypeList.Descriptions._3Indirect);
			return list;
		});
	}
}
