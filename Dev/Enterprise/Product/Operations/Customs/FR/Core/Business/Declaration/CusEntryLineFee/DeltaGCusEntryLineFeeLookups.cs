using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGCusEntryLineFeeLookups : CusEntryLineFeeLookups
	{
		public DeltaGCusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList NationalFeeTypeCodeList => new NationalFeeTypeCodeList(Factory, Parent.EntryLine?.RandomLine.UniversalTariff, Parent.EntryLine?.RandomLine.EffectiveAssessmentDate);
	}
}
