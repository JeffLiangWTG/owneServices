using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.Module
{
	public partial class DocumentListMessagesFilterStripControl : ZFilterStripControl
	{
		public DocumentListMessagesFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			ReOrderColumns();
		}
		void ReOrderColumns()
		{
			Grid.ReOrderColumnsAndChangeVisibility(listColumns);
		}
		readonly string[] listColumns =
		{
			EDIMessage.Schema.EM_MessageNum,
			EDIMessage.Schema.EM_ReceiveTransmit,
			EDIMessage.Schema.EM_ApplicationCode,
			EDIMessage.Schema.EM_MessageSubType,
			EDIMessage.Schema.EM_Status,
			EDIMessage.Schema.EM_MessageDateTime,
			EDIMessage.Schema.EM_SendingUser,
			EDIMessage.Schema.EM_DateTimeInterchangeSent,
			EDIMessage.Schema.EM_MessageText,
			EDIMessage.Schema.EM_InterchangeNumber,
			EDIMessage.Schema.EM_InterchangeStatus,
			EDIMessage.Schema.EM_InterchangeSender,
			EDIMessage.Schema.EM_InterchangeReceiver,
			nameof(EDIMessage.Branch),
			EDIMessage.Schema.EM_SystemCreateUser,
			EDIMessage.Schema.EM_SystemCreateTimeUtc,
			EDIMessage.Schema.EM_SystemLastEditUser,
			EDIMessage.Schema.EM_SystemLastEditTimeUtc,
		};
	}
}
