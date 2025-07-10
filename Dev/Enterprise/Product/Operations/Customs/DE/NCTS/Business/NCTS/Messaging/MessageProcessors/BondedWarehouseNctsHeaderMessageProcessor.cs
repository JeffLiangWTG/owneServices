using System;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business;

public sealed class BondedWarehouseNctsHeaderMessageProcessor : EU.NCTS.Business.BondedWarehouseNctsHeaderMessageProcessor
{
	public BondedWarehouseNctsHeaderMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
	{
	}

	protected override IWarehouseIntegrationSupporter GetSupporter() => ((NctsDepartureMovementHeader)message.EM_LinkedObject).Header;
}
