using System;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	partial class WriteToLogForm : ZChildForm
	{
		public WriteToLogForm(BusinessObjectLogger logger, string caption)
			: base(logger)
		{
			this.logger = logger;
			this.caption = caption;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override string FormCaption
		{
			get { return string.IsNullOrEmpty(base.FormCaption) ? caption : base.FormCaption + " " + caption; }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void WriteToLogButton_Click(object sender, EventArgs e)
		{
			logger.RunPreSaveValidation();

			if (!logger.HasErrors)
			{
				WriteToLog();
			}
		}

		internal void WriteToLog()
		{
			try
			{
				loggingSucceeded = logger.WriteToLog();

				if (!loggingSucceeded)
				{
					Globals.Message.ShowWarning(Res.GetString("c1e1b4be-11f9-4606-9baf-6abbdbde3c96", "This record has been modified since the form was last loaded. Please close the form and re-open it to view the latest version of this record."));
				}
			}
			catch (InvalidOperationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			Close();
		}

		public bool LoggingSucceeded
		{
			get { return loggingSucceeded; }
		}

		bool loggingSucceeded;
		readonly string caption;
		readonly BusinessObjectLogger logger;
	}
}
