using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public partial class ImportFromTemporaryStorageRegisterModuleForm : ZChildForm
{
	public ImportFromTemporaryStorageRegisterModuleForm()
	{
		InitializeComponent();
	}

	public ICusSupportingInfoCollection<CusSupportingInfo> PreviousDocuments
	{
		get;
		set;
	}

	void Cancel_Button_Click(object sender, EventArgs e)
	{
		Close();
	}

	void OK_Button_Click(object sender, EventArgs e)
	{
		var selectedElements = FilterControlPanel.FindSingle<ZFilterStripControl>().FilteredGrid.SelectedElements;
		var selectedElementsLength = selectedElements.Length;

		var firstExistingDoc = PreviousDocuments.FirstOrDefault();
		var existingDocsCount = PreviousDocuments.Count;
		var existsOnlyAndEmptyDoc = ExistsOnlyAndEmptyDoc(existingDocsCount, firstExistingDoc);
		var existingValidDocsCount = existsOnlyAndEmptyDoc ? 0 : existingDocsCount;

		if (selectedElementsLength == 0)
		{
			Globals.Message.ShowError(Res.GetString("C11678E2-FE1A-4488-A4B5-F8B93BD0DEC2", "Please select Register lines."));
		}
		else if (selectedElementsLength + existingValidDocsCount > MaximumNumberOfRecords)
		{
			Globals.Message.ShowError(Res.GetString("36796145-48EF-4D98-9390-BEB7D520EDAA", "Exceeding the maximum records {0}, selected lines count is {1} and existing Docs count is {2}.", MaximumNumberOfRecords, selectedElementsLength, existingValidDocsCount));
		}
		else
		{
			LinkRegLineToCusEntryInstruction(selectedElements.Cast<CusTempStorageRegLine>().ToArray(), PreviousDocuments, existsOnlyAndEmptyDoc ? firstExistingDoc : null);
			Close();
		}
	}

	ZBool ExistsOnlyAndEmptyDoc(ZInt existingDocsCount, CusSupportingInfo firstExistingDoc)
	{
		var result = false;
		if (existingDocsCount == 1)
		{
			if (firstExistingDoc.CSI_SubType.IsEmpty && firstExistingDoc.CSI_ReferenceNumber.IsEmpty && firstExistingDoc.CSI_Quantity.IsEmpty)
			{
				result = true;
			}
		}
		return result;
	}

	void LinkRegLineToCusEntryInstruction(CusTempStorageRegLine[] lines, ICusSupportingInfoCollection<CusSupportingInfo> previousDocuments, CusSupportingInfo onlyAndEmptyDoc)
	{
		var isFirstSelectedLine = true;
		foreach (var line in lines)
		{
			var previousDocument = isFirstSelectedLine && onlyAndEmptyDoc != null ? onlyAndEmptyDoc : previousDocuments.AddNew();
			LinkToCusEntryInstruction(line, previousDocument);
			isFirstSelectedLine = false;
		}

		void LinkToCusEntryInstruction(CusTempStorageRegLine line, CusSupportingInfo previousDocument)
		{
			previousDocument.CSI_SubType = Constants.PreviousDocumentSubTypeList.REG;
			previousDocument.CSI_ReferenceNumber = ((INeedRow)line.RegHeader).Row[CusTempStorageRegHeaderSchema.SRH_Reference.Name].ToString();
			previousDocument.CSI_LineNo = line.SRL_LineNumber;
			previousDocument.CSI_Quantity = new ZDecimal(line.SRL_PackagesRemaining);
		}
	}

	const int MaximumNumberOfRecords = 999;
}
