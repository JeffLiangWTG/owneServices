namespace Enterprise.Customs.FR.Business.Declaration
{
	public class ExportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		public ExportIncoTermAndCustomsChargeFactory() : base()
		{
		}

		protected override string GetConfigurationFile()
		{
			return ExportConfigurationFile;
		}

		const string ExportConfigurationFile = "Enterprise.Customs.FR.Business.Declaration.Valuation.ExportIncoTermsConfiguration.xml";
	}
}
