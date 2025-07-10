using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.DocumentScanning.GUI.Res;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Interfaces between a StorageDocs record and a UserDisplay control. If you make changes in the display, it will
	/// update the StorageDocs record appropriately.  It uses the Filename passed in to instantiate the image and provides
	/// that image for editing on the user display.
	/// </summary>
	public class ImageManager : IDisposable
	{
		IPreviewableDocument imageFile;
		public IPreviewableDocument ImageFile => imageFile;

		#region Public

		public string ManagedImageFilename { get; }

		public ImageManager(string filename, StorageDocsBase document, IGraphicalDisplay userDisplay)
			: this(filename, PreviewableDocumentHelper.GetPreviewableDocument(filename), document, userDisplay) { }

		public ImageManager(string filename, IPreviewableDocument imageFile, StorageDocsBase document, IGraphicalDisplay userDisplay)
		{
			Argument.NotNullOrEmpty(filename, nameof(filename));
			Argument.NotNull(imageFile, nameof(imageFile));

			ManagedImageFilename = filename;
			this.imageFile = imageFile;

			this.UserDisplay = userDisplay;
			this.fDocument = document;

			ReloadImage();

			userDisplay.Rotation += Rotation;
			userDisplay.PageCopy += PageCopy;

			userDisplay.PagePaste += PagePaste;
			userDisplay.PageDropped += PageDropped;
			userDisplay.PageInsert += PageInsert;

			userDisplay.PageReorder += PageReorder;
			userDisplay.PageDelete += PageDelete;

			userDisplay.PageCut += PageCut;
			userDisplay.MoveToNewDocument += OnMoveToNewDocument;

			userDisplay.Closed += Closed;
		}

		public void Close()
		{
			UserDisplay.Close();
		}

		bool Disposed;

		public void Dispose()
		{
			if (!Disposed)
			{
				Close();

				UserDisplay.Rotation -= Rotation;
				UserDisplay.PageCopy -= PageCopy;

				UserDisplay.PagePaste -= PagePaste;
				UserDisplay.PageDropped -= PageDropped;
				UserDisplay.PageInsert -= PageInsert;

				UserDisplay.PageReorder -= PageReorder;
				UserDisplay.PageDelete -= PageDelete;

				UserDisplay.PageCut -= PageCut;
				UserDisplay.MoveToNewDocument -= OnMoveToNewDocument;

				UserDisplay.Closed -= Closed;

				Disposing?.Invoke(this, EventArgs.Empty);

				imageFile.Dispose();
				if (File.Exists(ManagedImageFilename))
				{
					TempFile.TryDeleteHandleAllExceptions(ManagedImageFilename);
				}

				Disposed = true;
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Dev exception message")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Debugging")]
		void ReplaceImage(IPreviewableDocument newImage)
		{
			imageFile.Dispose();

			try
			{
				newImage.Save(ManagedImageFilename);
			}
			catch (ExternalException ex)
			{
				// Got generic gdi+ exception. Reasons this can happen:
				// No write permissions
				// Corrupt image
				// Image size too big
				// MemoryStream for image has closed (must be open when saving) https://stackoverflow.com/questions/336387/image-save-throws-a-gdi-exception-because-the-memory-stream-is-closed/336396#336396

				// try extracting PreviewableImageDocument
				var subTypeInfo = string.Empty;
				subTypeInfo += $"[{newImage.GetType().Name}] NumberOfPages: {newImage.NumberOfPages}"; // This is an exception message only seen by developers.
				var additionalInfo = $"Filename: {ManagedImageFilename} AdditionalInfo_ImageManager: {subTypeInfo}";
				throw new ExternalException(additionalInfo, ex);
			}

			imageFile = newImage;

			ReloadImage();
		}

		public void ReloadImage()
		{
			UserDisplay.ShowFile(imageFile, Document, 0);
		}

		public void Activate()
		{
			UserDisplay.BringToFront();
		}

		public event EventHandler ImageDialogClose;
		public event EventHandler DocumentEmpty;
		public event MoveToNewDocumentEventHandler MoveToNewDocument;
		public event DocumentChangedEventHandler ManagedFileChanged;
		public event EventHandler Disposing;

		#endregion

		#region Implementation

		readonly IGraphicalDisplay UserDisplay;
		public object Tag;

		readonly StorageDocsBase fDocument;
		StorageDocsBase Document
		{
			get { return fDocument; }
		}

		#region Auditing String Tools

		string GetPageNumberDescription(int[] pageNumbers)
		{
			return string.Join(", ", pageNumbers);
		}

		#endregion

		#region Event Firing

		protected void OnManagedFileChanged(Event @event, string description)
		{
			ManagedFileChanged?.Invoke(this, new DocumentChangedEventArgs(@event, description));
		}

		protected void OnDocumentEmpty()
		{
			Close();
			DocumentEmpty?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Event Response

		void Rotation(object sender, RotateEventArgs e)
		{
			imageFile.RotatePage(e.PageNumber, e.Clockwise);
			ReloadImage();

			string auditMessage = e.Clockwise
				? Res.GetString("28ef6a3e-bcb5-436d-82d2-0be2a0235ce0", "Page {0} rotated clockwise", e.PageNumber + 1)
				: Res.GetString("FD1F7CB4-6F0E-49BC-B81A-8B363F08BFFE", "Page {0} rotated anti-clockwise", e.PageNumber + 1);

			OnManagedFileChanged(Events.PageRotated, auditMessage);
		}

		void PageReorder(object sender, PageReorderEventArgs e)
		{
			ReplaceImage(imageFile.ExtractPages(e.NewOrder));

			string auditMessage = DocumentUtilities.GetReorderPagesComment(e.PagesToMove, e.DestIndex);
			OnManagedFileChanged(Events.PageSReordered, auditMessage);
		}

		void PageDelete(object sender, PageDeleteEventArgs e)
		{
			var pagesToKeep = DocumentUtilities.GetInverseList(e.DeletedPages, imageFile.NumberOfPages);
			if (pagesToKeep.Length > 0)
			{
				ReplaceImage(imageFile.ExtractPages(pagesToKeep));
				ReloadImage();

				OnManagedFileChanged(Events.PageSRemoved, Res.GetString("b1f0c061-cd90-40a0-9543-f4e87ffe8baf", "Page(s)") + " " + GetPageNumberDescription(e.DeletedPages) + " " + Res.GetString("da362c07-85ba-4cb4-a468-eb0bf76215b4", "deleted"));
			}
			else
			{
				OnDocumentEmpty();
			}
		}

		void PageCopy(object sender, PageCopyEventArgs e)
		{
			PageCopy(e);
		}

		bool PageCopy(PageCopyEventArgs e)
		{
			bool result = true;
			if (e.PagesCopied.Length > 0)
			{
				var dataObject = GetSerializedCollectionForClipboard(e.PagesCopied);
				result = SafeClipboard.SetDataObject(dataObject);
			}
			return result;
		}

#if DEBUG
		internal
#endif
		void PageCut(object sender, PageCutEventArgs e)
		{
			if (PageCopy(new PageCopyEventArgs(e.PagesCut)))
			{
				PageDelete(sender, new PageDeleteEventArgs(e.PagesCut));
			}
		}

		void OnMoveToNewDocument(object sender, PageCutEventArgs e)
		{
			MoveToNewDocument?.Invoke(this, new MoveToNewDocumentEventArgs((SerializableEDocCollection)GetSerializedCollectionForClipboard(e.PagesCut).Elements));
			PageDelete(sender, new PageDeleteEventArgs(e.PagesCut));
		}

		DocManagerDataObject GetSerializedCollectionForClipboard(int[] pageIndexesToCopy)
		{
			using (var tempFile = TempFile.NewWithExtension(Document.SC_DataType))
			using (var subImage = imageFile.ExtractPages(pageIndexesToCopy))
			{
				subImage.Save(tempFile.Filename);

				var serialisableEdoc = new SerializableEDoc(Document, File.ReadAllBytes(tempFile.Filename));
				return new DocManagerDataObject(serialisableEdoc);
			}
		}

		void PageDropped(object sender, PageDroppedEventArgs e)
		{
			InsertFromSerializableEDocs(e.Data, e.PasteAfterIndex);
		}

#if DEBUG
		internal
#endif
		void PagePaste(object sender, PagePasteEventArgs e)
		{
#if !WINZOR

			IDataObject dataObject = SafeClipboard.GetDataObject();
			if (dataObject != null)
			{
				if (dataObject.GetDataPresent(DocManagerDataObject.DataFormatType))
				{
					SerializableEDocCollection docsToInsert = (SerializableEDocCollection)dataObject.GetData(DocManagerDataObject.DataFormatType);
					InsertFromSerializableEDocs(docsToInsert, e.PasteAfterIndex);
				}
				else if (dataObject.GetDataPresent(DataFormats.FileDrop))
				{
					string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
					InsertPages(files, e.PasteAfterIndex, isLocalFile: true);
				}
				else
				{
					using (ZDataObject insertableData = ZDataObject.FromData(dataObject))
					{
						if (insertableData.FileDropCount > 0)
						{
							InsertPages((string[])insertableData.GetData(DataFormats.FileDrop), e.PasteAfterIndex);
						}
					}
				}
			}

#endif
		}

		void InsertFromSerializableEDocs(SerializableEDocCollection collection, int insertAfterIndex)
		{
			string[] files = collection.GetContentsAsFiles();
			if (files.Length > 0)
			{
				InsertPages(files, insertAfterIndex);
			}
		}

		void PageInsert(object sender, PageInsertEventArgs e)
		{
			if (e.Files.Length > 0)
			{
				InsertPages(e.Files, e.InsertAfterIndex);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
		void InsertPages(string[] files, int pasteAfterIndex, bool isLocalFile = false)
		{
			var mergedDocument = imageFile.Clone();
			var unsupportedFiles = new StringBuilder();
			var shouldReplaceImage = false;
			foreach (var file in files)
			{
				if (File.Exists(file))
				{
					if (PreviewableDocumentHelper.IsAllowedToInsert(mergedDocument, file, out var docToInsert))
					{
						try
						{
							using (var oldMergedDoc = mergedDocument.Clone())
							{
								try
								{
									var newDocument = oldMergedDoc.Insert(pasteAfterIndex, docToInsert);
									mergedDocument.Dispose();
									mergedDocument = newDocument;
									shouldReplaceImage = true;
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									unsupportedFiles.AppendLine(GetUnsupportedFileNames(file, isLocalFile))
										.AppendLine(Res.GetString("3CC07AC1-943D-4EF0-8F8F-3DC10DE14E03", "The error message is {0}", ex.Message))
										.AppendLine();
								}
							}
						}
						finally
						{
							docToInsert.Dispose();
						}
					}
					else
					{
						unsupportedFiles.AppendLine(GetUnsupportedFileNames(file, isLocalFile));
					}
				}
			}

			if (unsupportedFiles.Length > 0)
			{
				Globals.Message.ShowWarning(PreviewableDocumentHelper.GetUnsupportedPasteMessage(mergedDocument) + System.Environment.NewLine + unsupportedFiles.ToString());
			}

			if (shouldReplaceImage)
			{
				var pagesAdded = mergedDocument.NumberOfPages - imageFile.NumberOfPages;
				ReplaceImage(mergedDocument);
				OnManagedFileChanged(Events.PageSAdded, Res.GetString("0d636c41-2d7e-4b7c-a78f-265adcfb3b18", "{0} page(s) pasted before page {1}", pagesAdded, pasteAfterIndex + 1));
			}
			else
			{
				mergedDocument.Dispose();
			}

			if (!isLocalFile)
			{
				SerializableEDocCollection.DisposeFiles(files);
			}
		}

		string GetUnsupportedFileNames(string file, bool isLocalFile)
		{
			return isLocalFile ? file : Path.GetFileName(file);
		}

		void Closed(object sender, EventArgs e)
		{
			ImageDialogClose?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#endregion
	}
}
