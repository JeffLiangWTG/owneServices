using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IBaseCusSCAOceanBill : ICancellable
			{
				ZGuid PK { get; }

				ZString CB_ApplicationCode { get; set; }
				ZPropertyInfo CB_ApplicationCodeInfo { get; }

				ZDateTime CB_DateOfArrival { get; set; }
				ZPropertyInfo CB_DateOfArrivalInfo { get; }

				ZDateTime CB_DateOfFirstArrival { get; set; }
				ZPropertyInfo CB_DateOfFirstArrivalInfo { get; }

				ZGuid CB_GB { get; set; }
				ZPropertyInfo CB_GBInfo { get; }

				ZBool CB_IsBureau { get; set; }
				ZPropertyInfo CB_IsBureauInfo { get; }

				ZGuid CB_ParentId { get; set; }
				ZPropertyInfo CB_ParentIdInfo { get; }

				ZString CB_ParentTableCode { get; set; }
				ZPropertyInfo CB_ParentTableCodeInfo { get; }

				ZString CB_LloydsIMO { get; set; }
				ZPropertyInfo CB_LloydsIMOInfo { get; }

				ZString CB_MasterHouseBill { get; set; }
				ZPropertyInfo CB_MasterHouseBillInfo { get; }

				ZBool CB_MultiOBLUnpack { get; set; }
				ZPropertyInfo CB_MultiOBLUnpackInfo { get; }

				ZString CB_OceanBill { get; set; }
				ZPropertyInfo CB_OceanBillInfo { get; }

				ZGuid CB_OH_ShippingLine { get; set; }
				ZPropertyInfo CB_OH_ShippingLineInfo { get; }

				ZString CB_PrincipalID { get; set; }
				ZPropertyInfo CB_PrincipalIDInfo { get; }

				ZString CB_ResponsiblePartyID { get; set; }
				ZPropertyInfo CB_ResponsiblePartyIDInfo { get; }

				ZString CB_RL_NKPortOfDischarge { get; set; }
				ZPropertyInfo CB_RL_NKPortOfDischargeInfo { get; }

				ZString CB_RL_NKPortOfFirstArrival { get; set; }
				ZPropertyInfo CB_RL_NKPortOfFirstArrivalInfo { get; }

				ZString CB_RL_NKPortOfLoading { get; set; }
				ZPropertyInfo CB_RL_NKPortOfLoadingInfo { get; }

				ZString CB_VesselName { get; set; }
				ZPropertyInfo CB_VesselNameInfo { get; }

				ZString CB_Voyage { get; set; }
				ZPropertyInfo CB_VoyageInfo { get; }

				ZString CB_MessageReference { get; set; }
				ZPropertyInfo CB_MessageReferenceInfo { get; }
			}
		}
	}
}
