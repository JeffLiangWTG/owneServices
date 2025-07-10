using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects;

public class EUR1BoxItemsBuilder : EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder
{
	public EUR1BoxItemsBuilder(IEnumerable<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) : base(invoiceLines)
	{
	}

	protected override void BuildCore(ZStringBuilder box8Builder, ZStringBuilder box9Builder, ZStringBuilder box10Builder)
	{
		using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
		{
			int lineNumber = 1;
			var entryHeader = (CusEntryHeader)InvoiceLines.FirstOrDefault()?.CusEntryLine?.Header;
			ZInt totalItemsCount = 0;
			ZDecimal totalItemsWeight = 0;

			if (entryHeader != null)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					var (itemsBox8Text, itemsCount) = GetItemsInfoBox8Row(entryLine, lineNumber);
					box8Builder.Append(itemsBox8Text);
					totalItemsCount += itemsCount;

					var (grossMassText, totalWeight) = GetGrossMassRow(entryLine);
					box9Builder.Append(grossMassText);
					totalItemsWeight += totalWeight;

					box10Builder.AppendIfNotEmpty(GetBox10Row(entryLine));

					lineNumber++;
				}
			}

			box8Builder.AppendIfBuilderIsNotEmpty(GetBox8Summary(totalItemsCount, totalItemsWeight));
		}
	}

	ZString GetBox8Summary(ZInt totalItemsCount, ZDecimal totalItemsWeight)
	{
		var box8Summary = new ZStringBuilder();
		box8Summary.Append(FormattableString.Invariant($"{System.Environment.NewLine}{DocumentsResStrings.GoodsDescription.TotalGrossWeight} {DecimalHelper.GetESStringDecimalFormat(totalItemsWeight)} Kg {System.Environment.NewLine}"));
		box8Summary.Append(FormattableString.Invariant($"{DocumentsResStrings.GoodsDescription.TotalCaption} {totalItemsCount} {DocumentsResStrings.GoodsDescription.PackagesCaption}"));

		return box8Summary.ToStringWithNewLineBetweenAppends();
	}

	ZString GetBox10Row(CusEntryLine entryLine)
	{
		ZString box10FirstLine = "";
		ZString box10SecondLine = "";

		foreach (SupportingDocument supDoc in entryLine.SupportingDocuments)
		{
			if (esInvoices.Contains(supDoc.CSI_Code))
			{
				box10FirstLine = supDoc.CSI_ReferenceNumber;
				box10SecondLine = supDoc.CSI_DateOfExpiry.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture);
				if (box10SecondLine.IsEmpty)
				{
					box10SecondLine = supDoc.CSI_DateOfIssue.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture);
				}
				break;
			}
		}

		var result = ZString.Empty;
		if (!box10FirstLine.IsEmpty)
		{
			result = box10FirstLine;
			if (!box10SecondLine.IsEmpty)
			{
				result += "\n" + box10SecondLine;
			}
		}
		return result;
	}

	readonly ZString[] esInvoices = { "N380", "N325", "1001", "1003", "N935" };

	(ZString grossMassText, ZDecimal totalWeight) GetGrossMassRow(CusEntryLine entryLine)
	{
		ZDecimal totalWeight = 0;

		foreach (JobComInvoiceLine invLine in entryLine.InvoiceLines)
		{
			totalWeight += invLine.GrossWeightInKG;
		}
		var totalWeightStr = DecimalHelper.GetESStringDecimalFormat(totalWeight);
		return ($"{totalWeightStr} Kg", totalWeight);
	}

	(ZString itemsBox8Text, ZInt itemsCount) GetItemsInfoBox8Row(CusEntryLine entryLine, int lineNumber)
	{
		var fourDigitsTariff = entryLine.RandomLine?.JI_Tariff.Substring(0, 4) ?? ZString.Empty;

		var itemsFirstLine = $"{lineNumber} PDTA: {fourDigitsTariff}";

		var (itemsText, itemsCount) = GetSecondLineInfo(entryLine);

		var box8Row = new ZStringBuilder();
		box8Row.AppendIfNotEmpty(itemsFirstLine);
		box8Row.AppendIfNotEmpty(itemsText);
		box8Row.AppendIfNotEmpty(entryLine.RandomLine?.JI_Description);

		return (box8Row.ToStringWithNewLineBetweenAppends(), itemsCount);
	}

	(ZString itemsText, ZInt itemsCount) GetSecondLineInfo(CusEntryLine entryLine)
	{
		var anyVehicle = entryLine.RandomLine?.Vehicles?.Cast<CusVehicle>().Any() ?? false;
		return !anyVehicle ? GetPackagesInfo(entryLine) : GetVehiclesInfo(entryLine);
	}

	(ZString, ZInt) GetPackagesInfo(CusEntryLine entryLine)
	{
		var result = "";

		var packages = entryLine.PackagingDetails;
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

		foreach (var package in packagesToInclude)
		{
			result += $"{package.Value} {package.Key}, ";
		}

		var resultLength = result.Length;
		if (resultLength > 0)
		{
			result = result.Substring(0, resultLength - 2) + ".";
		}

		return (result, packagesCount);
	}

	(ZString, ZInt) GetVehiclesInfo(CusEntryLine entryLine)
	{
		var result = "";

		var numberVehicles = 0;
		var vehiclesStr = "";
		var vehiclesCount = 0;

		foreach (JobComInvoiceLine line in entryLine.InvoiceLines)
		{
			foreach (CusVehicle vehicle in line.Vehicles)
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
			result = $"{numberVehicles} {frameStr}, {vehiclesStr}.";

			vehiclesCount += numberVehicles;
		}

		return (result, vehiclesCount);
	}
}
