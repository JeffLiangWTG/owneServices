#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.Extensions;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	[CodeProperty(Schema.XP_RequestID)]
	[DescriptionProperty(Schema.XP_ReasonDescription)]
	public class GLJournalApprovalRequest : TransactionApprovalRequest<GLJournalApprovalRequestDetails>, IDocManagerSupport, IEDocsParsingSupport
	{
		public enum Context
		{
			Posting,
			Editing
		}

		public GLJournalApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void Initialize(GLJournal journal)
		{
			Argument.NotNull(journal, nameof(journal));

			OriginalJournalPK = journal.PK;
			OriginalJournalHumanReadableName = journal.HumanReadableName;
			if (journal.IsInDatabase)
			{
				base.Initialize(journal.PK, journal.TablePrefix);
			}
			else
			{
				base.Initialize(ZGuid.Empty, journal.TablePrefix);

				if (journal.Factory.HasContext(Context.Editing))  // if in edit mode, then relink edocs to current request
				{
					ReLinksEdocsFromPreviousRequestToCurrentRequest(journal);
				}

				LinkRequestToJournal(journal);
			}

			SerializeJournalToPostingDetailsXML(journal);
		}

		void ReLinksEdocsFromPreviousRequestToCurrentRequest(GLJournal journal)
		{
			var previousRequest = journal.GetLatestLinkedApprovalRequestInDB();
			if (previousRequest != null)
			{
				var storageMainOnPreviousRequest = previousRequest.StorageMain;
				if (storageMainOnPreviousRequest != null)
				{
					var storageDocsOnPreviousRequest = storageMainOnPreviousRequest.eDocs;
					if (storageDocsOnPreviousRequest != null && storageDocsOnPreviousRequest.Count > 0)
					{
						var storageMainOnNewRequest = StorageMain;

						foreach (StorageDocsBase eDoc in storageDocsOnPreviousRequest.Cast<StorageDocsBase>().ToArray())
						{
							((DocumentFactory)DocManagerInfo.MasterFactory).Allocate(eDoc, PK);
						}
						Factory.ChildFactories.Add(storageMainOnNewRequest.MasterFactory);
						Factory.ChildFactories.Add(storageDocsOnPreviousRequest.MasterFactory);
					}
				}
			}
		}

		#region LatestLinkedApprovalRequestInDBPK

		internal ZGuid LatestLinkedApprovalRequestInDBPK { get; private set; }

		void LinkRequestToJournal(GLJournal journal)
		{
			journal.SetLatestLinkedApprovalRequest(this);
			var journalLatestLinkedApprovalRequest = journal.GetLatestLinkedApprovalRequestInDB();
			LatestLinkedApprovalRequestInDBPK = journalLatestLinkedApprovalRequest != null ? journalLatestLinkedApprovalRequest.PK : ZGuid.Empty;
		}

		#endregion

		internal ZGuid OriginalJournalPK { get; private set; }
		ZString OriginalJournalHumanReadableName { get; set; }

		internal void PrepareToPost(ZGuid postingJournalPK, ZString defaultDescription)
		{
			if (!XP_ParentID.IsValid)
			{
				XP_ParentID = postingJournalPK;
			}
			if (XP_ReasonDescription.IsEmpty)
			{
				XP_ReasonDescription = defaultDescription;
			}
		}

		internal void SetJournalTransactionNumber(GLJournal postingJournal)
		{
			if (XP_ParentID == postingJournal.PK)
			{
				SerializeJournalToPostingDetailsXML(postingJournal); //this is faster then desrialize journal in posting details, just to update number and serialize again.
			}
			else
			{
				ErrorReporter.ReportOnce("GLJournalApprovalRequest.SetJournalTransactionNumber", string.Format("Request PK {0} must be equal journal PK {1}", XP_ParentID, postingJournal.PK));
			}
		}

		public GLJournal.LoadedJournalWithMutex GetLinkedJournal(BusinessObjectFactory factory = null, bool loadJournalWithMutex = false)
		{
			var factoryForJournal = factory ?? Factory;
			var result = new GLJournal.LoadedJournalWithMutex();
			if (!XP_ParentID.IsEmpty)
			{
				if (loadJournalWithMutex)
				{
					result = GLJournal.LoadWithMutex(XP_ParentID, factoryForJournal);
				}
				else
				{
					result.journal = factoryForJournal.Load<GLJournal>(XP_ParentID);
				}
			}

			if (XP_ParentID.IsEmpty || result.journal != null)
			{
				ReadXMLFromBlobAndDeserialize(XP_ApprovalRequestData, reader =>
				{
					var tempDetails = CreatePostingApprovalDetails(factoryForJournal);

					tempDetails.JournalReplacementForDeserialization = result.journal;
					tempDetails.FactoryReplacementForDeserialization = factoryForJournal;

					((IXmlSerializable)tempDetails).ReadXml(reader);

					var shouldNewJournalBeCreatedByAdapter = result.journal == null;
					if (shouldNewJournalBeCreatedByAdapter)
					{
						result.journal = tempDetails.Journal;
						LinkRequestToJournal(result.journal);
					}
				});

				var lastRequest = result.journal.GetLatestLinkedApprovalRequestInDB();
				if (lastRequest != null)
				{
					var originalJournal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, lastRequest.PK));
					if (originalJournal != null)
					{
						result.journal.OriginalTransaction = originalJournal;
						result.journal.IsReverseTransaction = true;
						result.journal.Lines.SetReadOnlyIncludingChildren(true);
					}
				}
			}

			return result;
		}

		[ResourceStringData("JournalNumber", Caption = "Journal Number", ShortCaption = "Journal Num.")]
		public ZString JournalNumber
		{
			get
			{
				if (IsPostingDetailsLoaded)
				{
					journalNumber = null;
					return PostingDetails.Journal != null ? PostingDetails.Journal.AH_TransactionNum : ZString.Empty;
				}

				if (!journalNumber.HasValue)
				{
					MemoryStream stream = null;
					try
					{
						stream = new MemoryStream(XP_ApprovalRequestData);
						using (XmlReader reader = XmlReader.Create(stream))
						{
							stream = null;
							reader.MoveToContent();
							var root = XElement.Load(reader);
							var journalNumberNode = MasterFiles.Business.Extensions.GetPropertyInfo((Xsd.GLJournalGLDetail x) => x.JournalNumber).Name;
							var element = root.Descendants().FirstOrDefault(c => c.Name.LocalName == journalNumberNode);
							journalNumber = element != null ? (ZString)element.Value : ZString.Empty;
						}
					}
					finally
					{
						if (stream != null)
						{
							stream.Dispose();
						}
					}
				}

				return journalNumber ?? ZString.Empty;
			}
		}
		ZString? journalNumber;

		#region IsRequestDataXMLTheSame

		static bool IsRequestDataXMLTheSame(ZBlob xP_ApprovalRequestData1, ZBlob xP_ApprovalRequestData2)
		{
			MemoryStream stream1 = null;
			MemoryStream stream2 = null;
			try
			{
				stream1 = new MemoryStream(xP_ApprovalRequestData1);
				stream2 = new MemoryStream(xP_ApprovalRequestData2);
				using (var reader1 = XmlReader.Create(stream1))
				using (var reader2 = XmlReader.Create(stream2))
				{
					stream1 = null;
					stream2 = null;
					reader1.MoveToContent();
					reader2.MoveToContent();
					var root1 = XElement.Load(reader1);
					var root2 = XElement.Load(reader2);

					return IsRequestDataXMLTheSame(root1, root2);
				}
			}
			finally
			{
				if (stream1 != null)
				{
					stream1.Dispose();
				}
				if (stream2 != null)
				{
					stream2.Dispose();
				}
			}
		}

		static bool IsRequestDataXMLTheSame(XElement root1, XElement root2)
		{
			var journalNode = nameof(Xsd.GLJournal);
			var journal1 = root1.Descendants().FirstOrDefault(x => x.Name.LocalName == journalNode);
			var journal2 = root2.Descendants().FirstOrDefault(x => x.Name.LocalName == journalNode);

			var journalLinesNode = GetPropertyInfo((Xsd.GLJournal x) => x.JournalLines).Name;
			var elementsExcludingLines1 = journal1?.Elements().Where(x => x.Name.LocalName != journalLinesNode).ToList();
			var elementsExcludingLines2 = journal2?.Elements().Where(x => x.Name.LocalName != journalLinesNode).ToList();
			var isTheSame = IsElementsTheSameInAnyOrder(elementsExcludingLines1, elementsExcludingLines2);

			if (isTheSame)
			{
				var lines1 = journal1?.Elements().FirstOrDefault(x => x.Name.LocalName == journalLinesNode)?.Elements().ToList();
				var lines2 = journal2?.Elements().FirstOrDefault(x => x.Name.LocalName == journalLinesNode)?.Elements().ToList();

				var linePKNode = GetPropertyInfo((Xsd.GLJournalJournalLine x) => x.PK).Name;
				RemovePKNodeValues(lines1, linePKNode);
				RemovePKNodeValues(lines2, linePKNode);

				isTheSame = IsElementsTheSameInAnyOrder(lines1, lines2);
			}

			return isTheSame;
		}

		static void RemovePKNodeValues(List<XElement> elementsWithPKNodes, string pkElementName)
		{
			elementsWithPKNodes?.ForEach(elementWithPKNodes => elementWithPKNodes.Descendants().Where(x => x.Name.LocalName == pkElementName).ToList().ForEach(x => x.Value = ""));
		}

		static bool IsElementsTheSameInAnyOrder(List<XElement> listA, List<XElement> listB)
		{
			if (listA == null && listB == null)
			{
				return true;
			}

			if (listA == null || listB == null)
			{
				return false;
			}

			if (listA.Count != listB.Count)
			{
				return false;
			}

			for (int indexA = 0; indexA < listA.Count; indexA++)
			{
				var xmlA = listA[indexA].ToString();
				int indexB;
				for (indexB = 0; indexB < listB.Count; indexB++)
				{
					var xmlB = listB[indexB].ToString();
					if (xmlA == xmlB)
					{
						listA.RemoveAt(indexA--);
						listB.RemoveAt(indexB--);
						break;
					}
				}
				if (indexB == listB.Count)
				{
					break;
				}
			}

			return listA.Count == 0 && listB.Count == 0;
		}

		#endregion

		#region Overrides

		protected override ZString ReferenceTypeCore => Res.GetString("9680FC83-3ACD-43B4-B36B-CF491E39B5A1", "Journal");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			XP_ApprovalType = Core.Constants.GenApprovalRequestApprovalType.GLJournal;
		}

		protected override Type ApprovingTransactionTypeCore
		{
			get { return typeof(GLJournal); }
		}

		protected override GLJournalApprovalRequestDetails CreatePostingApprovalDetails()
		{
			return CreatePostingApprovalDetails(Factory);
		}

		static GLJournalApprovalRequestDetails CreatePostingApprovalDetails(BusinessObjectFactory factory)
		{
			return new GLJournalApprovalRequestDetails(factory);
		}

		protected override TransactionApprovalRequestEmail CreateEmail()
		{
			return new GLJournalApprovalRequestEmail(this, OriginalJournalHumanReadableName);
		}

		protected override bool ShouldEmailBeSent
		{
			get { return (!IsInDatabase && XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested) || base.ShouldEmailBeSent; }
		}

		protected override INumberFountainProxy NumberFountainForRequestID
		{
			get { return Env.NumberFountains.GLJournalApprovalRequestReferenceID; }
		}

		protected override ZString ReferenceIDCore
		{
			get { return XP_RequestID; }
		}

		protected override void OnUpdatedByDataRefresh()
		{
			//base is not called to not read Posting Details on data refresh for performance reasons

			journalNumber = null;
		}

		protected override void OnAfterReadPostingDetails()
		{
			base.OnAfterReadPostingDetails();

			IsPostingDetailsLoaded = true;
		}

		bool IsPostingDetailsLoaded;

		protected override bool IsPostingActionTheSameCore(TransactionApprovalRequest<GLJournalApprovalRequestDetails> request) => request is GLJournalApprovalRequest; //only one posting action is allowed for this type

		protected override bool ArePostingDetailsTheSameCore(TransactionApprovalRequest<GLJournalApprovalRequestDetails> request)
		{
			var glJournalRequest = request as GLJournalApprovalRequest;
			if (glJournalRequest == null)
			{
				return false;
			}

			bool isTheSame = false;
			if (!IsPostingDetailsLoaded || !glJournalRequest.IsPostingDetailsLoaded)
			{
				isTheSame = IsRequestDataXMLTheSame(XP_ApprovalRequestData, glJournalRequest.XP_ApprovalRequestData);
			}
			if (!isTheSame)
			{
				isTheSame = base.ArePostingDetailsTheSameCore(glJournalRequest);
			}

			return isTheSame;
		}

		#endregion

		void SerializeJournalToPostingDetailsXML(GLJournal journal)
		{
			using (CreateTemporaryPostingDetails())
			{
				PostingDetails.JournalReplacementForSerialization = journal;
				WritePostingDetails(true);
			}
		}

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.GLJournalApprovalRequest);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		public StorageMain StorageMain
		{
			get
			{
				if (storageMain == null)
				{
					storageMain = DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(this, Core.Constants.DocManagerCodes.GLJournalApprovalRequest) as StorageMain;
				}
				return storageMain;
			}
		}
		StorageMain storageMain;

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}
