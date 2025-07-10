using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public class DeliveryInstructions : DocBaseWrapperBaseWithImageSupport, IObsoleteValidation, ICustomTextTemplateContext, IJsonSerializable
	{
		public DeliveryInstructions(FactoryStrategy saveStrategy)
			: this(DocumentPack.EmptyPack, saveStrategy)
		{
		}

		public DeliveryInstructions()
			: this(DocumentPack.EmptyPack)
		{
		}

		public DeliveryInstructions(DocumentPack docPack)
			: this(docPack, new FactoryStrategy.SaveInChunks())
		{
		}

		public DeliveryInstructions(DocumentPack docPack, ZGuid parentGuid)
			: this(docPack)
		{
			ParentGuid = parentGuid;
		}

		public DeliveryInstructions(DocumentPack docPack, FactoryStrategy factoryStrategy)
			: base(docPack?.BusinessObjectToLogAgainst, factoryStrategy.GetFactory())
		{
			fDocPack = docPack;
			fFactorySaveStrategy = factoryStrategy;
			RunDateTime = ZDateTime.UtcNow;
		}

		#region Constructor For IJsonSerializable

		internal DeliveryInstructions(DeliveryInstructionsJsonData data)
			: this()
		{
			CoverNote = data.CoverNote;
			IncludeCoverNote = data.IncludeCoverNote;
			PrinterDelivery.NumberOfCopies = data.NumberOfCopies;
			Language = data.Language;
		}

		#endregion

		#region User Notification Events

		#region Document Processing Start

		public ZBool DocPackAlreadyPrinted = false;
		public bool IsProceedingToDelivery;

		public event EventHandler StartDocProcessing;
		public void OnStartDocProcessing()
		{
			if (StartDocProcessing != null)
			{
				StartDocProcessing(this, EventArgs.Empty);
				DocPackAlreadyPrinted = false;
			}
		}

		#endregion

		#region DeliveryInstructionsHelper

		public DeliveryInstructionsHelper InstructionsHelper
		{
			get { return instructionsHelper ?? (instructionsHelper = DeliveryInstructionsHelper.New(this)); }
		}
		DeliveryInstructionsHelper instructionsHelper;
		#endregion

		#region Document Processing End

		public event EventHandler EndDocProcessing;
		public void OnEndDocProcessing()
		{
			if (EndDocProcessing != null)
			{
				EndDocProcessing(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Document Printed for Contact

		public delegate void PrintedForContact(int contactNumber, int totalContacts);
		public event PrintedForContact DocPrintedForContact;
		public void OnDocPrintedForContact(int contactNumber, int totalContacts)
		{
			if (DocPrintedForContact != null)
			{
				DocPrintedForContact(contactNumber, totalContacts);
			}
		}

		#endregion

		#region Document Printed within Pack

		public delegate void DocWithinPack(int docNumber, int totalDocsInPack);
		public event DocWithinPack DocPrintedWithinPack;
		public void OnDocPrintedWithinPack(int docNumber, int totalDocsInPack)
		{
			if (DocPrintedWithinPack != null)
			{
				DocPrintedWithinPack(docNumber, totalDocsInPack);
			}
		}

		#endregion

		#region Document Pack Print Start

		public delegate void DocPrinted(int docNumber);

		public event DocPrinted DocPackStarted;
		public void OnDocPackStarted(int docNumber)
		{
			if (DocPackStarted != null)
			{
				DocPackStarted(docNumber);
			}
		}

		#endregion

		#region Document Pack Print End

		public event DocPrinted DocPackPrinted;
		public void OnDocPackPrinted(int docNumber)
		{
			if (DocPackPrinted != null)
			{
				DocPackPrinted(docNumber);
				DocPackAlreadyPrinted = true;
			}
		}

		#endregion

		#endregion

		#region Properties

		public Action ActionForFailedToDeliver;

		[MaxLength(50)]
		public ZString DocumentPackTitle
		{
			get { return documentPackTitle.IsEmpty && DocPack.StmMenuCommand != null ? DocPack.StmMenuCommand.SU_MenuNameMultilingual : documentPackTitle; }
			set
			{
				CheckMaximumLength(DocumentPackTitleInfo, value);
				SetNonPersistentPropertyValue(DocumentPackTitleInfo, ref documentPackTitle, value);
			}
		}

		ZString documentPackTitle = ZString.Empty;

		public virtual ZBool IsDeliveringFormDocument => false;

		public ZPropertyInfo DocumentPackTitleInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentPackTitle));
			}
		}

		public ZDateTime RunDateTime { get; }

		#region IsDefaultDeliveryRecipient

		public bool IsDefaultDeliveryRecipient
		{
			get { return Recipients.Count == 0 || Recipients[0].Name.IsEmpty; }
		}

		#endregion

		#region ParentForm

		public IDeliverCapableForm ParentForm
		{
			get { return fParentForm; }
			set { fParentForm = value; }
		}

		IDeliverCapableForm fParentForm;

		#endregion

		#region DeliveryOptions

		public AllowedDeliveryOptions DeliveryOptions
		{
			get { return fDeliveryOptions; }
			set { fDeliveryOptions = value; }
		}

		AllowedDeliveryOptions fDeliveryOptions;

		#endregion

		#region Destination

		public DeliveryInstructionDestination Destination
		{
			get { return fDestination; }
			set { fDestination = value; }
		}

		DeliveryInstructionDestination fDestination = DeliveryInstructionDestination.None;

		#endregion

		#region AllowAutoDelivery

		public bool AllowAutoDelivery
		{
			get => fAllowAutoDelivery;
			set => fAllowAutoDelivery = value;
		}

		bool fAllowAutoDelivery = true;

		#endregion

		#region AllowSaveDefaults

		public bool AllowSaveDefaults
		{
			get { return fAllowSaveDefaults; }
			set { fAllowSaveDefaults = value; }
		}

		bool fAllowSaveDefaults;

		#endregion

		#region AllowPreview

		public ZBool AllowPreview
		{
			get { return fAllowPreview; }
			set { fAllowPreview = value; }
		}

		ZBool fAllowPreview = true;

		#endregion

		#region AllowModifyAndPreviewInExcel

		public ZBool AllowModifyAndPreviewInExcel
		{
			get { return allowModifyAndPreviewInExcel; }
			set { allowModifyAndPreviewInExcel = value; }
		}

		ZBool allowModifyAndPreviewInExcel = false;

		#endregion

		#region AllowPrint

		public ZBool AllowPrint
		{
			get { return fAllowPrint; }
			set { fAllowPrint = value; }
		}

		ZBool fAllowPrint = true;

		#endregion

		#region AllowEmail

		public ZBool AllowEmail
		{
			get { return fAllowEmail; }
			set { fAllowEmail = value; }
		}

		ZBool fAllowEmail = true;

		#endregion

		#region AllowFax

		public ZBool AllowFax
		{
			get { return fAllowFax; }
			set { fAllowFax = value; }
		}

		ZBool fAllowFax = true;

		#endregion

		#region AllowModify

		public virtual ZBool AllowModify
		{
			get { return allowModify; }
			set { allowModify = value; }
		}

		ZBool allowModify = true;

		#endregion

		#region SendToEDocs

		public ZBool SendToEDocs
		{
			get { return sendToEDocs; }
			set { sendToEDocs = value; }
		}

		ZBool sendToEDocs = false;

		#endregion

		#region IsDraft

		public ZBool IsDraft
		{
			get { return fIsDraft; }
			set
			{
				fIsDraft = value;
				IsDraftInfo.RefreshBinding();
			}
		}

		ZBool fIsDraft = ZBool.False;

		public ZPropertyInfo IsDraftInfo
		{
			get { return GetZPropertyInfo(nameof(IsDraft)); }
		}

		public bool IsDraft_ReadOnly { get; set; }

		#endregion

		#region DocumentPackIndexer

		public ZInt DocumentPackIndexer
		{
			get { return fDocumentPackIndexer; }
			set { fDocumentPackIndexer = value; }
		}

		ZInt fDocumentPackIndexer = 0;

		#endregion

		#region Language

		[List("Languages")]
		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(5)]
		public ZString Language
		{
			get
			{
				if (!LanguageHasBeenSet)
				{
					var defaultLanguageOrder = DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.Value;
					if (defaultLanguageOrder != null && defaultLanguageOrder.Count > 0 && DocPack != null)
					{
						var newLanguage = TemplateLanguageSelector.SelectDefaultLanguage(DocPack.OrgHeaderContact, defaultLanguageOrder, DocPack.DocumentGroup, DocPack.DocumentSupporter?.TransportMode, DocumentMenuItemPK);
						if (!string.IsNullOrEmpty(newLanguage))
						{
							HasDocumentDeliveryDefaultLanguage = true;
							return newLanguage;
						}
					}
				}

				return language;
			}
			set
			{
				var erroredNames = Recipients
					.Where((recipient, i) =>
					{
						var recipientContact = (DocDeliveryContact)recipient;
						var defaultSalutationLength = DefaultSalutationProvider
							.GetDefaultSalutation(value, recipientContact.Contact == null ? ZString.Empty : recipientContact.Contact.OC_Gender)
							.Replace(Core.Constants.SalutationMacros.Name, recipientContact.SystemDefaultContactName == null ? ZString.Empty.ToString() : recipientContact.SystemDefaultContactName.ToString(value))
							.Length;
						return defaultSalutationLength > recipientContact.SalutationInfo.MaxLength;
					})
					.Select(recipient => ((DocDeliveryContact)recipient).SystemDefaultContactName)
					.Aggregate("", (resultString, recipient) => resultString + "\r\n  - " + recipient);

				if (!string.IsNullOrEmpty(erroredNames))
				{
					Globals.Message.ShowError(Res.GetString("85271408-fc71-4be0-b71f-10ab01e46014", "Language could not be set because the following recipient's names would exceed the length limit of the default salutation:{0}\r\nChange the recipients names or choose a valid language.", erroredNames));
					return;
				}

				if (value == Res.DefaultLanguage)
				{
					value = DataRegistry.Instance.EnglishSpelling;
				}

				var languageHasChanged = language != value;

				if ((languageHasChanged || !LanguageHasBeenSet) && Languages.ContainsCode(value))
				{
					LanguageHasBeenSet = true;
					language = value;

					var isReport = false;
					var report = DocPack?.GetFirstReport();
					if (report != null)
					{
						isReport = report.Style == Report.Styles.Report;
					}

					if (languageHasChanged && !isReport)
					{
						Recipients.SetDeliveryLanguage(language);
						OnLanguageChanged();
					}
				}
				LanguageInfo.RefreshBinding();
			}
		}

		public bool Language_ReadOnly { get; internal set; }

		public bool LanguageHasBeenSet
		{
			get;
			private set;
		}

		public bool HasDocumentDeliveryDefaultLanguage
		{
			get;
			private set;
		}

		void OnLanguageChanged()
		{
			UpdateLanguageWarning();
			officialRecipient = null;
			UpdateSystemDefaultContactNameAndSalutation();
		}

		ZString language = DataRegistry.Instance.EnglishSpelling;

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(nameof(Language)); }
		}

		public AvailableDocBuilderLanguageList Languages
		{
			get { return languages ?? (languages = new AvailableDocBuilderLanguageList(Factory)); }
		}

		AvailableDocBuilderLanguageList languages;

		void UpdateLanguageWarning()
		{
			LanguageInfo.ClearAllNotifications();
			if (!Res.IsEnglish(language))
			{
				LanguageLicenceCheckpoint licence;
				Env.Licence.LanguagePackLookup.TryGetValue(language, out licence);
				if (licence != null)
				{
					LanguageInfo.AddWarning(Res.GetString("f07e99eb-b1a9-464c-b67d-165c85d18aa4", "You have selected to issue this document translated into {0}.", Languages.GetDescriptionFromCode(language)));
				}
			}
		}

		void UpdateSystemDefaultContactNameAndSalutation()
		{
			foreach (DocDeliveryContact recipient in Recipients.ToArray())
			{
				if (recipient != null)
				{
					if (recipient.Contacts != null)
					{
						var defaultContacts = recipient.Contacts.Cast<OrgContact>().Where(r => r.IsSystemDefaultContact);
						foreach (OrgContact defaultContact in defaultContacts)
						{
							if (defaultContact != null)
							{
								if (defaultContact.SystemDefaultContactName != null)
								{
									defaultContact.OC_ContactName = defaultContact.SystemDefaultContactName.ToString(language);
								}
								defaultContact.OC_Salutation = Enterprise.Core.Constants.DefaultSalutations.DefaultSalutation.ToString(language);
							}
						}
					}
					if (recipient.SystemDefaultContactName != null)
					{
						recipient.Name = recipient.SystemDefaultContactName.ToString(language);
						recipient.Salutation = DefaultSalutationProvider.GetDefaultSalutation(language, recipient.Contact == null ? ZString.Empty : recipient.Contact.OC_Gender)
							.Replace(Core.Constants.SalutationMacros.Name, recipient.Name);
					}
				}
			}
		}

		#endregion

		#region Page Ranges

		public Report ReportToSpecifyPageRanges => DocPack?.OfType<Report>().FirstOrDefault(report => report.CanSpecifyPageRanges);

		public bool PageRangesSpecified
		{
			get => ReportToSpecifyPageRanges?.PageRangesSpecified ?? false;
			set
			{
				if (ReportToSpecifyPageRanges != null)
				{
					ReportToSpecifyPageRanges.PageRangesSpecified = value;
					if (!value)
					{
						SpecifiedPageRangesText = ZString.Empty;
					}
				}
			}
		}

		public int DataSourceRowCountIfPageRangesSpecifiable => ReportToSpecifyPageRanges?.DataRowSourceRowCountIfPageRangesSpecifiable ?? 0;

		public ZString SpecifiedPageRangesText
		{
			get { return specifiedPageRangesText; }
			set
			{
				if (SetNonPersistentPropertyValue(SpecifiedPageRangesTextInfo, ref specifiedPageRangesText, value) && !IsValidationSuspended)
				{
					ValidateSpecifiedPageRangesText();
				}
			}
		}
		ZString specifiedPageRangesText;

		public ZPropertyInfo SpecifiedPageRangesTextInfo => GetZPropertyInfo(nameof(SpecifiedPageRangesText), "SpecifiedPageRanges");

		void ValidateSpecifiedPageRangesText()
		{
			if (ReportToSpecifyPageRanges != null)
			{
				var indexes = new List<int>();
				SpecifiedPageRangesTextInfo.ClearAllNotifications();
				ReportToSpecifyPageRanges.UpdateSpecifiedDataRowSource(null);
				if (!SpecifiedPageRangesText.IsEmpty)
				{
					var pageRanges = SpecifiedPageRangesText.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var pageRange in pageRanges)
					{
						var matched = subPageRangesRegex.Match(pageRange);
						if (matched.Success)
						{
							var start = int.Parse(matched.Groups["start"].Value);
							var end = int.Parse(matched.Groups["end"].Value);
							if (start > 0 && end <= DataSourceRowCountIfPageRangesSpecifiable && start < end)
							{
								indexes.AddRange(Enumerable.Range(start, end - start + 1));
							}
							else
							{
								SpecifiedPageRangesTextInfo.AddError(SpecifiedPageRangesError);
								return;
							}
						}
						else
						{
							if (ZInt.TryParse(pageRange, out ZInt index) && index > 0 && index <= DataSourceRowCountIfPageRangesSpecifiable)
							{
								indexes.Add(index);
							}
							else
							{
								SpecifiedPageRangesTextInfo.AddError(SpecifiedPageRangesError);
								return;
							}
						}
					}
					ReportToSpecifyPageRanges.UpdateSpecifiedDataRowSource(indexes.Distinct().OrderBy(index => index).ToArray());
				}
			}
		}

		public ResourceString SpecifiedPageRangesError => ResString.GetMultilingualString("CF56B4E3-9B61-439B-9CB4-339CF8951D1A", @"Please enter valid page ranges.
To print a range of pages, e.g pages 2 through to 9 enter as follows 2 - 9. To print specific pages only, enter as follows 2, 5, 9.");

		static readonly Regex subPageRangesRegex = new Regex("^(?<start>\\d+)-(?<end>\\d+)$", RegexOptions.Compiled);

		#endregion

		#region MultipleDocumentPacks

		public ZBool MultipleDocumentPacks
		{
			get { return DocumentPackCount > 1; }
		}

		public ZInt DocumentPackCount
		{
			get { return documentPackCount; }
			set
			{
				SetNonPersistentPropertyValue(DocumentPackCountInfo, ref documentPackCount, value);
				if (documentPackCount > 1)
				{
					SetDefaultsForMultiDocPack();
				}
				else
				{
					Destination = DeliveryInstructionDestination.TakenFromContact;
				}
			}
		}

		void SetDefaultsForMultiDocPack()
		{
			AutoDeliverMultiDocPack = true;
			PrintMultiDocPack = false;
		}

		public ZPropertyInfo DocumentPackCountInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentPackCount)); }
		}

		public ZString DocumentPackCountAsString
		{
			get { return fDocumentPackCountAsString.IsEmpty ? (ZString)DocumentPackCount.ToString() : fDocumentPackCountAsString; }
			set
			{
				fDocumentPackCountAsString = value;
				try
				{
					DocumentPackCount = ZInt.Parse(value);
					fDocumentPackCountAsString = ZString.Empty;
				}
				catch (ArgumentOutOfRangeException)
				{
					DocumentPackCount = 0;
				}
			}
		}

		public ZPropertyInfo DocumentPackCountAsStringInfo
		{
			get { return DocumentPackCountInfo; }
		}

		ZInt documentPackCount;
		ZString fDocumentPackCountAsString;

		#endregion

		#region BackgroundDelivery

		public ZBool BackgroundDelivery
		{
			get { return backgroundDelivery; }
			set { SetNonPersistentPropertyValue(BackgroundDeliveryInfo, ref backgroundDelivery, value); }
		}

		ZBool backgroundDelivery;

		public ZPropertyInfo BackgroundDeliveryInfo
		{
			get { return GetZPropertyInfo(nameof(BackgroundDelivery)); }
		}

		#endregion

		#region OutputDirectory

		public ZString OutputDirectory
		{
			get { return fOutputDirectory; }
			set { fOutputDirectory = value; }
		}

		ZString fOutputDirectory;

		#endregion

		#region OutputFileFormat
		public OutputFormatType OutputFormatOverride
		{
			get;
			set;
		}
		#endregion

		#region CoverNote

		public ZBool IncludeCoverNote
		{
			get { return includeCoverNote; }
			set
			{
				SetNonPersistentPropertyValue(IncludeCoverNoteInfo, ref includeCoverNote, value);
				if (!includeCoverNote)
				{
					CoverNote = "";
				}
				CoverNoteInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IncludeCoverNoteInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeCoverNote)); }
		}

		public ZString CoverNote
		{
			get { return coverNote; }
			set
			{
				if (coverNote != value)
				{
					CheckMaximumLength(CoverNoteInfo, value);
					SetNonPersistentPropertyValue(CoverNoteInfo, ref coverNote, value);
				}
			}
		}

		public ZPropertyInfo CoverNoteInfo
		{
			get { return GetZPropertyInfo(nameof(CoverNote)); }
		}

		protected bool CoverNote_ReadOnly
		{
			get { return !IncludeCoverNote; }
		}

		ZBool includeCoverNote;
		ZString coverNote;

		#endregion

		#region AutoDeliverMultiDocPack

		public ZBool AutoDeliverMultiDocPack
		{
			get { return autoDeliverMultiDocPack; }
			set
			{
				SetNonPersistentPropertyValue(AutoDeliverMultiDocPackInfo, ref autoDeliverMultiDocPack, value);
				if (value)
				{
					Destination = DeliveryInstructionDestination.Auto;
				}
			}
		}

		public ZPropertyInfo AutoDeliverMultiDocPackInfo
		{
			get { return GetZPropertyInfo(nameof(AutoDeliverMultiDocPack)); }
		}

		ZBool autoDeliverMultiDocPack;

		#endregion

		#region PrintMultiDocPack

		public ZBool PrintMultiDocPack
		{
			get { return printMultiDocPack; }
			set
			{
				SetNonPersistentPropertyValue(PrintMultiDocPackInfo, ref printMultiDocPack, value);
				if (value)
				{
					Destination = DeliveryInstructionDestination.Print;
				}
			}
		}

		public ZPropertyInfo PrintMultiDocPackInfo
		{
			get { return GetZPropertyInfo(nameof(PrintMultiDocPack)); }
		}

		ZBool printMultiDocPack;

		#endregion

		#region Has Printed Documents

		public bool HasPrintedDocuments
		{
			get
			{
				if (!PrintMultiDocPack)
				{
					foreach (DocDeliveryContact contact in Recipients)
					{
						if (contact.DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
						{
							return true;
						}
					}
				}

				return PrintMultiDocPack;
			}
		}

		#endregion

		#region Attachment Type Disabled

		public bool AttachmentTypeDisabled
		{
			get { return attachmentTypeDisabled; }
			set
			{
				attachmentTypeDisabled = value;
				Recipients.DisableAttachmentType(value);
			}
		}
		bool attachmentTypeDisabled;
		#endregion

		#region TIF Attachments Only

		public bool TIFAttachmentsOnly
		{
			get { return fTIFAttachmentsOnly; }
			set
			{
				fTIFAttachmentsOnly = value;

				CodeDescriptionPairList list = OrgCodeLists.AttachmentType_List;
				if (value)
				{
					list = GenerateAttachmentTypeList(OrgConstants.AttachmentType.TIF);
				}

				Recipients.OverrideAttachmentTypeListOnChildren(list);
			}
		}

		bool fTIFAttachmentsOnly;

		CodeDescriptionPairList GenerateAttachmentTypeList(params string[] types)
		{
			CodeDescriptionPairList fullList = OrgCodeLists.AttachmentType_List;
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair pair in fullList)
			{
				if (Array.IndexOf(types, pair.Code) != -1)
				{
					result.Add(pair);
				}
			}
			return result;
		}

		#endregion

		#region Override Notify Modes

		public void OverrideNotifyModesList(CodeDescriptionPairList modes)
		{
			Recipients.OverrideNotifyModeTypeListOnChildren(modes);
		}

		#endregion

		#region DeliveryMethod

		internal DeliveryMethod DeliveryMethod
		{
			get;
			set;
		}

		#endregion

		#region MemoryDeliveryMethod

		List<(string fileName, string docType, byte[] imageBytes)> outputForMemoryDeliveryMethod;

		public List<(string fileName, string docType, byte[] imageBytes)> OutputForMemoryDeliveryMethod
		{
			get
			{
				if (outputForMemoryDeliveryMethod == null)
				{
					outputForMemoryDeliveryMethod = new List<(string fileName, string docType, byte[] imageBytes)>();
				}

				return outputForMemoryDeliveryMethod;
			}
		}

		#endregion

		#region OverriddenValueForOrgLookupFilter

		internal ZGuid OverriddenValueForOrgLookupFilter { get; set; }

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (Recipients != null)
			{
				foreach (DocDeliveryContact contact in Recipients.ToArray())
				{
					if (contact.Factory == Factory) // Validation on proprty info can be run only once and messages cannot be overridden
					{
						ValidateAgainstNotDeliverableDocuments(contact);
						ValidateAgainstAttachmentType(contact);
					}
					contact.Validation.ValidateAll();
					if (contact.Factory != Factory) // Validation messages were overridden by ValidateAll()
					{
						ValidateAgainstNotDeliverableDocuments(contact);
						ValidateAgainstAttachmentType(contact);
					}
				}
			}
			InitialisePrinterDelivery();
		}

		void ValidateAgainstAttachmentType(DocDeliveryContact contact)
		{
			if (DocPack != null)
			{
				var reports = DocPack.OfType<Report>();
				foreach (var report in reports)
				{
					var fileFormat = report.XlInterface?.GetExtensionForExcelFromFile();
					if (!string.IsNullOrEmpty(fileFormat))
					{
						var hasChart = report.Analyser is { Config: not null } && report.HasChart;
						var disableXLSXExport = report.Analyser is { Config.DisableXLSXExport: true };
						var errorMessages = new List<string>();

						if (hasChart)
						{
							if (fileFormat == AttachmentTypeList.Codes.Xls && contact.AttachmentType == AttachmentTypeList.Codes.Xlsx)
							{
								errorMessages.Add(Res.GetString("D05C6E69-25C9-43A5-A6DC-7C2F2304CD46", "{0} template that contains chart objects can't be exported as {1} files.", AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx));
							}

							if (fileFormat == AttachmentTypeList.Codes.Xlsx && contact.AttachmentType == AttachmentTypeList.Codes.Xls)
							{
								errorMessages.Add(Res.GetString("EA108085-828F-48AF-B6F9-C2703D41B701", "{0} template that contains chart objects can't be exported as {1} files.", AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xls));
							}
						}

						if (contact.AttachmentType == AttachmentTypeList.Codes.Xlsx && disableXLSXExport)
						{
							errorMessages.Add(Res.GetString("225025C3-4E1D-4FE6-BB3F-4251B53CCD68", "Template that contains the parameter '{0}' can't be exported as {1} files.", "DisableXlsxExport", AttachmentTypeList.Codes.Xlsx));
						}

						if (errorMessages.Count > 0)
						{
							AddContactPropertyError(contact.AttachmentTypeInfo, errorMessages, contact.Validation.ValidateAttachmentType);
						}
					}
				}
			}
		}

		void ValidateAgainstNotDeliverableDocuments(DocDeliveryContact contact)
		{
			if (contact.OrgHeader != null &&
				contact.OrgHeader.SuppressedDocumentsIncludeDummyContact.Count > 0)
			{
				foreach (IDeliverable deliverable in DeliverablesToBePrinted)
				{
					StmMenuItem menuItem = deliverable.MenuItem;
					if (menuItem != null)
					{
						IList<string> documentNames = new List<string>();

						foreach (OrgDocument document in contact.OrgHeader.SuppressedDocumentsIncludeDummyContact.ToArray()) //Use ToArray() to prevent changing collection during iteration.
						{
							if (!document.OD_DocumentGroup.IsEmpty && document.OD_DocumentGroup == menuItem.SU_ContactType)
							{
								documentNames.Add(Res.GetString("7c59c9dd-ca66-49a4-96a3-c7a1189baf88",
									"Document group '{0}' is marked as Never to Deliver to this organization.", document.OD_DocumentGroup));
							}
							else if (document.OD_SU_MenuItem == menuItem.PK ||
								document.MenuItem != null && document.MenuItem.DocumentId == menuItem.DocumentId)
							{
								documentNames.Add(Res.GetString("97f45c52-cb56-4f51-ae73-e952b9b90fac",
									"Document '{0}' is marked as Never to Deliver to this organization.", deliverable.DocumentName));
							}
						}

						if (documentNames.Count > 0)
						{
							AddContactPropertyError(contact.OrgHeaderPKInfo, documentNames, contact.Validation.ValidateOrgHeaderPK);
						}
					}
				}
			}
		}

		void AddContactPropertyError(ZPropertyInfo propertyInfo, IEnumerable<string> messages, Action validateAction)
		{
			if (propertyInfo == null || messages == null)
			{
				return;
			}
			try
			{
				propertyInfo.AdditionalValidation += PropertyValidation;
				validateAction();
			}
			finally
			{
				propertyInfo.AdditionalValidation -= PropertyValidation;
			}

			void PropertyValidation()
			{
				foreach (var message in messages)
				{
					propertyInfo.AddError(message);
				}
			}
		}

		#endregion

		#region Related Objects

		#region Delivery Group

		public List<StmDeliveryGroup> DeliveryGroups
		{
			get
			{
				if (fDeliveryGroups == null)
				{
					fDeliveryGroups = new List<StmDeliveryGroup>();
				}

				if (fDeliveryGroups.Count == 0)
				{
					fDeliveryGroups.Add(Factory.New<StmDeliveryGroup>());
				}
				return fDeliveryGroups;
			}
		}
		List<StmDeliveryGroup> fDeliveryGroups;

		public void SetAndSaveDeliveryGroupSubjectLine(DocumentPack documentPack, PrintTask.ReportSubjectLineMapping mapping)
		{
			StmDeliveryGroup deliveryGroup = DeliveryGroups[0];
			if (deliveryGroup.SB_EmailSubjectLine != "" && !DeliverDocumentsInOneEmail)
			{
				deliveryGroup = Factory.New<StmDeliveryGroup>();
				DeliveryGroups.Add(deliveryGroup);
			}

			deliveryGroup.SB_EmailSubjectLine = mapping.SubjectLine;

			if (documentPack.Parent != null)
			{
				var parentCommand = documentPack.Parent.ParentMenuCommand;
				if (parentCommand != null && parentCommand.SU_IsDocPack && parentCommand.SU_IsZippedDocPack)
				{
					deliveryGroup.SB_IsZippedDocPack = true;
				}
			}

			if (mapping.ReportWithDeliverables != null)
			{
				if (mapping.ReportWithDeliverables.Report != null)
				{
					((IDeliverable)mapping.ReportWithDeliverables.Report).DeliveryGroupID = deliveryGroup.PK;
				}
				if (mapping.ReportWithDeliverables.Deliverables != null)
				{
					foreach (IDeliverable eDoc in mapping.ReportWithDeliverables.Deliverables)
					{
						eDoc.DeliveryGroupID = deliveryGroup.PK;
					}
				}
			}
			FactorySaveStrategy.SaveChunk(deliveryGroup.Factory);
		}

		/// <summary>
		/// Controls how the delivery method works with the BusinessObjectFactory.
		/// </summary>
		public FactoryStrategy FactorySaveStrategy
		{
			get { return fFactorySaveStrategy; }
		}

		readonly FactoryStrategy fFactorySaveStrategy;

		#endregion

		#region Recipients

		public DocDeliveryContactCollection GetRecipients(IStmMenuItem menuItem)
		{
			return GetRecipientsCore(menuItem);
		}

		protected virtual DocDeliveryContactCollection GetRecipientsCore(IStmMenuItem menuItem)
		{
			var result = GetNewDocDeliveryContactCollection(menuItem);

			if (DocPackIsSet)
			{
				if (AllowAutoDelivery)
				{
					var autoDelivery = DocPack.AutoDocumentDelivery ?? new DocAutoDelivery();

					if (LanguageHasBeenSet)
					{
						autoDelivery.SetDeliveryLanguage(Language);
					}

					result = autoDelivery.GetDeliveryContactsForDocPack(menuItem, DocPack.DocumentSupporter, DocPack.DocumentGroup, DocPack.Parent?.ParentMenuCommand);
				}
				else
				{
					result.Add(GetNewPrintDestinationContact(result));
				}
			}

			var report = DocPack?.GetFirstReport();

			var defaultEmailFromAddress = string.Empty;
			if (menuItem != null)
			{
				defaultEmailFromAddress = menuItem.SU_EmailSenderOverride;
			}
			else if (report != null)
			{
				defaultEmailFromAddress = report.GetEmailFromAddress();
			}

			if (!string.IsNullOrEmpty(defaultEmailFromAddress))
			{
				result.DefaultEmailFromAddress = defaultEmailFromAddress;

				foreach (DocDeliveryContact contact in result)
				{
					contact.DefaultEmailFromAddress = defaultEmailFromAddress;
				}
			}

			result.Deliverables = DocPack;

			return result;
		}

		protected virtual DocDeliveryContactCollection GetNewDocDeliveryContactCollection(IStmMenuItem menuItem)
		{
			return new DocDeliveryContactCollection(menuItem, null, Factory);
		}

		DocDeliveryContact GetNewPrintDestinationContact(DocDeliveryContactCollection collection)
		{
			var result = collection.AddNew();
			result.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			return result;
		}

		public DocDeliveryContactCollection Recipients
		{
			get
			{
				if (fRecipients == null)
				{
					fRecipients = GetRecipients(DocPack.StmMenuCommand);
					RegisterEditableChildObject(fRecipients);

					((IBindingList)fRecipients).ListChanged += new ListChangedEventHandler(DeliveryInstructions_ListChanged);
				}

				return fRecipients;
			}
		}
		DocDeliveryContactCollection fRecipients;

		public void ClearRecipients()
		{
			Recipients.RemoveAndDeleteAll();
			fRecipients = null;
		}

		public DocDeliveryContact OfficialRecipient
		{
			get { return officialRecipient ?? (officialRecipient = GetOfficialRecipient()); }
		}
		DocDeliveryContact officialRecipient;

