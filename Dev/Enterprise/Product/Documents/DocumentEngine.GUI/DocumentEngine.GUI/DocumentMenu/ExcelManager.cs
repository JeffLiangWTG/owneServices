using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Win32;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class ExcelManager : IExcelManager
	{
		internal ExcelManager(Form parentForm)
		{
			this.parentForm = parentForm;
		}
		readonly Form parentForm;

		public void Edit(string workingFile, int row, int column)
		{
			try
			{
				OpenExcelDocumentForEdit(workingFile, row, column);

				editingForm = new EditingExcelForm();
				editingForm.CheckEditFinished += new EditingExcelForm.CheckEditFinishedDelegate(editingForm_CheckEditFinished);
				editingForm.Closed += new EventHandler(EditingForm_Closed);
				ZFormModaliser.Show(editingForm, parentForm);
			}
			catch (DocumentEngineComException ex)
			{
				var comException = ex.InnerException as COMException;
				if (comException != null)
				{
					Globals.Message.ShowError(Res.GetString("789dc4bd-16ae-42dd-93f1-caba7d2eaac3", "There was a problem opening this document. Details:\r\n{0}", comException.Message));
				}
				else
				{
					throw;
				}
			}
		}

		Excel.WorkbookEvents_BeforeCloseEventHandler workbookBeforeCloseHandler;
		Excel.Workbook templateBook;
		EditingExcelForm editingForm;
		bool editFinished;

		internal Excel.Application ExcelApplication;
		internal readonly CultureInfo ForceEnglishUSLocale = new CultureInfo("en-US");

		void OpenExcelDocumentForEdit(string filePath, int row, int column)
		{
			try
			{
				OpenTemplateBook(filePath);

				workbookBeforeCloseHandler = new Excel.WorkbookEvents_BeforeCloseEventHandler(TemplateBook_BeforeClose);
				templateBook.BeforeClose += workbookBeforeCloseHandler;

				Excel._Worksheet workSheet1 = (Excel._Worksheet)templateBook.Worksheets[1];
				workSheet1.GetType().InvokeMember("Activate", System.Reflection.BindingFlags.InvokeMethod, null, workSheet1, Array.Empty<object>(), ForceEnglishUSLocale);

				ExcelApplication.Visible = true;
				ExcelApplication.UserControl = true;

				if (row >= 0 || column >= 0)
				{
					if (row < 0)
					{
						row = 0;
					}

					if (column < 0)
					{
						column = 0;
					}

					var cellReference = CellReference.GetCellRef(row, column);
					workSheet1.get_Range(cellReference, Type.Missing).Select();
				}
			}
			catch (COMException comEx)
			{
				if (comEx.Message.StartsWith((NoResString)"Creating an instance of the COM component with CLSID {00024500-0000-0000-C000-000000000046} from the IClassFactory failed") ||
					comEx.IsHResult(TYPE_E_LIBNOTREGISTERED))
				{
					Globals.Message.ShowError(Res.GetString("dfd92a51-510e-46f5-b66c-0a10236ea847", "There was a problem running Excel. Please check your Excel installation."));
				}
				else
				{
					throw new DocumentEngineComException("COM Exception", comEx);
				}
			}
			catch (InvalidCastException castEx) when (castEx.Message.StartsWith((NoResString)"Unable to cast COM object of type 'Excel.ApplicationClass' to interface type 'Excel._Application'") || castEx.IsHResult(TYPE_E_CANTLOADLIBRARY))
			{
				Globals.Message.ShowError(Res.GetString("4bdd60e2-2ff6-41e9-9ed3-ad92b1fe74d0", "A problem occurred while running Excel. The Windows registry version value of Excel is different to its installed version.\r\nIt is recommended that you completely uninstall Excel and then reinstall it."));
			}
			catch (Exception ex)
			{
				var comException = ex.InnerException as COMException;
				if (comException != null)
				{
					throw new DocumentEngineComException("COM Exception", comException);
				}
				else
				{
					throw;
				}
			}
		}

		protected virtual void OpenTemplateBook(string filePath)
		{
			ExcelApplication = new Excel.ApplicationClass();
			templateBook = ExcelApplication.Workbooks.GetType().InvokeMember("Open", System.Reflection.BindingFlags.InvokeMethod, null, ExcelApplication.Workbooks, new object[] { filePath, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value }, ForceEnglishUSLocale) as Excel.Workbook;
		}

		const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);
		const int TYPE_E_CANTLOADLIBRARY = unchecked((int)0x80029C4A);

		public bool IsSupported
		{
			get
			{
				RegistryKey classIDKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("CLSID");
				RegistryKey excelIDKey = classIDKey.OpenSubKey("{00024500-0000-0000-C000-000000000046}");
				return excelIDKey != null;
			}
		}

		void TemplateBook_BeforeClose(ref bool cancel)
		{
			templateBook.BeforeClose -= workbookBeforeCloseHandler;
			templateBook = null;
			workbookBeforeCloseHandler = null;
			GC.Collect(); // Force the release of Excel COM.
			editFinished = true;
		}

		void EditingForm_Closed(object sender, EventArgs e)
		{
			editingForm.Closed -= new EventHandler(EditingForm_Closed);
			CallExcelClosed(editingForm.DialogResult);
		}

		bool editingForm_CheckEditFinished()
		{
			return editFinished;
		}

		public event ExcelClosedEventHandler ExcelClosed;

		void CallExcelClosed(DialogResult dialogResult)
		{
			if (ExcelClosed != null)
			{
				ExcelClosed(dialogResult);
			}
		}

		[Serializable]
		class DocumentEngineComException : DocumentEngineException
		{
			internal DocumentEngineComException(string message, Exception innerException)
				: base(message, innerException)
			{
			}

#if NETFRAMEWORK
			protected DocumentEngineComException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}
}
