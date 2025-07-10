using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.DataTransfer.Universal;

class CustomsSupportingInformationCollectionDataObjectReader : Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader
{
	public CustomsSupportingInformationCollectionDataObjectReader(IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, string dataContext = "") : base(logger, helper, dataContext)
	{
	}

	protected override Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader CreateNewCustomsSupportingInformationDataObjectReader(CustomsSupportingInformation customsSupportingInformation, ZGuid parentPK,
		ZString parentTableCode, Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate matchExisting)
	{
		return new CustomsSupportingInformationDataObjectReader(customsSupportingInformation, logger, helper.Factory, parentPK, parentTableCode, matchExisting);
	}
}
