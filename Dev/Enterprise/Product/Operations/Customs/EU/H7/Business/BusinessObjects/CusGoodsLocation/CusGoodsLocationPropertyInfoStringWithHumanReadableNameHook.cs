using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.H7.Business
{
	public class CusGoodsLocationPropertyInfoStringWithHumanReadableNameHook : ZPropertyInfoString
	{
		public CusGoodsLocationPropertyInfoStringWithHumanReadableNameHook(BusinessObject businessObject, ZPropertyInfo innerInfo)
			: base(businessObject, innerInfo.Name)
		{
			this.businessObject = businessObject;
		}

		readonly BusinessObject businessObject;

		protected override ZString GetHumanReadableNameCore()
		{
			using (((IBusinessObjectInternals)businessObject).SuppressReportRowDeletedError())
			{
				return Res.GetString("6e0d7fd2-9d6c-4d0e-828f-29138f69a415", "Location of Goods: {0}", Description);
			}
		}
	}
}
