using Enterprise.Customs.Common.MX;

namespace Enterprise.Customs.MX.Business
{
	public class ClearanceCollection : Customs.Business.CusSupportingInfoCollection<Clearance>
	{
		public ClearanceCollection(CusEntryInstruction entryInstruction)
			: base(entryInstruction, CusSupportingInfoTypeList.Codes.Clearance)
		{
		}
	}
}
