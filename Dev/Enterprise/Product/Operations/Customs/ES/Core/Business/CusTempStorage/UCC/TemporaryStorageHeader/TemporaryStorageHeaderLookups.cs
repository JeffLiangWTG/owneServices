using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeaderLookups : EU.Business.CusTempStorage.TemporaryStorageHeaderLookups
{
	public TemporaryStorageHeaderLookups(TemporaryStorageHeader parent) : base(parent)
	{
	}

	protected override CodeDescriptionPairList MessageTypeListCore => Factory.GetCachedValue<G5MessageTypeCodeList>();

	public CodeDescriptionPairList CertificateNames => CertificateHelper.CertificateNames(Factory, Parent.CustomsAgent, GetType().Name);

	protected override CustomsOfficeCodeCollection CustomsOfficeCodeListCore => EU.Business.EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);

	public CustomsOfficeCodeCollection DestinationCustomsOfficeCodeList => EU.Business.EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);

	public CusTempStorageRegPremisesCollection CusTempStorageRegPremisesList
	{
		get
		{
			var parent = (TemporaryStorageHeader)Parent;
			if (parent.IsMessageTypeTSM)
			{
				return new CusTempStorageRegPremisesCollection(Factory, EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);
			}
			if (parent.IsMessageTypeLAM)
			{
				return new CusTempStorageRegPremisesCollection(Factory, EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility);
			}
			return new CusTempStorageRegPremisesCollection(Factory);
		}
	}

	protected override CodeDescriptionPairList TransportMeansList => Factory.GetCachedValue<TemporaryStorageMeansOfTransportList>();
}
