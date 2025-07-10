using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal;

public class CommercialInvoiceHeaderDataObjectReader : EU.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader
{
	public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, EU.Business.Declaration.JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null, JobDeclarationDataObjectReader parentDeclarationReader = null) : base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, typeof(JobComInvoiceHeader))
	{
		this.parentDeclarationReader = parentDeclarationReader;
	}

	protected override BaseJobComInvoiceHeader PopulateInvoiceDataAndFinaliseImport(BaseJobComInvoiceHeader invoiceBO, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, CollectionContent? commercialInvoiceCollectioncontent = null)
	{
		if (parentDeclarationReader is null || !parentDeclarationReader.IsUsedForSnapshotReverting || parentDeclarationReader.Strategy.Equals(SnapshotRevertingStrategy.Override) || invoiceBO.InvoiceLines.All(line => parentDeclarationReader.EntryHeaderToRevert.InvoiceLines.Select(entryInvoiceLine => entryInvoiceLine.PK).Contains(line.PK)))
		{
			parentDeclarationReader?.SnapshotReaderLogger?.Log(Integration.LogType.Information, $"Invoice Header#{invoiceBO.JZ_InvoiceNumber}: Reverting properties and inner invoice lines.");
			var result = base.PopulateInvoiceDataAndFinaliseImport(invoiceBO, commercialInvoiceHeaderRelatedData, commercialInvoiceCollectioncontent);
			return result;
		}
		else
		{
			parentDeclarationReader.SnapshotReaderLogger.Log(Integration.LogType.Warning, $"Invoice Header#{invoiceBO.JZ_InvoiceNumber}: Fields of this invoice would not be reverted as not all inner invoice lines is merged under current entry header to be reverted. Reverting applicable inner invoice lines.");
			using (SuspendSetters(invoiceBO))
			{
				var invoice = invoiceBO;
				FillCommercialInvoiceLineData(invoice, false, commercialInvoiceCollectioncontent);
				return invoice;
			}
		}
	}

	protected override void FillCommercialInvoiceLineData(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, ZString invoiceNumber, BaseJobComInvoiceLine parentInvoiceLine, bool isStandalone, bool isDeclarationIntegrated, bool supportsChcPivotBetweenInvoiceLineAndPacking)
	{
		if (parentDeclarationReader is null || !parentDeclarationReader.IsUsedForSnapshotReverting || parentDeclarationReader.EntryHeaderToRevert.InvoiceLines.Any(line => line.PK == invoiceLine.PK))
		{
			parentDeclarationReader?.SnapshotReaderLogger?.Log(Integration.LogType.Information, $"Invoice Line#{invoiceLine.JI_LineNo}: Reverting properties and package links.");
			base.FillCommercialInvoiceLineData(invoiceLineData, invoiceLine, invoiceNumber, parentInvoiceLine, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking);
		}
		else
		{
			parentDeclarationReader.SnapshotReaderLogger.Log(Integration.LogType.Warning, $"Invoice Line#{invoiceLine.JI_LineNo}: Fields of this invoice line would not be reverted as it's not merged under current entry header to be reverted. Reverting package links.");
			FillContainerOrPackagesPivots(isStandalone, invoiceLine, invoiceLineData, supportsChcPivotBetweenInvoiceLineAndPacking);
		}
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

	protected override Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader() => new CustomsSupportingInformationCollectionDataObjectReader(logger, helper, parentDeclarationReader: parentDeclarationReader);

	readonly JobDeclarationDataObjectReader parentDeclarationReader;
}
