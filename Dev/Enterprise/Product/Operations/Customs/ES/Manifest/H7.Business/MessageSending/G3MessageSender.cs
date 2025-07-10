using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3MessageSender
	{
		public G3MessageSender(G3MessageSendingObjectParent sendingObjectParent)
		{
			this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
		}

		readonly G3MessageSendingObjectParent sendingObjectParent;
		readonly int chunkSize = 9999;

		ICertificateProvider CertificateData
		{
			get
			{
				if (certificateData == null)
				{
					certificateData = sendingObjectParent.Header.Certificate;
				}
				return certificateData;
			}
		}
		ICertificateProvider certificateData;

		AsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = sendingObjectParent.Header;
				}
				return header;
			}
		}
		AsycudaManifestHeader header;

		public int Send()
		{
			var messagesSent = 0;
			var sendingObjects = sendingObjectParent.SelectedSendingObjects
				.OrderBy(x => x.Bill.ABL_BillNumber);
			var chunks = IEnumerableExtensions.Chunk(sendingObjects, chunkSize).Select(x => x.ToArray());
			var lrnArr = GenerateG3LRNs(Header, chunks.Count());

			if (!lrnArr.IsNullOrEmpty())
			{
				int index = 0;
				foreach (var chunk in chunks)
				{
					if (index < lrnArr.Length && ProcessGroup(chunk, lrnArr[index]))
					{
						messagesSent++;
					}
					index++;
				}

				if (messagesSent > 0)
				{
					Header.Messages.Reload(false);
				}
			}

			return messagesSent;
		}

		ZString[] GenerateG3LRNs(AsycudaManifestHeader header, int count)
		{
			try
			{
				using (var transactionManager = ((IDbConnected)header.Factory).Connection.BeginTransactionWithManager())
				{
					var eori = OrgHeaderExtension.GetEORIForLRNGeneration(header);
					var lrnArr = Enumerable.Range(0, count)
						.Select(_ => header.GenerateLocalReferenceNumber(eori))
						.ToArray();
					transactionManager.CommitTransaction();

					return lrnArr;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("G3MessageSender.Send", "Exception thrown when trying to generate G3LRN to send declaration", ex);
				return null;
			}
		}

		bool ProcessGroup(G3MessageSendingObject[] chunk, string lrn)
		{
			try
			{
				var messageBuilder = new G3MessageBuilderManager(chunk, CertificateData, lrn).NewMessageBuilder();
				var messageCreator = new EDIMessageCreator(messageBuilder, Header.Factory);
				var message = messageCreator.CreateMessage();
				message.EM_LinkedObject = Header;
				message.EM_MessageText = chunk.FirstOrDefault()?.MessageCreated(message.EM_MessageText);

				foreach (var sendingObject in chunk)
				{
					var bill = sendingObject.Bill;
					bill.G3LocalReferenceNumber = lrn;
					bill.ABL_MessageStatus = LogicalStatusList.Codes.Sent;
				}

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("G3MessageSender.Send", "Exception thrown when trying to send declaration", ex);
				return false;
			}
		}
	}
}
