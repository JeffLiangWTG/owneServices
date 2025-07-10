using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class RiskManagementCollection : CusSupportingInfoCollection<RiskManagement>
	{
		public RiskManagementCollection(CusEntryInstruction parent) : base(parent, Constants.CusSupportingInfoTypes.RiskManagement)
		{
		}
	}
}
