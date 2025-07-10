using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSRegistrationNumberValidation : CusCodeDataValidation
	{
		public AIRSRegistrationNumberValidation(AIRSRegistrationNumber parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
		}

		protected override void CheckCY_Data()
		{
			if (!Parent.CY_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
				if (!Parent.CY_Data.IsEmpty)
				{
					var cfia = ((AIRSRegistrationNumber)Parent).Parent as CFIAPGAHeader;
					if (cfia != null)
					{
						RegistrationNumberHelper.ValidateCFIARegNum(Parent as CusCodeData);
					}
				}
			}
		}
	}
}
