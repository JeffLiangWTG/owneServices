namespace Enterprise.Client.JAS.Business.Testing
{
	class JASDataExporterBizOForTest : JASDataExporterBizO
	{
		protected override string EmailSubject
		{
			get
			{
				return "EmailSubject";
			}
		}

		protected override JASDataExporterBizOValidation GetNewJASDataExporterBizOValidation()
		{
			return new JASDataExporterBizOValidation(this);
		}

		public new void DeliverFiles(params string[] fullPathToSourceFiles)
		{
			base.DeliverFiles(fullPathToSourceFiles);
		}
	}
}
