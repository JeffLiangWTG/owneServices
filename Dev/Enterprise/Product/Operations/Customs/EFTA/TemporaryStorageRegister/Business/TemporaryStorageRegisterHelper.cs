using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public static class TemporaryStorageRegisterHelper
{
	public static ActiveBusinessObjectCollection<CusTempStorageRegLine> GetCusTempStorageRegLineCollection(ZString customsOffice, string temporaryStorageApplicationCode, BusinessObjectFactory factory)
	{
		var regHeaderQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegHeader), CusTempStorageRegHeaderSchema.PK);
		regHeaderQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_AppCode, temporaryStorageApplicationCode);
		if (!customsOffice.IsEmpty)
		{
			regHeaderQuery.AddToFilter(CusTempStorageRegHeaderSchema.SRH_CustomsOffice, customsOffice);
		}

		var limitDateQuery = new ZQuery(CusTempStorageRegLineSchema.SRL_LimitDate, null);
		limitDateQuery.AddToFilter(new ZQuery(CusTempStorageRegLineSchema.SRL_LimitDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today), JoinCondition.Or);
		var regLineQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
		regLineQuery.AddToFilter(CusTempStorageRegLineSchema.SRL_CustomsStatus, SQLComparisonOperator.NotEqual, CustomsStatusList.Codes.EFTA_FIN);
		regLineQuery.AddToFilter(CusTempStorageRegLineSchema.SRL_CustomsStatus, SQLComparisonOperator.NotEqual, CustomsStatusList.Codes.EFTA_DEL);
		regLineQuery.AddToFilter(limitDateQuery);
		regLineQuery.AddSubQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeaderQuery, JoinCondition.And);

		var regLineCollection = new ActiveBusinessObjectCollection<CusTempStorageRegLine>(factory, regLineQuery);
		return regLineCollection;
	}
}
