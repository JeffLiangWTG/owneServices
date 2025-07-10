using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.NCTS.DataTransfer;

public class DepartureGoodsItemDataObjectWriter : EU.NCTS.DataTransfer.Phase4.DepartureGoodsItemDataObjectWriter
{
	public DepartureGoodsItemDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData, CommercialInvoiceHeader commercialInvoiceHeaderData) : base(manager, helper, headerData, commercialInvoiceHeaderData)
	{
	}

	protected override CommercialInvoiceLine PopulateCommercialInvoiceLineData(EU.NCTS.Business.NctsDepartureCargoDesc goodsItemBO)
	{
		var commercialInvoiceLine = base.PopulateCommercialInvoiceLineData(goodsItemBO);
		if (goodsItemBO is NctsDepartureCargoDesc itGoodsItemBO)
		{
			commercialInvoiceLine.SetTaxOrFeeCollection(() => ProcessCollection(itGoodsItemBO.Fees, new DepartureGoodsItemFeeDataObjectWriter(writeManager)));
		}
		return commercialInvoiceLine;
	}

	protected override List<CustomsSupportingInformation> GetInvoiceCustomsSupportingInformationCollectionCore(EU.NCTS.Business.NctsDepartureCargoDesc goodsItemBO)
	{
		var customsSupportingInformationCollection = base.GetInvoiceCustomsSupportingInformationCollectionCore(goodsItemBO);
		if (goodsItemBO is NctsDepartureCargoDesc itGoodsItemBO)
		{
			customsSupportingInformationCollection = customsSupportingInformationCollection ?? new List<CustomsSupportingInformation>();
			PopulateRemarksSupportingInfo(customsSupportingInformationCollection, itGoodsItemBO);
		}
		return customsSupportingInformationCollection;
	}

	void PopulateRemarksSupportingInfo(List<CustomsSupportingInformation> customsSupportingInformationCollection, NctsDepartureCargoDesc itGoodsItemBO)
	{
		var remarksSupportingInfo = new CustomsSupportingInformation()
		{
			Category = new CodeDescriptionPair() { Code = CustomsSupportingInformationList.Codes.Remarks, Description = CustomsSupportingInformationList.Descriptions.Remarks },
			Description = itGoodsItemBO.Remarks
		};
		customsSupportingInformationCollection.Add(remarksSupportingInfo);
	}
}
