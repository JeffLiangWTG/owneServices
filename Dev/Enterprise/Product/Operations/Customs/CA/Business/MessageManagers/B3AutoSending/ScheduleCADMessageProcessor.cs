using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class ScheduleCADMessageProcessor : IProcessor
	{
		public ScheduleCADMessageProcessor(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			using (DisposableEnvironment.ForBranch(declaration.JE_GB.ToGuid()))
			{
				if (declaration.ShowSubmitMenuItem)
				{
					notifications.AddWarning(Res.GetString("5e9dce31-4e44-445a-95a5-f4ddb6374709", "System cannot send the CAD for Job: {0}, because this job is configured to submit through a designated service provider interface.", declaration.JE_DeclarationReference));
				}
				else
				{
					var scheduledDate = declaration.CalculatedB3SendingDate;
					if (scheduledDate.IsValid)
					{
						declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
						var entryHeader = declaration.B3EntryHeader;
						if (entryHeader.IsCAD)
						{
							var wrapper = new CADMessageWrapper(entryHeader);
							var delayInstruction = new B3DeferInstruction(declaration, scheduledDate.Date.AddHours(4));
							if (!SendMessage(new CADMessageManager(wrapper, new UserNotificationWrapper(notifications), delayInstruction, true), notifications, wrapper.messageSubType))
							{
								notifications.AddWarning(Res.GetString("152B5C6A-872D-4B72-8752-1B4F727BEB5B", "Sending CAD message for Declaration {0} failed", declaration.JE_DeclarationReference));
							}
							else
							{
								notifications.AddWarning(Res.GetString("FF0CDED8-BC8E-4AF9-9311-924CBBEBD3BA", "CAD message scheduled successfully at {0} for Declaration {1}", scheduledDate, declaration.JE_DeclarationReference));
							}
						}
					}
					else
					{
						notifications.AddWarning(Res.GetString("346c7064-d1fa-421e-b8d9-024385651bf3", "Sending CAD message for Declaration {0} is not allowed", declaration.JE_DeclarationReference));
					}
				}
			}
		}

		bool SendMessage(B3CADBaseMessageManager manager, INotifications notifications, MessageSubTypes messageSubType)
		{
			var result = false;
			try
			{
				result = manager.SendMessage(messageSubType, false);
			}
			catch (ZSaveException e)
			{
				notifications.AddError(e.Message);
			}
			return result;
		}

		#endregion
	}
}
