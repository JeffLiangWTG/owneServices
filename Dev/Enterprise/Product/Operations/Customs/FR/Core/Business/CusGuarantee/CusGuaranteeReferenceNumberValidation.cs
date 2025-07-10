using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeReferenceNumberValidation : CusCodeDataValidation
	{
		public CusGuaranteeReferenceNumberValidation(CusGuaranteeReferenceNumber parent)
			: base(parent)
		{
		}

		new CusGuaranteeReferenceNumber Parent
		{
			get { return (CusGuaranteeReferenceNumber)base.Parent; }
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var parent = Parent;
			if (parent.CusGuarantee.CPH_Type == GuaranteeTypeList.Codes.COD && parent.CY_Code == OrgCusAccountDeltaTTypeList.Codes.TR)
			{
				ValidationExtendMethods.RuleTR301(parent.CusGuarantee.CPH_SubType, parent.CY_Data, parent.CY_DataInfo);
			}
		}
	}
}
