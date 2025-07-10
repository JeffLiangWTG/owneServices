using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSCommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>
	{
		public EMCSCommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, EMCSJobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void FillCustomizedFields(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine is EMCSJobComInvoiceLine line)
			{
				var addInfos = invoiceLineData.AddInfoCollection ?? new List<AddInfo>();
				if (addInfos.Any())
				{
					var wineDetailsComments = addInfos.GetZStringValue(Constants.AddInfo.Keys.WineDetailsComments) ?? ZString.Empty;
					if (!wineDetailsComments.IsEmpty)
					{
						line.JI_WineDetailsComments = wineDetailsComments.Left(EMCSJobComInvoiceLine.Schema.JI_WineDetailsComments_MaxLength);
					}
				}
			}
		}
	}
}
