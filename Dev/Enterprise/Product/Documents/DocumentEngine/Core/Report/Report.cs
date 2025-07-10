using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentWrappersCore;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using static Enterprise.DocumentEngine.Visualisation.VisualizerNote;
using DataContext = Enterprise.Core.Constants.DataContext;
using SortOrder = Enterprise.DocumentEngine.RuntimeOptions.SortOrder;

namespace Enterprise.DocumentEngine
{
	#region IWebReport Interface

	public interface IWebReport
	{
		CodeDescriptionPairList ColumnLayoutList { get; }
		ZGuid CurrentOrgPK { get; set; }
		ZString SelectedColumnLayout { get; set; }
		ZPropertyInfo SelectedColumnLayoutInfo { get; }
		bool IsOnlyCompanyDefaultLayout { get; }
	}

	#endregion

	public partial class Report : NonPersistentBusinessObject,
		IDeliverable,
		IWebReport,
		IObsoleteValidation,
		IDisposable,
		IDeliveryEmailAttachment,
		IJsonSerializable
	{
		#region Styles Enumeration

		public enum Styles
		{
			Report,
			Document
		}

		#endregion

		#region Constructors

		public Report(DocumentPack pack, ExcelTemplate template, Guid mainPK, DataContext dataContext)
			: this(pack, Styles.Report)
		{
			this.Template = template;
			fDataContextValue = dataContext == DataContext.None ? DataContextValue.None : new DataContextValue(dataContext.ToString());
			this.MainPK = mainPK;
		}

		public Report(DocumentPack pack, ExcelTemplate template, string reportName, ContactType typeOfContact, bool isPasswordProtectedForModifying)
			: this(pack, template, Guid.Empty, DataContext.None)
		{
			this.Name = reportName;
			this.TypeOfContact = typeOfContact;
			this.IsPasswordProtectedForModifying = isPasswordProtectedForModifying;
		}

		public Report(DocumentPack pack, ExcelTemplate template, string reportName, ContactType typeOfContact, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening)
			: this(pack, template, reportName, typeOfContact, isPasswordProtectedForModifying)
		{
			this.IsPasswordProtectedForOpening = isPasswordProtectedForOpening;
		}

		public Report(DocumentPack pack, ExcelTemplate template, Guid mainPK, DataContext dataContext, string reportName)
			: this(pack, template, mainPK, dataContext)
		{
			this.Name = reportName;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, ZGuid menuTemplatePivotPK)
			: this(pack, template, docDataProvider, reportName, typeOfContact, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
			this.menuTemplatePivotPK = menuTemplatePivotPK;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening, ZGuid menuTemplatePivotPK)
			: this(pack, template, docDataProvider, reportName, typeOfContact, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, menuTemplatePivotPK)
		{
			this.IsPasswordProtectedForOpening = isPasswordProtectedForOpening;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: this(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
			this.TypeOfContact = typeOfContact;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, ZGuid menuTemplatePivotPK)
			: this(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
			this.menuTemplatePivotPK = menuTemplatePivotPK;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening, ZGuid menuTemplatePivotPK)
			: this(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, menuTemplatePivotPK)
		{
			this.IsPasswordProtectedForOpening = isPasswordProtectedForOpening;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, BusinessObjectFactory factory = null)
			: this(pack, Styles.Document, factory)
		{
			if (docDataProvider == null)
			{
				ErrorManager.Add(new ReportProcessingError((NoResString)"DocDataProvider object can not be null.", ReportProcessingErrorSeverity.Error));
			}
			else
			{
				var wrapper = docDataProvider.PrimaryDataProvider as DocumentWrapper;
				if (wrapper != null && wrapper.DeliveryContact != null)
				{
					var contact = wrapper.DeliveryContact as DocDeliveryContact;
					DeliveryContact = contact;
				}
			}

			this.DataProviderList = docDataProvider;
			this.Name = reportName;
			this.Template = template;
			fUserDefinedFieldValueList = userDefinedFieldValueList;
			this.Direction = direction;
			this.IsPasswordProtectedForModifying = isPasswordProtectedForModifying;
			this.PrintCopyType = DataProviderList == null ? PrintCopyType.ALL : DataProviderList.PrintCopyType;
			this.JobNumber = (pack != null && pack.BizObject is BusinessObject bizObject) ? bizObject.HumanReadableName : ZString.Empty;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening, BusinessObjectFactory factory = null)
			: this(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, factory)
		{
			this.IsPasswordProtectedForOpening = isPasswordProtectedForOpening;
		}

		public Report(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isReportForTextMacroProcessor, bool useJs, BusinessObjectFactory factory = null)
			: this(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, factory)
		{
			UseJsEvaluator = useJs;
			this.isReportForTextMacroProcessor = isReportForTextMacroProcessor;
		}

		public Report(DocumentPack pack, ExcelTemplate template, IBODocDataProvider docDataProvider, string reportName, ContactType typeOfContact, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: this(pack, template, new DataProviderList(docDataProvider), reportName, typeOfContact, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public Report(DocumentPack pack, ExcelTemplate template, IBODocDataProvider docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: this(pack, template, new DataProviderList(docDataProvider), reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public Report(DocumentPack documentPack, ExcelTemplate excelTemplate)
			: this(documentPack, excelTemplate, Guid.Empty, DataContext.None)
		{
		}

		Report(DocumentPack pack, Styles style, BusinessObjectFactory factory = null)
			: base(pack?.Factory ?? factory)
		{
			this.parent = pack;
			Style = style;

			if (IsTheFirstTimeEngineIsUsedInThisSession)
			{
				IsTheFirstTimeEngineIsUsedInThisSession = false;
				lock (this)
				{
					if (Directory.Exists(Temp.TempPath))
					{
						try
						{
							foreach (var fileName in Directory.GetFiles(Temp.TempPath))
							{
								if (File.GetCreationTime(fileName) < Env.Time.CurrentLocalDate)
								{
									File.SetAttributes(fileName, FileAttributes.Archive);
									File.Delete(fileName);
								}
							}
						}
						catch (IOException)
						{
						}
						catch (UnauthorizedAccessException)
						{
						}
					}
				}
			}

			fDataContextValue = DataContextValue.None;
			fUserDefinedFieldList = new UserControlProviderList();
			this.EmailSubject = "";
			this.Name = "";
			this.PrintCopyType = PrintCopyType.ALL;
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static bool IsTheFirstTimeEngineIsUsedInThisSession = true;

		internal readonly Guid MainPK;

		#region Constructor For IJsonSerializable

		internal Report(DocumentJsonData data)
		{
			Name = data.ReportName;
			if (data.FilterCollection != null)
			{
				FilterCollection = new CollectionOfIFilter(data.FilterCollection);
			}

			OptionalTemplateSheetCollection = new OptionalTemplateSheetCollection(data.OptionalTemplateSheetCollection);

			if (data.SelectedSortOrder != null && !string.IsNullOrEmpty(data.SelectedSortOrder.DisplayName))
			{
				SortOrderCollection.Add(new SortOrder(data.SelectedSortOrder));
				data.SelectedSortOrder.Selected = true;
			}

			if (data.SelectedGroupBy != null && !string.IsNullOrEmpty(data.SelectedGroupBy.DisplayName))
			{
				GroupByCollection.Add(new GroupBy(data.SelectedGroupBy));
				data.SelectedGroupBy.Selected = true;
			}

			if (data.ColumnHeadingManager != null)
			{
				fColumnHeadingManager = new ColumnConfigurationsManager(data.ColumnHeadingManager);
			}

			GroupByCollection.BreakPageOverride = data.BreakPageOverride;
			OverrideReportDbOption = data.OverrideReportDbOption;
			Orientation = data.PageOrientation;
			TimeOut = data.TimeOut;
			IsEdwDataSource = data.IsEdwDataSource;
			ShouldUpdateSchedulableFilters = data.ShouldUpdateSchedulableFilters;
			MaxDop = data.MaxDop;
		}

		#endregion

		#endregion

		#region Parent

		StmMenuItem menuItem;
		public StmMenuItem MenuItem
		{
			get
			{
				if (menuItem != null)
				{
					return menuItem;
				}
				else
				{
					return Parent != null ? Parent.StmMenuCommand : null;
				}
			}
			set
			{
				menuItem = value;
			}
		}

		public bool MustRunOnline
		{
			get { return (MenuItem != null && MenuItem.SU_MustRunOnline); }
		}

		public DocumentPack Parent
		{
			get { return parent; }
		}
		DocumentPack parent;

		internal void SetParent(DocumentPack parentPack)
		{
			this.parent = parentPack;
		}

		#endregion

		#region Properties

		#region IsDisposed

		public bool IsDisposed { get; set; }

		#endregion

		public DocDeliveryPrintDetails PrinterDetails
		{
			get
			{
				if (printerDetails == null)
				{
					printerDetails = new DocDeliveryPrintDetails(Factory ?? new BusinessObjectFactory());
				}
				return printerDetails;
			}
		}
		DocDeliveryPrintDetails printerDetails;

		public bool IsInTaskBuild { get; set; }

		#region Report Name

		public ZString Name
		{
			get
			{
				if (string.IsNullOrEmpty(fName) && Analyser != null)
				{
					fName = Analyser.Config.ReportName;
				}

				return fName;
			}
			internal set
			{
				fName = value;
			}
		}

		string fName;

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		ZString IDeliverable.NameForBinding => Name;

		public StmTemplate StTemplate { get; set; }

		internal StmMenuTemplatePivot Pivot
		{
			get
			{
				StmMenuTemplatePivot pivot = null;
				if (!SourcePivotPK.IsEmpty)
				{
					pivot = Factory.Load<StmMenuTemplatePivot>(SourcePivotPK);
				}
				else if (MenuItem != null && StTemplate != null)
				{
					var query = new ZQuery(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, MenuItem.PK), new ZQuery(StmMenuTemplatePivotSchema.SI_SO, StTemplate.PK));
					query.OrderBy = StmMenuTemplatePivotSchema.Constants.SI_Index;
					pivot = Factory.LoadTop1<StmMenuTemplatePivot>(query);
				}

				return pivot;
			}
		}

		internal void SetDocumentName()
		{
			var emailSubject = string.Empty;
			var pivot = Pivot;

			if (pivot == null || DeliveryContact == null)
			{
				if (Analyser != null)
				{
					emailSubject = Analyser.Config.EmailSubject;
				}
			}
			else if (pivot != null)
			{
				var config = new DocumentConfigPicker(pivot, false).GetDocumentConfig(DeliveryContact.OrgHeader, GlbCompany.CurrentCompany);
				if (config == null || string.IsNullOrEmpty(config.S3_OverrideEmailSubject))
				{
					if (Analyser != null)
					{
						emailSubject = Analyser.Config.EmailSubject;
					}
				}
				else
				{
					emailSubject = config.S3_OverrideEmailSubject;
				}
			}

			if (string.IsNullOrEmpty(emailSubject))
			{
				EmailFormatter.UpdateDocumentName(Name + (BODocDataProvider == null ? "" : " - " + BODocDataProvider.ToString()));
			}
			else
			{
				var subject = TranslateMacros(emailSubject);
				//sanitize EmailSubjects that end with <Now> by removing trailing milliseconds that look like an extension
				if (emailSubject.EndsWith(Now.Useage) && new Regex(@"\.\d{3}$").IsMatch(subject))
				{
					subject = subject.Substring(0, subject.Length - 4);
				}
				EmailFormatter.UpdateDocumentName(subject);
			}
		}

		void SetSheetName()
		{
			SheetNames.Clear();
			foreach (var workSheet in XlInterface.WorkSheets)
			{
				var translatedSheetName = workSheet.UpdateSheetName();
				if (ShouldSheetBeRendered(workSheet.SheetName))
				{
					SheetNames.Add(new SheetName { StrictName = XlInterface.Xls.SheetName, EntireName = translatedSheetName });
				}
			}
		}

		public List<SheetName> SheetNames { get; } = new List<SheetName>();
		internal List<ReportAnalyser> Analysers { get; } = new List<ReportAnalyser>();
		#region Email Formatter

		protected EmailFormatter EmailFormatter
		{
			get
			{
				if (emailFormatter == null)
				{
					emailFormatter = new EmailFormatter(GlbStaff.CurrentUser);
					if (ImageSupportWrapper != null)
					{
						emailFormatter.UpdateCompanyNameToBrandName(ImageSupportWrapper.BrandName);
					}
				}
				return emailFormatter;
			}
		}
		EmailFormatter emailFormatter;

		EmailFormat RegistryEmailFormat => registryEmailFormat ?? (registryEmailFormat = DocumentsDataRegistry.Instance?.EmailFormat?.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
		EmailFormat registryEmailFormat;

		#endregion

		#endregion

		#region JobNumber

		public ZString JobNumber { get; }

		#endregion

		#region Delivery and Original Contact

		public DocDeliveryContact DeliveryContact
		{
			get
			{
				if (!suspendSetDeliveryContactsDataWasAccessed)
				{
					deliveryContactsDataWasAccessed = true;
				}
				return deliveryContact;
			}
			private set { deliveryContact = value; }
		}
		DocDeliveryContact deliveryContact;

		public DocDeliveryContact MostOfficialContact
		{
			get
			{
				deliveryContactsDataWasAccessed = true;
				return mostOfficialContact;
			}
			private set { mostOfficialContact = value; }
		}
		DocDeliveryContact mostOfficialContact;

		bool deliveryContactsDataWasAccessed;

		#endregion

		#region Delivery Mode

		public PrintCopyType PrintCopyType { get; set; }

		public ZString DeliveryMode
		{
			get { return PrintCopyType.ToString(); }
		}

		public ZString AllAvailableDeliveryModes
		{
			get { return nameof(PrintCopyType.ALL); }
		}

		public ZPropertyInfo DeliveryModeInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryMode)); }
		}

		public bool SupportsDeliveryMethod(string deliveryMethod)
			=> !string.IsNullOrEmpty(deliveryMethod) && GetSupportedDeliveryMethods().Contains(deliveryMethod.ToUpperInvariant());

		public IEnumerable<string> GetSupportedDeliveryMethods()
			=> DeliveryMethodHelper.GetSupportedDelvieryMethodsFor(PrintCopyType);

		public IEnumerable<string> GetSupportedDeliveryMethodDespiteOfPrintCopyType() => DeliveryMethodHelper.GetSupportedDelvieryMethodsFor(PrintCopyType.ALL);

		#endregion

		#region Document Type

		public ZString DocumentTypeCode => Pivot?.DocType?.RT_DocType ?? ZString.Empty;

		public ZPropertyInfo DocumentTypeCodeInfo => GetZPropertyInfo(nameof(DocumentTypeCode));

		public ZString DocumentTypeDescription => Pivot?.DocType?.RT_DescMultilingual ?? ZString.Empty;

		public ZPropertyInfo DocumentTypeDescriptionInfo => GetZPropertyInfo(nameof(DocumentTypeDescription));

		#endregion

		#region IncludedInPrint

		public ZBool IncludedInPrint
		{
			get { return includedInPrint; }
			set
			{
				SetNonPersistentPropertyValue(IncludedInPrintInfo, ref includedInPrint, value);

				var collection = ((IBusinessObjectInternals)this).ParentCollections;
				collection?.OfType<DeliverableCollectionView>().FirstOrDefault()?.ValidateDeliveryMethodForRecipients();
			}
		}

		public ZPropertyInfo IncludedInPrintInfo
		{
			get { return GetZPropertyInfo(nameof(IncludedInPrint)); }
		}
		ZBool includedInPrint = true;

		public bool IncludedInPrint_ReadOnly { get; set; }

		#endregion

		#region Email Subject

		public string EmailSubject
		{
			get { return emailSubject; }
			set { emailSubject = value; }
		}
		string emailSubject;

		internal void SetEmailSubject()
		{
			EmailSubjectFieldCollection subjectFields;
			if (ScheduleTask != null && DocumentsDataRegistry.Instance.UseScheduledTaskDescriptionInEmailSubjectForScheduledReports.Value)
			{
				subjectFields = EmailSubjectFieldsWithScheduledTaskDescription;
			}
			else
			{
				subjectFields = RegistryEmailFormat.EmailSubjectFields;
			}
			this.EmailSubject = EmailFormatter.GetEmailSubjectLine(subjectFields);
		}

		EmailSubjectFieldCollection EmailSubjectFieldsWithScheduledTaskDescription
		{
			get
			{
				// EmailSubjectFieldCollection doesn't support yield
				var fields = new EmailSubjectFieldCollection();
				foreach (EmailSubjectField field in RegistryEmailFormat.EmailSubjectFields)
				{
					if (field.Code == Core.Constants.EmailFormat.EmailFieldCodes.DocumentName)
					{
						fields.Add(new EmailSubjectField(field.Index, Core.Constants.EmailFormat.EmailFieldCodes.ScheduledTaskDescription));
					}
					else
					{
						fields.Add(field);
					}
				}
				return fields;
			}
		}

		#endregion

		#region FormatType

		public CodeDescriptionPairList FormatType_List
		{
			get
			{
				if (fFormatType_List == null)
				{
					fFormatType_List = new CodeDescriptionPairList
										{
											new CodeDescriptionPair(OrgConstants.AttachmentType.PDF, Res.GetString("9203e4b3-ec86-4c6e-a3b1-04881f5a49a8", "PDF - Adobe PDF File")),
											new CodeDescriptionPair(OrgConstants.AttachmentType.PDFA, Res.GetString("d104ffbb-e976-4d04-ae75-39a5b4ae7a2d", "PDF/A-2 - Adobe PDF File Archive")),
											new CodeDescriptionPair(OrgConstants.AttachmentType.XLS, Res.GetString("9aca7da1-ff04-4948-8fa4-36cb6d4bf854", "XLS - Microsoft Excel Spreadsheet")),
											new CodeDescriptionPair(OrgConstants.AttachmentType.XLSX, Res.GetString("60acd24d-015e-4adc-9532-e6dd8f3dfc09", "XLSX - Microsoft Excel 2007 Spreadsheet")),
											new CodeDescriptionPair(OrgConstants.AttachmentType.TIF, Res.GetString("9377c4ad-c345-409d-a7ef-3cc0471f65b0", "TIF - TIFF Image File"))
										};
					SelectedFormatType = fFormatType_List[0].Code;
				}
				return fFormatType_List;
			}
		}
		CodeDescriptionPairList fFormatType_List;

		[MaxLength(4)]
		public ZString SelectedFormatType
		{
			get { return FormatType_List != null ? fSelectedFormatType : fSelectedFormatType; }
			set
			{
				CheckMaximumLength(SelectedFormatTypeInfo, value);
				fSelectedFormatType = value;
				SelectedFormatTypeInfo.RefreshBinding();
			}
		}
		ZString fSelectedFormatType;

		public ZPropertyInfo SelectedFormatTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedFormatType)); }
		}

		#endregion

		#region Languages

		public CodeDescriptionPairList Languages
		{
			get
			{
				if (languages == null)
				{
					languages = new CodeDescriptionPairList();
					var languageLookup = new CodeDescriptionPairList(OLookUpEditType.Language);

					foreach (CodeSelection selection in WebDataRegistry.Instance.AllowedLanguages.Value)
					{
						if (Res.IsSystemDefinedEnglish(selection.Code))
						{
							languages.AddPair(Core.Constants.Languages.EnglishAmerican, languageLookup.GetMultilingualDescriptionFromCode(Core.Constants.Languages.EnglishAmerican));
							languages.AddPair(Core.Constants.Languages.EnglishBritish, languageLookup.GetMultilingualDescriptionFromCode(Core.Constants.Languages.EnglishBritish));
						}
						else if (AvailableDocBuilderLanguageList.IsLanguageAvailable(selection.Code))
						{
							languages.AddPair(selection.Code, languageLookup.GetMultilingualDescriptionFromCode(selection.Code));
						}
					}
				}
				return languages;
			}
		}

		CodeDescriptionPairList languages;

		public ZString SelectedLanguage
		{
			get => selectedLanguage;
			set
			{
				selectedLanguage = value;
				SelectedLanguageInfo.RefreshBinding();
			}
		}
		ZString selectedLanguage;

		public ZPropertyInfo SelectedLanguageInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedLanguage)); }
		}

		#endregion

		#region TranslateLegacyDocument

		public bool TranslateLegacyDocument { get; set; }

		#endregion

		#region ContainsAnyCustomisation

		/// <summary>
		/// Returns whether this report contains any customised DocBuilder sections, or is a non-system defined document
		/// </summary>
		public bool ContainsAnyCustomisation
		{
			get
			{
				return (Template != null && Template.ContainsCustomisedSections)
					|| (Template is ExcelTemplateReadFromStmTemplateTable excelTemplateReadFromStm && !excelTemplateReadFromStm.IsSystemDefined)
					|| (MenuItem != null && !MenuItem.SU_IsSystemDefined)
					|| (StTemplate != null && !StTemplate.SO_IsSystemDefined)
					|| (Pivot != null && (!Pivot.SI_IsSystemDefined || Pivot.Template != null && !Pivot.Template.SO_IsSystemDefined));
			}
		}

		#endregion

		public bool DisableFixedValueCache { get; set; }

		internal MacroTranslator MacroTranslatorForProcessingDisableFixedValueCacheFlag
		{
			get { return macroTranslatorForProcessingDisableFixedValueCacheFlag ?? (macroTranslatorForProcessingDisableFixedValueCacheFlag = new MacroTranslator(this)); }
		}
		MacroTranslator macroTranslatorForProcessingDisableFixedValueCacheFlag;

		public bool ForceWebPublish
		{
			get
			{
				var result = false;
				if (Analyser != null)
				{
					if (Analyser.Config != null)
					{
						result = Analyser.Config.ForceWebPublish;
					}
				}
				return result;
			}
		}

		public ZString DocumentDeliveredEventCode { get; set; }
		public ZString DocumentPasswordInformationEventCode { get; set; }
		public readonly Styles Style;
		public readonly DocumentDirection Direction;
		public readonly bool IsPasswordProtectedForModifying;
		public readonly bool IsPasswordProtectedForOpening;
		public OverflowNoteDocument OverflowNoteDocument => overflowNoteDocument ?? (overflowNoteDocument = new OverflowNoteDocument());
		OverflowNoteDocument overflowNoteDocument;

		DocumentProtector protector;
		internal DocumentProtector Protector => protector ??= new DocumentProtector(this);

		internal bool IsReportForTextMacroProcessor
		{
			get
			{
				return isReportForTextMacroProcessor;
			}
		}
		readonly bool isReportForTextMacroProcessor;

		public bool IsReportForSectionPreview { get; set; }

		#region Orientation
		[List("OrientationList")]
		[MaxLength(3)]
		public ZString Orientation
		{
			get
			{
				return OrientationManager.Value;
			}
			set
			{
				if (OrientationManager.Value != value)
				{
					CheckMaximumLength(OrientationInfo, value);
					OrientationManager.Value = value;
				}
				OrientationInfo.RefreshBinding();
			}
		}

		public class OrientationManagement
		{
			public ZString Value = ReportOrientationTypeList.Codes.Default;
			public ReportOrientationTypeList List = new ReportOrientationTypeList();
		}

		public OrientationManagement OrientationManager
		{
			get { return fOrientationManager ?? (fOrientationManager = new OrientationManagement()); }
		}

		OrientationManagement fOrientationManager;

		public ZPropertyInfo OrientationInfo
		{
			get { return GetZPropertyInfo(nameof(Orientation)); }
		}

		public CodeDescriptionPairList OrientationList
		{
			get { return OrientationManager.List; }
		}

		public ZInt TimeOut
		{
			get
			{
				return timeout;
			}
			set
			{
				SetNonPersistentPropertyValue(TimeOutInfo, ref timeout, value);
			}
		}
		ZInt timeout;

		public ZPropertyInfo TimeOutInfo
		{
			get { return GetZPropertyInfo(nameof(TimeOut)); }
		}

		#endregion

		public PageStyles PageStyle
		{
			get
			{
				PrepareForRender();
				return Analyser.Config.PageStyle;
			}
		}

		public bool ExpandRowsForAutoHeight
		{
			get { return Analyser.Config.ExpandRowsForAutoHeight; }
		}

		public ContactType TypeOfContact;

		public void ReadUserDefinedFields()
		{
			GetNewReportAnalyser().ReadUserDefinedFieldsFromFieldsTab();
		}

		public IEnumerable<IReportError> GetReportErrors()
		{
			return ErrorManager.GetReportErrors();
		}

		internal void SetTemplateConstantsOnBODocDataProvider()
		{
			if (BODocDataProvider != null)
			{
				BODocDataProvider.SetDocWrapperContext(TemplateDefinedConstants);
			}
		}

		public string TranslateMacros(string macro, bool raiseExceptions = false)
		{
			return RegexProvider.OutermostMacroRegex.Replace(macro, new MatchEvaluator((match) => ReplaceSingleMacroWithSkippingEscapeAngleBracket(match, raiseExceptions)));
		}

		internal string ReplaceSingleMacroWithSkippingEscapeAngleBracket(Match aMatch, bool raiseExceptions = false)
		{
			return ReplaceSingleMacroCore(aMatch, true, raiseExceptions);
		}

		internal string ReplaceSingleMacroNotInTemplateBody(Match aMatch)
		{
			return ReplaceSingleMacroCore(aMatch, false, false);
		}

		public bool TryGetCachingService(out ITextMacroProcessingCachingService result)
		{
			result = null;
			if (DataProviderList == null)
			{
				return false;
			}

			foreach (var dataProvider in DataProviderList.AllDataProviders)
			{
				var data = Enterprise.DocumentEngineCore.DocWrappers.BODocDataProvider.GetObject(dataProvider);
				if (data is BusinessObject bo && bo.Factory != null)
				{
					result = bo.Factory.ServiceContainer.GetService<ITextMacroProcessingCachingService>();
					return result != null;
				}
			}

			return false;
		}

		string ReplaceSingleMacroCore(Match aMatch, bool shouldSkipEscapingAngleBrackets, bool raiseExceptions)
		{
			var result = "";
			try
			{
				var macro = aMatch.Groups[0].Value;
				object resultObj = null;
				if (TryGetCachingService(out var cachingService) && cachingService.TryGetMacroValue(macro, out var returnValue))
				{
					if (returnValue is string str)
					{
						return str;
					}

					resultObj = returnValue;
				}
				else
				{
					resultObj = MacroTranslator.GetValue(macro, Passes.SecondPass, shouldSkipEscapingAngleBrackets);
				}

				if (resultObj is ZDateTime dateTime)
				{
					var includeTicks = dateTime.Ticks % 10000000 > 0;
					result = dateTime.ToString(Culture.Invariant.DateTimeFormat.FullDateTimePattern + (includeTicks ? ".fff" : ""), CultureInfo.InvariantCulture);
				}
				else if (resultObj is ZDateTimeOffset dateTimeOffset)
				{
					var includeTicks = dateTimeOffset.Ticks % 10000000 > 0;
					result = dateTimeOffset.ToString(Culture.Invariant.DateTimeFormat.FullDateTimePattern + (includeTicks ? ".fffffff" : "") + " zzz", CultureInfo.InvariantCulture);
				}
				else
				{
					result = resultObj.ToString();
				}

				cachingService?.CacheMacroValue(macro, result);
			}
			catch (FieldNotFoundException exception)
			{
				exception.AddAsWarningToReport(this);

				if (raiseExceptions)
				{
					throw;
				}
			}
			return result;
		}

		internal readonly Dictionary<string, object> TemplateDefinedConstants = new Dictionary<string, object>();
		internal readonly Dictionary<string, object> SheetDefinedConstants = new Dictionary<string, object>();

		public bool OverrideReportDbOption { get; set; }

		public bool IsLegacyDocument => Style == Styles.Document && StTemplate != null && !StTemplate.IsDocBuilderStyle;

		public ZString Language
		{
			get
			{
				var isLegacyDocument = IsLegacyDocument;

#if DEBUG
				if (isLegacyDocument)
				{
					isLegacyDocument = !StTemplate.IsDocBuilderStyleForTest;
				}
#endif

				if (isLegacyDocument)
				{
					if (!TranslateLegacyDocument)
					{
						return RawDataRegistry.Instance.EnglishSpelling.Value;
					}

					if (!string.IsNullOrEmpty(Analyser?.Config?.ForcedLanguage))
					{
						return Analyser.Config.ForcedLanguage;
					}
				}

				return Parent != null && !Parent.Language.IsEmpty ? Parent.Language : (ZString)(Res.CurrentLanguage == Res.DefaultLanguage ? Core.Constants.Languages.EnglishAmerican : Res.CurrentLanguage);
			}
		}

		internal ZString RenderedLanguage;

		internal string SourceFile
		{
			get
			{
				if (StTemplate != null && !string.IsNullOrEmpty(StTemplate.SO_ExcelTemplatePath))
				{
					return StTemplate.SO_ExcelTemplatePath;
				}
				if (Template != null && !string.IsNullOrEmpty(Template.TemplateSourceLocation))
				{
					return Template.TemplateSourceLocation;
				}
				if (StTemplate != null && !string.IsNullOrEmpty(StTemplate.SO_Name))
				{
					return (NoResString)"Template Name: " + StTemplate.SO_Name;
				}
				if (Template != null && !string.IsNullOrEmpty(Template.TemplateName))
				{
					return (NoResString)"ExcelTemplate Name: " + Template.TemplateName;
				}
				return "";
			}
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			var result = new ZStringBuilder();

			IStmMenuItem menuItem = MenuItem;
			if (menuItem != null)
			{
				ZString menuNameWithPath = menuItem.SU_MenuPath + (string.IsNullOrEmpty(menuItem.SU_MenuPath) || menuItem.SU_MenuPath.EndsWith("/") ? "" : "/") + menuItem.SU_MenuName;

				result.Append((NoResString)"Report Information:");

				result.Append(string.Format((NoResString)@"
MenuItem:-
   BusinessContext = [{0}]
   Name with Path = [{1}]
   Filter = [{2}]
   PK = [{3}]
   IsSystemDefined = [{4}]
   IsClientSpecific = [{5}]",
							menuItem.SU_BusinessContext, //0
							menuNameWithPath, //1
							menuItem.SU_FilterList, //2
							menuItem.PK, //3
							menuItem.SU_IsSystemDefined.ToYN(), //4
							menuItem.SU_IsClientSpecific.ToYN())); //5
			}
			else
			{
				result.Append("");
				result.Append((NoResString)@"No MenuItem Found on the Report.");
			}

			var curStTemplate = StTemplate;
			if (curStTemplate != null)
			{
				result.Append(string.Format((NoResString)@"
Template:-
   Name = [{0}]
   DataContext = [{1}]
   ExcelFilePath = [{2}]
   PK = [{3}]
   IsSystemDefined = [{4}]
   IsClientSpecific = [{5}]",
	curStTemplate.SO_Name, curStTemplate.SO_DataContext, curStTemplate.SO_ExcelTemplatePath, curStTemplate.PK, curStTemplate.SO_IsSystemDefined.ToYN(), curStTemplate.SO_IsClientSpecific.ToYN()));
			}
			else
			{
				result.Append("");
				result.Append((NoResString)"No StmTemplate Found on the Report.");
			}

			if (ScheduleTask != null)
			{
				result.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)@"
Scheduled Task:-
   Description = [{0}]",
	ScheduleTask.S5_ScheduleDescription));
			}
			else
			{
				result.Append("");
				result.Append((NoResString)"Not Running from Scheduled Report.");
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region Render

		public bool ContainsDataRows
		{
			get
			{
				foreach (var analyser in Analysers)
				{
					if (analyser != null && analyser.Sections != null)
					{
						foreach (var section in analyser.Sections)
						{
							if (section.SectionBody.RowCount > 0)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public bool ContainsDataRowsForBusinessObjectDataSource => Analyser != null && GetRowCountForBusinessObjectDataSource() > 0;

		public int GetRowCountForBusinessObjectDataSource()
		{
			var rowCount = 0;
			foreach (var analyser in Analysers)
			{
				if (analyser != null && analyser.Sections != null)
				{
					foreach (var section in Analyser.Sections)
					{
						if (section.SectionBody.DataRowSource is BusinessObjectDataSource)
						{
							rowCount += section.SectionBody.RowCount;
						}
					}
				}
			}
			return rowCount;
		}

		public void PrepareForRender()
		{
			if (IsPreparingForRender)
			{
				throw new ApplicationException("PrepareForRender must not recurse.");
			}

			IsPreparingForRender = true;

			using (Res.TemporarilySwitchLanguage(Language))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Language) ?? Culture.Default))
			{
				try
				{
					if (!IsPreparedForRender)
					{
						ResetDataProvider();
						AnalyseReportAndUpdateFilters();
						IsPreparedForRender = true;

						if (!ErrorManager.HasErrors)
						{
							EmailSubject = Analyser.Config.EmailSubject;
							TranslateLegacyDocument = Analyser.Config.TranslateLegacyDocument;
						}

						RegisterDocumentAndReportRelatedMacroProviders();
						HideSheetIfConditionallyHidden();
						UpdatedSheetNameOnceWhenPreRendering();
					}
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{
						throw;
					}

					ErrorManager.ReportFatalException(exception);
				}
				finally
				{
					IsPreparingForRender = false;
				}
			}
		}

		void HideSheetIfConditionallyHidden()
		{
			if (Analyser.Config != null)
			{
				var hideSheetIfExpression = Analyser.Config.HideSheetIfExpression;
				if (!string.IsNullOrEmpty(hideSheetIfExpression))
				{
					var result = Analyser.EvaluateExpressionForConditionalAreas(hideSheetIfExpression);
					if (result)
					{
						var sheetName = WorkSheetCurrentlyBeingProcessed.SheetName;
						if (!HiddenSheetsNames.Contains(sheetName))
						{
							HiddenSheetsNames.Add(sheetName);
							WorkSheetCurrentlyBeingProcessed.Clear();
						}
					}
				}
			}
		}

		bool updatedSheetNameWhenPreRendering;

		void UpdatedSheetNameOnceWhenPreRendering()
		{
			if (!updatedSheetNameWhenPreRendering)
			{
				updatedSheetNameWhenPreRendering = true;
				SheetNames.Clear();
				var analyser = GetNewReportAnalyser();

				for (var workSheetIndex = 0; workSheetIndex < XlInterface.WorkSheets.Count; workSheetIndex++)
				{
					var workSheet = XlInterface.WorkSheets[workSheetIndex];
					if (workSheet == null)
					{
						continue;
					}

					if (IsTemplateSheet(workSheet.SheetName))
					{
						var sheetNameOverride = string.Empty;
						if (WorkSheetCurrentlyBeingProcessed != workSheet)
						{
							sheetNameOverride = analyser.ReadSpecificParameterValueFromConfigArea(workSheet, Constants.ConfigAreaParameters.SheetNameOverride);
						}
						else
						{
							sheetNameOverride = Analyser.Config.SheetNameOverride;
						}

						if (!string.IsNullOrEmpty(sheetNameOverride))
						{
							sheetNameOverride = Analyser.Config.ReplaceMacros(sheetNameOverride).ToString();
							workSheet.SheetNameOverride = sheetNameOverride;
						}

						var translatedSheetName = workSheet.UpdateSheetName();
						SheetNames.Add(new SheetName { StrictName = workSheet.SheetName, EntireName = translatedSheetName });
					}
				}
			}
		}

		internal bool IsGenerated
		{
			get { return isGenerated; }
		}
		bool isGenerated;

		bool IsPreparedForRender;
		bool IsPreparingForRender;
		internal bool HasChart;
		internal bool IsAnalyzed;

		void AnalyseReportAndUpdateFilters()
		{
			if (IsPreparedForRender)
			{
				throw new InvalidOperationException("AnalyseAndUpdateFilters() should never be called when Report.IsPreparedForRender = true");
			}

			Analyser = GetNewReportAnalyser();
			Analyser.Analyse();
			UpdateSchedulableFilters();
			if (!DisableXlsxExportInfo.ContainsKey(WorkSheetCurrentlyBeingProcessed.SheetName) && Analyser.Config != null)
			{
				DisableXlsxExportInfo.Add(WorkSheetCurrentlyBeingProcessed.SheetName, Analyser.Config.DisableXLSXExport);
			}
		}

		internal void UpdateAndSynchroniseFilters()
		{
			try
			{
				AnalyseReportAndUpdateFilters();
			}
			finally
			{
				if (fXlInterface != null)
				{
					fXlInterface.Dispose();
					fXlInterface = null;
				}
			}
		}

		internal void ResetIsPreparedForRender()
		{
			IsPreparedForRender = false;
		}

		internal void RegisterDocumentAndReportRelatedMacroProviders()
		{
			if (MainPK != Guid.Empty)
			{
				FilterCollection.Add(new PrimaryKeyFilter(fDataContextValue.ToString() + "ID", MainPK));
				MacroTranslator.RegisterValueProvider(new FixedValueProvider(fDataContextValue.ToString() + "ID", MainPK));
			}

			foreach (var filter in FilterCollection)
			{
				var filterValueProviderList = filter as IValueProviderListProvider;
				if (filterValueProviderList != null)
				{
					foreach (var valueProvider in filterValueProviderList.ValueProviders)
					{
						MacroTranslator.RegisterValueProvider(valueProvider);
					}
				}
			}

			foreach (FilterField userControlProvider in UserDefinedFieldList)
			{
				foreach (var valueProvider in userControlProvider.ValueProviders)
				{
					MacroTranslator.RegisterValueProvider(valueProvider);
				}
			}
		}

		internal void ValidateFilters()
		{
			if (FilterCollection != null)
			{
				FilterCollection.RunPreSaveValidation();
				if (FilterCollection.HasErrors)
				{
					foreach (var error in FilterCollection.GetErrors())
					{
						ErrorManager.Add(new ReportFilterValidationError(error.Message, ReportProcessingErrorSeverity.ErrorWithoutErrorReport));
					}

					if (ScheduleTask != null)
					{
						ScheduleTask.MarkTaskAsInactive();
					}
				}
			}
		}

		internal static bool IsTooManyRowsException(FlexCelXlsAdapterException exception)
		{
			if (exception.ErrorCode == XlsErr.ErrTooManyRows || exception.ErrorCode == XlsErr.ErrTooManyEntries)
			{
				return true;
			}
			return false;
		}

		#endregion

		#region Visualisation

		internal VisualiserDataSet OverridingDataSet
		{
			get
			{
				/*
				 * M.K, 2011-08-11: Temporary fix to prevent NullReferenceException on later stages.
				 * Happens when Cover Note is entered and Modify is selected in Delivery form due to unresolved data source for the cover note text.
				 * Should be fixed property in correct place.
				 * Original code is:
				 * return VisualizerContentNote != null ? VisualizerContentNote.DataSource : null;
				 */
				return VisualizerContentNote != null && VisualizerContentNote.DataSource != null ? VisualizerContentNote.DataSource : new VisualiserDataSet { DataSetName = NullVisualiserDataSetId };
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Identification constant")]
		public const string NullVisualiserDataSetId = "<<null>>";

		VisualizerNote visualizerContentNote;
		public VisualizerNote VisualizerContentNote
		{
			get
			{
				if (Parent?.StmMenuCommand != null && (visualizerContentNote == null || visualizerContentNote.IsDeleted))
				{
					visualizerContentNote = Get(VisualizerNoteFactory, Parent?.StmMenuCommand, MenuItem, GetVisualizerNoteSupporter());
				}
				return visualizerContentNote;
			}
		}

		BusinessObjectFactory VisualizerNoteFactory
		{
			get { return visualizerNoteFactory ?? (visualizerNoteFactory = Parent?.VisualizerNoteFactory ?? new BusinessObjectFactory() { NameForDebugging = "Report.VisualizerContentNote" }); }
		}
		BusinessObjectFactory visualizerNoteFactory;

		internal IVisualizerNoteSupporter GetVisualizerNoteSupporter()
		{
			if (DataProviderList != null)
			{
				foreach (var dataProvider in DataProviderList.AllDataProviders)
				{
					var docWrapperWithChildBizObjToWrap = dataProvider as IDocWrapperWithChildBizObjToWrap;
					if (docWrapperWithChildBizObjToWrap != null
						&& docWrapperWithChildBizObjToWrap.WrappedBusinessObject != null
						&& docWrapperWithChildBizObjToWrap.WrappedChildBusinessObject != null)
					{
						return new VisualizerNoteSupporter(docWrapperWithChildBizObjToWrap.WrappedBusinessObject, docWrapperWithChildBizObjToWrap.WrappedChildBusinessObject);
					}
					else if (dataProvider.BusinessObjectToLogAgainst != null)
					{
						return dataProvider.BusinessObjectToLogAgainst as IVisualizerNoteSupporter ?? new VisualizerNoteSupporter(dataProvider.BusinessObjectToLogAgainst);
					}
				}
			}

			return null;
		}

		internal BusinessObject GetBusinessObjectToLogAgainst()
		{
			return (DataProviderList != null) ? DataProviderList.AllDataProviders.Select(dataProvider => dataProvider.BusinessObjectToLogAgainst).FirstOrDefault(bizObj => bizObj != null) : null;
		}

		internal BusinessObject BusinessObjectForPrintJob => DataProviderList?.AllDataProviders.OfType<IBODocDataProviderWithBOForPrintJob>()
				.Select(d => d.BusinessObjectForPrintJob).FirstOrDefault(bo => bo != null && !(bo is NonPersistentBusinessObject));

		internal void SaveVisualizerContentNote()
		{
			if (VisualizerContentNote != null)
			{
				var dataSource = VisualizerContentNote.DataSource;
				VisualizerContentNote.DD_DocumentData = new ZBlob(dataSource.SerialiseToByteArray());

				ResetIsPreparedForRender();
				ResetCachedExcelFile();
			}
		}

		internal void DeleteVisualizerContentNote()
		{
			if (visualizerContentNote != null)
			{
				visualizerContentNote.Delete();

				ZExceptionReporting.ProcessWithSaveExceptionHandling(visualizerContentNote.Factory.Save, null, true);

				RevertVisualizerNote();
				ResetIsPreparedForRender();
				ResetCachedExcelFile();
			}
		}

		internal void RevertVisualizerNote()
		{
			if (visualizerContentNote != null)
			{
				visualizerContentNote.ResetDataSource();
				visualizerContentNote = null;
				ResetCachedExcelFile();
			}
		}

		#endregion

		#region Trailing Space

		/// <summary>
		/// Trailing space, in 24ths of an inch.
		/// </summary>
		public int TrailingSpace
		{
			get
			{
				var result = 0;

				if (IsDotMatrixAWB)
				{
					var pageNo = fName.Substring(fName.Length - 1, 1);

					if (pageNo == "1")
					{
						result = Page1TrailingSpace(GetPaperType(fName));
					}
					else if (pageNo == "2")
					{
						result = Page2TrailingSpace(GetPaperType(fName));
					}
				}
				else if (IsCMRDocument)
				{
					result = 24;
				}

				return result;
			}
		}

		#region CMR Document

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		bool IsCMRDocument
		{
			get { return fName == "CMR International Consignement Note"; }
		}

		#endregion

		#region AWB

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		bool IsDotMatrixAWB
		{
			get
			{
				return
					fName == "Neutral HAWB1" ||
					fName == "Neutral HAWB2" ||
					fName == "Neutral MAWB1" ||
					fName == "Neutral MAWB2" ||
					fName == "Carrier MAWB1" ||
					fName == "Carrier MAWB2";
			}
		}

		string GetPaperType(string aWBTemplateName)
		{
			return aWBTemplateName.IndexOf("HAWB") > -1 ? Env.Registry.Freight.AirWaybill.HAWBPaperType : Env.Registry.Freight.AirWaybill.MAWBPaperType;
		}

		int Page1TrailingSpace(string paperType)
		{
			return paperType == Core.Constants.AWB.PaperTypes.Traxon ? 24 : 0;
		}

		int Page2TrailingSpace(string paperType)
		{
			var result = 0;

			switch (paperType)
			{
				case Core.Constants.AWB.PaperTypes.Iata:
				case Core.Constants.AWB.PaperTypes.IataOld:
					result = 48;
					break;

				case Core.Constants.AWB.PaperTypes.Traxon:
					result = 24;
					break;
			}

			return result;
		}

		#endregion

		#endregion

		#region Save

		public virtual void Save(DocDeliveryContact deliveryContact, DocDeliveryContact mostOfficialContact, Stream fileContent)
		{
			if (mostOfficialContact != null
				&& mostOfficialContact.OrgHeader == null
				&& deliveryContact != null
				&& deliveryContact.OrgHeader != null)
			{
				var contactOrganisationPK = mostOfficialContact.OrgHeaderPK = deliveryContact.OrgHeaderPK;

				if (!AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK.ToString()))
				{
					if (!contactOrganisationPK.IsEmpty && new ZGuid(TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK]).IsEmpty)
					{
						TemplateDefinedConstants[DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK] = contactOrganisationPK.ToString();
					}
				}
			}

			this.DeliveryContact = deliveryContact;
			this.MostOfficialContact = mostOfficialContact;

			Save(fileContent);
		}

		internal bool AddTemplateDefinedConstantIfNotAlreadyAdded(string key, object value)
		{
			if (!TemplateDefinedConstants.ContainsKey(key))
			{
				TemplateDefinedConstants.Add(key, value);
				return true;
			}
			return false;
		}

		internal bool AddSheetDefinedConstantIfNotAlreadyAdded(string key, object value)
		{
			if (!SheetDefinedConstants.ContainsKey(key))
			{
				SheetDefinedConstants.Add(key, value);
				return true;
			}
			return false;
		}

		public int Save(Stream stream)
		{
			var hasUserRequiredToRunOnline = (DeserializedReport == null) ? OverrideReportDbOption : DeserializedReport.OverrideReportDbOption;

			var useReportDbConnection =
				Style == Report.Styles.Report &&
				!hasUserRequiredToRunOnline &&
				ReportDBManager.IsReportingDbEnabled &&
				!MustRunOnline;

			if (ReportDBManager.IsReportingDbEnabled)
			{
				if (hasUserRequiredToRunOnline)
				{
					OnAddDBServerInfo((NoResString)"User required to run online, report will be run on primary server.");
				}
				else if (MustRunOnline)
				{
					OnAddDBServerInfo((NoResString)"Report is set to must run online, report will be run on primary server.");
				}
			}

			var createdConnectionSpid = SaveWithDbConnection(stream, useReportDbConnection ? ReportDBManager : PrimaryConnectionProvider);

			if (DataProvider is NonPersistentBusinessObject)
			{
				visualizerContentNote.Delete(); //NPBizO has non-persistent PK. We would never find it again.
			}

			return createdConnectionSpid;
		}

		void RenderAndSave(Stream stream)
		{
			var stopReportGeneration = false;
			var forceXlsxFormat = false;
			isGenerated = false;
			var startingErrorsCount = 0;

			try
			{
				OnOnRenderAndSave();
#if DEBUG
				if (ObjectFactory.Contains(typeof(IExceptionWhenRenderAndSave)))
				{
					ObjectFactory.Get<IExceptionWhenRenderAndSave>().Throw();
				}
#endif
				using (var logger = new DocumentRendererDiskLogger())
				{
					DocumentRendererLogger = logger;
					DocumentRendererLogger.Log(Res.GetString("412d0bea-8db8-49b9-9fd9-7eb509177f91", "Beginning document rendering."));

					ErrorManager.ClearErrors();

					foreach (var templateGenerationError in TemplateGenerationErrors)
					{
						if (templateGenerationError != null)
						{
							ErrorManager.Add(templateGenerationError.ToReportProcessingError());
						}
					}

					if (Globals.IsWeb || !TryRestoreRenderedTemplateFromCache())
					{
						deliveryContactsDataWasAccessed = false;
						cachedSheetsVisibility = new Dictionary<string, bool>();
						Analysers.Clear();
						for (var workSheetIndex = 0; workSheetIndex < XlInterface.WorkSheets.Count; workSheetIndex++)
						{
							var workSheet = XlInterface.WorkSheets[workSheetIndex];
							if (workSheet == null)
							{
								continue;
							}
							else if (workSheet.IsRendered)
							{
								AddToRenderedWorkSheetsForTesting(workSheet);
								continue;
							}

							var workSheetName = workSheet.SheetName;

							cachedSheetsVisibility[workSheetName] = IsDisabledOptionalTemplateSheet(workSheetName);

							if (ShouldSheetBeRendered(workSheetName))
							{
								if (fWorkSheetCurrentlyBeingProcessed != workSheet)
								{
									fWorkSheetCurrentlyBeingProcessed = workSheet;
									IsPreparedForRender = false;
								}

								Renderer.Render();

								Analysers.Add(Analyser);

								AddToRenderedWorkSheetsForTesting(workSheet);
							}
							else if (IsTranslationsSheetName(workSheetName))
							{
								ErrorManager.Add(new ReportProcessingError(Res.GetString("839D011B-9EAC-4324-B8BC-E3202A6980B0", "'Translations' sheet was obsoleted, please delete it."), ReportProcessingErrorSeverity.WarningWithoutErrorReport));
							}
						}
						hasCopySpecificConfiguration = deliveryContactsDataWasAccessed;
					}

					// This should go when Error Form is split off and GUI is properly separated off.
					IsPreparedForRender = true;

					var isOkToGenerate = !ErrorManager.HasErrors;

					SetOkToGenerateRegardlessOfErrorsForTesting(ref isOkToGenerate);

					startingErrorsCount = ErrorManager.ErrorsCount;
					ShowErrorsIfAny();
					if (isOkToGenerate || ContinueToGenerateReportRegardlessOfWarnings)
					{
						DoGenerate(stream);
					}

					DocumentRendererLogger.Log(Res.GetString("d86cfee6-24c4-4266-bb90-bc19f42c04de", "Finishing document rendering."));
				}
			}
			catch (MaxConcurrentReportConnectionsExceeded ex)
			{
				stopReportGeneration = true;

				if (!IsScheduledReport)
				{
					var message = Res.GetString("aa7736d6-a081-4f64-8992-70512c9efc23", "Maximum Concurrent Report Limit Reached.\r\n\r\nThe following reports are currently running...\r\n\r\n{0}\r\nPlease try again in a few minutes.", ex.Message);
					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
				}
				else
				{
					throw;
				}
			}
			catch (MetadataHasChangedException ex)
			{
				stopReportGeneration = true;

				if (!IsScheduledReport)
				{
					var message = Res.GetString("8f25ea70-4cea-49a0-9949-2304221946b8", "The report could not be generated due to a transient conflict with the Index Update Service.\r\n\r\nPlease try again in a few minutes.");
					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
				}
				else
				{
					throw;
				}
			}
			catch (GhostRecordsBeingDeletedException ex)
			{
				stopReportGeneration = true;

				if (!IsScheduledReport)
				{
					var message = Res.GetString("CB8AA796-FA36-432C-9473-B9CBC39FD7B4", "The transaction was terminated due to the state change of records.\r\n\r\nPlease try again in a few minutes.");//The transaction was terminated because of the availability replica config/state change or because ghost records are being deleted on the primary and the secondary availability replica that might be needed by queries running under snapshot isolation.\r\n\r\nPlease try again in a few minutes."
					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
				}
				else
				{
					throw;
				}
			}
			catch (ExcelLimitationException ex)
			{
				var message = ExcelLimitationsHelper.Messages.GetTooMany(ex);
				ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport));
				stopReportGeneration = true;
			}
			catch (ExcelLimitationForThisFileFormatException ex)
			{
				HandleExcelFormatChangeIfRequired(ref stopReportGeneration, ref forceXlsxFormat, ex);
			}
			catch (DataProviderException ex)
			{
				stopReportGeneration = true; // no valid excel document were generated in this case - cache is invalid, reset excel interface
				var message = Res.GetString("6da86853-df0e-4666-a483-c9a9941436b6", "The report could not be generated. Please ensure that the report is being run in the correct company. Error details:\r\n{0}", ex.Message);
				ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
			}
			catch (ReportSQLTimeoutException ex)
			{
				stopReportGeneration = true;
				var message = Res.GetString("3deba4b3-ee77-4f68-838e-759216c107ca", @"During the running of this report, the query timed out.
This could be because the server is very busy or the report is complex and runs on a large data set and needs a longer query timeout.

1) Try running this report at a time that the system is not busy, or
2) Try increasing the query timeout for reports from:
   i) Query Timeout Override field with current value : {0}, see (Change Report Filters -> Query Timeout Override field where default value : 0)
   ii) SQL Timeout with current value : {2}, see (Report template config area that may be set as ""{1}=600"" or may not be defined where default value : -1)
   iii) Report Command Timeout with current value : {3}, see ({4} where default value : {5})
   iv) Report Preview Timeout with current value : {6}, see ({7} where default value : {8})",
				TimeOut,
				"SqlTimeout",
				Analyser?.Config?.SqlTimeout,
				DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.Value,
				((IRegistryItemInternals)DocumentsDataRegistry.Instance.ReportDBCommandTimeOut).Location,
				DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.DefaultValue,
				DocumentsDataRegistry.Instance.ReportPreviewTimeout.Value,
				((IRegistryItemInternals)DocumentsDataRegistry.Instance.ReportPreviewTimeout).Location,
				DocumentsDataRegistry.Instance.ReportPreviewTimeout.DefaultValue);

				ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
			}
			catch (SqlLockLostException)
			{
				throw;
			}
			catch (InvalidOperationException ex)
			{
				stopReportGeneration = true;
				if (ex.Message.Contains((NoResString)"Object is currently in use elsewhere."))
				{
					var message = Res.GetString("ca07db79-365b-495e-86df-34346858b9a0", "The report could not be generated due to a transient graphics failure in Windows. However, the report will be rendered fine when next attempted. Error details:\r\n{0}", ex.Message);
					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
				}
				else
				{
					ErrorManager.ReportFatalException(ex);
				}
			}
			catch (ArgumentException ex)
			{
				stopReportGeneration = true;
				if (ex.Message.Contains((NoResString)"Invalid high surrogate character") || (ex.Message.Contains((NoResString)"The surrogate pair") && ex.Message.Contains((NoResString)"is invalid")))
				{
					var message = Res.GetString("2df60170-16a8-46e9-8728-b0f44ac8234b", "The report could not be generated due to unsupported character(s). Please, check the consistency of your data's then submit it again. Error details : \r\n{0}", ex.Message);

					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
				}
				else if (ex.Message.Contains((NoResString)"does not support style"))
				{
					var message = Res.GetString("E8C3AC1B-721D-4C95-BB6D-CB8340F2EB89", "Font not found. Error details : \r\n{0}", ex.Message);

					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.WarningWithoutErrorReport, ex));
				}
				else
				{
					ErrorManager.ReportFatalException(ex);
				}
			}
			catch (DocumentAreaUnbreakableException ex)
			{
				stopReportGeneration = true;

				var message = Res.GetString("77F30D07-8C97-4263-8CA6-B833A9D3494C", "The report could not be generated due to some area does not fit on the page and is too large to print/preview. Error details : \r\n{0}", ex.Message);
				ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport, ex));
			}
			catch (Exception exception)
			{
				#region Test
#if DEBUG
				ThrowDocumentEngineExceptionForTesting();
#endif
				#endregion

				stopReportGeneration = true;
				if (exception.IsCriticalException())
				{
					throw;
				}

				var (shouldSkip, displayMessage) = ObjectFactory.Get<IReportErrorHandler>().ShouldSkipReportError(exception);
				if (shouldSkip)
				{
					ErrorManager.Add(new ReportProcessingError(displayMessage, ReportProcessingErrorSeverity.FatalWithoutErrorReport));
				}
				else
				{
					ErrorManager.ReportFatalException(exception);
				}
			}
			finally
			{
				if (startingErrorsCount != ErrorManager.ErrorsCount)
				{
					ShowErrorsIfAny();
				}
				ReportFieldNotFoundErrorsForFullyQualifiedMacros();
				if (!stopReportGeneration)
				{
					if (ErrorManager.HasErrors && ContinueToGenerateReportRegardlessOfWarnings && !forceXlsxFormat)
					{
						DoGenerate(stream);
					}
					else if (forceXlsxFormat)
					{
						stream.SetLength(0);
						DoGenerateAsXlsx(stream);
					}
					ContinueToGenerateReportRegardlessOfWarnings = true;
				}
				else
				{
					stream.SetLength(0);
				}
				ClearRenderedTemplate(stopReportGeneration);
				DocumentRendererLogger = null;
				OnAfterRenderAndSave();
			}
		}

		internal void DoGenerate(Stream streamToSave)
		{
			DoGenerate(streamToSave, ((IDeliverable)this).FileExtension);
		}

		void DoGenerate(Stream streamToSave, string fileExtension)
		{
			HideExtraSheets();
			SelectFirstVisibleSheet();
			Protector.PasswordProtectForModifying();

			SetSheetName();
			XlInterface.SaveToStream(streamToSave, fileExtension);
			if (stmReportRun != null)
			{
				stmReportRun.RRI_ReportSizeBytes = streamToSave.Length;
			}

			using (Res.TemporarilySwitchLanguage(Language))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Language) ?? Culture.Default))
			{
				SetDocumentName();
				SetEmailSubject();
			}

			isGenerated = true;

			// This should go when Error Form is split off and GUI is properly separated off.
			IsPreparedForRender = true;
		}

		void HandleExcelFormatChangeIfRequired(ref bool stopReportGeneration, ref bool forceXlsxFormat, ExcelLimitationForThisFileFormatException ex)
		{
			if (DisableXLSXExport)
			{
				var message = ExcelLimitationsHelper.Messages.GetTooManyForExcel2003WithReportName(ex, Name);
				ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport));

				if (ScheduleTaskNotifications != null)
				{
					ScheduleTaskNotifications.Notify(new WarningNotification(message));
				}
				stopReportGeneration = true;
			}
			else
			{
				if (AttachmentType == AttachmentTypeList.Codes.Xlsx)
				{
					var message = ExcelLimitationsHelper.Messages.GetTooMany(ex);
					ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.FatalWithoutErrorReport));
					stopReportGeneration = true;
				}
				else
				{
					if (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.Value || AttachmentType != AttachmentTypeList.Codes.Xls)
					{
						forceXlsxFormat = true;
						SwitchFromXLSToXLSX();
					}
					else
					{
						if (Globals.CanShowDialogs)
						{
							var answer = ExcelLimitationsHelper.Messages.ShowTooManyForExcel2003WithFormatSwitchQuestion(ex);
							if (answer == ZDialogResult.Yes)
							{
								forceXlsxFormat = true;
								SwitchFromXLSToXLSX();
							}
							else
							{
								stopReportGeneration = true;
							}
						}
						else
						{
							stopReportGeneration = true;
							if (ScheduleTaskNotifications != null)
							{
								ScheduleTaskNotifications.Notify(new WarningNotification(ExcelLimitationsHelper.Messages.GetTooManyWithReportName(ex, Name)));
							}
						}
					}
				}
			}
		}

		void DoGenerateAsXlsx(Stream streamToSave)
		{
			try
			{
				DoGenerate(streamToSave, AttachmentTypeList.Codes.Xlsx);
			}
			catch (ExcelLimitationBaseException ex)
			{
				ExcelLimitationsHelper.Messages.ShowTooMany(ex);
				streamToSave.SetLength(0);
			}
		}

		void ClearRenderedTemplate(bool forceInterfaceReset)
		{
			OverflowNoteDocument.OverflowNotes.RemoveAll();
			try
			{
				if (fWorkSheetCurrentlyBeingProcessed != null)
				{
					fWorkSheetCurrentlyBeingProcessed.Dispose();
					fWorkSheetCurrentlyBeingProcessed = null;
				}
			}
			finally
			{
				try
				{
					if (fXlInterface != null)
					{
						if (!hasCopySpecificConfiguration && !forceInterfaceReset && isGenerated)
						{
							if (cachedXlInterface == null)
							{
								cacheTimestamp = ZDateTime.Now;
							}
							cachedXlInterface = new WeakReference(fXlInterface);
#if DEBUG
							if (!IsSkipUnRegisterDisposable)
							{
								CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(fXlInterface);
							}
#endif
						}
						else
						{
							fXlInterface.Dispose();
							cachedXlInterface = null;
							cachedSheetsVisibility = null;
						}
						fXlInterface = null;
					}
				}
				finally
				{
					IsPreparedForRender = false;
				}
			}
		}

