using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class CSARevenueSummaryFormMessageManager : NonPersistentBusinessObject
	{
		public CSARevenueSummaryFormMessageManager(CusStatementHeader cusStatementHeader, MessageSubTypes messageSubType)
		{
			this.csaRevenueSummaryForm = cusStatementHeader;
			this.messageSubType = messageSubType;
		}

		readonly CusStatementHeader csaRevenueSummaryForm;
		readonly MessageSubTypes messageSubType;

		public void SendMessage()
		{
			var wrapper = new CSARSFMessageWrapper(csaRevenueSummaryForm);
			try
			{
				wrapper.PreProcessBeforeSendMessage(messageSubType);
				var results = new CSARevenueSummaryFormMessageBuilder(wrapper, messageSubType).PopulateMessages().GetBuilderResults();
				var testMode = !Env.Instance.IsProductionSystem;
				foreach (var result in results)
				{
					var message = result.Message;
					message.EM_LinkedObject = csaRevenueSummaryForm;
					message.EM_IsTestMessage = testMode;
					csaRevenueSummaryForm.Messages.Add(message);
				}
				csaRevenueSummaryForm.Factory.Save();
			}
			catch (Exception ex)
			{
				wrapper.UndoPreProcess();
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}
	}
}
