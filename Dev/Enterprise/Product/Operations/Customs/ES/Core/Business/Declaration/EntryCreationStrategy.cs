using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration;

public class EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
{
	public EntryCreationStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
	{
		var result = base.GetKeyForHeaderCore(invoiceLine);
		var line = (JobComInvoiceLine)invoiceLine;

		result.Add(line.InvoiceHeader.JZ_IncoTerm);
		result.Add(line.InvoiceHeader.JZ_RX_NKInvoice_Currency);
		result.Add(line.InvoiceHeader.JZ_ValuationCode);

		result.Add(line.ZG_MethodOfPayment);
		result.Add(line.ZG_MethodOfPayment2);

		return result;
	}

	public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
	{
		var line = (JobComInvoiceLine)invoiceLine;
		var result = base.GetKeyForLine(line);

		result.Add(line.ZG_REAProductCode);
		result.Add(line.UNDGs.FirstItemForBinding[0].DI_DG);
		result.Add(line.JI_StateOrRegionOfOrigin);
		result.Add(line.JI_ZZF_NKTaxType);
		result.Add(line.ZG_AIEMType);
		result.Add(line.ZG_TotalRetailPrice);
		result.Add(line.ZG_ExciseCode);
		result.Add(line.ZG_IsREADirectConsumption);
		result.Add(line.ZG_GlobalWarmingPotential);

		var entryInstruction = (CusEntryInstruction)invoiceLine.EntryInstruction;
		if (IsUCC6 && entryInstruction != null && entryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ)
		{
			result.Add(line.JI_OA_ExporterAddress);
		}

		var exemption = line.ZG_ExciseExemption;
		if (exemption.IsEmpty)
		{
			exemption = "0";
		}
		result.Add(exemption);

		var previousDoc = line.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
		if (previousDoc != null)
		{
			AddResults(result, previousDoc, GetPreviousDocumentKeys());
		}

		result.Add(line.ZG_HasNonRecycledPlastics);

		return result;
	}

	protected override string[] GetPreviousDocumentKeysCore()
	{
		return new string[]
		{
			PreviousDocument.Schema.CSI_SubType,
			PreviousDocument.Schema.CSI_Description,
			PreviousDocument.Schema.CSI_ReferenceNumber,
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_LineNo
		};
	}

	protected override bool EntryLineNeedsToBeSplit(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
	{
		var entryLine2 = (CusEntryLine)entryLine;
		entryLine2.InvoiceLines.Load();
		return !InvoiceLineMeetRequirementsToBeMerged(entryLine2, (JobComInvoiceLine)invoiceLine);
	}
	protected override Customs.Business.CusEntryLine GetEntryLineFromSplitIfItCanBeReused(BaseJobComInvoiceLine invoiceLine, List<Customs.Business.CusEntryLine> splitEntryLines)
	{
		foreach (CusEntryLine entryLine in splitEntryLines)
		{
			entryLine.InvoiceLines.Load();
			if (InvoiceLineMeetRequirementsToBeMerged(entryLine, (JobComInvoiceLine)invoiceLine))
			{
				return entryLine;
			}
		}
		return null;
	}

	bool InvoiceLineMeetRequirementsToBeMerged(CusEntryLine entryLine, JobComInvoiceLine invLine)
	{
		return GetTotalContainers(entryLine.ContainersPivot, invLine.ContainersPivot) < 100 &&
			   !IsOver9PackageTypes(entryLine, invLine) &&
			   GetTotalSupportingDocuments(entryLine, invLine) <= MaxSupportingDocumentTotal(invLine) &&
			   !MaxVINExceed(entryLine, invLine) &&
			   !ProcedureIsOutOfOutwardProcessing(invLine);
	}

	int MaxSupportingDocumentTotal(JobComInvoiceLine invLine) => invLine.EntryInstruction?.IsEXS ?? false ? 10 : 99;

	ZBool MaxVINExceed(CusEntryLine entryLine, JobComInvoiceLine newInvoiceLine)
	{
		var numberVehicles = 0;
		foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
		{
			numberVehicles += invoiceLine.Vehicles.Cast<CusVehicle>().Count();
		}
		if (!entryLine.InvoiceLines.Contains(newInvoiceLine))
		{
			numberVehicles += newInvoiceLine.Vehicles.Cast<CusVehicle>().Count();
		}

		return numberVehicles > 99 ? ZBool.True : ZBool.False;
	}

	ZBool ProcedureIsOutOfOutwardProcessing(JobComInvoiceLine invLine) => invLine.CusProcedure?.IsOutOfOutwardProcessing() ?? ZBool.False;

	ZInt GetTotalContainers(NonPersistentCusContainerCollection entryContainers, CusContainersInvoiceLinesCollection invContainers)
	{
		var totalContainersToAdd = 0;
		var entryContList = entryContainers.ToList();
		foreach (CusContainerInvoiceLinePivot container in invContainers)
		{
			if (entryContList.Where(x => ((CusContainerInvoiceLinePivot)x).ContainerNumber.Equals(container.ContainerNumber)).ToList().Count == 0)
			{
				totalContainersToAdd++;
			}
		}
		return (entryContList.Count + totalContainersToAdd);
	}

	ZInt GetTotalSupportingDocuments(CusEntryLine entryLine, JobComInvoiceLine newInvoiceLine)
	{
		var list = new List<string>();
		foreach (SupportingDocument doc in entryLine.Declaration.SupportingDocuments)
		{
			if (!list.Contains(doc.KeyToDeterimeUniqueness))
			{
				list.Add(doc.KeyToDeterimeUniqueness);
			}
		}
		foreach (JobComInvoiceLine invLine in entryLine.InvoiceLines)
		{
			foreach (SupportingDocument doc in invLine.InvoiceHeader.SupportingDocuments)
			{
				if (!list.Contains(doc.KeyToDeterimeUniqueness))
				{
					list.Add(doc.KeyToDeterimeUniqueness);
				}
			}
			foreach (SupportingDocument doc in invLine.SupportingDocuments)
			{
				if (!list.Contains(doc.KeyToDeterimeUniqueness))
				{
					list.Add(doc.KeyToDeterimeUniqueness);
				}
			}
		}
		foreach (SupportingDocument doc in newInvoiceLine.SupportingDocuments)
		{
			if (!list.Contains(doc.KeyToDeterimeUniqueness))
			{
				list.Add(doc.KeyToDeterimeUniqueness);
			}
		}

		return list.Count;
	}

	bool IsOver9PackageTypes(CusEntryLine entryLine, JobComInvoiceLine newInvoiceLine)
	{
		var packagesTypes = new List<ZString>();

		foreach (JobComInvoiceLine line in entryLine.InvoiceLines)
		{
			foreach (InvoiceLinePackagePivot packagePivot in line.PackagesPivot)
			{
				var package = packagePivot.Package;
				var type = package.CW_PackType + package.CW_MarksAndNos;
				if (!packagesTypes.Contains(type))
				{
					packagesTypes.Add(type);
				}
			}
		}
		foreach (InvoiceLinePackagePivot packagePivot in newInvoiceLine.PackagesPivot)
		{
			var package = packagePivot.Package;
			var type = package.CW_PackType + package.CW_MarksAndNos;
			if (!packagesTypes.Contains(type))
			{
				packagesTypes.Add(type);
			}
		}
		return packagesTypes.Count > MaxPackageTypes(newInvoiceLine);
	}

	int MaxPackageTypes(JobComInvoiceLine invLine) => invLine.EntryInstruction?.IsEXS ?? false ? 99 : 9;
}
