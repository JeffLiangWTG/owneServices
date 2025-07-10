using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class B2LineAsClaimedForUserControlTest : TestCaseWithFactory
	{
		public void TestTariffUserControlForCAGlobalTariff()
		{
			var gridName = "AsClaimForGrid";
			var columnName = JobComInvoiceLine.Schema.JI_FormattedTariff;
			var tariffFindBoxName_TrfCA = "AsClaimedClassificationTariffFindBox";
			var tariffFindBoxName_SRDb = "AsClaimedClassificationTariffFromSRDbFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var b2 = Factory.New<JobDeclaration>();
			b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = b2.Invoices.AddNew();
			var asClaimedLine = subHeader.AsClaimForFilteredInvoiceLines.AddNew();
			asClaimedLine.OnLoaded();

			using (var form = new ZForm(b2))
			using (var control = new B2LineAsClaimedForUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(control, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(control, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(control, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}
	}
}
