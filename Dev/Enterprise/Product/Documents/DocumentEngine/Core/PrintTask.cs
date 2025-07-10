using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Scheduler.Module;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using DeliveryFormats = Enterprise.DocumentEngine.DeliveryMethods.DeliveryInfo.DeliveryFormats;
using StmPrintJob = Enterprise.DocumentEngine.Scheduler.Business.StmPrintJob;

namespace Enterprise.DocumentEngine
{
	#region Allowed Delivery Options Enumeration

	/// <summary>
	/// Defines the different delivery options allowed when printing a document.
	/// </summary>
	public enum AllowedDeliveryOptions
	{
		All,
		AllExceptPreview,
		HardCopyOnly,
		PreviewOnly,
		OverridePrintDetails
	}

	#endregion

	/// <summary>
	/// A single printing task from the user's point of view.
	/// </summary>
	public class PrintTask : IPrintTask, IDisposable
	{
		public PrintTask()
		{
		}

		public PrintTask(IStmMenuItem parentMenuCommand)
		{
			fParentMenuCommand = parentMenuCommand;
		}

		protected PrintTask(IStmMenuItem parentMenuCommand, IEnumerable<DocumentPack> docPacks)
		{
			fParentMenuCommand = parentMenuCommand;
			documentPacks = docPacks;
		}

		//DO NOT call .ToList() / .ToArray() / ... on DocumentPacks as it will force a full evaluation of the IEnumerable at once.
		//This is an IEnumerable for performance and memory useage reasons. When using stream mode (UseStreamMode), each pack is built only when it is iterated through / required, then processed, then disposed. This keeps the memory consumption constant.
		protected IEnumerable<DocumentPack> DocumentPacks => documentPacks;

		IEnumerable<DocumentPack> documentPacks = new List<DocumentPack>();

		public virtual DocumentPack this[int index]
		{
			get { return DocumentPacks.ElementAt(index); }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("DocumentPack");
				}
				(DocumentPacks as List<DocumentPack>)[index] = value;
			}
		}

		public bool IsAutoDocumentDelivery { get; set; }

		public bool SupportsLanguageSelectionOverride { get; protected set; }

		public INotifications CustomNotifications { get; set; }

		public IEnumerable<DocumentPack> GetDocumentPacks()
		{
			foreach (DocumentPack pack in DocumentPacks)
			{
				yield return pack;
			}
		}

		public DocumentPack GetFirstDocumentPack() => DocumentPacks.First();

		public BusinessObject MostTopLevelBusinessObject { get; set; }

		public ZBool UseStreamMode { get; protected set; }

		public virtual void ResetCachedReports()
		{
			foreach (var docPack in GetDocumentPacks())
			{
				foreach (var report in docPack.OfType<Report>())
				{
					report.ResetCachedExcelFile();
				}
			}
		}

		public static IDisposable SuspendDocumentPackAutoAddEDocs()
		{
			DocumentPack.AutoAddeDocsSuspendedCount.Value++;
			return new DisposableAction(() => DocumentPack.AutoAddeDocsSuspendedCount.Value--);
		}

		#region Parent Menu Item

		public IStmMenuItem ParentMenuCommand => fParentMenuCommand;

		readonly IStmMenuItem fParentMenuCommand;

		#endregion

		#region Print Task Settings

		public PrintTaskSettings TaskSettings
		{
			get
			{
				if (taskSettings == null)
				{
					taskSettings = new PrintTaskSettings(this);
				}
				return taskSettings;
			}
		}

		PrintTaskSettings taskSettings;

		#endregion

		#region DeliveryInstructionsForDeliveryForm

		internal DeliveryInstructions DeliveryInstructionsForDeliveryForm { get; private set; }

		#endregion

		#region Run

		#region Delivery Options

		/// <summary>
		/// Run print task by showing a form to ask user to enter delivery instructions.
		/// </summary>
		public DeliveryInstructionDestination Run(ISecurityCheckpoint modifyDocumentCheckPoint, bool isRunFromMenuCustomisationForm)
		{
			if (isRunFromMenuCustomisationForm)
			{
				if (DocumentPacks?.Count() > 0)
				{
					foreach (DocumentPack pack in GetDocumentPacks())
					{
						pack.IsRunFromMenusCustomisationForm = true;
					}
				}
				else
				{
					this.PrintTaskUIProvider.ShowWarning(Res.GetString("a7c025b9-bae8-404f-bddc-4496b9847911", "Document Errors"), Res.GetString("C2DA6DE1-8095-4F2A-89B6-5B0D65C77B58", "There is no valid document that can be delivered. If this is a document pack document with only template configurations, please copy the configuration into a non-template before using it."));
				}

				if (ParentMenuCommand != null)
				{
					if (((AutoStmMenuItem)ParentMenuCommand).NotificationsIncludingChildren.GetErrors().Count() > 0)
					{
						this.PrintTaskUIProvider.ShowWarning(Res.GetString("a7d025b9-bde8-404f-bddc-4496b9847900", "Errors are present"), Res.GetString("0a9d45f3-7c50-4fcc-8c06-bc52e5060369", "Please resolve errors before trying to run the document"));
						return DeliveryInstructionDestination.None;
					}
				}
			}

			return Run(modifyDocumentCheckPoint);
		}

		/// <summary>
		/// Run print task by showing a form to ask user to enter delivery instructions.
		/// </summary>
		public DeliveryInstructionDestination Run(ISecurityCheckpoint modifyDocumentCheckPoint) => Run(AllowedDeliveryOptions.All, modifyDocumentCheckPoint);

		/// <summary>
		/// Run print task by showing a form to ask user to enter delivery instructions.
		/// </summary>
		public DeliveryInstructionDestination Run(AllowedDeliveryOptions deliveryOptions, ISecurityCheckpoint modifyDocumentCheckPoint) => RunWithPartialInstructions(deliveryOptions, null, modifyDocumentCheckPoint);

		#endregion

		#region Mutliple Document Packs

		#region Run With Delivery Instructions Form

#if DEBUG
		virtual
