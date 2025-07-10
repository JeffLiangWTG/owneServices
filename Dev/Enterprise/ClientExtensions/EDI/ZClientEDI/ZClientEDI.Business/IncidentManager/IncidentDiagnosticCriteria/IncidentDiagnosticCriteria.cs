using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeProperty(IncidentDiagnosticCriteriaSchema.Constants.IMD_Question)]
	public class IncidentDiagnosticCriteria : AutoIncidentDiagnosticCriteria, IDocManagerSupport
	{
		public IncidentDiagnosticCriteria(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => SingularName;

		public static ResourceString SingularName
		{
			get { return ResString.GetMultilingualString("2dccbd80-a683-48f5-87ed-3cdcc74c7686", "Diagnostic Criterion"); }
		}

		[List("Lookups.Types")]
		[ResourceStringData("IncidentDiagnosticCriteria|IMD_Type", Caption = "Type")]
		[MaxLength(3)]
		public override ZString IMD_Type
		{
			get => base.IMD_Type;
			set
			{
				base.IMD_Type = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMD_Type();
				}
			}
		}

		public ZString TypeDescription => Lookups.Types.GetDescriptionFromCode(IMD_Type);

		[ResourceStringData("IncidentDiagnosticCriteria|IMD_Question", Caption = "Client Question")]
		public override ZString IMD_Question
		{
			get => base.IMD_Question;
			set
			{
				base.IMD_Question = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMD_Question();
				}
			}
		}

		[ResourceStringData("IncidentDiagnosticCriteria|IMD_InternalSupportNote", Caption = "Internal Support Note")]
		public override ZString IMD_InternalSupportNote
		{
			get => base.IMD_InternalSupportNote;
			set
			{
				base.IMD_InternalSupportNote = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMD_InternalSupportNote();
				}
			}
		}

		[ResourceStringData("IncidentDiagnosticCriteria|IMD_Keywords", Caption = "Keywords")]
		[MaxLength(256)]
		public override ZString IMD_Keywords
		{
			get => base.IMD_Keywords;
			set
			{
				base.IMD_Keywords = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMD_Keywords();
				}
			}
		}
		[ResourceStringData("IncidentDiagnosticCriteria|IMD_Description", Caption = "Description")]
		[MaxLength(256)]
		public override ZString IMD_Description
		{
			get => base.IMD_Description;
			set
			{
				base.IMD_Description = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateIMD_Description();
				}
			}
		}

		[ChildEditable]
		public IncidentDiagnosticCriteriaTriagePivotCollection IncidentTriagePivots
		{
			get
			{
				if (incidentTriagePivots == null)
				{
					incidentTriagePivots = new IncidentDiagnosticCriteriaTriagePivotCollection(this, Factory);
					RegisterEditableChildObject(incidentTriagePivots);
					incidentTriagePivots.Load();
				}

				return incidentTriagePivots;
			}
		}

		IncidentDiagnosticCriteriaTriagePivotCollection incidentTriagePivots;

		[ChildEditable]
		public DiagnosticCriteriaInvestigationItemPivotCollection InvestigationItemPivots
		{
			get
			{
				if (investigationItemPivots == null)
				{
					investigationItemPivots = new DiagnosticCriteriaInvestigationItemPivotCollection(this, Factory);
					RegisterEditableChildObject(investigationItemPivots);
					investigationItemPivots.Load();
				}

				return investigationItemPivots;
			}
		}

		DiagnosticCriteriaInvestigationItemPivotCollection investigationItemPivots;

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, IncidentDiagnosticCriteriaSchema.Constants.Prefix));

		DocManagerInfo docManagerInfo;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			IMD_Type = "";
		}

#endif
	}
}
