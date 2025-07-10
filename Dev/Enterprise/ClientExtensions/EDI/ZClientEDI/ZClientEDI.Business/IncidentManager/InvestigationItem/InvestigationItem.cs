using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class InvestigationItem : AutoInvestigationItem
	{
		public InvestigationItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => SingularName;

		public static ResourceString SingularName
		{
			get { return ResString.GetMultilingualString("e9ebe2ec-37c8-4044-9547-5a5934fb57c2", "Investigation Item"); }
		}

		[List("Lookups.Types")]
		[ResourceStringData("InvestigationItem|INV_Type", Caption = "Type")]
		[MaxLength(3)]
		public override ZString INV_Type
		{
			get => base.INV_Type;
			set
			{
				base.INV_Type = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateINV_Type();
				}
			}
		}

		[ResourceStringData("InvestigationItem|INV_Description", Caption = "Description")]
		public override ZString INV_Description
		{
			get => base.INV_Description;
			set
			{
				base.INV_Description = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateINV_Description();
				}
			}
		}

		[ResourceStringData("InvestigationItem|INV_ItemText", Caption = "Item Text")]
		public override ZString INV_ItemText
		{
			get => base.INV_ItemText;

			set
			{
				base.INV_ItemText = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateINV_ItemText();
				}
			}
		}

		[ChildEditable]
		public InvestigationItemDiagnosticCriteriaPivotCollection DiagnosticCriteriaPivots
		{
			get
			{
				if (diagnosticCriteriaPivots == null)
				{
					diagnosticCriteriaPivots = new InvestigationItemDiagnosticCriteriaPivotCollection(this, Factory);
					RegisterEditableChildObject(diagnosticCriteriaPivots);
					diagnosticCriteriaPivots.Load();
				}

				return diagnosticCriteriaPivots;
			}
		}

		InvestigationItemDiagnosticCriteriaPivotCollection diagnosticCriteriaPivots;

		[ChildEditable]
		public InvestigationItemResponseOptionCollection ResponseOptions
		{
			get
			{
				if (responseOptions == null)
				{
					responseOptions = new InvestigationItemResponseOptionCollection(this, Factory);
					RegisterEditableChildObject(responseOptions);
					responseOptions.Load();
				}

				return responseOptions;
			}

			private set
			{
				responseOptions = value;
			}
		}

		InvestigationItemResponseOptionCollection responseOptions;

		protected void SetIncidentNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(INV_ItemNumberInfo, Fountains.InvestigationItemNumber);
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
