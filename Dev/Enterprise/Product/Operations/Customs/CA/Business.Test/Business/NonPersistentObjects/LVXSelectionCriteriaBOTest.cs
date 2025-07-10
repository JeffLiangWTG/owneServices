using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business
{
	[TestedType(typeof(LVXSelectionCriteriaBO))]
	sealed class LVXSelectionCriteriaBOTest : NonPersistentBusinessObjectTestCase
	{
		public override void TestBizObjectFields()
		{
			var businessObject = (LVXSelectionCriteriaBO)GetNewBusinessObject();

			businessObject.PeriodYear = 0;
			AssertEquals("PeriodYear", 1900, businessObject.PeriodYear);
			businessObject.PeriodYear = 9999;
			AssertEquals("PeriodYear", 2078, businessObject.PeriodYear);
			businessObject.PeriodMonth = 0;
			AssertEquals("PeriodMonth", 1, businessObject.PeriodMonth);
			businessObject.PeriodMonth = 13;
			AssertEquals("PeriodMonth", 12, businessObject.PeriodMonth);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LVXSelectionCriteriaBO(Factory);
		}

		#endregion
	}
}
