using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class EquipmentWrapper : GenericWrapper
	{
		public EquipmentWrapper(RefEquipment equipment, BusinessObjectFactory factory)
			: base(equipment, factory)
		{
			Argument.NotNull(equipment, "equipment");
		}

		RefEquipment RefEquipment
		{
			get { return (RefEquipment)WrappedObject; }
		}

		public ZString TwoWayInfo
		{
			get { return RefEquipment.RQ_2WayInfo; }
		}

		public ZString CubicUnit
		{
			get { return RefEquipment.RQ_CubicUnit; }
		}

		public ZString Description
		{
			get { return RefEquipment.RQ_DescriptionMultilingual; }
		}

		public ZString EquipmentType
		{
			get { return RefEquipment.RoadContainerType != null ? RefEquipment.RoadContainerType.RC_Code : ZString.Empty; }
		}

		public ZString GateTransponder1
		{
			get { return RefEquipment.RQ_GateTransponder1; }
		}

		public ZString GateTransponder2
		{
			get { return RefEquipment.RQ_GateTransponder2; }
		}

		public ZString GateTransponder3
		{
			get { return RefEquipment.RQ_GateTransponder3; }
		}

		public StaffWrapper Staff
		{
			get
			{
				GlbStaff staff;
				return (staff = RefEquipment.PreferredDriver) != null ? new StaffWrapper(staff, Factory) : null;
			}
		}

		public ZBool IsActive
		{
			get { return RefEquipment.RQ_IsActive; }
		}

		public ZBool IsVehicle
		{
			get { return RefEquipment.RQ_IsVehicle; }
		}

		public ZString Registration
		{
			get { return RefEquipment.RQ_Registration; }
		}

		public ZString ShortCode
		{
			get { return RefEquipment.RQ_ShortCode; }
		}

		public ZString TollPass
		{
			get { return RefEquipment.RQ_TollPass; }
		}

		public ZDecimal TotalCubic
		{
			get { return RefEquipment.RQ_CubicCapacity; }
		}

		public ZDecimal TotalWeight
		{
			get { return RefEquipment.RQ_WeightCapacity; }
		}

		public ZString WeightUnit
		{
			get { return RefEquipment.RQ_WeightUnit; }
		}
	}
}
