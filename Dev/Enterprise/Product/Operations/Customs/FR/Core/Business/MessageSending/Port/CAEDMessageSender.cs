using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class CAEDMessageSender : MessageSender<CAEDMessageSendingObject>
	{
		public CAEDMessageSender(CAEDMessageSendingObject objectToSend, ErrorCollector errorCollector) : base(objectToSend, errorCollector)
		{
		}

		protected override MessageBuilderManager<CAEDMessageSendingObject> GetBuilderManager()
		{
			return new CAEDMessageBuilderManager(errorCollector);
		}

		protected override void PostSend(ZBool result)
		{
			MarkMessagesFailToSave(result);
		}

		void MarkMessagesFailToSave(ZBool result)
		{
			EnterpriseBusinessObject headerObject;
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			switch (objectToSend.StmALogParent.LogsParentTableName)
			{
				case AutoCusInBondHeader.Schema.TableName:
					headerObject = factory.Load<NctsHeader>(objectToSend.StmALogParent.LogsParentPK.ToGuid());
					break;
				case AutoCusEntryHeader.Schema.TableName:
					headerObject = factory.Load<CusEntryHeader>(objectToSend.StmALogParent.LogsParentPK.ToGuid());
					break;
				default:
					return;
			}
			if (null != headerObject)
			{
				headerObject.Logs.AddNew(result ? AutoEvents.MessageSent : AutoEvents.MessageGenerationFailed, objectToSend.MessageType, ZDateTimeOffset.Now);
			}
			ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true, false);
		}

		protected override ZString ApplicationCode => FREDIMessage.ApplicationCodes.FRPortMessage;
	}
}