#endif
		public void RunWithDeliveryForm(AllowedDeliveryOptions deliveryOptions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			TaskSettings.ModifyDocumentCheckPoint = modifyDocumentCheckPoint;
			TaskSettings.DeliveryOptions = deliveryOptions;
			SetDeliveryInstructionDefaults();

			if (DeliveryInstructionsDefaultPK.IsValid)
			{
				TaskSettings.AllowSaveDefaults = true;
				LoadPrinterDeliveryDefaults(TaskSettings);
			}
			else
			{
				TaskSettings.AllowSaveDefaults = false;
			}

			if (TaskSettings.DeliveryOptions == AllowedDeliveryOptions.HardCopyOnly)
			{
				ShowHardCopyOnlyForm(TaskSettings);
			}
			else
			{
				ShowDeliveryForm(TaskSettings);
			}

			Run(TaskSettings);
			SavePrinterDeliveryDefaults(TaskSettings);
		}

		protected virtual void ShowDeliveryForm(PrintTaskSettings taskSettings)
		{
			taskSettings.Destination = DeliveryInstructionDestination.TakenFromContact;

			if (!PrintTaskUIProvider.ShowPrintTaskDeliveryUI(taskSettings))
			{
				taskSettings.Destination = DeliveryInstructionDestination.UserCancelled;
			}

			foreach (DeliveryInstructions instructions in taskSettings.DocPacksDeliveryInstructions)
			{
				instructions.PrinterDelivery = taskSettings.PrinterDelivery;
				if (instructions.Destination == DeliveryInstructionDestination.Auto)
				{
					SelectPrinterForAutoDeliveryIfRequired(instructions);
				}
			}
		}
		#endregion

		#region Automated Run

		/// <summary>
		/// Run print task using the given delivery instructions - does not show the instructions form.
		/// </summary>
		/// 

#if DEBUG
		internal virtual
#endif
		void Run(PrintTaskSettings taskSettings)
		{
			if (taskSettings == null)
			{
				throw new ArgumentNullException("PrintTaskSettings TaskSettings");
			}

			if (taskSettings.Destination != DeliveryInstructionDestination.UserCancelled)
			{
				using (PrintTaskUIProvider.GetNewProgressNotificationUI(taskSettings))
				{
					try
					{
						taskSettings.OnStartDocProcessing();

						int index = 0;
						foreach (DocumentPack documentPack in GetDocumentPacks())
						{
							if (documentPack == null)
							{
								throw new InvalidOperationException("You cannot call Run(PrintTaskSettings TaskSettings) twice in a row without setting up a new set of DocumentPacks.");
							}

							DeliveryInstructions currentInstructions = documentPack.DeliveryInstructions;
							if (currentInstructions.Destination != DeliveryInstructionDestination.UserCancelled)
							{
								var e = new DocumentPrintedEventArgs(currentInstructions.Destination, documentPack.StmMenuCommand, taskSettings.IsDraft, documentPack);
								if (currentInstructions.Destination == DeliveryInstructionDestination.Preview)
								{
									NotifyDocumentPrePreviewed(documentPack, e);
								}
								else
								{
									NotifyDocumentPrePrinted(documentPack, e);
								}

								if (currentInstructions.UsesDeliveryGroup)
								{
									currentInstructions.SetAndSaveDeliveryGroupSubjectLine(documentPack, GenerateSubjectLineMappingForPrintTask(documentPack));
								}

								currentInstructions.OnDocPackStarted(index);

								documentPack.OnAfterReportRun += new AfterReportRunEventHandler(OnAfterReportRun);
								try
								{
									documentPack.Run(currentInstructions);
								}
								finally
								{
									documentPack.OnAfterReportRun -= new AfterReportRunEventHandler(OnAfterReportRun);
								}

								currentInstructions.OnDocPackPrinted(index + 1);
							}

							if (currentInstructions.UsesDeliveryGroup)
							{
								MarkJobsAsProcessedAndDeleteUnusedGroups(currentInstructions);
							}

							++index;

							if (UseStreamMode)
							{
								documentPack.Dispose();
							}
						}
					}
					finally
					{
						taskSettings.OnEndDocProcessing();
					}
				}
			}
		}

		#endregion

		#endregion

		#region Partial Instructions

		/// <summary>
		/// Run print task by showing a form based on the partial instructions passed in,
		/// and allows the user to specify further instructions.
		/// 
		/// Primarily used by eDocs.
		/// </summary>

#if DEBUG
		virtual
