using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public partial class ProfessionalServicesQuoteLookups : IncidentMainLookups
	{
		#region Active Client List

		public OrgHeaderCollection ActiveClientList
		{
			get
			{
				if (fActiveClientList == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsActive, ZBool.True);
					fActiveClientList = new OrgHeaderCollection(Factory, filter);
				}

				return fActiveClientList;
			}
		}

		OrgHeaderCollection fActiveClientList;

		#endregion

		#region Customer Service Contacts

		public override GlbStaffCollection CustServiceContacts
		{
			get { return new GlbStaffCollection(Factory, new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True)); }
		}

		#endregion

		#region Priority List

		public CodeDescriptionPairList PriorityList
		{
			get { return IncidentConstants.GetPriorityCodeDescPairList(); }
		}

		#endregion		

		#region Work Item List

		public NewWorkItemCollection WorkItemList
		{
			get
			{
				if (fWorkItemList == null)
				{
					fWorkItemList = GetNewWorkItemList();
				}

				return fWorkItemList;
			}
		}

		protected virtual NewWorkItemCollection GetNewWorkItemList()
		{
			return new NewWorkItemCollection(Factory);
		}

		NewWorkItemCollection fWorkItemList;

		#endregion

		#region Project List

		public ProjectCollection ProjectList
		{
			get { return projectList ?? (projectList = new ProjectCollection(Factory)); }
		}
		ProjectCollection projectList;

		#endregion

		#region Work Item Type List

		public CodeDescriptionPairList WorkItemTypeList
		{
			get { return GetWorkItemTypeList(); }
		}

		public static class WorkItemType
		{
			public const string Fix = "FIX";
			public const string Enhancement = "ENH";
			public const string Refactor = "RFF";
			public const string DataFix = "DFX";
			public const string Investigation = "INV";
		}

		#endregion

		#region Implementation

		protected new ProfessionalServicesQuote Parent
		{
			get { return (ProfessionalServicesQuote)base.Parent; }
		}

		#endregion
	}
}