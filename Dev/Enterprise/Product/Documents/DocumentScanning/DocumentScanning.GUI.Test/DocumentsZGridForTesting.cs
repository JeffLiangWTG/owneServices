using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	public class DocumentsZGridForTesting : DocumentsZGrid
	{
		public DeliveryInstructions PartialInstructionsForTest;

		protected override DeliveryInstructions CreateInstructions(DocumentPack pack)
		{
			PartialInstructionsForTest = new DeliveryInstructions(pack);

			PartialInstructionsForTest.Recipients.RemoveAndDeleteAll();
			var contact = PartialInstructionsForTest.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = OrgConstants.AttachmentType.TIF;
			contact.Email = "test@example.com";
			contact.Name = "Mary Mary Quite COntrary";
			if (PrinterPkForTesting.IsValid)
			{
				PartialInstructionsForTest.PrinterDelivery.PrintQueuePK = PrinterPkForTesting;
				PartialInstructionsForTest.PrinterDelivery.NumberOfCopies = 5;
			}

			return PartialInstructionsForTest;
		}

		public ZGuid PrinterPkForTesting { get; set; }

		public int DoubleClickCounter;

		new public void DoubleClicked()
		{
			DoubleClickCounter++;
			base.DoubleClicked();
		}

		public Func<IDataObject, ZDataObject> GetZDataObjectFromDataForTest { get; set; }

		protected override ZDataObject GetZDataObjectFromData(IDataObject dataToInsert)
		{
			if (GetZDataObjectFromDataForTest != null)
			{
				return GetZDataObjectFromDataForTest.Invoke(dataToInsert);
			}
			else
			{
				return base.GetZDataObjectFromData(dataToInsert);
			}
		}

		public ZDataObject GetZDataObjectFromData_Exposed(IDataObject dataToInsert)
		{
			return base.GetZDataObjectFromData(dataToInsert);
		}

		protected override void CopyDocuments(BusinessObject[] documentsToCopy)
		{
			// Do not copy documents to clipboard in test
		}

		#region Exposed Methods

		new public RefDocSource OldDocSource
		{
			get => base.OldDocSource;
			set => base.OldDocSource = value;
		}

		new public void AddCutCopyPasteSelectMenuItems(ContextMenu contextMenu) => base.AddCutCopyPasteSelectMenuItems(contextMenu);
		new public void AddCopyLinkMenuItem(ContextMenu contextMenu) => base.AddCopyLinkMenuItem(contextMenu);
		new public void ContextMenu_Popup(object sender, EventArgs e) => base.ContextMenu_Popup(sender, e);
		new public void DeleteDocuments(BusinessObject[] documentsToDelete) => base.DeleteDocuments(documentsToDelete);
		new public void FireDoubleClickedIfApplicable() => base.FireDoubleClickedIfApplicable();
		new public void View(StorageDocsBase doc) => base.View(doc);
		new public void Grid_CutDocuments(object sender, EventArgs e) => base.Grid_CutDocuments(sender, e);
		new public void Grid_CopyDocumentLink(object sender, EventArgs e) => base.Grid_CopyDocumentLink(sender, e);
		new public void Grid_SplitDocument(object sender, EventArgs e) => base.Grid_SplitDocument(sender, e);
		new public void SetDeliveryMethodOnInstructions(DeliveryInstructions instructions) => base.SetDeliveryMethodOnInstructions(instructions);
		new public DocumentPack CreateDocumentPack() => base.CreateDocumentPack();
		new public void PopulateSaveFileDialogDefaults(ZSaveFileDialog dialog, StorageDocsBase bizOToSave) => base.PopulateSaveFileDialogDefaults(dialog, bizOToSave);
		new public GridRowsDataObject GetDataObject(ICollection<BusinessObject> documentsToSerialize) => base.GetDataObject(documentsToSerialize);
		new public void InsertFromData(IDataObject dataToInsert) => base.InsertFromData(dataToInsert);
		new public void SetupContextMenu() => base.SetupContextMenu();
		new public string GetDataFormatType() => base.GetDataFormatType();
		new public void DoDragDrop() => base.DoDragDrop();
		new public void OnDragDrop(DragEventArgs e) => base.OnDragDrop(e);
		new public void OnDragLeave(EventArgs e) => base.OnDragLeave(e);
		new public void OnDragOver(DragEventArgs e) => base.OnDragOver(e);
		new public void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e) => base.Grid_ColourDeciding(sender, e);

#if !WINZOR
		new public void OnDragEnter(DragEventArgs e) => base.OnDragEnter(e);
#endif

		#endregion
	}
}
