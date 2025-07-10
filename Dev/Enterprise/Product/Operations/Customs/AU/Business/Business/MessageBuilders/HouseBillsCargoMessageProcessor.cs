using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class HouseBillsCargoMessageProcessor : ICargoMessageProcessor, Integration.Customs.AU.IHouseBillsCargoMessageProcessor
	{
		public HouseBillsCargoMessageProcessor(INotifications notify)
		{
			NotificationBuffer = new NotificationBuffer(notify);
			generatedHouseMessages = new List<EDIMessage>();
		}
		readonly List<EDIMessage> generatedHouseMessages;

		internal IEnumerable<EDIMessage> GeneratedHouseMessages => generatedHouseMessages;
		internal NotificationBuffer NotificationBuffer { get; }

		public void Process(IHouseBillsCargoMessageProcessorJob processorJob, bool saveFactory = true)
		{
			if (processorJob != null)
			{
				if (processorJob.IsAcceptable)
				{
					ProcessAcceptableJob(processorJob, saveFactory);
				}
				else
				{
					ProcessNotAcceptableJob(processorJob);
				}
			}
		}

		void ProcessAcceptableJob(IHouseBillsCargoMessageProcessorJob processorJob, bool saveFactory)
		{
			generatedHouseMessages.Clear();
			var masterBill = processorJob.MasterBill;
			if (masterBill != null)
			{
				bool messageSent = false;
				using (((IBusinessObjectInternals)masterBill).ResumeValidationForAllDescendantsTemporarily())
				{
					if (processorJob.SendChildren)
					{
						foreach (var child in processorJob.Children)
						{
							var messenger = processorJob.GetMessageManager(child);
							if (messenger.CanSendOriginal)
							{
								processorJob.SetSACIfRequired(child);
								child.LoadChildEditableObjects();
								child.RunPreSaveValidation();
								if (!child.HasMessageErrors && !child.HasErrors)
								{
									generatedHouseMessages.AddRange(messenger.GenerateOriginalMessages(child));
									NotificationBuffer.Notify(new InfoNotification(string.Format("{0} created and message prepared for sending to customs.", processorJob.GetReferenceNumber(child))));
									messageSent = true;
								}
								else
								{
									NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, processorJob.GetReferenceNumber(child) + ":" + System.Environment.NewLine +
										new ZNotificationCollector(child, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().ToUniqueMessageListString()));
								}
							}
						}
					}
					else
					{
						var messenger = processorJob.GetMessageManager(masterBill);
						if (messenger.CanSendOriginal)
						{
							masterBill.LoadChildEditableObjects();
							masterBill.RunPreSaveValidation();
							if (!masterBill.HasMessageErrors && !masterBill.HasErrors)
							{
								generatedHouseMessages.AddRange(messenger.GenerateOriginalMessages(masterBill));
								NotificationBuffer.Notify(new InfoNotification(string.Format("{0} created and message prepared for sending to customs.", processorJob.GetReferenceNumber(masterBill))));
								messageSent = true;
							}
							else
							{
								NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, processorJob.GetReferenceNumber(masterBill) + ":" + System.Environment.NewLine +
									new ZNotificationCollector(masterBill, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().ToUniqueMessageListString()));
							}
						}
					}
				}
				try
				{
					if (saveFactory)
					{
						masterBill.Factory.Save();
					}

					if (messageSent)
					{
						NotificationBuffer.Notify(new InfoNotification("Messages successfully sent."));
					}
				}
				catch (ZSaveConcurrencyException ex)
				{
					NotificationBuffer.AddWarning("Messages were not sent, error during saving.");
					NotificationBuffer.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message.Replace("\n", "")));
				}
			}
		}

		void ProcessNotAcceptableJob(IHouseBillsCargoMessageProcessorJob processorJob)
		{
			var reason = processorJob.ReasonWhyNotAcceptable;
			if (!string.IsNullOrEmpty(reason))
			{
				NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, reason));
			}
		}

		void ICargoMessageProcessor.Process(ICargoMessageProcessorJob processorJob, bool saveFactory)
		{
			Process(processorJob as IHouseBillsCargoMessageProcessorJob, saveFactory);
		}
	}
}
