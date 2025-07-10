using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(IncidentMainSchema.Constants.IM_IncidentNumber)]
	public abstract class IncidentMainBase : AutoIncidentMain, IJobHeaderParent, IWorkItemSource, Integration.ZClientEDI.IIncidentMain, IDocManagerSupport
	{
		public new class Schema : AutoIncidentMain.Schema
		{
			public const string IM_ChargableWorkYesSelection = "IM_ChargableWorkYesSelection";
			public const string IM_ChargableWorkNoSelection = "IM_ChargableWorkNoSelection";
		}

		protected IncidentMainBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(IncidentMainBase incident)
				: base(incident)
			{
				this.incident = incident;
			}
			readonly IncidentMainBase incident;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var column in columns)
				{
					if (column.ColumnName.StartsWith("Client", StringComparison.Ordinal))
					{
						Factory.AddFetchHint(OrgHeaderSchema.PK, incident.IM_OH_Client);
					}
				}
			}
		}

		#region Properties

		#region IM_ClientHasInvoicingPreferencesNote

		public ZBool IM_ClientHasInvoicingPreferencesNote
		{
			get
			{
				if (BranchAddress != null
					&& BranchAddress.Header != null)
				{
					return BranchAddress.Header.Notes.FindByDescription(PredefinedNoteTypes.Instance.InvoicingPreferences.Description).Length > 0;
				}
				return ZBool.False;
			}
		}

		#endregion

		#region IM_InvoicingLocalClientHasInvoicingPreferencesNote

		public ZBool IM_InvoicingLocalClientHasInvoicingPreferencesNote
		{
			get
			{
				var job = new Job.Loader(this).Load();
				if (job != null && job.LocalCharges != null)
				{
					return job.LocalCharges.Notes.FindByDescription(PredefinedNoteTypes.Instance.InvoicingPreferences.Description).Length > 0;
				}

				return ZBool.False;
			}
		}

		#endregion

		public ZString ClientCode
		{
			get
			{
				OrgHeader client = Client;
				return (client != null) ? client.OH_Code : ZString.Empty;
			}
		}

		public ZPropertyInfo ClientCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ClientCode)); }
		}

		public ZString ClientName
		{
			get
			{
				OrgHeader client = Client;
				return (client != null) ? client.OH_FullName : ZString.Empty;
			}
		}

		public ZPropertyInfo ClientNameInfo
		{
			get { return GetZPropertyInfo(nameof(ClientName)); }
		}

		public ZString IM_StatusWithDescription
		{
			get { return IM_Status + " - " + Statuses.GetDescriptionFromCode(IM_Status); }
		}

		public ZPropertyInfo IM_StatusWithDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(IM_StatusWithDescription)); }
		}

		protected abstract ReadOnlyCodeDescriptionPairList Statuses { get; }

		public GlbStaff AddedBy
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, IM_SystemCreateUser); }
		}

		public ZBlob IM_Details_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(IM_Details);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.IM_Details = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZDateTime IM_CloseTime
		{
			get
			{
				if (IM_CloseTimeUtc.IsValid)
				{
					return IM_CloseTimeUtc.UtcToDateTimeOffset().ToLocalZDateTime();
				}

				return ZDateTime.Empty;
			}
		}

		public ZPropertyInfo IM_CloseTimeInfo
		{
			get { return GetZPropertyInfo(nameof(IM_CloseTime)); }
		}

		#region Contact

		public override OrgContact Contact
		{
			get { return (EDIOrgContact)base.Contact; }
		}

		#endregion

		#region IM_Calc_BranchPhone

		public ZString IM_Calc_BranchPhone
		{
			get
			{
				ZString result = "";
				if (BranchAddress != null)
				{
					result = BranchAddress.OA_Phone;
				}
				return result;
			}
		}

		public ZPropertyInfo IM_Calc_BranchPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(IM_Calc_BranchPhone)); }
		}

		#endregion

		#region IM_Calc_BranchFax

		public ZString IM_Calc_BranchFax
		{
			get
			{
				ZString result = "";
				if (BranchAddress != null)
				{
					result = BranchAddress.OA_Fax;
				}
				return result;
			}
		}

		public ZPropertyInfo IM_Calc_BranchFaxInfo
		{
			get { return GetZPropertyInfo(nameof(IM_Calc_BranchFax)); }
		}

		#endregion

		#region Chargable Work

		#region IM_ChargableWorkYesSelection

		public ZBool IM_ChargableWorkYesSelection
		{
			get { return IM_ChargableWork; }
			set { IM_ChargableWork = value; }
		}

		public ZPropertyInfo IM_ChargableWorkYesSelectionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IM_ChargableWorkYesSelection, x => IM_ChargableWorkInfo); }
		}

		#endregion

		#region IM_ChargableWorkNoSelection

		public ZBool IM_ChargableWorkNoSelection
		{
			get { return !IM_ChargableWork; }
			set { IM_ChargableWork = !value; }
		}

		public ZPropertyInfo IM_ChargableWorkNoSelectionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.IM_ChargableWorkNoSelection, x => IM_ChargableWorkInfo); }
		}

		#endregion

		#region IM_ChargableWorkAsText

		public ZString IM_ChargableWorkAsText
		{
			get { return (IM_ChargableWork) ? "Yes" : "No"; }
		}

		public ZPropertyInfo IM_ChargableWorkAsTextInfo
		{
			get { return GetZPropertyInfo(nameof(IM_ChargableWorkAsText)); }
		}
		#endregion

		#endregion Chargable Work

		#endregion Data properties

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new IncidentMainTypeDecider();

		class IncidentMainTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(IncidentMainBase);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				string incidentType = row[IncidentMainSchema.Constants.IM_IncidentType].ToString();
				switch (incidentType)
				{
					case IncidentConstants.IncidentType.ProfessionalServicesQuote:
						return typeof(ProfessionalServicesQuote);
					case IncidentConstants.IncidentType.SupportIncident:
						return typeof(SupportIncident);
					default:
						return typeof(IncidentMainBase);
				}
			}

			public override Type GetTypeForNew()
			{
				return typeof(SupportIncident);
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IJobHeaderParent Members

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			SetJobNumberFieldOnSaving();
		}

		protected virtual void SetJobNumberFieldOnSaving()
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return IM_IncidentNumber; }
		}

		#endregion

		#region IWorkItemSource Members

		public virtual void PopulateWorkItem(NewWorkItem workItem)
		{
		}

		#endregion IWorkItemSource Members

		#region IWorkTaskRelatedItem Members

		public ZString SelectionCriterion1 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.ListHelper.ProductList, IM_Product);
		public ZPropertyInfo SelectionCriterion1Info => GetZPropertyInfo(nameof(SelectionCriterion1));

		public ZString SelectionCriterion2 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.ListHelper.ProgramAreaList, IM_ProgramArea);
		public ZPropertyInfo SelectionCriterion2Info => GetZPropertyInfo(nameof(SelectionCriterion2));

		public ZString SelectionCriterion3 => GetSelectionCriterion3Core();
		protected abstract string GetSelectionCriterion3Core();
		public ZPropertyInfo SelectionCriterion3Info => GetZPropertyInfo(nameof(SelectionCriterion3));

		public ZString SelectionCriterion4 => string.Empty;
		public ZPropertyInfo SelectionCriterion4Info => GetZPropertyInfo(nameof(SelectionCriterion4));

		public ZString SelectionCriterion5 => string.Empty;
		public ZPropertyInfo SelectionCriterion5Info => GetZPropertyInfo(nameof(SelectionCriterion5));

		public Type PivotCollectionType => typeof(GenPivotCollection);

		#endregion

		#region IDocManagerSupport Members

		public abstract DocManagerInfo DocManagerInfo { get; }

		#endregion
	}
}
