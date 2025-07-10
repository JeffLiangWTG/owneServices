using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("ScheduledStartUtc: {BNC_ScheduledStartUtc}, ScheduledFinishUtc: {BNC_ScheduledFinishUtc}, DurationMinutes: {BNC_DurationMinutes}, IsScaled: {BNC_IsScaled}, ScaleUnit: {BNC_ScaleUnit}, ScaleMagnitude: {BNC_ScaleMagnitude}, ResolutionIncrement: {BNC_ResolutionIncrement}")] // Debugging info
	public class BMNCNSchedule : AutoBMNCNSchedule
	{
		public BMNCNSchedule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BNC_GB_Branch = GlbBranch.CurrentBranch.PK;
			BNC_GE_Department = GlbDepartment.CurrentDepartment.PK;

			BNC_ScaleMagnitude = 60 * BMConstants.WorkingHoursPerDay;
			BNC_ResolutionIncrement = 60 * BMConstants.WorkingHoursPerDay;
			BNC_ScaleUnit = ScaleUnitList.Codes.Hour;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override bool IsSavedByFactory
		{
			get
			{
				var shape = !IsDeleted ? Shape : null;
				return base.IsSavedByFactory
					&& (IsInDatabase || shape == null || shape.IsDeleted || shape.IsSavedByFactory);
			}
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("Shape")]
		public override ZGuid BNC_BNS_Shape
		{
			get { return base.BNC_BNS_Shape; }
			set { base.BNC_BNS_Shape = value; }
		}

		[ReadOnlyMember(nameof(BranchAndDepartment_ReadOnly))]
		public override ZGuid BNC_GB_Branch
		{
			get { return base.BNC_GB_Branch; }
			set { base.BNC_GB_Branch = value; }
		}

		[ReadOnlyMember(nameof(BranchAndDepartment_ReadOnly))]
		public override ZGuid BNC_GE_Department
		{
			get { return base.BNC_GE_Department; }
			set { base.BNC_GE_Department = value; }
		}
		bool BranchAndDepartment_ReadOnly
		{
			get { return !BNC_IsScaled || (Shape.IsApproved && !BNC_GB_BranchInfo.OriginalValue.IsEmpty && !BNC_GE_DepartmentInfo.OriginalValue.IsEmpty); }
		}

		public override ZBool BNC_IsScaled
		{
			get { return base.BNC_IsScaled; }
			set
			{
				if (base.BNC_IsScaled != value)
				{
					if (value)
					{
						SetBranchAndDepartmentToCurrentUser();
					}
					base.BNC_IsScaled = value;
				}
			}
		}

		void SetBranchAndDepartmentToCurrentUser()
		{
			BNC_GB_Branch = GlbBranch.CurrentBranch.PK;
			BNC_GE_Department = GlbDepartment.CurrentDepartment.PK;
		}

		public GlbBranch GetBranch(BusinessObjectFactory factory)
		{
			return factory.Load<GlbBranch>(BNC_GB_Branch);
		}

		public GlbDepartment GetDepartment(BusinessObjectFactory factory)
		{
			return factory.Load<GlbDepartment>(BNC_GE_Department);
		}

		#endregion

		#region Related Business Objects

		public BMNCNShape Shape
		{
			get { return Factory.Load<BMNCNShape>(BNC_BNS_Shape); }
		}

		#endregion
	}
}
