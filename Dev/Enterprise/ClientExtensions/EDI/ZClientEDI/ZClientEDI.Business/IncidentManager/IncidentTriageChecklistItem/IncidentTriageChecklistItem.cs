using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.IncidentManager.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeProperty(IncidentTriageChecklistItemSchema.Constants.IMC_SupportDescription), DescriptionProperty(IncidentTriageChecklistItemSchema.Constants.IMC_ResponseType)]
	public class IncidentTriageChecklistItem : AutoIncidentTriageChecklistItem
	{
		public IncidentTriageChecklistItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => "Checklist Item";

		protected override ZString HumanReadableShortcutNameCore => $"Checklist Item - {IMC_SupportDescription}";

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ResourceStringData("IncidentTriageChecklistItem|IMC_SupportDescription", Caption = "Support Description")]
		public override ZString IMC_SupportDescription { get => base.IMC_SupportDescription; set => base.IMC_SupportDescription = value; }

		[List("Lookups.Categorys")]
		[ResourceStringData("IncidentTriageChecklistItem|IMC_Category", Caption = "Category")]
		public override ZString IMC_Category
		{
			get => base.IMC_Category;
			set
			{
				base.IMC_Category = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMC_Category();
				}
			}
		}

		[List("Lookups.ResponseTypes")]
		[ResourceStringData("IncidentTriageChecklistItem|IMC_ResponseType", Caption = "Response Type", ShortCaption = "Resp. Type")]
		public override ZString IMC_ResponseType
		{
			get => base.IMC_ResponseType;
			set
			{
				base.IMC_ResponseType = value;
			}
		}

		public bool IMC_ResponseType_ReadOnly => true;

		public override ZBool IMC_IsPublished
		{
			get => base.IMC_IsPublished;
			set
			{
				base.IMC_IsPublished = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePublishedDescriptionText();
				}
			}
		}

		[ResourceStringData("IncidentTriageChecklistItem|PublishedDescriptionText", Caption = "Published Description")]
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
			get { return EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription.TextOnlyMaxLength; }
		}

		UniqueNote PublishedDescription
		{
			get
			{
				if (fPublishedDescription == null)
				{
					fPublishedDescription = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription);
				}
				return fPublishedDescription;
			}
		}

		UniqueNote fPublishedDescription;

		protected override sealed EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new IncidentTriageChecklistItemFetchStrategy(this);
		}

		protected void SetIncidentNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(IMC_ChecklistNumberInfo, Fountains.IncidentTriageChecklistItemNumber);
			}
		}

		readonly ClientNumberFountainRegistration Fountains = ClientNumberFountainRegistration.GetInstance();

		public override void OnSaving()
		{
			SetIncidentNumberIfRequired();
			base.OnSaving();
		}
	}
}
