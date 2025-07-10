namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	class ZZDatabaseValidationHelper : ASYCUDA.Business.ZZDatabaseValidationHelper
	{
		public ZZDatabaseValidationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override bool GetMandatoryFieldsInZZ => false;
	}
}
