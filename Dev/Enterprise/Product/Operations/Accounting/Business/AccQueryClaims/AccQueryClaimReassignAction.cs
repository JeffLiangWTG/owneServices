using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class AccQueryClaimReassignAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccQueryClaimReassignAction(AccQueryClaimBase claim)
			: base(claim.Factory)
		{
			this.fClaim = claim;
		}

		readonly AccQueryClaimBase fClaim;

		public bool Sycnhronise()
		{
			RunPreSaveValidation();
			bool result = false;
			if (!HasErrors)
			{
				fClaim.AddToLog(Comment);
				fClaim.AY_GS_NKStaffAssignedTo = Staff.GS_Code;
				fClaim.AY_GB = BranchPK;
				result = true;
			}
			fClaim.IsReassigned = result;

			return result;
		}

		public void SetDefaultsFromClaim()
		{
			BranchPK = fClaim.AY_GB;
			StaffCode = fClaim.AY_GS_NKStaffAssignedTo;
		}

		#region Properties

		#region Selected Staff

		[List("StaffList")]
		[MaxLength(GlbStaff.Schema.GS_CodeMaxLength)]
		public ZString StaffCode
		{
			get { return fStaffCode; }
			set
			{
				if (fStaffCode != value)
				{
					CheckMaximumLength(StaffCodeInfo, value);
					fStaffCode = value;
					StaffCodeInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateStaffCode();
					}
				}
			}
		}

		ZString fStaffCode;

		public ZPropertyInfo StaffCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StaffCode)); }
		}

		public GlbStaff Staff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, StaffCode); }
		}

		GlbStaffCollection fStaffList;
		public GlbStaffCollection StaffList
		{
			get { return fStaffList ?? (fStaffList = fClaim.Lookups.Staff); }
		}

		#endregion

		#region Selected Branch

		[List("BranchList")]
		public ZGuid BranchPK
		{
			get { return fBranchPK; }
			set
			{
				if (fBranchPK != value)
				{
					fBranchPK = value;
					BranchPKInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateBranchPK();
					}
				}
			}
		}

		ZGuid fBranchPK;

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(nameof(BranchPK)); }
		}

		public GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(BranchPK); }
		}

		GlbBranchCollection fBranchList;
		public GlbBranchCollection BranchList
		{
			get { return fBranchList ?? (fBranchList = fClaim.Lookups.Branches); }
		}

		#endregion

		#region Comment

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString Comment
		{
			get { return fComment; }
			set
			{
				if (fComment != value)
				{
					CheckMaximumLength(CommentInfo, value);
					fComment = value;
					CommentInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CommentInfo
		{
			get { return GetZPropertyInfo(nameof(Comment)); }
		}

		ZString fComment;

		#endregion

		#endregion

		#region Validation

		public void ValidateStaffCode()
		{
			StaffCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StaffCodeInfo);
			ListValidation.ErrorIfInvalidCode(StaffCodeInfo, StaffList);
			if (Staff != null && Staff.GS_EmailAddress.IsEmpty)
			{
				StaffCodeInfo.AddWarning(Res.GetString("039388c8-e6dd-4258-8ae2-a623ebaaa3f8", @"The selected staff member does not have an email address entered in the Staff and Resources record.
No notification email will be sent to the staff member.

Alternatively, enter the email address of the staff member under Maintain > System > Staff and Resources before reassigning the claim."));
			}
		}

		public void ValidateBranchPK()
		{
			BranchPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BranchPKInfo);
			ListValidation.ErrorIfInvalidPK(BranchPKInfo, BranchList);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateStaffCode();
			ValidateBranchPK();
		}

		#endregion
	}
}