#if DEBUG
		[ThreadStatic]
		internal static bool FireDummyGarbageCollection;

		internal bool IsSkipUnRegisterDisposable;
#endif

		bool TryRestoreRenderedTemplateFromCache()
		{
#if DEBUG
			if (FireDummyGarbageCollection)
			{
				if (cachedXlInterface != null)
				{
					(cachedXlInterface.Target as ExcelInterface)?.Dispose();
					cachedXlInterface.Target = null;
				}
			}
#endif

			var xlInterfaceFromCache = cachedXlInterface != null ? cachedXlInterface.Target as ExcelInterface : null;

			if (xlInterfaceFromCache != null)
			{
				var outDatedCache = specifiedDataRowSourceChanged ||
					(ZDateTime.Now - cacheTimestamp).TotalMinutes >= 60 ||
					RenderedLanguage != Language ||
						cachedSheetsVisibility != null && (cachedSheetsVisibility.Count != xlInterfaceFromCache.WorkSheets.Count ||
							xlInterfaceFromCache.WorkSheets.Any(
								sheet =>
								{
									bool sheetVisibility;
									return !cachedSheetsVisibility.TryGetValue(sheet.SheetName, out sheetVisibility) || sheetVisibility != IsDisabledOptionalTemplateSheet(sheet.SheetName);
								}));

				if (outDatedCache)
				{
					xlInterfaceFromCache.Dispose();
					cachedXlInterface = null;
					specifiedDataRowSourceChanged = false;
				}
				else
				{
					if (fXlInterface != null && fXlInterface != xlInterfaceFromCache)
					{
						fXlInterface.Dispose();
					}
					fXlInterface = xlInterfaceFromCache;

					return true;
				}
			}
			else if (cachedXlInterface != null) // cachedXlInterface.Target has been recycled.
			{
				cachedXlInterface = null;
			}

			return false;
		}

		internal void SetSheetVisibility(string workSheetName, bool isDisable)
		{
			if (cachedSheetsVisibility != null)
			{
				cachedSheetsVisibility[workSheetName] = isDisable;
			}
		}

		WeakReference cachedXlInterface;
		Dictionary<string, bool> cachedSheetsVisibility;
		bool hasCopySpecificConfiguration;
		ZDateTime cacheTimestamp;

		#region Running Connection - Reporting Database

		public enum ReportRunSource
		{
			Unknown = 0,
			Preview = 1,
			ManualPrint = 2,
			ServiceTask = 3,
		}

		public static DisposableAction TemporarilySetReportRunSource(ReportRunSource rrs)
		{
			var old = reportRunSource;
			reportRunSource = rrs;

			return new DisposableAction(() => { reportRunSource = old; });
		}

		[ThreadStatic]
		static ReportRunSource reportRunSource;

		int SaveWithDbConnection(Stream streamToSave, IConnectionProvider connectionProvider)
		{
			var createdConnectionSpid = -1;

			var useStaffLogin = DocumentsDataRegistry.Instance.EnforceDataAccessOnReport.Value;
			var impersonateUserName = useStaffLogin && (ScheduleTask?.PrintUser?.IsReadOnlyDBUser == ZBool.True) ? ScheduleTask?.PrintUser?.SQLUserName : string.Empty;
			var applicationNameSuffix = (MenuItem?.SU_IsSystemDefined ?? true) ? null : string.Format("_Customized_{0}_", MenuItem.IsADocumentMenuItem ? (NoResString)"Document" : (NoResString)"Report") + MenuItem.PK;
			if (applicationNameSuffix != null)
			{
				applicationNameSuffix += "_" + reportRunSource.ToString();
			}

			using (var reportDbConnectionWrapper = connectionProvider.GetNewConnectionWrapper(impersonateUserName, applicationNameSuffix))
			{
				try
				{
					if (reportDbConnectionWrapper.Connection != Db.Connection)
					{
						RunningConnection = reportDbConnectionWrapper.Connection;
					}
					createdConnectionSpid = RunningConnection.SPID;

					OnReportServerNameChanged();
					RenderAndSave(streamToSave);
				}
				finally
				{
					if (RunningConnection != Db.Connection)
					{
						RunningConnection.Dispose();
					}
					RunningConnection = null;
				}
			}

			return createdConnectionSpid;
		}

		internal bool IsScheduledReport;

		public EventHandler<string> ReportServerNameChanged;
		public EventHandler<DbConnection> OnRenderAndSave;
		public EventHandler<DbConnection> AfterRenderAndSave;
		public EventHandler<string> AddDBServerInfo;

		void OnReportServerNameChanged()
		{
			ReportServerNameChanged?.Invoke(this, RunningConnection.ServerName);
		}

		void OnOnRenderAndSave()
		{
			OnRenderAndSave?.Invoke(this, RunningConnection);
		}

		void OnAfterRenderAndSave()
		{
			AfterRenderAndSave?.Invoke(this, RunningConnection);
		}

		void OnAddDBServerInfo(string message) => AddDBServerInfo?.Invoke(this, message);

		public virtual DbConnection RunningConnection
		{
			get
			{
#if DEBUG
				//We end up needing a kludge anyway (due to unit tests that run a customized report, and thus 'need' to run on a separate connection with ApplicationNameSuffix,
				//but read from a ## temporary table, which seems to deadlock for some reason above my paygrade). but if we mark specific unit tests as
				//'yes we need to run on the main connection' rather than it being a default behaviour, at least we can limit the damage rather than it being automatically broken.
				if (useMainConnection)
				{
					return Db.Connection;
				}
#endif
				return ShouldUseEdwConnection ? EdwConnection : runningConnection ?? Db.Connection;
			}
			private set
			{
				runningConnection = value;
			}
		}

		protected DbConnection runningConnection;

		DbConnection EdwConnection
		{
			get
			{
				if (edwConnection == null)
				{
					var edwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
					edwConnection = string.IsNullOrEmpty(edwServer) ? null : Db.NewExtraConnectionWithMainDbCredentials(edwServer, Db.EdwDatabaseName);
				}

				return edwConnection;
			}
		}
		DbConnection edwConnection;

		public ZBool IsEdwDataSource
		{
			get
			{
				return fIsEdwDataSource;
			}
			set
			{
				SetNonPersistentPropertyValue(IsEdwDataSourceInfo, ref fIsEdwDataSource, value);
			}
		}
		ZBool fIsEdwDataSource;

		public ZPropertyInfo IsEdwDataSourceInfo
		{
			get { return GetZPropertyInfo(nameof(IsEdwDataSource)); }
		}

		ZBool ShouldUseEdwConnection;

		public IDisposable TrySwitchConnection(bool isEdwData = false)
		{
			var originalDataSource = ShouldUseEdwConnection;
			if (IsEdwDataSource)
			{
				ShouldUseEdwConnection = isEdwData;
			}
			return new DisposableAction(() => { ShouldUseEdwConnection = originalDataSource; });
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "for unit tests only")]
		public static bool useMainConnection;

		public static IDisposable TemporarilyUseMainConnection()
		{
			useMainConnection = true;
			return new DisposableAction(() => { useMainConnection = false; });
		}
