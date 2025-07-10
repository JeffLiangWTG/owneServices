using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.DataTransfer.Universal;

public class CustomsSupportingInformationDataObjectReader : Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader
{
	public CustomsSupportingInformationDataObjectReader(CustomsSupportingInformation supportingInfoDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK, ZString parentTableCode, GetMatchingDataPredicate getMatchingData = null) : base(supportingInfoDataObject, logger, factory, parentPK, parentTableCode, getMatchingData)
	{
	}

	protected override CharacterCase CodeValueCharacterCase => CharacterCase.Normal;
}
