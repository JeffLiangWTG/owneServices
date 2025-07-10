
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DeferredB3SendActionListWithCancel : DeferredB3SendActionList
	{
		public new class Codes : DeferredB3SendActionList.Codes
		{
			public const string Cancel = "CAN";
		}

		public new class Descriptions : DeferredB3SendActionList.Descriptions
		{
			public static MultilingualString Cancel { get { return ResString.GetMultilingualString("DeferredB3SendActionListWithCancel|Cancel", "Cancel Scheduled Message without Rescheduling"); } }
		}

		public DeferredB3SendActionListWithCancel()
			: base()
		{
			AddPair(Codes.Cancel, Descriptions.Cancel);
		}
	}
}
