namespace Enterprise.Customs.GB.Business
{
	public class GuaranteeConfiguration : EU.NCTS.Business.GuaranteeConfiguration
	{
		protected override bool ApplySecurityToPW_OverrideCore(EU.NCTS.Business.NctsHeader header) => true;
	}
}
