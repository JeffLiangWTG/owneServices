using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	#region DataObject Pasted Event Handler

	public delegate void DataObjectPastedEventHandler(object sender, DataObjectPastedEventArgs e);

	public class DataObjectPastedEventArgs : EventArgs
	{
		public DataObjectPastedEventArgs(IDataObject dataToPaste)
		{
			DataToPaste = dataToPaste;
		}

		public readonly IDataObject DataToPaste;

		public DataObjectPastedFileInfo[] PastedFiles
		{
			get { return PastedFileList.ToArray(); }
		}

		public void AddPastedFile(string fileName, string fileNameWithExtension, string fileCaption, Guid parentStorageDocsPk, Guid storageDocsPk, bool addedSuccessfully)
		{
			PastedFileList.Add(new DataObjectPastedFileInfo(fileName, fileNameWithExtension, fileCaption, parentStorageDocsPk, storageDocsPk, addedSuccessfully));
		}

		List<DataObjectPastedFileInfo> pastedFileList;
		List<DataObjectPastedFileInfo> PastedFileList
		{
			get { return pastedFileList ?? (pastedFileList = new List<DataObjectPastedFileInfo>()); }
		}
	}

	public class DataObjectPastedFileInfo
	{
		public DataObjectPastedFileInfo(string fileName, string fileNameWithExtension, string fileCaption, Guid parentStorageDocsPk, Guid storageDocsPk, bool addedSuccessfully)
		{
			FileName = fileName;
			FileNameWithExtension = fileNameWithExtension;
			FileCaption = fileCaption;
			AddedSuccessfully = addedSuccessfully;
			ParentStorageDocsPk = parentStorageDocsPk;
			StorageDocsPk = storageDocsPk;
		}

		public readonly string FileName;
		public readonly string FileNameWithExtension;
		public readonly string FileCaption;
		public readonly bool AddedSuccessfully;
		public readonly Guid ParentStorageDocsPk;
		public readonly Guid StorageDocsPk;
	}

	#endregion

	#region IPasteSupport

	public interface IPasteSupport
	{
		void HandleDataObjectPasted(DataObjectPastedEventArgs args);
	}

	#endregion

	public static class ZFormPaster
	{
		#region Paste

#if !WINZOR

		/// <summary>
		/// Find a suitable control that implements IPastableControl. Null is returned if one is not found.
		/// </summary>
		static IPastableControl FindActivePastableControl(Form form)
		{
			var currentControl = form.GetFrontMostActiveControl();
			while (currentControl != null && !(currentControl is IPastableControl))
			{
				currentControl = currentControl.Parent;
			}
			return currentControl as IPastableControl;
		}

		public static void PasteToActiveControl(Form form)
		{
			var pastable = FindActivePastableControl(form);
			if (pastable != null)
			{
				pastable.TryPaste();
			}
			else
			{
				var activeChildControl = form.GetFrontMostActiveControl();
				if (activeChildControl != null && !activeChildControl.GetReadOnly())
				{
					UnsafeNativeMethods.PostMessage(new HandleRef(activeChildControl, activeChildControl.Handle), WindowsMessage.WM_PASTE, IntPtr.Zero, IntPtr.Zero);
				}
			}
		}

#endif

		public static DataObjectPastedFileInfo[] PasteData(IPasteSupport pasteSupport, IDataObject dataToPaste)
		{
			var args = new DataObjectPastedEventArgs(dataToPaste);
			pasteSupport.HandleDataObjectPasted(args);
			return args.PastedFiles;
		}

		#endregion
	}
}
