using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMNCNAttachment : IBusiness
	{
		ZGuid BNA_BNS_FromShape { get; set; }
		ZGuid BNA_BNS_ToShape { get; set; }
		ZGuid BNA_BNS_Owner { get; set; }

		ZBool BNA_IsHidden { get; set; }
		ZBool BNA_IsDecouple { get; set; }

		ZString BNA_LayoutData { get; set; }
		ZString BNA_GS_NKApprovedBy { get; set; }

		ZGuid BNA_FP_ProcessHeaderLink { get; set; }

		void Decouple();
		bool IsBuffered { get; }
	}
}
