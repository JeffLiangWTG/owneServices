using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class DocumentRunnerParentForm : IDocumentRunnerParentForm
	{
		public DocumentRunnerParentForm(Form form)
		{
			Argument.NotNull(form, "form");
			this.form = form;
		}

		public bool HandleSaveException(Exception ex)
		{
			if (form is ZForm zForm)
			{
				try
				{
					zForm.TryHandleSaveException(ex);
					return true;
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
			return false;
		}

		public void SetCursorPrevious()
		{
			form.Cursor = previousCursor;
		}

		public void SetCursorWait()
		{
			previousCursor = form.Cursor;
			form.Cursor = Cursors.WaitCursor;
		}

		Cursor previousCursor;
		readonly Form form;
	}
}
