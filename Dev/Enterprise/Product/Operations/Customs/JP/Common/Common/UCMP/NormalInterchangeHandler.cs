using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using MimeKit;

namespace Enterprise.Customs.JP.Common
{
	sealed class NormalInterchangeHandler
	{
		public NormalInterchangeHandler(NACCSMessageParentFinder parentFinder)
		{
			this.parentFinder = Argument.NotNull(parentFinder, nameof(parentFinder));
		}

		readonly NACCSMessageParentFinder parentFinder;

		public EDIInterchangeUnpackerResult CreateNACCSMessage(Messaging.Business.EDIInterchange interchange, BusinessObject messageParent, LoggingInformation logger)
		{
			var factory = interchange.Factory;
			byte[] messageData = null;

			var messages = new List<Messaging.Business.EDIMessage>();

			try
			{
				using (var stream = interchange.GetEI_BodyDataReader())
				using (var mimeMessage = MimeMessage.Load(stream))
				{
					messageData = JPMessageUtils.ConvertStringToMessage(mimeMessage.TextBody);
				}
			}
			catch (FormatException ex)
			{
				logger.Log($"The body data of {interchange.EI_InterchangeNum} is not MimeMessage. Format Result: {ex.Message}");
				messageData = interchange.EI_BodyData;
			}
			finally
			{
				var warningMessage = string.Empty;

				messageParent ??= (parentFinder.FindParentFromMessageData(factory, messageData) ?? parentFinder.FindParentFromSubject(factory, messageData));
				if (messageParent == null)
				{
					warningMessage = "Unable to find business object for message ";
				}

				var branchPk = (messageParent as IBranchProvider)?.Branch?.PK ?? interchange.EI_GB;

				using (DisposableEnvironment.ForBranch(branchPk.ToGuid()))
				{
					var message = EDIMessage.CreateFromInboundMessageLoad(interchange.Factory, messageData);

					if (string.IsNullOrEmpty(warningMessage))
					{
						message.EM_LinkedObject = messageParent;
						message.EM_Status = EDIMessage.Status.Queued;
					}
					else
					{
						message.EM_Status = EDIMessage.Status.Discarded;
						if (message.EM_MessageNum.IsEmpty)
						{
							message.EM_MessageNumInfo.ValueChanged += (s, e) =>
							{
								logger.LogWarning(warningMessage + $"(Number:{message.EM_MessageNum})");
							};
						}
						else
						{
							logger.LogWarning(warningMessage + $"(Number:{message.EM_MessageNum})");
						}
					}

					interchange.EI_GB = branchPk;
					message.EM_GB = branchPk;

					interchange.ContainedMessages.Add(message);
					messages.Add(message);
				}
			}

			return new EDIInterchangeUnpackerResult(messages);
		}
	}
}
