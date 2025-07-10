using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class SendNativePortMessageOriginal : CustomCommand
	{
		public override string Id => CommandIds.SendMessage;
		public override string Caption => CommandResources.Captions.SendMessage;

		public override bool IsEnabled => true;

		public override bool IsVisible => true;

		public override bool Invoke()
		{
			var sent = false;
			object documentData = documentInfo.DocumentData?.Parent;
			if (documentData is IFRMessagesOwner frMessagesOwner)
			{
				switch (documentInfo.Template?.DataContext)
				{
					case DataContext.FRPortsRegularizationTransitDOA:
						if (documentInfo.Document is IDocument document)
						{
							if (document.Data?.Value is DOADataObject doaDataObject)
							{
								var objectToSend = new DOAMessageSendingObject(documentData, doaDataObject);
								var userNotification = documentInfo.Services.Resolve<IUserNotificationService>();

								if (IsValid(document, userNotification))
								{
									var errorCollector = new ErrorCollector();
									var sender = new DOAMessageSender(objectToSend, errorCollector);
									sender.Send();
									sent = errorCollector.ErrorCount == 0;

									if (sent)
									{
										userNotification.ShowMessage(Res.GetString("de033063-ff49-491a-96ea-aa1d7b6e7f41", "Sent successfully."), Res.GetString("75d55443-0e8d-4183-b977-224a03a2aa96", "Successful"));
									}
									else
									{
										userNotification.ShowMessage(FormattableString.Invariant($"Sent failed.\n{errorCollector.GetErrorsAsString()}"), Res.GetString("e4def527-9f71-4080-891c-4c6b614fcaeb", "Failed"));
									}
								}
							}
						}
						break;

					case DataContext.FRPortsCustomsCheckCAED:
						if (documentInfo.Document is IDocument documentCaed)
						{
							if (documentCaed.Data?.Value is CAEDDataObject caedDataObject)
							{
								var objectToSend = new CAEDMessageSendingObject(documentData, caedDataObject);
								var userNotification = documentInfo.Services.Resolve<IUserNotificationService>();

								if (IsValid(documentCaed, userNotification))
								{
									var errorCollector = new ErrorCollector();
									var sender = new CAEDMessageSender(objectToSend, errorCollector);
									sender.Send();
									sent = errorCollector.ErrorCount == 0;

									if (sent)
									{
										userNotification.ShowMessage(Res.GetString("de033063-ff49-491a-96ea-aa1d7b6e7f41", "Sent successfully."), Res.GetString("75d55443-0e8d-4183-b977-224a03a2aa96", "Successful"));
									}
									else
									{
										userNotification.ShowMessage(FormattableString.Invariant($"Sent failed.\n{errorCollector.GetErrorsAsString()}"), Res.GetString("e4def527-9f71-4080-891c-4c6b614fcaeb", "Failed"));
									}
								}
							}
						}
						break;
				}
			}

			return sent;
		}

		bool IsValid(IDocument document, IUserNotificationService notificationService)
		{
			var data = document.Data;

			var caption = Res.GetString("107C6ED2-A0A7-4431-83AF-224007471E9A", "Sending Message");

			if (data == null)
			{
				notificationService.ShowMessage(Res.GetString("EC4D455E-8CC7-412B-BE66-F8CFBC06F1F2", "Document data could not be found."), caption);
				return false;
			}

			if (data.HasChanges)
			{
				notificationService.ShowMessage(Res.GetString("4E5B0B90-A61B-493D-A840-649F7233A073", "Please save all changes before sending."), caption);
				return false;
			}

			if (document.HasErrors())
			{
				notificationService.ShowMessage(Res.GetString("31A5C46A-94B3-445F-AA17-4B2B0478F388", "This document contains errors. Please fix all errors before sending."), caption);
				return false;
			}

			if (document.HasMessageErrors())
			{
				notificationService.ShowMessage(Res.GetString("1E0E1669-F632-4682-B284-63B3FA4C0222", "This document contains message errors. Please fix all message errors before sending."), caption);
				return false;
			}

			return true;
		}
	}
}
