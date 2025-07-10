using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsGuaranteeLookups : EU.NCTS.Business.NctsGuaranteeLookups
{
	public NctsGuaranteeLookups(NctsGuarantee parent) : base(parent)
	{
	}

	protected new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

	NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.Parent.NctsHeader.MovementHeader;

	protected override CodeDescriptionPairList BondTypeListCore => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NCTSBondType, MovementHeader?.ValuationDate ?? ZDateTime.Now);

	protected override CusGuaranteeHeaderCollection GetGuaranteeHeaderCollection() => Factory.GetCachedValue("CH.NCTS.Business.NctsGuaranteeLookups.GuaranteeHeaderCollection", () =>
		new CusGuaranteeHeaderCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Switzerland }, new ZString[] { GuaranteeTypeList.Codes.TRA }));

	public override CusGuaranteeHeaderCollection ReferenceNumbers
	{
		get
		{
			var bondType = Parent.PW_BondType;
			return Factory.GetCachedValue("CH.NCTS.Business.NctsGuaranteeLookups.ReferenceNumbers_" + bondType, () =>
			{
				var guarantees = new CusGuaranteeHeaderCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Switzerland }, new ZString[] { EUGuaranteeTypeList.Codes.TRA });
				guarantees.AdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_SubType, bondType);
				return guarantees;
			});
		}
	}

	public BaseCusGuaranteeHeader SingleReferenceNumber => Factory.GetCachedValue("CH.NCTS.Business.NctsGuaranteeLookups.SingleReferenceNumber_" + Parent.PW_BondType, () => ReferenceNumbers.Count == 1 ? ReferenceNumbers[0] : null);
}
