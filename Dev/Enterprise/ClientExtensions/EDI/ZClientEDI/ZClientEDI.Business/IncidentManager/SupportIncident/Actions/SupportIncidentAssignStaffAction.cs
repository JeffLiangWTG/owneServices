using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentAssignStaffAction : SupportIncidentAction
	{
		public SupportIncidentAssignStaffAction(SupportIncident incident)
			: base(incident)
		{
		}

		protected override void PerformAction()
		{
			if (AssignToSelf || AssignToOther)
			{
				Incident.AssignToStaff(Staff, Comment);
			}

			if (!Comment.IsEmpty)
			{
				Incident.AddInternalMessage(Comment);
			}
		}

		#region Properties

		#region AssignToSelf

		public ZBool AssignToSelf
		{
			get { return fAssignToSelf; }
			set
			{
				SetNonPersistentPropertyValue(AssignToSelfInfo, ref fAssignToSelf, value);
				if (value)
				{
					AssignToOther = false;
					StaffPK = GlbStaff.CurrentUser.PK;
				}
				StaffPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AssignToSelfInfo
		{
			get { return GetZPropertyInfo(nameof(AssignToSelf)); }
		}

		ZBool fAssignToSelf;

		#endregion

		#region AssignToOther

		public ZBool AssignToOther
		{
			get { return fAssignToOther; }
			set
			{
				SetNonPersistentPropertyValue(AssignToOtherInfo, ref fAssignToOther, value);
				if (value)
				{
					AssignToSelf = false;
				}

				StaffPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AssignToOtherInfo
		{
			get { return GetZPropertyInfo(nameof(AssignToOther)); }
		}

		ZBool fAssignToOther;

		#endregion

		#region Selected Staff

		public ZGuid StaffPK
		{
			get { return fStaffPK; }
			set
			{
				if (fStaffPK != value)
				{
					fStaffPK = value;
					StaffPKInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateStaffPK();
					}
				}
			}
		}

		ZGuid fStaffPK;

		public ZPropertyInfo StaffPKInfo
		{
			get { return GetZPropertyInfo(nameof(StaffPK)); }
		}

		public bool StaffPK_ReadOnly
		{
			get { return !AssignToOther; }
		}

		public GlbStaff Staff
		{
			get { return Factory.Load<GlbStaff>(StaffPK); }
		}

		public GlbStaffCollection AllStaffList
		{
			get
			{
				if (fAllStaffList == null)
				{
					fAllStaffList = new GlbStaffCollection(Factory, ActiveStaffFilter);
				}

				return fAllStaffList;
			}
		}

		GlbStaffCollection fAllStaffList;

		public GlbStaffCollection SupportStaffList
		{
			get
			{
				if (fSupportStaffList == null)
				{
					GlbGroup supportGroup = Factory.Load<GlbGroup>(EDIDataRegistry.Instance.IncidentSupportGroup.Value);
					if (supportGroup != null)
					{
						fSupportStaffList = new GlbStaffForGroupCollection(Factory, supportGroup.PK);
						fSupportStaffList.AdditionalFilter = ActiveStaffFilter;
					}
					else
					{
						fSupportStaffList = AllStaffList;
					}
				}

				return fSupportStaffList;
			}
		}

		GlbStaffCollection fSupportStaffList;

		ZQuery ActiveStaffFilter
		{
			get { return activeStaffFilter ?? (activeStaffFilter = new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True)); }
		}
		ZQuery activeStaffFilter;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateStaffPK();
		}

		void ValidateStaffPK()
		{
			StaffPKInfo.ClearAllNotifications();
			if (AssignToOther)
			{
				MandatoryValidation.CheckEntered(StaffPKInfo);
			}
			ListValidation.ErrorIfInvalidPK(StaffPKInfo, AllStaffList);
		}

		#endregion
	}
}

