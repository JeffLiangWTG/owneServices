using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusVehicle = Enterprise.Customs.CH.Business.CusVehicle;
using Vehicle = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Vehicle;

namespace Enterprise.Customs.CH.DataTransfer;

public class CommercialInvoiceHeaderDataObjectWriter : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
{
	public CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
		: base(manager, helper, landedCostDataWriter, relatedEntry)
	{
	}

	protected override void PopulateCountrySpecificLineData(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLineBO)
	{
		if (invoiceLineBO is JobComInvoiceLine line && line.Declaration is JobDeclaration declaration)
		{
			var vehicles = new List<Vehicle>();
			line.Vehicles.Cast<CusVehicle>().ToList().ForEach(vehicle =>
			{
				vehicles.Add(new Vehicle()
				{
					VIN = vehicle.CVH_VehicleIdentificationNumber,
					RegistrationNumber = vehicle.CVH_RegistrationNumber,
					Model = vehicle.CVH_ModelName,
				});
			});

			invoiceLineData.SetVehicleCollection(() => { return vehicles; });
		}
	}
}
