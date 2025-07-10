namespace Enterprise.Customs.BR.Business
{
	public class LocalPartNumberValidation : Customs.Business.CusGoodsCatalogProductionInfoValidation
	{
		public LocalPartNumberValidation(LocalPartNumber parent)
			: base(parent)
		{
		}

		public new LocalPartNumber Parent => (LocalPartNumber)base.Parent;

		protected override void CheckCGI_Reference()
		{
			base.CheckCGI_Reference();
			if (!Parent.CGI_Reference.IsEmpty)
			{
				var warningMessage = Parent.PivotFinder.WarningMessage;
				if (!warningMessage.IsEmpty)
				{
					Parent.CGI_ReferenceInfo.AddWarning(warningMessage);
				}
			}
		}
	}
}
