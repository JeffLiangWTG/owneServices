using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>
	{
		internal CommercialInvoiceHeaderDataObjectReader(UniversalCustoms.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, BaseJobComInvoiceGroupHeader groupHeader, Shipment dataObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, dataObject, landedCostDataReader, invoiceType: typeof(JobComInvoiceHeader))
		{
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoiceLine(BaseJobComInvoiceLine invoiceLine, UniversalCustoms.CommercialInvoiceLine invoiceLineData)
		{
			return new CommercialInvoiceLineAddInfoDataObjectReader(logger, helper, JobComInvoiceLineSchema.JI_AddInfo);
		}

		protected override void FillBondedWarehouseProperties(UniversalCustoms.CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
		}

		protected override DataTransfer.Universal.AddInfoGroupCollectionDataObjectReader GetNewAddInfoGroupCollectionDataObjectReader() => new AddInfoGroupCollectionDataObjectReader(logger, helper);
	}
}
