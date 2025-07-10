using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class DutyRateParser
	{
		//5% ?+ $61.71/LITRE ALCOHOL
		public static DutyResult ParseAndGetResult(BusinessObjectFactory factory, ZString dutyRateDescription)
		{
			var result = new DutyResult();

			foreach (ZString dutyRate in dutyRateDescription.Split('+'))
			{
				if (dutyRate.Contains('%'))
				{
					result.Percent = ZDecimal.Parse(dutyRate.KeepChars("0123456789."));
				}
				else if (dutyRate.Contains('$') && dutyRate.Contains('/'))
				{
					ZString[] rateSplit = dutyRate.Split('/');

					if (rateSplit.Length == 2)
					{
						result.FlatRateAmount = ZDecimal.Parse(rateSplit[0].KeepChars("0123456789."));

						var codeFromCustoms = rateSplit[1];
						var partialCodeFilter = new ZQuery(CMRCodeListsSchema.CI_CodeType, SQLComparisonOperator.Equal, CMRCodeLists.CodeTypes.QUANUNIT);
						partialCodeFilter.AddToFilter(JoinCondition.And, CMRCodeListsSchema.CI_Name, SQLComparisonOperator.StartsWith, codeFromCustoms);
						partialCodeFilter.OrderBy = CMRCodeListsSchema.Constants.CI_Name;
						var unitOfQuantityMatches = factory.Load<CMRCodeLists>(partialCodeFilter);
						if (unitOfQuantityMatches.Length > 0)
						{
							var firstMatch = unitOfQuantityMatches[0];
							result.FlatRateUQ = firstMatch.CI_Code;

							if (firstMatch.CI_Name != codeFromCustoms && unitOfQuantityMatches.Length > 1)
							{
								var errorMessage = $@"Customs have truncated the duty per quantity UQ code and multiple potential matches have been found: " + dutyRateDescription;
								ExceptionReporter.Instance.ReportDeveloperException("Customs have truncated the duty UQ", errorMessage, new InvalidOperationException(errorMessage));
							}
						}
					}
				}
			}

			return result;
		}
	}
}
