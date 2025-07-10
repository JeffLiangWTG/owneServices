using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CusAttachmentCollection : CusCodeDataCollection<CusAttachment>
	{
		public CusAttachmentCollection(CusEntryInstruction parent) : base(parent, Constants.CusCodeDataTypes.Codes.CusAttachment)
		{
		}
	}
}
