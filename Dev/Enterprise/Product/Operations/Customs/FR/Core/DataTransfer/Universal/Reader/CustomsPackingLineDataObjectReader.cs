using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal;

public class CustomsPackingLineDataObjectReader : Customs.DataTransfer.Universal.CustomsPackingLineDataObjectReader
{
	public CustomsPackingLineDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, BaseJobDeclaration declaration, IColumnIndexer billRow, bool supportsParentPackage, JobDeclarationDataObjectReader parentDeclarationReader = null) : base(packingLineDataObject, logger, helper, declaration, billRow, supportsParentPackage)
	{
		this.parentDeclarationReader = parentDeclarationReader;
	}

	protected override bool ShouldRevertPackLine(PackingLine packingLine)
	{
		var result = base.ShouldRevertPackLine(packingLine);
		if (parentDeclarationReader is not null)
		{
			result = !parentDeclarationReader.IsUsedForSnapshotReverting || parentDeclarationReader.Strategy.Equals(SnapshotRevertingStrategy.Override);
			if (!result)
			{
				result = packingLine.PackedItemCollection.All(item => parentDeclarationReader.PackingLinksFromInvoiceLinesOfTheEntryHeaderToBeReverted.Contains(item.CommercialInvoiceLineLink.GetValueOrDefault()));
			}
			if (result)
			{
				parentDeclarationReader.SnapshotReaderLogger?.Log(Integration.LogType.Information, $"Pack Line MarksAndNos#{packingLine.MarksAndNos}: Reverting properties.");
			}
		}
		return result;
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

	readonly JobDeclarationDataObjectReader parentDeclarationReader;
}
