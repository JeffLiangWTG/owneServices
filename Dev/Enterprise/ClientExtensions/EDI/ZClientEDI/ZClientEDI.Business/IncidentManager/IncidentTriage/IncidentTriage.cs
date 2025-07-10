using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeProperty(IncidentTriageSchema.Constants.IMT_TriageNumber), DescriptionProperty(IncidentTriageSchema.Constants.IMT_SupportDescription)]
	public class IncidentTriage : AutoIncidentTriage, IWorkflowProvider
	{
		public IncidentTriage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IMT_Level = ZString.Empty;
		}

		protected override ZString HumanReadableNameCore => SingularName;

		protected override ZString HumanReadableShortcutNameCore => $"{SingularName} - {IMT_SupportDescription}";

		public static ResourceString SingularName
		{
			get { return ResString.GetMultilingualString("56640616-f6be-41f2-8675-6dcbe050d3d2", "Incident Triage"); }
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		#endregion

		#region Properties

		[ResourceStringData("IncidentTriage|IMT_SupportDescription", Caption = "Support Description")]
		public override ZString IMT_SupportDescription { get => base.IMT_SupportDescription; set => base.IMT_SupportDescription = value; }

		[List("Lookups.Types")]
		[ResourceStringData("IncidentTriage|IMT_Type", Caption = "Node Type")]
		public override ZString IMT_Type
		{
			get => base.IMT_Type;
			set => base.IMT_Type = value;
		}

		[List("Lookups.Levels")]
		[ResourceStringData("IncidentTriage|IMT_Level", Caption = "Node Level")]
		public override ZString IMT_Level
		{
			get => base.IMT_Level;
			set => base.IMT_Level = value;
		}

		public ZString LevelDescription
		{
			get { return Lookups.Levels.GetDescriptionFromCode(IMT_Level); }
		}

		public ZPropertyInfo LevelDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LevelDescription)); }
		}

		#region Type Description

		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(IMT_Type); }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		#endregion

		#region Published

		public bool IsPublishedPropertiesReadOnly => !EDISecurityCheckpoints.CustomerServiceIncidentTriagePublishAccess.IsAllowed;

		[ResourceStringData("IncidentTriage|IMT_IsPublished", Caption = "Publish to eRequest")]
		public override ZBool IMT_IsPublished
		{
			get => base.IMT_IsPublished;
			set
			{
				base.IMT_IsPublished = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePublishedDescriptionText();
				}
			}
		}

		public bool IMT_IsPublished_ReadOnly => IsPublishedPropertiesReadOnly;

		[ResourceStringData("IncidentTriage|PublishedDescriptionText", Caption = "Published Description")]
		public virtual ZString PublishedDescriptionText
		{
			get => PublishedDescription.Text;
			set
			{
				CheckMaximumLength(PublishedDescriptionTextInfo, value);
				PublishedDescription.Text = value;
				PublishedDescriptionTextInfo.RefreshBinding();
				HasChanges = true;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePublishedDescriptionText();
				}
			}
		}

		public ZPropertyInfo PublishedDescriptionTextInfo
		{
			get { return GetZPropertyInfo(nameof(PublishedDescriptionText)); }
		}

		public int PublishedDescriptionText_MaxLength
		{
			get { return EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription.TextOnlyMaxLength; }
		}

		UniqueNote PublishedDescription
		{
			get
			{
				if (publishedDescription == null)
				{
					publishedDescription = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription);
				}
				return publishedDescription;
			}
		}

		UniqueNote publishedDescription;

		#endregion

		[ResourceStringData("IncidentTriage|IMT_IsPublishedToAssist", Caption = "Publish to Triage Assist")]
		public override ZBool IMT_IsPublishedToAssist { get => base.IMT_IsPublishedToAssist; set => base.IMT_IsPublishedToAssist = value; }

		public bool IMT_IsPublishedToAssist_ReadOnly => IsPublishedPropertiesReadOnly;

		#region Support Notes

		UniqueNote SupportNotes
		{
			get
			{
				if (supportNotes == null)
				{
					supportNotes = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentTriageSupportNotes);
				}
				return supportNotes;
			}
		}
		UniqueNote supportNotes;

		[ResourceStringData("IncidentTriage|SupportNotesAsBlob", Caption = "Support Notes")]
		public ZBlob SupportNotesAsBlob
		{
			get { return SupportNotes != null ? SupportNotes.Blob : ZBlob.Empty; }
			set
			{
				if (SupportNotes == null || SupportNotes.Blob == value)
				{
					return;
				}

				SupportNotes.Blob = value;
				SupportNotesAsBlobInfo.RefreshBinding();
				HasChanges = true;
			}
		}

		public ZBlob SupportNotesAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(SupportNotesAsBlob);
			}
			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				SupportNotesAsBlob = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo SupportNotesAsBlobInfo
		{
			get { return GetZPropertyInfo(nameof(SupportNotesAsBlob)); }
		}

		#endregion

		#region ModuleType

		public ModuleListType ModuleType
		{
			get
			{
				switch (IMT_Type)
				{
					case IncidentTriageTypes.Codes.Compliance:
						return ModuleListType.Cr8;
					case IncidentTriageTypes.Codes.Service:
						return ModuleListType.Cr9;
				}

				return ModuleListType.MenuSection;
			}
		}

		#endregion

		[List("Lookups.ProductList")]
		[ResourceStringData("IncidentTriage|IMT_Product", Caption = "Product")]
		public override ZString IMT_Product
		{
			get => base.IMT_Product;
			set
			{
				base.IMT_Product = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMT_ProductArea();
					Validation.ValidateIMT_Module();
				}
			}
		}

		#region Product Description

		public ZString ProductDescription
		{
			get { return Lookups.ProductList.GetDescriptionFromCode(IMT_Product); }
		}

		public ZPropertyInfo ProductDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ProductDescription)); }
		}

		#endregion

		[List("Lookups.ProductAreaList")]
		[ResourceStringData("IncidentTriage|IMT_ProductArea", Caption = "Product Area")]
		public override ZString IMT_ProductArea
		{
			get => base.IMT_ProductArea;
			set
			{
				base.IMT_ProductArea = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMT_Product();
					Validation.ValidateIMT_Module();
				}
			}
		}

		public bool IMT_ProductArea_ReadOnly => IMT_SetProductAreaByMenuItem;

		public ZString ProductAreaDescription
		{
			get { return !IMT_ProductArea.IsEmpty ? Lookups.ProductAreaList.GetDescriptionFromCode(IMT_ProductArea) : string.Empty; }
		}

		[List("Lookups.ModuleListAllModules")]
		[ResourceStringData("IncidentTriage|IMT_Module", Caption = "Sec./Svc./Req.")]
		public override ZString IMT_Module
		{
			get => base.IMT_Module;
			set
			{
				base.IMT_Module = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMT_ProductArea();
					Validation.ValidateIMT_Product();
				}
			}
		}

		#region Module Description

		public ZString ModuleDescription
		{
			get { return Lookups.ModuleListAllModules.GetDescriptionFromCode(IMT_Module); }
		}

		public ZPropertyInfo ModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ModuleDescription)); }
		}

		#endregion

		[ResourceStringData("IncidentTriage|IMT_IsInternal", Caption = "Internal")]
		public override ZBool IMT_IsInternal { get => base.IMT_IsInternal; set => base.IMT_IsInternal = value; }

		public override ZBool IMT_SetProductAreaByMenuItem
		{
			get => base.IMT_SetProductAreaByMenuItem;
			set
			{
				base.IMT_SetProductAreaByMenuItem = value;
				if (IMT_SetProductAreaByMenuItem)
				{
					IMT_ProductArea = "";
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMT_ProductArea();
					Validation.ValidateIMT_Product();
					Validation.ValidateIMT_Module();
				}
			}
		}

		#endregion

		public IncidentTriageChecklistItemPivotCollection ChecklistPivots
		{
			get
			{
				if (checklistPivots == null)
				{
					checklistPivots = new IncidentTriageChecklistItemPivotCollection(this, Factory);
					checklistPivots.Load();
				}

				return checklistPivots;
			}
		}

		IncidentTriageChecklistItemPivotCollection checklistPivots;

		[ChildEditable]
		public IncidentTriageDiagnosticCriteriaPivotCollection DiagnosticCriteriaPivots
		{
			get
			{
				if (diagnosticCriteriaPivots == null)
				{
					diagnosticCriteriaPivots = new IncidentTriageDiagnosticCriteriaPivotCollection(this, Factory);
					RegisterEditableChildObject(diagnosticCriteriaPivots);
					diagnosticCriteriaPivots.Load();
				}

				return diagnosticCriteriaPivots;
			}
		}

		#region IWorkflowProvider
		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}
				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetIncidentTriageProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetIncidentTriageProcessTaskCollection()
		{
			return new ProcessTaskCollection<IncidentTriageProcessTask, IncidentTriage>(this);
		}

		public ZString WorkflowType => IncidentTriageConstants.WorkflowDescriptorInformation.Code;

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, IMT_Product, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, IMT_ProductArea, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, IMT_Module, ZString.Empty);
			return result;
		}

		#endregion

		IncidentTriageDiagnosticCriteriaPivotCollection diagnosticCriteriaPivots;

		protected override sealed EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new IncidentTriageFetchStrategy(this);
		}

		protected void SetIncidentNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(IMT_TriageNumberInfo, Fountains.IncidentTriageNumber);
			}
		}

		readonly ClientNumberFountainRegistration Fountains = ClientNumberFountainRegistration.GetInstance();

		public override void OnSaving()
		{
			SetIncidentNumberIfRequired();
			base.OnSaving();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			IMT_Level = IncidentTriageLevels.Codes.Diagnostics;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}
	}
}
