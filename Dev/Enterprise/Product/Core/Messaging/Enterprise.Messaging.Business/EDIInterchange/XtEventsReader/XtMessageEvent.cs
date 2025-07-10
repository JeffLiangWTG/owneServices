using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business
{
	public class XtMessageEvent : NonPersistentBusinessObject
	{
		public XtMessageEvent(EDIInterchange interchange, IXtMessageEventData eventData)
		{
			sourceData = eventData;
			this.interchange = interchange;
		}

		readonly EDIInterchange interchange;

		public ZInt Sequence { get; set; }

		public ZString XtMsgId => sourceData.XtMsgId.ToString();

		public ZGuid InterchangeGuid => interchange?.PK ?? ZGuid.Empty;

		public ZString InterchangeId => interchange?.EI_InterchangeNum ?? ZString.Empty;

		public ZInt LogEvent => sourceData.LogEvent;

		public ZString LogText => sourceData.LogText;

		public ZDateTime LogTime => new ZDateTimeOffset(sourceData.LogTime).ToUtcZDateTime().ToLocalBranchTime();

		readonly IXtMessageEventData sourceData;
	}
}
