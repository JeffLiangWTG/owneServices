using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class TSTCustomsNumberViewStmNumsValidation : CustomsNumberViewStmNumsValidation
	{
		public TSTCustomsNumberViewStmNumsValidation(CustomsNumberViewStmNums parent) : base(parent)
		{
		}

		protected override void CheckSN_FountainName()
		{
			base.CheckSN_FountainName();
			MandatoryValidation.CheckEntered(Parent.SN_FountainNameInfo);
		}
	}
}
