using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETLineSecurityBlock : IETSecurityBlockCommon
{
	ZString UNDangerousGoodsCode { get; }
}
