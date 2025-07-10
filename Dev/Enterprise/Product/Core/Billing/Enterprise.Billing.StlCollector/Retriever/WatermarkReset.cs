using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public static class WatermarkReset
	{
		public const string STLWatermarkResetInfoCode = "STLWMRESET";

		public static void RequestWatermarkReset(BusinessObjectFactory factory, DateTime resetTo, DateTime resetTime, string codes = "")
		{
			var config = factory.Load<RefSysConfig>(new ZQuery(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, SQLComparisonOperator.Equal, STLWatermarkResetInfoCode)).FirstOrDefault();
			config.ZRC_StringValue = $"{SqlFormatInfo.ToSqlDateTimeString(resetTo)}|{SqlFormatInfo.ToSqlDateTimeString(resetTime)}|{codes}";
			factory.Save();
		}
	}
}
