using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportFromSumARegisterModuleForm : ZChildForm
	{
		public ImportFromSumARegisterModuleForm()
		{
			InitializeComponent();
		}

		public ICusSupportingInfoCollection<PreviousDocument> PreviousDocuments
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
				Globals.Message.ShowError(Res.GetString("C84EDAD1-D1EE-4932-A994-F37097C36FB9", "Please select Register lines."));
			}
			else if (selectedElementsLength + existingValidDocsCount > MaximumNumberOfRecords)
			{
				Globals.Message.ShowError(Res.GetString("D86E4143-7174-456A-8352-B87FF4099345", "Exceeding the maximum records {0}, selected lines count is {1} and existing Docs count is {2}.", MaximumNumberOfRecords, selectedElementsLength, existingValidDocsCount));
			}
			else
			{
				LinkRegLineToCusEntryInstruction(selectedElements.Cast<CusTempStorageRegLine>().ToArray(), PreviousDocuments, existsOnlyAndEmptyDoc ? firstExistingDoc : null);
				Close();
			}
		}

		ZBool ExistsOnlyAndEmptyDoc(ZInt existingDocsCount, PreviousDocument firstExistingDoc)
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

		void LinkRegLineToCusEntryInstruction(CusTempStorageRegLine[] lines, ICusSupportingInfoCollection<PreviousDocument> previousDocuments, PreviousDocument onlyAndEmptyDoc)
		{
			var isFirstSelectedLine = true;
			foreach (var line in lines)
			{
				var previousDocument = isFirstSelectedLine && onlyAndEmptyDoc != null ? onlyAndEmptyDoc : previousDocuments.AddNew();
				LinkToCusEntryInstruction(line, previousDocument);
				isFirstSelectedLine = false;
			}

			void LinkToCusEntryInstruction(CusTempStorageRegLine line, PreviousDocument previousDocument)
			{
				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				previousDocument.CSI_ReferenceNumber = ((INeedRow)line.RegHeader).Row[CusTempStorageRegHeaderSchema.SRH_Reference.Name].ToString();
				previousDocument.CSI_LineNo = line.SRL_LineNumber;
				previousDocument.CSI_Quantity = new ZDecimal(line.SRL_PackagesRemaining);
			}
		}

		const int MaximumNumberOfRecords = 999;
	}
}
