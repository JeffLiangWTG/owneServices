using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ExpeditionLineWrapper : IExpeditionLine
{
	public ExpeditionLineWrapper(CusEntryLine cusEntryLine)
	{
		entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
		Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
	}
	readonly CusEntryLine entryLine;

	public ZInt LineNumber => entryLine.CL_LineNumber;

	public ZString GoodsCode => entryLine.RandomLine.JI_Tariff.SubstringSafe(0, (entryLine.RandomLine.JI_Tariff.Length < 8 ? entryLine.RandomLine.JI_Tariff.Length : 8));

	public ZString GoodsDescription => entryLine.RandomLine.JI_Description;

	public ZDecimal GrossWeightInKG
	{
		get
		{
			var grossWeight = entryLine.EffectiveGrossWeight.InKilogramsSafe;
			return grossWeight > 1 ? (ZDecimal)Math.Ceiling(grossWeight) : grossWeight;
		}
	}

	public ZDecimal NetWeightInKG => entryLine.EffectiveCustomsWeight.InKilogramsSafe.Round(3);

	public IReadOnlyCollection<IPackageCommonNumbers> Packages
	{
		get
		{
			if (packages == null)
			{
				packages = PackageWrapper.GetPackagesList(entryLine);
			}
			return packages;
		}
	}
	IReadOnlyCollection<PackageWrapper> packages;

	public IReadOnlyCollection<ZString> Containers => containers ?? (containers = entryLine.Containers.ToList().AsReadOnly());
	IReadOnlyCollection<ZString> containers;

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
	ReadOnlyCollection<VehicleCommonWrapper> vehicles;

	public IReadOnlyCollection<IExpeditionDocumentSubmitted> DocumentsSubmitted
	{
		get
		{
			if (documentsSubmitted == null)
			{
				var documentsSubmittedList = new List<ExpeditionDocumentSubmittedWrapper>();

				documentsSubmittedList.AddRange(entryLine.SupportingDocuments.Cast<SupportingDocument>()
																				.Select(doc => new ExpeditionDocumentSubmittedWrapper(doc)));

				documentsSubmittedList.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>()
																					.Select(doc => new ExpeditionDocumentSubmittedWrapper(doc)));

				documentsSubmitted = documentsSubmittedList.AsReadOnly();
			}
			return documentsSubmitted;
		}
	}
	IReadOnlyCollection<ExpeditionDocumentSubmittedWrapper> documentsSubmitted;
}
