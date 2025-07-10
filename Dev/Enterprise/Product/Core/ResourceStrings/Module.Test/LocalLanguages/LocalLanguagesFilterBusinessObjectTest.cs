using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	[TestedType(typeof(LocalLanguagesFilterBusinessObject))]
	public class LocalLanguagesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFullLanguageCodeFilter()
		{
			var testLanguage1 = Factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";

			var testLanguage2 = Factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var fullLanguageCodeFilter = (ModuleTextFilter)filterBizObj["Full Language Code"];
			fullLanguageCodeFilter.IsActive = true;
			fullLanguageCodeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			fullLanguageCodeFilter.Property = "AA";
			AssertContainsExactElementsInAnyOrder(
				new[] { "AA-CN" },
				Factory.Load<RefLocalLanguage>(filterBizObj.Filter).Select(x => x.FullLanguageCode.ToString()));

			fullLanguageCodeFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			fullLanguageCodeFilter.Property = "AA";
			AssertContainsExactElementsInAnyOrder(
				new[] { "BB-CN" },
				Factory.Load<RefLocalLanguage>(filterBizObj.Filter).Select(x => x.FullLanguageCode.ToString()));

			fullLanguageCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			fullLanguageCodeFilter.Property = "AA-CN";
			AssertContainsExactElementsInAnyOrder(
				new[] { "AA-CN" },
				Factory.Load<RefLocalLanguage>(filterBizObj.Filter).Select(x => x.FullLanguageCode.ToString()));

			fullLanguageCodeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			fullLanguageCodeFilter.Property = "AA-CN";
			AssertContainsExactElementsInAnyOrder(
				new[] { "BB-CN" },
				Factory.Load<RefLocalLanguage>(filterBizObj.Filter).Select(x => x.FullLanguageCode.ToString()));

			fullLanguageCodeFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals("Should find nothing", 0, Factory.Load<RefLocalLanguage>(filterBizObj.Filter).Length);

			fullLanguageCodeFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertContainsExactElementsInAnyOrder(
				new[] { "AA-CN", "BB-CN" },
				Factory.Load<RefLocalLanguage>(filterBizObj.Filter).Select(x => x.FullLanguageCode.ToString()));
		}

		#region Implementation

		LocalLanguagesFilterBusinessObject GetFilterBizObj()
		{
			return new LocalLanguagesFilterBusinessObject();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LocalLanguagesFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(RefLocalLanguageSchema.Constants.TableName);
		}

		#endregion
	}
}
