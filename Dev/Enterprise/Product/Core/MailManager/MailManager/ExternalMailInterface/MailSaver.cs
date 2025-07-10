using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using Res = MailManager.Res;

#if DEBUG
using CargoWise.Data.Testing;
#endif

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailSaver : MailSaverBase
	{
		public MailSaver(IMailDownloader downloader, ILogger logger = null)
			: base(downloader)
		{
			PersistingEmail += saver_PersistingEmail;
			EndOfDownload += saver_EndOfDownload;
			this.logger = logger;
		}

		void saver_EndOfDownload()
		{
			try
			{
				Factory.Save();
#if DEBUG
				ThrowDeadlockExceptionForTestIfNeeded();
#endif
			}
			catch (ZSaveException ex)
			{
				if (ex.InnerException.InnerException is SqlException sqlEx)
				{
					var match = new DbErrorMatch(sqlEx);
					if (match.ExceptionType == DbErrorType.DeadlockError)
					{
						Thread.Sleep(2000);
						Factory.Save();
						return;
					}
				}
				throw;
			}
			finally
			{
				FactorySaveCount++;
				factory = null;
			}
		}

		protected static string GenerateUniqueIdForEmail(string toEncode, HashAlgorithm hashAlgorithm)
		{
			if (string.IsNullOrEmpty(toEncode))
			{
				throw new ArgumentNullException(nameof(toEncode));
			}

			if (hashAlgorithm is null)
			{
				throw new ArgumentNullException(nameof(hashAlgorithm));
			}

			var byteStream = System.Text.Encoding.UTF8.GetBytes(toEncode);
			var hash = hashAlgorithm.ComputeHash(byteStream);
			return System.Convert.ToBase64String(hash);
		}

		// SHA1 is used purely because after base64 encoding it still fits in the MI_POP3UIDL column data type (255 chars).
		protected static readonly Overridable<HashAlgorithm> DefaultHashAlgorithm = new Overridable<HashAlgorithm>(SHA1.Create());

		protected static string GenerateUniqueIdForEmail(string toEncode)
		{
			return GenerateUniqueIdForEmail(toEncode, DefaultHashAlgorithm.Value);
		}

		void SetupMail(MailItem item, string uniqueId, string email)
		{
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			item.MI_Direction = MailDirection.Receive;
			item.MI_Application = MailApplication.Standard;
			item.MI_Status = MailStatus.Queued;
			item.RawMIMEString = email;

			if (string.IsNullOrEmpty(uniqueId))
			{
				uniqueId = GenerateUniqueIdForEmail(email);
				logger?.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, (NoResString)"Email has null or empty unique ID; generated a new one.\r\nUniqueId:{0}", uniqueId));
				logger?.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "RawEmail:{0}", email));
			}

			if (uniqueId.Length > MailDBItemsSchema.MI_POP3UIDL.MaxLength)
			{
				uniqueId = GenerateUniqueIdForEmail(email);
				logger?.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, (NoResString)"Unique ID from mail server is too long; generated a new one.\r\nUniqueId:{0}", uniqueId));
				logger?.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "RawEmail:{0}", email));
			}

			uniqueId = new ZString(uniqueId).StripNonPrintableASCIICharacters().Trim().Left(item.MI_POP3UIDLInfo.MaxLength);
			item.MI_POP3UIDL = uniqueId;
			uniqueId = item.MI_POP3UIDL;

			item.SetHeaderItem("X-UIDL", uniqueId);

			var query = new ZQuery(MailDBItemsSchema.MI_Subject, item.MI_Subject);
			if (Env.Registry.MailRetrievalProtocol != MailRetrievalProtocols.POP3)
			{
				query.AddToFilter(MailDBItemsSchema.MI_Header, item.MI_Header);
			}
			query.AddToFilter(MailDBItemsSchema.PK, SQLComparisonOperator.NotEqual, item.PK);
			query.AddToFilter(MailDBItemsSchema.MI_POP3UIDL, uniqueId);

			foreach (var match in Factory.Load<MailItem>(query))
			{
				if (string.Equals(match.MI_POP3UIDL, uniqueId, StringComparison.Ordinal)) //because SQL comparison is case-insensitive
				{
					item.Delete();
					logger?.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, (NoResString)"Email dropped: already exists in database.\r\nUniqueId:{0}", uniqueId));
					logger?.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "RawEmail:{0}", email));
					return;
				}
			}

			var message = new MimeMessage();
			try
			{
				message = MimeMessageExtensions.CreateMessageFromEml(item.RawEmailBytes.ToArray());
			}
			catch (Exception ex)
			{
				var key = $"{GetType().Name}.SetupMail.{ex.GetType().Name}";
				ErrorReporter.ReportOnce(key, $"Exception parsing EML string: {email}", ex);
			}
			var attachments = message.GetFullAttachments();
			foreach (var attachment in attachments)
			{
				var fileName = ZString.Empty;
				if (attachment.ContentType != null && attachment is MessagePart)
				{
					var attachmentData = attachment.GetData();
					if (attachmentData?.Length > 0)
					{
						var innerMessage = MimeMessageExtensions.CreateMessageFromEml(attachmentData);
						fileName = PathValidation.GetSafeFilename(innerMessage.Subject, ' ').Trim();
						fileName = fileName.Left(MailDBAttachmentsSchema.MA_FileName.MaxLength - 4);

						if (fileName.Length == 0)
						{
							fileName = Res.GetString("d4cc3829-b249-43c4-af77-62a19f70ef9d", "Embedded Email");
						}
						fileName += ".eml";
					}
				}
				else
				{
					fileName = attachment.GetName();
				}

				if (!fileName.IsEmpty)
				{
					var mailAttachment = item.MailAttachments.AddNew();
					mailAttachment.MA_Data = attachment.GetData();
					fileName = MakeFilenameSafe.MakeSafeAndFixFileExtension(fileName, '_');
					mailAttachment.MA_FileName = fileName.Right(mailAttachment.MA_FileNameInfo.MaxLength);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void saver_PersistingEmail(string uniqueId, string email, ref bool continueDownloading)
		{
			if (string.IsNullOrEmpty(email))
			{
				logger?.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, (NoResString)"Email dropped: null or empty contents.\r\nUniqueId:{0}", uniqueId ?? (NoResString)"[null]"));
				return;
			}

			var items = new List<MailItem>();
			var item = Factory.New<MailItem>();
			items.Add(item);
			var success = false;

			try
			{
				SetupMail(item, uniqueId, email);
				if (item.IsDeleted)
				{
					return;
				}

				var filterLocator = new MailFilters.MailFilterLocator(true);
				var applicableFilters = filterLocator.GetFilters().Where(x => x.CanProcess(item)).ToList();

				if (applicableFilters.Any())
				{
					item.MI_Application = applicableFilters.First().Code;
					logger?.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, (NoResString)"Email assigned mail filter: \r\nUniqueId:{0} MailFilter:{1}", uniqueId, item.MI_Application));

					if (applicableFilters.Count > 1)
					{
						//for each additional filter, clone the mail item, only change being filter's code, so each filter can independently process mail
						foreach (var filter in applicableFilters.Skip(1))
						{
							var clone = Factory.New<MailItem>();
							clone.CopyValuesFrom(item);
							clone.MI_Application = filter.Code;
							logger?.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, (NoResString)"Email cloned, assigned mail filter: \r\nUniqueId:{0} MailFilter:{1}", uniqueId, clone.MI_Application));
							items.Add(clone);
						}
					}
				}
				else
				{
					logger?.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, (NoResString)"No mail filters found for email: \r\nUniqueId:{0} From:{1} Subject:{2} AllRecipients:{3}", uniqueId, item.MI_From, item.MI_Subject, item.AllRecipients));
				}

				success = true;
			}
			finally
			{
				if (!success)
				{
					items.ForEach(x => x.Delete());
				}
			}
		}

		protected BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public int FactorySaveCount { get; private set; }

		protected ILogger logger;

#if DEBUG
		public bool IsDeadlockExceptionThrown { get; set; }

		void ThrowDeadlockExceptionForTestIfNeeded()
		{
			if (IsDeadlockExceptionThrown)
			{
				var error = SqlExceptionBuilder.CreateSqlError(1205, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Deadlock...Something is wrong running this query.", "", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var exception = SqlExceptionBuilder.CreateSqlException(errors);
				var dataException = new ZDataException(exception, null, Db.Connection);
				var saveException = new ZSaveException(dataException, Factory);
				throw saveException;
			}
		}
#endif

	}
}
