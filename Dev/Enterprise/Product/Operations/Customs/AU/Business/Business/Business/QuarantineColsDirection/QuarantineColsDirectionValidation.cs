using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsDirectionValidation : AutoQuarantineColsDirectionValidation
	{
		public QuarantineColsDirectionValidation(AutoQuarantineColsDirection parent) : base(parent)
		{
		}

		protected override void CheckQCD_Direction()
		{
			base.CheckQCD_Direction();
			ListValidation.MessageErrorIfInvalidCode(Parent.QCD_DirectionInfo);
		}
	}
}
