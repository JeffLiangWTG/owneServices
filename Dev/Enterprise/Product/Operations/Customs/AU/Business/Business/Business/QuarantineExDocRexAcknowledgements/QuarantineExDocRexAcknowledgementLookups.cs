using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocRexAcknowledgementLookups : Customs.Business.CusCodeDataLookups
	{
		public QuarantineExDocRexAcknowledgementLookups(QuarantineExDocRexAcknowledgement parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue("Enterprise.Customs.AU.Business.EXDOC.QuarantineExDocRexAcknowledgementLookups.CY_CodeList", () => new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(QuarantineExDocRexAcknowledgement.AcknowledgementCode, "Acknowledgement")
		});
	}
}
