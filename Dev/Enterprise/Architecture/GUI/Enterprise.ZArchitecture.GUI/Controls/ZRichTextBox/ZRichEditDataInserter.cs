using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Interop;
using CargoWise.Common.Testing;
using CargoWise.Interop.DataObjects;
using Enterprise.ZArchitecture.Environment;
#if WINZOR
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	internal class ZRichEditDataInserter : IDisposable
	{
		public ZRichEditDataInserter(ZRichTextBox zRichTextBox)
		{
			this.ZRichTextBox = zRichTextBox;
			this.RichEdit = zRichTextBox.RichEdit;
#if !WINZOR
			this.RichEditOleLocator = ZComRichEditOleInterfaceLocator.GetInstance(RichEdit);
#endif

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public readonly RichTextBox RichEdit;

		#region InsertObject

		/// <summary>
		/// Insert text, an image or an attachment at the current cursor location.
		/// </summary>
		public void InsertObject(object data)
		{
			if (!InInsertObject)
			{
				InInsertObject = true;
				try
				{
					using (var dataObject = ZDataObject.FromData(data))
					{
						InsertObjectCore(dataObject);
					}
				}
				finally
				{
					InInsertObject = false;
				}
			}
		}
		bool InInsertObject;

		void InsertObjectCore(ZDataObject dataObject)
		{
			RichEdit.Focus();
			if (dataObject.FileDropCount > 1)
			{
				var files = (string[])dataObject.GetData(DataFormats.FileDrop);

				if (ZRichTextBox.EDocsPlugIn != null)
				{
					InsertFilesWithEDocAttachmentMonitor(files);
				}
				else
				{
					InsertFiles(files);
				}
			}
			else
			{
				InsertOneSupportedObject(dataObject); // plain or formatted text
			}
		}

		void InsertFiles(string[] files)
		{
			foreach (var file in files)
			{
				using (var nextFileDrop = ZDataObject.FromData(DataFormats.FileDrop, new string[] { file }))
				{
					InsertOneSupportedObject(nextFileDrop);
				}
			}
		}

		void InsertFilesWithEDocAttachmentMonitor(string[] files)
		{
			if (ZRichTextBox.EDocsPlugIn != null && !ZRichTextBox.EDocsPlugIn.IsMonitoringDocumentAttachment)
			{
				using (EDocsPluginAttachMonitor.StartMonitorEDocAttachment(ZRichTextBox.EDocsPlugIn))
				{
					InsertFiles(files);
				}
			}
		}

		void InsertOneSupportedObject(ZDataObject data)
		{
			if (!RichEdit.ReadOnly && RichEdit.Enabled)
			{
				try
				{
					if (data.GetDataPresent(DataFormats.FileDrop))
					{
						if (ZRichTextBox.IsEDocsAvailable)
						{
							var pastedFiles = ZRichTextBox.WrapDataAndSendToParentForm(data);
							InsertFilesSavedToEDocsMessage(pastedFiles);
						}
						else
						{
							ShowBalloonWithEDocsRequiredMessage();
						}
					}
					else if (data.GetDataPresent(DataFormats.Bitmap) || data.GetDataPresent(DataFormats.Dib))
					{
						Globals.Message.ShowWarning(Res.GetString("4284A600-BB4C-406E-BBCF-7331D2F0D83A", "The file image size has exceeded the limit. Please attach file directly to eDocs tab."));
					}
					else
					{
						var embeddedRtfImageSource = data as IEmbeddedRtfImageSource;
						if (embeddedRtfImageSource != null && embeddedRtfImageSource.Valid)
						{
							if (embeddedRtfImageSource.ImageFiles.Length > 0)
							{
								ZRichTextBox.WrapDataAndSendToParentForm(data);
							}
							embeddedRtfImageSource.FinaliseRtf();
						}

#if !WINZOR
						var pDataObject = data.GetInnerDNetIOleDataObjectImpl();
						try
						{
							ImportFormattedText(pDataObject);
						}
						finally
						{
							Marshal.Release(pDataObject);
						}
#else
						if (data.GetDataPresent(DataFormats.Html))
						{
							RichEdit.SelectedHtml = (string)data.GetData(DataFormats.Html);
						}
						else
						{
							var rtf = (data.GetData(DataFormats.Rtf) ?? data.GetData(typeof(string)));
							if (rtf is not null && !RichEdit.ReadOnly && RichEdit.Enabled)
							{
								try
								{
									RichEdit.SelectedHtml = ORtfTextUtil.RtfToHtml((string)rtf);
								}
								catch (NotSupportedException)
								{
									// We ignore NotSupportedException because the conversion triggered unsupported functionalities,
									// WTG.RtfConverter not yet supports save images.
								}
							}
						}
#endif
					}
				}
				catch (ExternalException)
				{
					if (Globals.IsTest)
					{
						throw;
					}
					// if u get this exception there is nothing that can be done..
				}
				ZRichTextBoxDiagnosticsCollector.Instance.NotifyObjectInserted(data);
			}
		}

		internal void InsertFilesSavedToEDocsMessage(DataObjectPastedFileInfo[] pastedFiles)
		{
			foreach (var pastedFile in pastedFiles)
			{
				if (pastedFile.AddedSuccessfully && File.Exists(pastedFile.FileName) && ZRichTextBox.IsEDocsAvailable)
				{
					var fileDesc = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", pastedFile.FileNameWithExtension, pastedFile.FileCaption);
					var message = "\r\n" + Res.GetString("e274ab29-3674-4877-be61-59ec7df2ae26", "{0} File {1} added to eDocs tab.", EnvProxy.Instance.CurrentUser.InitialsAndDateTime, fileDesc) + "\r\n";

					if (pastedFile.ParentStorageDocsPk != Guid.Empty && pastedFile.StorageDocsPk != Guid.Empty)
					{
						var docHyperlink = ShowStorageDocUrlHandler.Instance.Create(pastedFile.ParentStorageDocsPk, pastedFile.StorageDocsPk);
						var hyperlinkMessage = ZMenuStrategyHelper.ShortcutCreator.FormatHyperlink(message, docHyperlink);
						InsertOneSupportedObjectWithMessage(hyperlinkMessage);
					}
					else
					{
						InsertOneSupportedObjectWithMessage(message);
					}
				}
			}
		}

		void InsertOneSupportedObjectWithMessage(object dataMessage)
		{
			using (var data = ZDataObject.FromData(dataMessage))
			{
				InsertOneSupportedObject(data);
			}
		}

		void ShowBalloonWithEDocsRequiredMessage()
		{
			Globals.Message.ShowWarning(
				Res.GetString("502cfe8f-83cf-4cb2-ac99-5d8000685451", @"You cannot add files to this form."),
				Res.GetString("e256aba9-3371-41ba-a696-1a06eaa064b3", "Cannot Add Files"));
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
#if !WINZOR
			RichEditOleLocator.Dispose();
#endif
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		#region Implementation

#if !WINZOR
		readonly ZComRichEditOleInterfaceLocator RichEditOleLocator;
#endif
		readonly ZRichTextBox ZRichTextBox;

		#region ImportFormattedText

#if !WINZOR
		void ImportFormattedText(IntPtr pDataObject)
		{
			try
			{
				ImportFormattedTextCore(pDataObject);
			}
			catch (ExternalException ex)
			{
				if (ex.ErrorCode == HResult.RPC_E_SERVERCALL_RETRYLATER)
				{
					Thread.Sleep(2000);
					try
					{
						ImportFormattedTextCore(pDataObject);
					}
					catch (Exception ex1) when (!ex1.IsCriticalException())
					{
						// this is not critical
					}
				}
			}
		}

		protected virtual void ImportFormattedTextCore(IntPtr pDataObject)
		{
			if (!RichEdit.ReadOnly && RichEdit.Enabled)
			{
				try
				{
					RichEditOleLocator.RichEditOle.ImportDataObject(pDataObject, 0, IntPtr.Zero);
				}
				catch (OutOfMemoryException)
				{
					// this happens intermittently for some text formats and can't be recovered from
				}
				catch (EndOfStreamException)
				{
					// this happens intermittently for some text formats and can't be recovered from
				}
			}
		}
#endif

		#endregion

		#endregion
	}
}