#endif
		public DeliveryInstructionDestination RunWithPartialInstructions(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions deliveryInstructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			var result = DeliveryInstructionDestination.None;

			if (deliveryInstructions == null)
			{
				deliveryInstructions = this.Count == 1 && !UseStreamMode ? new DeliveryInstructions(this[0]) : new DeliveryInstructions();
			}
			deliveryInstructions.AllowAutoDelivery = PermissionForAutoDelivery;

			DeliveryInstructionsForDeliveryForm = deliveryInstructions;

			var isRuntimOptionsFormShown = false;
			if (IsReportPrintSet)
			{
				isRuntimOptionsFormShown = ShowRuntimeOptionsForm(deliveryOptions, deliveryInstructions, modifyDocumentCheckPoint);
			}

			result = !isRuntimOptionsFormShown
				? ShowDeliveryDialogForm(deliveryOptions, deliveryInstructions, modifyDocumentCheckPoint)
				: DeliveryInstructionDestination.UserCancelled;

			return result;
		}

		#endregion

		#region Automated Run

		internal void NotifyDeliveryInfoCreated(DeliveryInfo deliveryInfo, DocDeliveryContact deliveryContact)
		{
			if (deliveredWithinGivenRun != null && deliveryInfo != null)
			{
				if (!deliveredWithinGivenRun.ContainsKey(deliveryInfo.DeliveryGroupID))
				{
					deliveredWithinGivenRun.Add(deliveryInfo.DeliveryGroupID, new List<DeliveryInfoSnapshot>());
				}
				deliveredWithinGivenRun[deliveryInfo.DeliveryGroupID].Add(new DeliveryInfoSnapshot(deliveryInfo, deliveryContact));
			}
		}

		internal bool IsSimilarDeliveryAlreadyProcessed(DeliveryInfo deliveryInfo, DocDeliveryContact deliveryContact)
		{
			if (deliveredWithinGivenRun == null || deliveryInfo == null)
			{
				return false;
			}

			List<DeliveryInfoSnapshot> alreadyDelivered;
			deliveredWithinGivenRun.TryGetValue(deliveryInfo.DeliveryGroupID, out alreadyDelivered);

			return alreadyDelivered != null && alreadyDelivered.Contains(new DeliveryInfoSnapshot(deliveryInfo, deliveryContact));
		}

		#region DeliveryInfoSnapshot

		class DeliveryInfoSnapshot
		{
			public DeliveryInfoSnapshot(DeliveryInfo deliveryInfo, DocDeliveryContact deliveryContact)
			{
				if (deliveryInfo != null)
				{
					name = deliveryInfo.Name;
					attachedFilename = deliveryInfo.AttachedFilename;
					emailSubjectLine = deliveryInfo.EmailSubjectLine;
					emailSignature = deliveryInfo.EmailSignature;
					relatedBusinessContext = deliveryInfo.RelatedBusinessContext;
					showDraftWatermark = deliveryInfo.ShowDraftWatermark;
					deliveryGroupId = deliveryInfo.DeliveryGroupID;
					deliveryFormat = deliveryInfo.DeliveryFormat;
					businessObjectPk = deliveryInfo.BusinessObjectPk;
					parentGuid = deliveryInfo.ParentGuid;
					using (var sha1 = SHA1.Create())
					{
						long originalPosition = deliveryInfo.FileContents.Position;
						deliveryInfo.FileContents.Position = 0;
						fileHash = sha1.ComputeHash(deliveryInfo.FileContents);
						deliveryInfo.FileContents.Position = originalPosition;
					}
				}

				if (deliveryContact != null)
				{
					deliveryAddress = deliveryContact.DeliveryAddress;
					attachmentType = deliveryContact.AttachmentType;
				}
			}

			readonly ZString name;
			readonly ZString attachedFilename;
			readonly ZString emailSubjectLine;
			readonly ZString emailSignature;
			readonly ZString relatedBusinessContext;
			readonly bool showDraftWatermark;
			readonly ZGuid deliveryGroupId;
			readonly DeliveryFormats deliveryFormat;
			readonly ZGuid businessObjectPk;
			readonly ZGuid parentGuid;
			readonly ZString deliveryAddress;
			readonly ZString attachmentType;
			readonly byte[] fileHash;

			public override bool Equals(object obj)
			{
				if (obj is DeliveryInfoSnapshot other)
				{
					return name == other.name
						&& attachedFilename == other.attachedFilename
						&& emailSubjectLine == other.emailSubjectLine
						&& emailSignature == other.emailSignature
						&& relatedBusinessContext == other.relatedBusinessContext
						&& showDraftWatermark == other.showDraftWatermark
						&& deliveryGroupId == other.deliveryGroupId
						&& deliveryFormat == other.deliveryFormat
						&& (businessObjectPk.IsEmpty || businessObjectPk == other.businessObjectPk)
						&& parentGuid == other.parentGuid
						&& deliveryAddress == other.deliveryAddress
						&& attachmentType == other.attachmentType
						&& (fileHash == null || other.fileHash == null || fileHash.SequenceEqual(other.fileHash));
				}

				return base.Equals(obj);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
			public override int GetHashCode() => base.GetHashCode();
		}

		#endregion

		Dictionary<ZGuid, List<DeliveryInfoSnapshot>> deliveredWithinGivenRun = new Dictionary<ZGuid, List<DeliveryInfoSnapshot>>();

		/// <summary>
		/// Run print task using the given delivery instructions - does not show the instructions form.
		/// </summary>
		public virtual void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null) => Run(deliveryInstructions, GetDocumentPacks(), notifications);

		public void RunDocumentPack(DeliveryInstructions deliveryInstructions, DocumentPack pack)
		{
			if (!this.DocumentPacks.Contains(pack))
			{
				throw new ArgumentException("Document pack doesn't belong to the current printTask.");
			}
			Run(deliveryInstructions, new[] { pack });
		}

		List<IGrouping<string, (string Organisation, string ContactName)>> GetIncompleteRecipients(DeliveryInstructions docPackInstructions)
		{
			return docPackInstructions.Recipients.OfType<DocDeliveryContact>()
				.Where(r => !r.IsSystemDefaultContact && r.DeliveryMethod == Core.Constants.ContactNotifyModes.Email && string.IsNullOrEmpty(r.Email) && r.OrgHeader != null)
				.Select(r => (Organisation: r.OrgHeader.OH_Code.ToString(), ContactName: r.Name.ToString())).GroupBy(r => r.Organisation).ToList();
		}

		//DO NOT call .ToList() / .ToArray() / ... on documentPacks as it will force a full evaluation of the IEnumerable at once.
		//This is an IEnumerable for performance and memory useage reasons. When using stream mode (UseStreamMode), each pack is built only when it is iterated through / required, then processed, then disposed. This keeps the memory consumption constant.
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // I am not seeing any easy way to refactor my changes to get the complexity below 25.
		void Run(DeliveryInstructions deliveryInstructions, IEnumerable<DocumentPack> packs, INotifications notifications = null)
		{
			var reportsWithIncompleteRecipients = new List<(string ReportSubject, List<IGrouping<string, (string Organisation, string ContactName)>>)>();

			deliveredWithinGivenRun = new Dictionary<ZGuid, List<DeliveryInfoSnapshot>>();

			if (deliveryInstructions == null)
			{
				throw new ArgumentNullException(nameof(deliveryInstructions));
			}

			using (PrintTaskUIProvider.GetNewProgressNotificationUI(deliveryInstructions, Count))
			using (deliveryInstructions.ResetEDocsToBeDeliveredIfNeeded())
			{
				if (deliveryInstructions.Destination != DeliveryInstructionDestination.UserCancelled)
				{
					var processCompleted = false;
					var usesDeliveryGroup = deliveryInstructions.UsesDeliveryGroup;
					var isPreview = deliveryInstructions.Destination == DeliveryInstructionDestination.Preview;
					var originalInstructionsHasEmailSet = !string.IsNullOrEmpty(deliveryInstructions.Recipients.Cast<DocDeliveryContact>().FirstOrDefault()?.Email);
					var usageReporter = InitializeDocumentUsageReporter(isPreview);

					try
					{
						var e = new DocumentPrintedEventArgs(deliveryInstructions.Destination, ParentMenuCommand, deliveryInstructions.IsDraft, this);
						if (isPreview)
						{
							NotifyDocumentPrePreviewed(this, e);
						}
						else
						{
							NotifyDocumentPrePrinted(this, e);
						}

						deliveryInstructions.OnStartDocProcessing();

						var instructionsLanguage = deliveryInstructions.Language;
						var instructionsLanguageHasBeenSet = deliveryInstructions.LanguageHasBeenSet;
						var hasDocumentDeliveryDefaultLanguage = deliveryInstructions.HasDocumentDeliveryDefaultLanguage;
						var index = 0;

						foreach (var documentPack in packs)
						{
							documentPack?.ResetIsDraftIfNeeded(deliveryInstructions);
							deliveryInstructions.SetDocumentPackDeliverDocumentsInOneEmailInfo(documentPack);

							if (!deliveryInstructions.ShouldContinueIfDocumentsFileSizeGreaterThanSizeLimitInBytes())
							{
								break;
							}

							documentPack.SetSupportsLanguageSelectionForTranslatableLegacyDocument();
							List<IGrouping<string, (string Organisation, string ContactName)>> incompleteRecipients = null;

							if (documentPack == null)
							{
								throw new InvalidOperationException("You cannot call Run(DeliveryInstructions Instructions) twice in a row without setting up a new set of DocumentPacks.");
							}

							if (!isPreview && !originalInstructionsHasEmailSet)
							{
								incompleteRecipients = GetIncompleteRecipients(documentPack.DeliveryInstructions);
							}

							if (incompleteRecipients != null && incompleteRecipients.Any())
							{
								var report = documentPack.GetFirstReport();
								if (report != null)
								{
									report.SetDocumentName();
									report.SetEmailSubject();
									reportsWithIncompleteRecipients.Add((report.EmailSubject, incompleteRecipients));
								}

								if (documentPack.BusinessObjectForPrintJob != null)
								{
									invalidDocumentParentPKs.Add(documentPack.BusinessObjectForPrintJob.PK);
								}
							}
							else
							{
								if (instructionsLanguageHasBeenSet || hasDocumentDeliveryDefaultLanguage)
								{
									documentPack.Language = instructionsLanguage;
								}

								if (notifications != null)
								{
									notifications.Add(NotificationSubscriberType.Info, Res.GetString("31b54cbf-125d-4fb5-9eb0-5ac5b7f8fcf2", "Preferred language is {0}.", documentPack.Language));
								}

								deliveryInstructions.OnDocPackStarted(index);

								if (usesDeliveryGroup)
								{
									deliveryInstructions.SetAndSaveDeliveryGroupSubjectLine(documentPack, GenerateSubjectLineMappingForPrintTask(documentPack));
								}

								documentPack.OnAfterReportRun += new AfterReportRunEventHandler(OnAfterReportRun);

								try
								{
									documentPack.Run(deliveryInstructions, notifications);
#if DEBUG
									DocunmentRunCounter++;
#endif
								}
								finally
								{
									documentPack.OnAfterReportRun -= new AfterReportRunEventHandler(OnAfterReportRun);
								}

								deliveryInstructions.OnDocPackPrinted(index + 1);

								++index;
							}

							if (UseStreamMode)
							{
								documentPack.Dispose();
							}
						}

						if (usesDeliveryGroup)
						{
							MarkJobsAsProcessedAndDeleteUnusedGroups(deliveryInstructions);
						}
						processCompleted = true;

						usageReporter?.ReportForContacts();
					}
					catch
					{
						if (usesDeliveryGroup && !processCompleted)
						{
							PurgeUnProcessedPrintJobs(deliveryInstructions);
						}

						throw;
					}
					finally
					{
						deliveryInstructions.OnEndDocProcessing();

						RemoveDocumentUsageDetailsCollectorService();
					}
				}
			}

			if (reportsWithIncompleteRecipients.Count > 0)
			{
				var formattedRecipientDetails = FormatEmptyEmailRecipientsMessage(reportsWithIncompleteRecipients);

				var message = Res.GetString("acbefa1e-1d0d-4e76-af42-4d6f0293a52f", @"These documents could not be delivered as the following Contacts do not have an email address:

{0}

Please add an email address to these Contacts and deliver the documents again.", formattedRecipientDetails);

				Globals.Message.ShowWarning(message);
			}
		}

		DocumentUsageReporter InitializeDocumentUsageReporter(bool isPreview)
		{
			var usageReporter = new DocumentUsageReporter();
			var usageDetailsCollector = (ParentMenuCommand as BusinessObject)?.Factory?.ServiceContainer.AddService(new DocumentUsageDetailsCollector());
			if (usageDetailsCollector != null)
			{
				usageDetailsCollector.Reporter = usageReporter;
				usageDetailsCollector.GetMenuTitle(ParentMenuCommand?.SU_MenuName);
				usageDetailsCollector.GetIsPreview(isPreview);
				usageDetailsCollector.GetTriggeredFrom(this);
			}
			return usageReporter;
		}

		void RemoveDocumentUsageDetailsCollectorService()
		{
			(ParentMenuCommand as BusinessObject)?.Factory?.ServiceContainer.RemoveService<DocumentUsageDetailsCollector>();
		}

		string FormatEmptyEmailRecipientsMessage(List<(string ReportSubject, List<IGrouping<string, (string Organisation, string ContactName)>>)> recipients)
		{
			var result = new ZStringBuilder();

			recipients.ForEach(r =>
			{
				result.Append(r.ReportSubject);
				r.Item2.ForEach(i =>
				{
					result.Append(FormattableString.Invariant($"  Organisation: {i.Key}"));
					i.ToList().ForEach(j =>
					{
						result.Append(FormattableString.Invariant($"    Contact: {j.ContactName}"));
					});
				});
			});

			return result.ToStringWithNewLineBetweenAppends();
		}

#if DEBUG
		public
#endif
		List<ZGuid> invalidDocumentParentPKs = new List<ZGuid>();

		public bool IsDocValidForPrinting(ZGuid docPK)
		{
			return !invalidDocumentParentPKs.Contains(docPK);
		}

		void PurgeUnProcessedPrintJobs(DeliveryInstructions instructions)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "PrintTask_PurgeUnprocessedPrintJobs" };

			foreach (var deliveryGroup in instructions.DeliveryGroups)
			{
				if (!deliveryGroup.SB_IsProcessed)
				{
					factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, deliveryGroup.PK)).ForEach(job => job.Delete());
					factory.Load<StmDeliveryGroup>(deliveryGroup.PK)?.Delete();

					factory.Save();
				}
			}
		}

