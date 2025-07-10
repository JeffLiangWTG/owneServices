namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSPackageValidation : EU.EMCS.Business.EMCSPackageValidation
	{
		public EMCSPackageValidation(EMCSPackage parent)
			: base(parent)
		{
		}

		protected override bool IsMarksAndNumbersRequired => !Parent.B5_MarksAndNumbersInfo.ReadOnly;
	}
}
