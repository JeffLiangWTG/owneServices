using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal;

public class CustomsSupportingInformationCollectionDataObjectReader : Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader
{
	public CustomsSupportingInformationCollectionDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, string dataContext = "", JobDeclarationDataObjectReader parentDeclarationReader = null) : base(logger, helper, dataContext)
	{
		this.parentDeclarationReader = parentDeclarationReader;
	}

	protected override void AddOrExecuteSetter(Dictionary<string, ValueSetter> delaySetters, ValueSetter setter)
	{
		if (delaySetters == null && (parentDeclarationReader?.IsUsedForSnapshotReverting ?? false ) && setter is IColumnValueSetterInfo columnValueSetter)
		{
			var originalValue = columnValueSetter.Row[columnValueSetter.Column.Name];
			base.AddOrExecuteSetter(delaySetters, setter);
			parentDeclarationReader.LogSetterMessage(setter, originalValue);
		}
		else
		{
			base.AddOrExecuteSetter(delaySetters, setter);
		}
	}

	protected override Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader CreateNewCustomsSupportingInformationDataObjectReader(CustomsSupportingInformation customsSupportingInformation, ZGuid parentPK, ZString parentTableCode, Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader.GetMatchingDataPredicate matchExisting) =>
	new CustomsSupportingInformationDataObjectReader(customsSupportingInformation, logger, helper.Factory, parentPK, parentTableCode, matchExisting);

	readonly JobDeclarationDataObjectReader parentDeclarationReader;
}
