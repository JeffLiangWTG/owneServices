using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using Common.Logging;
using CargoWise.eHub.Gateway.ITCustoms;

namespace CargoWise.eHub.Gateway
{
	class ITCustomsRequestResponseMessageHandler : MessageHandler
	{
		protected static readonly ILog log = LogManager.GetLogger(typeof(ITCustomsRequestResponseMessageHandler));

		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			log.Debug("Processing ITCustoms Request Response message...");
			var systemID = GetClientSystemID(senderID);
			var xmlString = message.MessageStream.DecodeAndDecompress().ReadToEnd();
			var files = ParseFiles(message.SchemaName, xmlString);
			JobStatusManager.TriggerJobStatus(systemID, files);

			NewOutboxAccessor.GenerateSuccessStatusMessage(message.MessageTrackingID, senderID);
			log.Debug("Processing ITCustoms Request Response message...Finished");
		}

		private static Files ParseFiles(string ns, string xmlString)
		{
			using (var reader = XmlReader.Create(new StringReader(xmlString)))
			{
				var serializer = new XmlSerializer(typeof(Files));
				reader.ReadToDescendant("Files");
				var files = (Files)serializer.Deserialize(reader);
				if (files == null || files.File == null)
					throw new ArgumentException("Message does not contains FileName.");
				files.File = files.File.Where(x => x.Name != null).ToArray();
				return files;
			}
		}

		internal virtual IJobStatusManager JobStatusManager { get { return new JobStatusManager(); } }

		public virtual IOutboxAccessor NewOutboxAccessor // TODO: create OutboxHandler
		{
			get { return DataAccessFactories.NewOutboxAccessorInstance(); }
		}
	}
}
