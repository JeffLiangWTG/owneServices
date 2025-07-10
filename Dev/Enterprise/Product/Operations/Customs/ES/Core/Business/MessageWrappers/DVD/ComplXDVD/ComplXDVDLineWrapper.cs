using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXDVDLineWrapper : IComplXDVDLine
	{
		public ComplXDVDLineWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}
		readonly CusEntryLine entryLine;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public IReadOnlyCollection<IDVDCommonPackage> Packages => packages ?? (packages = DVDCommonPackageWrapper.GetPackagesList(entryLine, canSendFRPackages: true, canSendNEPackageQty: false));
		IReadOnlyCollection<DVDCommonPackageWrapper> packages;

		public IReadOnlyCollection<IVehicleCommon> Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					var vehiclesList = new List<VehicleCommonWrapper>();
					entryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => vehiclesList.Add(new VehicleCommonWrapper(vehicle.CVH_VehicleIdentificationNumber, vehicle.CVH_BrandName, vehicle.CVH_ModelName))));

					vehicles = vehiclesList.AsReadOnly();
				}
				return vehicles;
			}
		}
		IReadOnlyCollection<VehicleCommonWrapper> vehicles;

		public ZString DepositUnitOfMeasureCodeEU => entryLine.ThirdUQ;

		public ZDecimal DepositUnitOfMeasureQuantity => entryLine.ThirdQuantity.Round(3);

		public ZDecimal GrossMassKg => entryLine.EffectiveGrossWeight.InUnroundedKilogramsSafe;

		public ZDecimal NetMassKg => entryLine.EffectiveCustomsWeight.InKilogramsSafe.Round(3);

		public ZDecimal SupplementaryQuantity => entryLine.SupplementaryQuantity.Round(3);
	}
}
