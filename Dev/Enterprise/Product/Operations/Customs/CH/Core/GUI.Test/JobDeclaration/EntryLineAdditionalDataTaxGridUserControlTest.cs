using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EntryLineAdditionalDataTaxGridUserControl))]
sealed class EntryLineAdditionalDataTaxGridUserControlTest : TestCaseWithFactory
{
	public void TestEntryLineDutyAndTaxGrid()
	{
		using (var userControl = new EntryLineAdditionalDataTaxGridUserControl())
		{
			var index = 0;
			var dutyAndTaxGridColumnStyles = userControl.EntryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(dutyAndTaxGridColumnStyles, CusEntryLineFee.Schema.CF_ChargeType, index++);
				UserControlTestHelper.AssertColumnStyles(dutyAndTaxGridColumnStyles, nameof(CusEntryLineFee.ChargeTypeDescription), index++);
				UserControlTestHelper.AssertColumnStyles(dutyAndTaxGridColumnStyles, CusEntryLineFee.Schema.CF_RateOverrideReasonCode, index++);
				UserControlTestHelper.AssertColumnStyles(dutyAndTaxGridColumnStyles, CusEntryLineFee.Schema.CF_BaseValue, index++);
				UserControlTestHelper.AssertColumnStyles(dutyAndTaxGridColumnStyles, CusEntryLineFee.Schema.CF_Rate, index++);
				UserControlTestHelper.AssertColumnStyles(dutyAndTaxGridColumnStyles, CusEntryLineFee.Schema.CF_ChargeAmount, index++);
			});
		}
	}
}
