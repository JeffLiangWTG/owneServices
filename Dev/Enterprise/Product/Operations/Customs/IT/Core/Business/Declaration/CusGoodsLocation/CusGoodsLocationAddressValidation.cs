namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
{
	public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent) : base(parent)
	{
	}

	protected override void CheckE2_Email()
	{
		if (!IsUcc6AndExport)
		{
			base.CheckE2_Email();
			return;
		}

		CusGoodsLocationAddressValidationHelper.ValidateE2_Email(GoodsLocationAddress);
	}

		protected override void CheckE2_Address1AndE2_Address2()
		{
			base.CheckE2_Address1AndE2_Address2();

			if (IsUcc6AndExport)
			{
				CusGoodsLocationAddressValidationHelper.ValidateE2_Address1AndE2_Address2(GoodsLocationAddress);
			}
		}

	protected override void CheckE2_RN_NKCountryCode()
	{
		base.CheckE2_RN_NKCountryCode();
		CusGoodsLocationAddressValidationHelper.ValidateE2_RN_NKCountryCode(GoodsLocationAddress, IsUcc6AndExport);
	}

	protected override void CheckE2_City()
	{
		base.CheckE2_City();
		if (IsUcc6AndExport)
		{
			CusGoodsLocationAddressValidationHelper.ValidateE2_City(GoodsLocationAddress);
		}
	}

	protected override void CheckE2_Contact()
	{
		base.CheckE2_Contact();
		if (IsUcc6AndExport)
		{
			CusGoodsLocationAddressValidationHelper.ValidateE2_Contact(GoodsLocationAddress);
		}
	}

	protected override void CheckE2_Phone()
	{
		base.CheckE2_Phone();
		if (IsUcc6AndExport)
		{
			CusGoodsLocationAddressValidationHelper.ValidateE2_Phone(GoodsLocationAddress);
		}
	}

	#region Implementation

	JobDeclaration ParentDeclaration => GoodsLocation?.Parent as JobDeclaration;

	CusGoodsLocationAddress GoodsLocationAddress => (CusGoodsLocationAddress)Parent;

	CusGoodsLocation GoodsLocation => GoodsLocationAddress.GoodsLocation;

	bool IsUcc6AndExport => ParentDeclaration?.IsUCC6AndIsExport ?? false;

	#endregion
}
