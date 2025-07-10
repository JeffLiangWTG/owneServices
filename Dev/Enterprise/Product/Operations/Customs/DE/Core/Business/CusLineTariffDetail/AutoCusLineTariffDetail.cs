using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public abstract class AutoCusLineTariffDetail : EU.Business.CusLineTariffDetail
	{
		protected AutoCusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AddInfo
		protected new AddInfoCusLineTariffDetail AddInfo => (AddInfoCusLineTariffDetail)base.AddInfo;

		protected override EU.Business.AddInfoCusLineTariffDetail GetNewAddInfo() => new AddInfoCusLineTariffDetail(BZ_NAddInfoInfo);

		public new AddInfoCusLineTariffDetailValidation AddInfoValidation => AddInfo.Validation;
		#endregion
	}
}
