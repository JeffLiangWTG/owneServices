using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class PreviousDocumentApportionedCollection
{
	PreviousDocumentApportionedCollection(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	public static PreviousDocumentApportionedCollection LoadNew(JobDeclaration jobDeclaration)
	{
		var result = new PreviousDocumentApportionedCollection(jobDeclaration);
		result.PerformApportion();
		return result;
	}

	public IEnumerable<MergedPreviousDocument> this[string entryLinePK]
	{
		get
		{
			if (!ZGuid.TryParse(entryLinePK, out var parsedGuid) || !parsedGuid.IsValid)
			{
				throw new ArgumentException(FormattableString.Invariant($"The parameter {nameof(entryLinePK)} is not a valid guid."));
			}
			else if (!allEntryLinesWithMergedDocumentsDictionary.TryGetValue(parsedGuid, out var mergedPreviousDocumentsForEntryLine))
			{
				throw new ArgumentException(FormattableString.Invariant($"The parameter {nameof(entryLinePK)} is not the PK of any of the registered entry lines."));
			}
			else
			{
				return mergedPreviousDocumentsForEntryLine.Values.ToArray();
			}
		}
	}

	#region Implementation

	Dictionary<ZGuid, Dictionary<ZString, MergedPreviousDocument>> allEntryLinesWithMergedDocumentsDictionary;

	void PerformApportion()
	{
		var allEntryLines = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.MergedLines).ToArray();
		InitializeDictionary(allEntryLines);

		DistributeJobLevelPreviousDocuments(allEntryLines);
		DistributeInvoiceLevelPreviousDocuments();
		DistributeInvoiceLineLevelPreviousDocuments(allEntryLines);
	}

	void DistributeInvoiceLineLevelPreviousDocuments(CusEntryLine[] entryLines)
	{
		foreach (var entryLine in entryLines)
		{
			var previouslyManagedDocumentsForCurrentEntryLine = allEntryLinesWithMergedDocumentsDictionary[entryLine.PK];
			var previousDocumentsOfInvoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()).ToArray();
			foreach (var previousDocument in previousDocumentsOfInvoiceLines)
			{
				var mergedDocument = MergedPreviousDocument.FromPreviousDocument(previousDocument);
				AddNewOrUpdateExistingItem(previouslyManagedDocumentsForCurrentEntryLine, mergedDocument);
			}
		}
	}

	void DistributeInvoiceLevelPreviousDocuments()
	{
		var invoicePreviousDocuments = declaration.Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()).ToArray();
		foreach (var invoiceDocument in invoicePreviousDocuments)
		{
			var invoice = (JobComInvoiceHeader)invoiceDocument.ImportExportParent;
			var entryLinesRelatedToTheDocument = invoice.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.CusEntryLine != null).Select(x => x.CusEntryLine).Cast<CusEntryLine>().Distinct().ToArray();
			DistributeToEntryLines(entryLinesRelatedToTheDocument, invoiceDocument);
		}
	}

	void DistributeJobLevelPreviousDocuments(CusEntryLine[] entryLines)
	{
		var declarationPreviousDocuments = declaration.PreviousDocuments.Cast<PreviousDocument>().ToArray();
		foreach (var previousDocument in declarationPreviousDocuments)
		{
			DistributeToEntryLines(entryLines, previousDocument);
		}
	}

	void DistributeToEntryLines(CusEntryLine[] entryLines, PreviousDocument previousDocument)
	{
		var entryLinesConsideredTotalAmount = previousDocument.IsSummaryDeclarationDocument ? entryLines.Sum(x => x.EffectiveGrossWeight.InKilogramsSafe) : entryLines.Sum(x => x.EffectiveNetWeight.InKilogramsSafe);
		var documentQuantitiesToDistribute = new Portion(previousDocument.EffectiveNetMass.InKilogramsSafe, previousDocument.CSI_Quantity2, previousDocument.EffectiveGrossMass.InKilogramsSafe, previousDocument.CSI_PackQty);
		var numberOfEntryLines = entryLines.Length;
		for (int i = 0; i < numberOfEntryLines; i++)
		{
			var entryLine = entryLines[i];
			var mergedDocument = MergedPreviousDocument.FromPreviousDocumentButQuantities(previousDocument);
			if (i < numberOfEntryLines - 1)
			{
				var entryLineConsideredAmount = previousDocument.IsSummaryDeclarationDocument ? entryLine.EffectiveGrossWeight.InKilogramsSafe : entryLine.EffectiveNetWeight.InKilogramsSafe;
				var apportionedQuantitiesForCurrentEntryLine = DoApportion(documentQuantitiesToDistribute, entryLineConsideredAmount, entryLinesConsideredTotalAmount);
				documentQuantitiesToDistribute.SubstractQuantities(apportionedQuantitiesForCurrentEntryLine.NetMass, apportionedQuantitiesForCurrentEntryLine.SupplementaryQuantity, apportionedQuantitiesForCurrentEntryLine.GrossMass, apportionedQuantitiesForCurrentEntryLine.PackageQuantity);

				mergedDocument.NetMass = apportionedQuantitiesForCurrentEntryLine.NetMass;
				mergedDocument.SupplementaryQuantity = apportionedQuantitiesForCurrentEntryLine.SupplementaryQuantity;
				mergedDocument.GrossMass = apportionedQuantitiesForCurrentEntryLine.GrossMass;
				mergedDocument.PackageQuantity = apportionedQuantitiesForCurrentEntryLine.PackageQuantity;
			}
			else
			{
				mergedDocument.NetMass = documentQuantitiesToDistribute.NetMass;
				mergedDocument.SupplementaryQuantity = documentQuantitiesToDistribute.SupplementaryQuantity;
				mergedDocument.GrossMass = documentQuantitiesToDistribute.GrossMass;
				mergedDocument.PackageQuantity = documentQuantitiesToDistribute.PackageQuantity;
			}
			AddNewOrUpdateExistingItem(allEntryLinesWithMergedDocumentsDictionary[entryLine.PK], mergedDocument);
		}
	}

	void AddNewOrUpdateExistingItem(Dictionary<ZString, MergedPreviousDocument> previouslyManagedDocumentsForCurrentEntryLine, MergedPreviousDocument mergedDocument)
	{
		if (previouslyManagedDocumentsForCurrentEntryLine.TryGetValue(mergedDocument.Key, out var alreadyManagedDocument))
		{
			alreadyManagedDocument.NetMass += mergedDocument.NetMass;
			alreadyManagedDocument.SupplementaryQuantity += mergedDocument.SupplementaryQuantity;
			alreadyManagedDocument.GrossMass += mergedDocument.GrossMass;
			alreadyManagedDocument.PackageQuantity += mergedDocument.PackageQuantity;
		}
		else
		{
			previouslyManagedDocumentsForCurrentEntryLine.Add(mergedDocument.Key, mergedDocument);
		}
	}

	void InitializeDictionary(CusEntryLine[] availableEntryLines)
	{
		allEntryLinesWithMergedDocumentsDictionary = new Dictionary<ZGuid, Dictionary<ZString, MergedPreviousDocument>>();
		foreach (var entryLine in availableEntryLines)
		{
			allEntryLinesWithMergedDocumentsDictionary.Add(entryLine.PK, new Dictionary<ZString, MergedPreviousDocument>());
		}
	}

	Portion DoApportion(Portion apportion, ZDecimal entryLineAmount, ZDecimal totalAmount)
	{
		totalAmount = totalAmount != ZDecimal.Zero ? totalAmount : 1;
		var factor = entryLineAmount / totalAmount;

		ZDecimal CalculateAndRound(ZDecimal value)
		{
			ZDecimal result = value * factor;
			return result.Round(5);
		}

		var quantity = CalculateAndRound(apportion.NetMass);
		var quantity2 = CalculateAndRound(apportion.SupplementaryQuantity);
		var grossMass = CalculateAndRound(apportion.GrossMass);
		var packageQuantity = (ZInt)CalculateAndRound((ZDecimal)apportion.PackageQuantity);
		return new Portion(quantity, quantity2, grossMass, packageQuantity);
	}

	class Portion
	{
		public ZDecimal NetMass { get; private set; }
		public ZDecimal SupplementaryQuantity { get; private set; }
		public ZDecimal GrossMass { get; private set; }
		public ZInt PackageQuantity { get; private set; }

		public Portion(ZDecimal netMass, ZDecimal supplementaryQuantity, ZDecimal grossMass, ZInt packageQuantity)
		{
			NetMass = netMass;
			SupplementaryQuantity = supplementaryQuantity;
			GrossMass = grossMass;
			PackageQuantity = packageQuantity;
		}

		public void SubstractQuantities(ZDecimal netMass, ZDecimal supplementaryQuantity, ZDecimal grossMass, ZInt packageQuantity)
		{
			NetMass -= netMass;
			SupplementaryQuantity -= supplementaryQuantity;
			GrossMass -= grossMass;
			PackageQuantity -= packageQuantity;
		}
	}

	#endregion
}
