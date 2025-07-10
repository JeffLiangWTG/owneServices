using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs.CN;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public static class EnterpriseQualificationListHelper
	{
		static readonly ImmutableArray<string> EnterpriseQualificationCodesImport = ImmutableArray.Create(
			EnterpriseQualificationList.Codes._100,
			EnterpriseQualificationList.Codes._101,
			EnterpriseQualificationList.Codes._200,
			EnterpriseQualificationList.Codes._300,
			EnterpriseQualificationList.Codes._303,
			EnterpriseQualificationList.Codes._306,
			EnterpriseQualificationList.Codes._307,
			EnterpriseQualificationList.Codes._312,
			EnterpriseQualificationList.Codes._317,
			EnterpriseQualificationList.Codes._319,
			EnterpriseQualificationList.Codes._320,
			EnterpriseQualificationList.Codes._321,
			EnterpriseQualificationList.Codes._322,
			EnterpriseQualificationList.Codes._326,
			EnterpriseQualificationList.Codes._327,
			EnterpriseQualificationList.Codes._400,
			EnterpriseQualificationList.Codes._413,
			EnterpriseQualificationList.Codes._414,
			EnterpriseQualificationList.Codes._415,
			EnterpriseQualificationList.Codes._416,
			EnterpriseQualificationList.Codes._418,
			EnterpriseQualificationList.Codes._421,
			EnterpriseQualificationList.Codes._500,
			EnterpriseQualificationList.Codes._508,
			EnterpriseQualificationList.Codes._509,
			EnterpriseQualificationList.Codes._510,
			EnterpriseQualificationList.Codes._511,
			EnterpriseQualificationList.Codes._513,
			EnterpriseQualificationList.Codes._515,
			EnterpriseQualificationList.Codes._524,
			EnterpriseQualificationList.Codes._600,
			EnterpriseQualificationList.Codes._601,
			EnterpriseQualificationList.Codes._603,
			EnterpriseQualificationList.Codes._700);

		static readonly ImmutableArray<string> EnterpriseQualificationCodesExport = ImmutableArray.Create(
			EnterpriseQualificationList.Codes._100,
			EnterpriseQualificationList.Codes._101,
			EnterpriseQualificationList.Codes._102,
			EnterpriseQualificationList.Codes._200,
			EnterpriseQualificationList.Codes._300,
			EnterpriseQualificationList.Codes._301,
			EnterpriseQualificationList.Codes._302,
			EnterpriseQualificationList.Codes._304,
			EnterpriseQualificationList.Codes._305,
			EnterpriseQualificationList.Codes._308,
			EnterpriseQualificationList.Codes._309,
			EnterpriseQualificationList.Codes._310,
			EnterpriseQualificationList.Codes._311,
			EnterpriseQualificationList.Codes._315,
			EnterpriseQualificationList.Codes._317,
			EnterpriseQualificationList.Codes._318,
			EnterpriseQualificationList.Codes._323,
			EnterpriseQualificationList.Codes._324,
			EnterpriseQualificationList.Codes._329,
			EnterpriseQualificationList.Codes._400,
			EnterpriseQualificationList.Codes._415,
			EnterpriseQualificationList.Codes._417,
			EnterpriseQualificationList.Codes._418,
			EnterpriseQualificationList.Codes._419,
			EnterpriseQualificationList.Codes._421,
			EnterpriseQualificationList.Codes._500,
			EnterpriseQualificationList.Codes._501,
			EnterpriseQualificationList.Codes._502,
			EnterpriseQualificationList.Codes._503,
			EnterpriseQualificationList.Codes._504,
			EnterpriseQualificationList.Codes._505,
			EnterpriseQualificationList.Codes._506,
			EnterpriseQualificationList.Codes._507,
			EnterpriseQualificationList.Codes._512,
			EnterpriseQualificationList.Codes._514,
			EnterpriseQualificationList.Codes._520,
			EnterpriseQualificationList.Codes._602,
			EnterpriseQualificationList.Codes._603,
			EnterpriseQualificationList.Codes._700);

		public static CodeDescriptionPairList GetCachedEnterpriseQualificationList(BusinessObjectFactory factory, bool isImport)
		{
			return factory.GetCachedValue<EnterpriseQualificationList>().FilterListByCodes(isImport ? EnterpriseQualificationCodesImport : EnterpriseQualificationCodesExport);
		}
	}
}
