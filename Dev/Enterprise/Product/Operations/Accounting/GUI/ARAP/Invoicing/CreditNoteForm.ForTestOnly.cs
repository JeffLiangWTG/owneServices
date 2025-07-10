#if DEBUG

using System;

namespace Enterprise.Accounting.GUI
{
	public partial class CreditNoteForm
	{
		public void HandleSaveException_ForTestOnly(Exception e)
		{
			HandleSaveException(e);
		}
	}
}

#endif
