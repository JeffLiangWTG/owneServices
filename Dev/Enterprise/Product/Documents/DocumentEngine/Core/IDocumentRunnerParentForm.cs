using System;

namespace Enterprise.DocumentEngine
{
	public interface IDocumentRunnerParentForm
	{
		bool HandleSaveException(Exception ex);
		void SetCursorWait();
		void SetCursorPrevious();
	}
}
