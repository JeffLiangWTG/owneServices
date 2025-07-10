using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DataTransfer;
public class CommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
{
	public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null, Type invoiceType = null)
		: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, invoiceType: invoiceType)
	{
	}

	protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
	{
		base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);

		var invoiceLinePK = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
		ImportVehicle(invoiceLineData, invoiceLinePK, JobComInvoiceLineSchema.Constants.Prefix, invoiceLineIsInDatabase, invoiceLineData);
	}

	void ImportVehicle(CommercialInvoiceLine invoiceLineData, ZGuid invoiceLinePK, ZString tablePrefix, bool invoiceLineIsInDatabase, CommercialInvoiceLine realDataObject)
	{
		if (invoiceLineData.VehicleCollection != null)
		{
			new VehicleCollectionDataObjectReader(logger, helper).ReadIntoDataRows(invoiceLinePK, tablePrefix, invoiceLineIsInDatabase, invoiceLineData);
		}
	}
}

