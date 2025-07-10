using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

static class ResponseMessageCustomsStatusAnalyzerFactory
{
	public static IResponseMessageCustomsStatusAnalyzer CreateAnalyzerForMessageType(NctsDepartureMovementHeader movementHeader, ZString msgType)
	{
		_ = Argument.NotNullOrEmpty(msgType, nameof(msgType));

		return (string)msgType switch
		{
			IT.Business.EDIMessageTypeList.Codes.IrildesResponse => new IrildesResponseMessageCustomsStatusAnalyzer(movementHeader),
			IT.Business.EDIMessageTypeList.Codes.NewDeclaration => new XmlResponseMessageCustomsStatusAnalyzer(movementHeader),
			_ => throw new InvalidOperationException(),
		};
	}
}
