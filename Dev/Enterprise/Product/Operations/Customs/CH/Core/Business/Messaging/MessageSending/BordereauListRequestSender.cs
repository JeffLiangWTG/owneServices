using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

public sealed class BordereauListRequestSender : BaseEdecCompanyMessageSender
{
	public BordereauListRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Bordereau List message";

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsEdec;

	protected override ZString MessageType => MessageTypeCodeList.Codes.BOR;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.BordereauList;

	protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbExternalPassword credentials)
	{
		var factory = company.Factory;

		var today = ZDate.Today;
		var sendingObject = new BordereauListRequestSendingObject { StartDate = today.AddDays(-CHCustomsDataRegistry.Instance.EdecBordereauConfig.Value.NumberOfDays), EndDate = today };

		CreateEDIMessage(factory, company, sendingObject.ToMessageString(), ZString.Empty, credentialsPK: credentials.PK);
		ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);

		return 1;
	}
}
