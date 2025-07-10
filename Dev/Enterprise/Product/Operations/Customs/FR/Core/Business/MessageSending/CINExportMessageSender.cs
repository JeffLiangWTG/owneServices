using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class CINExportMessageSender : EntryMessageSender<DeltaGJobDeclarationMessageSendingObject>
	{
		public CINExportMessageSender(DeltaGJobDeclarationMessageSendingObject decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
		{
		}

		protected override MessageBuilderManager<DeltaGJobDeclarationMessageSendingObject> GetBuilderManager()
		{
			return new CINExportMessageBuilderManager(errorCollector);
		}

		protected override bool ShouldIncreaseSequenceNumber => false;

		protected override void PostSend(ZBool result)
		{
			MarkMessagesFailToSave(result);
		}

		void MarkMessagesFailToSave(ZBool result)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var header = factory.Load<CusEntryHeader>(entryHeader.PK.ToGuid());
			if (null != header)
			{
				header.Logs.AddNew(result ? AutoEvents.MessageSent : AutoEvents.MessageGenerationFailed, objectToSend.MessageType, ZDateTimeOffset.Now);
			}
			ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true, false);
		}
	}
}
