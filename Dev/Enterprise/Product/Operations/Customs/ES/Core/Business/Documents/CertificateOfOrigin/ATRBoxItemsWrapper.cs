using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin;

public class ATRBoxItemsWrapper : EU.Business.Documents.CertificateOfOrigin.ATRBoxItemsWrapper
{
	public ATRBoxItemsWrapper(IEnumerable<BaseJobComInvoiceLine> invoiceLines) : base(invoiceLines)
	{
	}

	protected override ZString GetItemsInfoBox10Row(EU.Business.Declaration.JobComInvoiceLine line)
	{
		using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
		{
			var fourDigitsTariff = line.JI_Tariff.Substring(0, 4);
			var itemsFirstLine = $"PDTA: {fourDigitsTariff}";
			var itemsText = GetSecondLineInfo((JobComInvoiceLine)line);

			var descriptionRow = new ZStringBuilder();
			descriptionRow.AppendIfNotEmpty(itemsFirstLine);
			descriptionRow.AppendIfNotEmpty(itemsText);
			descriptionRow.AppendIfNotEmpty(line.JI_Description.ToUpperInvariant());

			return descriptionRow.ToStringWithNewLineBetweenAppends();
		}
	}

	protected override ZString GetGrossMassRow(EU.Business.Declaration.JobComInvoiceLine line)
	{
		var totalWeightStr = DecimalHelper.GetESStringDecimalFormat(line.EffectiveGrossWeight.InKilogramsSafe);

		return totalWeightStr + " KG";
	}

	protected override ZString GetVolume(EU.Business.Declaration.JobComInvoiceLine line) => ZString.Empty;

	ZString GetSecondLineInfo(JobComInvoiceLine line)
	{
		var anyVehicle = line.Vehicles?.Cast<CusVehicle>().Any() ?? false;
		return !anyVehicle ? GetPackagesInfo(line) : GetVehiclesInfo(line);
	}

	ZString GetPackagesInfo(JobComInvoiceLine line)
	{
		var packages = line.CusEntryLine.PackagingDetails;
		var packagesCount = 0;
		var packagesToInclude = new Dictionary<ZString, ZInt>();

		foreach (var package in packages)
		{
			var packQty = package.CHC_NumberOfPacks;
			var packageKey = $"{package.Package.CW_PackType}, {package.Package.CW_MarksAndNos}";
			if (!packagesToInclude.ContainsKey(packageKey))
			{
				packagesToInclude.Add(packageKey, packQty);
			}
			else
			{
				packagesToInclude[packageKey] += packQty;
			}
			packagesCount += packQty;
		}

		var packageRow = new ZStringBuilder();

		foreach (var package in packagesToInclude)
		{
			packageRow.AppendIfNotEmpty($"{package.Value} {package.Key.ToUpperInvariant()}.");
		}

		return packageRow.ToStringWithNewLineBetweenAppends();
	}

	ZString GetVehiclesInfo(JobComInvoiceLine line)
	{
		var result = "";

		var numberVehicles = 0;
		var vehiclesStr = "";
		var vehiclesCount = 0;

		foreach (JobComInvoiceLine invLine in line.CusEntryLine.InvoiceLines)
		{
			foreach (CusVehicle vehicle in invLine.Vehicles)
			{
				numberVehicles++;
				vehiclesStr += $"{vehicle.CVH_VehicleIdentificationNumber} {vehicle.CVH_BrandName} {vehicle.CVH_ModelName}, ";
			}
		}

		var vehiclesLength = vehiclesStr.Length;
		if (vehiclesLength > 0)
		{
			vehiclesStr = vehiclesStr.Substring(0, vehiclesLength - 2);

			var frameStr = numberVehicles > 1 ? DocumentsResStrings.GoodsDescription.PluralFramesCaption : DocumentsResStrings.GoodsDescription.SingularFrameCaption;
			result = $"{numberVehicles} {frameStr.ToUpperInvariant()}, {vehiclesStr.ToUpperInvariant()}.";

			vehiclesCount += numberVehicles;
		}

		return result;
	}
}
