using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DUAExportVehiclePackagesInfoWrapper : IVehiclePackagesInfoCommon
{
	public DUAExportVehiclePackagesInfoWrapper(CusEntryLine cusEntryLine)
	{
		entryLine = Argument.NotNull(cusEntryLine, "CusEntryLine cannot be null");
		Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
	}

	readonly CusEntryLine entryLine;

	public IReadOnlyCollection<IVehicleCommon> Packages
	{
		get
		{
			if (packages == null)
			{
				var packs = new List<VehicleCommonWrapper>();

				entryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => packs.Add(new VehicleCommonWrapper(vehicle.CVH_VehicleIdentificationNumber, vehicle.CVH_BrandName, vehicle.CVH_ModelName))));
				packages = packs.AsReadOnly();
			}
			return packages;
		}
	}
	ReadOnlyCollection<VehicleCommonWrapper> packages;
}
