using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportAmendmentEntryDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestEntryDetails()
		{
			using (var control = new ImportAmendmentEntryDetailsUserControl())
			{
				var dynamicEntryDetailsLayoutPanel = control.FindSingle<DynamicLayoutPanel>("DynamicImportAmendmentEntryDetailsLayoutPanel");
				AssertNotNull(dynamicEntryDetailsLayoutPanel);
			}
		}

		public void TestPenaltyAndRefundDetails()
		{
			using (var control = new ImportAmendmentEntryDetailsUserControl())
			{
				var dynamicPenaltyAndRefundDetailsLayoutPanel = control.FindSingle<DynamicLayoutPanel>("DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel");
				AssertNotNull(dynamicPenaltyAndRefundDetailsLayoutPanel);
			}
		}
	}
}
