using System;

namespace Enterprise.DataTransfer.Business
{
	public delegate void EmailExportInstructionsEventHandler(object sender, EmailExportInstructionsEventArgs e);

	public class EmailExportInstructionsEventArgs : EventArgs
	{
		public EmailExportInstructionsEventArgs(EmailExportInstructions instructions)
		{
			this.Instructions = instructions;
		}

		public readonly EmailExportInstructions Instructions;
	}
}
