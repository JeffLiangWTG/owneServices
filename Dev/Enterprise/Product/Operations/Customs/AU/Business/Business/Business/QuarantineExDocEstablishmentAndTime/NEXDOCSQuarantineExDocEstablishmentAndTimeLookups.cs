using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCSQuarantineExDocEstablishmentAndTimeLookups : QuarantineExDocEstablishmentAndTimeLookups
	{
		public NEXDOCSQuarantineExDocEstablishmentAndTimeLookups(QuarantineExDocEstablishmentAndTime parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList EstablishmentIndicatorList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSEstablishmentIndicator, ZDate.Today);

		public override CodeDescriptionPairList EstablishmentPostedStatusList => Factory.GetCachedValue<NEXDOCEstablishmentPostedStatus>();

		public override CodeDescriptionPairList TreatmentCode => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSTreatment, ZDate.Today);
	}
}
