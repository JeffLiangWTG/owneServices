using System;
using System.Globalization;
using System.Linq;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class EnterpriseModuleListTest : TransactionedTestCase
	{
		public void TestOrder()
		{
			EnterpriseModuleList moduleList = new EnterpriseModuleList();

			for (int i = 0, j = 1; j < moduleList.Count; i++, j++)
			{
				ICodeDescription x = moduleList[i];
				ICodeDescription y = moduleList[j];

				Assert("Order should be correct. " + x.Description + " - " + y.Description + ", " + x.Code + " - " + y.Code + ": "
					+ string.Compare(x.Description, y.Description, StringComparison.Ordinal)
					+ ", "
					+ string.Compare(x.Code, y.Code, StringComparison.Ordinal),
					(string.Compare(x.Description, y.Description, StringComparison.Ordinal) < 0) ||
					(string.Compare(x.Description, y.Description, StringComparison.Ordinal) == 0 && string.Compare(x.Code, y.Code, StringComparison.Ordinal) <= 0));
			}
		}

		public void TestCheckDuplicates()
		{
			EnterpriseModuleList moduleList = new EnterpriseModuleList();
			var codesUsedByMultipleModules =
				from ICodeDescription module in moduleList
				group module by module.Code into moduleGroup
				where moduleGroup.Count() > 1
				select new { Code = moduleGroup.Key, Modules = moduleGroup.ToList() };

			if (codesUsedByMultipleModules.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var codeUsedByMultipleModules in codesUsedByMultipleModules)
					{
						Fail(string.Format(CultureInfo.CurrentCulture, "Code [{0}] is not unique. The following modules have this Code: {1}", codeUsedByMultipleModules.Code, string.Join(", ", codeUsedByMultipleModules.Modules.Select(m => m.Description))));
					}
				});
			}
			else
			{
				Assert("All modules have unique codes", true);
			}
		}
	}
}