using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business
{
	class CusAuthorisationHeaderValidation : Customs.Business.CusAuthorisationHeaderValidation
	{
		public CusAuthorisationHeaderValidation(CusAuthorisationHeader parent)
		: base(parent)
		{
		}

		protected override void CheckCPH_NumberFormat()
		{
			var parent = Parent;
			var provider = (CusAuthorisationHeaderProvider)parent.Provider;
			if (!parent.CPH_Number.IsEmpty && !provider.IsAuthorisationNumberValid(parent))
			{
				parent.CPH_NumberInfo.AddWarning(provider.GetAuthorisationNumberInvalidFormatMessage(parent));
			}
		}
	}
}
