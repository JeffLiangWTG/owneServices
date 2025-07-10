using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseMessageListRequestSender : BasePassarCompanyMessageSender
{
	public BaseMessageListRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString MessageType => MessageTypeCodeList.Codes.MSL;

	protected override ZString MessageSubType => ZString.Empty;

	protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbCompanyTokenCredentials tokenCredentials)
	{
		var lastMessageId = company.LoadLastMessageIdTransaction(ApplicationCode)?.CPT_TransactionID ?? ZString.Empty;

		var factory = company.Factory;
		var message = CreateEDIMessage(factory, company, MessagePlaceHolder, ZString.Empty, tokenCredentials?.PK);

		var interchange = CreateEDIInterchanges(message).FirstOrDefault();

		var headerAttributes = GetHeaderTextWithAttribute(lastMessageId);

		interchange.SetHeaderTextWithAttributeDictionary(headerAttributes);
		interchange.EI_BodyText = ZString.Empty;
		message.EM_MessageText = interchange.EI_HeaderText;

		ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);

		return 1;
	}

	protected virtual Dictionary<string, string> GetHeaderTextWithAttribute(ZString lastMessageId)
	{
		return CustomsMessageHelper.CreateHeaderAttributes()
			.AddBpId()
			.AddLastMessageId(lastMessageId);
	}
}
