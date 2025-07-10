using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine
{
	public class DocumentPack : ReportCollection, IDisposable
	{
		#region Construction

		public DocumentPack()
		{
		}

		public DocumentPack(StmMenuItem menuItem)
			: base(menuItem != null ? menuItem.Factory : null)
		{
			this.StmMenuCommand = menuItem;

			if (menuItem != null)
			{
				this.DocumentGroup = menuItem.SU_ContactType;

				var contextManager = menuItem.Factory.GetDocWrapperContextManager();
				contextManager.SetupDocWrapperContextFromDocumentPack(menuItem.SU_DocumentDirection
					, menuItem.PK
					, menuItem.SU_MenuName
					, menuItem.SU_ContactType
					, BrandedOrganisation
					, Organisation);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public DocumentPack(ReportCommand reportCommand)
			: this((StmMenuItem)reportCommand)
		{
			SupportsLanguageSelection |= reportCommand.Documents.Cast<StmMenuTemplatePivotBase>().Any(pivot => pivot.Template != null && pivot.Template.IsDocBuilderStyle);

			foreach (StmMenuTemplatePivotBase pivot1 in reportCommand.Documents)
			{
				var template = pivot1.Template;
				var exlTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
				Report newReport = null;
				const string restriction = @"Requires data-warehouse(Edw database) instance";
				string templateRestriction = template.SO_TemplateRestriction;

				if (templateRestriction.Equals(restriction, StringComparison.OrdinalIgnoreCase))
				{
					newReport = new ReportEdw(this, exlTemplate, pivot1.SI_DocumentTitle, ContactType.Find(reportCommand.SU_ContactType), pivot1.SI_IsPasswordProtected, pivot1.SI_IsPasswordProtectedForOpening);
				}
				else
				{
					newReport = new Report(this, exlTemplate, pivot1.SI_DocumentTitle, ContactType.Find(reportCommand.SU_ContactType), pivot1.SI_IsPasswordProtected, pivot1.SI_IsPasswordProtectedForOpening);
				}
				newReport.StTemplate = template;
				AddAndSetMenuItem(newReport, reportCommand);
			}
		}

		public DocumentPack(DocumentCommand documentCommand, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, DocumentCommand parentCommand, bool shouldAutoAddeDocs = true, Guid[] documentSuppressionList = null, string language = "", DocManagerInfo bizoToForcedToBeParentOfEDoc = null)
			: this(documentCommand)
		{
			this.BizObject = documentSupportable;
			this.UserFieldList = userFieldList;
			this.ParentCommand = parentCommand;
			this.shouldAutoAddeDocs = shouldAutoAddeDocs;
			this.documentSuppressionList = documentSuppressionList;
			this.language = language;

			ContactType = ContactType.Find(documentCommand.SU_ContactType);
			DocumentDirection = documentCommand.DocumentDirection;

			if (documentSupportable != null)
			{
				DocumentSupporter = documentSupportable.DocumentSupporter;
			}

			AddReportsToPack(documentCommand, parentCommand, documentSupportable, userFieldList);
			this.bizoForcedToBeEdocsParent = bizoToForcedToBeParentOfEDoc;
		}
		readonly DocManagerInfo bizoForcedToBeEdocsParent;
		readonly bool shouldAutoAddeDocs;
		readonly Guid[] documentSuppressionList;

		internal PrintTaskDocumentPackLoader Loader { get; set; }
		#endregion

		#region SuspendAddEDoc
		[ThreadStatic]
		static Overridable<int> autoAddeDocsSuspendedCount;

		public static Overridable<int> AutoAddeDocsSuspendedCount => autoAddeDocsSuspendedCount ?? (autoAddeDocsSuspendedCount = new Overridable<int>());
		#endregion

		#region Parent PrintTask

		public PrintTask Parent { get; set; }

		#endregion

		public Report GetFirstReport()
		{
			return this.OfType<Report>().FirstOrDefault();
		}

		public int GetRowCountForBusinessObjectDataSource()
		{
			var reports = this.OfType<Report>();
			var rowCount = 0;
			foreach (var report in reports)
			{
				rowCount += report.GetRowCountForBusinessObjectDataSource();
			}
			return rowCount;
		}

		public Report GetFirstNonCoverSheetReport()
		{
			return this.OfType<Report>().FirstOrDefault(r => !r.IsCoverSheet && r.IncludedInPrint);
		}

		public void DeserializeDocPackFromReportCollection(Report deserializedReport, INotifications notifications)
		{
			foreach (Report currentReport in this)
			{
				currentReport.DeserializedReport = deserializedReport;
				currentReport.ScheduleTaskNotifications = notifications;
				if (deserializedReport.ScheduleTask != null)
				{
					currentReport.SetScheduleTask(deserializedReport.ScheduleTask);
				}
			}
		}

		internal List<string> ReasonsForEmptyPacks
		{
			get { return reasonsForEmptyPacks ?? (reasonsForEmptyPacks = new List<string>()); }
		}
		List<string> reasonsForEmptyPacks;

		public static DocumentPack EmptyPack
		{
			get { return emptyPack ?? (emptyPack = new DocumentPack()); }
		}
		[ThreadStatic]
		static DocumentPack emptyPack;

		#region Delvier Documents in One Email

		internal bool DeliverDocumentsInOneEmail { get; set; }

		internal bool IsSingleAttachment { get; set; }

		#endregion

		#region Instructions

		public DeliveryInstructions DeliveryInstructions
		{
			get
			{
				if (deliveryInstructions == null)
				{
					deliveryInstructions = new DeliveryInstructions(this);
				}
				return deliveryInstructions;
			}
		}

		DeliveryInstructions deliveryInstructions;

		public void SetDeliveryDetailsFromDocumentPrintSet(DocumentCommand command)
		{
			ContactType = ContactType.Find(command.SU_ContactType);
			DocumentGroup = command.SU_ContactType;
			DocumentSupporter = command.Parent.DocumentSupporter;
		}

		#endregion

		#region Recipients

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter; }
			set
			{
				documentSupporter = value;
				ZString menuName = (StmMenuCommand != null) ? StmMenuCommand.SU_MenuName : ZString.Empty;
				OrgHeaderContact = (OrgHeaderContact)DocumentSupporter.GetContactOrganisation(menuName, ContactType, DocumentDirection);
			}
		}

		DocumentSupporter documentSupporter;

		protected ContactType ContactType;

		public bool IsRunFromMenusCustomisationForm
		{
			get { return isRunFromMenusCustomisationForm; }
			set { isRunFromMenusCustomisationForm = value; }
		}

		bool isRunFromMenusCustomisationForm;

		public OrgHeaderContact OrgHeaderContact { get; set; }

		public OrgHeader Organisation
		{
			get { return OrgHeaderContact == null ? null : OrgHeaderContact.OrgHeader; }
			set { OrgHeaderContact = new OrgHeaderContact(value, null); }
		}

		public OrgContact Contact
		{
			get { return OrgHeaderContact == null ? null : OrgHeaderContact.OrgContact; }
			set { OrgHeaderContact = new OrgHeaderContact(value); }
		}

		public IOrgHeader BrandedOrganisation
		{
			get { return DocumentSupporter != null ? DocumentSupporter.GetBrandedOrganisation(ContactType, DocumentDirection) : null; }
		}

		public StmMenuItem StmMenuCommand { get; private set; }

		public ZString DocumentGroup { get; private set; }

		public string TransportMode
		{
			get { return fTransportMode; }
			set { fTransportMode = value; }
		}

		string fTransportMode = "";

		readonly DocumentDirection DocumentDirection;

		#endregion

		#region Delivery

		internal protected virtual void Run(DeliveryInstructions instructions, INotifications notifications = null)
		{
			CultureInfo originalCulture = Culture.Current;
			try
			{
				if (StmMenuCommand != null)
				{
					Culture.Set(StmMenuCommand.RenderCulture);
				}

				instructions.InstructionsHelper.AddExtraInfoOnDeliveryInstructions();

				if (instructions.Recipients != null && instructions.Recipients.Count > 0)
				{
					RunForRecipients(instructions, notifications);
				}
				else if (Organisation != null)
				{
					RunForDeliveryOfMultiDocPack(instructions, notifications);
				}
				else if (Contact != null)
				{
					RunForContact(Contact, instructions, notifications);
				}
				else
				{
					RunForContact((DocDeliveryContact)null, instructions, notifications);
				}
			}
			finally
			{
				Culture.Set(originalCulture);
			}
		}

		#region Recipient Delivery

#if DEBUG
		internal
#endif
		void RunForRecipients(DeliveryInstructions instructions, INotifications notifications = null)
		{
			using (ReportDataProvider.EnableReportSqlResultCache(StmMenuCommand is ReportCommand))
			{
				ZString lastAttachmentType = ZString.Empty;
				foreach (DocDeliveryContact recipient in instructions.Recipients.ToArray()) //Use ToArray() to prevent changing collection during iteration.
				{
					if (!string.IsNullOrEmpty(lastAttachmentType) && lastAttachmentType != recipient.AttachmentType)
					{
						foreach (var deliverable in this.OfType<Report>())
						{
							deliverable.ResetCachedExcelFile();
						}
					}
					lastAttachmentType = recipient.AttachmentType;
					RunForContact(recipient, instructions, notifications);
				}
			}
		}

		#endregion

		#region Multiple Document Pack Print and AutoDelivery

		void RunForDeliveryOfMultiDocPack(DeliveryInstructions instructions, INotifications notifications = null)
		{
			if (instructions.Destination == DeliveryInstructionDestination.Auto)
			{
				RunForAutoDeliveryOfMultiDocPack(instructions, notifications);
			}
			else if (instructions.Destination == DeliveryInstructionDestination.Preview)
			{
				RunForPreviewOfMultiDocPack(instructions, notifications);
			}
			else
			{
				RunForPrintOfMultiDocPack(instructions, notifications);
			}
		}

		void RunForPrintOfMultiDocPack(DeliveryInstructions instructions, INotifications notifications = null)
		{
			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(DocumentGroup, TransportMode);
			RunForContact(defaultContact, instructions, notifications);
		}

		void RunForPreviewOfMultiDocPack(DeliveryInstructions instructions, INotifications notifications = null)
		{
			var deliveryContacts = GetDeliveryContacts(instructions);

			if (deliveryContacts.Count == 0)
			{
				RunForPrintOfMultiDocPack(instructions, notifications);
			}
			else
			{
				deliveryContacts.Cast<DocDeliveryContact>().ForEach(contact => RunForContact(contact, instructions, notifications));
			}
		}

		void RunForAutoDeliveryOfMultiDocPack(DeliveryInstructions instructions, INotifications notifications = null)
		{
			DocDeliveryContactCollection deliveryContacts = GetDeliveryContacts(instructions);

			if (deliveryContacts.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("ee422abe-d965-41ca-99d3-e1f22f275a28", "There are no contacts found that can be used for Delivery for '{0}'", Organisation.OH_FullName), Res.GetString("c4775e8d-9d81-4d99-b195-3482f3596b54", "Delivery Failed"));
			}
			else
			{
				bool firstPrintJobEncountered = false;
				for (int i = 0; i < deliveryContacts.Count; i++)
				{
					if (!firstPrintJobEncountered && deliveryContacts[i].DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
					{
						if (instructions.PrinterDelivery.PrintQueue == null)
						{
							instructions.ShowPrinterSelectionUI();
						}
						firstPrintJobEncountered = true;
					}
					RunForContact(deliveryContacts[i], instructions, notifications);
					instructions.OnDocPrintedForContact(i + 1, deliveryContacts.Count);
				}
			}
		}

		internal DocDeliveryContactCollection GetDeliveryContacts(DeliveryInstructions instructions)
		{
			DocDeliveryContactCollection result = new DocDeliveryContactCollection(Factory);

			if (!instructions.MultipleDocumentPacks)
			{
				result = instructions.Recipients;
			}
			else if (DocumentSupporter != null)
			{
				result = AutoDocumentDelivery.GetDeliveryContactsForDocPack(StmMenuCommand, DocumentSupporter, DocumentGroup);
				if (instructions.PrintMultiDocPack)
				{
					SetAllDeliveryContactsToPrint(result);
				}
			}

			return result;
		}

		void SetAllDeliveryContactsToPrint(DocDeliveryContactCollection contacts)
		{
			foreach (DocDeliveryContact deliveryContact in contacts)
			{
				deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			}
		}

		#endregion

		#region Cover Sheet

		IDeliverable AddCoverSheetIfNotAlreadyAdded(ZString deliveryMethod, DeliveryInstructions instructions)
		{
			ExcelTemplate template = null;
			ZString reportName = "";
			PrintCopyType printCopyType = PrintCopyType.ALL;

			switch (deliveryMethod)
			{
				case Core.Constants.ContactNotifyModes.Fax:
					if (!FaxCoverSheetAdded)
					{
						FaxCoverSheetAdded = true;
						template = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.FaxCoverSheet);
						reportName = (NoResString)"Fax Cover Sheet";
						printCopyType = PrintCopyType.FAX;
					}
					break;

				case Core.Constants.ContactNotifyModes.Email:
				case Core.Constants.ContactNotifyModes.EPrint:
					if (!EmailCoverSheetAdded)
					{
						EmailCoverSheetAdded = true;
						template = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.EmailCoverSheet);
						reportName = (NoResString)"Email Cover Sheet";
						printCopyType = PrintCopyType.EML;
					}
					break;

				case Core.Constants.ContactNotifyModes.Print:
					if (!PrintCoverSheetAdded)
					{
						PrintCoverSheetAdded = true;
						template = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.PrintCoverSheet);
						reportName = (NoResString)"Cover Sheet";
						printCopyType = PrintCopyType.PRN;
					}
					break;

				default:
					break;
			}

			Report coverSheet = null;
			if (template != null)
			{
				coverSheet = new Report(this, template, instructions, reportName, null, DocumentDirection.ANY, false);
				coverSheet.PrintCopyType = printCopyType;
				coverSheet.IsCoverSheet = true;
				this.Insert(0, coverSheet);
			}
			return coverSheet;
		}

		bool FaxCoverSheetAdded;
		bool EmailCoverSheetAdded;
		bool PrintCoverSheetAdded;

		void RemoveCoverSheets(DeliveryInstructions instructions)
		{
			for (var index = this.Count - 1; index >= 0; index--)
			{
				var report = this[index] as Report;
				if (report != null && report.IsCoverSheet)
				{
					report.Dispose();
					Remove(report);
				}
			}

			FaxCoverSheetAdded = false;
			EmailCoverSheetAdded = false;
			PrintCoverSheetAdded = false;
		}

		#endregion

		#region Individual Contact Delivery

		public IPrintTaskUIProvider PrintTaskUIProvider
		{
			get { return printTaskUIProvider ?? (printTaskUIProvider = PrintTaskUIProviderFactory.Create()); }
		}

		IPrintTaskUIProvider printTaskUIProvider;

		void RunForContact(OrgContact contact, DeliveryInstructions instructions, INotifications notifications = null)
		{
			DocDeliveryContact deliveryContact = AutoDocumentDelivery.GetDeliveryDetailsForContact(contact, StmMenuCommand);
			RunForContact(deliveryContact, instructions, notifications);
		}

		internal void ResetIsDraftIfNeeded(DeliveryInstructions instructions)
		{
			if (DeliveryInstructions.IsDraft != instructions.IsDraft)
			{
				DeliveryInstructions.IsDraft = instructions.IsDraft;
				foreach (var deliverable in this)
				{
					var report = deliverable as Report;
					if (report != null)
					{
						report.ResetAnalyserRendererAndExcelInterface();
					}
				}
			}
		}

		protected virtual void RunForContact(DocDeliveryContact contact, DeliveryInstructions instructions, INotifications notifications = null)
		{
			Factory?.ServiceContainer.GetService<DocumentUsageDetailsCollector>()?.GetContactDetails(contact);

			RebuildIfLanguageChanged(instructions);

			ResetIsDraftIfNeeded(instructions);

			var method = DeliveryMethod.FromContact(contact, instructions);
			instructions.DeliveryMethod = method;

			if (method != null)
			{
				var isEmail = method is Email;
				if (isEmail)
				{
					if (contact != null && !contact.EmailSubjectMacro.IsEmpty)
					{
						ZString translatedEmailSubject = new TextMacroProcessor().Replace(contact.EmailSubjectMacro, instructions.GetRelatedBusinessObjectsForEmailSubject(contact));
						if (!translatedEmailSubject.IsEmpty)
						{
							translatedEmailSubject = translatedEmailSubject.TrimEndSpaceTab();
							var group = instructions.DeliveryGroups.FirstOrDefault(g => g.SB_EmailSubjectLine == translatedEmailSubject);
							if (group == null)
							{
								group = instructions.Factory.New<StmDeliveryGroup>();
								group.SB_EmailSubjectLine = translatedEmailSubject.SubstringSafe(0, StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength);

								instructions.DeliveryGroups.Add(group);
							}

							contact.DeliveryGroupId = group.PK;
							instructions.FactorySaveStrategy.SaveChunk(group.Factory);
						}
					}
				}

				bool includeCoverNote = instructions.IncludeCoverNote;
				instructions.DeliverablesToBePrinted.Sort(DeliverableConstants.Index, System.ComponentModel.ListSortDirection.Ascending);
				var collection = instructions.DocPackIsSet ? instructions.DeliverablesToBePrinted : (IList)this;
				for (int i = 0; i < collection.Count; i++)
				{
					if (!instructions.ShouldContinueIfDocumentsFileSizeGreaterThanSizeLimitInBytes())
					{
						break;
					}

					IDeliverable deliverable = (IDeliverable)collection[i];
					deliverable.IsDeliveredByEmail = isEmail;

					Report report = deliverable as Report;

					var contactForDelivery = ShouldRenderUsingReportContact(report, instructions)
						? report.DeliveryContact
						: contact;

					RenderDeliverable(contactForDelivery, instructions, method, deliverable);

					if (deliverable.CoverSheetRequired)
					{
						includeCoverNote = true;
					}

					instructions.OnDocPrintedWithinPack(i + 1, collection.Count);
				}

				if (includeCoverNote && (contact != null))
				{
					using (IDeliverable coverSheet = AddCoverSheetIfNotAlreadyAdded(contact.DeliveryMethod, instructions))
					{
						if (coverSheet != null)
						{
							RenderDeliverable(contact, instructions, method, coverSheet);
							instructions.OnDocPrintedWithinPack(collection.Count + 1, collection.Count + 1);
						}
					}
				}

				AdjustEmailSubjectLineForCoverSheet(method);

				if (contact != null)
				{
					method.EmptyReportContingency = contact.EmptyReportContingency;
				}

				Deliver(method, instructions, notifications);
				method.ReleaseDeliveryInfos();
				RemoveCoverSheets(instructions);
			}
		}

		bool ShouldRenderUsingReportContact(Report report, DeliveryInstructions instructions)
		{
			return instructions.Recipients.Count == 1 && report != null && report.DeliveryContact != null;
		}

		internal void RebuildIfLanguageChanged(DeliveryInstructions instructions)
		{
			if (Language != instructions.Language && (instructions.LanguageHasBeenSet || instructions.HasDocumentDeliveryDefaultLanguage))
			{
				Language = instructions.Language;
			}
			RebuildIfLanguageChangedCore();
		}

		protected virtual void RebuildIfLanguageChangedCore()
		{
			if (LastTemplateGeneratorLanguage != null && LastTemplateGeneratorLanguage != Language)
			{
				this.OfType<Report>().ForEach(r => r.ReGenerateTemplate());
				LastTemplateGeneratorLanguage = Language;
			}
		}

		protected void RenderDeliverable(DocDeliveryContact deliveryContact, DeliveryInstructions deliveryInstructions,
			DeliveryMethod deliveryMethod, IDeliverable deliverable)
		{
			if (deliverable.IncludedInPrint)
			{
				if (ShouldAddDeliveryInfo(deliveryContact, deliveryInstructions, deliverable))
				{
					var strategy = new CreateDeliveryInfoStrategyFactory().Create(deliveryMethod, deliverable.MenuItem);
					var deliveryInfo = strategy.CreateDeliveryInfo(deliverable, deliveryContact, deliveryInstructions);

					if (deliveryInfo != null)
					{
						var report = deliverable as Report;
						if (report == null || (report != null && !IsSimilarDeliveryAlreadyProcessed(deliveryInfo, deliveryContact)))
						{
							deliveryInfo.DocumentPack = this;

							if (report != null)
							{
								NotifyDeliveryInfoCreated(deliveryInfo, deliveryContact);
								if (report.Style == Report.Styles.Report)
								{
									deliveryMethod.ReportHasNoDataRows = !report.ContainsDataRows;
								}
							}

							deliveryInstructions.SumAttachmentFileSize(deliveryInfo);
							if (deliveryInfo.IsCoverSheet)
							{
								deliveryMethod.AddFileAtBeginning(deliveryInfo);
							}
							else
							{
								deliveryMethod.AddFile(deliveryInfo);
							}
						}
					}

					CallOnAfterReportRun(deliverable);
				}
			}
		}

		void NotifyDeliveryInfoCreated(DeliveryInfo info, DocDeliveryContact deliveryContact)
		{
			if (Parent != null && info != null && Parent.IsAutoDocumentDelivery)
			{
				Parent.NotifyDeliveryInfoCreated(info, deliveryContact);
			}
		}

		bool IsSimilarDeliveryAlreadyProcessed(DeliveryInfo info, DocDeliveryContact deliveryContact)
		{
			bool result = false;
			if (Parent != null && info != null && Parent.IsAutoDocumentDelivery)
			{
				result = Parent.IsSimilarDeliveryAlreadyProcessed(info, deliveryContact);
			}
			return result;
		}

		static bool ShouldAddDeliveryInfo(DocDeliveryContact contact, DeliveryInstructions instructions, IDeliverable deliverable)
		{
			var isInstructionNotToPrintButDeliverablesDeliveryModeMatchesContact =
				instructions.Destination != DeliveryInstructionDestination.Print &&
				(contact == null || deliverable.SupportsDeliveryMethod(contact.DeliveryMethod));

			var isInstructionToPrintAndDeliverablesDeliveryModeAllowsPrint =
				instructions.Destination == DeliveryInstructionDestination.Print &&
				deliverable.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print);

			//Refer to DeliverableCollectionView.RebuildCore, deliverable.CanIncludeInPrint will be false if deliverable doesn't support deliveryMethod, if IncludeInPrint is still true, then it is set by user manually.
			var isDeliveryModeNotMatchesContactButUserChooseToPrint = !(isInstructionNotToPrintButDeliverablesDeliveryModeMatchesContact || deliverable.CanIncludeInPrint);

			return isInstructionNotToPrintButDeliverablesDeliveryModeMatchesContact || isInstructionToPrintAndDeliverablesDeliveryModeAllowsPrint || isDeliveryModeNotMatchesContactButUserChooseToPrint;
		}

		static void AdjustEmailSubjectLineForCoverSheet(DeliveryMethod method)
		{
			if (method.Infos.Count > 1 && method.Infos[0].EmailSubjectLine.IndexOf("Cover Sheet") != -1)
			{
				method.Infos[0].EmailSubjectLine = method.Infos[1].EmailSubjectLine;
			}
		}

		internal BusinessObject BusinessObjectForPrintJob
		{
			get
			{
				if (businessObjectForPrintJob == null)
				{
					businessObjectForPrintJob = this.OfType<Report>().FirstOrDefault(r => r.SourcePivotPK == StmMenuCommand?.SU_PrimaryDocPackItemId)?.BusinessObjectForPrintJob;
				}

				if (businessObjectForPrintJob == null && !isBOToLogAgainstForced)
				{
					businessObjectForPrintJob = this.OfType<Report>().FirstOrDefault()?.BusinessObjectForPrintJob;
				}

				return businessObjectForPrintJob ?? BusinessObjectToLogAgainst;
			}
		}
		BusinessObject businessObjectForPrintJob;

		public BusinessObject BusinessObjectToLogAgainst
		{
			get { return fBusinessObjectToLogAgainst ?? (fBusinessObjectToLogAgainst = GetBusinessObjectToLogAgainstFromTheFirstAttachedReportThatHasOne()); }
		}

		BusinessObject fBusinessObjectToLogAgainst;
		bool isBOToLogAgainstForced;

		BusinessObject GetBusinessObjectToLogAgainstFromTheFirstAttachedReportThatHasOne()
		{
			BusinessObject result = null;
			foreach (IDeliverable itemToDeliver in this)
			{
				//Pick the first business object to log against we find in all deliverables...
				//Unless one of them is equal to BizObject, then always choose that one (and break as there's no point in searching further).
				//Fixes WI00197237 / CS00630742, a case where a Shipment/Consol customized document pack sent from a Consol would log
				//against the Shipment OR the Consol based on whether the first document was a Shipment or Consol document, rather than
				//always logging against the Consol.

				var report = itemToDeliver as Report;
				BusinessObject tempResult = null;
				if (report != null)
				{
					tempResult = report.GetBusinessObjectToLogAgainst();
				}

				if (tempResult != null && BizObject is BusinessObject && tempResult.PK == ((BusinessObject)BizObject).PK)
				{
					result = tempResult;
					break;
				}
				else if (result == null)
				{
					result = tempResult;
				}
			}

			if (result == null || result is NonPersistentBusinessObject)
			{
				BusinessObject parent = (BusinessObject)BizObject;

				if (parent != null && !(parent is NonPersistentBusinessObject))
				{
					result = parent;
				}
			}

			return result;
		}

		/// <summary>
		/// Would like to have this internal, but Accounting, Rating, Customs and Warehousing have used it as a DodgyHack (tm).
		/// </summary>
		/// <param name="businessObjectToLogAgainst"></param>
		public void ForceBusinessObjectToLogAgainst(BusinessObject businessObjectToLogAgainst)
		{
			fBusinessObjectToLogAgainst = businessObjectToLogAgainst;
			isBOToLogAgainstForced = true;
		}

		public IEnumerable<BusinessObject> BusinessObjectsForPrintJob(bool? includedInPrint = null) => this.OfType<Report>()
			.Where(report => includedInPrint == null || report.IncludedInPrint == includedInPrint)
			.Select(report => report.BusinessObjectForPrintJob)
			.WhereNotNull();

		public event PrintTask.AfterReportRunEventHandler OnAfterReportRun;

		protected void CallOnAfterReportRun(IDeliverable itemToDeliver)
		{
			if (OnAfterReportRun != null)
			{
				OnAfterReportRun(this, itemToDeliver);
			}
		}
		#endregion

		#region Delivery

		internal void Deliver(DeliveryMethod method, DeliveryInstructions instructions, INotifications notifications = null)
		{
			method.ForcedParentDocManagerInfo = this.bizoForcedToBeEdocsParent;
			DeliverCore(method, notifications);
			AddAndSaveDocumentDeliveredLogToDocumentBusinessObject(method, instructions);
		}

		protected virtual void DeliverCore(DeliveryMethod method, INotifications notifications = null)
		{
			method.EmailSubjectForConsolidateReports = EmailSubjectForConsolidateReports;
			method.AttachedFileNameForConsolidateReports = EmailSubjectForConsolidateReports;
			method.Deliver(notifications);
		}

		void AddAndSaveDocumentDeliveredLogToDocumentBusinessObject(DeliveryMethod method, DeliveryInstructions instructions)
		{
			if (!instructions.IsDraft && !(method is DeliveryMethods.ExcelPreview) && !(method is DeliveryMethods.Disk))
			{
				var factoryToSaveLogs = instructions.FactorySaveStrategy.GetFactory();
				var eventsToDeliver = new List<(BusinessObject, EventValue)>();

				foreach (IDeliverable deliverable in this)
				{
					if (deliverable.IncludedInPrint)
					{
						if (deliverable.MenuItem is StmMenuItemBase menuItemBase && BusinessObjectToLogAgainst != null && (!deliverable.DocumentDeliveredEventCode.IsEmpty || !deliverable.DocumentPasswordInformationEventCode.IsEmpty))
						{
							BusinessObject businessObjectInOtherFactory = null;
							if (BusinessObjectToLogAgainst is IDocumentDeliveredLogSupporter logSupporter)
							{
								businessObjectInOtherFactory = factoryToSaveLogs.Load(logSupporter.BusinessObjectTypeToLogAgainst, logSupporter.Identifier);
							}
							else if (!(BusinessObjectToLogAgainst is NonPersistentBusinessObject))
							{
								businessObjectInOtherFactory = factoryToSaveLogs.Load(BusinessObjectToLogAgainst.GetType(), BusinessObjectToLogAgainst.PK);
							}
							eventsToDeliver.AddRange(deliverable.GetEventsToDeliver(businessObjectInOtherFactory, menuItemBase));
						}
					}
				}

				try
				{
					TryReallyHardToAddEvents(instructions, factoryToSaveLogs, eventsToDeliver);
				}
				catch (ZSaveConcurrencyException e)
				{
					var events = string.Join(",", eventsToDeliver.ConvertAll(x => x.Item2.Code));
					Globals.Message.ShowError(Res.GetString("36C2D872-8F0D-4D04-B69A-D29724F43056",
						"The document is successfully delivered, but error occurring when generating the {0} event: {1}.",
						events, e.Message));
				}
			}
		}

		static void TryReallyHardToAddEvents(DeliveryInstructions instructions, BusinessObjectFactory factoryToSaveLogs, List<(BusinessObject, EventValue)> eventsToDeliver)
		{
			if (eventsToDeliver.Any())
			{
				foreach ((var biz, var evnt) in eventsToDeliver)
				{
					biz.GetLogs().AddNew(evnt);
				}

				try
				{
					instructions.FactorySaveStrategy.SaveChunk(factoryToSaveLogs);
				}
				catch (ZCannotSaveException)
				{
					// Workflow template application can cause save to be blocked in the GUI.
					// In the case where that occurs we would still like to add these events.
					var retryFactory = instructions.CreateNewFactory();
					using (ProcessTask.Loader.SuppressTemplateApplication())
					{
						foreach ((var biz, var evnt) in eventsToDeliver)
						{
							retryFactory.Load(biz.GetType(), biz.PK).GetLogs().AddNew(evnt);
						}
						instructions.FactorySaveStrategy.SaveChunk(retryFactory);
					}
				}
			}
		}

		/// <summary>
		/// If ConsolidateReports is set to true, then you can specify a subject for the consolidated email.
		/// </summary>
		public string EmailSubjectForConsolidateReports { get; set; }
		/// <summary>
		/// NumberOfDocumentNeedToBeConsolidated will be shown in the consolidated email subject when there are multiple documents in a pack.
		/// </summary>
		public int NumberOfDocumentNeedToBeConsolidated { get; set; }

		public virtual DocAutoDelivery AutoDocumentDelivery
		{
			get { return new DocAutoDelivery(); }
		}

		#endregion

		#endregion

		#region Language

		string DefaultPrintLanguage
		{
			get
			{
				var report = GetFirstReport();
				if (report != null && report.Style == Report.Styles.Report)
				{
					return Res.CurrentLanguage == Res.DefaultLanguage ? DataRegistry.Instance.EnglishSpelling : Res.CurrentLanguage;
				}
				return DataRegistry.Instance.EnglishSpelling;
			}
		}

		public ZString Language
		{
			get
			{
				if (language.IsEmpty)
				{
					language = SupportsLanguageSelection ? TemplateLanguageSelector.SelectLanguage(OrgHeaderContact, DocumentGroup, DocumentSupporter?.TransportMode, StmMenuCommand) : (ZString)DefaultPrintLanguage;
					ResetReportsHeadingTextIfLanguageChanged();
				}
				return language;
			}
			set
			{
				if (language != value)
				{
					if (Culture.LanguageCodeMapping.TryGetValue(value, out string newLanguage))
					{
						language = newLanguage;
					}
					else
					{
						language = value;
					}
					ResetReportsHeadingTextIfLanguageChanged();
				}
			}
		}

		ZString language;
		internal bool IsResetingReportsHeadingTextSuspended { get; set; }

		public IDisposable SuspendResetingReportHeadingText()
		{
			IsResetingReportsHeadingTextSuspended = true;
			return new DisposableAction(() => IsResetingReportsHeadingTextSuspended = false);
		}

		void ResetReportsHeadingTextIfLanguageChanged()
		{
			if (IsResetingReportsHeadingTextSuspended)
			{
				return;
			}
			foreach (var deliverable in this)
			{
				if (deliverable is Report report && report.Style == Report.Styles.Report && report.StTemplate != null && report.StTemplate.SO_IsSystemDefined)
				{
					string templateFileName = Path.GetFileNameWithoutExtension(report.StTemplate.SO_ExcelTemplatePath);

					using (Res.TemporarilySwitchLanguage(report.Language))
					{
						if (report.ColumnHeadingManager.CurrentColumnConfigurationManager?.RefreshColumnSettingsIfLanguageChangedBack(report.Language) ?? false)
						{
							continue;
						}

						foreach (Worksheet workSheet in report.ColumnHeadingManager.CurrentConfiguration.Worksheets)
						{
							foreach (ColumnHeading heading in workSheet.ColumnHeadings)
							{
								heading.HeadingText = !string.IsNullOrEmpty(heading.EnglishHeadingText)
									? (string)report.MacroTranslator.GetValue(
										DocBuilderResourceStrings.GetReportString(templateFileName, heading.EnglishHeadingText), Passes.FirstPass)
									: (string)heading.HeadingText;
							}
						}
					}
				}
			}
		}

		public bool SupportsLanguageSelection { get; internal set; }
		public string ForcedLanguage { get; internal set; } = string.Empty;

		internal void SetSupportsLanguageSelectionForTranslatableLegacyDocument()
		{
			if (!SupportsLanguageSelection && !supportsLanguageSelectionHasBeenSetForTranslatableLegacyDocument && StmMenuCommand is DocumentCommand documentCommand && documentCommand.Parent != null)
			{
				foreach (var report in this.OfType<Report>())
				{
					report.PrepareForRender();
					var config = report.Analyser?.Config;
					if (config is { TranslateLegacyDocument: true })
					{
						SupportsLanguageSelection = true;
						if (config.ForcedLanguage.IsNullOrEmpty() || !ForcedLanguage.IsNullOrEmpty() && ForcedLanguage != config.ForcedLanguage)
						{
							ForcedLanguage = "";
							report.ResetAnalyserRendererAndExcelInterface();
							break;
						}
						ForcedLanguage = config.ForcedLanguage;
					}
					report.ResetAnalyserRendererAndExcelInterface();
				}
				language = ForcedLanguage;
				supportsLanguageSelectionHasBeenSetForTranslatableLegacyDocument = true;
			}
		}

		bool supportsLanguageSelectionHasBeenSetForTranslatableLegacyDocument;

		#endregion

		#region Add

		#region eDocs

		public void AddEDocsToPack(DocumentCommand command)
		{
			if (AutoAddeDocsSuspendedCount.Value == 0)
			{
				var eDocsProvider = new EDocsToDeliverProvider(command);
				foreach (var deliverable in eDocsProvider.EDocsToAttach)
				{
					AddAndSetMenuItem(deliverable, command);
				}

				otherEDocsToAttach = eDocsProvider.OtherEDocsToAttach;
			}
		}

		List<IDeliverable> otherEDocsToAttach;
		internal List<IDeliverable> OtherEDocsToAttach => otherEDocsToAttach ?? (otherEDocsToAttach = new List<IDeliverable>());

		class EDocsToDeliverProvider
		{
			internal EDocsToDeliverProvider(DocumentCommand command)
			{
				this.command = command;
			}

			readonly DocumentCommand command;

			internal List<IDeliverable> EDocsToAttach
			{
				get
				{
					if (eDocsToAttach == null)
					{
						eDocsToAttach = new List<IDeliverable>();
						var eDocsDocumentParent = command.Parent as IDocManagerSupport;
						if (eDocsDocumentParent != null)
						{
							eDocsDocumentParent.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
							AddEDocsFromDocumentConfiguration(eDocsDocumentParent);
							if (command.Documents.Any())
							{
								AddEDocsFromJob(eDocsDocumentParent);
								AddEDocsFromRelatedObjects(eDocsDocumentParent);
							}
						}
					}
					return eDocsToAttach;
				}
			}
			List<IDeliverable> eDocsToAttach;

			internal List<IDeliverable> OtherEDocsToAttach { get; } = new List<IDeliverable>();

			void AddEDocsFromJob(IDocManagerSupport eDocsDocumentParent)
			{
				foreach (IDeliverable eDoc in eDocsDocumentParent.DocManagerInfo.EDocsView)
				{
					if (!((IeDoc)eDoc).IsDeleted && !eDocsToAttach.Contains(eDoc))
					{
						OtherEDocsToAttach.Add(eDoc);
					}
				}
			}

			void AddEDocsFromRelatedObjects(IDocManagerSupport eDocsDocumentParent)
			{
				var relatedObjects = eDocsDocumentParent.DocManagerInfo.RelatedObjects.Where(x => x is IDocManagerSupport && !(x is OrgHeader)).ToArray();
				foreach (IDocManagerSupport relatedEDocsDocumentParent in relatedObjects)
				{
					relatedEDocsDocumentParent.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
					AddEDocsFromJob(relatedEDocsDocumentParent);
				}
			}

			void AddEDocsFromDocumentConfiguration(IDocManagerSupport eDocsDocumentParent)
			{
				foreach (StmMenuEDocs eDocTypeToAttach in command.EDocsView)
				{
					AddEDocsToAttach(eDocTypeToAttach, eDocsDocumentParent);
				}

				foreach (IEDocsProvider childEDocsDocumentParent in eDocsDocumentParent.DocManagerInfo.GetEDocsProviders())
				{
					EDocsProviderSupporter childSupporter = childEDocsDocumentParent.GetEDocsProviderSupporter();
					DocumentCommand childProviderPlaceholder = childSupporter.GetProviderPlaceholder<DocumentCommand>(command);
					if (childProviderPlaceholder != null)
					{
						foreach (StmMenuEDocs eDocTypeToAttach in childProviderPlaceholder.EDocsView)
						{
							AddEDocsToAttach(eDocTypeToAttach, childEDocsDocumentParent);
						}
					}
				}
			}

			void AddEDocsToAttach(StmMenuEDocs eDocTypeToAttach, IDocManagerSupport eDocsDocumentParent)
			{
				RefDocType refDocType = eDocTypeToAttach.DocType;
				ZString documentTypeCode = (refDocType != null) ? refDocType.RT_DocType : ZString.Empty;
				if (!documentTypeCode.IsEmpty)
				{
					switch (DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.Value)
					{
						case DeliverEDocsOptionList.Codes.SendMostRecentOnly:
							AddEDocsForSendMostRecent(documentTypeCode, eDocTypeToAttach.SX_Filter, eDocsDocumentParent);
							break;

						case DeliverEDocsOptionList.Codes.SendAll:
							AddEDocsForSendAll(documentTypeCode, eDocTypeToAttach.SX_Filter, eDocsDocumentParent);
							break;

						case DeliverEDocsOptionList.Codes.SendMostRecentSystemGeneratedAndAllManuallyAdded:
							AddEDocsForSendMostRecentAndAllManuallyAdded(documentTypeCode, eDocTypeToAttach.SX_Filter, eDocsDocumentParent);
							break;
					}
				}
			}

			void AddEDocsForSendMostRecent(ZString docTypeCode, ZString eDocFilter, IDocManagerSupport eDocsDocumentParent)
			{
				var eDoc = eDocsDocumentParent.DocManagerInfo.EDocsView.GetMostRecentEDoc(docTypeCode);
				AttachEDocs(eDoc, eDocFilter);
			}

			void AddEDocsForSendAll(ZString docTypeCode, ZString eDocFilter, IDocManagerSupport eDocsDocumentParent)
			{
				foreach (IeDoc eDoc in eDocsDocumentParent.DocManagerInfo.EDocsView)
				{
					if (!eDoc.IsDeleted && eDoc.DocType.Equals(docTypeCode))
					{
						AttachEDocs(eDoc, eDocFilter);
					}
				}
			}

			void AddEDocsForSendMostRecentAndAllManuallyAdded(ZString docTypeCode, ZString eDocFilter, IDocManagerSupport eDocsDocumentParent)
			{
				IeDoc mostRecentSystemEDoc = null;

				foreach (IeDoc eDoc in eDocsDocumentParent.DocManagerInfo.EDocsView)
				{
					if (!eDoc.IsDeleted && eDoc.DocType.Equals(docTypeCode))
					{
						if (eDoc.IsSystemGenerated)
						{
							if (IsEDocMatchingFilter(eDoc, eDocFilter))
							{
								if (mostRecentSystemEDoc == null || eDoc.DateAdded > mostRecentSystemEDoc.DateAdded)
								{
									mostRecentSystemEDoc = eDoc;
								}
							}
						}
						else
						{
							AttachEDocs(eDoc, eDocFilter);
						}
					}
				}

				if (mostRecentSystemEDoc != null)
				{
					AttachEDocs(mostRecentSystemEDoc);
				}
			}

			void AttachEDocs(IeDoc eDoc, ZString eDocFilter)
			{
				if (eDoc == null)
				{
					return;
				}

				if (IsEDocMatchingFilter(eDoc, eDocFilter))
				{
					AttachEDocs(eDoc);
				}
			}

			bool IsEDocMatchingFilter(IeDoc eDoc, ZString eDocFilter)
			{
				return eDocFilter.IsEmpty ||
					ZExpressionEvaluator.Evaluate(eDocFilter, command.Parent, BODocDataProvider.Get(command.Parent as BusinessObject), BODocDataProvider.Get(eDoc as BusinessObject));
			}

			void AttachEDocs(IeDoc eDoc)
			{
				var deliverable = (IDeliverable)eDoc;
				deliverable.ShouldPrintByDefault = true;
				eDocsToAttach.Add(deliverable);
			}
		}

		public void InsertAtStart(Report report)
		{
			Insert(0, report);
			report.SetParent(this);
		}

		#endregion

		#region Reports

		readonly List<ZGuid> documentListChecked = new List<ZGuid>();
		void CheckDocumentCommandSupportsLanguageSelection(DocumentCommand documentCommand)
		{
			if (!SupportsLanguageSelection && !documentListChecked.Contains(documentCommand.PK))
			{
				documentListChecked.Add(documentCommand.PK);
				foreach (StmMenuTemplatePivotBase pivot in documentCommand.Documents.ToArray()) //Use ToArray() to prevent changing collection during iteration.
				{
					if (pivot.Template != null && (pivot.Template.IsDocBuilderStyle || pivot.Template.IsClientSpecificDocBuilderStyle))
					{
						SupportsLanguageSelection = true;
						break;
					}
				}

				if (!SupportsLanguageSelection)
				{
					DataProviderList dataProviders = ChildCommandsLoader.GetDataProviderListByDocumentCommand(documentCommand);
					foreach (StmMenuMenuPivotBase childCommandPivot in documentCommand.ChildMenus)
					{
						if (SupportsLanguageSelection)
						{
							break;
						}

						if (ChildCommandsLoader.MeetsMenuFilter(childCommandPivot, dataProviders))
						{
							var childCommand = documentCommand.Factory.Load<DocumentCommand>(childCommandPivot.SF_SU_Outward);
							CheckDocumentCommandSupportsLanguageSelection(childCommand);
						}
					}
				}
			}
		}

		public void AddReportsToPack(DocumentCommand documentCommand, DocumentCommand parentDocumentCommand, IDocumentSupportable topLevelBusinessObject, UserControlProviderList userDefinedFieldList)
		{
			if (documentCommand == null)
			{
				throw new InvalidOperationException("Cannot add null document command to a document pack.");
			}

			if (shouldAutoAddeDocs)
			{
				AddEDocsToPack(documentCommand);
			}

			CheckDocumentCommandSupportsLanguageSelection(documentCommand);
			bool shouldShowMessageForNotPrinting = false;

			if (documentCommand.SU_MenuType == Core.Constants.StmMenuItemTypes.Forms)
			{
				var formDeliverables = ObjectFactory.Get<IFormDeliverablesProvider>().GetDeliverables(documentCommand, topLevelBusinessObject)
					?? Array.Empty<IDeliverable>();

				foreach (var deliverable in formDeliverables)
				{
					AddAndSetMenuItem(deliverable, documentCommand);
				}
			}
			else
			{
				foreach (StmMenuTemplatePivotBase pivot in documentCommand.Documents.ToArray()) //Use ToArray() to prevent changing collection during iteration.
				{
					if (ShouldAddPivotAsReport(pivot))
					{
						var templateGenerator = GetTemplateGenerator(pivot);
						var topLevelBODocumentSupporter = topLevelBusinessObject?.DocumentSupporter;
						if (!templateGenerator.IsDocumentConfigValid)
						{
							shouldShowMessageForNotPrinting = true;
							if (templateGenerator.IsDocumentConfigTemplate)
							{
								ReasonsForEmptyPacks.Add(Res.GetString("0de7dee3-4f10-488f-a189-99240d7ba416", @"No Customizable Document Configuration could be applied for DocBuilder template '{0}' of document '{1}'.
The system configuration provided serves as an example and needs to be copied with a default recipient.", pivot.SI_DocumentTitle, documentCommand.SU_MenuName));
							}
							else
							{
								ReasonsForEmptyPacks.Add(Res.GetString("e808f5b0-033f-4259-8dfd-2cffb4a13e3d", @"No Customizable Document Configuration could be applied for DocBuilder template '{0}' of document '{1}'.
Please check if a non-client specific configuration exists for this template, or whether client-specific configurations match the Document Group of this document.", pivot.SI_DocumentTitle, documentCommand.SU_MenuName));
							}
						}
						else if (topLevelBODocumentSupporter != null)
						{
							var boDocDataProviders = topLevelBODocumentSupporter.GetBODocDataProviders(templateGenerator.DataContext, documentCommand);
							bool documentCanBePrinted = boDocDataProviders != null && boDocDataProviders.Length > 0;

							if (documentCanBePrinted)
							{
								AddDocWrappersAsReports(pivot, boDocDataProviders, documentCommand, parentDocumentCommand, topLevelBusinessObject, userDefinedFieldList, templateGenerator);
							}
							else if (topLevelBODocumentSupporter.ShowReasonForNotPrinting(templateGenerator.DataContext.DataContext, documentCommand))
							{
								shouldShowMessageForNotPrinting = true;
								var messageForCurrentCommand = topLevelBODocumentSupporter.GetBODocDataProvidersNotFoundMessage(templateGenerator.DataContext, documentCommand);
								if (!string.IsNullOrEmpty(messageForCurrentCommand) && !ReasonsForEmptyPacks.Contains(messageForCurrentCommand))
								{
									ReasonsForEmptyPacks.Add(messageForCurrentCommand);
								}
							}
						}
					}
				}
			}

			if (shouldShowMessageForNotPrinting || Count == 0)
			{
				ReasonsForEmptyPacks.Insert(0, Res.GetString("8d4e364a-a4a8-4218-b64b-a93a08cca936", "Cannot produce this Document because the data required to do so is not present"));
				if (ReasonsForEmptyPacks.Count > 1)
				{
					ReasonsForEmptyPacks.Insert(1, Res.GetString("8ae3e110-5e67-4782-ab98-d0721199233e", "Details:"));
				}
			}

			if (string.IsNullOrEmpty(LastTemplateGeneratorLanguage))
			{
				LastTemplateGeneratorLanguage = DefaultPrintLanguage;
			}
		}

		bool ShouldAddPivotAsReport(StmMenuTemplatePivotBase pivot)
		{
			return documentSuppressionList == null || !documentSuppressionList.Contains(pivot.PK.ToGuid());
		}

		public TemplateGenerator GetTemplateGenerator(StmMenuTemplatePivotBase pivot)
		{
			LastTemplateGeneratorLanguage = Language;
			return new TemplateGenerator(pivot, Organisation, Language);
		}

		internal string LastTemplateGeneratorLanguage { get; set; }

		protected virtual Report GetNewReport(ExcelTemplate template, DataProviderList dataProviders, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening, ZGuid menuTemplatePivotPK)
		{
			return new Report(this, template, dataProviders, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, isPasswordProtectedForOpening, menuTemplatePivotPK);
		}

		#endregion

		#region Document Wrappers

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		internal void AddDocWrappersAsReports(StmMenuTemplatePivotBase pivot, IBODocDataProvider[] boDocDataProviders, DocumentCommand childCommand, DocumentCommand parentCommand,
			IDocumentSupportable topLevelBusinessObject, UserControlProviderList userFieldList, TemplateGenerator templateGenerator)
		{
			foreach (IBODocDataProvider docDataProvider in boDocDataProviders)
			{
				if (docDataProvider == null)
				{
					continue;
				}

				var printCopyType = GetPrintCopyType(docDataProvider, pivot);
				var dataProviders = new DataProviderList(docDataProvider);
				dataProviders.PrintCopyType = printCopyType;

				var topLevelDataProvider = BODocDataProvider.Get((BusinessObject)topLevelBusinessObject);
				if (!(docDataProvider is BODocDataProvider && docDataProvider.ParentBusinessObject == topLevelDataProvider.ParentBusinessObject))
				{
					dataProviders.Add(topLevelDataProvider);
				}

				if (MeetsMenuTemplateFilter(topLevelBusinessObject, pivot, dataProviders))
				{
					IDocTypeCode docDataTypeCode = docDataProvider as IDocTypeCode;
					if (pivot.DocType != null && docDataTypeCode != null)
					{
						docDataTypeCode.DocTypeCode = pivot.DocType.RT_DocType;
					}

					var bizObjectToGetTitleFor = parentCommand?.Parent ?? topLevelBusinessObject;
					var documentMenuName = parentCommand?.SU_MenuName ?? childCommand.SU_MenuName;

					var titleCopyCount = GetTitleCopyCount(docDataProvider, topLevelBusinessObject, documentMenuName, bizObjectToGetTitleFor, pivot, templateGenerator);

					if (titleCopyCount.CopyCount > 0)
					{
						var filterEvaluator = new FilterEvaluator(docDataProvider, topLevelDataProvider);

						var showByDefault = !((childCommand.SU_IsDocPack || (parentCommand != null && parentCommand.SU_IsDocPack)) && docDataProvider is IShouldExcludeFromDocPackByDefault && ((IShouldExcludeFromDocPackByDefault)docDataProvider).IsExcluded);

						AddReportToPackForDocWrapper(titleCopyCount, pivot, dataProviders, docDataTypeCode?.DocTypeCode ?? (pivot.DocType?.RT_DocType ?? ZString.Empty),
							userFieldList, childCommand, printCopyType, templateGenerator, filterEvaluator, showByDefault, topLevelBusinessObject);
					}
					else if (titleCopyCount is NothingToPrint)
					{
						ReasonsForEmptyPacks.Add(((NothingToPrint)titleCopyCount).Reason);
					}
				}
			}
		}

		protected TitleCopyCountPair GetTitleCopyCount(IBODocDataProvider docDataProvider, IDocumentSupportable docSupportProvider, string documentMenuName, IDocumentSupportable bizObjectToGetTitleFor, StmMenuTemplatePivotBase pivot, TemplateGenerator templateGenerator)
		{
			if (docDataProvider == null)
			{
				throw new ArgumentNullException(nameof(docDataProvider));
			}

			if (docSupportProvider == null)
			{
				throw new ArgumentNullException(nameof(docSupportProvider));
			}

			TitleCopyCountPair titleCopyCount = null;

			DocWrapperCopyInfo additionalCopyInfo = docDataProvider.AdditionalCopyInfo;
			if (additionalCopyInfo != null)
			{
				titleCopyCount = additionalCopyInfo.TitleCopyCountPair;
			}
			else
			{
				var parentBO = docDataProvider.ParentBusinessObject;
				var currentCountryIsEU = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().ToList().Contains(GlbCompany.CurrentCompany.Country.Code);
				if (currentCountryIsEU && parentBO != bizObjectToGetTitleFor && parentBO is IDocumentSupportable)
				{
					titleCopyCount = docSupportProvider.DocumentSupporter.GetDocumentTitlesForPivot(documentMenuName, (IDocumentSupportable)parentBO, pivot);
				}
				else
				{
					titleCopyCount = docSupportProvider.DocumentSupporter.GetDocumentTitlesForPivot(documentMenuName, bizObjectToGetTitleFor, pivot);
				}

				if (titleCopyCount == null)
				{
					var title = pivot.SI_DocumentTitle;
					if (pivot.Template != null && pivot.Template.IsDocBuilderStyle && templateGenerator != null)
					{
						title = templateGenerator.DocumentTitle;
					}

					titleCopyCount = new TitleCopyCountPair(title);
				}
			}

			return titleCopyCount;
		}

		protected PrintCopyType GetPrintCopyType(IBODocDataProvider docDataProvider, StmMenuTemplatePivotBase pivot)
		{
			PrintCopyType deliveryMethod = PrintCopyType.ALL;

			DocWrapperCopyInfo additionalCopyInfo = docDataProvider.AdditionalCopyInfo;
			if (additionalCopyInfo != null)
			{
				deliveryMethod = additionalCopyInfo.DeliveryMethod;
			}
			else if (!pivot.SI_PrintCopyType.IsEmpty)
			{
				try
				{
					deliveryMethod = (PrintCopyType)Enum.Parse(typeof(PrintCopyType), pivot.SI_PrintCopyType, true);
				}
				catch (ArgumentException)
				{
					deliveryMethod = PrintCopyType.ALL;
				}
			}

			return deliveryMethod;
		}

		void AddReportToPackForDocWrapper(TitleCopyCountPair titleCopyCount, StmMenuTemplatePivotBase menuTemplatePivot, DataProviderList docDataProvider, ZString docTypeCode,
			UserControlProviderList userFieldList, DocumentCommand documentCommand, PrintCopyType printCopyType, TemplateGenerator templateGenerator, FilterEvaluator filterEvaluator, bool showByDefault, IDocumentSupportable topLevelBusinessObject)
		{
			var excelTemplate = SharedTemplateInPack ?? templateGenerator.Generate(filterEvaluator);

			var newReport = GetNewReport(excelTemplate, docDataProvider, "", userFieldList, DocumentDirection, menuTemplatePivot.SI_IsPasswordProtected, menuTemplatePivot.SI_IsPasswordProtectedForOpening, menuTemplatePivot.PK);
			newReport.Name = newReport.TranslateMacros(titleCopyCount.Title);
			newReport.SourcePivotPK = menuTemplatePivot.PK;
			newReport.TypeOfContact = ContactType;
			newReport.PrinterDetails.NumberOfCopies = titleCopyCount.CopyCount;
			newReport.PrintCopyType = printCopyType;
			newReport.DocumentDeliveredEventCode = menuTemplatePivot.DocType != null && !menuTemplatePivot.DocType.RT_SE_NKDocumentReceivedEvent.IsEmpty ? menuTemplatePivot.DocType.RT_SE_NKDocumentReceivedEvent : (ZString)Events.DocumentDelivered.Code;
			newReport.DocTypeCode = docTypeCode;
			newReport.StTemplate = menuTemplatePivot.Template;
			newReport.TemplateGenerationErrors.AddRange(templateGenerator.Errors);
			newReport.IncludedInPrint = showByDefault && menuTemplatePivot.SI_PrintByDefault;
			newReport.FilterEvaluator = filterEvaluator;
			newReport.CustomWatermarkText = topLevelBusinessObject?.DocumentSupporter?.CustomWatermarkText ?? topLevelBusinessObject?.DocumentSupporter?.GetCustomWatermarkText(documentCommand, docDataProvider.PrimaryDataProvider);

			AddAndSetMenuItem(newReport, documentCommand);
		}

		bool MeetsMenuTemplateFilter(IDocumentSupportable parentBusinessObject, StmMenuTemplatePivotBase pivot, DataProviderList dataProviderList)
		{
			bool result = true;
			if (!pivot.SI_MenuTemplateFilter.IsEmpty)
			{
				var pivotDataProvider = BODocDataProvider.Get(pivot);
				dataProviderList.Add(pivotDataProvider);

				if (RegexProvider.InnermostMacrosRegex.IsMatch(pivot.SI_MenuTemplateFilter))
				{
					TranslateMacroForDocumentFilter translator = new TranslateMacroForDocumentFilter(dataProviderList);
					string translated = translator.ReplaceMacros(pivot.SI_MenuTemplateFilter);
					try
					{
						result = ExpressionEvaluator.Evaluate(translated, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
					}
					catch (ExpressionEvaluationException)
					{
						string errorInformation =
							Res.GetString("02D5B668-3BE0-4218-A4A8-BBBF5DCB97A6", @"Invalid filter expression:
Document name: {0}
Document path: {1}
Template name: {2}
Template title: {3}
Template filter: {4}", pivot.Menu.SU_MenuName, pivot.Menu.SU_MenuPath, pivot.SO_Name, pivot.DocumentTitle, pivot.SI_MenuTemplateFilter);
						throw new InvalidMenuTemplateFilterException(errorInformation);
					}
				}
				else
				{
					ZString[] codeValuePair = pivot.SI_MenuTemplateFilter.Split('=');
					string code = codeValuePair[0];
					string expectedValue = codeValuePair[1];

					MenuTemplateFilterType filterType = (MenuTemplateFilterType)Enum.Parse(typeof(MenuTemplateFilterType), code);
					string actualValue = parentBusinessObject.DocumentSupporter.GetMenuTemplateFilterValue(filterType, dataProviderList.PrimaryDataProvider);
					result = (expectedValue == actualValue);
				}
			}
			return result;
		}

		#endregion

		protected void AddAndSetMenuItem(IDeliverable deliverableItem, StmMenuItemBase command)
		{
			deliverableItem.MenuItem = command;
			Add(deliverableItem);
		}

		#endregion

		public bool ShareSameTemplateInPack { get; set; }

		public ExcelTemplate SharedTemplateInPack
		{
			get
			{
				if (ShareSameTemplateInPack && this.Count > 0)
				{
					var firstReport = GetFirstReport();
					return firstReport == null ? null : firstReport.Template;
				}
				return null;
			}
		}

		#region Remove

		public void RemoveAndDisposeAll()
		{
			foreach (IDeliverable deliverable in this)
			{
				if (deliverable != null)
				{
					deliverable.Dispose();
				}
			}

			RemoveAll();
		}

		#endregion

		#region Dispose

		public void Dispose()
		{
			RemoveAndDisposeAll();
			IsDisposed = true;
		}

		public bool IsDisposed { get; private set; }

		#endregion

		#region Visualiser Manager

		internal BusinessObjectFactory VisualizerNoteFactory => visualizerNoteFactory ?? (visualizerNoteFactory = new BusinessObjectFactory { NameForDebugging = "Report.VisualizerContentNote" });
		BusinessObjectFactory visualizerNoteFactory;

		internal void SaveVisualizerContentNote()
		{
			foreach (var deliverable in this)
			{
				var report = deliverable as Report;
				if (report != null)
				{
					report.SaveVisualizerContentNote();
				}
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(this.VisualizerNoteFactory.Save, null, true);
		}

		internal void DeleteVisualizerContentNote()
		{
			foreach (var deliverable in this)
			{
				var report = deliverable as Report;
				if (report != null)
				{
					report.DeleteVisualizerContentNote();
				}
			}
		}

		internal void RevertVisualizerNote()
		{
			foreach (var deliverable in this)
			{
				var report = deliverable as Report;
				if (report != null)
				{
					report.RevertVisualizerNote();
				}
			}
		}

		#endregion

		#region Cloning

		public IDocumentSupportable BizObject { get; private set; }
		internal readonly UserControlProviderList UserFieldList;
		readonly DocumentCommand ParentCommand;

		public DocumentPack Clone()
		{
			DocumentPack theClone = null;
			if (StmMenuCommand == null)
			{
				theClone = new DocumentPack();
			}
			else if (StmMenuCommand is ReportCommand)
			{
				theClone = new DocumentPack((ReportCommand)StmMenuCommand);
			}
			else if (StmMenuCommand is DocumentCommand)
			{
				theClone = new DocumentPack((DocumentCommand)StmMenuCommand, BizObject, UserFieldList, ParentCommand);
			}

			foreach (Report deliverable in this)
			{
				var json = JsonConverterHelper.Serialize(deliverable);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(json))
				{
					theClone.DeserializeDocPackFromReportCollection(deserialisedReport, new NotificationBuffer());
				}
			}

			theClone.Language = Language;

			foreach (Report report in theClone)
			{
				report.UpdateAndSynchroniseFilters();
			}

			return theClone;
		}

		#endregion
	}
}
