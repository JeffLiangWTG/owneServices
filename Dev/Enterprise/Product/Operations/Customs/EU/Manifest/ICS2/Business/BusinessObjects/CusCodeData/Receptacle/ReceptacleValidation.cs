using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ReceptacleValidation : CusCodeDataValidation
	{
		public ReceptacleValidation(Receptacle parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
