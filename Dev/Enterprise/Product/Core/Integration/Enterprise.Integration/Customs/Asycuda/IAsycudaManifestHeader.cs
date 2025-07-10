using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface IAsycudaManifestHeader : ManifestBase.IAsycudaManifestHeader
			{
				ZString AMA_MasterBill { get; set; }
				ZDate AMA_MasterBillIssueDate { get; set; }
				IAsycudaBill MasterBill { get; }
				ZString MasterBOL { get; set; }
				ZString ShippingAgentAddress { get; }
				ZString ShippingAgentName { get; }
				ZGuid ShippingAgentOrgPK { get; set; }
				ZDateTime RegistrationDate { get; set; }
				ZString RegistrationNumber { get; set; }
				ZString RegistrationStatus { get; set; }
			}
		}
	}
}
