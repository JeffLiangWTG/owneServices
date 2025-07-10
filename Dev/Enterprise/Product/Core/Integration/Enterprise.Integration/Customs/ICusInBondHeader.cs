using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondHeader : IBusiness, ICancellable
		{
			ZGuid PK { get; }
			ZString BH_ApplicationCode { get; set; }
			ZGuid BH_ParentID { get; set; }
			ZString BH_ParentTableCode { get; set; }
			ZString BH_CarrierSCAC { get; set; }
			ZGuid BH_GB { get; set; }
			ZBool BH_OverrideFreightDefaults { get; set; }
			ZString BH_JobReference { get; set; }
			ZString BH_ImportConveyanceName { get; set; }
			ZString BH_VoyageNumber { get; set; }
			ZString BH_PortUnladingDCode { get; set; }
			ZDateTime BH_ETA { get; set; }
			ZDateTime BH_SystemCreateTimeUtc { get; set; }
			ZGuid BH_OA_Importer { get; set; }
			ZGuid BH_OH_Supplier { get; set; }
			ZBool BH_FTZMove { get; set; }
			ZPropertyInfo BH_FTZMoveInfo { get; }
			Type MovementHeaderType { get; }
			ZString BH_MessageStatus { get; set; }
			ZPropertyInfo BH_MessageStatusInfo { get; }
		}
	}
}
