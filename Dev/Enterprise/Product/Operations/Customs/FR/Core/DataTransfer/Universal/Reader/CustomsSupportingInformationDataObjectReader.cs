using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal;

public class CustomsSupportingInformationDataObjectReader : Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader
{
	public CustomsSupportingInformationDataObjectReader(CustomsSupportingInformation supportingInfoDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK, ZString parentTableCode, GetMatchingDataPredicate getMatchingData = null, JobDeclarationDataObjectReader parentDeclarationReader = null) : base(supportingInfoDataObject, logger, factory, parentPK, parentTableCode, getMatchingData)
	{
		this.parentDeclarationReader = parentDeclarationReader;
	}

	protected override void AddOrExecuteSetter(Dictionary<string, ValueSetter> delaySetters, ValueSetter setter)
	{
		if (delaySetters == null && (parentDeclarationReader?.IsUsedForSnapshotReverting ?? false) && setter is IColumnValueSetterInfo columnValueSetter)
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

	readonly JobDeclarationDataObjectReader parentDeclarationReader;
}
