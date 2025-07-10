using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS
{
	public class TP5MessageSendingObjectLookups : NctsHeaderMessageSendingObjectLookups
	{
		public TP5MessageSendingObjectLookups(TP5MessageSendingObject parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RegularJustificationCodesList => Factory.GetCachedValue("Enterprise.Customs.FR.NCTS.TP5MessageSendingObjectLookups.RegularJustificationCodesList", () => new TP5RegularJustificationCodeList());

		public CodeDescriptionPairList QueryIdentifierCodesList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL054, ZDate.Today);

		public CodeDescriptionPairList RequesterRoleCodesList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL156, ZDate.Today);
	}
}
