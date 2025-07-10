using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class AESCommonLocationOfGoodsWrapper(CusEntryInstruction entryInstruction) : CommonLocationOfGoodsWrapper(entryInstruction), IAESCommonLocationOfGoods
{
	public IPartyContactProvider LocationContactPerson => locationContactPerson ?? (locationContactPerson = goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
																												? null
																												: PartyContactWrapper.New(goodsLocation.Address.E2_Contact,
																																			goodsLocation.Address.E2_Email,
																																			goodsLocation.Address.E2_Phone));
	PartyContactWrapper locationContactPerson;
}
