using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class OnlineOrderCollection : CusCodeDataCollection<OnlineOrder>
	{
		public OnlineOrderCollection(CusEntryInstruction entryInstruction)
			: base(entryInstruction, CusCodeDataTypeList.Codes.OnlineOrder)
		{
		}
	}
}
