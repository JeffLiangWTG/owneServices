using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineDutyAndTaxGrid()
		{
			using (var control = new EntryLineAdditionalDataUserControl())
			{
				var feesGrid = control.FindSingle<ZGrid>("EntryLineDutyAndTaxGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("CF_ChargeType column", feesGrid.GetColumnStyle("CF_ChargeType"));
					AssertNotNull("CF_ChargeType column", feesGrid.GetColumnStyle("CF_ChargeAmount"));
					AssertNotNull("CF_ChargeType column", feesGrid.GetColumnStyle("LocalCurrencyCode"));
				});
			}
		}
	}
}
