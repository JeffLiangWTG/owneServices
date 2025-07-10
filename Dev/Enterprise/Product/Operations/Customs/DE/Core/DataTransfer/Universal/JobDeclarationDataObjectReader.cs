using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.DataTransfer.Universal;

public class JobDeclarationDataObjectReader : EU.DataTransfer.Universal.JobDeclarationDataObjectReader
{
	public JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment) : base(declarationDataObject, logger, factory, forwardingShipment)
	{
	}

	protected override IEnumerable<ZString> GetSettingOrder(JobDeclaration declaration)
	{
		var result = base.GetSettingOrder(declaration).ToList();
		var vesselNameKey = ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_VesselName);
		var masterBillKey = ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_MasterBill);

		var vesselNameIndex = result.IndexOf(vesselNameKey);
		var masterBillIndex = result.IndexOf(masterBillKey);

		if (vesselNameIndex != -1 && masterBillIndex != -1 && masterBillIndex < vesselNameIndex)
		{
			result[vesselNameIndex] = masterBillKey;
			result[masterBillIndex] = vesselNameKey;
		}

		return result;
	}

	protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
	{
		return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
	}
}
