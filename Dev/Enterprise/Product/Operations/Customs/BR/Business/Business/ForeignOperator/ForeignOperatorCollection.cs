using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ForeignOperatorCollection : BaseCusGoodsCatalogProductionInfoCollection<ForeignOperator>
	{
		public ForeignOperatorCollection(CusGoodsCatalog catalog) : base(catalog, CusGoodsCatalogProductionInfoTypeList.Codes.FOR)
		{
		}

		public override void Delete(ForeignOperator businessObject)
		{
			if (businessObject.CGI_CustomsStatus == CustomsPostedStatusList.Codes.Accepted)
			{
				businessObject.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			}
			else
			{
				base.Delete(businessObject);
			}
		}
	}
}
