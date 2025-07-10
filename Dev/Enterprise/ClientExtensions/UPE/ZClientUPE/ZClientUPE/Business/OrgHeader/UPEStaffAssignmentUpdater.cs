using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEStaffAssignmentUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UPEStaffAssignmentUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void UpdateClassiferRole()
		{
			if (!StaffPKToReplace.IsEmpty && !NewStaffPK.IsEmpty)
			{
				int totalStaffAssignmentsToUpdated = Factory.GetDatabaseCount(typeof(UPEOrgStaffAssignment), StaffAssignmentsQuery);
				fPercentageComplete = 0;
				fNumberOfRecordsUpdated = 0;

				do
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					Factory.RefreshEnabled = false;
					StaffAssignmentsQuery.MaximumRows = 100;
					UPEOrgStaffAssignment[] staffAssignments = newFactory.Load<UPEOrgStaffAssignment>(StaffAssignmentsQuery);

					if (staffAssignments.Length > 0)
					{
						foreach (UPEOrgStaffAssignment staff in staffAssignments)
						{
							staff.O8_GS_NKPersonResponsible = Factory.Load<GlbStaff>(NewStaffPK).GS_Code;
							fPercentageComplete = Convert.ToInt32(++fNumberOfRecordsUpdated / (float)totalStaffAssignmentsToUpdated * 100);
							if (ProcessingProgressed != null)
							{
								ProcessingProgressed(this, EventArgs.Empty);
							}
						}
						newFactory.Save();
					}
				}
				while (fNumberOfRecordsUpdated < totalStaffAssignmentsToUpdated);
			}
		}

		public event EventHandler ProcessingProgressed;

		#region Properties

		#region StaffPKToReplace

		public ZGuid StaffPKToReplace
		{
			get { return fStaffPKToReplace; }
			set
			{
				SetNonPersistentPropertyValue(StaffPKToReplaceInfo, ref fStaffPKToReplace, value);
			}
		}
		ZGuid fStaffPKToReplace;

		public ZPropertyInfo StaffPKToReplaceInfo
		{
			get { return GetZPropertyInfo(nameof(StaffPKToReplace)); }
		}

		#endregion

		#region NewStaffPK

		public ZGuid NewStaffPK
		{
			get { return fNewStaffPK; }
			set
			{
				SetNonPersistentPropertyValue(NewStaffPKInfo, ref fNewStaffPK, value);
			}
		}
		ZGuid fNewStaffPK;

		public ZPropertyInfo NewStaffPKInfo
		{
			get { return GetZPropertyInfo(nameof(NewStaffPK)); }
		}

		#endregion

		#region PercentageComplete

		public int PercentageComplete
		{
			get { return fPercentageComplete; }
		}
		int fPercentageComplete;

		#endregion

		#region NumberOfRecordsUpdated

		public int NumberOfRecordsUpdated
		{
			get { return fNumberOfRecordsUpdated; }
		}
		int fNumberOfRecordsUpdated;

		#endregion

		#endregion

		#region Validation

		public void ValidateStaffPKToReplace()
		{
			StaffPKToReplaceInfo.ClearAllNotifications();
			if (!StaffPKToReplace.IsValid)
			{
				StaffPKToReplaceInfo.AddError("Specify a valid staff member to replace as classifier");
			}
		}

		public void ValidateNewStaffPK()
		{
			NewStaffPKInfo.ClearAllNotifications();
			if (!NewStaffPK.IsValid)
			{
				NewStaffPKInfo.AddError("Specify a valid staff member for the classifier role");
			}
		}

		#endregion

		#region Lookups

		public GlbStaffCollection Staff
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region Implementation

		ZQuery StaffAssignmentsQuery
		{
			get
			{
				if (fStaffAssignmentsQuery == null)
				{
					fStaffAssignmentsQuery = new ZDBOnlyQuery(typeof(UPEOrgStaffAssignment));
					ZDBOnlySubQuery staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
					staffSubQuery.AddToFilter(GlbStaffSchema.PK, StaffPKToReplace);
					fStaffAssignmentsQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible,
																						 staffSubQuery,
																						 JoinCondition.And);
					fStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, UPEStaffRoles.Codes.Classifier);
					fStaffAssignmentsQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
				}
				return fStaffAssignmentsQuery;
			}
		}
		ZDBOnlyQuery fStaffAssignmentsQuery;

		#endregion
	}
}
