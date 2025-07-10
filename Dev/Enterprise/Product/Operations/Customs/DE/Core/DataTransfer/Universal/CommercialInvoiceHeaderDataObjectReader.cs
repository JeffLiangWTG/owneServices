using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.DataTransfer.Universal;

class CommercialInvoiceHeaderDataObjectReader : EU.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader
{
	public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null, Type invoiceType = null) : base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, invoiceType)
	{
	}

	protected override Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader()
	{
		return new CustomsSupportingInformationCollectionDataObjectReader(logger, helper);
	}
}
