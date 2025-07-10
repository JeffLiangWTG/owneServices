using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	class CusAuthorisationHeaderValidation : Customs.Business.CusAuthorisationHeaderValidation
	{
		public CusAuthorisationHeaderValidation(CusAuthorisationHeader parent)
		: base(parent)
		{
		}

		protected override void CheckCPH_OH_PermitHolder()
		{
			base.CheckCPH_OH_PermitHolder();

			if (!Parent.CPH_OH_PermitHolder.IsEmpty && Parent.CPH_Type.IsACEorACT() && Parent.PermitHolder.GetEUEoriNumber(Core.Constants.CountryCodes.Germany).IsEmpty)
			{
				Parent.CPH_OH_PermitHolderInfo.AddMessageError(Res.GetString("99ff24bf-e020-4dd9-a14c-a4856340c83c", "The captured Authorization Type requires a German EORI-Number in Authorization Holders Organization (Registration Numbers / Codes)."));
			}
		}
	}
}
