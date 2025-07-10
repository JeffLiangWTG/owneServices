using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportCURRELMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICURREL>, ICURREL>
	{
		public ImportCURRELMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("CDFAC20C-35D1-49ED-A6E0-04EFC4BD0E2B", "Import CURREL Message Processor");

		protected override bool DelayStatusError => true;

		protected override bool UpdateWarehouseCore
		{
			get
			{
				var result = false;
				if (CurrentMessage?.EM_LinkedObject is CusEntryHeader cusEntryHeader)
				{
					result = (cusEntryHeader.IsIntoWarehouseWarehousing && cusEntryHeader.CH_EntryStatus == UniversalReferenceConstants.EntryStatus.RL5 && !cusEntryHeader.EntryInstruction.CEI_OA_Warehouse2.IsEmpty)
						|| cusEntryHeader.IsOutOfWarehouseWarehousing
						|| cusEntryHeader.Declaration.IsOutwardOrderImported;
				}
				return result;
			}
		}

		protected override bool UpdateWarehouseOrderForCurrentEntryLine(CusEntryLine entryLine) => entryLine.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL5;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICURREL> message)
		{
			CusEntryHeader result = null;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				var referenceNumber = dataProvider.ReferenceNumber;
				var mrn = dataProvider.MRN;
				var factory = message.Factory;

				result = GetEntryHeaderFromMRN(factory, mrn, referenceNumber);

				if (result == null)
				{
					result = GetEntryHeaderFromMRN(factory, dataProvider.TemporaryReferenceNumber);
					if (result != null)
					{
						var cusEntryNumber = result.CusEntryNumber;
						if ((!string.IsNullOrEmpty(referenceNumber) || !string.IsNullOrEmpty(mrn)) && cusEntryNumber != null)
						{
							cusEntryNumber.CE_EntryNum = string.IsNullOrEmpty(referenceNumber) ? mrn : referenceNumber;
							cusEntryNumber.CE_IssueDate = ZDateTime.Now;
						}
					}
				}
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICURREL> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

			message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;

			var dataProvider = message.DataProvider;
			var goodsItems = dataProvider.GoodsItems;
			var entryLines = entryHeader.AllEntryLines.ToList();
			message.SetLogbookRegistrationNumber(new ZString[] { dataProvider.ReferenceNumber, dataProvider.TemporaryReferenceNumber, dataProvider.MRN });

			if (!entryHeader.CH_EntryStatus.StartsWith(PrefixTX))
			{
				var reconEntry = entryHeader.GetCusReconEntry();

				var codesForWhichEventCanBeCreated = new List<string>()
				{
					UniversalReferenceConstants.EntryStatus.RL1,
					UniversalReferenceConstants.EntryStatus.RL2,
					UniversalReferenceConstants.EntryStatus.RL3,
					UniversalReferenceConstants.EntryStatus.RL4,
					UniversalReferenceConstants.EntryStatus.RL5,
				};

				foreach (var goodsItem in goodsItems)
				{
					var entryLine = entryLines.FirstOrDefault(x => x.CL_LineNumber.ToString() == goodsItem.SequenceNumber);
					if (entryLine != null)
					{
						SetEntryLineStatus(entryLine, goodsItem);

						var customsStatus = entryLine.ZG_CustomsStatus;
						if (codesForWhichEventCanBeCreated.Remove(customsStatus))
						{
							entryHeader.Logs.AddNew(Events.CustomsEntryStatus, customsStatus, ZDateTimeOffset.Now);
						}
						SetCusReconEntryLineStatusOrDeleteIfRequired(reconEntry, goodsItem.SequenceNumber, customsStatus);
					}
				}

				DeleteCusReconEntryIfRequired(reconEntry);

				if (entryLines.All(x => x.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL1))
				{
					entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL1;
				}
				else if (entryLines.All(x => x.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL2))
				{
					entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL2;
				}
				else if (entryLines.All(x => x.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL3))
				{
					entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL3;
				}
				else if (entryLines.All(x => x.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL4))
				{
					entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL4;
				}
				else if (entryLines.All(x => x.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL4 || x.ZG_CustomsStatus == UniversalReferenceConstants.EntryStatus.RL5))
				{
					entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RL5;
					if (entryHeader.CH_EntryReleaseDate.IsEmpty)
					{
						entryHeader.CH_EntryReleaseDate = ZDateTime.Now;
					}

					BondedWarehousingHelper.SetWarehouseTransactionStatusForImportMessageProcessing(entryHeader);
				}
				else
				{
					entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RLB;
				}
			}

			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, UniversalReferenceConstants.EntryStatus.RLB, ZDateTimeOffset.Now);

			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				attachedDocumentsCached = message.AttachedDocuments;

				var controlNotifications = GetCustomsControlNotifications(message.DataProvider);
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, GetEmailSubject(controlNotifications)
					, GetEmailBody(declaration, message.DataProvider, controlNotifications)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure,
			IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(email, GetDocumentsToAttachToEmail(attachedDocumentsCached));
			return email;
		}

		static IEnumerable<AttachedDocument> GetDocumentsToAttachToEmail(IReadOnlyCollection<AttachedDocument> attachedDocuments)
		{
			return attachedDocuments.Where(x =>
				x.Type.Description.HasValue && x.Type.Description.Value == ReportDescription);
		}

		void SetEntryLineStatus(CusEntryLine entryLine, ICURRELGoodsItem goodsItem)
		{
			if (DirectiveFlags.Contains(goodsItem.DirectiveFlag))
			{
				entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL3;
			}
			else if (goodsItem.IssuingFlag == FlagJ)
			{
				entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL5;
			}
			else
			{
				var acceptanceFlag = goodsItem.AcceptanceFlag;
				if (acceptanceFlag == FlagJ)
				{
					entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL2;
				}
				else if (goodsItem.RejectionFlag == FlagJ)
				{
					entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL4;
				}
				else if (acceptanceFlag == FlagN)
				{
					entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL1;
				}
				else if (goodsItem.RejectionFlag == FlagN)
				{
					entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL2;
				}
			}
		}

		void SetCusReconEntryLineStatusOrDeleteIfRequired(CusReconEntry reconEntry, ZString lineNumber, ZString customsStatus)
		{
			var cusReconEntryLine = reconEntry.GetCusReconEntryLineByOriginalEntryLineNumber(lineNumber);
			if (cusReconEntryLine != null)
			{
				if (customsStatus.In(new ZString[] { UniversalReferenceConstants.EntryStatus.RL1, UniversalReferenceConstants.EntryStatus.RL4 }))
				{
					cusReconEntryLine.Delete();
				}
				else
				{
					cusReconEntryLine.CRL_CustomsStatus = customsStatus;
				}
			}
		}

		void DeleteCusReconEntryIfRequired(CusReconEntry cusReconEntry)
		{
			if (cusReconEntry != null && cusReconEntry.CusReconEntryLines.Count == 0)
			{
				cusReconEntry.Delete();
			}
		}

		static string GetEmailSubject(List<string> controlNotifications)
		{
			if (!controlNotifications.Any())
			{
				return Res.GetString("24A94501-CC14-4213-B1A2-F89870FBD0FF", "Import CURREL – Customs Release");
			}

			var joinedNotifications = string.Join(" / ", controlNotifications);
			return Res.GetString("8E2247F6-5D0E-49CD-8F16-42DC9BD32043", "Import CURREL – Customs Release – {0}", joinedNotifications);
		}

		static List<string> GetCustomsControlNotifications(ICURREL provider)
		{
			var rejectionFlag = provider.GoodsItems.Any(x => x.RejectionFlag == FlagJ);
			var notAcceptedFlag = provider.GoodsItems.Any(x => x.AcceptanceFlag == FlagN);
			var customsControlFlag = provider.GoodsItems.Any(x => DirectiveFlags.Contains(x.DirectiveFlag));

			var notifications = new List<string>();
			if (rejectionFlag)
			{
				notifications.Add(Res.GetString("A17F9B97-E915-42DB-9496-F3D3D7E12C04", "Declaration rejected"));
			}
			if (notAcceptedFlag)
			{
				notifications.Add(Res.GetString("CDA6FDE7-27BE-43C0-A9FF-DA66C77827C3", "Declaration not accepted"));
			}
			if (customsControlFlag)
			{
				notifications.Add(Res.GetString("CCAA52FB-830A-41C8-864A-06A81EA7ED58", "Customs Control/Inspection"));
			}

			return notifications;
		}

		static ZString GetEmailBody(JobDeclaration declaration, ICURREL provider, List<string> controlNotifications)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("71BA8142-C251-451A-A212-09FD5F2D5B4E", "Your Import Declaration for Job {0} received a Customs Response for Release. For details please follow the link to the job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var temporaryReferenceNumber = provider.TemporaryReferenceNumber;
			var referenceNumber = provider.ReferenceNumber;
			var localReferenceNumber = provider.LocalReferenceNumber;
			var mrn = provider.MRN;
			var modalitiesNotification = provider.PresentationModalitiesNotification;

			if (!string.IsNullOrEmpty(temporaryReferenceNumber) || !string.IsNullOrEmpty(referenceNumber) || !string.IsNullOrEmpty(localReferenceNumber) || !string.IsNullOrEmpty(modalitiesNotification) || !string.IsNullOrEmpty(mrn)
				|| controlNotifications.Any())
			{
				var tableCreator = new HtmlTableCreator();
				if (!string.IsNullOrEmpty(temporaryReferenceNumber))
				{
					tableCreator.WriteRow(Res.GetString("A920C9D7-54B3-468E-95E7-77F6BD4356A3", "Temporary Registration Number"), temporaryReferenceNumber);
				}
				if (!string.IsNullOrEmpty(referenceNumber))
				{
					tableCreator.WriteRow(Res.GetString("7D1CD059-A7E2-40A6-931A-CC1DCF7F47F4", "Registration Number"), referenceNumber);
				}
				if (!string.IsNullOrEmpty(localReferenceNumber))
				{
					tableCreator.WriteRow(Res.GetString("C840598B-BCC4-47AF-A837-09997674EFD0", "Local Reference Number"), localReferenceNumber);
				}
				if (!string.IsNullOrEmpty(mrn))
				{
					tableCreator.WriteRow(Res.GetString("A66DA7E1-0059-4198-A786-5487025D74C6", "MRN"), mrn);
				}
				if (!string.IsNullOrEmpty(modalitiesNotification))
				{
					tableCreator.WriteRow(Res.GetString("6104C884-FD49-4D35-8479-2CDD0ED4BEA9", "Customs Notification"), modalitiesNotification);
				}
				if (controlNotifications.Any())
				{
					var joinedNotifications = string.Join("\n", controlNotifications);
					tableCreator.WriteRow(Res.GetString("E1926B29-2930-47AE-9DD8-A40E99A10356", "Notification"),
						joinedNotifications);
				}
				htmlBody.Append(tableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;

		static readonly ImmutableHashSet<string> DirectiveFlags = new HashSet<string> { "1", "2", "3", "4", "5", "9" }.ToImmutableHashSet();

		const string PrefixTX = "TX";

		const string FlagJ = "J";

		const string FlagN = "N";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description string")]
		const string ReportDescription = "Report GCRELF";
	}
}
