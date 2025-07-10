using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgStaffAssignment : OrgStaffAssignments
	{
		public UPEOrgStaffAssignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void Delete()
		{
			if (O8_Role == UPEStaffRoles.Codes.RV)
			{
				ADPScoring.T4_RegularVolumeCount--;
			}
			base.Delete();
		}

		#endregion

		#region Property Overrides

		public override ZString O8_Role
		{
			get { return base.O8_Role; }
			set
			{
				if (base.O8_Role != value)
				{
					if (O8_Role == UPEStaffRoles.Codes.RV)
					{
						ADPScoring.T4_RegularVolumeCount--;
					}
					base.O8_Role = value;
					if (O8_Role == UPEStaffRoles.Codes.RV)
					{
						ADPScoring.T4_RegularVolumeCount++;
					}
				}
			}
		}

		#endregion

		#region Implementation

		UPEADPScoring ADPScoring
		{
			get
			{
				UPEADPScoring.Loader loader = new UPEADPScoring.Loader(Factory);
				return loader.LoadOrCreate();
			}
		}

		#endregion
	}
}
