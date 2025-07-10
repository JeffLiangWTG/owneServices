using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNInvoiceHeaderDataObjectWriter : CommercialInvoiceHeaderDataObjectWriter
	{
		public CNInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, CusEntryHeader relatedEntry = null) : base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override List<UniversalDataBuss.DataObjects.Universal.AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO) ?? new List<UniversalDataBuss.DataObjects.Universal.AddInfo>();
			if (invoiceLineBO is JobComInvoiceLine invoiceLineCN)
			{
				result.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
				{
					Key = Constants.AddInfoKeys.InvoiceLine.CIQIngredient,
					Value = invoiceLineCN.CIQIngredient
				});
				result.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
				{
					Key = JobComInvoiceLine.Schema.XC_GoodsSpecModel,
					Value = invoiceLineCN.XC_GoodsSpecModel
				});
				result.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
				{
					Key = JobComInvoiceLine.Schema.XC_GoodsSpecModel2,
					Value = invoiceLineCN.XC_GoodsSpecModel2
				});
			}
			return result;
		}
	}
}
