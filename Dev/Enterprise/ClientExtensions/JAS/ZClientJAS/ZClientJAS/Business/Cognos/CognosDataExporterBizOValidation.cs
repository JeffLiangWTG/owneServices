
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosDataExporterBizOValidation : JASDataExporterBizOValidation
	{
		public CognosDataExporterBizOValidation(CognosDataExporterBizO parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEndingPeriod();
		}

		public void ValidateEndingPeriod()
		{
			ValidateCalculatedProperty(Parent.EndingPeriodInfo);
		}

		protected void CheckEndingPeriod()
		{
			MandatoryValidation.CheckEntered(Parent.EndingPeriodInfo);
			if (!Parent.EndingPeriod.IsEmpty && !Parent.PeriodCalculator.IsPeriodValid(Parent.EndingPeriod))
			{
				Parent.EndingPeriodInfo.AddError("Invalid period. Please enter period as the following format YYYYMM (i.e. 200512) and please ensure that the period is set up in GL.");
			}
		}

		new CognosDataExporterBizO Parent
		{
			get { return (CognosDataExporterBizO)base.Parent; }
		}
	}
}

#region Implementation
#endregion
