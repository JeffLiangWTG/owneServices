using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class TransactionIDRequestProcessor : XTTInboundInterchangeProcessor
	{
		public TransactionIDRequestProcessor(TransactionIDManager.SendErrorNotificationDelegate sendErrorNotification, int noOfDaysOld, string transactionType, string applicationCode)
		{
			this.sendErrorNotification = sendErrorNotification;
			this.noOfDaysOld = noOfDaysOld;
			this.transactionType = transactionType;
			this.applicationCode = Argument.NotNullOrEmpty(applicationCode, nameof(applicationCode));
		}
		protected readonly TransactionIDManager.SendErrorNotificationDelegate sendErrorNotification;
		protected readonly string transactionType;
		readonly string applicationCode;
		readonly protected int noOfDaysOld;

		protected override string[] ApplicationCodes => new[] { applicationCode };

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new TransactionIDCreator(Logger, sendErrorNotification, noOfDaysOld, transactionType);
		}

		protected override ZQuery GetInterchangeTypeFilter() => new ZQuery(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID);
	}
}