#if DEBUG
		internal static int DocunmentRunCounter { get; set; }
#endif

		void MarkJobsAsProcessedAndDeleteUnusedGroups(DeliveryInstructions instructions)
		{
			bool DoesNotContainJobsPredicate(StmDeliveryGroup group) => group.ContainedPrintJobs.Count == 0;
			instructions.DeliveryGroups.Where(g => !g.IsDeleted).ForEach(group => group.SB_IsProcessed = true);
			instructions.DeliveryGroups.Where(DoesNotContainJobsPredicate).DeleteAll();
			instructions.DeliveryGroups.RemoveAll(DoesNotContainJobsPredicate);

			instructions.FactorySaveStrategy.SaveChunk(instructions.Factory);
		}

		protected virtual ReportSubjectLineMapping GenerateSubjectLineMappingForPrintTask(DocumentPack docPack) => ReportSubjectLineMapping.EmptyMapping;

		public class ReportSubjectLineMapping
		{
			public ReportSubjectLineMapping(ReportWithDeliverables reportWithDeliverables, ZString subjectLine)
			{
				ReportWithDeliverables = reportWithDeliverables;
				SubjectLine = subjectLine;
			}

			public ReportWithDeliverables ReportWithDeliverables { get; private set; }
			public ZString SubjectLine { get; set; }

			public static ReportSubjectLineMapping EmptyMapping => new ReportSubjectLineMapping(null, string.Empty);

			public override bool Equals(object obj)
			{
				ReportSubjectLineMapping mapping = obj as ReportSubjectLineMapping;
				if (mapping == null)
				{
					return false;
				}

				return ReportWithDeliverables == mapping.ReportWithDeliverables && SubjectLine == mapping.SubjectLine;
			}

			public override int GetHashCode() => (ReportWithDeliverables == null ? 0 : ReportWithDeliverables.GetHashCode()) ^ SubjectLine.GetHashCode();
		}

		public class ReportWithDeliverables
		{
			public ReportWithDeliverables(Report report, IEnumerable<IDeliverable> deliverables)
			{
				Report = report;
				Deliverables = deliverables;
			}

			public Report Report { get; private set; }
			public IEnumerable<IDeliverable> Deliverables { get; private set; }
		}

		#endregion

		#region Report

		public static void RunSingleReport(Report report)
		{
			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack();
			pack.Add(report);
			task.Add(pack);
			task.Run(Env.Security.None);
		}

		public bool IsReportPrintSet { get; set; }

		bool ShowRuntimeOptionsForm(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions deliveryInstructions, ISecurityCheckpoint modifyDocumentCheckPoint) => PrintTaskUIProvider.ShowRuntimeOptionsUI(this, deliveryOptions, deliveryInstructions, modifyDocumentCheckPoint);

		public PrintTaskUIProviderTypes PrintTaskUIProviderType { get; set; }

		public IPrintTaskUIProvider PrintTaskUIProvider => printTaskUIProvider ?? (printTaskUIProvider = PrintTaskUIProviderFactory.CreateWithType(PrintTaskUIProviderType));

		IPrintTaskUIProvider printTaskUIProvider;

		#endregion

		#region Pre-Run Event

		/// <summary>
		/// This event is fired before the set is run if preview
		/// </summary>
		public event DocumentPrintedEventHandler DocumentPrePreviewed;

		void NotifyDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e) => DocumentPrePreviewed?.Invoke(sender, e);

		/// <summary>
		/// This event is fired before the set is run if not preview
		/// </summary>
		public event DocumentPrintedEventHandler DocumentPrePrinted;

		void NotifyDocumentPrePrinted(object sender, DocumentPrintedEventArgs e) => DocumentPrePrinted?.Invoke(sender, e);

		#endregion

		#region Post-Run Event

		/// <summary>
		/// This event is fired after each report is run, be mindful of the fact that report object passed to your callback is disposed immediately after this event is fired.
		/// </summary>
		public event AfterReportRunEventHandler AfterReportRun;

		public delegate void AfterReportRunEventHandler(object sender, IDeliverable itemToDeliver);

		void OnAfterReportRun(object sender, IDeliverable itemToDeliver) => AfterReportRun?.Invoke(sender, itemToDeliver);

		#endregion

		#endregion

		#region Schedule

		public void Schedule()
		{
			if (Count == 1)
			{
				DocumentPack pack = this[0];
				if (pack.StmMenuCommand != null)
				{
					IScheduledReportsController controller = (IScheduledReportsController)ObjectFactory.Get<IControllerFactory>().Create(ControllerIDs.ScheduledReports);
					controller.ShowNewForm(pack.StmMenuCommand);
#if DEBUG
					lastScheduleController = controller;
#endif
				}
			}
		}

