using System;

using Enterprise.Security;

namespace Enterprise.ZArchitecture.Modules
{
	/// <summary>
	/// This class will check if a client has security for the Import facility
	/// at the time the handler is triggered. Pass ImportCheckpoint from your module or null if import is not for module grid.
	/// </summary>
	public class ImportSecurityChecker
	{
		public ImportSecurityChecker(EventHandler handler, SecurityCheckpoint checkpoint)
		{
			this.handler = handler;
			this.checkpoint = checkpoint;
		}

		public virtual void OnClick(object sender, EventArgs e)
		{
			if (checkpoint == null || checkpoint.IsAllowed)
			{
				handler(this, e);
			}
			else
			{
				checkpoint.ShowError();
			}
		}

		readonly EventHandler handler;
		readonly SecurityCheckpoint checkpoint;
	}
}
