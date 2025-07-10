using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocDepartment : DocBaseWrapper
	{
		DocDepartment(GlbDepartment glbDepartment, BusinessObjectFactory factoryForWrapper)
			: base(glbDepartment, factoryForWrapper)
		{
		}

		public static DocDepartment New(GlbDepartment glbDepartment, BusinessObjectFactory factoryForWrapper)
		{
			if (glbDepartment == null)
			{
				return null;
			}
			else
			{
				return new DocDepartment(glbDepartment, factoryForWrapper);
			}
		}

		GlbDepartment GlbDepartment
		{
			get { return (GlbDepartment)WrappedObject; }
		}

		public ZGuid DepartmentPK
		{
			get { return GlbDepartment != null && GlbDepartment.PK.IsValid ? GlbDepartment.PK : Guid.Empty; }
		}

		public override string ToString()
		{
			return Desc;
		}

		DocChargeCodeCollection fDepartmentChargeCodes;
		public DocChargeCodeCollection DepartmentChargeCodes
		{
			get
			{
				if (fDepartmentChargeCodes == null)
				{
					fDepartmentChargeCodes = new DocChargeCodeCollection(Factory);
					foreach (GlbDeptCharges glbCharge in GlbDepartment.DeptCharges)
					{
						if (glbCharge.ChargeCode != null)
						{
							fDepartmentChargeCodes.Add(DocChargeCode.New(glbCharge.ChargeCode, Factory));
						}
					}
				}
				return fDepartmentChargeCodes;
			}
		}

		public ZString Mode
		{
			get { return GlbDepartment.GE_Mode; }
		}

		public ZString Activity
		{
			get { return GlbDepartment.GE_Activity; }
		}

		public ZString Direction
		{
			get { return GlbDepartment.GE_Direction; }
		}

		public ZString SystemDept
		{
			get { return GlbDepartment.SystemDept; }
		}

		public ZBool Air
		{
			get { return GlbDepartment.GE_Air; }
		}

		public ZString Code
		{
			get { return GlbDepartment.GE_Code; }
		}

		public ZBool CustomsBrokerage
		{
			get { return GlbDepartment.GE_CustomsBrokerage; }
		}

		public ZBool DepotCFS
		{
			get { return GlbDepartment.GE_DepotCFS; }
		}

		public ZString Desc
		{
			get { return GlbDepartment.GE_DescMultilingual; }
		}

		public ZBool Domestic
		{
			get { return GlbDepartment.GE_Domestic; }
		}

		public ZBool Export
		{
			get { return GlbDepartment.GE_Export; }
		}

		public ZString FridayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.FridayWorkingHours; }
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(GlbDepartment.Department, Factory); }
		}

		public ZBool Import
		{
			get { return GlbDepartment.GE_Import; }
		}

		public ZBool InternationalFreight
		{
			get { return GlbDepartment.GE_InternationalFreight; }
		}

		public ZBool IsActive
		{
			get { return GlbDepartment.GE_IsActive; }
		}

		public ZBool LineHaul
		{
			get { return GlbDepartment.GE_LineHaul; }
		}

		public ZBool LocalTransport
		{
			get { return GlbDepartment.GE_LocalTransport; }
		}

		public ZBool Misc
		{
			get { return GlbDepartment.GE_Misc; }
		}

		public ZString MondayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.MondayWorkingHours; }
		}

		public ZBool NonDirectional
		{
			get { return GlbDepartment.GE_NonDirectional; }
		}

		public ZBool NonTransport
		{
			get { return GlbDepartment.GE_NonTransport; }
		}

		public ZBool Post
		{
			get { return GlbDepartment.GE_Post; }
		}

		public ZBool Rail
		{
			get { return GlbDepartment.GE_Rail; }
		}

		public ZBool Road
		{
			get { return GlbDepartment.GE_Road; }
		}

		public ZString SaturdayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.SaturdayWorkingHours; }
		}

		public ZBool Sea
		{
			get { return GlbDepartment.GE_Sea; }
		}

		public ZString SundayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.SundayWorkingHours; }
		}

		public ZBool SystemCode
		{
			get { return GlbDepartment.GE_SystemCode; }
		}

		public ZString ThursdayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.ThursdayWorkingHours; }
		}

		public ZString TuesdayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.TuesdayWorkingHours; }
		}

		public ZBool Warehouse
		{
			get { return GlbDepartment.GE_Warehouse; }
		}

		public ZString WednesdayWorkingHours
		{
			get { return GlbDepartment.WorkTimes.WednesdayWorkingHours; }
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}
	}
}
