using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCSQuarantineExDocEstablishmentAndTimeLookups : QuarantineExDocEstablishmentAndTimeLookups
	{
		public EXDOCSQuarantineExDocEstablishmentAndTimeLookups(QuarantineExDocEstablishmentAndTime parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList TreatmentCode
		{
			get
			{
				return Factory.GetCachedValue("AQISTreatmentCodeLookups.AQISTreatmentCodeList", () =>
				{
					return CMRReferenceDataHelper.SetupAQISTreatmentCodeList(Factory);
				});
			}
		}
	}
}
