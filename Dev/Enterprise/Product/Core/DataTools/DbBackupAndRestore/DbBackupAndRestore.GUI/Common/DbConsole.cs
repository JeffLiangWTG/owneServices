using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	public abstract class DbConsole
	{
		public virtual void Start()
		{
			try
			{
				OnDatabaseTaskStart();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ExitCode = -1;
				Logger.Instance.LogErrorMessage(ex.ToString());
			}
		}

		protected virtual void OnDatabaseTaskStart() { }

		public int ExitCode
		{
			get;
			protected set;
		}

		#region Event Handlers

		protected void DbTools_OnTaskStarted(string message)
		{
			PrintOutput(message);
		}

		protected void DbTools_OnSubtaskStarted(string message, int stepNumber)
		{
			PrintOutput("\t" + message.Replace("\r\n", "\r\n\t"));
		}

		protected void DbTools_OnTaskCompleted(string message)
		{
			PrintOutput(message);
			PrintOutput("============================ SUCCESSFUL ===============================");
		}

		protected void DbTools_OnTaskFailed(string message)
		{
			ExitCode = -1;
			Console.Error.WriteLine(message);
			PrintOutput("============================= FAILED ===============================");
		}

		protected void DbTools_OnShowInfoMessage(string message)
		{
			PrintOutput("\t" + message.Replace("\r\n", "\r\n\t"));
		}

		public virtual void DbTools_OnConfirmationPrompt(ConfirmationPromptArgs promptArgs)
		{
			PrintOutput(promptArgs.PromptTitle);
			PrintOutput(promptArgs.PromptMessage);
			PrintOutput("Y/N?");
			PrintOutput($"User confirmation prompt skipped in CLI mode with default response. value='{promptArgs.Result.ToString()}'");
		}

		[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Using this class without GUI, so print messages in console")]
		protected void PrintOutput(string outputString)
		{
			Console.WriteLine(outputString);
		}

		#endregion Event Handlers
	}
}
