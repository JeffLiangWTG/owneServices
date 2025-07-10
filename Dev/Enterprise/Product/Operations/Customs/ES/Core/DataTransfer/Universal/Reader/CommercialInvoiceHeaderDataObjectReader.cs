using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : EU.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader
	{
		public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, EU.Business.Declaration.JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);

			if (invoiceLineData.AddInfoCollection != null)
			{
				var provinceOfOriginAddInfo = invoiceLineData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin, logger);

				if (provinceOfOriginAddInfo.HasValue)
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_StateOrRegionOfOrigin, provinceOfOriginAddInfo, delaySetters);
				}
			}

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
}
