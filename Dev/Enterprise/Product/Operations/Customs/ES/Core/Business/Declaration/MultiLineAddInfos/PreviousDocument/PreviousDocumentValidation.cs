using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration;

public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
{
	public PreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	protected override bool IsSubTypeMandatory => false;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();

		if (Parent.Parent.GetType() == typeof(JobDeclaration))
		{
			CheckMaxPreviousDocument(Parent);
			CheckChildrenPreviousDocument(Parent);
		}

		ValidateWhenHasEntryLineAndIsImportPOUST2C(Parent);
	}

	void CheckMaxPreviousDocument(PreviousDocument parent)
	{
		var dec = (JobDeclaration)parent.Parent;
		if (dec.PreviousDocuments.Count > 1)
		{
			parent.AddRowMessageError(Res.GetString("95181E20-6267-42D7-886B-AB35B1B76E5B", "Customs will not accept a declaration with more than 1 previous document for Box 40 per line"));
		}
	}

	void CheckChildrenPreviousDocument(PreviousDocument parent)
	{
		var dec = (JobDeclaration)parent.Parent;
		var allHeadersHavePreviousDocuments = true;
		var allInvoiceLinesHavePreviousDocuments = true;
		foreach (JobComInvoiceHeader header in dec.Invoices)
		{
			if (header.PreviousDocuments.Count == 0)
			{
				allHeadersHavePreviousDocuments = false;
			}
			foreach (JobComInvoiceLine invLine in header.InvoiceLines)
			{
				if (invLine.PreviousDocuments.Count == 0)
				{
					allInvoiceLinesHavePreviousDocuments = false;
				}
			}
		}
		if (allHeadersHavePreviousDocuments || allInvoiceLinesHavePreviousDocuments)
		{
			parent.AddRowMessageError(Res.GetString("3BCCE632-F5D0-421C-81FA-81EDAE592899", "This previous document will not be declared as all invoice lines have its own previous document for Box 40"));
		}
	}

	void ValidateWhenHasEntryLineAndIsImportPOUST2C(PreviousDocument prevDoc)
	{
		var parent = prevDoc.Parent;
		var entryLines = GetEntryline(parent);
		if (entryLines.Count > 0 && (entryLines[0].Declaration?.IsImport ?? false))
		{
			foreach (var entryLine in entryLines)
			{
				var entryHeader = entryLine.Header;
				var instruction = entryHeader.EntryInstruction;
				if (entryHeader.ZG_POUSVersion > 0 && instruction != null && instruction.IsT2C)
				{
					CheckNoEmptyDataWhenMoreThanOne(prevDoc, entryLine);
					CheckNoEmptyDataWhenMultiplePackageTypes(prevDoc, entryLine);
				}
			}
		}
	}

	List<CusEntryLine> GetEntryline(BusinessObject parent)
	{
		var entryLines = new List<CusEntryLine>();

		if (parent is JobDeclaration declaration)
		{
			declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault()?.AllEntryLines.ForEach(x => entryLines.Add(x));
		}
		else if (parent is JobComInvoiceHeader header)
		{
			var entryLine = (CusEntryLine)header.FirstEntryHeader?.AllEntryLines.FirstOrDefault();
			if (entryLine != null)
			{
				entryLines.Add(entryLine);
			}
		}
		else if (parent is JobComInvoiceLine invLine)
		{
			var entryLine = invLine.CusEntryLine;
			if (entryLine != null)
			{
				entryLines.Add(entryLine);
			}
		}

		return entryLines;
	}

	void CheckNoEmptyDataWhenMoreThanOne(PreviousDocument prevDoc, CusEntryLine entryLine)
	{
		if (entryLine.PreviousDocuments.Count() > 1)
		{
			AddMessageErrorIfUnitOfQuantityOrPackTypeOrPackQtyAreEmpty(prevDoc, Res.GetString("082156ED-CCAE-4E7F-90ED-89138907D254", "If there is more than one previous document per entry line, then Quantity, Pack Type and Pack Qty must be filled."));
		}
	}

	void CheckNoEmptyDataWhenMultiplePackageTypes(PreviousDocument prevDoc, CusEntryLine entryLine)
	{
		if (entryLine.PackagingDetails.GroupBy(x => x.Package.CW_PackType).Count() > 1)
		{
			AddMessageErrorIfUnitOfQuantityOrPackTypeOrPackQtyAreEmpty(prevDoc, Res.GetString("C2F18DC9-4E04-44B5-855F-E24919CD3F79", "If there is more than one Package Type per entry line, then Quantity, Pack Type and Pack Qty must be filled."));
		}
	}

	void AddMessageErrorIfUnitOfQuantityOrPackTypeOrPackQtyAreEmpty(PreviousDocument prevDoc, string message)
	{
		var packType = prevDoc.CSI_PackType;
		if (packType.IsEmpty || (prevDoc.CSI_PackQty.IsEmpty && !PackageHelper.PackTypeIsBulk(packType, prevDoc.Factory)) || prevDoc.CSI_Quantity.IsEmpty)
		{
			prevDoc.AddRowMessageError(message);
		}
	}
}
