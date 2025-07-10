using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class ISF
			{
				public interface ICusISFHeader
				{
					ZGuid PK { get; }
					ZDateTime BF_SystemCreateTimeUtc { get; set; }
					ZString BF_JobReference { get; set; }
					ZGuid BF_OH_Importer { get; set; }
					ZString BF_EntryType { get; set; }
					ZString BF_BondActivityCode { get; set; }
					ZString BF_BondReferenceNumber { get; set; }
					ZString BF_BondType { get; set; }
					ZString BF_CustomsStatusDescription { get; }
					ZString BF_EntryNumber { get; set; }
					ZDateTime BF_FirstAcceptedDate { get; }
					ZDateTime BF_LastAcceptedDate { get; }
					ZString BF_SCAC { get; set; }
					ZString BF_SuretyCode { get; set; }
					ZString BondHolderNumberForDocument { get; }
					ZString ConsigneeTypeAndCodeForDocument { get; }
					ZString ImporterCodeForDocument { get; }
					ZString BF_ShipmentType { get; }
					IBusinessObjectCollection<IUSISFDocAddress> DocAddresses { get; }
					ICusISFBillCollection<ICusISFBill> ReferenceDatas { get; }
					ICusISFLineCollection<ICusISFLine> Lines { get; }
					ICusISFEquipCollection<ICusISFEquip> Equipments { get; }
					IUSISFDocAddress BookingParty { get; }
					IUSISFDocAddress BuyingParty { get; }
					IUSISFDocAddress Consolidator { get; }
					IUSISFDocAddress MainShipToParty { get; }
					IUSISFDocAddress SellingParty { get; }
					IUSISFDocAddress StuffingLocation { get; }
				}

				public interface ICusISFAutoSendingMessageSupporter : IBaseAutoSendingMessageSupporter
				{
					IProcessor CreateISFMessageWorkflowTriggerProcessor();
				}

				public interface ICusISFBillCollection<out ICusISFBill> : IActiveBusinessObjectCollection<ICusISFBill>
				{
					new ICusISFBill this[int index] { get; }
				}

				public interface ICusISFLineCollection<out ICusISFLine> : IActiveBusinessObjectCollection<ICusISFLine>
				{
					new ICusISFLine this[int index] { get; }
				}

				public interface ICusISFEquipCollection<out ICusISFEquip> : IActiveBusinessObjectCollection<ICusISFEquip>
				{
					new ICusISFEquip this[int index] { get; }
				}
			}
		}
	}
}
