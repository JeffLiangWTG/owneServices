using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public static class WriteToLogFormInvoker
	{
		public static ZString ShowWriteToLogForm(IStmALogParent topLevelBusinessObject, IStmALogParent loggedBusinessObject, string loggedBOTypeName, SecurityCheckpoint security, ZString caption, string defaultReference, BusinessObjectLoggerOptions loggerOptions, Action<string> showHasChangesMessage, Action<bool> onLoggedAction, bool skipDialogBoxForm = false)
		{
			var result = ZString.Empty;

			if (security.IsAllowed)
			{
				if (loggedBusinessObject == null)
				{
					Globals.Message.ShowInformation(Res.GetString("d6000358-8e00-4042-a1c3-edf6bf6efd37", "The {0} record has not been created yet.", caption), loggedBOTypeName);
				}
				else
				{
					var logger = new BusinessObjectLogger(topLevelBusinessObject, (BusinessObject)loggedBusinessObject, loggerOptions);

					if (logger.TopLevelBusinessObjectHasChanges)
					{
						showHasChangesMessage?.Invoke(Res.GetString("737f828a-4371-4671-b50c-a62a90a05140", "This record needs to be saved. Please save the form first before write to log."));
					}
					else if (logger.RevisedBusinessObjectHasChanges)
					{
						showHasChangesMessage?.Invoke(Res.GetString("1072becb-5c03-45d3-8fa9-0e4bdd042b93", "The {0} record needs to be saved. Please save the form first before write to log.", caption));
					}
					else
					{
						bool loggingSucceeded;
						logger.Reference = defaultReference;

						using (var form = new WriteToLogForm(logger, caption))
						{
							if (!skipDialogBoxForm)
							{
								ZFormModaliser.ShowDialogWithoutDispose(form);
							}
							else
							{
								form.WriteToLog();
							}

							loggingSucceeded = form.LoggingSucceeded;
							result = logger.Reference;
						}

						onLoggedAction?.Invoke(loggingSucceeded);
					}
#if DEBUG
					loggerRefMaxLengthForTesting = logger.Reference_MaxLength;
#endif
				}
			}
			else
			{
				security.ShowError();
			}

			return result;
		}

#if DEBUG
		public static int LoggerRefMaxLengthForTesting
		{
			get { return loggerRefMaxLengthForTesting; }
		}

		[ThreadStatic]
		static int loggerRefMaxLengthForTesting;
#endif
	}
}
