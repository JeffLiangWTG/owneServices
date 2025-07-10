using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class SplitDocumentForm : ZChildForm
	{
		public SplitDocumentForm(DocumentSplitManager businessEntity)
				: base(businessEntity)
		{
			InitializeComponent();

			SplitButton.Click += SplitButton_Click;
			ConfirmButton.Click += SaveButton_Click;
			CloseButton.Click += CloseButton_Click;
			DocumentSplitConfigGrid.CurrentCellChanged += DocumentSplitConfigGrid_CurrentCellChanged;

			try
			{
				SplitManager.DocumentSplitConfigCollection.AddNew();
				SplitManager.DocumentSplitConfigCollection.HasChanges = false;
			}
			catch
			{
				Dispose();
				throw;
			}

#if DEBUG
			Enterprise.ZArchitecture.GUI.Testing.MissingResourceStringChecker.ExcludeFromTest(this.SizeInMBEdit);
#endif
		}

		void DocumentSplitConfigGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (!IsConfigGridInitialized && DocumentSplitConfigGrid.CurrentCell.RowNumber == 0) //A null reference exception will be thrown if user click on one column in the first row for the first time, so skip the first calling.
			{
				IsConfigGridInitialized = true;
				return;
			}
			DocumentSplitConfigGrid.CallColumnStartedEditing(DocumentSplitConfigGrid.GetCellBounds(DocumentSplitConfigGrid.CurrentCell));
		}

		bool IsConfigGridInitialized;

		DocumentSplitManager SplitManager
		{
			get { return (DocumentSplitManager)BusinessEntity; }
		}

		void SplitButton_Click(object sender, EventArgs e)
		{
			SplitDocument();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			CreateSplitDocuments();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SplitDocument()
		{
			if (SplitManager.DocumentSplitConfigCollection.HasNotSplitElements)
			{
				SplitManager.ValidateAll();

				if (SplitManager.HasErrors())
				{
					Globals.Message.Show(Res.GetString("a84559f2-f0c5-4b78-9fb0-c12ff5482add", "Please fix validation errors before splitting."));
					DialogResult = DialogResult.None;
				}
				else
				{
					try
					{
						SplitManager.SplitDocument();
					}
					catch (PdfiumException ex) when (ex.ErrorType == PdfiumError.CannotImportPages)
					{
						Globals.Message.Show(Res.GetString("c5719142-840d-4014-b6bc-5ae767397ee9", "Failed to split this document, it might be a corrupted file."));
					}
				}

				Refresh();
			}
		}

		void CreateSplitDocuments()
		{
			if (SplitManager.DocumentSplitResultCollection.Count == 0)
			{
				Globals.Message.Show(Res.GetString("99B28379-9307-4149-B836-00DF91753AE7", "There's no split result. Please do split before saving."));
				DialogResult = DialogResult.None;
				Refresh();
			}
			else
			{
				if (SplitManager.DocumentSplitConfigCollection.HasNotSplitElements)
				{
					if (ShowConfirmSaveForm() == DialogResult.Yes)
					{
						SplitManager.SplitDocument();
						return;
					}
				}
				CreateSplitDocumentCore();
				Close();
			}
		}

		void CreateSplitDocumentCore()
		{
			StorageDocsBase sourceDocument = SplitManager.DocumentToSplit;
			for (int i = 0; i < SplitManager.DocumentSplitResultCollection.Count; i++)
			{
				var currentDoc = SplitManager.DocumentSplitResultCollection[i];
				var doc = (StorageDocsBase)SplitManager.CollectionToAddTo.AddNew(sourceDocument.GetType());

				doc.SC_FileName = currentDoc.DocumentName;
				doc.SC_DocType = currentDoc.DocumentType;
				doc.SC_ImageData = currentDoc.DocumentData;
				doc.SC_IsPublished = currentDoc.IsPublished;
				doc.SC_Desc = currentDoc.DescriptionType;
				doc.SC_DataType = sourceDocument.SC_DataType;
				doc.SC_GC_Company = sourceDocument.SC_GC_Company;
				doc.SC_GB_Branch = sourceDocument.SC_GB_Branch;
				doc.SC_GE_Department = sourceDocument.SC_GE_Department;

				SplitManager.LogSplit(doc);
			}
		}

		DialogResult ShowConfirmSaveForm()
		{
			return Globals.Message.Show(ResString.GetMultilingualString("ce9411a3-cee9-4405-8ccf-4d911b1b798c", "Some of your split configs are modified after last splitting, would you like to split them before saving?"),
											ResString.GetMultilingualString("aa16a564-6515-48ad-93f4-610959c6497e", "Warning"),
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Warning);
		}

		public class ZSplitConfigGrid : ZGrid
		{
			public void CallColumnStartedEditing(Rectangle bounds)
			{
				ColumnStartedEditing(bounds);
			}
		}
	}
}
