using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using Extensions = Enterprise.Customs.DataTransfer.Universal.Extensions;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSCommercialInvoiceHeaderDataObjectWriter : CommercialInvoiceHeaderDataObjectWriter
	{
		public EMCSCommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var addInfos = base.GetInvoiceLineAddInfoCollection(invoiceLineBO) ?? new List<AddInfo>();

			var bo = (EMCSJobComInvoiceLine)invoiceLineBO;
			Extensions.AddOrUpdate(addInfos, Constants.AddInfo.Keys.WineDetailsComments, bo.JI_WineDetailsComments.Left(EMCSJobComInvoiceLine.Schema.JI_WineDetailsComments_MaxLength));

			return addInfos;
		}
	}
}