#endif

		ISecondaryServerConnectionProvider reportdbManager;
		ISecondaryServerConnectionProvider ReportDBManager
		{
			get
			{
				if (reportdbManager == null)
				{
					reportdbManager = SecondaryServerConnectionProviderProvider.GetProvider((exception, serverName) => { ShowMessageOfSecondaryServerConnectionException(exception, serverName); }, (message) => OnAddDBServerInfo(message));
				}
				return reportdbManager;
			}
		}

		IConnectionProvider primaryConnectionProvider;
		IConnectionProvider PrimaryConnectionProvider => primaryConnectionProvider ?? (primaryConnectionProvider = new PrimaryServerConnectionProvider());

		void ShowMessageOfSecondaryServerConnectionException(Exception exception, string serverName)
		{
			if (Globals.CanShowDialogs)
			{
				ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(exception, serverName);
			}
			else if (ScheduleTaskNotifications != null)
			{
				ScheduleTaskNotifications.Notify(new WarningNotification(string.Format(CultureInfo.InvariantCulture, "{0}\r\n  {1}\r\n{2}",
					CaptionOfSecondaryServerConnectionException(serverName), MessageOfSecondaryServerConnectionException, exception.Message)));
			}
		}

		static ResourceString CaptionOfSecondaryServerConnectionException(string serverName) => ResString.GetMultilingualString("6f22f513-1a69-4024-b15e-93b3a5027996", "Unable to connect to the Secondary Server {0}", serverName);

		static ResourceString MessageOfSecondaryServerConnectionException => ResString.GetMultilingualString("e05ebaa2-654a-4a51-a451-4dce4186dbe5",
			"If other Secondary Servers are set up in the registry setting under \"{0}\", the system will try to use them before reverting to the Primary Server.\r\nException details:"
			, SystemDataRegistry.Instance.ReportingDbServerNames.HumanReadableRegistryPath());

		public static void ShowMessageOfSecondaryServerConnectionExceptionByMessageBox(Exception exception, string serverName)
		{
			Globals.Message.Show(
				string.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", MessageOfSecondaryServerConnectionException, exception.Message),
				CaptionOfSecondaryServerConnectionException(serverName),
				ZMessageBoxButtons.OK,
				ZMessageBoxIcon.Warning);
		}

		#endregion

		#endregion

		#region Filters

		internal ExcelWorkSheet FilterSheet
		{
			get { return FindSheet(FilterSheetNameRegEx); }
		}

		internal ExcelWorkSheet TranslationsSheet
		{
			get { return FindSheet(TranslationsSheetNameRegEx); }
		}

		public CollectionOfIFilter FilterCollection
		{
			get
			{
				if (fFilterCollection == null)
				{
					fFilterCollection = new CollectionOfIFilter();
					RegisterEditableChildObject(fFilterCollection);
				}
				return fFilterCollection;
			}
			set
			{
				if (fFilterCollection != null)
				{
					UnRegisterEditableChildObject(fFilterCollection);
				}
				fFilterCollection = value;
				RegisterEditableChildObject(fFilterCollection);
			}
		}

		CollectionOfIFilter fFilterCollection;

		public bool FilterLoadedByReportAnalyser { get; internal set; }

		public LookupField LinkedLookupField
		{
			get { return ColumnHeadingManager.LinkedLookupField; }
		}

		void UpdateFilterCollectionOnReportFromDeserializedReport()
		{
			for (var i = 0; i < FilterCollection.Count; i++)
			{
				var fieldToReplace = (FilterField)FilterCollection[i];

				var hasValueFromDeserializedReport = false;

				foreach (FilterField deserialisedField in DeserializedReport.FilterCollection)
				{
					if (deserialisedField.DisplayName == fieldToReplace.DisplayName)
					{
						hasValueFromDeserializedReport = true;
						fieldToReplace.PreSetValueForDeserializingBeforeExchange(deserialisedField);
						if (!fieldToReplace.IsCompatibleWith(deserialisedField))
						{
							fieldToReplace.Validators.Add(new IncompatibleFilterValidator());
							fieldToReplace.RunPreSaveValidation();
						}
						else if (deserialisedField.IsValid)
						{
							FilterCollection[i] = deserialisedField;
							deserialisedField.SetNecessaryPropertiesForDeserializing(fieldToReplace, FilterCollection);
						}
						break;
					}
				}

				//Clears the default value of OptionGroup if it is not in filter collection of deserialized report, which means the default value has been un-ticked by user
				if (!hasValueFromDeserializedReport)
				{
					if (fieldToReplace is OptionGroup)
					{
						var optionGroup = fieldToReplace as OptionGroup;
						if (!string.IsNullOrEmpty(optionGroup.ValueAsStringForSerialisation))
						{
							foreach (var pair in optionGroup.DescriptionCodePairList)
							{
								pair.Value = false;
							}
						}
					}
					else
					{
						var multipleSelectionLookup = fieldToReplace as MultipleSelectionLookup;
						if (!string.IsNullOrEmpty(multipleSelectionLookup?.ValueAsStringForSerialisation))
						{
							multipleSelectionLookup.ClearValues();
						}
					}
				}
			}

			foreach (FilterField field in FilterCollection)
			{
				if (field.HasErrors)
				{
					field.RunPreSaveValidation();
				}
			}
		}

		class IncompatibleFilterValidator : FilterCollectionValidator
		{
			public override bool IsValid(FilterField filterToValidate)
			{
				return (wasFixed |= filterToValidate.HasChanges);
			}

			bool wasFixed;

			public override string GetErrorMessage(FilterField filterToValidate)
			{
				return !IsValid(filterToValidate) ? Res.GetString("f0b3ffcf-2a5d-4779-9506-3421ce0fb91f", "Stored value is outdated and incompatible with filter's options.") : "";
			}
		}

		#region OverriddenValueForOrgLookupFilter

		internal ZGuid OverriddenValueForOrgLookupFilter { get; set; }

		void UpdateLookupFieldValueIfIsLinkToScheduledReportRecipient(LookupField field)
		{
			if (IsPreparingForRender && field != null && field.LinkToScheduledReportRecipientForOrganisation && OverriddenValueForOrgLookupFilter.IsValid)
			{
				field.Value = OverriddenValueForOrgLookupFilter.ToGuid();
			}
		}

		#endregion

		#endregion

		#region Optional Template Sheets

		internal ExcelWorkSheet OptionalTemplatesSheet
		{
			get { return FindSheet(OptionalTemplateSheetNameRegEx); }
		}

		internal bool IsDisabledOptionalTemplateSheet(string workSheetName)
		{
			var reportDisabledOptionalTemplate = OptionalTemplateSheetCollection.Contains(workSheetName) && !OptionalTemplateSheetCollection[workSheetName].Selected;
			return reportDisabledOptionalTemplate || HiddenSheetsNames.Contains(workSheetName);
		}

		public OptionalTemplateSheetCollection OptionalTemplateSheetCollection
		{
			get
			{
				if (fOptionalTemplateSheetCollection == null)
				{
					fOptionalTemplateSheetCollection = new OptionalTemplateSheetCollection(Factory, new ValidatorPack());
					RegisterEditableChildObject(fOptionalTemplateSheetCollection);
				}
				return fOptionalTemplateSheetCollection;
			}
			set
			{
				if (fOptionalTemplateSheetCollection != null)
				{
					UnRegisterEditableChildObject(fOptionalTemplateSheetCollection);
				}
				fOptionalTemplateSheetCollection = value;
				RegisterEditableChildObject(fOptionalTemplateSheetCollection);
			}
		}

		OptionalTemplateSheetCollection fOptionalTemplateSheetCollection;

		internal List<string> HiddenSheetsNames
		{
			get { return hiddenSheetsNames ?? (hiddenSheetsNames = new List<string>()); }
		}
		List<string> hiddenSheetsNames;

		public bool OptionalTemplatesLoadedByReportAnalyser { get; internal set; }

		void UpdateOptionalTemplateSheetCollectionOnReportFromDeserializedReport()
		{
			foreach (OptionalTemplateSheet optionalTemplateSheet in DeserializedReport.OptionalTemplateSheetCollection)
			{
				var sheet = OptionalTemplateSheetCollection[optionalTemplateSheet.Name];

				if (sheet != null)
				{
					var order = sheet.Order;
					sheet = optionalTemplateSheet;
					sheet.Order = order;

					OptionalTemplateSheetCollection[optionalTemplateSheet.Name] = sheet;
				}
			}
		}

		#endregion

		#region Template Sheets

		internal ExcelWorkSheet ConstantsSheet
		{
			get { return FindSheet(ConstantsSheetNameRegEx); }
		}

		public ExcelWorkSheet UDFSheet
		{
			get { return FindSheet(FieldSheetNameRegEx); }
		}

		internal ExcelWorkSheet FlexCelScaleSheet
		{
			get { return FindSheet(FlexCelScaleSheetNameRegEx); }
		}

		public static bool IsTemplateSheet(string sheetName)
		{
			var result = true;
			if (IsSortSheetName(sheetName) || IsGroupBySheetName(sheetName) || IsConstantsSheetName(sheetName) || IsFilterSheetName(sheetName) || IsTranslationsSheetName(sheetName) || IsFieldSheetName(sheetName) || IsFlexCelScaleSheetName(sheetName) || IsOptionalTemplateSheetName(sheetName))
			{
				result = false;
			}
			return result;
		}

		public static bool IsSortSheetName(string sheetName)
		{
			return SortSheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsGroupBySheetName(string sheetName)
		{
			return GroupBySheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsConstantsSheetName(string sheetName)
		{
			return ConstantsSheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsFilterSheetName(string sheetName)
		{
			return FilterSheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsTranslationsSheetName(string sheetName)
		{
			return TranslationsSheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsFieldSheetName(string sheetName)
		{
			return FieldSheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsFlexCelScaleSheetName(string sheetName)
		{
			return FlexCelScaleSheetNameRegEx.IsMatch(sheetName);
		}

		public static bool IsOptionalTemplateSheetName(string sheetName)
		{
			return OptionalTemplateSheetNameRegEx.IsMatch(sheetName);
		}

		bool ShouldSheetBeRendered(string sheetName)
		{
			return IsTemplateSheet(sheetName) && !IsDisabledOptionalTemplateSheet(sheetName);
		}

		internal static string GetLocalizedSheetName(string sheetName)
		{
			var cellAnalyzer = new ExcelCellAnalyzer(sheetName);
			var translatable = cellAnalyzer.GetTextToBeTranslated();
			if (translatable.Length > 0)
			{
				foreach (var item in translatable)
				{
					var stringData = Res._GetData(DocBuilderResourceStrings.ReportNamesAsmid, DocBuilderResourceStrings.ReportNameKeyPrefix + item.TranslatableText, string.Empty);
					item.TranslatableText = stringData != null && !string.IsNullOrEmpty(stringData.Caption) ? stringData.Caption : item.TranslatableText;
				}
			}
			return cellAnalyzer.CellText;
		}

		public StringCollection TemplateSheets
		{
			get
			{
				if (fTemplateSheets == null)
				{
					fTemplateSheets = GetTemplateSheets();
				}
				return fTemplateSheets;
			}
		}

		StringCollection fTemplateSheets;

		StringCollection GetTemplateSheets()
		{
			var result = new StringCollection();
			for (var sheetNumber = 0; sheetNumber < XlInterface.WorkSheets.Count; sheetNumber++)
			{
				if (IsTemplateSheet(XlInterface.WorkSheets[sheetNumber].SheetName))
				{
					result.Add(XlInterface.WorkSheets[sheetNumber].SheetName);
				}
			}
			return result;
		}

		#endregion

		#region Sort Order

		internal ExcelWorkSheet SortSheet
		{
			get { return FindSheet(SortSheetNameRegEx); }
		}

		public SortOrderCollection SortOrderCollection
		{
			get
			{
				if (fSortOrderCollection == null)
				{
					fSortOrderCollection = new SortOrderCollection();
					RegisterEditableChildObject(fSortOrderCollection);
				}
				return fSortOrderCollection;
			}
			set
			{
				if (fSortOrderCollection != null)
				{
					UnRegisterEditableChildObject(fSortOrderCollection);
				}
				fSortOrderCollection = value;
				RegisterEditableChildObject(fSortOrderCollection);
			}
		}

		SortOrderCollection fSortOrderCollection;

		public bool SortOrderLoadedByReportAnalyser { get; internal set; }

		void UpdateSortOrderCollectionOnReportFromDeserializedReport()
		{
			SortOrder curSortOrder;
			foreach (SortOrder sortOrder in DeserializedReport.SortOrderCollection)
			{
				for (var i = 0; i < SortOrderCollection.Count; i++)
				{
					curSortOrder = SortOrderCollection[i];
					if (sortOrder.DisplayName == curSortOrder.DisplayName)
					{
						if (sortOrder.Selected)
						{
							SortOrderCollection.SelectedOrder = curSortOrder;
						}
						break;
					}
				}
			}
		}

		#endregion

		#region Group Bys

		internal ExcelWorkSheet GroupBySheet
		{
			get { return FindSheet(GroupBySheetNameRegEx); }
		}

		public GroupByCollection GroupByCollection
		{
			get
			{
				if (fGroupByCollection == null)
				{
					fGroupByCollection = new GroupByCollection();
					RegisterEditableChildObject(fGroupByCollection);
				}
				return fGroupByCollection;
			}
			set
			{
				if (fGroupByCollection != null)
				{
					UnRegisterEditableChildObject(fGroupByCollection);
				}
				fGroupByCollection = value;
				RegisterEditableChildObject(fGroupByCollection);
			}
		}

		GroupByCollection fGroupByCollection;

		public bool GroupBysLoadedByReportAnalyser { get; internal set; }

		void UpdateGroupByCollectionOnReportFromDeserializedReport()
		{
			GroupBy curGroupBy;
			foreach (GroupBy groupBy in DeserializedReport.GroupByCollection)
			{
				for (var i = 0; i < GroupByCollection.Count; i++)
				{
					curGroupBy = GroupByCollection[i];
					if (groupBy.DisplayName == curGroupBy.DisplayName)
					{
						if (groupBy.Selected)
						{
							GroupByCollection.SelectedGroupBy = curGroupBy;
						}
						break;
					}
				}
			}
			GroupByCollection.BreakPageOverride = DeserializedReport.GroupByCollection.BreakPageOverride;
		}

		#endregion

		#region IWebReport Members

		[MaxLength(256)]
		public ZString SelectedColumnLayout
		{
			get { return fSelectedColumnLayout; }
			set
			{
				if (fSelectedColumnLayout != value)
				{
					CheckMaximumLength(SelectedColumnLayoutInfo, value);
					fSelectedColumnLayout = value;
					var manager = GetSelectedColumnConfigurationManager();
					if (manager != null)
					{
						IncrementManagerLoadHitCountForTesting();
						manager.Load(this);
					}
					SelectedColumnLayoutInfo.RefreshBinding();
				}
			}
		}

		ZString fSelectedColumnLayout;

		public ZPropertyInfo SelectedColumnLayoutInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedColumnLayout)); }
		}

		protected ColumnConfigurationManager GetSelectedColumnConfigurationManager()
		{
			foreach (var columnConfigurationMan in SavedConfigurations)
			{
				if (columnConfigurationMan.Description == SelectedColumnLayout)
				{
					return columnConfigurationMan;
				}
			}
			return null;
		}

		public CodeDescriptionPairList ColumnLayoutList
		{
			get
			{
				if (fColumnLayoutList.Count == 0)
				{
					fColumnLayoutList = new CodeDescriptionPairList();

					ColumnConfigurationManager companyDefaultLayout = null;

					foreach (var layout in SavedConfigurations)
					{
						if (layout is CompanyDefaultConfigurationManager)
						{
							companyDefaultLayout = layout;
						}

						var orglayout = layout as CombinedConfigurationManager;

						if (orglayout != null && ((IWebReport)this).CurrentOrgPK.IsValid && orglayout.LinkPK == ((IWebReport)this).CurrentOrgPK)
						{
							fColumnLayoutList.Add(new CodeDescriptionPair((string)(layout.Description), layout.Description));
						}
					}

					if (companyDefaultLayout != null && fColumnLayoutList.Count == 0)
					{
						fColumnLayoutList.Add(new CodeDescriptionPair((string)(companyDefaultLayout.Description), companyDefaultLayout.Description));
						isOnlyCompanyDefaultLayout = true;
					}
					else
					{
						isOnlyCompanyDefaultLayout = false;
					}

					if (fColumnLayoutList.Count > 0)
					{
						SelectedColumnLayout = fColumnLayoutList[0].Code;
					}
				}
				return fColumnLayoutList;
			}
		}
		CodeDescriptionPairList fColumnLayoutList = new CodeDescriptionPairList();

		ZGuid IWebReport.CurrentOrgPK { get; set; }

		bool IWebReport.IsOnlyCompanyDefaultLayout
		{
			get { return ColumnLayoutList.Count == 1 && isOnlyCompanyDefaultLayout; }
		}
		bool isOnlyCompanyDefaultLayout;

		#endregion

		#region ColumnHeadings

		ColumnConfigurationManager[] SavedConfigurations
		{
			get
			{
				if (fSavedConfigurations == null)
				{
					foreach (FilterField filter in FilterCollection)
					{
						if (filter is ColumnConfigurationField field)
						{
							return (fSavedConfigurations = field.SavedConfigurations.ToArray());
						}
					}
					return Array.Empty<ColumnConfigurationManager>();
				}

				return fSavedConfigurations;
			}
		}
		ColumnConfigurationManager[] fSavedConfigurations;

		public ColumnConfigurationsManager ColumnHeadingManager
		{
			get { return fColumnHeadingManager ?? (fColumnHeadingManager = new ColumnConfigurationsManager(MenuItem != null ? MenuItem.PK : ZGuid.Empty, ScheduleTask != null)); }
		}
		protected ColumnConfigurationsManager fColumnHeadingManager;

		public bool ColumnHeadingsProcessedByReportAnalyser { get; set; }

		internal string GetTranslatedColumnHeading(string macros, bool updateColumnHeadingMacros = true)
		{
			var trimedMacros = macros.Trim();
			var result = RegexProvider.OutermostMacroRegex.Replace(trimedMacros, new MatchEvaluator((match) => MacroTranslator.GetValue(match.Groups[0].Value, Passes.FirstPass)?.ToString()));
			if (updateColumnHeadingMacros && ColumnHeadingMacros != null && RegexProvider.OutermostMacroRegex.IsMatch(result) && !ColumnHeadingMacros.Contains(result))
			{
				ColumnHeadingMacros.Add(result);
			}
			return result;
		}

		internal void TranslateColumnHeadingMacrosIfNeeded()
		{
			if (ColumnHeadingMacros != null && ColumnHeadingMacros.Any() && WorkSheetCurrentlyBeingProcessed != null)
			{
				if (ColumnHeadingManager.CurrentConfiguration.Worksheets.Contains(WorkSheetCurrentlyBeingProcessed.SheetName))
				{
					var columnHeadings = ColumnHeadingManager.CurrentConfiguration.Worksheets[WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.OfType<ColumnHeading>();
					foreach (var columnHeading in columnHeadings)
					{
						if (ColumnHeadingMacros.Any(macro => columnHeading.DisplayLabel == macro))
						{
							columnHeading.DisplayLabel = GetTranslatedColumnHeading(columnHeading.DisplayLabel, false);
						}

						if (ColumnHeadingMacros.Any(macro => columnHeading.Description == macro))
						{
							columnHeading.Description = GetTranslatedColumnHeading(columnHeading.Description, false);
						}

						if (ColumnHeadingMacros.Any(macro => columnHeading.HeadingText == macro))
						{
							columnHeading.HeadingText = GetTranslatedColumnHeading(columnHeading.HeadingText, false);
						}

						if (ColumnHeadingMacros.Any(macro => columnHeading.TagName == macro))
						{
							columnHeading.TagName = GetTranslatedColumnHeading(columnHeading.TagName, false);
						}
					}
				}
			}
		}

		internal List<string> ColumnHeadingMacros { get; set; }

		#endregion

		internal ConcurrentDictionary<string, bool> IsSingleMacroCache = new ConcurrentDictionary<string, bool>();

		#region Schedule Task

		public ReportScheduleTask ScheduleTask { get; private set; }

		public void SetScheduleTask(ReportScheduleTask value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			if (ScheduleTask != null)
			{
				throw new InvalidOperationException("SetScheduleTask cannot be called more than once.");
			}

			ScheduleTask = value;
			var taskDescription = ScheduleTask.S5_ScheduleDescription;
			if (RegexProvider.OutermostMacroRegex.IsMatch(taskDescription))
			{
				taskDescription = ObjectFactory.Get<ITextMacroProcessor>().Replace(ScheduleTask.S5_ScheduleDescription, new BusinessObject[] { ScheduleTask });
			}
			EmailFormatter.UpdateScheduledTaskDescription(taskDescription);
			UpdateSchedulableFilters();
		}

		internal INotifications ScheduleTaskNotifications { get; set; }

		void UpdateSchedulableFilters()
		{
			if (ScheduleTask != null && ShouldUpdateSchedulableFilters && (DeserializedReport?.ShouldUpdateSchedulableFilters ?? true))
			{
				if (fFilterCollection != null)
				{
					fFilterCollection.Scheduled = true;
					foreach (var filter in FilterCollection)
					{
						var schedulableFilter = filter as ISchedulableFilterField;
						if (schedulableFilter != null)
						{
							schedulableFilter.SetScheduleTask(ScheduleTask);
						}

						UpdateLookupFieldValueIfIsLinkToScheduledReportRecipient(filter as LookupField);
					}
				}
				if (UserDefinedFieldList != null)
				{
					foreach (FilterField filter in UserDefinedFieldList)
					{
						var schedulableFilter = filter as ISchedulableFilterField;
						if (schedulableFilter != null)
						{
							schedulableFilter.SetScheduleTask(ScheduleTask);
						}
					}
				}
			}
		}

		#endregion

		#region Worksheet Management

		internal string FirstRenderableSheetName
		{
			get
			{
				if (fFirstRenderableSheetName == null)
				{
					for (var i = 0; i < XlInterface.WorkSheets.Count; i++)
					{
						if (IsTemplateSheet(XlInterface.WorkSheets[i].SheetName))
						{
							fFirstRenderableSheetName = XlInterface.WorkSheets[i].SheetName;
							break;
						}
					}
				}
				return fFirstRenderableSheetName;
			}
		}
		string fFirstRenderableSheetName;

		internal ExcelWorkSheet WorkSheetCurrentlyBeingProcessed
		{
			get
			{
				if (fWorkSheetCurrentlyBeingProcessed == null && XlInterface != null && XlInterface.WorkSheets.Count > 0)
				{
					fWorkSheetCurrentlyBeingProcessed = XlInterface.WorkSheets[0];
				}

				return fWorkSheetCurrentlyBeingProcessed;
			}
		}

#if DEBUG

		public void SetWorkSheetCurrentlyBeingProcessedForTestOnly(ExcelWorkSheet worksheet)
		{
			fWorkSheetCurrentlyBeingProcessed = worksheet;
		}

#endif

		ExcelWorkSheet fWorkSheetCurrentlyBeingProcessed;

		internal int MaxCol
		{
			get { return WorkSheetCurrentlyBeingProcessed.ColumnCount; }
		}

		void HideExtraSheets()
		{
			HideWorkSheet(SortSheet);
			HideWorkSheet(GroupBySheet);
			HideWorkSheet(FilterSheet);
			HideWorkSheet(TranslationsSheet);
			HideWorkSheet(UDFSheet);
			HideWorkSheet(ConstantsSheet);
			HideWorkSheet(FlexCelScaleSheet);
			HideWorkSheet(OptionalTemplatesSheet);
			HideUnselectedTemplateSheets();
		}

		void HideUnselectedTemplateSheets()
		{
			for (var sheetNumber = 0; sheetNumber < XlInterface.WorkSheets.Count; sheetNumber++)
			{
				if (IsTemplateSheet(XlInterface.WorkSheets[sheetNumber].SheetName) && IsDisabledOptionalTemplateSheet(XlInterface.WorkSheets[sheetNumber].SheetName))
				{
					HideWorkSheet(XlInterface.WorkSheets[sheetNumber]);
				}
			}
		}

		internal void SetParametersOnDocDataProvider()
		{
			var parametrizedDocDataProvider = BODocDataProvider as IParametrizedDocWrapper;
			if (parametrizedDocDataProvider != null)
			{
				parametrizedDocDataProvider.Parameters = FilterCollection.ToDictionary();
			}
		}

		void SelectFirstVisibleSheet()
		{
			XlInterface.ActiveWorksheet = 0;
		}

		void HideWorkSheet(ExcelWorkSheet workSheet)
		{
			if (workSheet != null)
			{
				workSheet.Hide();
			}
		}

		static readonly Regex SortSheetNameRegEx = new Regex(@"^\s*sort(ing)?(\s*orders?)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex OptionalTemplateSheetNameRegEx = new Regex(@"^\s*optional templates?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex GroupBySheetNameRegEx = new Regex(@"^\s*groupbys?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex FilterSheetNameRegEx = new Regex(@"^\s*filters?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex TranslationsSheetNameRegEx = new Regex(@"^\s*translations?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex FieldSheetNameRegEx = new Regex(@"^\s*fields?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex ConstantsSheetNameRegEx = new Regex(@"^\s*Constants?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		internal const string FlexCelScaleSheetName = RemotePrinting.Engine.Constants.FlexCelScalingSheetName;
		internal static readonly Regex FlexCelScaleSheetNameRegEx = new Regex("^\\" + FlexCelScaleSheetName + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected ExcelWorkSheet FindSheet(Regex sheetNameRegEx)
		{
			return (ExcelWorkSheet)XlInterface.WorkSheets[sheetNameRegEx].FirstOrDefault();
		}

		#endregion

		public DateTime StartTime { get; private set; }
		public void SetStartTime()
		{
			StartTime = Env.Time.CurrentLocalDateTime;
		}

		#region Data Provider / Analyzer

		internal void SetTableProviderTimeConsumed(TableProvider tableProvider, double timeInSeconds)
		{
			var key = tableProvider.GetType().FullName + tableProvider.DataSourceString;
			if (tableProviderEvaluatedTimeList.ContainsKey(key))
			{
				ErrorManager.Add(new ReportProcessingError(Res.GetString("FD0836F6-9ADC-47D1-B1A5-99A0BF98B9D8", "The SQL data source below appears multiple times in the report '{0}'. To improve the report performance, please define this data source only once:{1}.", this.Name, tableProvider.DataSourceString)
						 , ReportProcessingErrorSeverity.WarningWithoutErrorReport));
			}
			else
			{
				tableProviderEvaluatedTimeList.Add(key, timeInSeconds);
			}
		}

		internal void ResetTableProvidersTimesForTimeout()
		{
			defaultEvaluateTimeOutValue = -1;
			tableProviderEvaluatedTimeList = new Dictionary<string, double>();
		}

		#region Current StmReportRun

		public StmReportRun stmReportRun { get; set; }

		#endregion

		internal double RemainingTimeForTimeout
		{
			get
			{
				if (defaultEvaluateTimeOutValue == -1)
				{
					if (TimeOut > 0)
					{
						defaultEvaluateTimeOutValue = TimeOut;
					}
					else if (Analyser?.Config?.SqlTimeout > 0)
					{
						defaultEvaluateTimeOutValue = Analyser.Config.SqlTimeout;
					}
					else if (IsScheduledReport)
					{
						defaultEvaluateTimeOutValue = DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.Value;
					}
					else
					{
						defaultEvaluateTimeOutValue = DocumentsDataRegistry.Instance.ReportPreviewTimeout.Value;
					}
				}
				var totalElapsedTime = SqlElapsedTime;
				return defaultEvaluateTimeOutValue > totalElapsedTime ? defaultEvaluateTimeOutValue - totalElapsedTime : 0;
			}
		}
		int defaultEvaluateTimeOutValue = -1;

		internal double SqlElapsedTime
		{
			get
			{
				return tableProviderEvaluatedTimeList.Sum(evaluate => evaluate.Value);
			}
		}

		Dictionary<string, double> tableProviderEvaluatedTimeList = new Dictionary<string, double>();

		internal DataContextValue DataContextValue
		{
			get
			{
				if (fDataContextValue == DataContextValue.None)
				{
					fDataContextValue = Analyser.Config.DataContextValue;
				}
				return fDataContextValue;
			}
		}
		DataContextValue fDataContextValue;

		internal virtual ReportAnalyser GetNewReportAnalyser()
		{
			return new ReportAnalyser(this);
		}

		internal IDataProvider DataProvider
		{
			get { return fDataProvider ?? (fDataProvider = GetNewDataProvider()); }
		}
		IDataProvider fDataProvider;

		IDataProvider GetNewDataProvider()
		{
			return DataProviderFactory.GetDataProvider(this);
		}

		internal void ResetDataProvider()
		{
			fDataProvider = null;
			ResetTableProvidersTimesForTimeout();
		}

		internal void ReGenerateTemplate()
		{
			if (!MenuTemplatePivotPK.IsEmpty)
			{
				var pivot = Factory.Load<StmMenuTemplatePivotBase>(MenuTemplatePivotPK);
				var templateGenerator = Parent.GetTemplateGenerator(pivot);
				Template = Parent.SharedTemplateInPack ?? templateGenerator.Generate(FilterEvaluator);
			}
		}

		public ExcelTemplate Template
		{
			get => template;
			private set
			{
				template = value;
				fXlInterface = null;
			}
		}
		ExcelTemplate template;

		internal FilterEvaluator FilterEvaluator { get; set; }

		public MultilingualString CustomWatermarkText { get; set; }

		public bool UseJsEvaluator { get; }

		internal ReportAnalyser Analyser { get; set; }

		IReportRenderer renderer;
		internal IReportRenderer Renderer
		{
			get
			{
				return renderer ?? (renderer = new ReportRenderer(this));
			}
			set
			{
				renderer = value;
			}
		}

		internal void AnalyzeResettingLoadedFlagsAfterwards()
		{
			Analyser = GetNewReportAnalyser();
			Analyser.Analyse();
			FilterLoadedByReportAnalyser = false;
			SortOrderLoadedByReportAnalyser = false;
			GroupBysLoadedByReportAnalyser = false;
			OptionalTemplatesLoadedByReportAnalyser = false;
			synchronisedWithDeserialisedReport = false;
		}

		public PageStyles GetPageStyle()
		{
			return Analyser.Config.PageStyle;
		}

		public bool CanSpecifyPageRanges => XlInterfaceSheetCount == 1 && SectionBodyCount == 1 && DataRowSourceRowCountIfPageRangesSpecifiable > 1;

		int XlInterfaceSheetCount
		{
			get
			{
				if (fXlInterfaceSheetCount == -1)
				{
					try
					{
						fXlInterfaceSheetCount = XlInterface.WorkSheets.Count(workSheet => IsTemplateSheet(workSheet.SheetName));
					}
					finally
					{
						if (fXlInterface != null)
						{
							fXlInterface.Dispose();
							fXlInterface = null;
						}
					}
				}
				return fXlInterfaceSheetCount;
			}
		}
		int fXlInterfaceSheetCount = -1;

		int SectionBodyCount
		{
			get
			{
				if (fSectionBodyCount == -1)
				{
					try
					{
						if (!IsPreparedForRender)
						{
							UpdateAndSynchroniseFilters();
							RegisterDocumentAndReportRelatedMacroProviders();
						}
						fSectionBodyCount = Analyser.Areas.OfType<SectionBodyArea>().Count();
						if (fSectionBodyCount == 1)
						{
							originalDataRowSourceIfPageRangesSpecifiable = Analyser.Areas.OfType<SectionBodyArea>().First().DataRowSource;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorManager.ReportFatalException(ex);
					}
				}
				return fSectionBodyCount;
			}
		}
		int fSectionBodyCount = -1;

		public int DataRowSourceRowCountIfPageRangesSpecifiable => SectionBodyCount == 1 ? originalDataRowSourceIfPageRangesSpecifiable?.RowCount ?? 0 : 0;

		public void UpdateSpecifiedDataRowSource(int[] indexes)
		{
			specifiedDataRowSourceChanged = true;
			if (indexes == null || indexes.Length == 0 || originalDataRowSourceIfPageRangesSpecifiable == null)
			{
				SpecifiedDataRowSource = null;
			}
			else
			{
				SpecifiedDataRowSource = originalDataRowSourceIfPageRangesSpecifiable.GetRowsFromIndexes(indexes);
			}
		}

		IDataRowSource originalDataRowSourceIfPageRangesSpecifiable;

#if DEBUG
		public void SetOriginalDataRowSourceIfPageRangesSpecifiableForTest(IDataRowSource dataRowSource)
		{
			originalDataRowSourceIfPageRangesSpecifiable = dataRowSource;
		}
#endif

		bool specifiedDataRowSourceChanged;

		public IDataRowSource SpecifiedDataRowSource
		{
			get
			{
				return PageRangesSpecified ? fSpecifiedDataRowSource : null;
			}
			private set
			{
				fSpecifiedDataRowSource = value;
			}
		}
		IDataRowSource fSpecifiedDataRowSource;

		public bool PageRangesSpecified { get; set; }

		internal MacroTranslator MacroTranslator
		{
			get
			{
				if (fMacroTranslator == null)
				{
					fMacroTranslator = new MacroTranslator(this);

					RegisterUsedUDFFields(fMacroTranslator);

					foreach (var templateDefinedConstant in TemplateDefinedConstants)
					{
						fMacroTranslator.RegisterValueProvider(new FixedValueProvider(templateDefinedConstant.Key, templateDefinedConstant.Value));
					}
				}
				return fMacroTranslator;
			}
		}

		MacroTranslator fMacroTranslator;

		void RegisterUsedUDFFields(MacroTranslator fMacroTranslator)
		{
			if (UserDefinedFieldValueList != null)
			{
				List<string> usedUDFFields = null;

				if (Template != null)
				{
					usedUDFFields = new List<string>();
					if (UDFSheet != null)
					{
						var node = new StringTreeBuilder(UDFSheet).GetTree();
						foreach (StringTreeNode fieldDef in node.Children)
						{
							usedUDFFields.Add(fieldDef.Value);
						}
					}
				}

				if (usedUDFFields == null || usedUDFFields.Count > 0)
				{
					foreach (FilterField field in UserDefinedFieldValueList)
					{
						if (usedUDFFields == null || usedUDFFields.Contains(field.DisplayName))
						{
							if (!field.IsOverriddenInDocData || DisableFixedValueCache)
							{
								foreach (var valueProvider in field.ValueProviders)
								{
									fMacroTranslator.RegisterValueProvider(valueProvider);
								}
							}
							else
							{
								foreach (DelegateValueProvider valueProvider in field.ValueProviders)
								{
									if (valueProvider.MacroName == field.DisplayName)
									{
										fMacroTranslator.RegisterValueProvider(new FixedValueProvider(valueProvider.MacroName, field.ValueAsObject));
									}
									else
									{
										fMacroTranslator.RegisterValueProvider(new FixedValueProvider(valueProvider.MacroName, GetFixedValueForFieldsExtendedValueProvider(valueProvider, field)));
									}
								}
							}
						}
					}
				}
			}
		}

		object GetFixedValueForFieldsExtendedValueProvider(DelegateValueProvider valueProvider, FilterField field)
		{
			try
			{
				return valueProvider.GetReplacement("<" + valueProvider.MacroName + ">", this);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"ErrorWhenGettingFixedValueForFieldsExtendedValueProvider - {field.GetType().Name} - {valueProvider.MacroName}"),
					FormattableString.Invariant($@"An error occurs when getting <{valueProvider.MacroName}> value for field {{{field.DisplayName}}}.
{ToString()}")
					, e);
				return field.ValueAsObject;
			}
		}

		#endregion

		#region DocData

		public DataProviderList DataProviderList { get; private set; }

		public IBODocDataProvider BODocDataProvider
		{
			get { return DataProviderList == null ? null : DataProviderList.PrimaryDataProvider; }
		}

		internal ZString DocTypeCode { get; set; }

		internal DocBaseWrapperBaseWithImageSupport ImageSupportWrapper
		{
			get { return BODocDataProvider as DocBaseWrapperBaseWithImageSupport; }
		}

		[BusinessObjectTestExclude]
		public UserControlProviderList UserDefinedFieldValueList
		{
			get { return fUserDefinedFieldValueList; }
		}

		[BusinessObjectTestExclude]
		internal UserControlProviderList UserDefinedFieldList
		{
			get { return fUserDefinedFieldList; }
			set
			{
				if (fUserDefinedFieldList != null)
				{
					UnRegisterEditableChildObject(fUserDefinedFieldList);
				}
				fUserDefinedFieldList = value;
				RegisterEditableChildObject(fUserDefinedFieldList);
			}
		}

		readonly UserControlProviderList fUserDefinedFieldValueList;
		UserControlProviderList fUserDefinedFieldList;

		#endregion

		#region Errors

		bool ContinueToGenerateReportRegardlessOfWarnings = true;

		#region ThrowDocumentEngineExceptionForTesting 
#if DEBUG
		void ThrowDocumentEngineExceptionForTesting()
		{
			if (Globals.IsTest && !((IReportForUnitTesting)this).StopErrorsThrowingAnException && !ObjectFactory.Contains(typeof(IExceptionWhenRenderAndSave)) && ErrorManager.HasErrors)
			{
				throw new DocumentEngineException("Errors found generating template:-\r\n" + ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell Content: [{5}]  Cell: [{2}] Sheetname: [{3}]", true));
			}
		}
#endif
		#endregion

		void ShowErrorsIfAny()
		{
			if (ErrorManager.HasErrors)
			{
				#region Test
#if DEBUG
				if (generateRegardlessOfAnyErrorsForTesting)
				{
					return;
				}

				ThrowDocumentEngineExceptionForTesting();
#endif
				#endregion

				ErrorManager.ReportErrors();

				if (!ErrorManager.HasWarningsOnly)
				{
					ContinueToGenerateReportRegardlessOfWarnings = false;
					if (ScheduleTask != null)
					{
						ScheduleTask.S5_IsActive = false;
					}
				}

				if (Parent?.Parent?.CustomNotifications != null)
				{
					Parent.Parent.CustomNotifications.Add(ErrorManager.GetNotificationType(), ErrorManager.ToString());
					ContinueToGenerateReportRegardlessOfWarnings = false;
				}
				else if (Globals.CanShowDialogs)
				{
					using (ObjectFactory.New<INeedToShowMessage>().SuppressNewFormInTransactionWarning())
					{
						if (!PrintTaskUIProvider.ShowErrors(this))
						{
							ContinueToGenerateReportRegardlessOfWarnings = false;
						}
					}
				}
				else if (ScheduleTaskNotifications != null)
				{
					ScheduleTaskNotifications.Add(ErrorManager.GetNotificationType(), ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}] TemplatePath: [{4}]", true));
				}
			}
		}

		public IPrintTaskUIProvider PrintTaskUIProvider
		{
			get { return printTaskUIProvider ?? (printTaskUIProvider = PrintTaskUIProviderFactory.Create()); }
		}

		IPrintTaskUIProvider printTaskUIProvider;

		#endregion

		#region Excel Interface

		/// <summary>
		/// *** OBOSLETE *** PLEASE DO NOT USE THIS PROPERTY ANY LONGER, AS THIS WILL BE REMOVED IN THE FUTURE.
		/// </summary>
		public ExcelInterface XlInterface
		{
			get
			{
				if (fXlInterface == null && Template != null)
				{
					fXlInterface = Template.GetNewExcelInterface();
				}

				return fXlInterface;
			}
		}

		ExcelInterface fXlInterface;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			try
			{
				ResetAnalyserRendererAndExcelInterface();
				ResetCachedExcelFile();
			}
			finally
			{
				IsDisposed = true;
			}
		}

		internal void ResetAnalyserRendererAndExcelInterface()
		{
			if (Analyser != null)
			{
				Analyser.Reset();
				Analyser = null;
			}

			if (Renderer != null)
			{
				Renderer = null;
				ResetIsPreparedForRender();
			}

			if (fWorkSheetCurrentlyBeingProcessed != null)
			{
				fWorkSheetCurrentlyBeingProcessed.Dispose();
				fWorkSheetCurrentlyBeingProcessed = null;
			}

			if (fXlInterface != null)
			{
				fXlInterface.Dispose();
				fXlInterface = null;
			}
		}

		internal virtual void ResetCachedExcelFile()
		{
			if (cachedXlInterface != null)
			{
				var xlInterface = cachedXlInterface.Target as IDisposable;
				if (xlInterface != null)
				{
					xlInterface.Dispose();
				}
				cachedXlInterface = null;
				cachedSheetsVisibility = null;
			}
		}

		#endregion

		#region IDeliverable Members

		public static string CustomFieldEmailFromAddress
		{
			get { return "EmailFromAddress"; }
		}

		public string GetEmailFromAddress()
		{
			if (MenuItem != null && !string.IsNullOrEmpty(MenuItem.SU_EmailSenderOverride))
			{
				return MenuItem.SU_EmailSenderOverride;
			}

			if (BODocDataProvider != null)
			{
				var customEmailFromAddress = BODocDataProvider.GetCustomField(CustomFieldEmailFromAddress).ToString();
				if (!string.IsNullOrEmpty(customEmailFromAddress))
				{
					return customEmailFromAddress;
				}
			}

			if (ImageSupportWrapper != null)
			{
				return ImageSupportWrapper.BrandEmailAddress;
			}

			return string.Empty;
		}

		public DeliveryInfo GetDeliveryInfo(bool isDraft)
		{
			var info = new DeliveryInfo(Style == Styles.Document ? DeliveryInfo.DeliveryFormats.Document : DeliveryInfo.DeliveryFormats.Report);
			if (MenuItem != null)
			{
				info.LineSpacing = MenuItem.SU_FlexCelLineSpacing;
				info.IsLocalDocument = MenuItem.IsLocalDocument;
				info.AllowRawView = MenuItem.SU_AllowRawView;
				info.SignBy = MenuItem.SU_SignBy;
			}

			if (IsDeliveredByEmail)
			{
				info.EmailFromAddress = GetEmailFromAddress();
				info.EmailSignature = EmailFormatter.GetEmailSignature(RegistryEmailFormat.EmailSignatureFields);
			}

			info.ParentPivotPK = SourcePivotPK;
			info.EmailSubjectLine = EmailSubject;

			info.Name = Name;
			info.SheetNames.Clear();
			SheetNames.ForEach(info.SheetNames.Add);
			info.AttachedFilename = EmailFormatter.GetDocumentName();

			info.TrailingSpace = TrailingSpace;
			info.Copies = PrinterDetails.NumberOfCopies > short.MaxValue ? short.MaxValue : (short)PrinterDetails.NumberOfCopies;
			info.PrintQueue = PrinterDetails.PrintQueue;
			info.CustomWatermarkText = CustomWatermarkText;

			if (BODocDataProvider != null)
			{
				if (Parent.BusinessObjectToLogAgainst == null)
				{
					Parent.ForceBusinessObjectToLogAgainst(BODocDataProvider.BusinessObjectToLogAgainst);
				}
				info.DocumentType = DocTypeCode;
			}

			var businessObjectToLogAgainst = GetBusinessObjectToLogAgainst();
			if (businessObjectToLogAgainst != null)
			{
				info.BusinessObjectPk = businessObjectToLogAgainst.PK;
			}

			if (Parent.BusinessObjectForPrintJob != null)
			{
				IDocManagerSupport docManagerSupport = null;
				var parentDocManagerSupport = Parent.BusinessObjectForPrintJob as IParentDocManagerSupport;
				if (parentDocManagerSupport != null)
				{
					info.ParentGuid = parentDocManagerSupport.ParentGuid;
					info.ParentTableName = parentDocManagerSupport.ParentTableName;
					docManagerSupport = parentDocManagerSupport;
				}
				else
				{
					if (Parent.BusinessObjectForPrintJob is IEDocsPluginHostDecider parentHostDecider && parentHostDecider.HostBusinessEntity != null)
					{
						info.ParentGuid = parentHostDecider.HostBusinessEntity.Identifier;
						info.ParentTableName = parentHostDecider.HostBusinessEntity.TableName;
					}
					else
					{
						info.ParentGuid = Parent.BusinessObjectForPrintJob.PK;
						info.ParentTableName = Parent.BusinessObjectForPrintJob.TableName;
					}

					docManagerSupport = Parent.BusinessObjectForPrintJob as IDocManagerSupport;
				}

				if (docManagerSupport != null)
				{
					var docManagerInfo = docManagerSupport.DocManagerInfo;
					if (docManagerInfo.ShouldRecordDocument(MenuItem))
					{
						info.RelatedBusinessContext = docManagerInfo.DocManagerCode;
					}
				}
			}

			info.IsCoverSheet = BODocDataProvider is DeliveryInstructions;

			if (Analyser != null && Analyser.Config != null && Analyser.Config.SuppressDraftWatermark)
			{
				info.ShowDraftWatermark = false;
			}
			else
			{
				info.ShowDraftWatermark = isDraft;
			}

			info.Protector = Protector;

			return info;
		}

		void SwitchFromXLSToXLSX()
		{
			if (this.DeliveryContact != null)
			{
				this.DeliveryContact.IsFormatSwitchingRequired = true;
			}
		}

		ZString IDeliverable.FileExtension
		{
			get
			{
				var extension = DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.Value;
				using (SuspendSetDeliveryContactsDataWasAccessed())
				{
					if (this.DeliveryContact != null && this.DeliveryContact.IsFormatSwitchingRequired)
					{
						extension = AttachmentTypeList.Codes.Xlsx;
					}
					else if (AttachmentType == AttachmentTypeList.Codes.Xls || AttachmentType == AttachmentTypeList.Codes.Xlsx)
					{
						extension = AttachmentType;
					}
				}

				if (DisableXLSXExport && extension == AttachmentTypeList.Codes.Xlsx)
				{
					extension = AttachmentTypeList.Codes.Xls;
				}
				return extension;
			}
		}

		IDisposable SuspendSetDeliveryContactsDataWasAccessed()
		{
			suspendSetDeliveryContactsDataWasAccessed = true;
			return new DisposableAction(() => { suspendSetDeliveryContactsDataWasAccessed = false; });
		}
		bool suspendSetDeliveryContactsDataWasAccessed;

		internal TFileFormats FileFormat => ExcelInterface.GetTFileFormat(((IDeliverable)this).FileExtension);

		bool DisableXLSXExport
		{
			get
			{
				return DisableXlsxExportInfo.Any(p => !IsDisabledOptionalTemplateSheet(p.Key) && p.Value);
			}
		}

		Dictionary<string, bool> DisableXlsxExportInfo
		{
			get { return disableXlsxExportInfo ?? (disableXlsxExportInfo = new Dictionary<string, bool>()); }
		}

		Dictionary<string, bool> disableXlsxExportInfo;

		internal ZString AttachmentType
		{
			get
			{
				using (SuspendSetDeliveryContactsDataWasAccessed())
				{
					var attachmentType = string.Empty;
					if (this.DeliveryContact != null)
					{
						attachmentType = this.DeliveryContact.AttachmentType;
					}
					return attachmentType;
				}
			}
		}

		ZBool IDeliverable.CoverSheetRequired
		{
			get { return coverSheetRequired; }
		}

		bool coverSheetRequired;

		internal void SetCoverSheetRequired(bool value)
		{
			coverSheetRequired = value;
		}

		internal ZBool IsCoverSheet;

		ZGuid IDeliverable.DeliveryGroupID { get; set; }

		public bool IsDeliveredByEmail { get; set; }

		#endregion

		#region IDeliveryEmailAttachment Members

		long IDeliveryEmailAttachment.FileSizeInBytes => Template?.FileSizeInBytes ?? default(long);

		string IDeliveryEmailAttachment.FileName => Name;

		bool IDeliveryEmailAttachment.ShouldBeAttached => IncludedInPrint;

		#endregion

		#region IDocument Members

		string IDocument.DocumentDeliveryMethod
		{
			get { return PrintCopyType.ToString(); }
		}

		string IDocument.DocumentName
		{
			get { return Name; }
		}

		bool IDocument.IncludeInPrint
		{
			get { return IncludedInPrint; }
		}

		bool IDocument.CanIncludeInPrint
		{
			get { return canIncludeInPrint; }
			set { canIncludeInPrint = value; }
		}
		bool canIncludeInPrint = true;

		#endregion

		#region ISerializable Members

		bool synchronisedWithDeserialisedReport;

		public Report DeserializedReport
		{
			get
			{
				return deserializedReport;
			}
			set
			{
				if (value != null && deserializedReport != value && IsAnalyzed)
				{
					ErrorReporter.ReportOnce("Report Serialization", "Deserialized report shouldn't be changed once report is analyzed as we may have several macros which referenced the saved SortOrder/GroupBy option.");
				}
				deserializedReport = value;
			}
		}
		Report deserializedReport;

		public bool ShouldUpdateSchedulableFilters { get; set; } = true;

		internal void SynchroniseReportDataWithDeserialisedReport()
		{
			if (!synchronisedWithDeserialisedReport && (DeserializedReport != null))
			{
				UpdateFilterCollectionOnReportFromDeserializedReport();
				UpdateSortOrderCollectionOnReportFromDeserializedReport();
				UpdateGroupByCollectionOnReportFromDeserializedReport();
				UpdateOptionalTemplateSheetCollectionOnReportFromDeserializedReport();
				ColumnHeadingManager.UpdateFromDeserialisedValue(DeserializedReport.ColumnHeadingManager, this);
				OverrideReportDbOption = DeserializedReport.OverrideReportDbOption;
				Orientation = DeserializedReport.Orientation;
				TimeOut = DeserializedReport.TimeOut;
				IsEdwDataSource = DeserializedReport.IsEdwDataSource;
				synchronisedWithDeserialisedReport = true;
				MaxDop = DeserializedReport.MaxDop;
			}
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new DocumentJsonData
			{
				ReportName = fName,
				FilterCollection = (CollectionOfIFilterJsonData)fFilterCollection?.GetJsonData(),
				OptionalTemplateSheetCollection = (OptionalTemplateSheetCollectionJsonData)fOptionalTemplateSheetCollection?.GetJsonData(),
				SelectedSortOrder = (SortOrderJsonData)fSortOrderCollection?.SelectedOrder?.GetJsonData(),
				SelectedGroupBy = (GroupByJsonData)fGroupByCollection?.SelectedGroupBy?.GetJsonData(),
				BreakPageOverride = fGroupByCollection?.BreakPageOverride ?? false,
				ColumnHeadingManager = (ColumnConfigurationsManagerJsonData)ColumnHeadingManager?.GetJsonData(),
				OverrideReportDbOption = OverrideReportDbOption,
				PageOrientation = Orientation,
				TimeOut = TimeOut,
				IsEdwDataSource = IsEdwDataSource,
				ShouldUpdateSchedulableFilters = ShouldUpdateSchedulableFilters,
				MaxDop = MaxDop
			};

		#endregion

		internal Dictionary<string, HashSet<string>> FieldNotFoundErrorsForFullyQualifiedMacros { get; } = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		#region SuppressResourceStringsCheckRegion
		void ReportFieldNotFoundErrorsForFullyQualifiedMacros()
		{
			if (FieldNotFoundErrorsForFullyQualifiedMacros.Any() && !ContainsAnyCustomisation)
			{
				var key = MenuItem == null ? "" : MenuItem.SU_MenuName + " - " + MenuItem.SU_BusinessContext;
				ErrorReporter.ReportOnce("FieldNotFoundErrorsForFullyQualifiedMacros - " + key, GetFormattedErrors());
			}

			string GetFormattedErrors()
			{
				var result = new ZStringBuilder();
				result.AppendLine("****If you see this issue please assign it to team responsible of that document.****");
				result.AppendLine();
				result.AppendLine("FieldNotFound errors occurred when translating the macros below inside SectionBodyArea.");
				result.AppendLine("-----------------------------------------------------------------------------------");
				var prefix = string.Empty;
				FieldNotFoundErrorsForFullyQualifiedMacros.ForEach(kv =>
				{
					prefix = kv.Key;
					result.AppendLine($"SectionBody:Data={kv.Key}");
					kv.Value.ForEach(v => { result.AppendLine($"    {v}"); });
				});
				result.AppendLine("-----------------------------------------------------------------------------------");
				result.AppendLine($"Please remove the prefix (e.g. '{prefix}.' in the macro above) first, then if you still see this error, either expose the properties from the BO in its wrapper or use other properties from the wrapper.");
				result.AppendLine();
				result.AppendLine($"{this}");
				return result.ToString();
			}
		}
		#endregion

		public ReportErrorManager ErrorManager
		{
			get { return errorManager ?? (errorManager = new ReportErrorManager(this)); }
		}
		ReportErrorManager errorManager;

		readonly List<TemplateGenerationError> templateGenerationErrors = new List<TemplateGenerationError>();
		internal List<TemplateGenerationError> TemplateGenerationErrors
		{
			get { return templateGenerationErrors; }
		}

		IDocumentRendererLogger documentRendererLogger;
		internal IDocumentRendererLogger DocumentRendererLogger
		{
			get { return documentRendererLogger ?? (documentRendererLogger = new DummyDocumentRendererLogger()); }
			set { documentRendererLogger = value; }
		}

		public ZGuid SourcePivotPK { get; set; }

		public ZGuid MenuTemplatePivotPK { get { return menuTemplatePivotPK; } }
		ZGuid menuTemplatePivotPK;

		public bool ShouldPrintByDefault { get; set; } = true;

		public ZByte Index { get; set; }

		public BusinessObject ParentBusinessObject => parentBusinessObject ??= DataProviderList?.PrimaryDataProvider?.ParentBusinessObject;
		BusinessObject parentBusinessObject;
		public ZGuid IdentifiablePK
		{
			get
			{
				if (ParentBusinessObject is IParentDocManagerSupport parentDocManagerSupport)
				{
					return parentDocManagerSupport.ParentGuid;
				}

				if (ParentBusinessObject is ISourceIdentifierProvider provider)
				{
					return provider.SourceIdentifier;
				}

				return ParentBusinessObject?.PK ?? ZGuid.Empty;
			}
		}

		public ZString Identifier
		{
			get
			{
				var key = string.Join("+",
					MenuTemplatePivotPK,
					IdentifiablePK,
					Name,
					DocumentTypeCode
					);
				using var md5 = MD5.Create();
				return Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(key)));
			}
		}

		public bool IsIdentifiablePKValid
		{
			get
			{
				if (!IdentifiablePK.IsValid)
				{
					return false;
				}
				if (ParentBusinessObject?.PK != IdentifiablePK)
				{
					return true;
				}
				return ParentBusinessObject != null && ParentBusinessObject.IsInDatabase;
			}
		}

		protected override void AddToFactoryCache()
		{
			// Don't cache reports in the factory - we don't get them back
		}

		#region MAXDOP

		ZInt maxDop;

		public ZInt MaxDop
		{
			get { return maxDop; }
			set
			{
				SetNonPersistentPropertyValue(MaxDopInfo, ref maxDop, value);
			}
		}

		public ZPropertyInfo MaxDopInfo
		{
			get { return GetZPropertyInfo(nameof(MaxDop)); }
		}

		#endregion
	}

	#region Test

#if DEBUG

	public interface IExceptionWhenRenderAndSave
	{
		void Throw();
	}

#endif

	partial class Report // Only add [Conditional("DEBUG")] methods in here.
	{
		[Conditional("DEBUG")]
		void IncrementManagerLoadHitCountForTesting()
		{
#if DEBUG
			ManagerLoadHitCountForTest++;
#endif
		}

		[Conditional("DEBUG")]
		void SetOkToGenerateRegardlessOfErrorsForTesting(ref bool isOkToGenerate)
		{
#if DEBUG
			if (generateRegardlessOfAnyErrorsForTesting)
			{
				isOkToGenerate = true;
			}
#endif
		}

		[Conditional("DEBUG")]
		static void AddToRenderedWorkSheetsForTesting(ExcelWorkSheet workSheet)
		{
#if DEBUG
			if (RenderedWorkSheetsForTesting != null)
			{
				RenderedWorkSheetsForTesting.Add(workSheet);
			}
#endif
		}
	}

#if DEBUG

	internal interface IReportForUnitTesting : IDisposable
	{
		bool IsPreparedForRender { get; set; }
		bool SynchronisedWithDeserialisedReport { get; }
		ExcelInterface XlInterfaceDirect { get; }

		string fName { get; set; }
		int TrailingSpace { get; }

		void UpdateFilterCollectionOnReportFromDeserializedReport();
		void UpdateGroupByCollectionOnReportFromDeserializedReport();
		void SynchroniseReportDataWithDeserialisedReport();
		bool StopErrorsThrowingAnException { get; set; }
		bool GenerateRegardlessOfAnyErrors { get; set; }
		void ShowErrorsIfAny();

		DocDeliveryContact DeliveryContact { get; set; }
		DocDeliveryContact MostOfficialContact { get; set; }

		void SetBusinessObjectForTesting(IBODocDataProvider docDataProvider);

		void SetMenuTemplatePivotPK(ZGuid pK);
	}

	interface IReportForReportDbTesting
	{
		void SetReportDbManagerForTesting(ISecondaryServerConnectionProvider testReportDbManager);
	}

	partial class Report : IReportForUnitTesting, IReportForReportDbTesting
	{
		public IDisposable SuspendFilterValidationCheckingForTesting()
		{
			return new TemporaryValueSetter<bool>(value => FilterValidationCheckingSuspendedForTesting = value, FilterValidationCheckingSuspendedForTesting, true);
		}

		internal bool FilterValidationCheckingSuspendedForTesting;

		public int ManagerLoadHitCountForTest;

		[ThreadStatic]
		internal static List<ExcelWorkSheet> RenderedWorkSheetsForTesting;

		internal bool IsPreparedForRenderForTesting
		{
			get { return IsPreparedForRender; }
		}

		internal static Report NewForTesting(DocumentPack pack)
		{
			return new Report(pack, Styles.Report);
		}

		// These are defined in DEBUG mode only on the Interface. Wierd. One day I might try and remove, is a bit of a hack.
		void IDeliverable.DeleteTempFilesForTesting() { }
		int IDeliverable.RunCountForTesting { get { return 0; } }

		DocDeliveryContact IReportForUnitTesting.DeliveryContact
		{
			get { return DeliveryContact; }
			set { DeliveryContact = value; }
		}

		DocDeliveryContact IReportForUnitTesting.MostOfficialContact
		{
			get { return MostOfficialContact; }
			set { MostOfficialContact = value; }
		}

		string IReportForUnitTesting.fName
		{
			get { return Name; }
			set { Name = value; }
		}

		int IReportForUnitTesting.TrailingSpace
		{
			get { return TrailingSpace; }
		}

		bool IReportForUnitTesting.IsPreparedForRender
		{
			get { return IsPreparedForRender; }
			set { IsPreparedForRender = value; }
		}

		ExcelInterface IReportForUnitTesting.XlInterfaceDirect
		{
			get { return fXlInterface; }
		}

		bool IReportForUnitTesting.SynchronisedWithDeserialisedReport
		{
			get { return synchronisedWithDeserialisedReport; }
		}

		void IReportForUnitTesting.UpdateFilterCollectionOnReportFromDeserializedReport()
		{
			UpdateFilterCollectionOnReportFromDeserializedReport();
		}

		void IReportForUnitTesting.UpdateGroupByCollectionOnReportFromDeserializedReport()
		{
			UpdateGroupByCollectionOnReportFromDeserializedReport();
		}

		void IReportForUnitTesting.SynchroniseReportDataWithDeserialisedReport()
		{
			SynchroniseReportDataWithDeserialisedReport();
		}

		static bool stopErrorsThrowingAnExceptionForTesting;
		bool IReportForUnitTesting.StopErrorsThrowingAnException
		{
			get { return stopErrorsThrowingAnExceptionForTesting; }
			set { stopErrorsThrowingAnExceptionForTesting = value; }
		}

		public static IDisposable TemporarilyStopErrorsThrowingAnException()
		{
			return new TemporaryValueSetter<bool>(
				value => stopErrorsThrowingAnExceptionForTesting = value,
				stopErrorsThrowingAnExceptionForTesting,
				true);
		}

		bool generateRegardlessOfAnyErrorsForTesting;
		bool IReportForUnitTesting.GenerateRegardlessOfAnyErrors
		{
			get { return generateRegardlessOfAnyErrorsForTesting || temporarilyGenerateRegardlessOfAnyErrors; }
			set { generateRegardlessOfAnyErrorsForTesting = value; }
		}

		void IReportForUnitTesting.SetMenuTemplatePivotPK(ZGuid pK)
		{
			menuTemplatePivotPK = pK;
		}

		[ThreadStatic]
		static bool temporarilyGenerateRegardlessOfAnyErrors;
		internal static IDisposable TemporarilyGenerateRegardlessOfAnyErrors()
		{
			return new TemporaryValueSetter<bool>(
				value => { temporarilyGenerateRegardlessOfAnyErrors = value; },
				temporarilyGenerateRegardlessOfAnyErrors,
				true);
		}

		void IReportForUnitTesting.ShowErrorsIfAny()
		{
			ShowErrorsIfAny();
		}

		void IReportForUnitTesting.SetBusinessObjectForTesting(IBODocDataProvider docDataProvider)
		{
			this.DataProviderList = new DataProviderList(docDataProvider);
		}

		void IReportForReportDbTesting.SetReportDbManagerForTesting(ISecondaryServerConnectionProvider testReportDbManager)
		{
			reportdbManager = testReportDbManager;
		}
	}

#endif

	#endregion
}
