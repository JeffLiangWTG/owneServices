using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

sealed class InventorySelectionHeaderPreviousDocumentFiller
{
	internal InventorySelectionHeaderPreviousDocumentFiller(JobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper = null)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		this.inventoryWrapper = inventoryWrapper;
	}

	internal void AddPreviousDocumentIfRequired()
	{
		var declaration = invoiceLine.Declaration;
		if (declaration is null)
		{
			return;
		}

		if (declaration.IsUCC6AndIsExport)
		{
			AddPreviousDocumentForUcc6Export();
			return;
		}

		if (declaration.IsImport)
		{
			AddPreviousDocumentForImport();
		}
	}

	#region Implementation

	void AddPreviousDocumentForUcc6Export()
	{
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();

		previousDocument.CSI_Code = inventoryWrapper?.QuantityToDraw.GetValueOrNullIfZero() is null
			&& invoiceLine.JI_PreviousEntryNumber.Contains(DashSeparator)
			? UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration
			: UniversalReferenceConstants.RefCusCodeListTypes.DeclarationOrNotificationMrn;

		previousDocument.CSI_ReferenceNumber = invoiceLine.JI_PreviousEntryNumber;
		previousDocument.CSI_Quantity = invoiceLine.JI_Weight;
		previousDocument.CSI_UnitOfQuantity = invoiceLine.JI_WeightUQ;
		previousDocument.CSI_ItemNumber = invoiceLine.JI_PreviousEntryLineNumber;
		FillPreviousDocumentPackDetails(previousDocument);
	}

	void AddPreviousDocumentForImport()
	{
		if (invoiceLine.JI_PreviousEntryNumber.Contains(DashSeparator))
		{
			AddPreviousDocumentForImportHavingDashInPreviousEntryNumber();
			return;
		}
		AddPreviousDocumentForImportNotHavingDashInPreviousEntryNumber();
	}

	void AddPreviousDocumentForImportHavingDashInPreviousEntryNumber()
	{
		var entryInfo = invoiceLine.JI_PreviousEntryNumber.Split(DashSeparator);
		const int numberOfDataElementsInPreviousEntryNumber = 4;

		if (entryInfo.Length < numberOfDataElementsInPreviousEntryNumber
			|| entryInfo.Take(numberOfDataElementsInPreviousEntryNumber).All(x => x.IsEmpty))
		{
			return;
		}

		var previousDocument = invoiceLine.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.AltriDocumenti;
		previousDocument.CSI_Procedure = entryInfo[0].Left(previousDocument.CSI_ProcedureInfo.MaxLength);
		previousDocument.CSI_ReferenceNumber = entryInfo[1].KeepNumericCharacters();
		previousDocument.CSI_DateOfIssue = ZDateTimeHelper.ParseToDateDDMMYYYYSafe(entryInfo[2]);

		var customsOfficePart = entryInfo[3];
		if (!customsOfficePart.IsEmpty)
		{
			previousDocument.CSI_CustomsOffice = customsOfficePart
				.InsertSafe(0, Core.Constants.CountryCodes.Italy)
				.Left(previousDocument.CSI_CustomsOfficeInfo.MaxLength);
		}

		FillPreviousDocumentForImport(previousDocument);
	}

	void AddPreviousDocumentForImportNotHavingDashInPreviousEntryNumber()
	{
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();

		previousDocument.CSI_Procedure = ImportPreviousDocumentProcedureList.Codes.DichiarazioneNotificaMrn;
		previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.Mrn;
		previousDocument.CSI_ReferenceNumber2 = invoiceLine
			.JI_PreviousEntryNumber
			.Left(previousDocument.CSI_ReferenceNumber2Info.MaxLength);

		FillPreviousDocumentForImport(previousDocument);
	}

	void FillPreviousDocumentForImport(PreviousDocument previousDocument)
	{
		previousDocument.CSI_LineNo = invoiceLine.JI_PreviousEntryLineNumber;
		previousDocument.CSI_Quantity = invoiceLine.JI_NetWeight;
		previousDocument.CSI_UnitOfQuantity = invoiceLine.JI_NetWeightUQ;
		previousDocument.CSI_Quantity3 = invoiceLine.JI_Weight;
		previousDocument.CSI_UnitOfQuantity3 = invoiceLine.JI_WeightUQ;
		FillPreviousDocumentPackDetails(previousDocument);
	}

	void FillPreviousDocumentPackDetails(PreviousDocument previousDocument)
	{
		var package = invoiceLine.PackagesPivot[0]?.Package;
		if (package is null)
		{
			return;
		}

		previousDocument.CSI_PackQty = package.CW_PackQty;
		previousDocument.CSI_PackType = package.CW_PackType;
	}

	#endregion

	const string DashSeparator = "-";
	readonly JobComInvoiceLine invoiceLine;
	readonly WhsInventoryWrapper inventoryWrapper;
}
