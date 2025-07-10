using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class EnterpriseQualificationCollection : CusCodeDataCollection<EnterpriseQualification>
	{
		public EnterpriseQualificationCollection(CusEntryInstruction parent) : base(parent, Constants.CusCodeDataTypes.Codes.EnterpriseQualification)
		{
		}
	}
}
