using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business
{
	public static class TranslationFeedbackLifecycleManager
	{
		public static void UpdateCurrentResourceStrings()
		{
			using (ResourceStringCacheBuilder.Instance.ExcludeTranslationFeedbackResource())
			using (ResourceStringsFactory.HoldCacheReferences())
			{
				var factory = new BusinessObjectFactory();
				var feedbacks = new TopLevelTranslationFeedbackCollection(factory);
				var filters = new ZQuery(
					StmTranslationFeedbackSchema.XT_Status,
					new[]
					{
						TranslationFeedbackStatusList.Codes.New,
						TranslationFeedbackStatusList.Codes.Approved,
						TranslationFeedbackStatusList.Codes.Current,
						TranslationFeedbackStatusList.Codes.Reverted,
						TranslationFeedbackStatusList.Codes.Overridden,
						TranslationFeedbackStatusList.Codes.Obsolete
					}).AddToFilter(new ZQuery(
						StmTranslationFeedbackSchema.XT_Application,
						new[] { TranslationFeedbackApplicationsList.Codes.Cargowise }));
				feedbacks.Load(filters);
				foreach (StmTranslationFeedback feedback in feedbacks)
				{
					foreach (StmTranslationFeedbackResource feedbackContext in feedback.SavedContexts)
					{
						var helpDataStringOfDefaultLanguage = ResourceStringsFactory.Lookup(Res.DefaultLanguage, feedbackContext.XQ_ResourceStringKey, false);
						var helpDataStringOfTargetLanguage = ResourceStringsFactory.Lookup(feedback.XT_Language, feedbackContext.XQ_ResourceStringKey, false);
						var helpDataString = helpDataStringOfTargetLanguage ?? helpDataStringOfDefaultLanguage;
						bool isTranslationDeleted = helpDataStringOfTargetLanguage == null && helpDataStringOfDefaultLanguage != null;

						if (helpDataString == null || string.IsNullOrEmpty(helpDataString.GetCaptionAtLevel(feedbackContext.XQ_ResourceStringLevel))
							|| (feedback.XT_Status == TranslationFeedbackStatusList.Codes.Approved && isTranslationDeleted &&
									feedback.XT_OriginalTranslation != helpDataStringOfDefaultLanguage.GetCaptionAtLevel(feedbackContext.XQ_ResourceStringLevel) &&
									feedback.XT_SuggestedTranslation != helpDataStringOfDefaultLanguage.GetCaptionAtLevel(feedbackContext.XQ_ResourceStringLevel)))
						{
							feedback.XT_Status = TranslationFeedbackStatusList.Codes.Obsolete;
						}
						else if (helpDataString.GetCaptionAtLevel(feedbackContext.XQ_ResourceStringLevel) == feedback.XT_SuggestedTranslation)
						{
							feedback.XT_Status = TranslationFeedbackStatusList.Codes.Current;
						}
						else if (helpDataString.GetCaptionAtLevel(feedbackContext.XQ_ResourceStringLevel) == feedback.XT_OriginalTranslation)
						{
							if (feedback.XT_Status != TranslationFeedbackStatusList.Codes.Approved && feedback.XT_Status != TranslationFeedbackStatusList.Codes.New)
							{
								feedback.XT_Status = TranslationFeedbackStatusList.Codes.Reverted;
							}
						}
						else // caption != suggested && caption != original
						{
							feedback.XT_Status = TranslationFeedbackStatusList.Codes.Overridden;
						}
					}
				}
				factory.Save();
			}
		}

		public class TranslationFeedbackEntryMessageAction : IMessageAction
		{
			public TranslationFeedbackEntryMessageAction()
			{ }

			public TranslationFeedbackEntryMessageAction(BusinessObjectFactoryProvider factoryProvider)
			{ }

			public bool ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
			{
				if (!TranslationFeedbackConfiguration.IsMasterDatabase)
				{
					notifications.AddError("Cannot accept Translation Feedback entry because this is not the master Translation Feedback database");
				}
				else
				{
					using (var xmlReader = SystemMessage.GetXmlReader(message))
					{
						var entry = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackEntry(xmlReader);
						var feedback = StmTranslationFeedback.New(new BusinessObjectFactory(Db.Connection), entry);
						feedback.Factory.Save();
					}
				}
				participant = new List<ITransactionParticipant>();
				return true;
			}

			public void SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
			{
			}
		}

		public class TranslationFeedbackUpdateMessageAction : IMessageAction
		{
			public TranslationFeedbackUpdateMessageAction()
			{ }

			public TranslationFeedbackUpdateMessageAction(BusinessObjectFactoryProvider factoryProvider)
			{ }

			public bool ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
			{
				using (var xmlReader = SystemMessage.GetXmlReader(message))
				{
					var update = TranslationFeedbackMessagesSerializer.DeserializeTranslationFeedbackUpdate(xmlReader);
					var feedbacks = new TopLevelTranslationFeedbackCollection(new BusinessObjectFactory());
					var feedback = feedbacks.Factory.Load<StmTranslationFeedback>(Guid.Parse(update.PK));
					if (feedback != null)
					{
						feedbacks.Add(feedback);
						if (!(update.Status == TranslationFeedbackStatusList.Codes.Canceled && feedback.XT_Status != TranslationFeedbackStatusList.Codes.New))
						{
							feedback.XT_Status = update.Status;
							if (!string.IsNullOrEmpty(update.ReviewComment))
							{
								feedback.XT_ReviewComment = update.ReviewComment;
							}
							if (!string.IsNullOrEmpty(update.SuggestedTranslation))
							{
								feedback.XT_SuggestedTranslation = update.SuggestedTranslation;
							}
							feedback.Factory.Save();
						}
					}
				}

				participant = new List<ITransactionParticipant>();
				return true;
			}

			public void SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
			{
			}
		}
	}
}
