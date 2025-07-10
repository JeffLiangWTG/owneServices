using System;
using System.IO;
using System.Xml;
using CargoWise.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Business.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.NZCustoms)]
	class NZCustomsReplyHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			string noteText = string.Empty;
			Message.MessageStream.SeekBegin();
			var interchange = base.CreateInterchange();
			var interchangeFrom = GetInterchangeFrom(Message.SenderID);

			using (var reader = XmlReader.Create(Message.MessageStream))
			{
				var reference = reader.GetElementAsString((NoResString)"Reference", "http://cargowise.com/ehub/products/");
				using (var interchangeStream = reader.GetElementAsStream((NoResString)"Content", "http://cargowise.com/ehub/products/"))
				{
					interchangeStream.Position = 0;
					var decodedInterchangeStream = interchangeStream.DecodeStream();

					string probeString = decodedInterchangeStream.GetFirstCharsAsString(20);

					if (probeString.StartsWith("<"))
					{
						interchange.EI_To = reference;
						interchange.EI_From = interchangeFrom;
						interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NZCustoms;
						interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.NZCustoms;
						interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
						interchange.SetEI_BodyTextSource(new TextReaderSource(decodedInterchangeStream));
					}
					else if (probeString.StartsWith("UNA") || probeString.StartsWith("UNB"))
					{
						try
						{
							interchange.PopulateInterchangeFromString(decodedInterchangeStream.ReadToEnd(), ApplicationCodeList.Codes.NZCustoms, true, true);
							interchange.EI_From = interchangeFrom;
							interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.NZCustoms;
							decodedInterchangeStream.Close();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							noteText = ex.Message;
							FailedInterchange(interchange, decodedInterchangeStream, interchangeFrom, reference, noteText);
						}
					}
					else
					{
						if (string.IsNullOrWhiteSpace(noteText))
						{
							noteText = Res.GetString("2AF4BE31-9E5C-4C06-B5A5-3C1939EE212B", "Message format was not recognized.");
						}

						FailedInterchange(interchange, decodedInterchangeStream, interchangeFrom, reference, noteText);
					}

					try
					{
						int attachmentCount = 0;
						while (reader.ReadUntilMatch((NoResString)"Attachment", "http://cargowise.com/ehub/products/"))
						{
							AddInterchangeAttachment(interchange, reader);
							attachmentCount++;
						}

						if (attachmentCount > 0)
						{
							EDocTransactionCoordinator.SaveFactory(interchange.DocManagerInfo.MasterFactory);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						FailedInterchange(interchange, Message.MessageStream, interchangeFrom, reference, Res.GetString("2AF4BE31-9E5C-4C06-B5A5-3C1939EE212C", "Attachment processing error: {0}", ex.Message));
						ErrorReporter.ReportOnce("NZCustoms interchange attachment processing error", ex);
					}
				}
			}

			return interchange;
		}

		static void AddInterchangeAttachment(EDIInterchange interchange, XmlReader reader)
		{
			string fileName = reader.GetAttribute("Filename");
			string extension = Path.GetExtension(fileName);
			extension = string.IsNullOrEmpty(extension) ? string.Empty : extension.Substring(1, extension.Length - 1);

			using (var stream = reader.GetElementAsStream((NoResString)"Attachment", "http://cargowise.com/ehub/products/"))
			{
				stream.Position = 0;

				using (var decodedStream = stream.DecodeStream())
				{
					decodedStream.Position = 0;
					var attachmentMemoryStream = new MemoryStream();
					decodedStream.CopyTo(attachmentMemoryStream);
					interchange.DocManagerInfo.AddFileOrDocument(attachmentMemoryStream.ToArray(), fileName, extension);
				}
			}
		}

		static void FailedInterchange(EDIInterchange interchange, Stream messagStream, string interchangeFrom, string reference, string noteText)
		{
			messagStream.Position = 0;
			interchange.EI_To = reference;
			interchange.EI_From = interchangeFrom;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NZCustoms;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.NZCustoms;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			interchange.SetEI_BodyTextSource(new TextReaderSource(messagStream));
			interchange.Notes.AddNew(true, Res.GetString("8D30242C-7351-4018-ADA4-482AB235D7D9", "Parse message error"), noteText);
		}

		string GetInterchangeFrom(string eHubMessageSenderID)
		{
			if (eHubMessageSenderID == "NZCustomsTest")
			{
				return EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox;
			}

			return EDIInterchange.InterchangePartyIDs.NZCustomsLiveMailbox;
		}

		class EDocTransactionCoordinator : TransactionCoordinator
		{
			public EDocTransactionCoordinator(ITransactionParticipant[] participants) : base(participants) { }

			protected override bool AllowMultipleParticipantsInTransaction
			{
				get { return true; }
			}

			public static void SaveFactory(ITransactionParticipant factory)
			{
				RowFactory.SaveTogether(new EDocTransactionCoordinator(new[] { factory }));
			}
		}
	}
}