#if DEBUG
		public IScheduledReportsController LastScheduleController => lastScheduleController;

		IScheduledReportsController lastScheduleController;
#endif
		#endregion

		#region Preview

		public const int MaxPreviewCount = 50;

		protected bool IsPreviewAllowed => !UseStreamMode && Count <= MaxPreviewCount;

		public delegate void PreviewRequestedEventHandler(IDeliverCapableForm sender, DeliveryInstructions instructions);

		public void Form_PreviewRequested(IDeliverCapableForm sender, DeliveryInstructions deliveryInstructions) => Preview(sender, deliveryInstructions);

		public void Preview(IDeliverCapableForm parentForm, DeliveryInstructions deliveryInstructions)
		{
			deliveryInstructions.ParentForm = parentForm;
			Preview(deliveryInstructions);
		}

		public void Preview(DeliveryInstructions instructions)
		{
			DeliveryInstructionDestination previousDestination = instructions.Destination;
			try
			{
				using (Report.TemporarilySetReportRunSource(Report.ReportRunSource.Preview))
				{
					instructions.Destination = DeliveryInstructionDestination.Preview;
					Run(instructions);
				}
			}
			finally
			{
				instructions.Destination = previousDestination;
			}
		}

		public void Preview(PrintTaskSettings taskSettings)
		{
			DeliveryInstructionDestination[] previousDestinations = new DeliveryInstructionDestination[taskSettings.DocPacksDeliveryInstructions.Count];

			for (int i = 0; i < taskSettings.DocPacksDeliveryInstructions.Count; i++)
			{
				previousDestinations[i] = taskSettings.DocPacksDeliveryInstructions[i].Destination;
				taskSettings.DocPacksDeliveryInstructions[i].Destination = DeliveryInstructionDestination.Preview;
			}
			try
			{
				using (Report.TemporarilySetReportRunSource(Report.ReportRunSource.Preview))
				{
					Run(taskSettings);
				}
			}
			finally
			{
				for (int i = 0; i < taskSettings.DocPacksDeliveryInstructions.Count; i++)
				{
					taskSettings.DocPacksDeliveryInstructions[i].Destination = previousDestinations[i];
				}
			}
		}

		public void RecordReportStatisticsForPreview(Report report, StmReportRun stmReportRun, ReportStatistics previewReportStatistics)
		{
			if (stmReportRun != null)
			{
				stmReportRun.RRI_Status = Enterprise.Core.Constants.StmReportRunState.Finished;

				if (report != null)
				{
					var endCPU = Process.GetCurrentProcess().TotalProcessorTime;
					var end = ZDateTime.UtcNow;
					stmReportRun.RRI_SQLRunDurationMilliseconds = Math.Max(0, (int)(report.SqlElapsedTime * 1000));
					stmReportRun.RRI_TotalRunDurationMilliseconds = Math.Max(0, (int)Math.Floor((end - previewReportStatistics.Start).TotalMilliseconds));
					stmReportRun.RRI_ClientCPUDurationMilliseconds = Math.Max(0, (int)Math.Floor((endCPU - previewReportStatistics.StartCPU).TotalMilliseconds));
					stmReportRun.RRI_SQLCPUDurationMilliseconds = Math.Max(0, previewReportStatistics.EndSQLCPU - previewReportStatistics.StartSQLCPU);
					stmReportRun.RRI_ClientRunDurationMilliseconds = Math.Max(0, stmReportRun.RRI_TotalRunDurationMilliseconds - stmReportRun.RRI_SQLRunDurationMilliseconds);
				}
				stmReportRun.Factory.Save();
			}
		}

		public void PrepareReportStatisticsForPreview(Report report, StmReportRun stmReportRun, ReportStatistics reportStatistics)
		{
			var isRenderAndSaved = false;
			var dbServerInfo = new StringBuilder();
			if (stmReportRun != null && report != null)
			{
				report.stmReportRun = stmReportRun;
				report.OnRenderAndSave += (sender, runningConnection) =>
				{
					if (!isRenderAndSaved)
					{
						reportStatistics.StartSQLCPU = StmReportHelper.SQLCPUTime(runningConnection);
					}
				};
				report.AfterRenderAndSave += (sender, runningConnection) =>
				{
					if (!isRenderAndSaved)
					{
						reportStatistics.EndSQLCPU = StmReportHelper.SQLCPUTime(runningConnection);
						isRenderAndSaved = true;
					}
				};
				report.AddDBServerInfo += (sender, message) =>
				{
					dbServerInfo.AppendLine(message);
				};
				reportStatistics.Start = ZDateTime.UtcNow;
				reportStatistics.StartCPU = Process.GetCurrentProcess().TotalProcessorTime;
			}
		}

		#endregion

		#region Delivery Instructions Form

		public delegate void DeliveryRequestedEventHandler(IDeliverCapableForm sender, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint);

		public void Form_DeliveryRequested(object sender, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions specifiedInstructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			ShowDeliveryDialogForm(deliveryOptions, specifiedInstructions, modifyDocumentCheckPoint);

			if (specifiedInstructions != null && specifiedInstructions.DocPack != null)
			{
				specifiedInstructions.DocPack.Dispose();
			}
		}

		protected virtual void SetNumberOfCopies(DeliveryInstructions instructions)
		{
			if (ParentMenuCommand != null)
			{
				instructions.PrinterDelivery.NumberOfCopies = ParentMenuCommand.NumberOfCopies != 0 ? ParentMenuCommand.NumberOfCopies : (ZShort)1;
				instructions.PrinterDelivery.NumberOfCopies_ReadOnly = !ParentMenuCommand.AllowMultipleCopies;
			}
		}

		protected virtual void SetDraftOptions(DeliveryInstructions instructions)
		{
			if (ParentMenuCommand != null)
			{
				if (ParentMenuCommand.SU_DraftOption != DraftOptionsList.Codes.Both)
				{
					instructions.IsDraft_ReadOnly = true;
					instructions.IsDraft = ParentMenuCommand.SU_DraftOption == DraftOptionsList.Codes.Draft;
				}
			}
			else
			{
				instructions.IsDraft_ReadOnly = true;
			}
		}

		protected DeliveryInstructionDestination ShowDeliveryDialogForm(AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			instructions.DeliveryOptions = deliveryOptions;
			SetDeliveryInstructionDefaults(instructions);
			if (deliveryOptions == AllowedDeliveryOptions.PreviewOnly)
			{
				instructions.Destination = DeliveryInstructionDestination.Preview;
			}
			else if (DocumentRunner.IsRunningBackgroundDelivery)
			{
				BackgroundDeliveryHelper.PrepareDeliveryInstructions(DocumentRunner.DocumentDelivery, instructions, ParentMenuCommand as DocumentCommand);
			}
			else
			{
				ShowDeliveryInstructionsForm(instructions, modifyDocumentCheckPoint);
			}

			if (instructions.Destination == DeliveryInstructionDestination.Print
				&& instructions.PrinterDelivery.PrintQueue == null)
			{
				instructions.Destination = DeliveryInstructionDestination.UserCancelled;
			}

			if (instructions.BackgroundDelivery && instructions.Destination != DeliveryInstructionDestination.UserCancelled)
			{
				CreateStmDocumentDelivery(instructions);
				instructions.Destination = DeliveryInstructionDestination.UserCancelled;
			}
			else
			{
				Run(instructions);
			}
			if (instructions.AllowSaveDefaults && instructions.Destination != DeliveryInstructionDestination.UserCancelled)
			{
				SavePrinterDeliveryDefaults(instructions);
			}

			return instructions.Destination;
		}

		internal void CreateStmDocumentDelivery(DeliveryInstructions instructions)
		{
			var delivery = Factory.New<StmDocumentDelivery>();
			delivery.SDL_IsProcessed = false;
			var menu = ParentMenuCommand as DocumentCommand;
			delivery.SDL_SU = ParentMenuCommand?.PK ?? ZGuid.Empty;
			var bizObj = menu?.Parent as BusinessObject ?? instructions.BusinessObjectForPrintJob;
			delivery.SDL_ParentId = bizObj?.PK ?? ZGuid.Empty;
			delivery.SDL_ParentControllerIdOrTableCode = menu?.ControllerId != null ? menu.ControllerId.ToString() : bizObj?.TablePrefix ?? "";
			delivery.SDL_RetryAttempts = 0;
			delivery.SDL_GS = GlbStaff.CurrentUser.PK;
			delivery.SDL_GB = GlbBranch.CurrentBranch.PK;
			delivery.SDL_GE = GlbDepartment.CurrentDepartment.PK;

			var contextInfomation = new SerializableDeliveryInstructions();
			contextInfomation.SetDeliveryInstructionsContextInformation(instructions);
			delivery.SDL_Instructions = JsonConvert.SerializeObject(contextInfomation, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });

			Factory.Save();
		}

		protected bool PermissionForAutoDelivery
		{
			get
			{
				bool allowed = !(ParentMenuCommand != null && (ParentMenuCommand.SU_PreventAutoDelivery || ParentMenuCommand.SU_ContactType == ContactType.NoContactType.Code));
				if (allowed && !UseStreamMode)
				{
					allowed = GetDocumentPacks().All(pack => pack.DocumentSupporter != null);
				}
				return allowed;
			}
		}

		protected virtual void ShowDeliveryInstructionsForm(DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			if (DeliveryInstructionsDefaultPK.IsValid)
			{
				instructions.AllowSaveDefaults = true;
				LoadPrinterDeliveryDefaults(instructions);
			}
			else
			{
				instructions.AllowSaveDefaults = false;
			}

			if (instructions.DeliveryOptions == AllowedDeliveryOptions.HardCopyOnly)
			{
				ShowHardCopyOnlyForm(instructions);
			}
			else
			{
				CheckAvailableDeliveryOptions(instructions);
				ShowAllModesForm(instructions, modifyDocumentCheckPoint);
			}
		}

		public void ShowOverridePrintDetailsDeliveryForm(DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			if (instructions.DeliveryOptions == AllowedDeliveryOptions.OverridePrintDetails)
			{
				if (!PrintTaskUIProvider.ShowDocDeliveryUI(this, instructions, modifyDocumentCheckPoint))
				{
					instructions.Destination = DeliveryInstructionDestination.UserCancelled;
				}
			}
		}

		void ShowAllModesForm(DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
		{
			instructions.DocumentPackCount = Count;
			instructions.AllowPreview = IsPreviewAllowed;

			if (!UseStreamMode)
			{
				DocumentPack firstPack = null;
				HashSet<string> packForcedLanguages = new();
				foreach (var documentPack in DocumentPacks)
				{
					documentPack.SetSupportsLanguageSelectionForTranslatableLegacyDocument();
					firstPack ??= documentPack;
					packForcedLanguages.Add(documentPack.ForcedLanguage);
				}
				SetInstructionsLanguage(packForcedLanguages, instructions, firstPack);
			}

			if (!PrintTaskUIProvider.ShowDocDeliveryUI(this, instructions, modifyDocumentCheckPoint))
			{
				instructions.Destination = DeliveryInstructionDestination.UserCancelled;
			}

			if (instructions.Destination == DeliveryInstructionDestination.Auto)
			{
				SelectPrinterForAutoDeliveryIfRequired(instructions);
			}
		}

		void SetInstructionsLanguage(HashSet<string> forcedLanguages, DeliveryInstructions instructions, DocumentPack firstPack)
		{
			if (forcedLanguages.Count == 1)
			{
				var forcedLanguage = forcedLanguages.First();
				if (!forcedLanguage.IsNullOrEmpty())
				{
					instructions.Language = forcedLanguage;
					instructions.Language_ReadOnly = true;
					return;
				}
			}
			if (firstPack != null)
			{
				instructions.Language = firstPack.Language;
			}
		}

		protected virtual void CheckAvailableDeliveryOptions(DeliveryInstructions instructions)
		{
		}

		protected virtual void ShowHardCopyOnlyForm(PrintTaskSettings taskSettings)
		{
			taskSettings.Destination = DeliveryInstructionDestination.Print;
			if (taskSettings.PrinterDelivery.PrintQueue == null)
			{
				DeliveryInstructions instructions = taskSettings.SingleDocPackInstructions;
				if (instructions == null)
				{
					instructions.Destination = taskSettings.Destination;
					instructions.PrinterDelivery = taskSettings.PrinterDelivery;
				}
				ShowPrinterSelectionForm(instructions);
				if (instructions != taskSettings.SingleDocPackInstructions)
				{
					taskSettings.PrinterDelivery = instructions.PrinterDelivery;
				}
			}
		}

		protected void ShowHardCopyOnlyForm(DeliveryInstructions instructions)
		{
			instructions.Destination = DeliveryInstructionDestination.Print;
			if (instructions.PrinterDelivery.PrintQueue == null)
			{
				ShowPrinterSelectionForm(instructions);
			}
		}

		protected void ShowPrinterSelectionForm(DeliveryInstructions deliveryInstructions) => deliveryInstructions.ShowPrinterSelectionUI();

		void SetDeliveryInstructionDefaults(DeliveryInstructions instructions)
		{
			instructions.AllowAutoDelivery = PermissionForAutoDelivery;
			SetNumberOfCopies(instructions);
			SetDraftOptions(instructions);
		}

		void SetDeliveryInstructionDefaults()
		{
			foreach (DeliveryInstructions instructions in TaskSettings.DocPacksDeliveryInstructions)
			{
				SetDeliveryInstructionDefaults(instructions);
			}
		}

		#endregion

		#region Printer Form

		void SelectPrinterForAutoDeliveryIfRequired(DeliveryInstructions instructions)
		{
			if (instructions.PrinterDelivery.PrintQueue == null && NeedPrinterForAutoDelivery)
			{
				SelectPrinter(instructions);
			}
		}

		protected bool NeedPrinterForAutoDelivery
		{
			get
			{
				if (UseStreamMode)
				{
					return true;
				}
				else
				{
					foreach (DocumentPack pack in GetDocumentPacks())
					{
						if (pack.Organisation != null)
						{
							DocDeliveryContactCollection contacts = pack.AutoDocumentDelivery.GetDeliveryContacts(pack.StmMenuCommand, pack.DocumentSupporter);
							foreach (DocDeliveryContact contact in contacts)
							{
								if (contact.DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
								{
									return true;
								}
							}
						}
					}
					return false;
				}
			}
		}

		protected void SelectPrinter(DeliveryInstructions instructions)
		{
			ShowPrinterSelectionForm(instructions);
			if (instructions.PrinterDelivery.PrintQueue == null)
			{
				Globals.Message.ShowWarning(Res.GetString("d8f6d092-404e-4b4a-b1e5-4603c148b439", "The delivery of these documents was canceled because a printer was not selected."), Res.GetString("b0145ee8-41ac-49d0-a2a3-194e73446ac2", "Auto-Delivery Canceled"));
				instructions.Destination = DeliveryInstructionDestination.UserCancelled;
			}
		}

		#endregion

		#region Default Printer Settings

		/// <summary>
		/// This is used to store Default Delivery Instructions.
		/// If your document does not have a menu item, you can set this to a constant so that
		/// the system will have a "key" to remember the user's print settings for this document.
		/// </summary>
		public ZGuid DeliveryInstructionsDefaultPK
		{
			get
			{
				if (ManuallySpecifiedDeliveryInstructionsDefaultPK.IsValid)
				{
					return ManuallySpecifiedDeliveryInstructionsDefaultPK;
				}
				else
				{
					return ParentMenuCommand != null ? ParentMenuCommand.PK : ZGuid.Missing;
				}
			}
			set { ManuallySpecifiedDeliveryInstructionsDefaultPK = value; }
		}

		ZGuid ManuallySpecifiedDeliveryInstructionsDefaultPK;

		internal void SavePrinterDeliveryDefaults(PrintTaskSettings taskSettings)
		{
			if (taskSettings != null && taskSettings.PrinterDelivery != null)
			{
				SavePrinterDeliveryDefaults(taskSettings.PrinterDelivery);
			}
		}

		public void SavePrinterDeliveryDefaults(DeliveryInstructions instructions)
		{
			if (instructions != null && instructions.PrinterDelivery != null)
			{
				SavePrinterDeliveryDefaults(instructions.PrinterDelivery, instructions);
			}
		}

		void SavePrinterDeliveryDefaults(DocDeliveryPrintDetails printerDelivery, DeliveryInstructions instructions = null)
		{
			if (!DefaultSettingKey.IsEmpty)
			{
				var needSave = false;
				var newFactory = new BusinessObjectFactory() { NameForDebugging = "PrintTask.SavePrinterDeliveryDefaults" };
				if (printerDelivery.PrintQueuePK.IsValid)
				{
					var defaultPrinter = LoadPrinter(newFactory, DefaultSettingKey, ZGuid.Empty, true);

					defaultPrinter.SDP_SQ_Printer = printerDelivery.PrintQueuePK;
					defaultPrinter.SDP_NumberOfCopies = (printerDelivery.NumberOfCopies <= byte.MaxValue) ? (byte)printerDelivery.NumberOfCopies : byte.MaxValue;
					needSave = true;
				}

				if (instructions != null)
				{
					foreach (var document in instructions.DocumentsToBeDelivered.OfType<IDeliverable>().Where(iDeliverable => !iDeliverable.MenuTemplatePivotPK.IsEmpty))
					{
						var defaultPrinter = LoadPrinter(newFactory, DefaultSettingKey, document.MenuTemplatePivotPK, true);

						if (document.PrinterDetails.PrintQueuePK.IsValid)
						{
							defaultPrinter.SDP_SQ_Printer = document.PrinterDetails.PrintQueuePK;
							if (!document.PrinterDetails.NumberOfCopies_ReadOnly)
							{
								var copies = document.PrinterDetails.NumberOfCopies;
								defaultPrinter.SDP_NumberOfCopies = (copies <= byte.MaxValue) ? (byte)copies : byte.MaxValue;
							}
						}
						else
						{
							defaultPrinter.Delete();
						}
						needSave = true;
					}
				}
				if (needSave)
				{
					newFactory.Save();
				}
			}
		}

		StmDefaultPrinter LoadPrinter(BusinessObjectFactory newFactory, ZGuid menuItemPK, ZGuid stmMenuTemplatePivotPK, bool shouldCreateIfNotFound)
		{
			var stmMenuItem = Factory.Load<IStmMenuItem>(menuItemPK);
			var templatePivot = Factory.Load<IStmMenuTemplatePivot>(stmMenuTemplatePivotPK);

			if (stmMenuItem == null && templatePivot != null)
			{
				stmMenuItem = Factory.Load<IStmMenuItem>(templatePivot.SI_SU);
			}

			StmDefaultPrinter defaultPrinter;
			if (shouldCreateIfNotFound)
			{
				defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(newFactory, GlbStaff.CurrentUser, stmMenuItem, false, templatePivot);
			}
			else
			{
				defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(newFactory, GlbStaff.CurrentUser, stmMenuItem, false, templatePivot);
			}

			return defaultPrinter;
		}

		ZGuid DefaultSettingKey => DeliveryInstructionsDefaultPK.IsValid ? DeliveryInstructionsDefaultPK.ToGuid() : ZGuid.Empty;

		internal void LoadPrinterDeliveryDefaults(PrintTaskSettings taskSettings)
		{
			if (!DefaultSettingKey.IsEmpty)
			{
				var newFactory = new BusinessObjectFactory() { NameForDebugging = "PrintTask.LoadPrinterDeliveryDefaults - PrintTaskSettings" };
				var defaultPrinter = LoadPrinter(newFactory, DefaultSettingKey, ZGuid.Empty, false);
				if (defaultPrinter != null)
				{
					taskSettings.PrinterDelivery.NumberOfCopies = defaultPrinter.SDP_NumberOfCopies;

					StmPrintQueue printQueue = (StmPrintQueue)Factory.Load(typeof(StmPrintQueue), defaultPrinter.SDP_SQ_Printer);
					if (printQueue != null &&
						printQueue.SQ_AllowPrinting &&
						printQueue.SQ_QueueDeleted.IsEmpty &&
						printQueue.IsPrintAllowed)
					{
						taskSettings.PrinterDelivery.PrintQueuePK = defaultPrinter.SDP_SQ_Printer;
					}
				}
			}
		}

		internal void LoadPrinterDeliveryDefaults(DeliveryInstructions instructions)
		{
			if (!DefaultSettingKey.IsEmpty)
			{
				var newFactory = new BusinessObjectFactory() { NameForDebugging = "PrintTask.LoadPrinterDeliveryDefaults - DeliveryInstructions" };

				var defaultPrinter = LoadPrinter(newFactory, DefaultSettingKey, ZGuid.Empty, false);
				if (defaultPrinter != null)
				{
					instructions.PrinterDelivery.NumberOfCopies = defaultPrinter.SDP_NumberOfCopies;
					var printQueue = Factory.Load<StmPrintQueue>(defaultPrinter.SDP_SQ_Printer);
					if (printQueue != null
						&& printQueue.SQ_AllowPrinting
						&& printQueue.SQ_QueueDeleted.IsEmpty
						&& printQueue.IsPrintAllowed)
					{
						instructions.PrinterDelivery.PrintQueuePK = defaultPrinter.SDP_SQ_Printer;
					}
				}

				foreach (var document in instructions.DocumentsToBeDelivered.OfType<IDeliverable>().Where(iDeliverable => !iDeliverable.MenuTemplatePivotPK.IsEmpty))
				{
					var defaultPrinterForPivot = LoadPrinter(newFactory, DefaultSettingKey, document.MenuTemplatePivotPK, false);
					if (defaultPrinterForPivot != null)
					{
						if (!document.PrinterDetails.NumberOfCopies_ReadOnly)
						{
							document.PrinterDetails.NumberOfCopies = defaultPrinterForPivot.SDP_NumberOfCopies;
						}
						var printQueue = Factory.Load<StmPrintQueue>(defaultPrinterForPivot.SDP_SQ_Printer);
						if (printQueue != null
							&& printQueue.SQ_AllowPrinting
							&& printQueue.SQ_QueueDeleted.IsEmpty
							&& printQueue.IsPrintAllowed)
						{
							document.PrinterDetails.PrintQueuePK = defaultPrinterForPivot.SDP_SQ_Printer;
						}
					}
				}
			}
		}

		public BusinessObjectFactory Factory = new BusinessObjectFactory() { NameForDebugging = "PrintTask.Ctor" };

		#endregion

		#region Dispose

		bool isDisposed;
		public bool IsDisposed => isDisposed;

		public void Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			foreach (var documentPack in DocumentPacks)
			{
				if (documentPack != null)
				{
					documentPack.Dispose();
				}
			}

			isDisposed = true;
		}

		#endregion

		#region Wrapped List Methods and Properties
		public virtual void Add(DocumentPack item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("DocumentPack item");
			}
			item.Parent = this;
			(DocumentPacks as List<DocumentPack>).Add(item);
		}

		/// <summary>
		/// Adds a Collection of Document Packs to the Print Task. Can't contain null Document Packs.
		/// </summary>
		/// <param name="collection"></param>
		public virtual void AddRange(IEnumerable<DocumentPack> collection)
		{
			if (new List<DocumentPack>(collection).TrueForAll(pack => pack != null))
			{
				(DocumentPacks as List<DocumentPack>).AddRange(collection);
			}
			else
			{
				throw new ArgumentNullException("IEnumerable<DocumentPack> collection. Collection can't contain null items.");
			}
		}

		public virtual void Remove(DocumentPack item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("DocumentPack item");
			}
			(DocumentPacks as List<DocumentPack>).Remove(item);
		}

		public virtual void Clear() => documentPacks = new List<DocumentPack>();

		public virtual int Count => DocumentPacks.Count();

		#endregion
	}
}
