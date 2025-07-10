using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : EU.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		public CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;

			UpdateAddInfoCollection(result, Enterprise.Customs.EU.DataTransfer.Universal.Constants.AddInfoKeys.InvoiceLine.EstimatedDutyBreakdown, invoiceLine.JI_Calc_DutyAmount);
			UpdateAddInfoCollection(result, Enterprise.Customs.EU.DataTransfer.Universal.Constants.AddInfoKeys.InvoiceLine.EstimatedVATBreakdown, invoiceLine.JI_Calc_GSTVATAmount);
			UpdateAddInfoCollection(result, Enterprise.Customs.EU.DataTransfer.Universal.Constants.AddInfoKeys.InvoiceLine.EstimatedOtherTaxesBreakdown, invoiceLine.JI_Calc_OtherTaxesAmount);

			return result;
		}

		void UpdateAddInfoCollection(List<AddInfo> addInfoList, ZString key, IZType value)
		{
			if (!value.IsEmpty)
			{
				helper.Update(addInfoList, key, value);
			}
		}
	}
}
