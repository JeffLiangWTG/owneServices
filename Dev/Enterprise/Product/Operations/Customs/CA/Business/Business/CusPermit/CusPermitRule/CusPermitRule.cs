using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusPermitRule : Customs.Business.BaseCusPermitRule, Integration.Customs.CA.ICusPermitRule
	{
		public CusPermitRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPR_RuleCode = CAPermitRuleCodeList.Codes.TAR;
		}

		#endregion

	}
}
