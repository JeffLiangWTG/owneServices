using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class MasterUCRHelper
	{
		public static MatchCollection PreparePatternMatch(ZString mucrNumber)
		{
			// Pattern: HBAC11122222222(33333333)(44) or HBAC11122222222        44
			var pattern = @"^
(?<ACP>([A-Z]))
(?<SHED>([A-Z]{3}))
(?<MAWB>([A-Z0-9]{3}\d{8}))
(?<HAWB>([A-Z0-9]{8}|[ ]{8})?)
(?<SPLIT>(\d{2})?)
$";
			pattern = pattern.Replace(System.Environment.NewLine, "");
			var matches = Regex.Matches(mucrNumber, pattern);
			return matches;
		}

		public static ZString GetMatchedCode(MatchCollection matches, string groupName)
		{
			return matches[0].Groups[groupName].Value;
		}

		public static Integration.Customs.GB.CCSUK.ICcsukCusAwbBase FindMawbFromPatternMatch(MatchCollection matches, BusinessObjectFactory factory)
		{
			Integration.Customs.GB.CCSUK.ICcsukCusAwbBase mawb = null;
			var acpCode = GetMatchedCode(matches, "ACP");
			var shedCode = GetMatchedCode(matches, "SHED");
			var shed = GetShedByShedCodeAndACPCode(factory, shedCode, acpCode);
			if (shed != null)
			{
				var mawbFromMucr = GetMatchedCode(matches, "MAWB");
				var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, mawbFromMucr);
				mawbQuery.AddToFilter(CusMAWBSchema.CM_IsActive, true);
				mawbQuery.OrderBy = Customs.Business.CusMAWB.Schema.CM_SystemCreateTimeUtc;
				mawb = factory.Load<Customs.Business.CusMAWB>(mawbQuery).OfType<Integration.Customs.GB.CCSUK.ICcsukCusAwbBase>().
					FirstOrDefault(m => m.CargoTerminalOperator == shed.ShedCode && m.CargoTerminalOperatorAirport == shed.PortCode);
			}
			return mawb;
		}

		public static Integration.Customs.GB.CCSUK.ICcsukCusAwbBase FindHawbFromPattern(ZString optionalHawbFromMucr, ZGuid mawbOrBasicPK, BusinessObjectFactory factory)
		{
			var hawbQuery = new ZQuery(CusHAWBSchema.CS_CM, mawbOrBasicPK);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_IsActive, true);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_HAWB, optionalHawbFromMucr);
			hawbQuery.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
			hawbQuery.OrderBy = Customs.Business.CusHAWB.Schema.CS_SystemCreateTimeUtc;
			return factory.Load<Customs.Business.CusHAWB>(hawbQuery).OfType<Integration.Customs.GB.CCSUK.ICcsukCusAwbBase>().FirstOrDefault();
		}

		static EU.Business.Shed GetShedByShedCodeAndACPCode(BusinessObjectFactory factory, ZString shedCode, ZString acpCode)
		{
			return factory.GetCachedValue("GetShedByShedCodeAndACPCode_" + shedCode + "_" + acpCode, delegate
			{
				var cusCodeList = factory.LoadTop1<ZZRefCusCodeListCombined>(GetShedQuery(factory, shedCode, acpCode));
				return cusCodeList != null ? new EU.Business.Shed(cusCodeList) : null;
			});
		}

		static ZQuery GetShedQuery(BusinessObjectFactory factory, ZString shedCode, ZString acpCode)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			result.AddToFilter(EU.Business.Shed.GetFilter(factory, Core.Constants.CountryCodes.UnitedKingdom));
			result.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.EndsWith, shedCode);

			var attributeQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
			attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, EU.Business.UniversalReferenceConstants.ShedAttributes.ACPCode);
			attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, acpCode);
			result.AddSubQuery(attributeQuery, JoinCondition.And);
			result.OrderBy = ZZRefCusCodeListCombined.Schema.ZZD_StartDate;

			return result;
		}
	}
}
