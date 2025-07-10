using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class ClassifierAllocation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ClassifierAllocation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region AllocatedTo

		[MaxLength(GlbStaff.Schema.GS_CodeMaxLength)]
		public ZString AllocatedTo
		{
			get { return fAllocatedTo; }
			set
			{
				if (fAllocatedTo != value)
				{
					CheckMaximumLength(AllocatedToInfo, value);
					SetNonPersistentPropertyValue(AllocatedToInfo, ref		fAllocatedTo, value);
					ResetStaff();
				}
			}
		}
		ZString fAllocatedTo;

		public ZPropertyInfo AllocatedToInfo
		{
			get { return GetZPropertyInfo(nameof(AllocatedTo)); }
		}

		#endregion

		#region AllocatedToFullName

		[MaxLength(GlbStaff.Schema.GS_FullNameMaxLength)]
		public ZString AllocatedToFullName
		{
			get
			{
				ZString result = "";
				if (Staff != null)
				{
					result = Staff.GS_FullName;
				}
				return result;
			}
		}

		public ZPropertyInfo AllocatedToFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(AllocatedToFullName)); }
		}

		#endregion

		#region NumberAllocated

		public ZInt NumberAllocated
		{
			get { return fNumberAllocated; }
			set
			{
				if (fNumberAllocated != value)
				{
					SetNonPersistentPropertyValue(NumberAllocatedInfo, ref	fNumberAllocated, value);
				}
			}
		}
		ZInt fNumberAllocated;

		public ZPropertyInfo NumberAllocatedInfo
		{
			get { return GetZPropertyInfo(nameof(NumberAllocated)); }
		}

		#endregion

		#region NextWorkingDay

		public ZDateTime NextWorkingDay
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (Staff != null)
				{
					if (!Staff.IsWorkingToday)
					{
						result = (ZDateTime)Staff.NextWorkingDay(false);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo NextWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(NextWorkingDay)); }
		}

		#endregion

		#region IncludeForAllocation

		public ZBool IncludeForAllocation
		{
			get { return fIncludeForAllocation; }
			set
			{
				SetNonPersistentPropertyValue(IncludeForAllocationInfo, ref fIncludeForAllocation, value);
			}
		}
		ZBool fIncludeForAllocation;

		public ZPropertyInfo IncludeForAllocationInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeForAllocation)); }
		}

		#endregion

		#region Staff

		GlbStaff Staff
		{
			get
			{
				if (fStaff == null && !AllocatedTo.IsEmpty)
				{
					fStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, AllocatedTo);
				}

				return fStaff;
			}
		}
		GlbStaff fStaff;

		void ResetStaff()
		{
			fStaff = null;
		}

		#endregion
	}
}