#if DEBUG
		internal void SetOfficialRecipientForTesting(DocDeliveryContact officialContact)
		{
			officialRecipient = officialContact;
		}
#endif

		DocDeliveryContact GetOfficialRecipient()
		{
			DocDeliveryContact result = null;

			IStmMenuItem menuItem = DocPack.StmMenuCommand;
			DocDeliveryContactCollection recipients = GetRecipients(menuItem);
			if (recipients.Count == 1)
			{
				result = recipients[0];
			}
			else if (recipients.Count > 1)
			{
				ZString menuItemContactType = menuItem != null ? menuItem.SU_ContactType : ZString.Empty;

				int bestScore = -1;
				foreach (DocDeliveryContact recipient in recipients)
				{
					OrgDocument documentDeliveryType = recipient.DocumentDeliveryType;
					int score = 0;

					if (documentDeliveryType != null)
					{
						if (documentDeliveryType.OD_DefaultContact)
						{
							score += 1;
						}

						if (!menuItemContactType.IsEmpty && documentDeliveryType.OD_DocumentGroup == menuItemContactType)
						{
							score += 2;
						}
					}

					if (score > bestScore)
					{
						bestScore = score;
						result = recipient;
					}
				}
			}

			if (result != null)
			{
				result.DeliveryLanguage = Language;
			}
			return result;
		}

		#endregion

		#region Document Pack

		[BusinessObjectTestExclude]
		public DocumentPack DocPack
		{
			get { return fDocPack; }
			set { fDocPack = value; }
		}

		public bool DocPackIsSet
		{
			get { return fDocPack != DocumentPack.EmptyPack; }
		}

		DocumentPack fDocPack;

		public bool AllowPreviewIfDocument => Env.Security.PreviewDocumentButton.IsAllowed && (IsDocument || MultipleDocumentPacks || PackContainsDocuments);

		public bool IsDocument => !IsDeliveringFormDocument && DocPack.StmMenuCommand is DocumentCommand;

		bool PackContainsDocuments => DocPack.GetFirstReport() != null && DocPack.GetFirstReport().Style == Report.Styles.Document;

		#endregion

		#region Deliverables to be Printed

		public DeliverableCollectionView DeliverablesToBePrinted
		{
			get
			{
				if (deliverablesToBePrinted == null)
				{
					AddOtherEDocsToAttachIfNeeded();
					deliverablesToBePrinted = GetNewDeliverableCollectionView();
				}
				return deliverablesToBePrinted;
			}
		}
		DeliverableCollectionView deliverablesToBePrinted;

		protected virtual DeliverableCollectionView GetNewDeliverableCollectionView()
		{
			return DocPackIsSet
				? new DeliverableCollectionView(DocPack, Recipients)
				: new DeliverableCollectionView(new DocumentPack(), new DocDeliveryContactCollection(Factory));
		}
		IDeliverableCollection DeliverableCollection => (IDeliverableCollection)DeliverablesToBePrinted.CollectionToFilter;
		DocDeliveryContactCollection DeliverableRecipients => DeliverablesToBePrinted.Recipients;

		public void DeliveryInstructions_ListChanged(object sender, ListChangedEventArgs e)
		{
			DeliverablesToBePrinted.Rebuild();
		}

		internal void AddOtherEDocsToAttachIfNeeded(bool isRebuild = false)
		{
			if (!IsDeliveringFormDocument &&
				!MultipleDocumentPacks &&
				DocPackIsSet &&
				DocPack.OtherEDocsToAttach.Any())
			{
				var otherEDocsGroups = DocPack.OtherEDocsToAttach.Where(d => !d.ShouldPrintByDefault).GroupBy(d => d.JobNumber);
				if (otherEDocsGroups.Any())
				{
					foreach (var deliverables in otherEDocsGroups)
					{
						deliverables.ForEach(d =>
						{
							if (!isRebuild)
							{
								d.IncludedInPrint = false;
							}
							if (!DocPack.Contains(d as BusinessObject))
							{
								DocPack.Add(d);
							}
						});
					}
				}
			}
		}

		#endregion

		#region Documents to be delivered

		public DocumentCollectionView DocumentsToBeDelivered
		{
			get { return documentsToBeDelivered ?? (documentsToBeDelivered = new DocumentCollectionView(DeliverableCollection, DeliverableRecipients)); }
		}
		DocumentCollectionView documentsToBeDelivered;

		#endregion

		#region EDocs to be delivered

		public EDocCollectionView EDocsToBeDelivered
		{
			get
			{
				if (eDocsToBeDelivered == null)
				{
					eDocsToBeDelivered = new EDocCollectionView(DeliverableCollection, DeliverableRecipients);
				}
				RegisterEditableChildObject(eDocsToBeDelivered);
				eDocsToBeDelivered?.Sort(new SortInfo(DeliverableConstants.Index, ListSortDirection.Ascending));
				return eDocsToBeDelivered;
			}
		}
		EDocCollectionView eDocsToBeDelivered;

		#endregion

		#region Reset EDocs

		bool IsInPreview => Destination == DeliveryInstructionDestination.Preview || Destination == DeliveryInstructionDestination.DocConfigPreview;

		public IDisposable ResetEDocsToBeDeliveredIfNeeded()
		{
			return new DisposableAction(() =>
			{
				if (!IsInPreview)
				{
					EDocsToBeDelivered.ResetEDocs();
				}
			});
		}

		#endregion

		#region Printer Delivery Details

		public virtual DocDeliveryPrintDetails PrinterDelivery
		{
			get
			{
				InitialisePrinterDelivery();

				return fPrinterDelivery;
			}
			set
			{
				fPrinterDelivery = value;
			}
		}

		void InitialisePrinterDelivery()
		{
			if (fPrinterDelivery == null)
			{
				fPrinterDelivery = new DocDeliveryPrintDetails(this);
				RegisterEditableChildObject(fPrinterDelivery);
			}
		}

		DocDeliveryPrintDetails fPrinterDelivery;

		#endregion

		#endregion

		#region Delivery Group

		/// <summary>
		/// Disk, preview and cancelled statuses don't need a delivery group.
		/// </summary>
		public bool UsesDeliveryGroup
		{
			get
			{
				return Destination == DeliveryInstructionDestination.Auto ||
					Destination == DeliveryInstructionDestination.DocManager ||
					Destination == DeliveryInstructionDestination.Print ||
					Destination == DeliveryInstructionDestination.Sms ||
					Destination == DeliveryInstructionDestination.TakenFromContact;
			}
		}

		#endregion

		#region Parent GUID
		public ZGuid ParentGuid { get; private set; }
		#endregion

		public override string ToString()
		{
			return "DeliveryInstruction";
		}

		#region Cloning

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var json = JsonConverterHelper.Serialize(this);

			var result = JsonConverterHelper.Deserialize<DeliveryInstructions>(json);
			result.DocPack = DocPack.Clone();
			result.Language = Language;
			result.ClearRecipients();
			return result;
		}

		protected sealed override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region OperationalAction DeliverDocumentsInOneEmail

		long attachmentFileSize;
		public bool DeliverDocumentsInOneEmail { get; private set; }
		public bool ContinuedWhenDocumentsFileSizeGreaterThanSizeLimit { get; private set; } = true;
		bool isWarningShowed;
		bool isSingleAttachment;

		internal void SumAttachmentFileSize(DeliveryInfo info)
		{
			if (DeliverDocumentsInOneEmail)
			{
				attachmentFileSize += info.FileContents.Length;
			}
		}

		internal void MarkPrintJobEDocsProcessedIfNeeded(StmPrintJob printJob)
		{
			if (DeliverDocumentsInOneEmail && isSingleAttachment)
			{
				printJob.SP_EDocsProcessed = true;
			}
		}

		public IDisposable EnableOperationalActionDeliverDocumentsInOneEmail(bool isSingleAttachment, DocumentCommand command)
		{
			DeliverDocumentsInOneEmail = true;
			this.isSingleAttachment = isSingleAttachment;

			return new DisposableAction(() =>
			{
				DeliverDocumentsInOneEmail = false;
			});
		}

		internal bool ShouldContinueIfDocumentsFileSizeGreaterThanSizeLimitInBytes()
		{
			if (!isWarningShowed && DeliverDocumentsInOneEmail && attachmentFileSize > RegistrySizeLimitInBytes)
			{
				var warningMessage = Res.GetString("A4186800-CEDD-4A97-B854-C30C15A08ED2", "Email attachment file size has exceeded the attachment limit size, do you still want to continue?");
				var caption = Res.GetString("29A9B8BC-C807-451B-A41A-B0FA658D5F4E", "Deliver Documents in One Email");
				var messageResult = Globals.Message.Show(warningMessage, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning, ZDialogResult.Yes);
				isWarningShowed = true;
				ContinuedWhenDocumentsFileSizeGreaterThanSizeLimit = messageResult == ZDialogResult.Yes;
			}
			return ContinuedWhenDocumentsFileSizeGreaterThanSizeLimit;
		}

		internal void SetDocumentPackDeliverDocumentsInOneEmailInfo(DocumentPack pack)
		{
			pack.DeliverDocumentsInOneEmail = DeliverDocumentsInOneEmail;
			pack.IsSingleAttachment = isSingleAttachment;
		}

		long registrySizeLimitInBytes;
		long RegistrySizeLimitInBytes
		{
			get
			{
				if (registrySizeLimitInBytes == 0)
				{
					registrySizeLimitInBytes = SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value * 1024 * 1024;
				}
				return registrySizeLimitInBytes;
			}
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new DeliveryInstructionsJsonData
			{
				CoverNote = CoverNote,
				Language = Language,
				IncludeCoverNote = IncludeCoverNote,
				NumberOfCopies = PrinterDelivery.NumberOfCopies
			};

		#endregion

		internal void ShowPrinterSelectionUI()
		{
			PrintTaskUIProvider.ShowPrinterSelectionUI(this);
		}

		IPrintTaskUIProvider PrintTaskUIProvider
		{
			get { return printTaskUIProvider ?? (printTaskUIProvider = PrintTaskUIProviderFactory.Create()); }
		}

		IPrintTaskUIProvider printTaskUIProvider;

		#region ICustomTextTemplateContext Members

		public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return DocPack.DocumentSupporter != null && DocPack.DocumentSupporter.BusinessObject != null ?
						new BusinessObject[] { this, DocPack.DocumentSupporter.BusinessObject } :
						new BusinessObject[] { this };
		}

		public string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return (DocPack.DocumentSupporter != null ? DocPack.DocumentSupporter.BusinessContext.ToString() : DocumentPackTitle.ToString()) + "." + bindingMemberInfo.BindingField;
		}

		#endregion

		public bool ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows { get; set; }

		public bool ExcludeDocumentsWhichContainNoBusinessObjectDataRows { get; set; }

		public void ResetCachedReports()
		{
			foreach (var report in DocPack.OfType<Report>())
			{
				report.ResetCachedExcelFile();
			}
		}

		public BusinessObject[] GetRelatedBusinessObjectsForEmailSubject(DocDeliveryContact contact)
		{
			return new[]
			{
				DocPack?.Parent?.MostTopLevelBusinessObject,
				DocPack?.DocumentSupporter?.BusinessObject,
				contact,
				contact.Contact,
				contact.DocumentDeliveryType,
				contact.OrgHeader,
				this,
				DocPack?.StmMenuCommand
			};
		}

		#region Get DeliveryInfo By File Path

		public DeliveryInfo GetDeliveryInfoByFilePath(string filePath)
		{
			return DeliveryMethod?.Infos?.FirstOrDefault(x => x.FilePath == filePath);
		}

		#endregion
	}
}
