using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Vehicle = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Vehicle;

namespace Enterprise.Customs.ES.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : EU.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
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
						Brand = vehicle.CVH_BrandName,
						Model = vehicle.CVH_ModelName,
					});
				});

				invoiceLineData.SetVehicleCollection(() => { return vehicles; });
			}
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;

			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin, invoiceLine.JI_StateOrRegionOfOrigin);

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
