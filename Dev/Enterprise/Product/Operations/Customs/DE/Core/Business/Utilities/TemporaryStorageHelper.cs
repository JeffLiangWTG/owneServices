using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public static class TemporaryStorageHelper
	{
		public static ActiveBusinessObjectCollection<CusTempStorageRegLine> GetCusTempStorageRegLineCollection(ZString customsOffice, BusinessObjectFactory factory)
		{
			var regHeaderQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegHeader), CusTempStorageRegHeaderSchema.PK);
			regHeaderQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_AppCode, TemporaryStorageApplicationCodeList.Codes.SumA);
			if (!customsOffice.IsEmpty)
			{
				regHeaderQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_CustomsOffice, customsOffice);
			}

			var limitDateQuery = new ZQuery(CusTempStorageRegLineSchema.SRL_LimitDate, null);
			limitDateQuery.AddToFilter(new ZQuery(CusTempStorageRegLineSchema.SRL_LimitDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today), JoinCondition.Or);
			var regLineQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
			regLineQuery.AddToFilter(CusTempStorageRegLineSchema.SRL_CustomsStatus, SQLComparisonOperator.NotEqual, CustomsStatusList.Codes.FIN);
			regLineQuery.AddToFilter(CusTempStorageRegLineSchema.SRL_CustomsStatus, SQLComparisonOperator.NotEqual, CustomsStatusList.Codes.DEL);
			regLineQuery.AddToFilter(limitDateQuery);
			regLineQuery.AddSubQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeaderQuery, JoinCondition.And);

			var gridCollection = new ActiveBusinessObjectCollection<CusTempStorageRegLine>(factory, regLineQuery);
			return gridCollection;
		}
	}
}
