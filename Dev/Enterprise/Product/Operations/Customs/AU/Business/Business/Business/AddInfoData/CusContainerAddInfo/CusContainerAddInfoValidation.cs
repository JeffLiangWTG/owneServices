
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusContainerAddInfoValidation : AUAddInfoValidation
	{
		public CusContainerAddInfoValidation(CusContainerAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_AQISSealStart_Hidden()
		{
			base.CheckZA_AQISSealStart_Hidden();
			DoSealChecks(Parent.ZA_AQISSealStart_HiddenInfo);
			Parent.Validation.ValidateZA_AQISSealEnd_Hidden();
		}

		protected override void CheckZA_AQISSealEnd_Hidden()
		{
			base.CheckZA_AQISSealEnd_Hidden();
			DoSealChecks(Parent.ZA_AQISSealEnd_HiddenInfo);
			Parent.Validation.ValidateZA_AQISSealStart_Hidden();
		}

		void DoSealChecks(ZPropertyInfo infoToNotify)
		{
			if (JobDeclaration != null && JobDeclaration.IsQuarantine && Parent.Parent != null)
			{
				if (Parent.Parent.CO_Seal.IsEmpty)
				{
					if (!Parent.ZA_AQISSealStart_Hidden.IsEmpty || !Parent.ZA_AQISSealEnd_Hidden.IsEmpty)
					{
						ZDecimal startSeal;
						ZDecimal startEnd;
						ZDecimal.TryParse(Parent.ZA_AQISSealStart_Hidden.KeepChars("0123456789"), out startSeal);
						ZDecimal.TryParse(Parent.ZA_AQISSealEnd_Hidden.KeepChars("0123456789"), out startEnd);
						if (startSeal > startEnd)
						{
							infoToNotify.AddMessageError(sealNumbersInvalid);
						}
					}
				}
				else
				{
					if (!infoToNotify.Value.IsEmpty)
					{
						infoToNotify.AddMessageError(sealEntered);
					}
				}
			}
		}
		internal const string sealNumbersInvalid = "The End Seal Number must be greater than the Start Seal Number";
		internal const string sealEntered = "Start and End Seal Numbers may not be entered if a Container Seal is entered";

		protected new CusContainerAddInfo Parent
		{
			get { return (CusContainerAddInfo)base.Parent; }
		}
	}
}
