using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.Declaration;

public class CopyDocumentsSelectionHeader(JobComInvoiceLine invoiceLine) : AutoCopyDocumentsSelectionHeader(invoiceLine.Factory)
{
	public CopyDocumentsSelectionLineCollection Lines => lines ??= new CopyDocumentsSelectionLineCollection(InvoiceLine);
	CopyDocumentsSelectionLineCollection lines;

	public JobComInvoiceLine InvoiceLine { get; } = Argument.NotNull(invoiceLine, nameof(invoiceLine));

	public JobDeclaration Declaration { get; } = Argument.NotNull(invoiceLine.Declaration, $"{nameof(invoiceLine)}.{nameof(invoiceLine.Declaration)}");

	public JobComInvoiceHeader TargetInvoice => Factory.Load<JobComInvoiceHeader>(JZ_Invoice);

	[RelatedBusinessObject(nameof(TargetInvoice))]
	public override ZGuid JZ_Invoice
	{
		get => base.JZ_Invoice;
		set => base.JZ_Invoice = value;
	}

	[List($"{nameof(Lookups)}.{nameof(CopyDocumentsSelectionHeaderLookups.Invoices)}")]
	public override ZString InvoiceNumber
	{
		get => base.InvoiceNumber;
		set
		{
			base.InvoiceNumber = value;

			if (InvoiceNumber == Lookups.AllCode)
			{
				JZ_Invoice = Guid.Empty;
				return;
			}

			foreach (JobComInvoiceHeader invoice in Declaration.SortedInvoiceList)
			{
				if (invoice.JZ_InvoiceNumber.EqualsIgnoringCase(InvoiceNumber))
				{
					JZ_Invoice = invoice.PK;
					break;
				}
			}
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		InvoiceNumber = Lookups.AllCode;
	}

	public void SelectAll()
	{
		foreach (var line in Lines.Cast<CopyDocumentsSelectionLine>())
		{
			line.IsSelected = true;
		}
	}

	public CopyDocumentsSelectionHeaderLookups Lookups => fLookups ??= new(this);
	CopyDocumentsSelectionHeaderLookups fLookups;

	public void Copy()
	{
		BusinessObjectCollection<BaseJobComInvoiceLine> targetInvoiceLines;
		if (InvoiceNumber == Lookups.AllCode)
		{
			targetInvoiceLines = Declaration.InvoiceLines;
		}
		else if (TargetInvoice is not null)
		{
			targetInvoiceLines = TargetInvoice.InvoiceLines;
		}
		else
		{
			return;
		}

		foreach (var info in Lines.Cast<CopyDocumentsSelectionLine>().Where(e => e.IsSelected))
		{
			foreach (JobComInvoiceLine targetInvoiceLine in targetInvoiceLines)
			{
				if (targetInvoiceLine == InvoiceLine)
				{
					continue;
				}
				TryAddAsSupportingDocument(info, targetInvoiceLine);
				TryAddAsAdditionalDocument(info, targetInvoiceLine);
			}
		}
	}

	static void TryAddAsSupportingDocument(CopyDocumentsSelectionLine line, JobComInvoiceLine target)
	{
		if (line.Info is not SupportingDocument info)
		{
			return;
		}

		if (!target.SupportingDocuments.Cast<SupportingDocument>()
				.Any(e => e.CSI_Code == line.CSI_Code && e.CSI_ReferenceNumber == line.CSI_ReferenceNumber))
		{
			var supportingDocument = target.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = info.CSI_ReferenceNumber;
			supportingDocument.CSI_UnitOfQuantity = info.CSI_UnitOfQuantity;
			supportingDocument.CSI_Code = info.CSI_Code;
			supportingDocument.CSI_DateOfIssue = info.CSI_DateOfIssue;

			if (target.IsExport)
			{
				supportingDocument.CSI_RX_NKCurrency = info.CSI_RX_NKCurrency;
				supportingDocument.CSI_ReferenceNumber2 = info.CSI_ReferenceNumber2;
				supportingDocument.CSI_DateOfExpiry = info.CSI_DateOfExpiry;
				supportingDocument.CSI_Description = info.CSI_Description;
				supportingDocument.CSI_SubType = info.CSI_SubType;
				supportingDocument.CSI_UnitOfQuantity2 = info.CSI_UnitOfQuantity2;
				supportingDocument.CSI_AdditionalDescription = info.CSI_AdditionalDescription;
			}
			else if (target.IsImport)
			{
				supportingDocument.CSI_Status = info.CSI_Status;
			}
		}
	}

	static void TryAddAsAdditionalDocument(CopyDocumentsSelectionLine line, JobComInvoiceLine target)
	{
		if (line.Info is not AdditionalInfo info)
		{
			return;
		}

		if (info.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && target.AdditionalInfos.Cast<AdditionalInfo>().Any(e => e.CSI_Code == line.CSI_Code))
		{
			return;
		}

		if (target.AdditionalInfos.Cast<AdditionalInfo>()
				.Any(e => e.CSI_Code == line.CSI_Code && e.CSI_ReferenceNumber == line.CSI_ReferenceNumber))
		{
			return;
		}

		var additionalInfo = target.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = info.CSI_SubType;
		additionalInfo.CSI_Code = info.CSI_Code;
		additionalInfo.CSI_ReferenceNumber = info.CSI_ReferenceNumber;
		additionalInfo.CSI_Description = info.CSI_Description;

		if (target.IsExport)
		{
			additionalInfo.CSI_RX_NKCurrency = info.CSI_RX_NKCurrency;
			additionalInfo.CSI_ReferenceNumber2 = info.CSI_ReferenceNumber2;
		}
	}
}
