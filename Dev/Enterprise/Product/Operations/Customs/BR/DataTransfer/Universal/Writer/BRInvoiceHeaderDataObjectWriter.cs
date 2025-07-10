using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRInvoiceHeaderDataObjectWriter : CommercialInvoiceHeaderDataObjectWriter
	{
		public BRInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, CusEntryHeader relatedEntry = null) : base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO) ?? new List<AddInfo>();

			var invoiceLineBr = invoiceLineBO as JobComInvoiceLine;
			if (invoiceLineBr != null && !invoiceLineBr.ComplementaryDescription.IsEmpty)
			{
				result.Add(new AddInfo
				{
					Key = Constants.AddInfoKeys.InvoiceLine.ComplementaryDescriptionExport,
					Value = invoiceLineBr.ComplementaryDescription
				});
			}

			return result;
		}

		protected override AdditionalLineTariffDetailDataObjectWriter CreateAdditionalLineTariffDetailDataObjectWriter()
		{
			return new BRAdditionalLineTariffDetailDataObjectWriter(writeManager);
		}
	}
}
