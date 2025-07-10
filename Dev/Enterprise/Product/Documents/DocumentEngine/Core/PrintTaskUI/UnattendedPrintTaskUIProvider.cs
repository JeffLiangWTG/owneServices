using System;
using System.IO;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	class UnattendedPrintTaskUIProvider : IPrintTaskUIProvider
	{
		bool IPrintTaskUIProvider.ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			return false;
		}

		bool IPrintTaskUIProvider.ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings)
		{
			return true;
		}

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(PrintTaskSettings taskSettings)
		{
			return new SilentProgressNotificationUI();
		}

		IProgressNotificationUI IPrintTaskUIProvider.GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks)
		{
			return new SilentProgressNotificationUI();
		}

		bool IPrintTaskUIProvider.ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			return true;
		}

		void IPrintTaskUIProvider.ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions)
		{
			// Do nothing as there is no user to select the printer.
		}

		void IPrintTaskUIProvider.ShowPreview(Stream xlsStream, DeliveryInfo[] deliveryInfos, IDeliverCapableForm parentForm)
		{
			// Do nothing. Preview not required if there is no user.
		}

		bool IPrintTaskUIProvider.ShowErrors(Report report)
		{
			return !report.ErrorManager.HasErrors || report.ErrorManager.HasWarningsOnly;
		}

		#region IPrintTaskUIProvider Members

		public void ShowWarning(string caption, string message)
		{
			EmailDef email = new EmailDef();
			email.Subject = caption;
			email.Body = message;

			IUser currentUser = Env.CurrentUser;
			if (currentUser != null && currentUser.IsActive && !string.IsNullOrEmpty(currentUser.EmailAddress))
			{
				email.AddRecipientForSystemCommunication(currentUser.EmailAddress, RecipientDef.RecipientTypes.TO);
				Env.OutgoingMailManager.CreateAndSave(email);
			}
			else
			{
				Env.OutgoingMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
			}
		}

		#endregion
	}

	class SilentProgressNotificationUI : IProgressNotificationUI
	{
		void IDisposable.Dispose()
		{
		}
	}
}
