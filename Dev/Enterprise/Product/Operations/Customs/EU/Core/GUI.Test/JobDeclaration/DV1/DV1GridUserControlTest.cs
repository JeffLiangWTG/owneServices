using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class DV1GridUserControlTest : TestCaseWithFactory
	{
		public void TestDV1DetailsGridColumnSize()
		{
			using (var control = new DV1GridUserControl())
			{
				var detailsGrid = control.FindSingle<ZGrid>("DetailsGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Relatonship", 40, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_Relationship));
					AssertEquals("RelationDetails", 150, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_RelationDetails));
					AssertEquals("PriceInfluence", 40, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_PriceInfluence));
					AssertEquals("Restrictions", 40, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_Restrictions));
					AssertEquals("Consideration", 40, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_Consideration));
					AssertEquals("RestrictionConsiderationDetails", 150, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_RestrictionConsiderationDetails));
					AssertEquals("RoyaltiesLicence", 40, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_RoyaltiesLicence));
					AssertEquals("RoyaltiesLicenceDetails", 150, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_RoyaltiesLicenceDetails));
					AssertEquals("Resale", 40, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_Resale));
					AssertEquals("ResaleDetails", 150, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_ResaleDetails));
					AssertEquals("CustomsDecisionNumber", 180, detailsGrid.GetColumnWidth(CusDV1DetailSchema.Constants.DV1_CustomsDecisionNumber));
				});
			}
		}
	}
}
