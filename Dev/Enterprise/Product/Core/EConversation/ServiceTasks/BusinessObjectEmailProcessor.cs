using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using HtmlAgilityPack;

namespace Enterprise.EConversation.ServiceTasks
{
	public abstract class BusinessObjectEmailProcessor<T> : IBusinessObjectEmailProcessor where T : BusinessObject, IAllowAttachEmailsToEDocs
	{
		#region Create / Process

		public bool CreateAndProcessMailItem(Email email, IEmailProcessorLogger logger)
		{
			if (email == null)
			{
				throw new ArgumentNullException(nameof(email));
			}

			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			if (IgnoreEmail(email))
			{
				return false;
			}

			var mimeString = email.GetEml();
			logger.Log(LogType.Information, true, "MIME Content:\r\n{0}", mimeString);

			try
			{
				var factory = new BusinessObjectFactory();
				var mail = CreateMailItem(factory, mimeString);
				mail.MI_Subject = ((ZString)email.Subject).Left(MailItem.Schema.MI_SubjectMaxLength);
				logger.Log(LogType.Information, true, "MailItem created. PK:{0} Subject:{1}", mail.PK, mail.MI_Subject);
				return ProcessMailItem(mail, logger);
			}
			catch (InvalidMailFormatException ex)
			{
				email.IsEmailProcessingSkipped = true;
				logger.Log(LogType.Warning, verboseModeOnly: false, $"The format of email is invalid. Subject:{email.Subject}; From:{email.From}; Exception:{ex}");
				return false;
			}
		}

		protected virtual bool IgnoreEmail(Email email)
		{
			return false;
		}

		MailItem CreateMailItem(BusinessObjectFactory factory, string emailString)
		{
			MailItem mail = factory.New<MailItem>();
			mail.RawMIMEString = emailString;
			mail.MI_Application = MailApplicationCode;
			mail.MI_Status = MailStatus.Unprocessed;
			mail.MI_Direction = MailDirection.Receive;
			return mail;
		}

		protected virtual bool ProcessMailItem(MailItem mailItem, IEmailProcessorLogger logger)
		{
			var bizO = LoadBusinessObject(mailItem, logger);

			var result = bizO != null ? AttachEmailCore(mailItem, bizO, logger) : ShouldProcessFailures;

			if (result)
			{
				SaveMailFactory(mailItem);
			}

			return result;
		}

		protected virtual bool ShouldProcessFailures => true;

		protected virtual void SaveMailFactory(MailItem mailItem)
		{
			mailItem.Factory.Save();
		}

		protected abstract T LoadBusinessObject(MailItem mailItem, IEmailProcessorLogger logger);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description Html Node")]
		protected T LoadFromUniqueEmailIdentifier(MailItem mailItem, IEmailProcessorLogger logger)
		{
			var message = mailItem.BuildMimeMessage();

			if (string.IsNullOrEmpty(message.HtmlBody))
			{
				return null;
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(message.HtmlBody);
			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<T>(mailItem.Factory, message.HtmlBody, out var result);

			if (!success.HasValue)
			{
				logger.Log(LogType.Information, true, "EConversation ID element not found in {0}:\r\n{1}", mailItem.MI_Subject, message.HtmlBody);

				return null;
			}

			if (!success.Value)
			{
				logger.Log(LogType.Information, true, "Business Object could not be loaded from {0}:\r\n{1}", mailItem.MI_Subject, message.HtmlBody);

				return null;
			}

			return result;
		}

		protected bool AttachEmailCore(MailItem mailItem, T workTask, IEmailProcessorLogger logger)
		{
			bool result = ShouldAttachEmailAndSave(mailItem, workTask);
			if (result)
			{
				AttachEmailToAnExistingBusinessObject(mailItem, workTask, logger);
			}
			return result;
		}

		protected virtual void AttachEmailToAnExistingBusinessObject(MailItem mailItem, T bizO, IEmailProcessorLogger logger)
		{
			BusinessObjectEmailAttacher.AttachEmail(bizO, DocType, mailItem);
			logger.Log(LogType.Information, false, "Email attached to: {0}", bizO.ReferenceNumber);
		}

		protected abstract string DocType { get; }

		#endregion

		#region Abstracts

		public abstract string EmailTypeName { get; }
		public abstract string MailApplicationCode { get; }

		protected abstract bool ShouldAttachEmailAndSave(MailItem mailItem, T bizO);

		#endregion
	}

	public interface IBusinessObjectEmailProcessor
	{
		bool CreateAndProcessMailItem(Email email, IEmailProcessorLogger logger);
	}
}


